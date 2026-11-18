using System;

namespace BepInEx
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BepInPluginAttribute : Attribute
    {
        public string GUID { get; }
        public string Name { get; }
        public string Version { get; }
        public BepInPluginAttribute(string guid, string name, string version)
        {
            GUID = guid;
            Name = name;
            Version = version;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BepInDependencyAttribute : Attribute
    {
        public string GUID { get; }
        public string? Version { get; }
        public BepInDependencyAttribute(string guid) { GUID = guid; }
        public BepInDependencyAttribute(string guid, string? version) { GUID = guid; Version = version; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class BepInProcessAttribute : Attribute
    {
        public string ProcessName { get; }
        public BepInProcessAttribute(string processName) { ProcessName = processName; }
    }

    public abstract class BaseUnityPlugin : UnityEngine.MonoBehaviour
    {
        protected BaseUnityPlugin() { }
        public BepInEx.Configuration.ConfigFile Config { get; } = new BepInEx.Configuration.ConfigFile(null!, true);
        public static void Log(string? message) { }
        public static void LogError(string? message) { }
        public static void LogWarning(string? message) { }
        public static void LogInfo(string? message) { }
    }

    public static class Logger
    {
        public static void Log(string? message) { }
        public static void LogError(string? message) { }
        public static void LogWarning(string? message) { }
        public static void LogInfo(string? message) { }
    }
}

namespace BepInEx.Configuration
{
    public class ConfigFile
    {
        public ConfigFile(string? configPath, bool saveOnDisk) { }
        public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string? description = null)
            => new ConfigEntry<T>(this, section, key, defaultValue, description);
        public ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, ConfigDescription description)
            => new ConfigEntry<T>(this, section, key, defaultValue, description);
        public void Save() { }
        public void Reload() { }
    }

    public class ConfigEntry<T>
    {
        public string Section { get; }
        public string Key { get; }
        public T Value { get; set; }
        public T DefaultValue { get; }
        public string? Description { get; }
        public ConfigEntry(ConfigFile configFile, string section, string key, T defaultValue, string? description)
        {
            Section = section;
            Key = key;
            Value = defaultValue;
            DefaultValue = defaultValue;
            Description = description;
        }
        public ConfigEntry(ConfigFile configFile, string section, string key, T defaultValue, ConfigDescription description)
        {
            Section = section;
            Key = key;
            Value = defaultValue;
            DefaultValue = defaultValue;
            Description = description.Description;
        }
    }

    public class ConfigDescription
    {
        public string? Description { get; }
        public ConfigDescription(string? description = null) { Description = description; }
        public ConfigDescription(string? description, AcceptableValueBase? acceptableValues) { Description = description; }
        public ConfigDescription(string? description, AcceptableValueBase? acceptableValues, Action? settingChanged) { Description = description; }
    }

    public abstract class AcceptableValueBase { }

    public class AcceptableValueRange<T> : AcceptableValueBase where T : struct, IComparable<T>
    {
        public T Min { get; }
        public T Max { get; }
        public AcceptableValueRange(T min, T max) { Min = min; Max = max; }
    }

    public class AcceptableValueList<T> : AcceptableValueBase
    {
        public AcceptableValueList(params T[] values) { }
    }
}