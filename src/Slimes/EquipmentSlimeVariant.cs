using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Items;
using GoodForBusiness.Slimes.Variants;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace GoodForBusiness.Slimes
{
    public class EquipmentSlimeVariant : MonoBehaviour, ISlimeVariant
    {
        public SlimeType Type => SlimeType.Equipment;
        public Color SlimeColor => Color.blue;

        private PluginConfig? _config;
        private List<(Item item, int weight)> _dropTable = new();
        private SlimeAI? _slimeAI;

        public int GrowthCost
        {
            get => _config?.EquipmentSlimeGrowthCost.Value ?? 300;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
            _slimeAI = GetComponent<SlimeAI>();
            ParseDropTable();
            // Register this variant
            SlimeVariantRegistry.Register(this);
        }

        private void ParseDropTable()
        {
            _dropTable.Clear();
            if (_config == null) return;

            try
            {
                var json = _config.EquipmentDropTable.Value;
                var entries = json.Trim('[', ']').Split('}');
                foreach (var entry in entries)
                {
                    if (string.IsNullOrWhiteSpace(entry)) continue;
                    var itemName = ExtractJsonValue(entry, "item");
                    var weightStr = ExtractJsonValue(entry, "weight");
                    if (!string.IsNullOrEmpty(itemName) && int.TryParse(weightStr, out int weight))
                    {
                        var item = LethalLib.Modules.Items.FindItemByName(itemName);
                        if (item != null)
                        {
                            _dropTable.Add((item, weight));
                        }
                    }
                }
            }
            catch
            {
                _dropTable.Add((LethalLib.Modules.Items.FindItemByName("Flashlight")!, 10));
                _dropTable.Add((LethalLib.Modules.Items.FindItemByName("Shovel")!, 10));
                _dropTable.Add((LethalLib.Modules.Items.FindItemByName("StunGrenade")!, 5));
                _dropTable.Add((LethalLib.Modules.Items.FindItemByName("ProbeScanner")!, 5));
            }
        }

        private string ExtractJsonValue(string json, string key)
        {
            var keyPattern = $"\"{key}\":";
            int idx = json.IndexOf(keyPattern);
            if (idx < 0) return "";
            idx += keyPattern.Length;
            while (idx < json.Length && (json[idx] == ' ' || json[idx] == '"')) idx++;
            int end = idx;
            while (end < json.Length && json[end] != '"' && json[end] != ',' && json[end] != '}') end++;
            return json.Substring(idx, end - idx).Trim('"');
        }

        public void OnFedScrap(int scrapValue)
        {
            // Growth handled by SlimeAI
        }

        public void ExecuteDailyAbility(SlimeAI slime)
        {
            if (_dropTable.Count == 0 || StartOfRound.Instance == null || _slimeAI == null)
                return;

            int totalWeight = 0;
            foreach (var (_, weight) in _dropTable) totalWeight += weight;
            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            Item? selectedItem = null;

            foreach (var (item, weight) in _dropTable)
            {
                cumulative += weight;
                if (roll < cumulative)
                {
                    selectedItem = item;
                    break;
                }
            }

            if (selectedItem == null || selectedItem.spawnPrefab == null)
                return;

            // Spawn at ship location
            Vector3 spawnPos = slime.transform.position;
            if (ShipBuildModeManager.Instance != null)
            {
                spawnPos = ShipBuildModeManager.Instance.transform.position;
            }
            else if (StartOfRound.Instance?.allPlayerScripts != null && StartOfRound.Instance.allPlayerScripts.Length > 0)
            {
                var player = StartOfRound.Instance.allPlayerScripts[0];
                if (player != null)
                {
                    spawnPos = player.transform.position;
                }
            }

            // Spawn an Equipment Crate containing the selected item (visual feedback + auto-pickup option)
            SpawnEquipmentCrate(selectedItem, spawnPos);
        }

        /// <summary>
        /// Spawns an Equipment Crate at the given position containing the specified item.
        /// The crate provides visual feedback (particle effect) and supports auto-pickup
        /// or ground spawn based on configuration.
        /// </summary>
        private void SpawnEquipmentCrate(Item item, Vector3 position)
        {
            if (_config == null || _slimeAI == null)
                return;

            // Find the Equipment Crate prefab from registered items
            var crateItem = LethalLib.Modules.Items.FindItemByName("EquipmentCrate");
            if (crateItem == null || crateItem.spawnPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Equipment Crate item not registered. Falling back to direct item spawn.");
                _slimeAI.SpawnItemAt(item.name, position);
                return;
            }

            // Instantiate the crate prefab
            var crateObj = UnityEngine.Object.Instantiate(crateItem.spawnPrefab, position, UnityEngine.Quaternion.identity);
            var crateComponent = crateObj.GetComponent<EquipmentCrateItem>();
            if (crateComponent == null)
            {
                crateComponent = crateObj.AddComponent<EquipmentCrateItem>();
            }

            // Configure the crate
            crateComponent.ContainedItem = item;
            crateComponent.AutoPickup = _config.EquipmentCrateAutoPickup.Value;
            crateComponent.PlaySpawnEffect = _config.EquipmentCrateVisualFeedback.Value;
            crateComponent.CreditValue = item.creditsWorth;
            crateComponent.itemValue = item.creditsWorth;

            // Network spawn the crate if host (OnNetworkSpawn will handle initialization)
            var networkObj = crateObj.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                networkObj.Spawn();
            }
        }

        public string GetDisplayName() => "Equipment Slime";
        public string GetDescription() => "A blue slime that produces random equipment daily.";
    }
}