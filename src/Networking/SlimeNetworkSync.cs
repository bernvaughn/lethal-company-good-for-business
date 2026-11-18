using GoodForBusiness.Core;
using GoodForBusiness.Egg;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using UnityEngine;

namespace GoodForBusiness.Networking
{
    /// <summary>
    /// Network synchronization components and RPCs for slime entities.
    /// This class provides shared networking logic used by SlimeAI and related systems.
    /// </summary>
    public static class SlimeNetworkSync
    {
        /// <summary>
        /// RPC parameter struct for feeding a slime.
        /// </summary>
        public struct FeedSlimeRpcParams
        {
            public ulong NetworkObjectId;
            public int ScrapValue;
            public int PlayerId;
        }

        /// <summary>
        /// RPC parameter struct for hatching an egg.
        /// </summary>
        public struct HatchEggRpcParams
        {
            public Vector3 Position;
            public int SlimeType;
            public int OwnerPlayerId;
        }

        /// <summary>
        /// RPC parameter struct for triggering a daily ability.
        /// </summary>
        public struct TriggerAbilityRpcParams
        {
            public ulong NetworkObjectId;
        }

        /// <summary>
        /// RPC parameter struct for spawning an item (equipment crate, slimed beehive).
        /// </summary>
        public struct SpawnItemRpcParams
        {
            public string ItemName;
            public Vector3 Position;
            public Quaternion Rotation;
            public int CreditValue;
        }
    }

    /// <summary>
    /// Component that handles NetworkTransform setup for slimes.
    /// Attach this to the slime prefab to ensure proper position/rotation synchronization.
    /// </summary>
    public class SlimeNetworkTransform : NetworkBehaviour
    {
        private NetworkTransform? _networkTransform;

        protected override void Awake()
        {
            base.Awake();
            _networkTransform = GetComponent<NetworkTransform>();
            if (_networkTransform == null)
            {
                _networkTransform = gameObject.AddComponent<NetworkTransform>();
            }
            ConfigureNetworkTransform();
        }
        private void ConfigureNetworkTransform()
        {
            if (_networkTransform == null) return;

            // Configure for smooth movement interpolation
            _networkTransform.Interpolate = true;
            _networkTransform.TeleportThreshold = 5f;
            _networkTransform.SynchronizeRotation = true;
            _networkTransform.ScaleSynchronization = ScaleSynchronizationMode.None;
        }

        /// <summary>
        /// Force teleport the slime to a new position (bypasses interpolation).
        /// Should only be called on the server.
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            if (!IsServer) return;

            transform.SetPositionAndRotation(position, rotation);
            _networkTransform?.Teleport(position, rotation);
        }
    }

    /// <summary>
    /// Handles feeding slimes via RPC to ensure proper network synchronization.
    /// </summary>
    public class SlimeFeedingSync : NetworkBehaviour
    {
        /// <summary>
        /// Called by clients to request feeding a slime.
        /// The server validates and applies the feeding.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void FeedSlimeServerRpc(ulong networkObjectId, int scrapValue, int playerId)
        {
            if (!IsServer) return;

            var networkObj = NetworkManager.Singleton?.GetNetworkObject(networkObjectId);
            if (networkObj == null) return;

            var slimeAI = networkObj.GetComponent<SlimeAI>();
            if (slimeAI == null) return;

            // Validate the player is close enough to feed
            if (StartOfRound.Instance?.allPlayerScripts != null)
            {
                var player = StartOfRound.Instance.allPlayerScripts[playerId];
                if (player != null)
                {
                    float dist = Vector3.Distance(transform.position, player.transform.position);
                    if (dist > 5f) // Max feeding distance
                    {
                        UnityEngine.Debug.LogWarning($"[GoodForBusiness] Player {playerId} too far to feed slime {networkObjectId}");
                        return;
                    }
                }
            }

            // Apply feeding on server
            slimeAI.OnFedScrap(scrapValue);
            slimeAI.OwnerPlayerId = playerId;

            // Notify all clients of the feeding (for effects/animations)
            FeedSlimeClientRpc(networkObjectId, scrapValue, playerId);
        }

        /// <summary>
        /// Called on all clients to play feeding effects.
        /// </summary>
        [ClientRpc]
        public void FeedSlimeClientRpc(ulong networkObjectId, int scrapValue, int playerId)
        {
            var networkObj = NetworkManager.Singleton?.GetNetworkObject(networkObjectId);
            if (networkObj == null) return;

            var slimeAI = networkObj.GetComponent<SlimeAI>();
            if (slimeAI == null) return;

            // Play local effects (already handled by SlimeAI.OnFedScrap via ServerRpc)
            // This ensures clients that didn't call the ServerRpc still see effects
            if (!IsServer)
            {
                slimeAI.PlayEatEffects();
            }
        }
    }

    /// <summary>
    /// Handles egg hatching via RPC for proper network synchronization.
    /// </summary>
    public class SlimeHatchingSync : NetworkBehaviour
    {
        /// <summary>
        /// Called by the host to hatch an egg at a specific position.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void HatchEggServerRpc(Vector3 position, int slimeType, int ownerPlayerId)
        {
            if (!IsServer) return;

            var slimeEnemyType = Enemies.GetEnemyType("Slime");
            if (slimeEnemyType == null || slimeEnemyType.enemyPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime enemy type not registered, cannot hatch egg.");
                return;
            }

            var slimeObj = UnityEngine.Object.Instantiate(slimeEnemyType.enemyPrefab, position, Quaternion.identity);
            var slimeAI = slimeObj.GetComponent<SlimeAI>();
            if (slimeAI == null)
            {
                slimeAI = slimeObj.AddComponent<SlimeAI>();
            }

            slimeAI.SlimeType = (SlimeType)slimeType;
            slimeAI.OwnerPlayerId = ownerPlayerId;
            slimeAI.InitializeVariant();

            // Register with SlimeManager
            SlimeManager.Instance?.RegisterSlime(slimeAI);

            // Network spawn
            var networkObj = slimeObj.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }

            // Notify clients of successful hatch (for effects)
            HatchEggClientRpc(networkObj?.NetworkObjectId ?? 0, position, slimeType);
        }

        /// <summary>
        /// Called on all clients to play hatching effects.
        /// </summary>
        [ClientRpc]
        public void HatchEggClientRpc(ulong networkObjectId, Vector3 position, int slimeType)
        {
            // Play local hatching effects at position
            // The actual slime is already spawned via NetworkObject.Spawn()
            UnityEngine.Debug.Log($"[GoodForBusiness] Client received hatch notification for slime {networkObjectId} at {position}");
        }
    }

    /// <summary>
    /// Handles daily ability triggering via RPC.
    /// </summary>
    public class SlimeAbilitySync : NetworkBehaviour
    {
        /// <summary>
        /// Called by the host to trigger a slime's daily ability.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TriggerAbilityServerRpc(ulong networkObjectId)
        {
            if (!IsServer) return;

            var networkObj = NetworkManager.Singleton?.GetNetworkObject(networkObjectId);
            if (networkObj == null) return;

            var slimeAI = networkObj.GetComponent<SlimeAI>();
            if (slimeAI == null || !slimeAI.IsGrown) return;

            slimeAI.ExecuteDailyAbility();

            // Notify all clients
            TriggerAbilityClientRpc(networkObjectId);
        }

        /// <summary>
        /// Called on all clients to play ability effects.
        /// </summary>
        [ClientRpc]
        public void TriggerAbilityClientRpc(ulong networkObjectId)
        {
            var networkObj = NetworkManager.Singleton?.GetNetworkObject(networkObjectId);
            if (networkObj == null) return;

            var slimeAI = networkObj.GetComponent<SlimeAI>();
            if (slimeAI == null) return;

            // Play local ability effects
            if (!IsServer)
            {
                slimeAI.PlayAbilityEffects();
            }
        }
    }

    /// <summary>
    /// Handles item spawning (equipment crates, slimed beehives) via RPC.
    /// </summary>
    public class SlimeItemSpawnSync : NetworkBehaviour
    {
        /// <summary>
        /// Called by the host to spawn an item at a position.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SpawnItemServerRpc(string itemName, Vector3 position, Quaternion rotation, int creditValue)
        {
            if (!IsServer) return;

            var item = LethalLib.Modules.Items.FindItemByName(itemName);
            if (item == null || item.spawnPrefab == null)
            {
                UnityEngine.Debug.LogWarning($"[GoodForBusiness] Item '{itemName}' not found for spawning.");
                return;
            }

            var itemObj = UnityEngine.Object.Instantiate(item.spawnPrefab, position, rotation);

            // Set credit value if it's a SlimedBeehiveItem
            var beehiveItem = itemObj.GetComponent<GoodForBusiness.Items.SlimedBeehiveItem>();
            if (beehiveItem != null)
            {
                beehiveItem.CreditValue = creditValue;
            }

            var networkObj = itemObj.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }

            // Notify clients
            SpawnItemClientRpc(itemName, position, rotation, creditValue);
        }

        /// <summary>
        /// Called on all clients for item spawn effects (if needed).
        /// </summary>
        [ClientRpc]
        public void SpawnItemClientRpc(string itemName, Vector3 position, Quaternion rotation, int creditValue)
        {
            // Item is already spawned via NetworkObject.Spawn()
            // This is for any additional client-side effects
            UnityEngine.Debug.Log($"[GoodForBusiness] Client received item spawn: {itemName} at {position}");
        }
    }

    /// <summary>
    /// Handles slime state synchronization (growth, grown status).
    /// </summary>
    public class SlimeStateSync : NetworkBehaviour
    {
        /// <summary>
        /// Called by the server to sync growth progress to all clients.
        /// </summary>
        [ClientRpc]
        public void SyncGrowthProgressClientRpc(ulong networkObjectId, float progress, bool isGrown)
        {
            var networkObj = NetworkManager.Singleton?.GetNetworkObject(networkObjectId);
            if (networkObj == null) return;

            var slimeAI = networkObj.GetComponent<SlimeAI>();
            if (slimeAI == null) return;

            slimeAI.GrowthProgress = progress;
            slimeAI.IsGrown = isGrown;

            // Update visual state on clients
            if (slimeAI.IsGrown && !IsServer)
            {
                // Trigger growth visual change
                slimeAI.TriggerAnimationClientRpc("Grow");
            }
        }

        /// <summary>
        /// Called by the server to sync slime state.
        /// </summary>
        [ClientRpc]
        public void SyncStateClientRpc(ulong networkObjectId, int state)
        {
            var networkObj = NetworkManager.Singleton?.GetNetworkObject(networkObjectId);
            if (networkObj == null) return;

            var slimeAI = networkObj.GetComponent<SlimeAI>();
            if (slimeAI == null) return;

            slimeAI.CurrentState = (SlimeState)state;
        }
    }
}