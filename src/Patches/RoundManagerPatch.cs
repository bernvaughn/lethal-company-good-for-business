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
            // Only the host should spawn eggs to prevent desyncs
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost)
                return;

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
            // Only host spawns the network objects; clients receive them via NetworkObject.Spawn()
            SlimeManager.Instance?.RestoreSlimesFromSave();
        }
    }
}