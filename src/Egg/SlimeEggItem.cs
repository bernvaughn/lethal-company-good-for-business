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

        public void Hatch(SlimeAI slimePrefab)
        {
            if (slimePrefab == null)
                return;
            var slime = UnityEngine.Object.Instantiate(slimePrefab, transform.position, Quaternion.identity) as SlimeAI;
            if (slime != null)
                slime.SlimeType = EggType;
            Destroy(gameObject);
        }
    }
}