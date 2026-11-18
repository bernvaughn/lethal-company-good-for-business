using HarmonyLib;
using GoodForBusiness.Stubs.Game;
using GoodForBusiness.Core;
using GoodForBusiness.Egg;
using GoodForBusiness.Slimes;
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
                        HatchEgg(eggItem);
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
                    HatchEgg(eggItem);
                    ShipBuildModeManager.Instance.shipStorage[i] = null;
                }
            }
        }

        private static void HatchEgg(SlimeEggItem eggItem)
        {
            if (eggItem == null || eggItem.EggType == SlimeType.None)
                return;

            eggItem.StartHatching();

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
            slimeAI.InitializeVariant();

            // Register with SlimeManager
            SlimeManager.Instance?.RegisterSlime(slimeAI);

            // Network spawn if host
            var networkObj = slimeObj.GetComponent<NetworkObject>();
            if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
            {
                networkObj.Spawn();
            }

            // Destroy the egg
            UnityEngine.Object.Destroy(eggItem.gameObject);
        }
    }
}