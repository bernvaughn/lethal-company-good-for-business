using HarmonyLib;
using GoodForBusiness.Stubs.Game;
using GoodForBusiness.Core;
using GoodForBusiness.Egg;
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
            // In real implementation, iterate through all players' inventories
            // Find SlimeEggItem instances and hatch them
        }

        private static void HatchEggsOnShip()
        {
            // In real implementation, check ship storage for eggs
        }
    }
}