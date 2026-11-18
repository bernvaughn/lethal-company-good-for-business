using HarmonyLib;
using GoodForBusiness.Stubs.Game;
using GoodForBusiness.Core;
using GoodForBusiness.Egg;
using GoodForBusiness.Slimes;
using GoodForBusiness.Networking;
using LethalLib.Modules;
using UnityEngine;

namespace GoodForBusiness.Patches
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.QuotaReset))]
    public class StartOfRoundPatch
    {
        [HarmonyPostfix]
        public static void OnNewDayPatch()
        {
            // Only the host should handle egg hatching and daily abilities
            // to prevent duplicate execution across clients
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
                return;

            // Called when a new day starts - trigger slime hatching and daily abilities
            SlimeManager.Instance?.OnNewDay();

            // Check for eggs in player inventories and on ship to hatch
            HatchEggsInInventory();
            HatchEggsOnShip();
        }

        private static void HatchEggsInInventory()
        {
            if (StartOfRound.Instance == null || StartOfRound.Instance.allPlayerScripts == null)
                return;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || player.ItemSlots == null)
                    continue;

                for (int i = 0; i < player.ItemSlots.Length; i++)
                {
                    var item = player.ItemSlots[i];
                    if (item is SlimeEggItem eggItem && !eggItem.IsHatching)
                    {
                        HatchEgg(eggItem, player.playerClientId);
                        player.ItemSlots[i] = null;
                    }
                }
            }
        }

        private static void HatchEggsOnShip()
        {
            if (ShipBuildModeManager.Instance == null || ShipBuildModeManager.Instance.shipStorage == null)
                return;

            for (int i = 0; i < ShipBuildModeManager.Instance.shipStorage.Length; i++)
            {
                var item = ShipBuildModeManager.Instance.shipStorage[i];
                if (item is SlimeEggItem eggItem && !eggItem.IsHatching)
                {
                    HatchEgg(eggItem, -1);
                    ShipBuildModeManager.Instance.shipStorage[i] = null;
                }
            }
        }

        private static void HatchEgg(SlimeEggItem eggItem, int ownerPlayerId)
        {
            if (eggItem == null || eggItem.EggType == SlimeType.None)
                return;

            eggItem.StartHatching();

            // Use the network sync component to hatch the egg properly
            var hatchingSync = FindObjectOfType<SlimeHatchingSync>();
            if (hatchingSync != null)
            {
                hatchingSync.HatchEggServerRpc(eggItem.transform.position, (int)eggItem.EggType, ownerPlayerId);
            }
            else
            {
                // Fallback to direct instantiation if no sync component found
                HatchEggDirect(eggItem, ownerPlayerId);
            }

            // Destroy the egg
            UnityEngine.Object.Destroy(eggItem.gameObject);
        }

        private static void HatchEggDirect(SlimeEggItem eggItem, int ownerPlayerId)
        {
            // Get the slime enemy prefab from LethalLib registration
            var slimeEnemyType = Enemies.GetEnemyType("Slime");
            if (slimeEnemyType == null || slimeEnemyType.enemyPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime enemy type not registered, cannot hatch egg.");
                return;
            }

            // Spawn the slime at the egg's position
            var slimeObj = UnityEngine.Object.Instantiate(slimeEnemyType.enemyPrefab, eggItem.transform.position, Quaternion.identity);
            var slimeAI = slimeObj.GetComponent<SlimeAI>();
            if (slimeAI == null)
            {
                slimeAI = slimeObj.AddComponent<SlimeAI>();
            }

            slimeAI.SlimeType = eggItem.EggType;
            slimeAI.OwnerPlayerId = ownerPlayerId;
            slimeAI.InitializeVariant();

            // Register with SlimeManager
            SlimeManager.Instance?.RegisterSlime(slimeAI);

            // Network spawn (host only)
            var networkObj = slimeObj.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }
        }
    }
}