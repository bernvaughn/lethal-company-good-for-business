using HarmonyLib;
using GoodForBusiness.Stubs.Game;
using GoodForBusiness.Core;
using GoodForBusiness.Egg;
using GoodForBusiness.Slimes;
using GoodForBusiness.Networking;
using UnityEngine;

namespace GoodForBusiness.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    public class PlayerControllerBPatch
    {
        private static float _lastFeedAttempt = 0f;
        private const float FeedCooldown = 0.5f;

        [HarmonyPostfix]
        public static void OnUpdatePostfix(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner)
                return;

            // Check for feed input (e.g., E key while looking at slime)
            if (Input.GetKeyDown(KeyCode.E) && Time.time - _lastFeedAttempt > FeedCooldown)
            {
                _lastFeedAttempt = Time.time;
                TryFeedSlimeInRange(__instance);
            }
        }

        private static void TryFeedSlimeInRange(PlayerControllerB player)
        {
            if (player.currentItemSlot < 0 || player.currentItemSlot >= player.ItemSlots.Length)
                return;

            var heldItem = player.ItemSlots[player.currentItemSlot];
            if (heldItem == null)
                return;

            // Check if the held item is scrap (has positive value)
            int scrapValue = heldItem.itemValue;
            if (scrapValue <= 0)
                return;

            // Raycast to find slime in front of player
            Vector3 rayOrigin = player.transform.position + Vector3.up * 1.5f;
            Vector3 rayDirection = player.transform.forward;
            
            if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 3f))
            {
                var slimeAI = hit.collider.GetComponentInParent<SlimeAI>();
                if (slimeAI != null)
                {
                    // Found a slime! Feed it the scrap.
                    slimeAI.OnFedScrap(scrapValue, player.playerClientId);

                    // Remove the scrap from player's inventory
                    player.ItemSlots[player.currentItemSlot] = null;
                    
                    UnityEngine.Debug.Log($"[GoodForBusiness] Player {player.playerClientId} fed {scrapValue} scrap to slime");
                }
            }
        }
    }
}