using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Items;
using GoodForBusiness.Slimes.Variants;
using GoodForBusiness.Stubs.Game;
using GoodForBusiness.Networking;
using LethalLib.Modules;
using System.Collections;
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
        private Coroutine? _huntCoroutine;

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
            if (_slimeAI == null || _config == null)
                return;

            // Only grown slimes can execute the daily ability
            if (!slime.IsGrown)
                return;

            // Cancel any existing hunt coroutine
            if (_huntCoroutine != null)
                StopCoroutine(_huntCoroutine);

            _huntCoroutine = StartCoroutine(HuntBeehiveRoutine(slime));
        }

        /// <summary>
        /// Coroutine that handles the full bee-hunting sequence:
        /// pathfind to the nearest beehive, attack and destroy it,
        /// with teleport fallback if pathfinding fails, then spawn
        /// a Slimed Beehive at the hive location.
        /// </summary>
        private System.Collections.IEnumerator HuntBeehiveRoutine(SlimeAI slime)
        {
            var detectionRange = _config.BeeDetectionRange.Value;
            var attackRange = _config.BeeLoverSlimeAttackRange.Value;
            var pathfindingTimeout = _config.BeeLoverSlimePathfindingTimeout.Value;
            var teleportFallback = _config.BeeLoverSlimeTeleportFallbackEnabled.Value;

            // Find the nearest beehive within detection range
            var beehive = FindNearestBeehive(detectionRange);
            if (beehive == null)
            {
                UnityEngine.Debug.Log("[GoodForBusiness] Bee Lover Slime: No beehive found within detection range.");
                slime.SetStateServerRpc(SlimeState.Idle);
                yield break;
            }

            UnityEngine.Debug.Log($"[GoodForBusiness] Bee Lover Slime: Found beehive at {beehive.transform.position}, starting hunt.");

            // Set state to AbilityActive and allow variant-controlled movement
            slime.AbilityControlsMovement = true;
            slime.SetStateServerRpc(SlimeState.AbilityActive);
            slime.PlayAbilityEffects();

            var agent = slime.GetComponent<NavMeshAgent>();
            var networkTransform = slime.GetComponent<SlimeNetworkTransform>();
            Vector3 hivePosition = beehive.transform.position;
            bool reachedHive = false;

            if (agent != null)
            {
                // Set destination to the beehive
                agent.SetDestination(hivePosition);

                // Wait until we reach the hive or pathfinding fails/times out
                float elapsed = 0f;
                while (elapsed < pathfindingTimeout)
                {
                    if (agent.pathPending)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                        continue;
                    }

                    // Pathfinding failed - path is invalid or incomplete
                    if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                    {
                        UnityEngine.Debug.LogWarning("[GoodForBusiness] Bee Lover Slime: Pathfinding failed, invalid path.");
                        break;
                    }

                    // Check if we've reached attack range
                    float dist = Vector3.Distance(transform.position, hivePosition);
                    if (dist <= attackRange)
                    {
                        reachedHive = true;
                        UnityEngine.Debug.Log($"[GoodForBusiness] Bee Lover Slime: Reached beehive at distance {dist:F2}.");
                        break;
                    }

                    // If agent has stopped moving (stuck), break and try teleport
                    if (agent.velocity.magnitude < 0.01f && elapsed > 1f)
                    {
                        UnityEngine.Debug.LogWarning("[GoodForBusiness] Bee Lover Slime: Agent appears stuck, considering teleport fallback.");
                        break;
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // If we didn't reach the hive via pathfinding, try teleport fallback
            if (!reachedHive && teleportFallback && networkTransform != null)
            {
                UnityEngine.Debug.Log("[GoodForBusiness] Bee Lover Slime: Using teleport fallback to reach beehive.");
                networkTransform.Teleport(hivePosition, transform.rotation);
                reachedHive = true;
            }

            if (!reachedHive)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Bee Lover Slime: Could not reach beehive, ability aborted.");
                slime.AbilityControlsMovement = false;
                slime.SetStateServerRpc(SlimeState.Idle);
                _huntCoroutine = null;
                yield break;
            }

            // Attack and destroy the beehive
            DestroyBeehive(beehive);

            // Spawn Slimed Beehive at the hive location
            int creditValue = _config.SlimedBeehiveValue.Value;
            slime.SpawnItemAt("SlimedBeehive", hivePosition, creditValue);

            UnityEngine.Debug.Log("[GoodForBusiness] Bee Lover Slime: Beehive destroyed, Slimed Beehive spawned.");

            // Return to idle
            slime.AbilityControlsMovement = false;
            slime.SetStateServerRpc(SlimeState.Idle);
            _huntCoroutine = null;
        }

        /// <summary>
        /// Attacks and destroys a beehive enemy by marking it dead.
        /// </summary>
        private void DestroyBeehive(EnemyAI beehive)
        {
            if (beehive == null)
                return;

            // Mark the beehive as destroyed
            beehive.isEnemyDead = true;

            // Stop the beehive's AI
            beehive.DoAIInterval();

            UnityEngine.Debug.Log($"[GoodForBusiness] Bee Lover Slime: Destroyed beehive {beehive.enemyType?.enemyName ?? "unknown"}.");
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
                var name = enemy.enemyType.enemyName.ToLower();
                if (name.Contains("bee") || name.Contains("hive"))
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

        private void OnDestroy()
        {
            if (_huntCoroutine != null)
                StopCoroutine(_huntCoroutine);
        }
    }
}
