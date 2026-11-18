using GoodForBusiness.Stubs.Game;
using UnityEngine;

namespace GoodForBusiness.Items
{
    public class SlimedBeehiveItem : GrabbableObject
    {
        public int CreditValue { get; set; } = 175;

        protected override void Awake()
        {
            base.Awake();

            // Set a unique scan node so the slimed beehive is identifiable in the scanner
            if (scanNodeProperties == null)
                scanNodeProperties = ScriptableObject.CreateInstance<ScanNodeProperties>();
            scanNodeProperties.headerText = "Slimed Beehive";
            scanNodeProperties.subText = "A beehive coated in slime. The Company pays a high price for it.";

            // Apply the configured credit value to the scrap's sell value
            itemValue = CreditValue;
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }
    }
}