using BepInEx;
using HarmonyLib;

namespace GoodForBusiness
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Lethal Company.exe")]
    public class GoodForBusinessPlugin : BaseUnityPlugin
    {
        public static GoodForBusinessPlugin Instance { get; private set; } = null!;

        public GoodForBusinessPlugin()
        {
            Instance = this;
        }

        public new void Awake()
        {
            Logger.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded.");
        }

        public new void Start()
        {
            // Initialize subsystems here (config, systems, patches)
        }

        public new void OnDestroy()
        {
            Instance = null!;
        }
    }
}