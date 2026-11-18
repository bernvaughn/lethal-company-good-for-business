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
    public class BeeLoverSlimeVariant : SlimeVariantBase
    {
        public override SlimeType Type => SlimeType.BeeLover;
        public override Color SlimeColor => new Color(1f, 0.8f, 0f);

        private PluginConfig? _config;

        public override int GrowthCost
        {
            get => _config?.BeeLoverSlimeGrowthCost.Value ?? 400;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
            // Register this variant
            SlimeVariantRegistry.Register(this);
        }

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility(SlimeAI slime)
        {
            if (StartOfRound.Instance == null)
                return;

            var detectionRange = _config?.BeeDetectionRange.Value ?? 30f;
            var beehive = FindNearestBeehive(detectionRange);

            if (beehive == null)
                return;

            SpawnSlimedBeehive(beehive.transform.position);
        }

        private EnemyAI? FindNearestBeehive(float range)
        {
            if (StartOfRound.Instance?.allPlayerScripts == null)
                return null;

            EnemyAI? nearestHive = null;
            float nearestDist = range * range;

            // Find all beehive enemies in the scene
            var allEnemies = UnityEngine.Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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

        private void SpawnSlimedBeehive(Vector3 position)
        {
            var beehiveItem = LethalLib.Modules.Items.FindItemByName("SlimedBeehive");
            if (beehiveItem == null || beehiveItem.spawnPrefab == null)
                return;

            var beehiveObj = UnityEngine.Object.Instantiate(beehiveItem.spawnPrefab, position, Quaternion.identity);
            var beehiveComponent = beehiveObj.GetComponent<SlimedBeehiveItem>();
            if (beehiveComponent != null)
            {
                beehiveComponent.CreditValue = _config?.SlimedBeehiveValue.Value ?? 175;
            }

            var networkObj = beehiveObj.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }
        }

        public override string GetDisplayName() => "Bee Lover Slime";
        public override string GetDescription() => "A yellow and black slime that hunts beehives.";
    }
}