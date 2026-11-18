using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Items;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using UnityEngine;

namespace GoodForBusiness.Slimes
{
    public class BeeLoverSlimeVariant : SlimeVariantBase
    {
        public override SlimeType Type => SlimeType.BeeLover;
        public override Color SlimeColor => new Color(1f, 0.8f, 0f);

        private PluginConfig? _config;
        private SlimeAI? _slimeAI;

        public override int GrowthCost
        {
            get => _config?.BeeLoverSlimeGrowthCost.Value ?? 400;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
            _slimeAI = GetComponent<SlimeAI>();
        }

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility()
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
            if (StartOfRound.Instance == null)
                return null;

            // Find all enemies in the scene and look for beehives
            var enemies = FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            EnemyAI? nearestHive = null;
            float nearestDist = range;

            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.enemyType == null)
                    continue;

                // Check if this is a beehive (by name or type)
                string enemyName = enemy.enemyType.name.ToLower();
                if (enemyName.Contains("bee") || enemyName.Contains("hive"))
                {
                    float dist = Vector3.Distance(_slimeAI!.transform.position, enemy.transform.position);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearestHive = enemy;
                    }
                }
            }

            return nearestHive;
        }

        public override string GetDisplayName() => "Bee Lover Slime";
        public override string GetDescription() => "A yellow and black slime that hunts beehives.";
    }
}