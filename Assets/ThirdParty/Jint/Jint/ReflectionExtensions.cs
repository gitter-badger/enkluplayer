﻿#if NETFX_CORE
using System;
using System.Linq;
using System.Reflection;

namespace Jint
{
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

        internal static object[] GetCustomAttributes(this Type @this, Type attributeType, bool inherit)
        {
            return @this.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }
    }
}
#else
using System;
using System.Reflection;

namespace Jint
{
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

        internal static MethodInfo GetMethodInfo(this Delegate d)
        {
            return d.Method;
        }
    }
}
#endif
