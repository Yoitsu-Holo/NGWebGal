using System;
using NGWebGal.Editor.Controls.PropertyEditors;
using NGWebGal.Editor.Models;
using NGWebGal.Layer;
using NGWebGal.Types;
using SkiaSharp;

namespace NGWebGal.Editor.Services.PropertyReflection;

/// <summary>
/// Factory for creating property editors based on property type
/// </summary>
public class PropertyEditorFactory
{
    private readonly Func<ILayer, EditorWidget?>? _widgetLookup;

    /// <summary>
    /// Initializes a new instance of the PropertyEditorFactory
    /// </summary>
    /// <param name="widgetLookup">Optional function to find EditorWidget for a given ILayer</param>
    public PropertyEditorFactory(Func<ILayer, EditorWidget?>? widgetLookup = null)
    {
        _widgetLookup = widgetLookup;
    }

    /// <summary>
    /// Creates an appropriate editor for the given property descriptor
    /// </summary>
    /// <param name="descriptor">The property descriptor</param>
    /// <returns>A property editor instance, or null if no suitable editor is available</returns>
    public IPropertyEditor? CreateEditor(PropertyDescriptor descriptor)
    {
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));

        var propertyType = descriptor.PropertyType;

        // Multi-image editor (for Images shadow property)
        if (descriptor.Name == "Images" && propertyType == typeof(object))
        {
            return new MultiImagePropertyEditor(descriptor, _widgetLookup);
        }

        // String editor
        if (propertyType == typeof(string))
        {
            return new StringPropertyEditor(descriptor);
        }

        // Numeric editors
        if (propertyType == typeof(int))
        {
            return new NumericPropertyEditor<int>(descriptor);
        }

        if (propertyType == typeof(float))
        {
            return new NumericPropertyEditor<float>(descriptor);
        }

        if (propertyType == typeof(double))
        {
            return new NumericPropertyEditor<double>(descriptor);
        }

        // Bool editor
        if (propertyType == typeof(bool))
        {
            return new BoolPropertyEditor(descriptor);
        }

        // Vector editors
        if (propertyType == typeof(IVector) || propertyType == typeof(FVector))
        {
            return new VectorPropertyEditor(descriptor);
        }

        // Color editor
        if (propertyType == typeof(SKColor))
        {
            return new ColorPropertyEditor(descriptor);
        }

        // Enum editor
        if (propertyType.IsEnum)
        {
            return new EnumPropertyEditor(descriptor);
        }

        // TextPadding specialized editor
        if (propertyType == typeof(TextPadding))
        {
            return new PaddingPropertyEditor(descriptor);
        }

        // Complex/Expandable types
        if (descriptor.IsExpandable)
        {
            return new ExpandablePropertyEditor(descriptor);
        }

        // Return null for unsupported types instead of throwing
        System.Diagnostics.Debug.WriteLine($"[PropertyEditorFactory] No editor available for property type: {propertyType.Name}");
        return null;
    }
}
