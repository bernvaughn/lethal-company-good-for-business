using UnityEngine;
using System;

namespace GoodForBusiness.Stubs.Game
{
    /// <summary>
    /// Stubs for Lethal Company game types that will be replaced
    /// with references to the actual game assemblies when building
    /// against a real game installation.
    /// </summary>

    public class ScanNodeProperties : UnityEngine.ScriptableObject
    {
        public string headerText = "";
        public string subText = "";
    }

    public class GrabbableObject : UnityEngine.MonoBehaviour
    {
        public int itemValue;
        public bool isHeld;
        public int playerHeldBy;
        public ScanNodeProperties scanNodeProperties;

        public virtual void OnPickup() { }
        public virtual void OnDrop() { }
    }

    public class EnemyAI : UnityEngine.MonoBehaviour
    {
        public bool isEnemy;
        public Vector3 transformPosition => transform.position;
    }

    public class NetworkObject : UnityEngine.MonoBehaviour
    {
        public ulong NetworkObjectId { get; set; }
        public bool IsSpawned { get; set; }
        public void Spawn() { }
        public void Despawn() { }
    }

    public class NetworkVariable<T>
    {
        public T Value { get; set; } = default!;
        public event Action<T>? OnValueChanged;
        public NetworkVariable() { }
        public NetworkVariable(T value) { Value = value; }
    }

    public class NetworkManager
    {
        public static NetworkManager? Singleton { get; private set; } = new NetworkManager();
        public bool IsHost = true;
        public static NetworkObject? SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var obj = UnityEngine.Object.Instantiate(prefab);
            return obj.GetComponent<NetworkObject>();
        }
    }

    public class RoundManager : UnityEngine.MonoBehaviour
    {
        public static RoundManager Instance { get; private set; } = null!;
        public event Action? OnSpawnScrap;
        public void SpawnScrapInLevel() { OnSpawnScrap?.Invoke(); }
    }

    public class ShipBuildModeManager : UnityEngine.MonoBehaviour
    {
        public static ShipBuildModeManager Instance { get; private set; } = null!;
        public GrabbableObject?[] shipStorage = new GrabbableObject?[16];
    }

    public class StartOfRound : UnityEngine.MonoBehaviour
    {
        public static StartOfRound Instance { get; private set; } = null!;
        public event Action? OnNewDay;
        public PlayerControllerB[]? allPlayerScripts;
        public ShipBuildModeManager? shipBuildModeManager;
        public void QuotaReset() { OnNewDay?.Invoke(); }
    }

    public class PlayerControllerB : UnityEngine.MonoBehaviour
    {
        public static PlayerControllerB? localPlayerController;
        public static PlayerControllerB[]? allPlayerScripts;
        public int playerClientId;
        public GrabbableObject?[] ItemSlots = new GrabbableObject?[4];
        public int currentItemSlot = 0;
        public Vector3 transformPosition => transform.position;
    }

    public enum LevelTypes
    {
        Experiment,
        Assurance,
        Vow,
        March,
        Offense,
        Rend,
        Dine,
        Modded
    }

    public class Item : ScriptableObject
    {
        public GameObject spawnPrefab;
        public bool twoHanded;
        public float weight;
        public int creditsWorth;
    }

    public class EnemyType : ScriptableObject
    {
        public GameObject enemyPrefab;
        public int PowerLevel;
        public int spawnWeight;
    }
}