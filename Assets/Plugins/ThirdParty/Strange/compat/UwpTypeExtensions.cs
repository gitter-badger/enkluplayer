#if !NETFX_CORE

using System;

/// <summary>
/// Looks like .NET Core's TypeInfo object.
/// </summary>
public class TypeInfo
{
    /// <summary>
    /// The Type this info refers to.
    /// </summary>
    public Type Type { get; private set; }

    /// <summary>
    /// True iff IsPrimitive.
    /// </summary>
    public bool IsPrimitive
    {
        get { return Type.IsPrimitive; }
    }

    /// <summary>
    /// True iff IsGenericType.
    /// </summary>
    public bool IsGenericType
    {
        get { return Type.IsGenericType; }
    }

    /// <summary>
    /// True iff IsInterface.
    /// </summary>
    public bool IsInterface
    {
        get { return Type.IsInterface; }
    }

    /// <summary>
    /// True iff IsAbstract.
    /// </summary>
    public bool IsAbstract
    {
        get { return Type.IsAbstract; }
    }

    /// <summary>
    /// True iff type is enum.
    /// </summary>
    public bool IsEnum
    {
        get { return Type.IsEnum; }
    }

    /// <summary>
    /// True iff type is value type.
    /// </summary>
    public bool IsValueType
    {
        get { return Type.IsValueType; }
    }

    /// <summary>
    /// Retrieves the type's base type.
    /// </summary>
    public Type BaseType
    {
        get { return Type.BaseType; }
    }
    
    /// <summary>
    /// Creates a new TypeInfo.
    /// </summary>
    /// <param name="type"></param>
    public TypeInfo(Type type)
    {
        Type = type;
    }

    /// <summary>
    /// True iff...
    /// </summary>
    /// <param name="anyType"></param>
    /// <returns></returns>
    public bool IsInstanceOfType(Type anyType)
    {
        return Type.IsInstanceOfType(anyType);
    }

    /// <summary>
    /// True iff...
    /// </summary>
    /// <param name="anyType"></param>
    /// <returns></returns>
    public bool IsSubclassOf(Type anyType)
    {
        return Type.IsSubclassOf(anyType);
    }
}

/// <summary>
/// Type extension to look like .NET Core.
/// </summary>
public static class UwpTypeExtensions
{
    /// <summary>
    /// Retrieves our fake TypeInfo object.
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static TypeInfo GetTypeInfo(this Type @this)
    {
        return new TypeInfo(@this);
    }
}

#endif