using GoodForBusiness.Core;
using GoodForBusiness.Configuration;
using GoodForBusiness.Stubs.Game;
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
        public bool IsGrown { get; set; } = false;
        public int OwnerPlayerId { get; set; } = -1;
        public ulong NetworkObjectId { get; set; } = 0;

        private SlimeVariantBase? _variant;
        private PluginConfig? _config;

        public void InitializeVariant()
        {
            _config = GoodForBusinessPlugin.Instance?.Config;

            switch (SlimeType)
            {
                case SlimeType.Equipment:
                    _variant = gameObject.AddComponent<EquipmentSlimeVariant>();
                    break;
                case SlimeType.BeeLover:
                    _variant = gameObject.AddComponent<BeeLoverSlimeVariant>();
                    break;
                default:
                    UnityEngine.Debug.LogWarning($"[GoodForBusiness] Unknown slime type: {SlimeType}");
                    break;
            }

            if (_variant != null)
            {
                // Apply variant color to renderer
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = _variant.SlimeColor;
                }
            }
        }

        public virtual void SetState(SlimeState newState)
        {
            CurrentState = newState;
        }

        public virtual void OnFedScrap(int scrapValue)
        {
            _variant?.OnFedScrap(scrapValue);
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
            if (_variant != null && IsGrown)
            {
                _variant.ExecuteDailyAbility();
            }
        }

        public int GetGrowthCost()
        {
            return _variant?.GrowthCost ?? 300;
        }

        private new void Update()
        {
            var slimesFacePlayers = _config?.SlimesFacePlayers.Value ?? true;
            if (slimesFacePlayers)
            {
                FaceNearestPlayer();
            }
        }

        private void FaceNearestPlayer()
        {
            if (StartOfRound.Instance?.allPlayerScripts == null)
                return;

            PlayerControllerB? nearestPlayer = null;
            float nearestDist = float.MaxValue;

            foreach (var player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null)
                    continue;

                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestPlayer = player;
                }
            }

            if (nearestPlayer != null)
            {
                Vector3 direction = (nearestPlayer.transform.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }
}