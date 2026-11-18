using GoodForBusiness.Core;
using UnityEngine;

namespace GoodForBusiness.Slimes
{
    public class BeeLoverSlimeVariant : SlimeVariantBase
    {
        public override SlimeType Type => SlimeType.BeeLover;
        public override int GrowthCost => 400;
        public override Color SlimeColor => Color.yellow;

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility()
        {
            // Seek and destroy beehives
        }

        public override string GetDisplayName() => "Bee Lover Slime";
        public override string GetDescription() => "A yellow and black slime that hunts beehives.";
    }
}