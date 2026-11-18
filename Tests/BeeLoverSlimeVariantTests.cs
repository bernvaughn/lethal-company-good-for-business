using GoodForBusiness.Core;
using GoodForBusiness.Slimes;
using UnityEngine;

namespace GoodForBusiness.Tests
{
    public class BeeLoverSlimeVariantTests
    {
        public static bool Type_IsBeeLover()
        {
            var variant = new BeeLoverSlimeVariant();
            return variant.Type == SlimeType.BeeLover;
        }

        public static bool GrowthCost_DefaultsTo400()
        {
            // Without a config instance, the variant falls back to the default value.
            var variant = new BeeLoverSlimeVariant();
            return variant.GrowthCost == 400;
        }

        public static bool SlimeColor_IsYellow()
        {
            var variant = new BeeLoverSlimeVariant();
            var color = variant.SlimeColor;
            // Yellow/black variant: yellow base (high red+green, low blue).
            return color.r == 1f && color.g == 0.8f && color.b == 0f;
        }

        public static bool GetDisplayName_IsNotEmpty()
        {
            var variant = new BeeLoverSlimeVariant();
            return !string.IsNullOrEmpty(variant.GetDisplayName());
        }

        public static void RunAll()
        {
            if (!Type_IsBeeLover()) throw new System.Exception("Type is not BeeLover");
            if (!GrowthCost_DefaultsTo400()) throw new System.Exception("GrowthCost is not 400");
            if (!SlimeColor_IsYellow()) throw new System.Exception("SlimeColor is not yellow");
            if (!GetDisplayName_IsNotEmpty()) throw new System.Exception("Display name is empty");

            UnityEngine.Debug.Log("[GoodForBusiness.Tests] BeeLoverSlimeVariantTests: all passed");
        }
    }
}
