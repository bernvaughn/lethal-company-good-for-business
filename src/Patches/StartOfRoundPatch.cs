using UnityEngine;

namespace GoodForBusiness.Patches
{
    public class StartOfRoundPatch
    {
        public static void OnNewDayPatch()
        {
            // Called when a new day starts - trigger slime hatching and daily abilities
        }
    }
}