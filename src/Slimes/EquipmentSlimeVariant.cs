using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Slimes;
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
        private SlimeAI? _slimeAI;

        public override int GrowthCost
        {
            get => _config?.EquipmentSlimeGrowthCost.Value ?? 300;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
            _slimeAI = GetComponent<SlimeAI>();
            ParseDropTable();
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

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility()
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

            Vector3 spawnPos = StartOfRound.Instance.allPlayerScripts?[0]?.transform.position ?? Vector3.zero;
            _slimeAI.SpawnItemAt(selectedItem.name, spawnPos);
        }

        public override string GetDisplayName() => "Equipment Slime";
        public override string GetDescription() => "A blue slime that produces random equipment daily.";
    }
}