using System;

namespace NGWebGal.Editor.Services.PropertyReflection;

/// <summary>
/// Immutable descriptor for a reflected property with compiled getter/setter delegates
/// </summary>
public sealed record PropertyDescriptor
{
    /// <summary>
    /// Display name for the property
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Hierarchical path (e.g., "Style.Padding.Left")
    /// </summary>
    public required string PropertyPath { get; init; }

    /// <summary>
    /// The property's type
    /// </summary>
    public required Type PropertyType { get; init; }

    /// <summary>
    /// The type that declares this property
    /// </summary>
    public required Type DeclaringType { get; init; }

    /// <summary>
    /// Depth in the inheritance hierarchy (0 = concrete type, higher = base types)
    /// </summary>
    public required int HierarchyDepth { get; init; }

    /// <summary>
    /// Whether the property is read-only
    /// </summary>
    public required bool IsReadOnly { get; init; }

    /// <summary>
    /// Whether the property is expandable (complex type with nested properties)
    /// </summary>
    public required bool IsExpandable { get; init; }

    /// <summary>
    /// Category for grouping properties in the UI
    /// </summary>
    public required PropertyCategory Category { get; init; }

    /// <summary>
    /// Compiled getter delegate for performance (retrieves value from an object instance)
    /// </summary>
    public required Func<object, object?> Getter { get; init; }

    /// <summary>
    /// Compiled setter delegate for performance (sets value on an object instance)
    /// Null if property is read-only
    /// </summary>
    public required Action<object, object?>? Setter { get; init; }
}
