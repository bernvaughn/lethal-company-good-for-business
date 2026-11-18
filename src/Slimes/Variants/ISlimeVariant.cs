using GoodForBusiness.Core;
using GoodForBusiness.Slimes;
using UnityEngine;

namespace GoodForBusiness.Slimes.Variants
{
    /// <summary>
    /// Interface defining the contract for slime variant implementations.
    /// Each slime type (Equipment, BeeLover, etc.) must implement this interface.
    /// </summary>
    public interface ISlimeVariant
    {
        /// <summary>
        /// The unique type identifier for this slime variant.
        /// </summary>
        SlimeType Type { get; }

        /// <summary>
        /// The scrap value required for this slime to grow from baby to adult.
        /// </summary>
        int GrowthCost { get; }

        /// <summary>
        /// The primary color associated with this slime variant.
        /// Used for visual identification and material tinting.
        /// </summary>
        Color SlimeColor { get; }

        /// <summary>
        /// Called when the slime is fed scrap. The variant can implement custom
        /// feeding behavior (e.g., special effects, bonus progress).
        /// </summary>
        /// <param name="scrapValue">The credit value of the scrap fed to the slime.</param>
        void OnFedScrap(int scrapValue);

        /// <summary>
        /// Executes the daily ability for this slime variant.
        /// Called once per day for grown slimes by the SlimeManager.
        /// </summary>
        /// <param name="slime">The SlimeAI instance executing the ability.</param>
        void ExecuteDailyAbility(SlimeAI slime);

        /// <summary>
        /// Gets the user-friendly display name for this variant.
        /// </summary>
        string GetDisplayName();

        /// <summary>
        /// Gets the description text for this variant.
        /// </summary>
        string GetDescription();
    }
}