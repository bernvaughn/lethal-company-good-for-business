using System.Collections.Generic;
using GoodForBusiness.Core;
using UnityEngine;

namespace GoodForBusiness.Slimes.Variants
{
    /// <summary>
    /// Handles visual appearance of slimes including color/material swapping
    /// for different variants. Supports both material property blocks and
    /// material replacement for compatibility with different shader setups.
    /// </summary>
    public static class SlimeVisuals
    {
        private static readonly Dictionary<SlimeType, Material> _variantMaterials = new();
        private static Material? _baseMaterial;
        private static readonly int _colorPropertyId = Shader.PropertyToID("_BaseColor");
        private static readonly int _emissionPropertyId = Shader.PropertyToID("_EmissionColor");
        private static readonly int _metallicPropertyId = Shader.PropertyToID("_Metallic");
        private static readonly int _smoothnessPropertyId = Shader.PropertyToID("_Smoothness");

        /// <summary>
        /// Initializes the visual system with a base material.
        /// Call this once during game initialization (e.g., from asset bundle load).
        /// </summary>
        /// <param name="baseMaterial">The base slime material to clone and tint.</param>
        public static void Initialize(Material baseMaterial)
        {
            _baseMaterial = baseMaterial;
            _variantMaterials.Clear();
        }

        /// <summary>
        /// Registers a custom material for a specific variant type.
        /// Use this when variants need completely different materials (e.g., different shaders).
        /// </summary>
        /// <param name="type">The slime variant type.</param>
        /// <param name="material">The material to use for this variant.</param>
        public static void RegisterVariantMaterial(SlimeType type, Material material)
        {
            if (material == null)
            {
                UnityEngine.Debug.LogWarning($"[GoodForBusiness] Attempted to register null material for {type}");
                return;
            }
            _variantMaterials[type] = material;
        }

        /// <summary>
        /// Applies the correct visual appearance for a slime variant to the given renderers.
        /// </summary>
        /// <param name="slimeType">The slime variant type.</param>
        /// <param name="renderers">The renderers to apply the visuals to.</param>
        /// <param name="variant">Optional variant instance for accessing variant-specific properties.</param>
        public static void ApplyVariantVisuals(SlimeType slimeType, IEnumerable<Renderer> renderers, ISlimeVariant? variant = null)
        {
            if (renderers == null) return;

            var color = variant?.SlimeColor ?? GetDefaultColor(slimeType);
            var material = GetOrCreateMaterial(slimeType, color);

            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // Use MaterialPropertyBlock for efficient per-instance color changes
                // when using the base material, otherwise swap the shared material
                if (material == _baseMaterial || _variantMaterials.ContainsKey(slimeType) == false)
                {
                    var propBlock = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(propBlock);
                    propBlock.SetColor(_colorPropertyId, color);
                    // Add subtle emission for visibility
                    propBlock.SetColor(_emissionPropertyId, color * 0.3f);
                    propBlock.SetFloat(_metallicPropertyId, 0.1f);
                    propBlock.SetFloat(_smoothnessPropertyId, 0.8f);
                    renderer.SetPropertyBlock(propBlock);
                }
                else
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        /// <summary>
        /// Applies variant visuals to a single GameObject and its children.
        /// </summary>
        /// <param name="slimeType">The slime variant type.</param>
        /// <param name="root">The root GameObject containing renderers.</param>
        /// <param name="variant">Optional variant instance.</param>
        public static void ApplyVariantVisuals(SlimeType slimeType, GameObject root, ISlimeVariant? variant = null)
        {
            if (root == null) return;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            ApplyVariantVisuals(slimeType, renderers, variant);
        }

        /// <summary>
        /// Applies variant visuals to a SlimeAI instance.
        /// </summary>
        /// <param name="slime">The slime to update visuals for.</param>
        public static void ApplyVariantVisuals(SlimeAI slime)
        {
            if (slime == null) return;
            var variant = SlimeVariantRegistry.GetVariant(slime.SlimeType);
            ApplyVariantVisuals(slime.SlimeType, slime.gameObject, variant);
        }

        /// <summary>
        /// Gets or creates a material for the given variant type and color.
        /// </summary>
        private static Material GetOrCreateMaterial(SlimeType type, Color color)
        {
            // If a custom material is registered for this type, use it
            if (_variantMaterials.TryGetValue(type, out var customMaterial))
            {
                return customMaterial;
            }

            // Otherwise, use the base material with a property block (handled in ApplyVariantVisuals)
            return _baseMaterial ?? new Material(Shader.Find("Standard"));
        }

        /// <summary>
        /// Gets the default color for a slime type if no variant is registered.
        /// </summary>
        private static Color GetDefaultColor(SlimeType type)
        {
            return type switch
            {
                SlimeType.Equipment => Color.blue,
                SlimeType.BeeLover => new Color(1f, 0.8f, 0f), // Yellow/Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// Creates a tinted copy of the base material for a specific variant.
        /// Use this when you need persistent material instances (e.g., for asset bundles).
        /// </summary>
        /// <param name="type">The slime variant type.</param>
        /// <param name="color">The color to tint the material.</param>
        /// <returns>A new material instance with the tint applied.</returns>
        public static Material CreateTintedMaterial(SlimeType type, Color color)
        {
            if (_baseMaterial == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Base material not initialized, creating default");
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                return mat;
            }

            var newMat = new Material(_baseMaterial);
            newMat.color = color;
            newMat.SetColor(_emissionPropertyId, color * 0.3f);
            newMat.name = $"Slime_{type}_Material";
            return newMat;
        }

        /// <summary>
        /// Resets the visual system. Primarily for testing or asset bundle reload.
        /// </summary>
        public static void Reset()
        {
            _baseMaterial = null;
            _variantMaterials.Clear();
        }
    }
}