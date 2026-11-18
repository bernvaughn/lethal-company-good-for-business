using GoodForBusiness.Core;
using UnityEngine;

namespace GoodForBusiness.Slimes
{
    public class EquipmentSlimeVariant : SlimeVariantBase
    {
        public override SlimeType Type => SlimeType.Equipment;
        public override int GrowthCost => 300;
        public override Color SlimeColor => Color.blue;

        public override void OnFedScrap(int scrapValue)
        {
            // Growth handled by base class
        }

        public override void ExecuteDailyAbility()
        {
            // Spawn random equipment at ship
        }

        public override string GetDisplayName() => "Equipment Slime";
        public override string GetDescription() => "A blue slime that produces random equipment daily.";
    }
}