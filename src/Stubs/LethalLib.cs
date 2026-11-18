using System.Collections.Generic;
using GoodForBusiness.Stubs.Game;

namespace LethalLib.Modules
{
    /// <summary>
    /// Offline stub for LethalLib.Items. Replaced by the real LethalLib NuGet
    /// package when building against a game installation.
    /// </summary>
    public static class Items
    {
        public static Item? FindItemByName(string name) => null;
        public static void RegisterScrap(ScrapItemRegistration registration) { }
    }

    /// <summary>
    /// Offline stub for LethalLib.Enemies.
    /// </summary>
    public static class Enemies
    {
        public static EnemyType? GetEnemyType(string name) => null;
        public static void RegisterEnemy(EnemyType enemyType) { }
    }

    /// <summary>
    /// Offline stub for a LethalLib scrap registration.
    /// </summary>
    public class ScrapItemRegistration
    {
        public Item? Item;
        public Dictionary<LevelTypes, int> Rarities = new();
    }
}
