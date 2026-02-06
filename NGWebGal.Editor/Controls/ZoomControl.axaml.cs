using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NGWebGal.Editor.Controls;

/// <summary>
/// Zoom control with slider for adjusting zoom level (10% to 500%)
/// </summary>
public partial class ZoomControl : UserControl
{
    /// <summary>
    /// Defines the ZoomLevel property
    /// </summary>
    public static readonly StyledProperty<double> ZoomLevelProperty =
        AvaloniaProperty.Register<ZoomControl, double>(
            nameof(ZoomLevel),
            defaultValue: 1.0,
            coerce: CoerceZoomLevel);

    /// <summary>
    /// Gets or sets the zoom level (0.1 to 5.0, where 1.0 = 100%)
    /// </summary>
    public double ZoomLevel
    {
        get => GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public ZoomControl()
    {
        InitializeComponent();

        // Wire up slider value changes
        ZoomSlider.PropertyChanged += OnSliderValueChanged;

        // Wire up reset button
        ResetButton.Click += OnResetButtonClick;

        // Initialize display
        UpdatePercentageDisplay();
    }

    private static double CoerceZoomLevel(AvaloniaObject sender, double value)
    {
        // Clamp between 0.1 and 5.0
        return Math.Max(0.1, Math.Min(5.0, value));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ZoomLevelProperty)
        {
            // Sync slider with ZoomLevel property
            if (Math.Abs(ZoomSlider.Value - ZoomLevel) > 0.001)
            {
                ZoomSlider.Value = ZoomLevel;
            }

            UpdatePercentageDisplay();
        }
    }

    private void OnSliderValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == nameof(ZoomSlider.Value))
        {
            // Update ZoomLevel when slider changes
            ZoomLevel = ZoomSlider.Value;
        }
    }

    private void OnResetButtonClick(object? sender, RoutedEventArgs e)
    {
        // Reset to 100%
        ZoomLevel = 1.0;
    }

    private void UpdatePercentageDisplay()
    {
        // Convert zoom level to percentage (e.g., 1.0 = 100%, 0.5 = 50%, 2.0 = 200%)
        int percentage = (int)Math.Round(ZoomLevel * 100);
        PercentageText.Text = $"{percentage}%";
    }
}
