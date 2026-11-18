using BepInEx.Configuration;

namespace GoodForBusiness.Configuration
{
    public class PluginConfig
    {
        private readonly ConfigFile _config;

        // General
        public ConfigEntry<bool> EnableMod { get; }
        public ConfigEntry<bool> DebugLogging { get; }

        // Egg spawning
        public ConfigEntry<float> EggGlobalSpawnChance { get; }
        public ConfigEntry<int> EggRarityExperiment { get; }
        public ConfigEntry<int> EggRarityAssurance { get; }
        public ConfigEntry<int> EggRarityVow { get; }
        public ConfigEntry<int> EggRarityMarch { get; }
        public ConfigEntry<int> EggRarityOffense { get; }
        public ConfigEntry<int> EggRarityRend { get; }
        public ConfigEntry<int> EggRarityDine { get; }
        public ConfigEntry<int> EggRarityModded { get; }

        // Equipment slime
        public ConfigEntry<bool> EnableEquipmentSlime { get; }
        public ConfigEntry<int> EquipmentSlimeGrowthCost { get; }
        public ConfigEntry<string> EquipmentDropTable { get; }

        // Bee lover slime
        public ConfigEntry<bool> EnableBeeLoverSlime { get; }
        public ConfigEntry<int> BeeLoverSlimeGrowthCost { get; }
        public ConfigEntry<int> SlimedBeehiveValue { get; }
        public ConfigEntry<float> BeeDetectionRange { get; }

        // Visuals
        public ConfigEntry<bool> SlimesFacePlayers { get; }
        public ConfigEntry<float> SlimeBobAmount { get; }

        public PluginConfig(ConfigFile config)
        {
            _config = config;

            // General
            EnableMod = config.Bind("General", "EnableMod", true, "Enable the GoodForBusiness mod.");
            DebugLogging = config.Bind("General", "DebugLogging", false, "Enable debug logging.");

            // Egg spawning
            EggGlobalSpawnChance = config.Bind("Egg Spawning", "GlobalSpawnChance", 1.0f,
                new ConfigDescription("Global multiplier for egg spawn chance (0.0-1.0).",
                    new AcceptableValueRange<float>(0f, 1f)));
            EggRarityExperiment = config.Bind("Egg Spawning", "RarityExperiment", 15, "Egg spawn rarity on Experiment.");
            EggRarityAssurance = config.Bind("Egg Spawning", "RarityAssurance", 10, "Egg spawn rarity on Assurance.");
            EggRarityVow = config.Bind("Egg Spawning", "RarityVow", 20, "Egg spawn rarity on Vow.");
            EggRarityMarch = config.Bind("Egg Spawning", "RarityMarch", 5, "Egg spawn rarity on March.");
            EggRarityOffense = config.Bind("Egg Spawning", "RarityOffense", 8, "Egg spawn rarity on Offense.");
            EggRarityRend = config.Bind("Egg Spawning", "RarityRend", 12, "Egg spawn rarity on Rend.");
            EggRarityDine = config.Bind("Egg Spawning", "RarityDine", 10, "Egg spawn rarity on Dine.");
            EggRarityModded = config.Bind("Egg Spawning", "RarityModded", 10, "Egg spawn rarity on modded moons.");

            // Equipment slime
            EnableEquipmentSlime = config.Bind("Equipment Slime", "Enable", true, "Enable the blue equipment slime variant.");
            EquipmentSlimeGrowthCost = config.Bind("Equipment Slime", "GrowthCost", 300, "Scrap value required to grow the equipment slime.");
            EquipmentDropTable = config.Bind("Equipment Slime", "DropTable", "[{\"item\":\"Flashlight\",\"weight\":10},{\"item\":\"Shovel\",\"weight\":10},{\"item\":\"StunGrenade\",\"weight\":5},{\"item\":\"ProbeScanner\",\"weight\":5}]", "JSON array of equipment items and weights for daily drops.");

            // Bee lover slime
            EnableBeeLoverSlime = config.Bind("Bee Lover Slime", "Enable", true, "Enable the yellow/black bee lover slime variant.");
            BeeLoverSlimeGrowthCost = config.Bind("Bee Lover Slime", "GrowthCost", 400, "Scrap value required to grow the bee lover slime.");
            SlimedBeehiveValue = config.Bind("Bee Lover Slime", "SlimedBeehiveValue", 175, "Credit value of a Slimed Beehive scrap item.");
            BeeDetectionRange = config.Bind("Bee Lover Slime", "BeeDetectionRange", 30f, "Maximum distance to detect beehives.");

            // Visuals
            SlimesFacePlayers = config.Bind("Visuals", "SlimesFacePlayers", true, "Slimes will face the nearest player.");
            SlimeBobAmount = config.Bind("Visuals", "SlimeBobAmount", 0.5f, "Amount slimes bob up and down while idle.");
        }
    }
}