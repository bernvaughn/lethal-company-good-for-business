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
        public static Object? FindObjectOfType<T>() where T : Object => null;
        public static T[] FindObjectsOfType<T>() where T : Object => Array.Empty<T>();
        public static T[] FindObjectsByType<T>(FindObjectsInactive findObjectsInactive, FindObjectsSortMode sortMode) where T : Object => Array.Empty<T>();
        public static void DontDestroyOnLoad(Object? obj) { }
        public static T Instantiate<T>(T original) where T : Object => default!;
        public static T Instantiate<T>(T original, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation) where T : Object => default!;
    }

    public enum FindObjectsInactive
    {
        Exclude,
        Include
    }

    public enum FindObjectsSortMode
    {
        None,
        InstanceID
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
    }

    public static class Random
    {
        public static float value => 0.5f;
        public static float Range(float min, float max) => min;
        public static int Range(int min, int max) => min;
        public static Vector3 insideUnitSphere => new Vector3(0.5f, 0.5f, 0.5f);
    }

    public static class Time
    {
        public static float time => 0f;
        public static float deltaTime => 0.016f;
    }

    public class Component : Object
    {
        public Transform transform => default!;
        public GameObject gameObject => default!;
        public T GetComponentInChildren<T>() where T : Component => default!;
        public T GetComponentInChildren<T>(bool includeInactive) where T : Component => default!;
        public T[] GetComponentsInChildren<T>() where T : Component => Array.Empty<T>();
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component => Array.Empty<T>();
        public T[] GetComponents<T>() where T : Component => Array.Empty<T>();
    }

    public class Behaviour : Component
    {
        public bool enabled { get; set; } = true;
        public bool isActiveAndEnabled => enabled;
    }

    public class Renderer : Component
    {
        public Material material => default!;
        public Material[] materials { get; set; } = Array.Empty<Material>();
        public Material sharedMaterial => default!;
        public void GetPropertyBlock(MaterialPropertyBlock properties) { }
        public void SetPropertyBlock(MaterialPropertyBlock properties) { }
    }

    public class MaterialPropertyBlock
    {
        public void SetColor(string? propertyName, Color color) { }
        public void SetFloat(string? propertyName, float value) { }
        public void GetColor(string? propertyName, out Color color) => color = default;
        public void GetFloat(string? propertyName, out float value) => value = default;
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
        public T[] GetComponentsInChildren<T>() where T : Component => Array.Empty<T>();
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component => Array.Empty<T>();
        public T[] GetComponents<T>() where T : Component => Array.Empty<T>();
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
        public Vector3 forward => new Vector3(0, 0, 1);
        public Vector3 up => new Vector3(0, 1, 0);
        public Vector3 right => new Vector3(1, 0, 0);
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) { }
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 down => new Vector3(0, -1, 0);
        public static Vector3 left => new Vector3(-1, 0, 0);
        public static Vector3 right => new Vector3(1, 0, 0);
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 back => new Vector3(0, 0, -1);
        public static float Distance(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z).magnitude;
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => a.x != b.x || a.y != b.y || a.z != b.z;
        public float magnitude => (float)System.Math.Sqrt(x * x + y * y + z * z);
        public Vector3 normalized => magnitude > 0 ? this / magnitude : zero;
        public override string ToString() => $"({x}, {y}, {z})";
        public override bool Equals(object? other) => other is Vector3 v && this == v;
        public override int GetHashCode() => HashCode.Combine(x, y, z);
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => new Quaternion(0, 0, 0, 1);
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion rotation, Vector3 point) => point;
        public static Quaternion LookRotation(Vector3 forward) => identity;
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards) => identity;
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
        public static Color cyan => new Color(0, 1, 1);
        public static Color magenta => new Color(1, 0, 1);
        public static Color gray => new Color(0.5f, 0.5f, 0.5f);
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
        public void SetColor(string? propertyName, Color color) { }
        public void SetFloat(string? propertyName, float value) { }
        public Shader? shader { get; set; }
    }

    public class Shader : Object
    {
        public static Shader? Find(string name) => null;
        public static Shader? Find(string name, out Shader shader)
        {
            shader = null!;
            return null;
        }
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

    public class Animator : Component
    {
        public void Play(string? stateName) { }
        public void SetFloat(string? name, float value) { }
        public void SetBool(string? name, bool value) { }
        public void SetInteger(string? name, int value) { }
        public void SetTrigger(string? name) { }
        public void ResetTrigger(string? name) { }
        public bool GetBool(string? name) => false;
        public float GetFloat(string? name) => 0f;
        public int GetInteger(string? name) => 0;
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

    public class AudioSource : Component
    {
        public void PlayOneShot(AudioClip? clip) { }
        public AudioClip? clip { get; set; }
        public bool isPlaying { get; set; }
        public void Play() { }
        public void Stop() { }
    }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject => default!;
    }

    public abstract class MonoBehaviour : Behaviour
    {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public Coroutine? StartCoroutine(IEnumerator routine) => null;
        public Coroutine? StartCoroutine(string methodName) => null;
        public Coroutine? StartCoroutine(string methodName, object value) => null;
        public void StopCoroutine(Coroutine? routine) { }
        public void StopCoroutine(IEnumerator routine) { }
        public void StopCoroutine(string methodName) { }
        public void StopAllCoroutines() { }
    }

    public class Coroutine : Object { }

    public struct WaitForSeconds : IEnumerator
    {
        public float waitTime;
        public WaitForSeconds(float seconds) { waitTime = seconds; }
        public object Current => null!;
        public bool MoveNext() => false;
        public void Reset() { }
    }

    public interface IEnumerator
    {
        object Current { get; }
        bool MoveNext();
        void Reset();
    }

    public class Physics
    {
        public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -1)
        {
            hitInfo = default;
            return false;
        }
        public static bool Raycast(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity) => false;
    }

    public struct RaycastHit
    {
        public Vector3 point;
        public Vector3 normal;
        public float distance;
        public Collider? collider;
        public Transform? transform;
    }

    public enum KeyCode
    {
        None = 0,
        Backspace = 8,
        Tab = 9,
        Return = 13,
        Escape = 27,
        Space = 32,
        E = 101,
        W = 119,
        A = 97,
        S = 115,
        D = 100,
    }

    public class Input
    {
        public static bool GetKeyDown(KeyCode key) => false;
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKeyUp(KeyCode key) => false;
        public static Vector3 mousePosition => Vector3.zero;
    }

    public class ShaderPropertyID
    {
        public static int Get(string name) => 0;
    }

    namespace AI
    {
        public enum NavMeshPathStatus
        {
            PathComplete,
            PathPartial,
            PathInvalid
        }

        public class NavMeshAgent : Component
        {
            public float speed { get; set; }
            public float acceleration { get; set; }
            public float stoppingDistance { get; set; }
            public bool autoBraking { get; set; }
            public bool hasPath { get; set; }
            public bool pathPending { get; set; }
            public NavMeshPathStatus pathStatus { get; set; }
            public Vector3 velocity { get; set; }
            public void SetDestination(Vector3 target) { }
            public void ResetPath() { }
        }

        public struct NavMeshHit
        {
            public Vector3 position;
            public Vector3 normal;
            public float distance;
            public int hit;
        }

        public static class NavMesh
        {
            public static int AllAreas => -1;
            public static bool SamplePosition(Vector3 source, out NavMeshHit hit, float distance, int areaMask)
            {
                hit = default;
                return false;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HeaderAttribute : Attribute
    {
        public string header { get; }
        public HeaderAttribute(string header) { this.header = header; }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SerializeFieldAttribute : Attribute
    {
        public SerializeFieldAttribute() { }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TooltipAttribute : Attribute
    {
        public string tooltip { get; }
        public TooltipAttribute(string tooltip) { this.tooltip = tooltip; }
    }
}