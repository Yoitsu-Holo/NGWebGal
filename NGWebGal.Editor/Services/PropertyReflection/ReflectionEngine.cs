using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NGWebGal.Editor.Models;
using NGWebGal.Handler;
using NGWebGal.Layer;
using NGWebGal.Types;
using SkiaSharp;

namespace NGWebGal.Editor.Services.PropertyReflection;

/// <summary>
/// Core reflection engine that discovers properties from ILayer types
/// </summary>
public class ReflectionEngine
{
    private readonly Dictionary<Type, PropertyDescriptor[]> _cache = new();
    private readonly Func<ILayer, EditorWidget?>? _widgetLookup;

    /// <summary>
    /// Initializes a new instance of the ReflectionEngine.
    /// </summary>
    /// <param name="widgetLookup">Optional function to find EditorWidget for a given ILayer (for shadow state)</param>
    public ReflectionEngine(Func<ILayer, EditorWidget?>? widgetLookup = null)
    {
        _widgetLookup = widgetLookup;
    }

    /// <summary>
    /// Discovers all visible properties from a concrete ILayer type
    /// </summary>
    /// <param name="concreteType">The concrete type to inspect (e.g., WidgetColorBox)</param>
    /// <returns>Array of property descriptors, ordered by hierarchy depth</returns>
    public PropertyDescriptor[] DiscoverProperties(Type concreteType)
    {
        if (concreteType == null)
            throw new ArgumentNullException(nameof(concreteType));

        // Check cache first
        if (_cache.TryGetValue(concreteType, out var cached))
            return cached;

        var properties = new List<PropertyDescriptor>();
        var hierarchy = new List<Type>();

        // Walk inheritance chain from concrete type up to (and including) LayerBase
        // Stop at HandlerBase or Object
        Type? currentType = concreteType;
        while (currentType != null && currentType != typeof(object))
        {
            // Stop if we've reached HandlerBase or below
            if (currentType == typeof(HandlerBase))
                break;

            hierarchy.Add(currentType);

            // Add LayerBase and then stop
            if (currentType == typeof(LayerBase))
                break;

            currentType = currentType.BaseType;
        }

        // Process hierarchy from base to concrete for proper ordering
        hierarchy.Reverse();

        for (int depth = 0; depth < hierarchy.Count; depth++)
        {
            var type = hierarchy[depth];

            // Get only properties declared on this specific type
            var typeProperties = type.GetProperties(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            foreach (var prop in typeProperties)
            {
                // Skip excluded properties
                if (TypeFilters.IsExcludedType(prop.PropertyType))
                    continue;

                // Create a new visited set for each property to avoid false circular reference detection
                var visitedTypes = new HashSet<Type>();

                // Create descriptor with compiled delegates
                var descriptor = CreatePropertyDescriptor(
                    prop,
                    type,
                    depth,
                    prop.Name,
                    visitedTypes);

                if (descriptor != null)
                    properties.Add(descriptor);
            }
        }

        // Add shadow properties for SetColor and SetImage methods
        AddShadowProperties(concreteType, properties, hierarchy[^1]); // Use concrete type for hierarchy depth

        // Cache results
        var result = properties.ToArray();
        _cache[concreteType] = result;

        return result;
    }

    /// <summary>
    /// Creates a PropertyDescriptor with compiled getter/setter delegates
    /// </summary>
    private PropertyDescriptor? CreatePropertyDescriptor(
        PropertyInfo prop,
        Type declaringType,
        int hierarchyDepth,
        string propertyPath,
        HashSet<Type> visitedTypes)
    {
        // Circular reference protection
        if (visitedTypes.Contains(prop.PropertyType))
            return null;

        visitedTypes.Add(prop.PropertyType);

        // Determine if property is expandable (complex type with nested properties)
        bool isExpandable = ShouldExpand(prop.PropertyType);

        // Categorize property
        var category = CategorizeProperty(prop.Name);

        // Compile getter
        var getter = CompileGetter(prop);

        // Compile setter (null if read-only)
        var setter = prop.CanWrite ? CompileSetter(prop) : null;

        return new PropertyDescriptor
        {
            Name = prop.Name,
            PropertyPath = propertyPath,
            PropertyType = prop.PropertyType,
            DeclaringType = declaringType,
            HierarchyDepth = hierarchyDepth,
            IsReadOnly = !prop.CanWrite,
            IsExpandable = isExpandable,
            Category = category,
            Getter = getter,
            Setter = setter
        };
    }

    /// <summary>
    /// Compiles a high-performance getter using Expression trees with error handling
    /// </summary>
    private static Func<object, object?> CompileGetter(PropertyInfo prop)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var castInstance = Expression.Convert(instance, prop.DeclaringType!);
        var propertyAccess = Expression.Property(castInstance, prop);
        var castResult = Expression.Convert(propertyAccess, typeof(object));

        var compiledGetter = Expression.Lambda<Func<object, object?>>(castResult, instance).Compile();

        // Wrap with error handling
        return (obj) =>
        {
            try
            {
                return compiledGetter(obj);
            }
            catch (Exception ex)
            {
                // Log error and return default value
                System.Diagnostics.Debug.WriteLine($"Error getting property {prop.Name}: {ex.Message}");
                return prop.PropertyType.IsValueType ? Activator.CreateInstance(prop.PropertyType) : null;
            }
        };
    }

    /// <summary>
    /// Compiles a high-performance setter using Expression trees with error handling
    /// </summary>
    private static Action<object, object?> CompileSetter(PropertyInfo prop)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var castInstance = Expression.Convert(instance, prop.DeclaringType!);
        var castValue = Expression.Convert(value, prop.PropertyType);
        var propertyAccess = Expression.Property(castInstance, prop);
        var assign = Expression.Assign(propertyAccess, castValue);

        var compiledSetter = Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();

        // Wrap with error handling
        return (obj, val) =>
        {
            try
            {
                compiledSetter(obj, val);
            }
            catch (Exception ex)
            {
                // Log error and continue
                System.Diagnostics.Debug.WriteLine($"Error setting property {prop.Name}: {ex.Message}");
            }
        };
    }

    /// <summary>
    /// Determines if a property type should be expandable in the UI
    /// </summary>
    private static bool ShouldExpand(Type type)
    {
        // Don't expand primitives, enums, strings, or whitelisted types
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || TypeFilters.IsWhitelisted(type))
            return false;

        // Expand complex types (classes, structs) that have properties
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Length > 0;
    }

    /// <summary>
    /// Categorizes a property based on its name
    /// </summary>
    private static PropertyCategory CategorizeProperty(string propertyName)
    {
        return propertyName switch
        {
            "Name" or "Id" or "Value" => PropertyCategory.Identity,
            "Position" or "Size" or "Offset" or "Window" => PropertyCategory.Transform,
            "Visible" or "Enable" or "Status" => PropertyCategory.Display,
            "Text" or "TextSize" or "Typeface" => PropertyCategory.Content,
            "Style" => PropertyCategory.Appearance,
            _ => PropertyCategory.Advanced
        };
    }

    /// <summary>
    /// Clears the internal cache (useful for testing)
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Adds shadow properties for SetColor and SetImage methods.
    /// These methods are imperative commands, not queryable properties,
    /// so we create shadow storage in EditorWidget.
    /// </summary>
    private void AddShadowProperties(Type concreteType, List<PropertyDescriptor> properties, Type hierarchyRoot)
    {
        if (_widgetLookup == null)
            return; // Shadow properties require widget lookup

        // Check for SetColor method - only create shadow property if method is overridden
        var setColorMethod = concreteType.GetMethod("SetColor", new[] { typeof(SKColor), typeof(int) });
        if (setColorMethod != null && setColorMethod.DeclaringType != typeof(LayerBase))
        {
            properties.Add(new PropertyDescriptor
            {
                Name = "Color",
                PropertyPath = "Color",
                PropertyType = typeof(SKColor),
                DeclaringType = concreteType,
                HierarchyDepth = 0, // Show at top level
                IsReadOnly = false,
                IsExpandable = false,
                Category = PropertyCategory.Appearance,
                Getter = CreateColorGetter(),
                Setter = CreateColorSetter()
            });
        }

        // Check for SetImage method - only create shadow property if method is overridden
        // Create both "Image" (simple) and "Images" (advanced multi-image) shadow properties
        var setImageMethod = concreteType.GetMethod("SetImage", new[] { typeof(SKBitmap), typeof(int), typeof(IRect) });
        if (setImageMethod != null && setImageMethod.DeclaringType != typeof(LayerBase))
        {
            // Simple image property for basic single-image editing
            properties.Add(new PropertyDescriptor
            {
                Name = "Image",
                PropertyPath = "Image",
                PropertyType = typeof(string), // File path, not SKBitmap
                DeclaringType = concreteType,
                HierarchyDepth = 0, // Show at top level
                IsReadOnly = false,
                IsExpandable = false,
                Category = PropertyCategory.Content,
                Getter = CreateImageGetter(),
                Setter = CreateImageSetter()
            });

            // Advanced multi-image property for image management with cropping
            properties.Add(new PropertyDescriptor
            {
                Name = "Images",
                PropertyPath = "Images",
                PropertyType = typeof(object), // Special marker type for multi-image editor
                DeclaringType = concreteType,
                HierarchyDepth = 0,
                IsReadOnly = false,
                IsExpandable = false,
                Category = PropertyCategory.Content,
                Getter = CreateMultiImageGetter(),
                Setter = CreateMultiImageSetter()
            });
        }
    }

    /// <summary>
    /// Creates a getter for the Color shadow property.
    /// Uses ILayer.GetColor(0) method instead of shadow state.
    /// </summary>
    private Func<object, object?> CreateColorGetter()
    {
        return (obj) =>
        {
            if (obj is not ILayer layer)
                return SKColors.Transparent;

            try
            {
                // Use the new GetColor method
                return layer.GetColor(0);
            }
            catch
            {
                return SKColors.Transparent;
            }
        };
    }

    /// <summary>
    /// Creates a setter for the Color shadow property.
    /// Writes to EditorWidget.Color AND calls SetColor(color, 0).
    /// </summary>
    private Action<object, object?> CreateColorSetter()
    {
        return (obj, value) =>
        {
            if (obj is not ILayer layer || value is not SKColor color || _widgetLookup == null)
                return;

            // Update shadow state
            var widget = _widgetLookup(layer);
            if (widget != null)
            {
                widget.Color = EditorColor.FromSKColor(color);
            }

            // Call the actual SetColor method
            layer.SetColor(color, 0);
        };
    }

    /// <summary>
    /// Creates a getter for the Image shadow property.
    /// Reads from EditorWidget.ImagePaths["Image"].
    /// </summary>
    private Func<object, object?> CreateImageGetter()
    {
        return (obj) =>
        {
            if (obj is not ILayer layer || _widgetLookup == null)
                return string.Empty;

            var widget = _widgetLookup(layer);
            return widget?.ImagePaths.GetValueOrDefault("Image") ?? string.Empty;
        };
    }

    /// <summary>
    /// Creates a setter for the Image shadow property.
    /// Writes to EditorWidget.ImagePaths["Image"] AND calls SetImage(bitmap, 0).
    /// </summary>
    private Action<object, object?> CreateImageSetter()
    {
        return (obj, value) =>
        {
            if (obj is not ILayer layer || value is not string path || _widgetLookup == null)
                return;

            // Update shadow state
            var widget = _widgetLookup(layer);
            if (widget != null)
            {
                if (string.IsNullOrEmpty(path))
                {
                    widget.ImagePaths.Remove("Image");
                }
                else
                {
                    widget.ImagePaths["Image"] = path;
                }
            }

            // Load and set the image
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    var bitmap = SKBitmap.Decode(path);
                    if (bitmap != null)
                    {
                        layer.SetImage(bitmap, 0);
                    }
                }
                catch
                {
                    // Ignore image loading errors
                }
            }
        };
    }

    /// <summary>
    /// Creates a getter for the Images shadow property.
    /// Returns the layer itself so the editor can call GetImageIds/GetSubImageInfo.
    /// </summary>
    private Func<object, object?> CreateMultiImageGetter()
    {
        return (obj) => obj; // Return the layer itself
    }

    /// <summary>
    /// Creates a setter for the Images shadow property.
    /// This is used by the property editor which calls SetImage directly.
    /// </summary>
    private Action<object, object?> CreateMultiImageSetter()
    {
        return (obj, value) =>
        {
            // Setter is handled by the property editor calling SetImage directly
            // This placeholder is required for the descriptor
        };
    }
}
