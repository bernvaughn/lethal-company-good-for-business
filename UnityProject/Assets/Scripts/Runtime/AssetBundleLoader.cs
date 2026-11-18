using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace GoodForBusiness
{
    /// <summary>
    /// Loads and manages the GoodForBusiness asset bundle at runtime.
    /// Compatible with Lethal Company's loading pipeline.
    /// </summary>
    public static class AssetBundleLoader
    {
        private const string BundleName = "gfbassets";
        private static AssetBundle _bundle;
        private static bool _isLoading;
        private static bool _loadAttempted;

        /// <summary>
        /// The loaded asset bundle, or null if not loaded.
        /// </summary>
        public static AssetBundle Bundle => _bundle;

        /// <summary>
        /// Whether the bundle is currently loaded.
        /// </summary>
        public static bool IsLoaded => _bundle != null;

        /// <summary>
        /// Whether a load operation is in progress.
        /// </summary>
        public static bool IsLoading => _isLoading;

        /// <summary>
        /// Loads the asset bundle from the plugin's StreamingAssets folder.
        /// </summary>
        /// <param name="pluginPath">Path to the plugin DLL directory.</param>
        /// <returns>True if loading started successfully.</returns>
        public static bool LoadBundle(string pluginPath)
        {
            if (_bundle != null)
            {
                UnityEngine.Debug.Log("[GoodForBusiness] Asset bundle already loaded.");
                return true;
            }

            if (_isLoading)
            {
                UnityEngine.Debug.Log("[GoodForBusiness] Asset bundle load already in progress.");
                return false;
            }

            if (_loadAttempted)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Asset bundle load already attempted and failed.");
                return false;
            }

            string bundlePath = Path.Combine(pluginPath, "StreamingAssets", BundleName);

            if (!File.Exists(bundlePath))
            {
                UnityEngine.Debug.LogError($"[GoodForBusiness] Asset bundle not found at: {bundlePath}");
                _loadAttempted = true;
                return false;
            }

            _isLoading = true;
            _loadAttempted = true;

            // Load synchronously for simplicity (Lethal Company loads mods synchronously on startup)
            try
            {
                _bundle = AssetBundle.LoadFromFile(bundlePath);

                if (_bundle == null)
                {
                    UnityEngine.Debug.LogError($"[GoodForBusiness] Failed to load asset bundle from: {bundlePath}");
                    _isLoading = false;
                    return false;
                }

                UnityEngine.Debug.Log($"[GoodForBusiness] Asset bundle loaded successfully: {_bundle.name}");
                UnityEngine.Debug.Log($"[GoodForBusiness] Bundle contains {_bundle.GetAllAssetNames().Length} assets.");

                _isLoading = false;
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[GoodForBusiness] Exception loading asset bundle: {e}");
                _isLoading = false;
                return false;
            }
        }

        /// <summary>
        /// Loads an asset of type T from the bundle.
        /// </summary>
        /// <typeparam name="T">Type of asset to load.</typeparam>
        /// <param name="assetName">Name of the asset (without extension).</param>
        /// <returns>The loaded asset, or null if not found.</returns>
        public static T LoadAsset<T>(string assetName) where T : UnityEngine.Object
        {
            if (_bundle == null)
            {
                UnityEngine.Debug.LogError($"[GoodForBusiness] Cannot load asset '{assetName}': Bundle not loaded.");
                return null;
            }

            // Try exact name first
            var asset = _bundle.LoadAsset<T>(assetName);
            if (asset != null)
                return asset;

            // Try with common prefixes
            string[] prefixes = { "Assets/Models/", "Assets/Prefabs/", "Assets/Materials/", "Assets/Animations/", "Assets/Audio/", "Assets/Items/" };
            foreach (string prefix in prefixes)
            {
                asset = _bundle.LoadAsset<T>(prefix + assetName);
                if (asset != null)
                    return asset;

                // Try with extension
                string ext = GetExtensionForType(typeof(T));
                if (!string.IsNullOrEmpty(ext))
                {
                    asset = _bundle.LoadAsset<T>(prefix + assetName + ext);
                    if (asset != null)
                        return asset;
                }
            }

            // Try loading all assets and finding by name
            var allAssets = _bundle.LoadAllAssets<T>();
            foreach (var a in allAssets)
            {
                if (a.name == assetName || a.name.EndsWith(assetName))
                    return a;
            }

            UnityEngine.Debug.LogWarning($"[GoodForBusiness] Asset '{assetName}' of type {typeof(T).Name} not found in bundle.");
            return null;
        }

        /// <summary>
        /// Loads all assets of type T from the bundle.
        /// </summary>
        public static T[] LoadAllAssets<T>() where T : UnityEngine.Object
        {
            if (_bundle == null)
            {
                UnityEngine.Debug.LogError("[GoodForBusiness] Cannot load assets: Bundle not loaded.");
                return Array.Empty<T>();
            }

            return _bundle.LoadAllAssets<T>();
        }

        /// <summary>
        /// Gets all asset names in the bundle.
        /// </summary>
        public static string[] GetAllAssetNames()
        {
            if (_bundle == null)
                return Array.Empty<string>();

            return _bundle.GetAllAssetNames();
        }

        /// <summary>
        /// Unloads the asset bundle.
        /// </summary>
        /// <param name="unloadAllLoadedObjects">Whether to unload all objects loaded from this bundle.</param>
        public static void UnloadBundle(bool unloadAllLoadedObjects = false)
        {
            if (_bundle != null)
            {
                _bundle.Unload(unloadAllLoadedObjects);
                _bundle = null;
                _isLoading = false;
                _loadAttempted = false;
                UnityEngine.Debug.Log("[GoodForBusiness] Asset bundle unloaded.");
            }
        }

        private static string GetExtensionForType(Type type)
        {
            if (typeof(GameObject).IsAssignableFrom(type)) return ".prefab";
            if (typeof(Material).IsAssignableFrom(type)) return ".mat";
            if (typeof(AnimationClip).IsAssignableFrom(type)) return ".anim";
            if (typeof(AudioClip).IsAssignableFrom(type)) return ".wav";
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return ".asset";
            return "";
        }
    }
}