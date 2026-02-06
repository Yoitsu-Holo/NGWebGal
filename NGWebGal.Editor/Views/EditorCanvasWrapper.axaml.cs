using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using NGWebGal.Editor.ViewModels;

namespace NGWebGal.Editor.Views;

/// <summary>
/// Scrollable wrapper for EditorCanvas with zoom transform support.
/// Provides viewport management and dynamic scrollbar sizing based on zoom level.
/// </summary>
public partial class EditorCanvasWrapper : UserControl
{
    #region Avalonia Properties

    /// <summary>
    /// Defines the CanvasWidth property for the canvas width.
    /// </summary>
    public static readonly StyledProperty<double> CanvasWidthProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, double>(nameof(CanvasWidth), 1280.0);

    /// <summary>
    /// Gets or sets the canvas width.
    /// </summary>
    public double CanvasWidth
    {
        get => GetValue(CanvasWidthProperty);
        set => SetValue(CanvasWidthProperty, value);
    }

    /// <summary>
    /// Defines the CanvasHeight property for the canvas height.
    /// </summary>
    public static readonly StyledProperty<double> CanvasHeightProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, double>(nameof(CanvasHeight), 720.0);

    /// <summary>
    /// Gets or sets the canvas height.
    /// </summary>
    public double CanvasHeight
    {
        get => GetValue(CanvasHeightProperty);
        set => SetValue(CanvasHeightProperty, value);
    }

    /// <summary>
    /// Defines the ZoomLevel property for the zoom scale factor.
    /// </summary>
    public static readonly StyledProperty<double> ZoomLevelProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, double>(nameof(ZoomLevel), 1.0);

    /// <summary>
    /// Gets or sets the zoom level (scale factor).
    /// </summary>
    public double ZoomLevel
    {
        get => GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    /// <summary>
    /// Defines the ShowPercentageLines property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowPercentageLinesProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, bool>(nameof(ShowPercentageLines), true);

    /// <summary>
    /// Gets or sets whether to show percentage guide lines.
    /// </summary>
    public bool ShowPercentageLines
    {
        get => GetValue(ShowPercentageLinesProperty);
        set => SetValue(ShowPercentageLinesProperty, value);
    }

    /// <summary>
    /// Defines the ShowGridLines property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowGridLinesProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, bool>(nameof(ShowGridLines), false);

    /// <summary>
    /// Gets or sets whether to show grid lines.
    /// </summary>
    public bool ShowGridLines
    {
        get => GetValue(ShowGridLinesProperty);
        set => SetValue(ShowGridLinesProperty, value);
    }

    /// <summary>
    /// Defines the GridSpacing property.
    /// </summary>
    public static readonly StyledProperty<int> GridSpacingProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, int>(nameof(GridSpacing), 50);

    /// <summary>
    /// Gets or sets the grid spacing in pixels.
    /// </summary>
    public int GridSpacing
    {
        get => GetValue(GridSpacingProperty);
        set => SetValue(GridSpacingProperty, value);
    }

    /// <summary>
    /// Defines the Widgets property forwarded to EditorCanvas.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<WidgetViewModel>?> WidgetsProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, IEnumerable<WidgetViewModel>?>(nameof(Widgets));

    /// <summary>
    /// Gets or sets the collection of widgets to display.
    /// </summary>
    public IEnumerable<WidgetViewModel>? Widgets
    {
        get => GetValue(WidgetsProperty);
        set => SetValue(WidgetsProperty, value);
    }

    /// <summary>
    /// Defines the SelectedWidget property forwarded to EditorCanvas.
    /// </summary>
    public static readonly StyledProperty<WidgetViewModel?> SelectedWidgetProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, WidgetViewModel?>(nameof(SelectedWidget));

    /// <summary>
    /// Gets or sets the currently selected widget.
    /// </summary>
    public WidgetViewModel? SelectedWidget
    {
        get => GetValue(SelectedWidgetProperty);
        set => SetValue(SelectedWidgetProperty, value);
    }

    /// <summary>
    /// Defines the AddWidgetCommand property forwarded to EditorCanvas.
    /// </summary>
    public static readonly StyledProperty<ICommand?> AddWidgetCommandProperty =
        AvaloniaProperty.Register<EditorCanvasWrapper, ICommand?>(nameof(AddWidgetCommand));

    /// <summary>
    /// Gets or sets the command to execute when adding a widget.
    /// </summary>
    public ICommand? AddWidgetCommand
    {
        get => GetValue(AddWidgetCommandProperty);
        set => SetValue(AddWidgetCommandProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorCanvasWrapper"/> class.
    /// </summary>
    public EditorCanvasWrapper()
    {
        InitializeComponent();

        // Subscribe to mouse wheel events using tunneling (preview) to intercept before ScrollViewer
        AddHandler(PointerWheelChangedEvent, OnPointerWheelChanged, handledEventsToo: true);
    }

    /// <summary>
    /// Called when a property value changes.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // Update scrollbar extent when zoom or canvas size changes
        if (change.Property == ZoomLevelProperty ||
            change.Property == CanvasWidthProperty ||
            change.Property == CanvasHeightProperty)
        {
            UpdateScrollViewerExtent();
        }
    }

    /// <summary>
    /// Updates the ScrollViewer extent based on canvas size and zoom level.
    /// This ensures scrollbars reflect the actual zoomed canvas dimensions.
    /// </summary>
    private void UpdateScrollViewerExtent()
    {
        // The Border with ScaleTransform handles the visual zoom
        // ScrollViewer automatically adjusts based on the transformed content size
        // No manual extent calculation needed - Avalonia handles this automatically
    }

    /// <summary>
    /// Handles mouse wheel events for scrolling and zooming.
    /// - Normal scroll: Vertical scrolling (up/down)
    /// - Shift + scroll: Horizontal scrolling (left/right)
    /// - Ctrl + scroll: Zoom in/out
    /// </summary>
    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");
        if (scrollViewer == null)
            return;

        var delta = e.Delta.Y;
        var keyModifiers = e.KeyModifiers;

        // Ctrl + Scroll: Zoom in/out
        if (keyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true; // Prevent default scroll behavior FIRST
            var zoomDelta = delta * 0.1;
            var newZoom = Math.Clamp(ZoomLevel + zoomDelta, 0.1, 5.0);
            ZoomLevel = newZoom;
        }
        // Shift + Scroll: Horizontal scrolling
        else if (keyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true; // Prevent default scroll behavior FIRST
            var scrollDelta = delta * 50; // Adjust scroll speed
            var newOffset = scrollViewer.Offset.X - scrollDelta;
            scrollViewer.Offset = new Vector(newOffset, scrollViewer.Offset.Y);
        }
        // Normal Scroll: Vertical scrolling (default behavior, but we handle it explicitly)
        else
        {
            e.Handled = true; // Prevent default scroll behavior FIRST
            var scrollDelta = delta * 50; // Adjust scroll speed
            var newOffset = scrollViewer.Offset.Y - scrollDelta;
            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newOffset);
        }
    }
}
