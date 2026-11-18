using System;
using UnityEditor;
using UnityEngine;

namespace GoodForBusiness.Editor
{
    /// <summary>
    /// Automatically applies asset bundle labels to assets in designated folders.
    /// </summary>
    [InitializeOnLoad]
    public class AssetBundlePostProcessor : AssetPostprocessor
    {
        private const string BundleName = "gfbassets";
        private static readonly string[] WatchFolders = new[]
        {
            "Assets/Models",
            "Assets/Materials",
            "Assets/Animations",
            "Assets/Audio",
            "Assets/Prefabs",
            "Assets/Items"
        };

        static AssetBundlePostProcessor()
        {
            // Subscribe to asset import events
            AssetPostprocessor.assetImported += OnAssetImported;
        }

        private static void OnAssetImported(string assetPath)
        {
            // Skip if not in our watched folders
            bool inWatchedFolder = false;
            foreach (string folder in WatchFolders)
            {
                if (assetPath.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
                {
                    inWatchedFolder = true;
                    break;
                }
            }

            if (!inWatchedFolder)
                return;

            // Skip meta files, scripts, and asmdefs
            if (assetPath.EndsWith(".meta") || assetPath.EndsWith(".cs") || assetPath.EndsWith(".asmdef"))
                return;

            // Apply asset bundle label
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && importer.assetBundleName != BundleName)
            {
                importer.assetBundleName = BundleName;
                UnityEngine.Debug.Log($"[GoodForBusiness] Auto-applied bundle label '{BundleName}' to: {assetPath}");
            }
        }

        /// <summary>
        /// Validates asset bundle setup on domain reload.
        /// </summary>
        [MenuItem("GoodForBusiness/Validate Asset Bundle Setup")]
        public static void ValidateAssetBundleSetup()
        {
            UnityEngine.Debug.Log("[GoodForBusiness] Validating asset bundle setup...");

            bool hasIssues = false;

            // Check Asset Bundle Browser package
            var manifest = UnityEditor.PackageManager.PackageInfo.GetForAssembly(typeof(AssetBundlePostProcessor).Assembly);
            bool hasBrowserPackage = false;
            foreach (var pkg in UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages())
            {
                if (pkg.name == "com.unity.assetbundlebrowser")
                {
                    hasBrowserPackage = true;
                    UnityEngine.Debug.Log($"[GoodForBusiness] Asset Bundle Browser package found: {pkg.version}");
                    break;
                }
            }

            if (!hasBrowserPackage)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Asset Bundle Browser package not found. Install via Package Manager.");
                hasIssues = true;
            }

            // Check bundle name consistency
            string[] labeledAssets = AssetDatabase.FindAssets($"l:{BundleName}");
            UnityEngine.Debug.Log($"[GoodForBusiness] Assets with '{BundleName}' label: {labeledAssets.Length}");

            if (labeledAssets.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] No assets labeled for bundling. Import assets to watched folders or use 'Apply Asset Bundle Labels' menu.");
                hasIssues = true;
            }

            // Verify output directory is writable
            string outputDir = Path.Combine(Application.dataPath, "..", "Build", "AssetBundles");
            try
            {
                Directory.CreateDirectory(outputDir);
                string testFile = Path.Combine(outputDir, ".write_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
                UnityEngine.Debug.Log($"[GoodForBusiness] Output directory writable: {outputDir}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[GoodForBusiness] Output directory not writable: {outputDir} - {e.Message}");
                hasIssues = true;
            }

            if (!hasIssues)
            {
                UnityEngine.Debug.Log("[GoodForBusiness] Asset bundle setup validation passed!");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Asset bundle setup has issues. See warnings above.");
            }
        }
    }
}