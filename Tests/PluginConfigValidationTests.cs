using BepInEx.Configuration;
using GoodForBusiness.Configuration;
using System.Collections.Generic;
using UnityEngine;

namespace GoodForBusiness.Tests
{
    public class PluginConfigValidationTests
    {
        private static PluginConfig CreateTestConfig()
        {
            var configFile = new ConfigFile(null, true);
            return new PluginConfig(configFile);
        }

        public static bool Config_DefaultValues_AreValid()
        {
            var config = CreateTestConfig();

            // General
            bool enableMod = config.EnableMod.Value == true;
            bool debugLogging = config.DebugLogging.Value == false;

            // Egg spawning
            bool globalSpawnChance = config.EggGlobalSpawnChance.Value == 1.0f;
            bool rarityExperiment = config.EggRarityExperiment.Value == 15;
            bool rarityAssurance = config.EggRarityAssurance.Value == 10;
            bool rarityVow = config.EggRarityVow.Value == 20;
            bool rarityMarch = config.EggRarityMarch.Value == 5;
            bool rarityOffense = config.EggRarityOffense.Value == 8;
            bool rarityRend = config.EggRarityRend.Value == 12;
            bool rarityDine = config.EggRarityDine.Value == 10;
            bool rarityModded = config.EggRarityModded.Value == 10;

            // Equipment slime
            bool enableEquipment = config.EnableEquipmentSlime.Value == true;
            bool growthCost = config.EquipmentSlimeGrowthCost.Value == 300;
            bool dropTable = !string.IsNullOrEmpty(config.EquipmentDropTable.Value);

            // Equipment crate
            bool autoPickup = config.EquipmentCrateAutoPickup.Value == false;
            bool visualFeedback = config.EquipmentCrateVisualFeedback.Value == true;

            // Bee lover slime
            bool enableBeeLover = config.EnableBeeLoverSlime.Value == true;
            bool beeGrowthCost = config.BeeLoverSlimeGrowthCost.Value == 400;
            bool beehiveValue = config.SlimedBeehiveValue.Value == 175;
            bool beeDetectionRange = config.BeeDetectionRange.Value == 30f;
            bool attackRange = config.BeeLoverSlimeAttackRange.Value == 2f;
            bool pathfindingTimeout = config.BeeLoverSlimePathfindingTimeout.Value == 10f;
            bool teleportFallback = config.BeeLoverSlimeTeleportFallbackEnabled.Value == true;

            // Visuals
            bool facePlayers = config.SlimesFacePlayers.Value == true;
            bool bobAmount = config.SlimeBobAmount.Value == 0.5f;

            return enableMod && debugLogging &&
                   globalSpawnChance && rarityExperiment && rarityAssurance && rarityVow &&
                   rarityMarch && rarityOffense && rarityRend && rarityDine && rarityModded &&
                   enableEquipment && growthCost && dropTable &&
                   autoPickup && visualFeedback &&
                   enableBeeLover && beeGrowthCost && beehiveValue && beeDetectionRange &&
                   attackRange && pathfindingTimeout && teleportFallback &&
                   facePlayers && bobAmount;
        }

        public static bool Config_EggGlobalSpawnChance_ClampedTo0To1()
        {
            var config = CreateTestConfig();

            // Test negative value gets clamped to default
            config.EggGlobalSpawnChance.Value = -0.5f;
            bool negativeClamped = config.EggGlobalSpawnChance.Value == 1.0f;

            // Test value > 1 gets clamped to default
            config.EggGlobalSpawnChance.Value = 2.0f;
            bool overOneClamped = config.EggGlobalSpawnChance.Value == 1.0f;

            // Test valid values are accepted
            config.EggGlobalSpawnChance.Value = 0.5f;
            bool validAccepted = config.EggGlobalSpawnChance.Value == 0.5f;

            config.EggGlobalSpawnChance.Value = 0f;
            bool zeroAccepted = config.EggGlobalSpawnChance.Value == 0f;

            config.EggGlobalSpawnChance.Value = 1f;
            bool oneAccepted = config.EggGlobalSpawnChance.Value == 1f;

            return negativeClamped && overOneClamped && validAccepted && zeroAccepted && oneAccepted;
        }

        public static bool Config_EggRarityValues_NonNegative()
        {
            var config = CreateTestConfig();

            var rarityEntries = new List<ConfigEntry<int>>
            {
                config.EggRarityExperiment,
                config.EggRarityAssurance,
                config.EggRarityVow,
                config.EggRarityMarch,
                config.EggRarityOffense,
                config.EggRarityRend,
                config.EggRarityDine,
                config.EggRarityModded
            };

            foreach (var entry in rarityEntries)
            {
                // Test negative value gets clamped to default
                entry.Value = -1;
                if (entry.Value != entry.DefaultValue)
                    return false;

                // Test zero is accepted
                entry.Value = 0;
                if (entry.Value != 0)
                    return false;

                // Test positive is accepted
                entry.Value = 100;
                if (entry.Value != 100)
                    return false;
            }

            return true;
        }

        public static bool Config_PositiveIntValues_ClampedToDefaults()
        {
            var config = CreateTestConfig();

            var positiveIntEntries = new List<ConfigEntry<int>>
            {
                config.EquipmentSlimeGrowthCost,
                config.BeeLoverSlimeGrowthCost,
                config.SlimedBeehiveValue
            };

            foreach (var entry in positiveIntEntries)
            {
                // Test zero gets clamped to default
                entry.Value = 0;
                if (entry.Value != entry.DefaultValue)
                    return false;

                // Test negative gets clamped to default
                entry.Value = -100;
                if (entry.Value != entry.DefaultValue)
                    return false;

                // Test valid positive is accepted
                entry.Value = 500;
                if (entry.Value != 500)
                    return false;
            }

            return true;
        }

        public static bool Config_PositiveFloatValues_ClampedToDefaults()
        {
            var config = CreateTestConfig();

            var positiveFloatEntries = new List<ConfigEntry<float>>
            {
                config.BeeDetectionRange,
                config.BeeLoverSlimeAttackRange,
                config.BeeLoverSlimePathfindingTimeout
            };

            foreach (var entry in positiveFloatEntries)
            {
                // Test zero gets clamped to default
                entry.Value = 0f;
                if (entry.Value != entry.DefaultValue)
                    return false;

                // Test negative gets clamped to default
                entry.Value = -10f;
                if (entry.Value != entry.DefaultValue)
                    return false;

                // Test valid positive is accepted
                entry.Value = 50f;
                if (entry.Value != 50f)
                    return false;
            }

            return true;
        }

        public static bool Config_NonNegativeFloatValues_ClampedToDefaults()
        {
            var config = CreateTestConfig();

            // Test SlimeBobAmount - zero should be accepted
            config.SlimeBobAmount.Value = 0f;
            bool zeroAccepted = config.SlimeBobAmount.Value == 0f;

            // Test negative gets clamped to default
            config.SlimeBobAmount.Value = -1f;
            bool negativeClamped = config.SlimeBobAmount.Value == 0.5f;

            // Test valid positive is accepted
            config.SlimeBobAmount.Value = 1.5f;
            bool positiveAccepted = config.SlimeBobAmount.Value == 1.5f;

            return zeroAccepted && negativeClamped && positiveAccepted;
        }

        public static bool Config_EquipmentDropTable_ValidatesJSON()
        {
            var config = CreateTestConfig();

            // Valid JSON should be accepted
            string validJson = "[{\"item\":\"Flashlight\",\"weight\":10},{\"item\":\"Shovel\",\"weight\":20}]";
            config.EquipmentDropTable.Value = validJson;
            bool validAccepted = config.EquipmentDropTable.Value == validJson;

            // Invalid JSON (not an array) should reset to default
            config.EquipmentDropTable.Value = "{\"item\":\"Flashlight\",\"weight\":10}";
            bool notArrayClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            // Invalid JSON (missing item) should reset to default
            config.EquipmentDropTable.Value = "[{\"weight\":10}]";
            bool missingItemClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            // Invalid JSON (missing weight) should reset to default
            config.EquipmentDropTable.Value = "[{\"item\":\"Flashlight\"}]";
            bool missingWeightClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            // Invalid JSON (weight <= 0) should reset to default
            config.EquipmentDropTable.Value = "[{\"item\":\"Flashlight\",\"weight\":0}]";
            bool zeroWeightClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            config.EquipmentDropTable.Value = "[{\"item\":\"Flashlight\",\"weight\":-5}]";
            bool negativeWeightClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            // Malformed JSON should reset to default
            config.EquipmentDropTable.Value = "{invalid json}";
            bool malformedClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            // Empty string should reset to default
            config.EquipmentDropTable.Value = "";
            bool emptyClamped = config.EquipmentDropTable.Value == config.EquipmentDropTable.DefaultValue;

            return validAccepted && notArrayClamped && missingItemClamped &&
                   missingWeightClamped && zeroWeightClamped && negativeWeightClamped &&
                   malformedClamped && emptyClamped;
        }

        public static void RunAll()
        {
            var tests = new List<(string Name, bool Result)>
            {
                ("Config_DefaultValues_AreValid", Config_DefaultValues_AreValid()),
                ("Config_EggGlobalSpawnChance_ClampedTo0To1", Config_EggGlobalSpawnChance_ClampedTo0To1()),
                ("Config_EggRarityValues_NonNegative", Config_EggRarityValues_NonNegative()),
                ("Config_PositiveIntValues_ClampedToDefaults", Config_PositiveIntValues_ClampedToDefaults()),
                ("Config_PositiveFloatValues_ClampedToDefaults", Config_PositiveFloatValues_ClampedToDefaults()),
                ("Config_NonNegativeFloatValues_ClampedToDefaults", Config_NonNegativeFloatValues_ClampedToDefaults()),
                ("Config_EquipmentDropTable_ValidatesJSON", Config_EquipmentDropTable_ValidatesJSON()),
            };

            int passed = 0;
            int failed = 0;
            foreach (var (name, result) in tests)
            {
                if (result)
                {
                    passed++;
                    UnityEngine.Debug.Log($"[PASS] {name}");
                }
                else
                {
                    failed++;
                    UnityEngine.Debug.Log($"[FAIL] {name}");
                }
            }

            UnityEngine.Debug.Log($"[GoodForBusiness.Tests] PluginConfigValidationTests: {passed} passed, {failed} failed");

            if (failed > 0)
                throw new System.Exception($"{failed} test(s) failed");
        }
    }
}