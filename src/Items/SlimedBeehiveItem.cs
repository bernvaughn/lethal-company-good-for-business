using GoodForBusiness.Stubs.Game;
using UnityEngine;
using Unity.Netcode;

namespace GoodForBusiness.Items
{
    public class SlimedBeehiveItem : GrabbableObject
    {
        private NetworkVariable<int> _creditValue = new NetworkVariable<int>(175);

        public int CreditValue
        {
            get => _creditValue.Value;
            set
            {
                if (IsServer)
                {
                    _creditValue.Value = value;
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            // Set a unique scan node so the slimed beehive is identifiable in the scanner
            if (scanNodeProperties == null)
                scanNodeProperties = ScriptableObject.CreateInstance<ScanNodeProperties>();
            scanNodeProperties.headerText = "Slimed Beehive";
            scanNodeProperties.subText = "A beehive coated in slime. The Company pays a high price for it.";
        }

        protected override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Apply the credit value to the scrap's sell value on all clients
            itemValue = CreditValue;
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }
    }
}