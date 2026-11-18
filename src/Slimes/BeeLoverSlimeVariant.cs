using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Items;
using GoodForBusiness.Slimes.Variants;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using UnityEngine;
using UnityEngine.AI;

namespace GoodForBusiness.Slimes
{
    public class BeeLoverSlimeVariant : MonoBehaviour, ISlimeVariant
    {
        public SlimeType Type => SlimeType.BeeLover;
        public Color SlimeColor => new Color(1f, 0.8f, 0f);

        private PluginConfig? _config;
        private SlimeAI? _slimeAI;

        public int GrowthCost
        {
            get => _config?.BeeLoverSlimeGrowthCost.Value ?? 400;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
            _slimeAI = GetComponent<SlimeAI>();
            // Register this variant
            SlimeVariantRegistry.Register(this);
        }

        public void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public void ExecuteDailyAbility(SlimeAI slime)
        {
            if (StartOfRound.Instance == null || _slimeAI == null)
                return;

            var detectionRange = _config?.BeeDetectionRange.Value ?? 30f;
            var beehive = FindNearestBeehive(detectionRange);

            if (beehive == null)
                return;

            int creditValue = _config?.SlimedBeehiveValue.Value ?? 175;
            _slimeAI.SpawnItemAt("SlimedBeehive", beehive.transform.position, creditValue);
        }

        private EnemyAI? FindNearestBeehive(float range)
        {
            if (StartOfRound.Instance?.allPlayerScripts == null)
                return null;

            EnemyAI? nearestHive = null;
            float nearestDist = range * range;

            // Find all beehive enemies in the scene
            var allEnemies = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
            foreach (var enemy in allEnemies)
            {
                if (enemy == null || enemy.enemyType == null)
                    continue;

                // Check if this is a beehive enemy
                if (enemy.enemyType.enemyName.ToLower().Contains("bee") || 
                    enemy.enemyType.enemyName.ToLower().Contains("hive"))
                {
                    float dist = Vector3.SqrMagnitude(transform.position - enemy.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestHive = enemy;
                    }
                }
            }

            return nearestHive;
        }

        public string GetDisplayName() => "Bee Lover Slime";
        public string GetDescription() => "A yellow and black slime that hunts beehives.";
    }
}