using System;
using System.Collections.Generic;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using UnityEngine;

namespace GoodForBusiness.Core
{
    public class SlimeManager : MonoBehaviour
    {
        public static SlimeManager Instance { get; private set; } = null!;

        private readonly List<SlimeAI> _activeSlimes = new List<SlimeAI>();

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
            // Restoration handled by Spawner on level load
        }
    }
}