using System;

namespace UnityEngine
{
    public abstract class Object
    {
        public static implicit operator bool(Object? obj) => obj != null;
        public static bool operator ==(Object? x, Object? y) => ReferenceEquals(x, y);
        public static bool operator !=(Object? x, Object? y) => !ReferenceEquals(x, y);
        public override bool Equals(object? obj) => ReferenceEquals(this, obj);
        public override int GetHashCode() => base.GetHashCode();
        public string? name { get; set; }
        public T GetComponent<T>() where T : Component => default!;
        public T AddComponent<T>() where T : Component => default!;
        public static void Destroy(Object? obj) { }
        public static void Destroy(Object? obj, float t) { }
        public static T? FindObjectOfType<T>() where T : Object => null;
        public static T[] FindObjectsOfType<T>() where T : Object => Array.Empty<T>();
        public static void DontDestroyOnLoad(Object? obj) { }
        public static T Instantiate<T>(T original) where T : Object => default!;
        public static T Instantiate<T>(T original, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation) where T : Object => default!;
    }

    public static class Debug
    {
        public static void Log(object? message) { }
        public static void LogWarning(object? message) { }
        public static void LogError(object? message) { }
        public static void LogFormat(string format, params object?[] args) { }
    }

    public static class Mathf
    {
        public static float Clamp01(float value) => value < 0 ? 0 : (value > 1 ? 1 : value);
        public static float Max(float a, float b) => a > b ? a : b;
        public static float Min(float a, float b) => a < b ? a : b;
        public static float Sin(float value) => 0f;
    }

    public static class Random
    {
        public static float value => 0.5f;
        public static float Range(float min, float max) => min;
        public static int Range(int min, int max) => min;
        public static Vector3 insideUnitSphere => Vector3.zero;
        public static Color color => Color.white;
        public static float rotation => 0f;
    }

    public class WaitForSeconds
    {
        public WaitForSeconds(float seconds) { }
    }

    public class Component : Object
    {
        public Transform transform => default!;
        public GameObject gameObject => default!;
        public T GetComponent<T>() where T : Component => default!;
        public T GetComponentInChildren<T>() where T : Component => default!;
        public T GetComponentInChildren<T>(bool includeInactive) where T : Component => default!;
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component => Array.Empty<T>();
        public T[] GetComponents<T>() where T : Component => Array.Empty<T>();
        public T GetComponentInParent<T>() where T : Component => default!;
    }

    public class Renderer : Component
    {
        public Material material => default!;
        public Material sharedMaterial { get => default!; set { } }
        public void GetPropertyBlock(MaterialPropertyBlock block) { }
        public void SetPropertyBlock(MaterialPropertyBlock block) { }
    }

    public class MaterialPropertyBlock
    {
        public void SetColor(int nameID, Color color) { }
        public void SetFloat(int nameID, float value) { }
    }

    public static class Shader
    {
        public static int PropertyToID(string name) => 0;
        public static Material? Find(string name) => null;
    }

    public class GameObject : Object
    {
        public Transform transform => default!;
        public string? name { get; set; }
        public GameObject() { }
        public GameObject(string? name) { this.name = name; }
        public static GameObject? Find(string? name) => null;
        public static GameObject CreatePrimitive(PrimitiveType type) => new GameObject();
        public new T GetComponent<T>() where T : Component => default!;
        public new T AddComponent<T>() where T : Component => default!;
        public new T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component => Array.Empty<T>();
        public void SetActive(bool active) { }
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 localScale { get; set; }
        public Transform? parent { get; set; }
        public Vector3 forward => Vector3.forward;
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static float Distance(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z).magnitude;
        public static float SqrMagnitude(Vector3 a) => (a.x - 0) * (a.x - 0) + (a.y - 0) * (a.y - 0) + (a.z - 0) * (a.z - 0);
        public float SqrMagnitude() => x * x + y * y + z * z;
        public float Magnitude => (float)System.Math.Sqrt(x * x + y * y + z * z);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => a.x != b.x || a.y != b.y || a.z != b.z;
        public float magnitude => (float)System.Math.Sqrt(x * x + y * y + z * z);
        public Vector3 normalized => magnitude > 0 ? this / magnitude : zero;
        public override string ToString() => $"({x}, {y}, {z})";
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => new Quaternion(0, 0, 0, 1);
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion rotation, Vector3 point) => point;
        public static Quaternion LookRotation(Vector3 forward) => identity;
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => a;
        public override string ToString() => $"({x}, {y}, {z}, {w})";
    }

    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color red => new Color(1, 0, 0);
        public static Color blue => new Color(0, 0, 1);
        public static Color green => new Color(0, 1, 0);
        public static Color yellow => new Color(1, 1, 0);
        public static Color black => new Color(0, 0, 0);
        public static Color white => new Color(1, 1, 1);
        public static Color clear => new Color(0, 0, 0, 0);
        public static Color operator *(Color a, Color b) => new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        public static Color operator *(Color a, float b) => new Color(a.r * b, a.g * b, a.b * b, a.a * b);
    }

    public enum PrimitiveType
    {
        Sphere,
        Cube,
        Capsule,
        Cylinder,
        Plane,
        Quad
    }

    public class Material : Object
    {
        public Material() { }
        public Material(Material? source) { }
        public Color color { get; set; } = Color.white;
        public void SetColor(int nameID, Color color) { }
        public void SetFloat(int nameID, float value) { }
    }

    public class Sprite : Object { }

    public class Texture2D : Object
    {
        public Texture2D(int width, int height) { }
    }

    public class AssetBundle : Object
    {
        public static AssetBundle? LoadFromFile(string path) => null;
        public static AssetBundle? LoadFromMemory(byte[] binary) => null;
        public T? LoadAsset<T>(string name) where T : Object => default!;
        public T[] LoadAllAssets<T>() where T : Object => Array.Empty<T>();
        public void Unload(bool unloadAllLoadedObjects) { }
    }

    public class AudioClip : Object { }

    public class AudioSource : Component
    {
        public AudioClip? clip;
        public void Play() { }
        public void PlayOneShot(AudioClip? clip) { }
    }

    public class NetworkTransform : Component
    {
        public bool Interpolate { get; set; } = true;
        public float TeleportThreshold { get; set; } = 5f;
        public bool SynchronizeRotation { get; set; } = true;
        public ScaleSynchronizationMode ScaleSynchronization { get; set; }
        public void Teleport(Vector3 position, Quaternion rotation) { }
    }

    public enum ScaleSynchronizationMode
    {
        None,
        LocalScale,
        UniformScale
    }

    public class Animator : Component
    {
        public void Play(string? stateName) { }
        public void SetFloat(string? name, float value) { }
        public void SetBool(string? name, bool value) { }
        public void SetInteger(string? name, int value) { }
        public void SetTrigger(string? name) { }
        public void ResetTrigger(string? name) { }
        public bool GetBool(string? name) => false;
    }

    public class Rigidbody : Component
    {
        public Vector3 velocity { get; set; }
        public bool useGravity { get; set; } = true;
        public bool isKinematic { get; set; }
    }

    public class Collider : Component { }

    public class MeshRenderer : Component
    {
        public Material[] materials { get; set; } = Array.Empty<Material>();
        public Material material => default!;
        public bool enabled { get; set; } = true;
    }

    public class SkinnedMeshRenderer : MeshRenderer { }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => default!;
    }

    public abstract class MonoBehaviour : Component
    {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        public virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public virtual Coroutine StartCoroutine(System.Collections.IEnumerator routine) => null!;
        public virtual void StopCoroutine(Coroutine? routine) { }
    }

    public class Coroutine { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class HeaderAttribute : Attribute
    {
        public string header;
        public HeaderAttribute(string header) { this.header = header; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SerializeFieldAttribute : Attribute { }

    public static class Physics
    {
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float maxDistance)
        {
            hit = default;
            return false;
        }
    }

    public struct RaycastHit
    {
        public Collider collider;
        public Vector3 point;
        public Vector3 normal;
    }

    public static class Input
    {
        public static bool GetKeyDown(KeyCode key) => false;
    }

    public enum KeyCode
    {
        E,
        None
    }

    public static class Time
    {
        public static float time => 0f;
        public static float deltaTime => 0f;
    }

    namespace AI
    {
        public class NavMeshAgent : Component
        {
            public float speed { get; set; } = 2f;
            public float acceleration { get; set; } = 8f;
            public float stoppingDistance { get; set; } = 0f;
            public bool autoBraking { get; set; } = true;
            public bool hasPath => false;
            public bool pathPending => false;
            public Vector3 destination { get; set; } = Vector3.zero;
            public Vector3 velocity => Vector3.zero;
            public void ResetPath() { }
            public void SetDestination(Vector3 position) { }
            public bool SamplePosition(Vector3 position, out NavMeshHit hit, float maxDistance, int areaMask) { hit = default; return false; }
        }

        public struct NavMeshHit
        {
            public Vector3 position;
        }

        public static class NavMesh
        {
            public static int AllAreas => 0;
            public static bool SamplePosition(Vector3 position, out NavMeshHit hit, float maxDistance, int areaMask) { hit = default; return false; }
        }
    }
}