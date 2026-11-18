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
        public void Destroy(Object? obj) { }
        public void Destroy(Object? obj, float t) { }
        public static Object? FindObjectOfType<T>() where T : Object => null;
        public static T[] FindObjectsOfType<T>() where T : Object => Array.Empty<T>();
        public static void DontDestroyOnLoad(Object? obj) { }
        public static T Instantiate<T>(T original) where T : Object => default!;
        public static T Instantiate<T>(T original, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation) where T : Object => default!;
    }

    public class Component : Object
    {
        public Transform transform => default!;
        public GameObject gameObject => default!;
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
    }

    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);
        public override string ToString() => $"({x}, {y}, {z})";
    }

    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => new Quaternion(0, 0, 0, 1);
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion rotation, Vector3 point) => point;
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
        public void SetColor(string? propertyName, Color color) { }
        public void SetFloat(string? propertyName, float value) { }
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

    public class ScriptableObject : Object { }

    public abstract class MonoBehaviour : Component
    {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
    }
}