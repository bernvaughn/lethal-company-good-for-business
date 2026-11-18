using System;
using UnityEditor;
using UnityEngine;

namespace GoodForBusiness.Editor
{
    /// <summary>
    /// Asset naming conventions and validation for GoodForBusiness mod.
    /// Ensures consistency across all assets in the bundle.
    /// </summary>
    public static class AssetNamingConventions
    {
        // Naming prefixes by asset type
        public const string Prefix_Model = "SM_";      // Static Mesh / Model
        public const string Prefix_Prefab = "BP_";     // Blueprint / Prefab
        public const string Prefix_Material = "M_";    // Material
        public const string Prefix_Texture = "T_";     // Texture
        public const string Prefix_Animation = "ANIM_"; // Animation Clip
        public const string Prefix_Audio = "SFX_";     // Sound Effect
        public const string Prefix_ScriptableObject = "SO_"; // ScriptableObject
        public const string Prefix_Item = "ITEM_";     // Item Asset (LethalLib)
        public const string Prefix_Enemy = "ENEMY_";   // EnemyType Asset (LethalLib)

        // Suffixes for variants
        public const string Suffix_Variant = "_Var";
        public const string Suffix_LOD0 = "_LOD0";
        public const string Suffix_LOD1 = "_LOD1";

        // Folder structure
        public const string Folder_Models = "Assets/Models";
        public const string Folder_Materials = "Assets/Materials";
        public const string Folder_Animations = "Assets/Animations";
        public const string Folder_Audio = "Assets/Audio";
        public const string Folder_Prefabs = "Assets/Prefabs";
        public const string Folder_Items = "Assets/Items"; // For LethalLib Item/EnemyType assets

        // Expected asset names for this mod
        public static readonly ExpectedAsset[] ExpectedAssets = new[]
        {
            // Models
            new ExpectedAsset("SlimeEgg", AssetType.Model, "SM_SlimeEgg"),
            new ExpectedAsset("SlimeBase", AssetType.Model, "SM_SlimeBase"),
            new ExpectedAsset("EquipmentCrate", AssetType.Model, "SM_EquipmentCrate"),
            new ExpectedAsset("SlimedBeehive", AssetType.Model, "SM_SlimedBeehive"),

            // Prefabs
            new ExpectedAsset("SlimeEgg", AssetType.Prefab, "BP_SlimeEgg"),
            new ExpectedAsset("SlimeBase", AssetType.Prefab, "BP_SlimeBase"),
            new ExpectedAsset("SlimeEquipment", AssetType.Prefab, "BP_SlimeEquipment"),
            new ExpectedAsset("SlimeBeeLover", AssetType.Prefab, "BP_SlimeBeeLover"),
            new ExpectedAsset("EquipmentCrate", AssetType.Prefab, "BP_EquipmentCrate"),
            new ExpectedAsset("SlimedBeehive", AssetType.Prefab, "BP_SlimedBeehive"),

            // Materials
            new ExpectedAsset("SlimeEgg", AssetType.Material, "M_SlimeEgg"),
            new ExpectedAsset("SlimeBlue", AssetType.Material, "M_SlimeBlue"),
            new ExpectedAsset("SlimeYellowBlack", AssetType.Material, "M_SlimeYellowBlack"),
            new ExpectedAsset("SlimedBeehive", AssetType.Material, "M_SlimedBeehive"),
            new ExpectedAsset("EquipmentCrate", AssetType.Material, "M_EquipmentCrate"),

            // Animations
            new ExpectedAsset("Hatch", AssetType.Animation, "ANIM_Slime_Hatch"),
            new ExpectedAsset("Idle", AssetType.Animation, "ANIM_Slime_Idle"),
            new ExpectedAsset("Eat", AssetType.Animation, "ANIM_Slime_Eat"),
            new ExpectedAsset("Grow", AssetType.Animation, "ANIM_Slime_Grow"),
            new ExpectedAsset("Ability", AssetType.Animation, "ANIM_Slime_Ability"),

            // Audio
            new ExpectedAsset("Hatch", AssetType.Audio, "SFX_Slime_Hatch"),
            new ExpectedAsset("Eat", AssetType.Audio, "SFX_Slime_Eat"),
            new ExpectedAsset("Grow", AssetType.Audio, "SFX_Slime_Grow"),
            new ExpectedAsset("Ability", AssetType.Audio, "SFX_Slime_Ability"),

            // LethalLib Assets (ScriptableObjects)
            new ExpectedAsset("SlimeEgg", AssetType.Item, "ITEM_SlimeEgg"),
            new ExpectedAsset("SlimeBase", AssetType.Enemy, "ENEMY_SlimeBase"),
            new ExpectedAsset("SlimedBeehive", AssetType.Item, "ITEM_SlimedBeehive"),
            new ExpectedAsset("EquipmentCrate", AssetType.Item, "ITEM_EquipmentCrate"),
        };

        public enum AssetType
        {
            Model,
            Prefab,
            Material,
            Texture,
            Animation,
            Audio,
            Item,      // LethalLib Item asset
            Enemy,     // LethalLib EnemyType asset
            ScriptableObject
        }

        public struct ExpectedAsset
        {
            public string LogicalName;
            public AssetType Type;
            public string ExpectedFileName;

            public ExpectedAsset(string logicalName, AssetType type, string expectedFileName)
            {
                LogicalName = logicalName;
                Type = type;
                ExpectedFileName = expectedFileName;
            }
        }

        /// <summary>
        /// Validates all assets in the project follow naming conventions.
        /// </summary>
        [MenuItem("GoodForBusiness/Validate Asset Naming")]
        public static void ValidateNamingConventions()
        {
            int errors = 0;
            int warnings = 0;

            UnityEngine.Debug.Log("[GoodForBusiness] Validating asset naming conventions...");

            // Check each expected asset
            foreach (var expected in ExpectedAssets)
            {
                string folder = GetFolderForType(expected.Type);
                string fullPath = Path.Combine(folder, expected.ExpectedFileName + GetExtensionForType(expected.Type));

                if (!AssetDatabase.IsValidFolder(folder))
                {
                    UnityEngine.Debug.LogWarning($"[GoodForBusiness] Folder missing: {folder}");
                    warnings++;
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fullPath);
                if (asset == null)
                {
                    UnityEngine.Debug.LogWarning($"[GoodForBusiness] Missing expected asset: {expected.ExpectedFileName} (Logical: {expected.LogicalName}, Type: {expected.Type})");
                    warnings++;
                }
                else
                {
                    UnityEngine.Debug.Log($"[GoodForBusiness] Found: {fullPath}");
                }
            }

            // Check for assets not following naming conventions
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in allAssetPaths)
            {
                if (!path.StartsWith("Assets/") || path.EndsWith(".cs") || path.EndsWith(".asmdef") || path.EndsWith(".meta"))
                    continue;

                string fileName = Path.GetFileNameWithoutExtension(path);
                string folder = Path.GetDirectoryName(path);

                // Skip editor scripts and non-content folders
                if (folder.Contains("/Editor") || folder.Contains("/Scripts"))
                    continue;

                AssetType? type = GetTypeFromFolder(folder);
                if (type.HasValue)
                {
                    string expectedPrefix = GetPrefixForType(type.Value);
                    if (!fileName.StartsWith(expectedPrefix))
                    {
                        UnityEngine.Debug.LogWarning($"[GoodForBusiness] Naming convention violation: {path} (Expected prefix: {expectedPrefix})");
                        warnings++;
                    }
                }
            }

            UnityEngine.Debug.Log($"[GoodForBusiness] Validation complete. Errors: {errors}, Warnings: {warnings}");
        }

        /// <summary>
        /// Auto-renames assets to follow naming conventions (use with caution).
        /// </summary>
        [MenuItem("GoodForBusiness/Auto-Fix Asset Naming")]
        public static void AutoFixNamingConventions()
        {
            UnityEngine.Debug.Log("[GoodForBusiness] Auto-fix not implemented - use manual rename to avoid breaking references.");
        }

        private static string GetFolderForType(AssetType type)
        {
            switch (type)
            {
                case AssetType.Model: return Folder_Models;
                case AssetType.Prefab: return Folder_Prefabs;
                case AssetType.Material: return Folder_Materials;
                case AssetType.Texture: return Folder_Materials; // Textures typically with materials
                case AssetType.Animation: return Folder_Animations;
                case AssetType.Audio: return Folder_Audio;
                case AssetType.Item: return Folder_Items;
                case AssetType.Enemy: return Folder_Items;
                case AssetType.ScriptableObject: return Folder_Items;
                default: return "Assets";
            }
        }

        private static string GetPrefixForType(AssetType type)
        {
            switch (type)
            {
                case AssetType.Model: return Prefix_Model;
                case AssetType.Prefab: return Prefix_Prefab;
                case AssetType.Material: return Prefix_Material;
                case AssetType.Texture: return Prefix_Texture;
                case AssetType.Animation: return Prefix_Animation;
                case AssetType.Audio: return Prefix_Audio;
                case AssetType.Item: return Prefix_Item;
                case AssetType.Enemy: return Prefix_Enemy;
                case AssetType.ScriptableObject: return Prefix_ScriptableObject;
                default: return "";
            }
        }

        private static string GetExtensionForType(AssetType type)
        {
            switch (type)
            {
                case AssetType.Model: return ".fbx";
                case AssetType.Prefab: return ".prefab";
                case AssetType.Material: return ".mat";
                case AssetType.Texture: return ".png";
                case AssetType.Animation: return ".anim";
                case AssetType.Audio: return ".wav";
                case AssetType.Item: return ".asset";
                case AssetType.Enemy: return ".asset";
                case AssetType.ScriptableObject: return ".asset";
                default: return "";
            }
        }

        private static AssetType? GetTypeFromFolder(string folder)
        {
            if (folder.Contains("/Models")) return AssetType.Model;
            if (folder.Contains("/Prefabs")) return AssetType.Prefab;
            if (folder.Contains("/Materials")) return AssetType.Material;
            if (folder.Contains("/Animations")) return AssetType.Animation;
            if (folder.Contains("/Audio")) return AssetType.Audio;
            if (folder.Contains("/Items")) return AssetType.Item; // Could be Item or Enemy
            return null;
        }
    }
}