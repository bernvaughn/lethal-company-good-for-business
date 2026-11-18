using System;

namespace Unity.Netcode
{
    public abstract class NetworkBehaviour : UnityEngine.MonoBehaviour
    {
        public NetworkObject NetworkObject => default!;
        public bool IsServer => true;
        public bool IsClient => true;
        public bool IsHost => true;
        public ulong NetworkObjectId => 0;
        public ulong OwnerClientId => 0;
        public bool IsOwner => true;
        public bool IsSpawned => true;
        public void Despawn(bool destroy = true) { }
        protected virtual void OnNetworkSpawn() { }
        protected virtual void OnNetworkDespawn() { }
    }

    public class NetworkObject : UnityEngine.Component
    {
        public ulong NetworkObjectId => 0;
        public ulong OwnerClientId => 0;
        public bool IsSpawned => true;
        public bool IsSceneObject => false;
        public NetworkBehaviour[] NetworkBehaviours => Array.Empty<NetworkBehaviour>();
    }

    public class NetworkVariable<T> where T : struct
    {
        public NetworkVariable() { }
        public NetworkVariable(T value) { Value = value; }
        public T Value { get; set; }
        public event Action<T, T> OnValueChanged;
        public void Set(T value) { Value = value; }
        public bool IsDirty => false;
        public static implicit operator T(NetworkVariable<T> variable) => variable.Value;
    }

    public class NetworkVariable<T> where T : class
    {
        public NetworkVariable() { }
        public NetworkVariable(T value) { Value = value; }
        public T Value { get; set; }
        public event Action<T, T> OnValueChanged;
        public void Set(T value) { Value = value; }
        public bool IsDirty => false;
    }

    public class NetworkList<T> where T : struct
    {
        public int Count => 0;
        public T this[int index] { get => default; set { } }
        public event Action<NetworkListEvent<T>> OnListChanged;
        public void Add(T value) { }
        public void RemoveAt(int index) { }
        public void Clear() { }
    }

    public struct NetworkListEvent<T>
    {
        public NetworkListEventType Type;
        public int Index;
        public T Value;
        public T PreviousValue;
    }

    public enum NetworkListEventType
    {
        Add,
        Remove,
        RemoveAt,
        Clear,
        Value,
        Insert
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ServerRpcAttribute : Attribute
    {
        public bool RequireOwnership { get; set; } = true;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ClientRpcAttribute : Attribute
    {
        public bool RequireOwnership { get; set; } = false;
    }

    public class RpcTarget
    {
        public static RpcTarget All => new RpcTarget();
        public static RpcTarget Server => new RpcTarget();
        public static RpcTarget Single(ulong clientId, RpcTargetUse use = RpcTargetUse.Temp) => new RpcTarget();
        public static RpcTarget Single(ulong clientId) => new RpcTarget();
    }

    public enum RpcTargetUse
    {
        Temp,
        Persistent
    }

    public class ClientRpcParams
    {
        public RpcTarget Send { get; set; }
    }

    public class ServerRpcParams
    {
        public ServerRpcReceiveParams Receive { get; set; }
    }

    public class ServerRpcReceiveParams
    {
        public ulong SenderClientId => 0;
    }

    public static class NetworkManager
    {
        public static NetworkManager Singleton => new NetworkManager();
        public bool IsServer => true;
        public bool IsClient => true;
        public bool IsHost => true;
        public bool IsListening => true;
        public ulong LocalClientId => 0;
        public NetworkConfig NetworkConfig => new NetworkConfig();
        public void Shutdown() { }
        public void StartHost() { }
        public void StartClient() { }
        public void StartServer() { }
    }

    public class NetworkConfig
    {
        public string NetworkTransport = "UnityTransport";
    }

    public class NetworkTransform : NetworkBehaviour
    {
        public bool Interpolate { get; set; } = true;
        public float InterpolationTime { get; set; } = 0.1f;
        public bool ServerAuthoritative { get; set; } = true;
    }
}

namespace Unity.Netcode.Transports.UTP
{
    public class UnityTransport : UnityEngine.Component
    {
        public ushort ServerListenPort { get; set; } = 7777;
        public string ConnectionAddress { get; set; } = "127.0.0.1";
    }
}