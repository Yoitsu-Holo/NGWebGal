using System;
using Avalonia.Controls;
using NGWebGal.Editor.Services.PropertyReflection;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for string properties
/// </summary>
public partial class StringPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;

    public event EventHandler? ValueChanged;

    public StringPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        InitializeComponent();

        // Configure label
        LabelTextBlock.Text = descriptor.Name + ":";

        // Multi-line for "Text" property
        if (descriptor.Name == "Text")
        {
            ValueTextBox.AcceptsReturn = true;
            ValueTextBox.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
            ValueTextBox.MinHeight = 42;
        }

        // Apply read-only styling
        if (descriptor.IsReadOnly)
        {
            ValueTextBox.IsReadOnly = true;
            ValueTextBox.Background = Avalonia.Media.Brushes.LightGray;
            ValueTextBox.Opacity = 0.7;
        }

        // Subscribe to changes
        ValueTextBox.PropertyChanged += OnTextBoxPropertyChanged;
    }

    public void Bind(object target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));

        // Load current value
        var currentValue = Descriptor.Getter(_target) as string ?? string.Empty;
        ValueTextBox.Text = currentValue;
    }

    public void Unbind()
    {
        _target = null;
        ValueTextBox.Text = string.Empty;
    }

    private void OnTextBoxPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        // Only react to Text property changes
        if (e.Property.Name != nameof(ValueTextBox.Text))
            return;

        if (_target == null || Descriptor.Setter == null)
            return;

        // Update target object
        var newValue = ValueTextBox.Text ?? string.Empty;
        Descriptor.Setter(_target, newValue);

        // Raise ValueChanged event
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
