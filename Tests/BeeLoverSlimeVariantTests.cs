using BepInEx.Configuration;
using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using System.Collections.Generic;
using UnityEngine;

namespace GoodForBusiness.Tests
{
    public class BeeLoverSlimeVariantTests
    {
        private static PluginConfig CreateTestConfig()
        {
            var configFile = new ConfigFile(null, true);
            return new PluginConfig(configFile);
        }

        public static bool Variant_HasCorrectType()
        {
            var variant = new BeeLoverSlimeVariant();
            bool result = variant.Type == SlimeType.BeeLover;
            return result;
        }

        public static bool Variant_HasCorrectColor()
        {
            var variant = new BeeLoverSlimeVariant();
            var color = variant.SlimeColor;
            bool result = color.r > 0.9f && color.g > 0.7f && color.b < 0.2f; // Yellow/gold
            return result;
        }

        public static bool Variant_GrowthCostDefaultsTo400()
        {
            var variant = new BeeLoverSlimeVariant();
            bool result = variant.GrowthCost == 400;
            return result;
        }

        public static bool Variant_GetDisplayName_ReturnsBeeLoverSlime()
        {
            var variant = new BeeLoverSlimeVariant();
            bool result = variant.GetDisplayName() == "Bee Lover Slime";
            return result;
        }

        public static bool Variant_GetDescription_NotEmpty()
        {
            var variant = new BeeLoverSlimeVariant();
            bool result = !string.IsNullOrEmpty(variant.GetDescription());
            return result;
        }

        public static bool Config_HasBeeHuntingSettings()
        {
            var config = CreateTestConfig();
            bool hasAttackRange = config.BeeLoverSlimeAttackRange != null;
            bool hasPathfindingTimeout = config.BeeLoverSlimePathfindingTimeout != null;
            bool hasTeleportFallback = config.BeeLoverSlimeTeleportFallbackEnabled != null;
            return hasAttackRange && hasPathfindingTimeout && hasTeleportFallback;
        }

        public static bool Config_DefaultAttackRange_Is2f()
        {
            var config = CreateTestConfig();
            bool result = config.BeeLoverSlimeAttackRange.Value == 2f;
            return result;
        }

        public static bool Config_DefaultPathfindingTimeout_Is10f()
        {
            var config = CreateTestConfig();
            bool result = config.BeeLoverSlimePathfindingTimeout.Value == 10f;
            return result;
        }

        public static bool Config_DefaultTeleportFallback_IsTrue()
        {
            var config = CreateTestConfig();
            bool result = config.BeeLoverSlimeTeleportFallbackEnabled.Value == true;
            return result;
        }

        public static bool Config_DefaultSlimedBeehiveValue_Is175()
        {
            var config = CreateTestConfig();
            bool result = config.SlimedBeehiveValue.Value == 175;
            return result;
        }

        public static bool Config_DefaultBeeDetectionRange_Is30f()
        {
            var config = CreateTestConfig();
            bool result = config.BeeDetectionRange.Value == 30f;
            return result;
        }

        public static void RunAll()
        {
            var tests = new List<(string Name, bool Result)>
            {
                ("Variant_HasCorrectType", Variant_HasCorrectType()),
                ("Variant_HasCorrectColor", Variant_HasCorrectColor()),
                ("Variant_GrowthCostDefaultsTo400", Variant_GrowthCostDefaultsTo400()),
                ("Variant_GetDisplayName_ReturnsBeeLoverSlime", Variant_GetDisplayName_ReturnsBeeLoverSlime()),
                ("Variant_GetDescription_NotEmpty", Variant_GetDescription_NotEmpty()),
                ("Config_HasBeeHuntingSettings", Config_HasBeeHuntingSettings()),
                ("Config_DefaultAttackRange_Is2f", Config_DefaultAttackRange_Is2f()),
                ("Config_DefaultPathfindingTimeout_Is10f", Config_DefaultPathfindingTimeout_Is10f()),
                ("Config_DefaultTeleportFallback_IsTrue", Config_DefaultTeleportFallback_IsTrue()),
                ("Config_DefaultSlimedBeehiveValue_Is175", Config_DefaultSlimedBeehiveValue_Is175()),
                ("Config_DefaultBeeDetectionRange_Is30f", Config_DefaultBeeDetectionRange_Is30f()),
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

            UnityEngine.Debug.Log($"[GoodForBusiness.Tests] BeeLoverSlimeVariantTests: {passed} passed, {failed} failed");

            if (failed > 0)
                throw new System.Exception($"{failed} test(s) failed");
        }
    }
}