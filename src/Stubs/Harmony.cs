using System;

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HarmonyPatchAttribute : Attribute
    {
        public Type? Type { get; set; }
        public string? MethodName { get; set; }
        public Type[]? ArgumentTypes { get; set; }
        public HarmonyPatchAttribute() { }
        public HarmonyPatchAttribute(Type? type, string? methodName = null) { Type = type; MethodName = methodName; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPrefixAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPostfixAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyTranspilerAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyFinalizerAttribute : Attribute { }

    public class HarmonyMethod
    {
        public Type? DeclaringType { get; set; }
        public string? MethodName { get; set; }
        public HarmonyMethod() { }
        public HarmonyMethod(Type? declaringType, string? methodName = null) { DeclaringType = declaringType; MethodName = methodName; }
    }

    public class Harmony
    {
        public string? Id { get; }
        public Harmony(string id) { Id = id; }
        public void PatchAll() { }
        public void PatchAll(System.Reflection.Assembly assembly) { }
        public void UnpatchAll(string? id) { }
    }

    public static class HarmonyExtensions
    {
        public static Harmony CreateAndPatchAll(string id, System.Reflection.Assembly? assembly = null) => new Harmony(id);
    }
}