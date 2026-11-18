using GoodForBusiness.Stubs.Game;
using UnityEngine;

namespace GoodForBusiness.Items
{
    public class SlimedBeehiveItem : GrabbableObject
    {
        public int CreditValue { get; set; } = 175;

        public override void OnPickup()
        {
            base.OnPickup();
        }
    }
}