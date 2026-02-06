using System;
using System.ComponentModel;
using Avalonia.Controls;
using PropertyDescriptor = NGWebGal.Editor.Services.PropertyReflection.PropertyDescriptor;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for string properties
/// </summary>
public partial class StringPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private INotifyPropertyChanged? _observableTarget;
    private bool _isUpdating = false;

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

        if (target is INotifyPropertyChanged observable)
        {
            _observableTarget = observable;
            _observableTarget.PropertyChanged += OnTargetPropertyChanged;
        }

        UpdateFromTarget();
    }

    private void UpdateFromTarget()
    {
        if (_target == null) return;

        _isUpdating = true;
        try
        {
            var currentValue = Descriptor.Getter(_target) as string ?? string.Empty;
            ValueTextBox.Text = currentValue;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == Descriptor.Name)
        {
            UpdateFromTarget();
        }
    }

    public void Unbind()
    {
        if (_observableTarget != null)
        {
            _observableTarget.PropertyChanged -= OnTargetPropertyChanged;
            _observableTarget = null;
        }
        _target = null;
        ValueTextBox.Text = string.Empty;
    }

    private void OnTextBoxPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        // Only react to Text property changes
        if (e.Property.Name != nameof(ValueTextBox.Text))
            return;

        if (_isUpdating || _target == null || Descriptor.Setter == null)
            return;

        // Update target object
        var newValue = ValueTextBox.Text ?? string.Empty;
        Descriptor.Setter(_target, newValue);

        // Raise ValueChanged event
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
