using GoodForBusiness.Core;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using UnityEngine;

namespace GoodForBusiness.Egg
{
    public class SlimeEggItem : GrabbableObject
    {
        public SlimeType EggType { get; set; } = SlimeType.None;
        public bool IsHatching { get; private set; } = false;

        // LethalLib Item properties
        public bool TwoHanded => false;
        public float Weight => 0.5f;
        public int CreditsWorth => 0;

        protected override void Awake()
        {
            base.Awake();
            // Set scanner name to "Slime Egg"
            if (scanNodeProperties == null)
                scanNodeProperties = ScriptableObject.CreateInstance<ScanNodeProperties>();
            scanNodeProperties.headerText = "Slime Egg";
            scanNodeProperties.subText = "A mysterious egg that hatches into a slime";
        }

        public void StartHatching()
        {
            IsHatching = true;
        }

        public override void OnPickup()
        {
            base.OnPickup();
        }

        public override void OnDrop()
        {
            base.OnDrop();
        }
    }
}