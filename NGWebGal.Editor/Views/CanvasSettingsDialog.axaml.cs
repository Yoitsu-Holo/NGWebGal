using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NGWebGal.Editor.Views;

/// <summary>
/// Dialog for configuring canvas dimensions.
/// </summary>
public partial class CanvasSettingsDialog : Window
{
    /// <summary>
    /// Gets the configured canvas width.
    /// </summary>
    public double CanvasWidth { get; private set; }

    /// <summary>
    /// Gets the configured canvas height.
    /// </summary>
    public double CanvasHeight { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasSettingsDialog"/> class.
    /// </summary>
    /// <param name="initialWidth">The initial canvas width.</param>
    /// <param name="initialHeight">The initial canvas height.</param>
    public CanvasSettingsDialog(double initialWidth, double initialHeight)
    {
        InitializeComponent();

        // Set initial values
        CanvasWidth = initialWidth;
        CanvasHeight = initialHeight;

        // Set NumericUpDown values
        WidthNumericUpDown.Value = (decimal)initialWidth;
        HeightNumericUpDown.Value = (decimal)initialHeight;
    }

    /// <summary>
    /// Handles the OK button click event.
    /// </summary>
    private void OnOkButtonClick(object? sender, RoutedEventArgs e)
    {
        // Update properties from controls
        CanvasWidth = (double)(WidthNumericUpDown.Value ?? (decimal)CanvasWidth);
        CanvasHeight = (double)(HeightNumericUpDown.Value ?? (decimal)CanvasHeight);

        // Close dialog with success result
        Close(true);
    }

    /// <summary>
    /// Handles the Cancel button click event.
    /// </summary>
    private void OnCancelButtonClick(object? sender, RoutedEventArgs e)
    {
        // Close dialog without saving
        Close(false);
    }
}
