using System;

namespace GoodForBusiness.Core
{
    [Serializable]
    public class SlimeSaveData
    {
        public SlimeType Type;
        public UnityEngine.Vector3 Position;
        public float GrowthProgress;
        public bool IsGrown;
        public int OwnerPlayerId;
        public ulong NetworkObjectId;

        public SlimeSaveData() { }

        public SlimeSaveData(SlimeType type, UnityEngine.Vector3 position, float growthProgress, bool isGrown, int ownerPlayerId, ulong networkObjectId)
        {
            Type = type;
            Position = position;
            GrowthProgress = growthProgress;
            IsGrown = isGrown;
            OwnerPlayerId = ownerPlayerId;
            NetworkObjectId = networkObjectId;
        }
    }
}