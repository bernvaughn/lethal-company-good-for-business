using GoodForBusiness.Slimes;
using UnityEngine;

namespace GoodForBusiness.Networking
{
    public class SlimeNetworkSync : MonoBehaviour
    {
        public SlimeAI? Slime { get; set; }

        public void SyncPosition(Vector3 position)
        {
            if (Slime != null)
                Slime.transform.position = position;
        }

        public void SyncState(int state)
        {
            if (Slime != null)
                Slime.SetState((SlimeState)state);
        }

        public void SyncGrowth(float progress)
        {
            if (Slime != null)
                Slime.GrowthProgress = progress;
        }
    }
}