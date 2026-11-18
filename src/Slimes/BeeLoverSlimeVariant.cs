using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Items;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using UnityEngine;

namespace GoodForBusiness.Slimes
{
    public class BeeLoverSlimeVariant : SlimeVariantBase
    {
        public override SlimeType Type => SlimeType.BeeLover;
        public override Color SlimeColor => new Color(1f, 0.8f, 0f); // Yellow/black striped - base yellow

        private PluginConfig? _config;

        public override int GrowthCost
        {
            get => _config?.BeeLoverSlimeGrowthCost.Value ?? 400;
        }

        private void Awake()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;
        }

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility()
        {
            if (StartOfRound.Instance == null)
                return;

            // Find nearest beehive
            var detectionRange = _config?.BeeDetectionRange.Value ?? 30f;
            var beehive = FindNearestBeehive(detectionRange);
            
            if (beehive == null)
                return;

            // Destroy the beehive (in real game, this would trigger the hive destruction logic)
            // For now, just spawn a Slimed Beehive at the hive location
            SpawnSlimedBeehive(beehive.transform.position);
        }

        private EnemyAI? FindNearestBeehive(float range)
        {
            // In real implementation, search for Beehive enemy types
            // For stub, return null
            return null;
        }

        private void SpawnSlimedBeehive(Vector3 position)
        {
            var beehiveItem = Items.FindItemByName("SlimedBeehive");
            if (beehiveItem == null || beehiveItem.spawnPrefab == null)
                return;

            var beehiveObj = UnityEngine.Object.Instantiate(beehiveItem.spawnPrefab, position, Quaternion.identity);
            var beehiveComponent = beehiveObj.GetComponent<SlimedBeehiveItem>();
            if (beehiveComponent != null)
            {
                beehiveComponent.CreditValue = _config?.SlimedBeehiveValue.Value ?? 175;
            }

            // Network spawn if host
            var networkObj = beehiveObj.GetComponent<NetworkObject>();
            if (networkObj != null && Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }
        }

        public override string GetDisplayName() => "Bee Lover Slime";
        public override string GetDescription() => "A yellow and black slime that hunts beehives.";
    }
}