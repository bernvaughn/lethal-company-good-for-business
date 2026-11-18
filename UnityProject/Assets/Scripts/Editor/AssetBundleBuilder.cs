using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEditor.Build.Pipeline.Interfaces;

namespace GoodForBusiness.Editor
{
    /// <summary>
    /// Asset Bundle build pipeline for GoodForBusiness mod.
    /// Builds asset bundles compatible with Lethal Company (Unity 2022.3.9f1).
    /// </summary>
    public static class AssetBundleBuilder
    {
        // Asset bundle configuration
        private const string BundleName = "gfbassets";
        private const string BundleVariant = "";
        private const BuildTarget TargetPlatform = BuildTarget.StandaloneWindows64;
        private const BuildAssetBundleOptions BuildOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode;

        // Output paths
        private static string OutputDirectory => Path.Combine(UnityEngine.Application.dataPath, "..", "Build", "AssetBundles", TargetPlatform.ToString());
        private static string PluginOutputDirectory => Path.Combine(UnityEngine.Application.dataPath, "..", "..", "GoodForBusiness", "Assets", "StreamingAssets");

        /// <summary>
        /// Menu item to build asset bundles for development.
        /// </summary>
        [MenuItem("GoodForBusiness/Build Asset Bundles (Development)")]
        public static void BuildAssetBundlesDevelopment()
        {
            BuildAssetBundles(false);
        }

        /// <summary>
        /// Menu item to build asset bundles for release.
        /// </summary>
        [MenuItem("GoodForBusiness/Build Asset Bundles (Release)")]
        public static void BuildAssetBundlesRelease()
        {
            BuildAssetBundles(true);
        }

        /// <summary>
        /// Menu item to clean build output.
        /// </summary>
        [MenuItem("GoodForBusiness/Clean Asset Bundle Build")]
        public static void CleanBuild()
        {
            if (Directory.Exists(OutputDirectory))
            {
                Directory.Delete(OutputDirectory, true);
                UnityEngine.Debug.Log($"[GoodForBusiness] Cleaned build directory: {OutputDirectory}");
            }

            if (Directory.Exists(PluginOutputDirectory))
            {
                Directory.Delete(PluginOutputDirectory, true);
                UnityEngine.Debug.Log($"[GoodForBusiness] Cleaned plugin output directory: {PluginOutputDirectory}");
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Builds the asset bundles.
        /// </summary>
        /// <param name="isRelease">Whether this is a release build.</param>
        private static void BuildAssetBundles(bool isRelease)
        {
            UnityEngine.Debug.Log($"[GoodForBusiness] Starting asset bundle build (Release: {isRelease})...");

            // Ensure output directories exist
            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(PluginOutputDirectory);

            // Find all assets marked with our bundle label
            string[] assetPaths = AssetDatabase.FindAssets($"l:{BundleName}", new[] { "Assets/Models", "Assets/Materials", "Assets/Animations", "Assets/Audio", "Assets/Prefabs" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();

            if (assetPaths.Length == 0)
            {
                UnityEngine.Debug.LogWarning($"[GoodForBusiness] No assets found with label '{BundleName}'. Please assign the '{BundleName}' label to assets in the Asset Bundle Browser or via script.");
                return;
            }

            UnityEngine.Debug.Log($"[GoodForBusiness] Found {assetPaths.Length} assets to bundle.");

            // Create AssetBundleBuild array
            var buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = BundleName;
            buildMap[0].assetNames = assetPaths;
            if (!string.IsNullOrEmpty(BundleVariant))
            {
                buildMap[0].assetBundleVariant = BundleVariant;
            }

            // Build the asset bundles using the legacy API (compatible with Unity 2022.3)
            var manifest = BuildPipeline.BuildAssetBundles(OutputDirectory, buildMap, BuildOptions, TargetPlatform);

            if (manifest == null)
            {
                UnityEngine.Debug.LogError("[GoodForBusiness] Asset bundle build failed!");
                return;
            }

            UnityEngine.Debug.Log($"[GoodForBusiness] Asset bundle build completed successfully.");
            UnityEngine.Debug.Log($"[GoodForBusiness] Output: {OutputDirectory}");

            // Copy to plugin directory for testing
            CopyToPluginDirectory();

            // Verify the bundle
            VerifyBundle(OutputDirectory);

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Copies the built asset bundle to the plugin's StreamingAssets folder.
        /// </summary>
        private static void CopyToPluginDirectory()
        {
            string sourceBundle = Path.Combine(OutputDirectory, BundleName);
            string destBundle = Path.Combine(PluginOutputDirectory, BundleName);

            if (File.Exists(sourceBundle))
            {
                File.Copy(sourceBundle, destBundle, true);
                UnityEngine.Debug.Log($"[GoodForBusiness] Copied bundle to plugin: {destBundle}");
            }

            // Also copy the manifest file
            string sourceManifest = Path.Combine(OutputDirectory, $"{BundleName}.manifest");
            string destManifest = Path.Combine(PluginOutputDirectory, $"{BundleName}.manifest");

            if (File.Exists(sourceManifest))
            {
                File.Copy(sourceManifest, destManifest, true);
                UnityEngine.Debug.Log($"[GoodForBusiness] Copied manifest to plugin: {destManifest}");
            }
        }

        /// <summary>
        /// Verifies the built asset bundle can be loaded.
        /// </summary>
        private static void VerifyBundle(string bundleDirectory)
        {
            string bundlePath = Path.Combine(bundleDirectory, BundleName);

            if (!File.Exists(bundlePath))
            {
                UnityEngine.Debug.LogError($"[GoodForBusiness] Bundle not found at: {bundlePath}");
                return;
            }

            // Load and verify
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            if (bundle == null)
            {
                UnityEngine.Debug.LogError("[GoodForBusiness] Failed to load built asset bundle!");
                return;
            }

            string[] assetNames = bundle.GetAllAssetNames();
            UnityEngine.Debug.Log($"[GoodForBusiness] Bundle contains {assetNames.Length} assets:");
            foreach (string name in assetNames)
            {
                UnityEngine.Debug.Log($"  - {name}");
            }

            bundle.Unload(false);
        }

        /// <summary>
        /// Applies the asset bundle label to all assets in the standard folders.
        /// Run this after adding new assets to ensure they're included in the bundle.
        /// </summary>
        [MenuItem("GoodForBusiness/Apply Asset Bundle Labels")]
        public static void ApplyAssetBundleLabels()
        {
            string[] folders = { "Assets/Models", "Assets/Materials", "Assets/Animations", "Assets/Audio", "Assets/Prefabs" };
            int labeledCount = 0;

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                    continue;

                string[] guids = AssetDatabase.FindAssets("", new[] { folder });
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path);
                    if (importer != null && importer.assetBundleName != BundleName)
                    {
                        importer.assetBundleName = BundleName;
                        labeledCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[GoodForBusiness] Applied '{BundleName}' label to {labeledCount} assets.");
        }

        /// <summary>
        /// Removes asset bundle labels from all assets.
        /// </summary>
        [MenuItem("GoodForBusiness/Remove Asset Bundle Labels")]
        public static void RemoveAssetBundleLabels()
        {
            string[] guids = AssetDatabase.FindAssets($"l:{BundleName}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.assetBundleName = "";
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[GoodForBusiness] Removed '{BundleName}' label from all assets.");
        }
    }
}