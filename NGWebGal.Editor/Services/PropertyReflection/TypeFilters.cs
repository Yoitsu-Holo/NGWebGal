using System;

namespace NGWebGal.Editor.Services.PropertyReflection;

/// <summary>
/// Static utility class for filtering types during reflection discovery
/// </summary>
public static class TypeFilters
{
    /// <summary>
    /// Maximum depth for nested property traversal
    /// </summary>
    public const int MaxDepth = 3;

    /// <summary>
    /// Checks if a type should be excluded from property discovery
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if type should be excluded</returns>
    public static bool IsExcludedType(Type type)
    {
        if (type == null)
            return true;

        // Check whitelist first - SKColor is explicitly allowed
        if (IsWhitelisted(type))
            return false;

        // Exclude SkiaSharp namespace (except whitelisted types)
        if (type.Namespace?.StartsWith("SkiaSharp") == true)
            return true;

        // Exclude specific SkiaSharp types
        var typeName = type.Name;
        if (typeName == "SKPaint" ||
            typeName == "SKFont" ||
            typeName == "SKBitmap" ||
            typeName == "SKMatrix" ||
            typeName == "SKCanvas" ||
            typeName == "SKTypeface")
            return true;

        // Exclude Action/Func delegates
        if (type.Name.StartsWith("Action") || type.Name.StartsWith("Func"))
            return true;

        // Exclude NGWebGal animation and handler interfaces/types
        if (type.Namespace?.Contains("Animations") == true ||
            type.Namespace?.Contains("Handler") == true)
            return true;

        // Exclude IAnimation and IHandler types
        if (type.Name == "IAnimation" || type.Name == "IHandler")
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a type is explicitly whitelisted despite normally being excluded
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if type is whitelisted</returns>
    public static bool IsWhitelisted(Type type)
    {
        if (type == null)
            return false;

        // SKColor is essential for visual editing
        return type.Name == "SKColor";
    }

    /// <summary>
    /// Determines if a type should be traversed for nested properties
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <param name="depth">Current depth in the traversal</param>
    /// <returns>True if type should be traversed</returns>
    public static bool ShouldTraverse(Type type, int depth)
    {
        // Respect depth limit
        if (depth >= MaxDepth)
            return false;

        // Don't traverse excluded types
        if (IsExcludedType(type))
            return false;

        // Don't traverse primitive types, enums, or strings
        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            return false;

        // Don't traverse whitelisted types (they're leaf nodes)
        if (IsWhitelisted(type))
            return false;

        return true;
    }
}
