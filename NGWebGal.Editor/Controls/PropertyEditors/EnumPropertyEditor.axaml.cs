using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using PropertyDescriptor = NGWebGal.Editor.Services.PropertyReflection.PropertyDescriptor;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for enum properties
/// </summary>
public partial class EnumPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private INotifyPropertyChanged? _observableTarget;
    private bool _isUpdating = false;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;

    public event EventHandler? ValueChanged;

    public EnumPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        if (!descriptor.PropertyType.IsEnum)
            throw new ArgumentException("EnumPropertyEditor requires an enum property type", nameof(descriptor));

        InitializeComponent();

        // Configure label
        LabelTextBlock.Text = descriptor.Name + ":";

        // Populate ComboBox with enum values
        var enumValues = Enum.GetValues(descriptor.PropertyType);
        ValueComboBox.ItemsSource = enumValues;

        // Apply read-only styling
        if (descriptor.IsReadOnly)
        {
            ValueComboBox.IsEnabled = false;
            ValueComboBox.Background = Avalonia.Media.Brushes.LightGray;
            ValueComboBox.Opacity = 0.7;
        }

        // Subscribe to changes
        ValueComboBox.SelectionChanged += OnComboBoxSelectionChanged;
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
            ValueComboBox.SelectedItem = currentValue;
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
        ValueComboBox.SelectedIndex = -1;
    }

    private void OnComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating || _target == null || Descriptor.Setter == null)
            return;

        if (ValueComboBox.SelectedItem == null)
            return;

        // Update target object
        Descriptor.Setter(_target, ValueComboBox.SelectedItem);

        // Raise ValueChanged event
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
