using System;
using System.Collections.Generic;
using System.IO;
using GoodForBusiness.Slimes;
using GoodForBusiness.Stubs.Game;
using LethalLib.Modules;
using UnityEngine;
using Unity.Netcode;

namespace GoodForBusiness.Core
{
    public class SlimeManager : MonoBehaviour
    {
        public static SlimeManager Instance { get; private set; } = null!;

        private readonly List<SlimeAI> _activeSlimes = new List<SlimeAI>();
        private SlimeSaveData[]? _pendingSaveData;
        private string _saveFilePath = "";

        public IReadOnlyList<SlimeAI> ActiveSlimes => _activeSlimes;

        private new void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _saveFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Lethal Company",
                "BepInEx",
                "config",
                "GoodForBusiness_slimes.json"
            );

            // Load persisted slime data on startup
            LoadFromDisk();
        }

        private new void OnDestroy()
        {
            if (Instance == this)
                Instance = null!;
        }

        public void RegisterSlime(SlimeAI slime)
        {
            if (!_activeSlimes.Contains(slime))
            {
                _activeSlimes.Add(slime);
                // Only save on host to avoid conflicts
                if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsHost)
                    SaveToDisk();
            }
        }

        public void UnregisterSlime(SlimeAI slime)
        {
            if (_activeSlimes.Remove(slime))
            {
                if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsHost)
                    SaveToDisk();
            }
        }

        public void OnNewDay()
        {
            // Only host should trigger daily abilities
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsHost)
                return;

            // Trigger daily abilities for all active slimes
            foreach (var slime in _activeSlimes)
            {
                slime.ExecuteDailyAbility();
            }
            SaveToDisk();
        }

        public bool TryFeedSlime(SlimeAI slime, int scrapValue)
        {
            if (scrapValue <= 0)
                return false;
            slime.OnFedScrap(scrapValue);
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsHost)
                SaveToDisk();
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
                    slime.OwnerPlayerId
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
                slimeAI.InitializeVariant();

                RegisterSlime(slimeAI);

                // Network spawn if host (clients receive via NetworkObject.Spawn())
                var networkObj = slimeObj.GetComponent<NetworkObject>();
                if (networkObj != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
                {
                    networkObj.Spawn();
                }
            }

            _pendingSaveData = null;
        }

        /// <summary>
        /// Saves all active slimes to disk as JSON.
        /// Only runs on host to avoid conflicts.
        /// </summary>
        public void SaveToDisk()
        {
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsHost)
                return;

            try
            {
                var dir = Path.GetDirectoryName(_saveFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var data = GetSaveData();
                var json = SlimeSaveDataArrayToJson(data);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[GoodForBusiness] Failed to save slime data: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads slime data from disk if a save file exists.
        /// </summary>
        public void LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_saveFilePath))
                    return;

                var json = File.ReadAllText(_saveFilePath);
                var data = SlimeSaveDataArrayFromJson(json);
                if (data != null && data.Length > 0)
                {
                    LoadSaveData(data);
                    // Restore immediately - slimes will be recreated on next level load
                    // via RestoreSlimesFromSave() if _pendingSaveData is set
                    UnityEngine.Debug.Log($"[GoodForBusiness] Loaded {data.Length} slime(s) from disk.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[GoodForBusiness] Failed to load slime data: {ex.Message}");
            }
        }

        /// <summary>
        /// Serializes a SlimeSaveData array to JSON string.
        /// </summary>
        private static string SlimeSaveDataArrayToJson(SlimeSaveData[] data)
        {
            var writer = new System.Text.StringBuilder();
            writer.Append('[');
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0) writer.Append(',');
                writer.Append(SlimeSaveDataToJson(data[i]));
            }
            writer.Append(']');
            return writer.ToString();
        }

        /// <summary>
        /// Serializes a single SlimeSaveData to JSON string.
        /// </summary>
        private static string SlimeSaveDataToJson(SlimeSaveData data)
        {
            var writer = new System.Text.StringBuilder();
            writer.Append('{');
            writer.Append($"\"type\":{(int)data.Type}");
            writer.Append($",\"position\":{{\"x\":{data.Position.x},\"y\":{data.Position.y},\"z\":{data.Position.z}}}");
            writer.Append($",\"growthProgress\":{data.GrowthProgress}");
            writer.Append($",\"isGrown\":{data.IsGrown.ToString().ToLower()}");
            writer.Append($",\"ownerPlayerId\":{data.OwnerPlayerId}");
            writer.Append('}');
            return writer.ToString();
        }

        /// <summary>
        /// Deserializes a JSON string to a SlimeSaveData array.
        /// </summary>
        private static SlimeSaveData[]? SlimeSaveDataArrayFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            json = json.Trim();
            if (!json.StartsWith("[") || !json.EndsWith("]"))
                return null;

            var results = new List<SlimeSaveData>();
            var inner = json.Substring(1, json.Length - 2).Trim();
            if (string.IsNullOrEmpty(inner))
                return results.ToArray();

            // Split by top-level objects
            var objects = SplitJsonObjects(inner);
            foreach (var obj in objects)
            {
                var data = SlimeSaveDataFromJson(obj);
                if (data != null)
                    results.Add(data);
            }

            return results.ToArray();
        }

        /// <summary>
        /// Deserializes a JSON object string to a SlimeSaveData.
        /// </summary>
        private static SlimeSaveData? SlimeSaveDataFromJson(string json)
        {
            try
            {
                if (!json.StartsWith("{") || !json.EndsWith("}"))
                    return null;

                var data = new SlimeSaveData();
                var inner = json.Substring(1, json.Length - 2);

                // Parse key-value pairs
                var pairs = SplitKeyValuePairs(inner);
                foreach (var pair in pairs)
                {
                    var colonIdx = pair.IndexOf(':');
                    if (colonIdx < 0) continue;

                    var key = pair.Substring(0, colonIdx).Trim('"', ' ');
                    var value = pair.Substring(colonIdx + 1).Trim();

                    switch (key)
                    {
                        case "type":
                            data.Type = (SlimeType)int.Parse(value);
                            break;
                        case "growthProgress":
                            data.GrowthProgress = float.Parse(value);
                            break;
                        case "isGrown":
                            data.IsGrown = bool.Parse(value);
                            break;
                        case "ownerPlayerId":
                            data.OwnerPlayerId = int.Parse(value);
                            break;
                        case "position":
                            data.Position = ParseVector3(value);
                            break;
                    }
                }

                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Splits a JSON array inner content into individual object strings.
        /// </summary>
        private static List<string> SplitJsonObjects(string inner)
        {
            var results = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < inner.Length; i++)
            {
                char c = inner[i];
                if (c == '{') depth++;
                else if (c == '}') depth--;
                else if (c == ',' && depth == 0)
                {
                    results.Add(inner.Substring(start, i - start).Trim());
                    start = i + 1;
                }
            }
            if (start < inner.Length)
                results.Add(inner.Substring(start).Trim());
            return results;
        }

        /// <summary>
        /// Splits a JSON object inner content into key:value pairs.
        /// </summary>
        private static List<string> SplitKeyValuePairs(string inner)
        {
            var results = new List<string>();
            int depth = 0;
            bool inString = false;
            int start = 0;
            for (int i = 0; i < inner.Length; i++)
            {
                char c = inner[i];
                if (c == '"' && (i == 0 || inner[i - 1] != '\\'))
                    inString = !inString;
                else if (!inString)
                {
                    if (c == '{' || c == '[') depth++;
                    else if (c == '}' || c == ']') depth--;
                    else if (c == ',' && depth == 0)
                    {
                        results.Add(inner.Substring(start, i - start).Trim());
                        start = i + 1;
                    }
                }
            }
            if (start < inner.Length)
                results.Add(inner.Substring(start).Trim());
            return results;
        }

        /// <summary>
        /// Parses a JSON position object like {"x":1,"y":2,"z":3} into a Vector3.
        /// </summary>
        private static UnityEngine.Vector3 ParseVector3(string json)
        {
            var result = UnityEngine.Vector3.zero;
            var inner = json.Trim();
            if (!inner.StartsWith("{") || !inner.EndsWith("}"))
                return result;

            inner = inner.Substring(1, inner.Length - 2);
            var pairs = SplitKeyValuePairs(inner);
            foreach (var pair in pairs)
            {
                var colonIdx = pair.IndexOf(':');
                if (colonIdx < 0) continue;

                var key = pair.Substring(0, colonIdx).Trim('"', ' ');
                var value = pair.Substring(colonIdx + 1).Trim();

                switch (key)
                {
                    case "x": result.x = float.Parse(value); break;
                    case "y": result.y = float.Parse(value); break;
                    case "z": result.z = float.Parse(value); break;
                }
            }
            return result;
        }
    }
}