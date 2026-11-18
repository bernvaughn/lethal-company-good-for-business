using GoodForBusiness.Configuration;
using GoodForBusiness.Core;
using GoodForBusiness.Egg;
using GoodForBusiness.Items;
using GoodForBusiness.Slimes;
using LethalLib.Modules;
using UnityEngine;

namespace GoodForBusiness.Core
{
    /// <summary>
    /// Registers custom items and enemies with LethalLib.
    /// </summary>
    public static class LethalLibRegistration
    {
        private static PluginConfig? _config;
        private static AssetBundle? _assetBundle;

        /// <summary>
        /// Initializes and registers all custom content with LethalLib.
        /// </summary>
        /// <param name="config">The plugin configuration.</param>
        /// <param name="assetBundle">The loaded asset bundle.</param>
        public static void RegisterAll(PluginConfig config, AssetBundle assetBundle)
        {
            _config = config;
            _assetBundle = assetBundle;

            RegisterSlimeEgg();
            RegisterSlimedBeehive();
            RegisterEquipmentCrate();
            RegisterSlimeEnemy();
        }

        /// <summary>
        /// Registers the Slime Egg as custom scrap.
        /// </summary>
        private static void RegisterSlimeEgg()
        {
            if (_assetBundle == null || _config == null)
                return;

            // Load the Slime Egg Item asset from the bundle
            var slimeEggItemAsset = _assetBundle.LoadAsset<Item>("Assets/Items/ITEM_SlimeEgg.asset");
            if (slimeEggItemAsset == null)
            {
                // Try alternative paths
                slimeEggItemAsset = _assetBundle.LoadAsset<Item>("ITEM_SlimeEgg");
            }

            if (slimeEggItemAsset == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime Egg Item asset not found in bundle. Skipping registration.");
                return;
            }

            // Load the Slime Egg prefab
            var slimeEggPrefab = _assetBundle.LoadAsset<GameObject>("Assets/Prefabs/BP_SlimeEgg.prefab");
            if (slimeEggPrefab == null)
            {
                slimeEggPrefab = _assetBundle.LoadAsset<GameObject>("BP_SlimeEgg");
            }

            if (slimeEggPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime Egg prefab not found in bundle. Skipping registration.");
                return;
            }

            // Ensure the prefab has the SlimeEggItem component
            var eggComponent = slimeEggPrefab.GetComponent<SlimeEggItem>();
            if (eggComponent == null)
            {
                eggComponent = slimeEggPrefab.AddComponent<SlimeEggItem>();
            }

            // Set the spawn prefab on the item asset
            slimeEggItemAsset.spawnPrefab = slimeEggPrefab;

            // Register as scrap with LethalLib
            var scrapRegistration = new ScrapItemRegistration
            {
                Item = slimeEggItemAsset,
                Rarities = new System.Collections.Generic.Dictionary<LevelTypes, int>
                {
                    { LevelTypes.Experiment, _config.EggRarityExperiment.Value },
                    { LevelTypes.Assurance, _config.EggRarityAssurance.Value },
                    { LevelTypes.Vow, _config.EggRarityVow.Value },
                    { LevelTypes.March, _config.EggRarityMarch.Value },
                    { LevelTypes.Offense, _config.EggRarityOffense.Value },
                    { LevelTypes.Rend, _config.EggRarityRend.Value },
                    { LevelTypes.Dine, _config.EggRarityDine.Value },
                    { LevelTypes.Modded, _config.EggRarityModded.Value },
                }
            };

            Items.RegisterScrap(scrapRegistration);
            UnityEngine.Debug.Log("[GoodForBusiness] Registered Slime Egg as custom scrap.");
        }

        /// <summary>
        /// Registers the Slimed Beehive as custom scrap.
        /// </summary>
        private static void RegisterSlimedBeehive()
        {
            if (_assetBundle == null || _config == null)
                return;

            var beehiveItemAsset = _assetBundle.LoadAsset<Item>("Assets/Items/ITEM_SlimedBeehive.asset");
            if (beehiveItemAsset == null)
                beehiveItemAsset = _assetBundle.LoadAsset<Item>("ITEM_SlimedBeehive");

            if (beehiveItemAsset == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slimed Beehive Item asset not found in bundle. Skipping registration.");
                return;
            }

            var beehivePrefab = _assetBundle.LoadAsset<GameObject>("Assets/Prefabs/BP_SlimedBeehive.prefab");
            if (beehivePrefab == null)
                beehivePrefab = _assetBundle.LoadAsset<GameObject>("BP_SlimedBeehive");

            if (beehivePrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slimed Beehive prefab not found in bundle. Skipping registration.");
                return;
            }

            var beehiveComponent = beehivePrefab.GetComponent<SlimedBeehiveItem>();
            if (beehiveComponent == null)
                beehiveComponent = beehivePrefab.AddComponent<SlimedBeehiveItem>();

            beehiveComponent.CreditValue = _config.SlimedBeehiveValue.Value;
            beehiveItemAsset.spawnPrefab = beehivePrefab;

            var scrapRegistration = new ScrapItemRegistration
            {
                Item = beehiveItemAsset,
                Rarities = new System.Collections.Generic.Dictionary<LevelTypes, int>
                {
                    // Slimed Beehive is not naturally spawned - it's produced by the Bee Lover Slime
                    { LevelTypes.Experiment, 0 },
                    { LevelTypes.Assurance, 0 },
                    { LevelTypes.Vow, 0 },
                    { LevelTypes.March, 0 },
                    { LevelTypes.Offense, 0 },
                    { LevelTypes.Rend, 0 },
                    { LevelTypes.Dine, 0 },
                    { LevelTypes.Modded, 0 },
                }
            };

            Items.RegisterScrap(scrapRegistration);
            UnityEngine.Debug.Log("[GoodForBusiness] Registered Slimed Beehive as custom scrap.");
        }

        /// <summary>
        /// Registers the Equipment Crate as custom scrap.
        /// </summary>
        private static void RegisterEquipmentCrate()
        {
            if (_assetBundle == null)
                return;

            var crateItemAsset = _assetBundle.LoadAsset<Item>("Assets/Items/ITEM_EquipmentCrate.asset");
            if (crateItemAsset == null)
                crateItemAsset = _assetBundle.LoadAsset<Item>("ITEM_EquipmentCrate");

            if (crateItemAsset == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Equipment Crate Item asset not found in bundle. Skipping registration.");
                return;
            }

            var cratePrefab = _assetBundle.LoadAsset<GameObject>("Assets/Prefabs/BP_EquipmentCrate.prefab");
            if (cratePrefab == null)
                cratePrefab = _assetBundle.LoadAsset<GameObject>("BP_EquipmentCrate");

            if (cratePrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Equipment Crate prefab not found in bundle. Skipping registration.");
                return;
            }

            var crateComponent = cratePrefab.GetComponent<EquipmentCrateItem>();
            if (crateComponent == null)
                crateComponent = cratePrefab.AddComponent<EquipmentCrateItem>();

            crateItemAsset.spawnPrefab = cratePrefab;

            var scrapRegistration = new ScrapItemRegistration
            {
                Item = crateItemAsset,
                Rarities = new System.Collections.Generic.Dictionary<LevelTypes, int>
                {
                    { LevelTypes.Experiment, 0 },
                    { LevelTypes.Assurance, 0 },
                    { LevelTypes.Vow, 0 },
                    { LevelTypes.March, 0 },
                    { LevelTypes.Offense, 0 },
                    { LevelTypes.Rend, 0 },
                    { LevelTypes.Dine, 0 },
                    { LevelTypes.Modded, 0 },
                }
            };

            Items.RegisterScrap(scrapRegistration);
            UnityEngine.Debug.Log("[GoodForBusiness] Registered Equipment Crate as custom scrap.");
        }

        /// <summary>
        /// Registers the Slime base enemy type.
        /// </summary>
        private static void RegisterSlimeEnemy()
        {
            if (_assetBundle == null)
                return;

            var slimeEnemyType = _assetBundle.LoadAsset<EnemyType>("Assets/Items/ENEMY_SlimeBase.asset");
            if (slimeEnemyType == null)
                slimeEnemyType = _assetBundle.LoadAsset<EnemyType>("ENEMY_SlimeBase");

            if (slimeEnemyType == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime EnemyType asset not found in bundle. Skipping registration.");
                return;
            }

            var slimePrefab = _assetBundle.LoadAsset<GameObject>("Assets/Prefabs/BP_SlimeBase.prefab");
            if (slimePrefab == null)
                slimePrefab = _assetBundle.LoadAsset<GameObject>("BP_SlimeBase");

            if (slimePrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime Base prefab not found in bundle. Skipping registration.");
                return;
            }

            var slimeAI = slimePrefab.GetComponent<SlimeAI>();
            if (slimeAI == null)
                slimeAI = slimePrefab.AddComponent<SlimeAI>();

            slimeEnemyType.enemyPrefab = slimePrefab;
            slimeEnemyType.PowerLevel = 0; // Passive
            slimeEnemyType.spawnWeight = 0; // Not naturally spawned - handled by SlimeManager

            Enemies.RegisterEnemy(slimeEnemyType);
            UnityEngine.Debug.Log("[GoodForBusiness] Registered Slime as custom enemy.");
        }
    }
}