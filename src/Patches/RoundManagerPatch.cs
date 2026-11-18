using HarmonyLib;
using GoodForBusiness.Stubs.Game;
using GoodForBusiness.Core;
using GoodForBusiness.Egg;

namespace GoodForBusiness.Patches
{
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    public class RoundManagerPatch
    {
        [HarmonyPostfix]
        public static void OnSpawnScrapPatch()
        {
            // Hook into scrap spawning to add slime eggs
            SlimeEggSpawner.Instance?.SpawnEggsInLevel();
        }
    }

    [HarmonyPatch(typeof(RoundManager), "LoadNewLevel")]
    public class RoundManagerLoadLevelPatch
    {
        [HarmonyPostfix]
        public static void OnLoadNewLevelPatch()
        {
            // Restore slimes from save data after level loads
            SlimeManager.Instance?.RestoreSlimesFromSave();
        }
    }
}