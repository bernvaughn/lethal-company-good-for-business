using BepInEx;
using HarmonyLib;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using GoodForBusiness.Configuration;
using GoodForBusiness.Core;
using GoodForBusiness.Patches;
using GoodForBusiness.Egg;
using GoodForBusiness.Slimes.Variants;
using LethalLib.Modules;

namespace GoodForBusiness
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("Evaisa.LethalLib")]
    [BepInProcess("Lethal Company.exe")]
    public class GoodForBusinessPlugin : BaseUnityPlugin
    {
        public static GoodForBusinessPlugin Instance { get; private set; } = null!;

        public AssetBundle? AssetBundle { get; private set; }
        public PluginConfig? Config { get; private set; }
        public SlimeEggSpawner? EggSpawner { get; private set; }
        public SlimeManager? SlimeManager { get; private set; }

        public GoodForBusinessPlugin()
        {
            Instance = this;
        }

        public new void Awake()
        {
            Logger.LogInfo($"{PluginInfo.Name} v{PluginInfo.Version} loaded.");

            // Initialize configuration
            Config = new PluginConfig(base.Config);

            // Load asset bundle
            LoadAssetBundle();

            // Initialize slime visuals with base material from asset bundle
            InitializeSlimeVisuals();

            // Register with LethalLib (requires asset bundle)
            if (AssetBundle != null && Config != null)
            {
                LethalLibRegistration.RegisterAll(Config, AssetBundle);
            }

            // Apply Harmony patches
            var harmony = new Harmony(PluginInfo.GUID);
            harmony.PatchAll();

            // Initialize egg spawner
            InitializeEggSpawner();

            // Initialize slime manager singleton
            InitializeSlimeManager();
        }

        private void InitializeSlimeVisuals()
        {
            if (AssetBundle == null) return;

            try
            {
                // Try to load base slime material from asset bundle
                var baseMaterial = AssetBundle.LoadAsset<Material>("Assets/Materials/Slime_Base.mat");
                if (baseMaterial != null)
                {
                    SlimeVisuals.Initialize(baseMaterial);
                    Logger.LogInfo("[GoodForBusiness] Slime visuals initialized with base material.");
                }
                else
                {
                    Logger.LogWarning("[GoodForBusiness] Base slime material not found in asset bundle.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"[GoodForBusiness] Failed to initialize slime visuals: {ex.Message}");
            }
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

        private void InitializeEggSpawner()
        {
            var spawnerObj = new GameObject("SlimeEggSpawner");
            DontDestroyOnLoad(spawnerObj);
            EggSpawner = spawnerObj.AddComponent<SlimeEggSpawner>();

            if (Config != null && AssetBundle != null)
            {
                EggSpawner.Initialize(Config, AssetBundle);
            }
            else
            {
                Logger.LogWarning("Cannot initialize egg spawner: config or asset bundle missing.");
            }
        }

        private void InitializeSlimeManager()
        {
            var managerObj = new GameObject("SlimeManager");
            DontDestroyOnLoad(managerObj);
            SlimeManager = managerObj.AddComponent<SlimeManager>();
            Logger.LogInfo("[GoodForBusiness] Slime Manager singleton initialized.");
        }
    }
}