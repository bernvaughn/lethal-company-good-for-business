using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;

namespace GoodForBusiness
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInProcess("Lethal Company.exe")]
    public class GoodForBusinessPlugin : BaseUnityPlugin
    {
        public static GoodForBusinessPlugin Instance { get; private set; } = null!;

        public AssetBundle? AssetBundle { get; private set; }

        public GoodForBusinessPlugin()
        {
            Instance = this;
        }

        public new void Awake()
        {
            Logger.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded.");
            LoadAssetBundle();
        }

        public new void Start()
        {
            // Initialize subsystems here (config, systems, patches)
        }

        public new void OnDestroy()
        {
            AssetBundle?.Unload(true);
            Instance = null!;
        }

        private void LoadAssetBundle()
        {
            try
            {
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(assemblyDir))
                {
                    Logger.LogError("Could not determine assembly directory for asset bundle loading.");
                    return;
                }

                var bundlePath = Path.Combine(assemblyDir, "gfbassets");
                if (!File.Exists(bundlePath))
                {
                    Logger.LogWarning($"Asset bundle not found at {bundlePath}. Asset-dependent features will be unavailable.");
                    return;
                }

                AssetBundle = AssetBundle.LoadFromFile(bundlePath);
                if (AssetBundle == null)
                {
                    Logger.LogError($"Failed to load asset bundle from {bundlePath}.");
                    return;
                }

                Logger.LogInfo($"Asset bundle loaded from {bundlePath}.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Exception loading asset bundle: {ex.Message}");
            }
        }
    }
}