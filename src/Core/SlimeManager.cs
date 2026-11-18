using System;
using System.Collections.Generic;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using UnityEngine;

namespace GoodForBusiness.Core
{
    public class SlimeManager : MonoBehaviour
    {
        public static SlimeManager Instance { get; private set; } = null!;

        private readonly List<SlimeAI> _activeSlimes = new List<SlimeAI>();
        private SlimeSaveData[]? _pendingSaveData;

        public IReadOnlyList<SlimeAI> ActiveSlimes => _activeSlimes;

        private new void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private new void OnDestroy()
        {
            if (Instance == this)
                Instance = null!;
        }

        public void RegisterSlime(SlimeAI slime)
        {
            if (!_activeSlimes.Contains(slime))
                _activeSlimes.Add(slime);
        }

        public void UnregisterSlime(SlimeAI slime)
        {
            _activeSlimes.Remove(slime);
        }

        public void OnNewDay()
        {
            // Trigger daily abilities for all active slimes
            foreach (var slime in _activeSlimes)
            {
                slime.ExecuteDailyAbility();
            }
        }

        public bool TryFeedSlime(SlimeAI slime, int scrapValue)
        {
            if (scrapValue <= 0)
                return false;
            slime.OnFedScrap(scrapValue);
            return true;
        }

        public SlimeSaveData[] GetSaveData()
        {
            var data = new SlimeSaveData[_activeSlimes.Count];
            for (int i = 0; i < _activeSlimes.Count; i++)
            {
                var slime = _activeSlimes[i];
                data[i] = new SlimeSaveData(
                    slime.SlimeType,
                    slime.transform.position,
                    slime.GrowthProgress,
                    slime.IsGrown,
                    slime.OwnerPlayerId,
                    slime.NetworkObjectId
                );
            }
            return data;
        }

        public void LoadSaveData(SlimeSaveData[] data)
        {
            if (data == null || data.Length == 0)
                return;

            // Store pending data to restore after level loads
            _pendingSaveData = data;
        }

        /// <summary>
        /// Call this after level is loaded to restore slimes from save data.
        /// </summary>
        public void RestoreSlimesFromSave()
        {
            if (_pendingSaveData == null || _pendingSaveData.Length == 0)
                return;

            var slimeEnemyType = Enemies.GetEnemyType("Slime");
            if (slimeEnemyType == null || slimeEnemyType.enemyPrefab == null)
            {
                UnityEngine.Debug.LogWarning("[GoodForBusiness] Slime enemy type not registered, cannot restore slimes.");
                _pendingSaveData = null;
                return;
            }

            foreach (var saveData in _pendingSaveData)
            {
                // Only restore if the slime type is enabled in config
                var config = GoodForBusinessPlugin.Instance?.Config;
                if (config != null)
                {
                    bool enabled = saveData.Type switch
                    {
                        SlimeType.Equipment => config.EnableEquipmentSlime.Value,
                        SlimeType.BeeLover => config.EnableBeeLoverSlime.Value,
                        _ => false
                    };
                    if (!enabled) continue;
                }

                var slimeObj = UnityEngine.Object.Instantiate(slimeEnemyType.enemyPrefab, saveData.Position, Quaternion.identity);
                var slimeAI = slimeObj.GetComponent<SlimeAI>();
                if (slimeAI == null)
                {
                    slimeAI = slimeObj.AddComponent<SlimeAI>();
                }

                slimeAI.SlimeType = saveData.Type;
                slimeAI.GrowthProgress = saveData.GrowthProgress;
                slimeAI.IsGrown = saveData.IsGrown;
                slimeAI.OwnerPlayerId = saveData.OwnerPlayerId;
                slimeAI.NetworkObjectId = saveData.NetworkObjectId;
                slimeAI.InitializeVariant();

                RegisterSlime(slimeAI);

                // Network spawn if host
                var networkObj = slimeObj.GetComponent<NetworkObject>();
                if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
                {
                    networkObj.Spawn();
                }
            }

            _pendingSaveData = null;
        }
    }
}