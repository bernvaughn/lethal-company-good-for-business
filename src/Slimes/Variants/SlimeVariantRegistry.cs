using System;
using System.Collections.Generic;
using GoodForBusiness.Core;
using UnityEngine;

namespace GoodForBusiness.Slimes.Variants
{
    /// <summary>
    /// Central registry for slime variants. Handles registration, lookup,
    /// and instantiation of variant implementations.
    /// </summary>
    public static class SlimeVariantRegistry
    {
        private static readonly Dictionary<SlimeType, ISlimeVariant> _variants = new();
        private static readonly Dictionary<SlimeType, Type> _variantTypes = new();

        /// <summary>
        /// Registers a slime variant implementation.
        /// </summary>
        /// <typeparam name="TVariant">The variant type implementing ISlimeVariant.</typeparam>
        /// <param name="variantInstance">An instance of the variant (used for static property access).</param>
        public static void Register<TVariant>(TVariant variantInstance) where TVariant : ISlimeVariant
        {
            if (variantInstance == null)
            {
                throw new ArgumentNullException(nameof(variantInstance));
            }

            var type = variantInstance.Type;
            if (_variants.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning($"[GoodForBusiness] Slime variant {type} already registered. Overwriting.");
            }

            _variants[type] = variantInstance;
            _variantTypes[type] = typeof(TVariant);
            UnityEngine.Debug.Log($"[GoodForBusiness] Registered slime variant: {type} ({variantInstance.GetDisplayName()})");
        }

        /// <summary>
        /// Registers a slime variant by type. Creates an instance using the default constructor.
        /// </summary>
        /// <typeparam name="TVariant">The variant type implementing ISlimeVariant.</typeparam>
        public static void Register<TVariant>() where TVariant : ISlimeVariant, new()
        {
            var instance = new TVariant();
            Register(instance);
        }

        /// <summary>
        /// Gets a registered variant by type.
        /// </summary>
        /// <param name="type">The slime type to look up.</param>
        /// <returns>The variant instance, or null if not registered.</returns>
        public static ISlimeVariant? GetVariant(SlimeType type)
        {
            _variants.TryGetValue(type, out var variant);
            return variant;
        }

        /// <summary>
        /// Tries to get a registered variant by type.
        /// </summary>
        /// <param name="type">The slime type to look up.</param>
        /// <param name="variant">Output parameter for the variant instance.</param>
        /// <returns>True if the variant was found; otherwise, false.</returns>
        public static bool TryGetVariant(SlimeType type, out ISlimeVariant? variant)
        {
            return _variants.TryGetValue(type, out variant);
        }

        /// <summary>
        /// Creates a new instance of the registered variant type.
        /// </summary>
        /// <param name="type">The slime type to instantiate.</param>
        /// <returns>A new instance of the variant, or null if not registered.</returns>
        public static ISlimeVariant? CreateVariant(SlimeType type)
        {
            if (_variantTypes.TryGetValue(type, out var variantType))
            {
                return Activator.CreateInstance(variantType) as ISlimeVariant;
            }
            return null;
        }

        /// <summary>
        /// Checks if a variant type is registered.
        /// </summary>
        public static bool IsRegistered(SlimeType type)
        {
            return _variants.ContainsKey(type);
        }

        /// <summary>
        /// Gets all registered variant types.
        /// </summary>
        public static IReadOnlyList<SlimeType> GetRegisteredTypes()
        {
            return new List<SlimeType>(_variants.Keys).AsReadOnly();
        }

        /// <summary>
        /// Gets all registered variant instances.
        /// </summary>
        public static IReadOnlyList<ISlimeVariant> GetAllVariants()
        {
            return new List<ISlimeVariant>(_variants.Values).AsReadOnly();
        }

        /// <summary>
        /// Unregisters a variant type. Primarily for testing.
        /// </summary>
        public static void Unregister(SlimeType type)
        {
            _variants.Remove(type);
            _variantTypes.Remove(type);
        }

        /// <summary>
        /// Clears all registered variants. Primarily for testing.
        /// </summary>
        public static void Clear()
        {
            _variants.Clear();
            _variantTypes.Clear();
        }
    }
}