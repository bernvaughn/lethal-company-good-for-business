using GoodForBusiness.Configuration;
using GoodForBusiness.Core;
using GoodForBusiness.Stubs.Game;
using System.Collections.Generic;
using UnityEngine;

namespace GoodForBusiness.Egg
{
    public class SlimeEggSpawner : MonoBehaviour
    {
        public static SlimeEggSpawner Instance { get; private set; } = null!;

        private PluginConfig? _config;
        private AssetBundle? _assetBundle;
        public GameObject? EggPrefab { get; private set; }
        public float GlobalSpawnChance = 1.0f;

        private new void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private new void OnDestroy()
        {
            if (Instance == this)
                Instance = null!;
        }

        public void Initialize(PluginConfig config, AssetBundle assetBundle)
        {
            _config = config;
            _assetBundle = assetBundle;
            GlobalSpawnChance = _config.EggGlobalSpawnChance.Value;

            // Load egg prefab from bundle
            EggPrefab = _assetBundle.LoadAsset<GameObject>("Assets/Prefabs/BP_SlimeEgg.prefab")
                        ?? _assetBundle.LoadAsset<GameObject>("BP_SlimeEgg");
        }

        public void SpawnEggsInLevel()
        {
            if (_config == null || EggPrefab == null || !_config.EnableMod.Value)
                return;

            // Determine current level type
            var levelType = GetCurrentLevelType();
            int rarity = GetRarityForLevel(levelType);

            // Calculate spawn chance based on rarity and global multiplier
            // Higher rarity value = more common (as per LethalLib convention)
            float baseSpawnChance = Mathf.Clamp01(rarity / 100f);
            float spawnChance = GlobalSpawnChance * baseSpawnChance;

            if (Random.value > spawnChance)
                return;

            // Spawn egg at a valid position
            var position = GetRandomSpawnPosition();
            if (position == null)
                return;

            var egg = Instantiate(EggPrefab, position.Value, Quaternion.identity);
            var eggItem = egg.GetComponent<SlimeEggItem>();
            if (eggItem == null)
                eggItem = egg.AddComponent<SlimeEggItem>();

            // Randomly assign slime type based on enabled variants
            eggItem.EggType = SelectRandomSlimeType();
        }

        private LevelTypes GetCurrentLevelType()
        {
            // In real implementation, this would get the current level from RoundManager
            // For stubs, return a default
            return LevelTypes.Experiment;
        }

        private int GetRarityForLevel(LevelTypes levelType)
        {
            if (_config == null) return 10;

            return levelType switch
            {
                LevelTypes.Experiment => _config.EggRarityExperiment.Value,
                LevelTypes.Assurance => _config.EggRarityAssurance.Value,
                LevelTypes.Vow => _config.EggRarityVow.Value,
                LevelTypes.March => _config.EggRarityMarch.Value,
                LevelTypes.Offense => _config.EggRarityOffense.Value,
                LevelTypes.Rend => _config.EggRarityRend.Value,
                LevelTypes.Dine => _config.EggRarityDine.Value,
                LevelTypes.Modded => _config.EggRarityModded.Value,
                _ => 10
            };
        }

        private Vector3? GetRandomSpawnPosition()
        {
            // In real implementation, this would find valid spawn points in the dungeon
            // For now, return a position near the ship/entrance
            // This is a stub - actual implementation would use RoundManager's spawn points
            return new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f));
        }

        private SlimeType SelectRandomSlimeType()
        {
            var enabledTypes = new List<SlimeType>();
            if (_config?.EnableEquipmentSlime.Value == true)
                enabledTypes.Add(SlimeType.Equipment);
            if (_config?.EnableBeeLoverSlime.Value == true)
                enabledTypes.Add(SlimeType.BeeLover);

            if (enabledTypes.Count == 0)
                return SlimeType.Equipment;

            return enabledTypes[Random.Range(0, enabledTypes.Count)];
        }
    }
}