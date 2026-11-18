using GoodForBusiness.Core;
using UnityEngine;

namespace GoodForBusiness.Slimes
{
    public enum SlimeState
    {
        Idle,
        Following,
        Eating,
        Growing,
        AbilityActive
    }

    public abstract class SlimeVariantBase : MonoBehaviour
    {
        public abstract SlimeType Type { get; }
        public abstract int GrowthCost { get; }
        public abstract Color SlimeColor { get; }
        public abstract void OnFedScrap(int scrapValue);
        public abstract void ExecuteDailyAbility();
        public abstract string GetDisplayName();
        public abstract string GetDescription();
    }

    public class SlimeAI : MonoBehaviour
    {
        public SlimeType SlimeType { get; set; } = SlimeType.None;
        public SlimeState CurrentState { get; private set; } = SlimeState.Idle;
        public float GrowthProgress { get; set; } = 0f;
        public bool IsGrown { get; protected set; } = false;
        public int OwnerPlayerId { get; set; } = -1;
        public ulong NetworkObjectId { get; set; } = 0;

        public virtual void SetState(SlimeState newState)
        {
            CurrentState = newState;
        }

        public virtual void OnFedScrap(int scrapValue)
        {
            GrowthProgress += scrapValue * 0.001f;
            if (GrowthProgress >= 1f)
            {
                GrowthProgress = 1f;
                IsGrown = true;
                SetState(SlimeState.Growing);
            }
        }

        public virtual void ExecuteDailyAbility()
        {
            // Default: no ability
        }

        private new void Update()
        {
            if (SlimesFacePlayers)
            {
                FaceNearestPlayer();
            }
        }

        private bool SlimesFacePlayers => true;

        private void FaceNearestPlayer()
        {
            // Stub: rotation logic handled by variant
        }
    }
}