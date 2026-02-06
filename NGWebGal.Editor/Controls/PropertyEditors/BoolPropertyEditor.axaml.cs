using System;
using System.ComponentModel;
using Avalonia.Controls;
using PropertyDescriptor = NGWebGal.Editor.Services.PropertyReflection.PropertyDescriptor;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for boolean properties
/// </summary>
public partial class BoolPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private INotifyPropertyChanged? _observableTarget;
    private bool _isUpdating = false;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;

    public event EventHandler? ValueChanged;

    public BoolPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        InitializeComponent();

        // Configure checkbox with property name as label
        ValueCheckBox.Content = descriptor.Name;

        // Apply read-only styling
        if (descriptor.IsReadOnly)
        {
            ValueCheckBox.IsEnabled = false;
            ValueCheckBox.Opacity = 0.7;
        }

        // Subscribe to changes
        ValueCheckBox.PropertyChanged += OnCheckBoxPropertyChanged;
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
            var currentValue = Descriptor.Getter(_target);
            if (currentValue is bool boolValue)
            {
                ValueCheckBox.IsChecked = boolValue;
            }
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
        ValueCheckBox.IsChecked = false;
    }

    private void OnCheckBoxPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        // Only react to IsChecked property changes
        if (e.Property.Name != nameof(ValueCheckBox.IsChecked))
            return;

        if (_isUpdating || _target == null || Descriptor.Setter == null)
            return;

        // Update target object
        var newValue = ValueCheckBox.IsChecked ?? false;
        Descriptor.Setter(_target, newValue);

        // Raise ValueChanged event
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
