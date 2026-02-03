using System;
using Avalonia.Controls;
using NGWebGal.Editor.Services.PropertyReflection;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Interface for property editors that provide UI controls for editing properties
/// </summary>
public interface IPropertyEditor
{
    /// <summary>
    /// Gets the property descriptor this editor is bound to
    /// </summary>
    PropertyDescriptor Descriptor { get; }

    /// <summary>
    /// Gets the Avalonia control for this editor
    /// </summary>
    Control Control { get; }

    /// <summary>
    /// Event raised when the property value changes
    /// </summary>
    event EventHandler? ValueChanged;

    /// <summary>
    /// Binds this editor to a target object
    /// </summary>
    /// <param name="target">The object whose property to edit</param>
    void Bind(object target);

    /// <summary>
    /// Unbinds this editor from its current target
    /// </summary>
    void Unbind();
}
