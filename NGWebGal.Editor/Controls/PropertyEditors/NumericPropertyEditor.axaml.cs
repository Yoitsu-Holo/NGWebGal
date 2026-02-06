using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Threading;
using PropertyDescriptor = NGWebGal.Editor.Services.PropertyReflection.PropertyDescriptor;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Generic property editor for numeric properties (int, float, double)
/// </summary>
/// <typeparam name="T">The numeric type</typeparam>
public partial class NumericPropertyEditor<T> : UserControl, IPropertyEditor where T : struct, IConvertible
{
    private object? _target;
    private readonly TextBlock _labelTextBlock;
    private readonly NumericUpDown _valueNumericUpDown;
    private DispatcherTimer? _updateThrottle;
    private bool _hasPendingUpdate;
    private INotifyPropertyChanged? _observableTarget;
    private bool _isUpdating;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;

    public event EventHandler? ValueChanged;

    public NumericPropertyEditor(PropertyDescriptor descriptor)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

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

        // Create controls manually (Avalonia code generator doesn't support generic types)
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            Margin = new Avalonia.Thickness(1)
        };

        _labelTextBlock = new TextBlock
        {
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            FontSize = 11,
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0x55, 0x55, 0x55)),
            Text = descriptor.Name + ":",
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 6, 0)
        };
        Grid.SetColumn(_labelTextBlock, 0);

        _valueNumericUpDown = new NumericUpDown
        {
            FontSize = 11,
            Minimum = 0,
            Maximum = 10000,
            MinWidth = 60,
            Height = 22,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            ShowButtonSpinner = true,
            ButtonSpinnerLocation = Avalonia.Controls.Location.Right,
            AllowSpin = true
        };
        Grid.SetColumn(_valueNumericUpDown, 1);

        // Configure numeric up-down based on type
        if (typeof(T) == typeof(float) || typeof(T) == typeof(double))
        {
            _valueNumericUpDown.FormatString = "F2";
            _valueNumericUpDown.Increment = 0.1m;
        }
        else
        {
            _valueNumericUpDown.FormatString = "F0";
            _valueNumericUpDown.Increment = 1;
        }

        // Apply read-only styling
        if (descriptor.IsReadOnly)
        {
            _valueNumericUpDown.IsEnabled = false;
            _valueNumericUpDown.Background = Avalonia.Media.Brushes.LightGray;
            _valueNumericUpDown.Opacity = 0.7;
        }

        grid.Children.Add(_labelTextBlock);
        grid.Children.Add(_valueNumericUpDown);

        Content = grid;

        // Subscribe to changes
        _valueNumericUpDown.PropertyChanged += OnNumericUpDownPropertyChanged;
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

        // Load current value
        UpdateFromTarget();
    }

    private void UpdateFromTarget()
    {
        if (_target == null || _isUpdating)
            return;

        _isUpdating = true;
        try
        {
            var currentValue = Descriptor.Getter(_target);
            if (currentValue != null)
            {
                decimal decimalValue = Convert.ToDecimal(currentValue);
                _valueNumericUpDown.Value = decimalValue;
            }
        }
        finally
        {
            _isUpdating = false;
        }
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
        _valueNumericUpDown.Value = 0;
    }

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Only update if the changed property matches our descriptor
        if (e.PropertyName == Descriptor.Name)
        {
            UpdateFromTarget();
        }
    }

    private void OnNumericUpDownPropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        // Only react to Value property changes
        if (e.Property.Name != nameof(_valueNumericUpDown.Value))
            return;

        if (_target == null || Descriptor.Setter == null || _isUpdating)
            return;

        if (_valueNumericUpDown.Value == null)
            return;

        _hasPendingUpdate = true;
        _updateThrottle?.Stop();
        _updateThrottle?.Start();
    }

    private void ApplyPendingUpdate()
    {
        if (_target == null || Descriptor.Setter == null || _isUpdating)
            return;

        if (_valueNumericUpDown.Value == null)
            return;

        _isUpdating = true;
        try
        {
            // Convert decimal back to T
            decimal decimalValue = _valueNumericUpDown.Value.Value;
            T typedValue;

            if (typeof(T) == typeof(int))
            {
                typedValue = (T)(object)(int)decimalValue;
            }
            else if (typeof(T) == typeof(float))
            {
                typedValue = (T)(object)(float)decimalValue;
            }
            else if (typeof(T) == typeof(double))
            {
                typedValue = (T)(object)(double)decimalValue;
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T).Name} not supported");
            }

            System.Diagnostics.Debug.WriteLine($"[NumericPropertyEditor] Setting {Descriptor.Name} = {typedValue}");

            // Update target object
            Descriptor.Setter(_target, typedValue);

            System.Diagnostics.Debug.WriteLine($"[NumericPropertyEditor] Setter called successfully");

            // Raise ValueChanged event
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isUpdating = false;
        }
    }
}
