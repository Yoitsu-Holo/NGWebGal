using System;
using Avalonia.Controls;
using NGWebGal.Editor.Services.PropertyReflection;
using NGWebGal.Types;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for IVector and FVector types
/// </summary>
public partial class VectorPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private bool _isUpdating;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;
    public string PropertyName => Descriptor.Name;

    public event EventHandler? ValueChanged;

    public VectorPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        InitializeComponent();
        DataContext = this;

        // Set property name label
        var nameLabel = this.FindControl<TextBlock>("PropertyNameLabel");
        if (nameLabel != null)
        {
            nameLabel.Text = descriptor.Name + ":";
        }

        // Hook up value changed events
        var xNumeric = this.FindControl<NumericUpDown>("XNumeric");
        var yNumeric = this.FindControl<NumericUpDown>("YNumeric");

        if (xNumeric != null)
            xNumeric.ValueChanged += OnXValueChanged;
        if (yNumeric != null)
            yNumeric.ValueChanged += OnYValueChanged;
    }

    public void Bind(object target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));
        UpdateFromTarget();
    }

    public void Unbind()
    {
        _target = null;
    }

    private void UpdateFromTarget()
    {
        if (_target == null || _isUpdating)
            return;

        _isUpdating = true;
        try
        {
            var value = Descriptor.Getter(_target);
            var xNumeric = this.FindControl<NumericUpDown>("XNumeric");
            var yNumeric = this.FindControl<NumericUpDown>("YNumeric");

            if (value is IVector iVec)
            {
                if (xNumeric != null) xNumeric.Value = iVec.X;
                if (yNumeric != null) yNumeric.Value = iVec.Y;
            }
            else if (value is FVector fVec)
            {
                if (xNumeric != null) xNumeric.Value = (decimal)fVec.X;
                if (yNumeric != null) yNumeric.Value = (decimal)fVec.Y;
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnXValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_target == null || _isUpdating || Descriptor.Setter == null)
            return;

        UpdateTarget();
    }

    private void OnYValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_target == null || _isUpdating || Descriptor.Setter == null)
            return;

        UpdateTarget();
    }

    private void UpdateTarget()
    {
        if (_target == null || _isUpdating || Descriptor.Setter == null)
            return;

        _isUpdating = true;
        try
        {
            var xNumeric = this.FindControl<NumericUpDown>("XNumeric");
            var yNumeric = this.FindControl<NumericUpDown>("YNumeric");

            if (xNumeric?.Value == null || yNumeric?.Value == null)
                return;

            var currentValue = Descriptor.Getter(_target);

            if (currentValue is IVector)
            {
                var newVector = new IVector
                {
                    X = (int)xNumeric.Value.Value,
                    Y = (int)yNumeric.Value.Value
                };
                System.Diagnostics.Debug.WriteLine($"[VectorPropertyEditor] Setting {Descriptor.Name} = ({newVector.X}, {newVector.Y})");
                Descriptor.Setter(_target, newVector);
                System.Diagnostics.Debug.WriteLine($"[VectorPropertyEditor] Setter called successfully");
            }
            else if (currentValue is FVector)
            {
                var newVector = new FVector
                {
                    X = (float)xNumeric.Value.Value,
                    Y = (float)yNumeric.Value.Value
                };
                System.Diagnostics.Debug.WriteLine($"[VectorPropertyEditor] Setting {Descriptor.Name} = ({newVector.X}, {newVector.Y})");
                Descriptor.Setter(_target, newVector);
                System.Diagnostics.Debug.WriteLine($"[VectorPropertyEditor] Setter called successfully");
            }

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isUpdating = false;
        }
    }
}
