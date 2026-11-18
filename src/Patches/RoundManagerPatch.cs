using HarmonyLib;
using GoodForBusiness.Stubs.Game;
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
}