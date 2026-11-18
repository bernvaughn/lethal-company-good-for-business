using BepInEx.Configuration;
using System;
using System.Text.Json;
using UnityEngine;

namespace GoodForBusiness.Configuration
{
    public class PluginConfig
    {
        private readonly ConfigFile _config;

        // General
        public ConfigEntry<bool> EnableMod { get; }
        public ConfigEntry<bool> DebugLogging { get; }

        // Egg spawning
        public ConfigEntry<float> EggGlobalSpawnChance { get; }
        public ConfigEntry<int> EggRarityExperiment { get; }
        public ConfigEntry<int> EggRarityAssurance { get; }
        public ConfigEntry<int> EggRarityVow { get; }
        public ConfigEntry<int> EggRarityMarch { get; }
        public ConfigEntry<int> EggRarityOffense { get; }
        public ConfigEntry<int> EggRarityRend { get; }
        public ConfigEntry<int> EggRarityDine { get; }
        public ConfigEntry<int> EggRarityModded { get; }

        // Equipment slime
        public ConfigEntry<bool> EnableEquipmentSlime { get; }
        public ConfigEntry<int> EquipmentSlimeGrowthCost { get; }
        public ConfigEntry<string> EquipmentDropTable { get; }

        // Equipment crate
        public ConfigEntry<bool> EquipmentCrateAutoPickup { get; }
        public ConfigEntry<bool> EquipmentCrateVisualFeedback { get; }

        // Bee lover slime
        public ConfigEntry<bool> EnableBeeLoverSlime { get; }
        public ConfigEntry<int> BeeLoverSlimeGrowthCost { get; }
        public ConfigEntry<int> SlimedBeehiveValue { get; }
        public ConfigEntry<float> BeeDetectionRange { get; }
        public ConfigEntry<float> BeeLoverSlimeAttackRange { get; }
        public ConfigEntry<float> BeeLoverSlimePathfindingTimeout { get; }
        public ConfigEntry<bool> BeeLoverSlimeTeleportFallbackEnabled { get; }

        // Visuals
        public ConfigEntry<bool> SlimesFacePlayers { get; }
        public ConfigEntry<float> SlimeBobAmount { get; }

        public PluginConfig(ConfigFile config)
        {
            _config = config;

            // General
            EnableMod = config.Bind("General", "EnableMod", true, "Enable the GoodForBusiness mod.");
            DebugLogging = config.Bind("General", "DebugLogging", false, "Enable debug logging.");

            // Egg spawning
            EggGlobalSpawnChance = config.Bind("Egg Spawning", "GlobalSpawnChance", 1.0f,
                new ConfigDescription("Global multiplier for egg spawn chance (0.0-1.0).",
                    new AcceptableValueRange<float>(0f, 1f),
                    OnEggGlobalSpawnChanceChanged));
            EggRarityExperiment = config.Bind("Egg Spawning", "RarityExperiment", 15,
                new ConfigDescription("Egg spawn rarity on Experiment. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityAssurance = config.Bind("Egg Spawning", "RarityAssurance", 10,
                new ConfigDescription("Egg spawn rarity on Assurance. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityVow = config.Bind("Egg Spawning", "RarityVow", 20,
                new ConfigDescription("Egg spawn rarity on Vow. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityMarch = config.Bind("Egg Spawning", "RarityMarch", 5,
                new ConfigDescription("Egg spawn rarity on March. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityOffense = config.Bind("Egg Spawning", "RarityOffense", 8,
                new ConfigDescription("Egg spawn rarity on Offense. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityRend = config.Bind("Egg Spawning", "RarityRend", 12,
                new ConfigDescription("Egg spawn rarity on Rend. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityDine = config.Bind("Egg Spawning", "RarityDine", 10,
                new ConfigDescription("Egg spawn rarity on Dine. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));
            EggRarityModded = config.Bind("Egg Spawning", "RarityModded", 10,
                new ConfigDescription("Egg spawn rarity on modded moons. Must be non-negative.",
                    new AcceptableValueRange<int>(0, int.MaxValue),
                    OnEggRarityChanged));

            // Equipment slime
            EnableEquipmentSlime = config.Bind("Equipment Slime", "Enable", true, "Enable the blue equipment slime variant.");
            EquipmentSlimeGrowthCost = config.Bind("Equipment Slime", "GrowthCost", 300,
                new ConfigDescription("Scrap value required to grow the equipment slime. Must be positive.",
                    new AcceptableValueRange<int>(1, int.MaxValue),
                    OnPositiveIntChanged));
            EquipmentDropTable = config.Bind("Equipment Slime", "DropTable",
                "[{\"item\":\"Flashlight\",\"weight\":10},{\"item\":\"Shovel\",\"weight\":10},{\"item\":\"StunGrenade\",\"weight\":5},{\"item\":\"ProbeScanner\",\"weight\":5}]",
                new ConfigDescription("JSON array of equipment items and weights for daily drops. Each entry: {\"item\":string,\"weight\":int>0}.",
                    null,
                    OnEquipmentDropTableChanged));

            // Equipment crate
            EquipmentCrateAutoPickup = config.Bind("Equipment Crate", "AutoPickup", false, "When true, equipment crates automatically transfer their contents to the nearest player on spawn. When false, crates stay on the ground for manual pickup.");
            EquipmentCrateVisualFeedback = config.Bind("Equipment Crate", "VisualFeedback", true, "When true, equipment crates play a particle effect on spawn for visual feedback.");

            // Bee lover slime
            EnableBeeLoverSlime = config.Bind("Bee Lover Slime", "Enable", true, "Enable the yellow/black bee lover slime variant.");
            BeeLoverSlimeGrowthCost = config.Bind("Bee Lover Slime", "GrowthCost", 400,
                new ConfigDescription("Scrap value required to grow the bee lover slime. Must be positive.",
                    new AcceptableValueRange<int>(1, int.MaxValue),
                    OnPositiveIntChanged));
            SlimedBeehiveValue = config.Bind("Bee Lover Slime", "SlimedBeehiveValue", 175,
                new ConfigDescription("Credit value of a Slimed Beehive scrap item. Must be positive.",
                    new AcceptableValueRange<int>(1, int.MaxValue),
                    OnPositiveIntChanged));
            BeeDetectionRange = config.Bind("Bee Lover Slime", "BeeDetectionRange", 30f,
                new ConfigDescription("Maximum distance to detect beehives. Must be positive.",
                    new AcceptableValueRange<float>(0.1f, float.MaxValue),
                    OnPositiveFloatChanged));
            BeeLoverSlimeAttackRange = config.Bind("Bee Lover Slime", "AttackRange", 2f,
                new ConfigDescription("Distance to get to a beehive before attacking it. Must be positive.",
                    new AcceptableValueRange<float>(0.1f, float.MaxValue),
                    OnPositiveFloatChanged));
            BeeLoverSlimePathfindingTimeout = config.Bind("Bee Lover Slime", "PathfindingTimeout", 10f,
                new ConfigDescription("Seconds to attempt pathfinding before falling back to teleport. Must be positive.",
                    new AcceptableValueRange<float>(0.1f, float.MaxValue),
                    OnPositiveFloatChanged));
            BeeLoverSlimeTeleportFallbackEnabled = config.Bind("Bee Lover Slime", "TeleportFallbackEnabled", true, "When true, teleport to the beehive if pathfinding fails.");

            // Visuals
            SlimesFacePlayers = config.Bind("Visuals", "SlimesFacePlayers", true, "Slimes will face the nearest player.");
            SlimeBobAmount = config.Bind("Visuals", "SlimeBobAmount", 0.5f,
                new ConfigDescription("Amount slimes bob up and down while idle. Must be non-negative.",
                    new AcceptableValueRange<float>(0f, float.MaxValue),
                    OnNonNegativeFloatChanged));
        }

        private static void OnEggGlobalSpawnChanceChanged()
        {
            // Handled by AcceptableValueRange automatically
        }

        private static void OnEggRarityChanged()
        {
            // Handled by AcceptableValueRange automatically
        }

        private static void OnPositiveIntChanged(ConfigEntry<int> entry)
        {
            if (entry.Value < 1)
            {
                LogValidationError(entry, entry.DefaultValue, "Value must be >= 1");
                entry.Value = (int)entry.DefaultValue;
            }
        }

        private static void OnPositiveFloatChanged(ConfigEntry<float> entry)
        {
            if (entry.Value < 0.1f)
            {
                LogValidationError(entry, entry.DefaultValue, "Value must be > 0");
                entry.Value = (float)entry.DefaultValue;
            }
        }

        private static void OnNonNegativeFloatChanged(ConfigEntry<float> entry)
        {
            if (entry.Value < 0f)
            {
                LogValidationError(entry, entry.DefaultValue, "Value must be >= 0");
                entry.Value = (float)entry.DefaultValue;
            }
        }

        private static void OnEquipmentDropTableChanged(ConfigEntry<string> entry)
        {
            if (!IsValidEquipmentDropTable(entry.Value))
            {
                LogValidationError(entry, entry.DefaultValue, "Invalid JSON format. Expected array of objects with 'item' (string) and 'weight' (int > 0).");
                entry.Value = (string)entry.DefaultValue;
            }
        }

        private static bool IsValidEquipmentDropTable(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return false;

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                    return false;

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Object)
                        return false;

                    if (!element.TryGetProperty("item", out var itemProp) || itemProp.ValueKind != JsonValueKind.String)
                        return false;

                    if (!element.TryGetProperty("weight", out var weightProp) || weightProp.ValueKind != JsonValueKind.Number)
                        return false;

                    if (weightProp.GetInt32() <= 0)
                        return false;
                }

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static void LogValidationError<T>(ConfigEntry<T> entry, object defaultValue, string reason)
        {
            Debug.LogError($"[GoodForBusiness] Config validation failed for {entry.Section}.{entry.Key}: {reason}. Resetting to default: {defaultValue}");
        }
    }
}