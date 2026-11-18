using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Items;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using System.Collections.Generic;
using UnityEngine;

namespace GoodForBusiness.Slimes
{
    public class EquipmentSlimeVariant : SlimeVariantBase
    {
        public override SlimeType Type => SlimeType.Equipment;
        public override Color SlimeColor => Color.blue;

        private PluginConfig? _config;
        private List<(Item item, int weight)> _dropTable = new();

        public override int GrowthCost
        {
            get => _config?.EquipmentSlimeGrowthCost.Value ?? 300;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
            ParseDropTable();
        }

        private void ParseDropTable()
        {
            _dropTable.Clear();
            if (_config == null) return;

            try
            {
                var json = _config.EquipmentDropTable.Value;
                // Simple JSON parsing for array of {"item":"name","weight":N}
                // In real implementation, use JsonUtility or Newtonsoft.Json
                var entries = json.Trim('[', ']').Split('}');
                foreach (var entry in entries)
                {
                    if (string.IsNullOrWhiteSpace(entry)) continue;
                    var itemName = ExtractJsonValue(entry, "item");
                    var weightStr = ExtractJsonValue(entry, "weight");
                    if (!string.IsNullOrEmpty(itemName) && int.TryParse(weightStr, out int weight))
                    {
                        var item = Items.FindItemByName(itemName);
                        if (item != null)
                        {
                            _dropTable.Add((item, weight));
                        }
                    }
                }
            }
            catch
            {
                // Fallback to defaults
                _dropTable.Add((Items.FindItemByName("Flashlight")!, 10));
                _dropTable.Add((Items.FindItemByName("Shovel")!, 10));
                _dropTable.Add((Items.FindItemByName("StunGrenade")!, 5));
                _dropTable.Add((Items.FindItemByName("ProbeScanner")!, 5));
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

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility()
        {
            if (_dropTable.Count == 0 || StartOfRound.Instance == null)
                return;

            // Select random equipment based on weights
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

            // Spawn at ship position (first player's position as fallback)
            Vector3 spawnPos = StartOfRound.Instance.allPlayerScripts?[0]?.transform.position ?? Vector3.zero;
            var crateObj = UnityEngine.Object.Instantiate(selectedItem.spawnPrefab, spawnPos, Quaternion.identity);
            
            // Network spawn if host
            var networkObj = crateObj.GetComponent<NetworkObject>();
            if (networkObj != null && Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }
        }

        public override string GetDisplayName() => "Equipment Slime";
        public override string GetDescription() => "A blue slime that produces random equipment daily.";
    }
}