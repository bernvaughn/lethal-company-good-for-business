using GoodForBusiness.Configuration;
using GoodForBusiness.Stubs.Game;
using UnityEngine;
using Unity.Netcode;

namespace GoodForBusiness.Items
{
    /// <summary>
    /// An equipment crate that spawns as visual feedback when an equipment slime
    /// uses its daily ability. Contains a single equipment item that players can
    /// pick up. Supports auto-pickup (contents transfer to nearest player) or
    /// ground spawn (crate stays on ground for manual pickup).
    /// </summary>
    public class EquipmentCrateItem : GrabbableObject
    {
        /// <summary>
        /// The equipment item contained inside this crate.
        /// Set before spawning so the crate knows what to give on pickup.
        /// </summary>
        public Item? ContainedItem { get; set; }

        /// <summary>
        /// Whether this crate should auto-pickup on spawn (transfer contents to
        /// nearest player) or stay on the ground for manual pickup.
        /// </summary>
        public bool AutoPickup { get; set; }

        /// <summary>
        /// Whether to play a particle effect / visual feedback on spawn.
        /// </summary>
        public bool PlaySpawnEffect { get; set; } = true;

        /// <summary>
        /// Credit value of the crate itself (0 = same as contained item).
        /// </summary>
        public int CreditValue { get; set; } = 0;

        private bool _hasBeenPickedUp = false;

        protected override void Awake()
        {
            base.Awake();

            // Set scanner name so players can identify the crate
            if (scanNodeProperties == null)
                scanNodeProperties = ScriptableObject.CreateInstance<ScanNodeProperties>();
            scanNodeProperties.headerText = "Equipment Crate";
            scanNodeProperties.subText = "Contains equipment dropped by a slime";

            // Default credit value to contained item's value if not set
            if (ContainedItem != null && CreditValue == 0)
            {
                CreditValue = ContainedItem.creditsWorth;
            }
            itemValue = CreditValue;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Initialize the crate on all clients when spawned over the network
            InitializeCrate();
        }

        /// <summary>
        /// Initializes the crate on spawn. Called on all clients via OnNetworkSpawn.
        /// </summary>
        private void InitializeCrate()
        {
            // Play visual feedback (particle effect) if enabled
            if (PlaySpawnEffect)
            {
                PlaySpawnEffects();
            }

            // Auto-pickup: transfer contents to nearest player immediately
            // Only run auto-pickup on the host to avoid duplicate item granting
            if (AutoPickup && IsServer)
            {
                TryAutoPickup();
            }
        }

        public override void OnPickup()
        {
            if (_hasBeenPickedUp)
                return;
            _hasBeenPickedUp = true;

            base.OnPickup();

            // Give the contained equipment to the player who picked up the crate
            if (ContainedItem != null && ContainedItem.spawnPrefab != null)
            {
                GiveContainedItemToPlayer();
            }

            // Destroy the crate after its contents are claimed
            if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        private void PlaySpawnEffects()
        {
            if (GoodForBusinessPlugin.Instance?.Config?.EquipmentCrateVisualFeedback.Value != true)
                return;

            // Play a particle effect at the crate position for visual feedback
            // In the real game this would spawn a particle system prefab.
            // For now, log the event and create a simple visual indicator.
            UnityEngine.Debug.Log(
                $"[GoodForBusiness] Equipment crate spawned at {transform.position} " +
                $"containing {ContainedItem?.name ?? "unknown item"}");

            // Create a simple visual indicator (a temporary glowing object)
            var effectObj = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere);
            effectObj.name = "EquipmentCrateSpawnEffect";
            effectObj.transform.position = transform.position + Vector3.up * 0.5f;
            effectObj.transform.localScale = Vector3.one * 0.3f;

            var renderer = effectObj.GetComponent<UnityEngine.MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.cyan;
            }

            // Auto-destroy the effect after a short delay
            UnityEngine.Object.Destroy(effectObj, 3f);
        }

        private void TryAutoPickup()
        {
            // Find the nearest player and transfer the crate contents to them
            var nearestPlayer = FindNearestPlayer();
            if (nearestPlayer == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] No player found for equipment crate auto-pickup.");
                return;
            }

            // Transfer the contained equipment to the player's inventory
            if (ContainedItem != null && ContainedItem.spawnPrefab != null)
            {
                GiveContainedItemToPlayer();
                UnityEngine.Debug.Log(
                    $"[GoodForBusiness] Equipment crate auto-picked up by player {nearestPlayer.playerClientId}. " +
                    $"Gave {ContainedItem.name}.");
            }

            // Despawn the crate since contents were transferred
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        private PlayerControllerB? FindNearestPlayer()
        {
            if (StartOfRound.Instance?.allPlayerScripts == null)
                return null;

            PlayerControllerB? nearestPlayer = null;
            float nearestDist = float.MaxValue;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player.isPlayerDead)
                    continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPlayer = player;
                }
            }

            return nearestPlayer;
        }

        private void GiveContainedItemToPlayer()
        {
            // Spawn the equipment prefab and place it in the nearest player's inventory
            // or drop it at their feet if inventory is full.
            var nearestPlayer = FindNearestPlayer();
            if (nearestPlayer == null || ContainedItem == null || ContainedItem.spawnPrefab == null)
                return;

            var equipmentObj = UnityEngine.Object.Instantiate(
                ContainedItem.spawnPrefab,
                nearestPlayer.transform.position + Vector3.up * 1f,
                UnityEngine.Quaternion.identity);

            // Try to place in an empty item slot
            bool placed = false;
            for (int i = 0; i < nearestPlayer.ItemSlots.Length; i++)
            {
                if (nearestPlayer.ItemSlots[i] == null)
                {
                    var grabbable = equipmentObj.GetComponent<GrabbableObject>();
                    if (grabbable != null)
                    {
                        nearestPlayer.ItemSlots[i] = grabbable;
                        placed = true;
                        break;
                    }
                }
            }

            if (!placed)
            {
                // Inventory full - drop the equipment at player's feet
                UnityEngine.Debug.Log(
                    $"[GoodForBusiness] Player inventory full, dropping {ContainedItem.name} at feet.");
                // equipmentObj stays in the world as a droppable item
            }

            // Set item value on the spawned equipment
            var itemComponent = equipmentObj.GetComponent<GrabbableObject>();
            if (itemComponent != null)
            {
                itemComponent.itemValue = ContainedItem.creditsWorth;
            }
        }

        public override void OnDrop()
        {
            base.OnDrop();
        }
    }
}