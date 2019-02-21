#if NETFX_CORE || NETSTANDARD1_3
using System;
using System.Linq;
using System.Reflection;

internal static class ReflectionExtensions
{
    internal static bool IsEnum(this Type type)
    {
        return type.GetTypeInfo().IsEnum;
    }

    internal static bool IsGenericType(this Type type)
    {
        return type.GetTypeInfo().IsGenericType;
    }

    internal static bool IsValueType(this Type type)
    {
        return type.GetTypeInfo().IsValueType;
    }

    internal static bool HasAttribute<T>(this ParameterInfo member) where T : Attribute
    {
        return member.GetCustomAttributes<T>().Any();
    }

    internal static bool HasAttribute<T>(this MethodBase methodBase) where T : Attribute
    {
        return methodBase.GetCustomAttributes<T>().Any();
    }

    internal static T[] GetCustomAttributes<T>(this Type @this, bool inherit) where T : Attribute
    {
        return (T[]) @this.GetTypeInfo().GetCustomAttributes(typeof(T), inherit).ToArray();
    }

    internal static object[] GetCustomAttributes(this Type @this, Type attributeType, bool inherit)
    {
        return @this.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
    }
}
#else
using System;
using System.Reflection;

internal static class ReflectionExtensions
{
    internal static bool IsEnum(this Type type)
    {
        return type.IsEnum;
    }

    internal static bool IsGenericType(this Type type)
    {
        return type.IsGenericType;
    }

    internal static bool IsValueType(this Type type)
    {
        return type.IsValueType;
    }

    internal static bool HasAttribute<T>(this ParameterInfo member) where T : Attribute
    {
        return Attribute.IsDefined(member, typeof(T));
    }

    internal static bool HasAttribute<T>(this MethodBase methodBase) where T : Attribute
    {
        return Attribute.IsDefined(methodBase, typeof(T), true);
    }

    internal static T[] GetCustomAttributes<T>(this Type @this, bool inherit) where T : Attribute
    {
        return (T[])Attribute.GetCustomAttributes(@this, typeof(T), inherit);
    }

    internal static object[] GetCustomAttributes(this Type @this, Type attributeType, bool inherit)
    {
        return (object[]) Attribute.GetCustomAttributes(@this, attributeType, inherit);
    }

    internal static MethodInfo GetMethodInfo(this Delegate d)
    {
        return d.Method;
    }
}
#endif
