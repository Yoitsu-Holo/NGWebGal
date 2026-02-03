using System;
using Avalonia.Controls;
using NGWebGal.Editor.Services.PropertyReflection;
using NGWebGal.Types;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor specialized for TextPadding type
/// </summary>
public partial class PaddingPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private bool _isUpdating = false;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;

    public event EventHandler? ValueChanged;

    public PaddingPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        if (descriptor.PropertyType != typeof(TextPadding))
            throw new ArgumentException("PaddingPropertyEditor requires TextPadding property type", nameof(descriptor));

        InitializeComponent();

        // Configure label
        LabelTextBlock.Text = descriptor.Name + ":";

        // Apply read-only styling
        if (descriptor.IsReadOnly)
        {
            TopNumeric.IsEnabled = false;
            BottomNumeric.IsEnabled = false;
            LeftNumeric.IsEnabled = false;
            RightNumeric.IsEnabled = false;

            TopNumeric.Background = Avalonia.Media.Brushes.LightGray;
            BottomNumeric.Background = Avalonia.Media.Brushes.LightGray;
            LeftNumeric.Background = Avalonia.Media.Brushes.LightGray;
            RightNumeric.Background = Avalonia.Media.Brushes.LightGray;

            TopNumeric.Opacity = 0.7;
            BottomNumeric.Opacity = 0.7;
            LeftNumeric.Opacity = 0.7;
            RightNumeric.Opacity = 0.7;
        }

        // Subscribe to changes
        TopNumeric.ValueChanged += OnNumericValueChanged;
        BottomNumeric.ValueChanged += OnNumericValueChanged;
        LeftNumeric.ValueChanged += OnNumericValueChanged;
        RightNumeric.ValueChanged += OnNumericValueChanged;
    }

    public void Bind(object target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));

        // Load current value
        _isUpdating = true;
        try
        {
            var currentValue = Descriptor.Getter(_target);
            if (currentValue is TextPadding padding)
            {
                TopNumeric.Value = padding.Top;
                BottomNumeric.Value = padding.Bottom;
                LeftNumeric.Value = padding.Left;
                RightNumeric.Value = padding.Right;
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    public void Unbind()
    {
        _target = null;
        _isUpdating = true;
        try
        {
            TopNumeric.Value = 0;
            BottomNumeric.Value = 0;
            LeftNumeric.Value = 0;
            RightNumeric.Value = 0;
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnNumericValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdating || _target == null || Descriptor.Setter == null)
            return;

        // Create new TextPadding with updated values
        var newPadding = new TextPadding(
            Top: (int)(TopNumeric.Value ?? 0),
            Bottom: (int)(BottomNumeric.Value ?? 0),
            Left: (int)(LeftNumeric.Value ?? 0),
            Right: (int)(RightNumeric.Value ?? 0)
        );

        // Update target object
        Descriptor.Setter(_target, newPadding);

        // Raise ValueChanged event
        ValueChanged?.Invoke(this, EventArgs.Empty);
    }
}
