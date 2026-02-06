using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using NGWebGal.Types;
using PropertyDescriptor = NGWebGal.Editor.Services.PropertyReflection.PropertyDescriptor;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for IVector and FVector types
/// </summary>
public partial class VectorPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private bool _isUpdating;
    private DispatcherTimer? _updateThrottle;
    private bool _hasPendingUpdate;
    private INotifyPropertyChanged? _observableTarget;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;
    public string PropertyName => Descriptor.Name;

    public event EventHandler? ValueChanged;

    public VectorPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        InitializeComponent();
        DataContext = this;

        // Initialize throttle timer
        _updateThrottle = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateThrottle.Tick += (_, _) =>
        {
            _updateThrottle.Stop();
            if (_hasPendingUpdate)
            {
                _hasPendingUpdate = false;
                ApplyPendingUpdate();
            }
        };

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

        // Subscribe to PropertyChanged if target is observable
        if (target is INotifyPropertyChanged observable)
        {
            _observableTarget = observable;
            _observableTarget.PropertyChanged += OnTargetPropertyChanged;
        }

        UpdateFromTarget();
    }

    public void Unbind()
    {
        if (_observableTarget != null)
        {
            _observableTarget.PropertyChanged -= OnTargetPropertyChanged;
            _observableTarget = null;
        }

        _updateThrottle?.Stop();
        _target = null;
    }

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Only update if the changed property matches our descriptor
        if (e.PropertyName == Descriptor.Name ||
            e.PropertyName == "X" ||
            e.PropertyName == "Y" ||
            e.PropertyName == "Position")
        {
            UpdateFromTarget();
        }
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

        _hasPendingUpdate = true;
        _updateThrottle?.Stop();
        _updateThrottle?.Start();
    }

    private void OnYValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_target == null || _isUpdating || Descriptor.Setter == null)
            return;

        _hasPendingUpdate = true;
        _updateThrottle?.Stop();
        _updateThrottle?.Start();
    }

    private void ApplyPendingUpdate()
    {
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
