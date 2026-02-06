using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using NGWebGal.Editor.Models;
using NGWebGal.Editor.ViewModels;

namespace NGWebGal.Editor.Views;

/// <summary>
/// Custom canvas control for displaying and editing widgets.
/// Provides visual rendering, selection, dragging, and drag-drop support.
/// </summary>
public class EditorCanvas : Control
{
    private WidgetViewModel? _draggedWidget;
    private Point _dragStartPosition;
    private Point _dragStartWidgetPosition;
    private bool _isRendering = false; // Prevent re-entrant rendering

    #region Constants
    // Widget rendering constants
    private const int TypeLabelFontSize = 12;
    private const int NameLabelFontSize = 11;
    private const double SelectedBorderThickness = 3.0;
    private const double DefaultBorderThickness = 2.0;
    private const byte TextBackgroundAlpha = 180; // ~70% opacity for readability
    private const byte WidgetFillAlpha = 128; // 50% transparency
    private const double LabelPaddingX = 4.0;
    private const double LabelPaddingY = 4.0;
    private const double LabelVerticalSpacing = 2.0;
    private const int DefaultWidgetSize = 100;
    private static readonly Color CanvasBackgroundColor = Color.FromRgb(250, 250, 250);
    private static readonly Color CanvasBorderColor = Color.FromRgb(204, 204, 204);
    private const double CanvasBorderThickness = 2.0;
    #endregion

    #region Avalonia Properties

    /// <summary>
    /// Defines the Widgets attached property for the collection of widgets to render.
    /// </summary>
    public static readonly StyledProperty<IEnumerable<WidgetViewModel>?> WidgetsProperty =
        AvaloniaProperty.Register<EditorCanvas, IEnumerable<WidgetViewModel>?>(nameof(Widgets));

    /// <summary>
    /// Gets or sets the collection of widgets to display on the canvas.
    /// </summary>
    public IEnumerable<WidgetViewModel>? Widgets
    {
        get => GetValue(WidgetsProperty);
        set => SetValue(WidgetsProperty, value);
    }

    /// <summary>
    /// Defines the SelectedWidget attached property for the currently selected widget.
    /// </summary>
    public static readonly StyledProperty<WidgetViewModel?> SelectedWidgetProperty =
        AvaloniaProperty.Register<EditorCanvas, WidgetViewModel?>(nameof(SelectedWidget));

    /// <summary>
    /// Gets or sets the currently selected widget.
    /// </summary>
    public WidgetViewModel? SelectedWidget
    {
        get => GetValue(SelectedWidgetProperty);
        set => SetValue(SelectedWidgetProperty, value);
    }

    /// <summary>
    /// Defines the AddWidgetCommand attached property for adding new widgets.
    /// </summary>
    public static readonly StyledProperty<ICommand?> AddWidgetCommandProperty =
        AvaloniaProperty.Register<EditorCanvas, ICommand?>(nameof(AddWidgetCommand));

    /// <summary>
    /// Gets or sets the command to execute when adding a widget via drag-drop.
    /// Command parameter: Tuple of (WidgetType, Point position)
    /// </summary>
    public ICommand? AddWidgetCommand
    {
        get => GetValue(AddWidgetCommandProperty);
        set => SetValue(AddWidgetCommandProperty, value);
    }

    /// <summary>
    /// Defines the ShowPercentageLines property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowPercentageLinesProperty =
        AvaloniaProperty.Register<EditorCanvas, bool>(nameof(ShowPercentageLines), true);

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
        AvaloniaProperty.Register<EditorCanvas, bool>(nameof(ShowGridLines), false);

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
        AvaloniaProperty.Register<EditorCanvas, int>(nameof(GridSpacing), 50);

    /// <summary>
    /// Gets or sets the grid spacing in pixels.
    /// </summary>
    public int GridSpacing
    {
        get => GetValue(GridSpacingProperty);
        set => SetValue(GridSpacingProperty, value);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="EditorCanvas"/> class.
    /// </summary>
    public EditorCanvas()
    {
        // Enable drag-drop from toolbox
        DragDrop.SetAllowDrop(this, true);

        // Subscribe to pointer events for selection and dragging
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;

        // Subscribe to drag-drop events
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
    }

    /// <summary>
    /// Measures the control's desired size.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        // Use explicit Width/Height if set, otherwise take available space
        double width = !double.IsNaN(Width) ? Width : availableSize.Width;
        double height = !double.IsNaN(Height) ? Height : availableSize.Height;
        return new Size(width, height);
    }

    /// <summary>
    /// Arranges the control's final size.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        // Use explicit Width/Height if set, otherwise use finalSize
        double width = !double.IsNaN(Width) ? Width : finalSize.Width;
        double height = !double.IsNaN(Height) ? Height : finalSize.Height;
        return new Size(width, height);
    }

    /// <summary>
    /// Called when a property value changes. Triggers re-render when Widgets or SelectedWidget changes.
    /// </summary>
    /// <param name="change">The property change event arguments.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WidgetsProperty)
        {
            // Unsubscribe from old collection
            if (change.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnWidgetsCollectionChanged;
            }

            // Unsubscribe from old widgets' property changes
            if (change.OldValue is IEnumerable<WidgetViewModel> oldWidgets)
            {
                foreach (var widget in oldWidgets)
                {
                    widget.PropertyChanged -= OnWidgetPropertyChanged;
                }
            }

            // Subscribe to new collection
            if (change.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnWidgetsCollectionChanged;
            }

            // Subscribe to new widgets' property changes
            if (change.NewValue is IEnumerable<WidgetViewModel> newWidgets)
            {
                foreach (var widget in newWidgets)
                {
                    widget.PropertyChanged += OnWidgetPropertyChanged;
                }
            }

            InvalidateVisual();
        }
        else if (change.Property == SelectedWidgetProperty)
        {
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Handles property changes on individual widgets to trigger re-render
    /// </summary>
    private void OnWidgetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[EditorCanvas] Widget property changed: {e.PropertyName}");

        // Force re-render for any property change, especially BitmapVersion
        // BitmapVersion changes whenever InvalidateBitmap() is called
        InvalidateVisual();
    }

    /// <summary>
    /// Handles collection changed events to trigger re-render when widgets are added/removed.
    /// </summary>
    private void OnWidgetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Subscribe to PropertyChanged for newly added widgets
        if (e.NewItems != null)
        {
            foreach (WidgetViewModel widget in e.NewItems)
            {
                widget.PropertyChanged += OnWidgetPropertyChanged;
                System.Diagnostics.Debug.WriteLine($"[EditorCanvas] Subscribed to PropertyChanged for newly added widget: {widget.Name}");
            }
        }

        // Unsubscribe from removed widgets to prevent memory leaks
        if (e.OldItems != null)
        {
            foreach (WidgetViewModel widget in e.OldItems)
            {
                widget.PropertyChanged -= OnWidgetPropertyChanged;
                System.Diagnostics.Debug.WriteLine($"[EditorCanvas] Unsubscribed from PropertyChanged for removed widget: {widget.Name}");
            }
        }

        InvalidateVisual();
    }

    /// <summary>
    /// Renders the canvas and all widgets.
    /// </summary>
    /// <param name="context">The drawing context to render to.</param>
    public override void Render(DrawingContext context)
    {
        // Prevent re-entrant rendering which causes infinite loop
        if (_isRendering)
        {
            return;
        }

        _isRendering = true;
        try
        {
            base.Render(context);

            // Draw canvas background with border
            var canvasRect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            var backgroundBrush = new SolidColorBrush(CanvasBackgroundColor);
            var borderPen = new Pen(new SolidColorBrush(CanvasBorderColor), CanvasBorderThickness);
            context.DrawRectangle(backgroundBrush, borderPen, canvasRect);

            // Render guide lines (before widgets)
            var canvasSize = new Size(Bounds.Width, Bounds.Height);

            if (ShowGridLines)
            {
                GuideLineRenderer.RenderGridLines(context, canvasSize, GridSpacing);
            }

            if (ShowPercentageLines)
            {
                var percentages = new List<double> { 0.1, 0.25, 0.5, 0.75, 0.9 };
                GuideLineRenderer.RenderPercentageLines(context, canvasSize, percentages);
            }

            if (Widgets == null)
            {
                return;
            }

            // Sort widgets by ZIndex for proper layering
            var sortedWidgets = Widgets.OrderBy(w => w.ZIndex).ToList();

            // Render ALL widgets using traditional Avalonia rendering
            // CoreLayer bitmap caching is handled in DrawWidget
            foreach (var widget in sortedWidgets)
            {
                DrawWidget(context, widget);
            }

            // Render dynamic crosshair when dragging a widget
            if (_draggedWidget != null)
            {
                var center = new Point(
                    _draggedWidget.X + _draggedWidget.Width / 2.0,
                    _draggedWidget.Y + _draggedWidget.Height / 2.0
                );
                GuideLineRenderer.RenderDynamicCrosshair(context, center, canvasSize);
            }
        }
        finally
        {
            _isRendering = false;
        }
    }

    /// <summary>
    /// Draws a single widget on the canvas.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="widget">The widget to draw.</param>
    private void DrawWidget(DrawingContext context, WidgetViewModel widget)
    {
        var rect = new Rect(widget.X, widget.Y, widget.Width, widget.Height);

        // Try to get cached bitmap from CoreLayer first
        var cachedBitmap = widget.GetCachedBitmap();

        if (cachedBitmap != null)
        {
            // Render using the cached CoreLayer bitmap
            using var bitmap = new Avalonia.Media.Imaging.Bitmap(
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul,
                cachedBitmap.GetPixels(),
                new Avalonia.PixelSize(cachedBitmap.Width, cachedBitmap.Height),
                new Avalonia.Vector(96, 96),
                cachedBitmap.RowBytes);

            context.DrawImage(bitmap, rect);

            // Draw selection border if needed
            if (widget == SelectedWidget)
            {
                var borderPen = new Pen(Brushes.Blue, SelectedBorderThickness);
                context.DrawRectangle(null, borderPen, rect);
            }
        }
        else
        {
            // Fall back to traditional rendering (color blocks)
            var fillBrush = GetBrushForWidgetType(widget.Type);

            // Determine border: selected widgets get thicker blue border
            var isSelected = widget == SelectedWidget;
            var borderBrush = isSelected ? Brushes.Blue : Brushes.Black;
            var borderPen = new Pen(borderBrush, isSelected ? SelectedBorderThickness : DefaultBorderThickness);

            // Draw filled rectangle
            context.DrawRectangle(fillBrush, borderPen, rect);

            // Draw type and name labels with better visibility
            var typeface = new Typeface("Inter");
            var foreground = Brushes.Black;

            var typeText = new FormattedText(
                widget.Type.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                TypeLabelFontSize,
                foreground);

            var nameText = new FormattedText(
                widget.Name,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                NameLabelFontSize,
                foreground);

            // Position labels inside the widget
            var typePosition = new Point(widget.X + LabelPaddingX, widget.Y + LabelPaddingY);
            var namePosition = new Point(widget.X + LabelPaddingX, widget.Y + LabelPaddingY + TypeLabelFontSize + LabelVerticalSpacing);

            // Draw semi-transparent white background for text (better readability)
            var typeBgRect = new Rect(typePosition.X - 2, typePosition.Y - 1,
                                       typeText.Width + 4, typeText.Height + 2);
            var nameBgRect = new Rect(namePosition.X - 2, namePosition.Y - 1,
                                       nameText.Width + 4, nameText.Height + 2);

            var textBackgroundBrush = new SolidColorBrush(Color.FromArgb(TextBackgroundAlpha, 255, 255, 255));
            context.DrawRectangle(textBackgroundBrush, null, typeBgRect);
            context.DrawRectangle(textBackgroundBrush, null, nameBgRect);

            context.DrawText(typeText, typePosition);
            context.DrawText(nameText, namePosition);
        }
    }

    /// <summary>
    /// Gets the fill brush color for a widget type.
    /// Uses the same color mapping as WidgetTypeToBrushConverter.
    /// </summary>
    /// <param name="widgetType">The widget type.</param>
    /// <returns>A semi-transparent brush (50% opacity) for the widget type.</returns>
    private IBrush GetBrushForWidgetType(WidgetType widgetType)
    {
        var color = widgetType switch
        {
            WidgetType.ImageBox => Colors.LightBlue,
            WidgetType.TextBox => Colors.LightGreen,
            WidgetType.ColorBox => Colors.LightCoral,
            WidgetType.ProgressBar => Colors.LightGoldenrodYellow,
            WidgetType.Button => Colors.LightSalmon,
            WidgetType.Toggle => Colors.LightSeaGreen,
            WidgetType.Checkbox => Colors.LightSkyBlue,
            WidgetType.SliderHorizontal => Colors.LightSteelBlue,
            WidgetType.SliderVertical => Colors.LightSteelBlue,
            WidgetType.InputField => Colors.LightYellow,
            _ => Colors.Gray
        };

        // 50% transparency (Alpha = 128) for color blending effect
        return new SolidColorBrush(Color.FromArgb(WidgetFillAlpha, color.R, color.G, color.B));
    }

    /// <summary>
    /// Handles pointer pressed events for widget selection and drag initiation.
    /// </summary>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Widgets == null)
        {
            return;
        }

        var position = e.GetPosition(this);

        // Hit-test to find widget at click position (check in reverse ZIndex order)
        var clickedWidget = Widgets
            .OrderByDescending(w => w.ZIndex)
            .FirstOrDefault(w => HitTest(w, position));

        if (clickedWidget != null)
        {
            // Select the widget
            SelectedWidget = clickedWidget;

            // Initiate drag
            _draggedWidget = clickedWidget;
            _dragStartPosition = position;
            _dragStartWidgetPosition = new Point(clickedWidget.X, clickedWidget.Y);

            e.Handled = true;
        }
        else
        {
            // Clicked on empty space, deselect
            SelectedWidget = null;
        }
    }

    /// <summary>
    /// Handles pointer moved events for widget dragging.
    /// </summary>
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_draggedWidget == null)
        {
            return;
        }

        var currentPosition = e.GetPosition(this);
        var delta = currentPosition - _dragStartPosition;

        // Update widget position
        _draggedWidget.X = (int)(_dragStartWidgetPosition.X + delta.X);
        _draggedWidget.Y = (int)(_dragStartWidgetPosition.Y + delta.Y);

        // Apply snap-to-grid
        var canvasSize = new Size(Bounds.Width, Bounds.Height);
        var widgetCenter = new Point(_draggedWidget.X + _draggedWidget.Width / 2.0, _draggedWidget.Y + _draggedWidget.Height / 2.0);
        var snappedCenter = Services.SnapToGridService.SnapPosition(widgetCenter, canvasSize, 10);

        // Update widget position based on snapped center
        _draggedWidget.X = (int)(snappedCenter.X - _draggedWidget.Width / 2.0);
        _draggedWidget.Y = (int)(snappedCenter.Y - _draggedWidget.Height / 2.0);

        // Clamp to canvas bounds (prevent negative positions)
        _draggedWidget.X = Math.Max(0, _draggedWidget.X);
        _draggedWidget.Y = Math.Max(0, _draggedWidget.Y);

        // Clamp to canvas bounds (prevent going beyond right/bottom edges)
        int maxX = (int)(Bounds.Width - _draggedWidget.Width);
        int maxY = (int)(Bounds.Height - _draggedWidget.Height);
        _draggedWidget.X = Math.Min(_draggedWidget.X, Math.Max(0, maxX));
        _draggedWidget.Y = Math.Min(_draggedWidget.Y, Math.Max(0, maxY));

        InvalidateVisual();
        e.Handled = true;
    }

    /// <summary>
    /// Handles pointer released events to end dragging.
    /// </summary>
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_draggedWidget != null)
        {
            _draggedWidget = null;
            InvalidateVisual(); // Force re-render to hide crosshair immediately
            e.Handled = true;
        }
    }

    /// <summary>
    /// Performs hit-testing to determine if a point is inside a widget's bounds.
    /// </summary>
    /// <param name="widget">The widget to test.</param>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the point is inside the widget; otherwise, false.</returns>
    private bool HitTest(WidgetViewModel widget, Point point)
    {
        var rect = new Rect(widget.X, widget.Y, widget.Width, widget.Height);
        return rect.Contains(point);
    }

    /// <summary>
    /// Handles drag-over events for drag-drop support from the toolbox.
    /// </summary>
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        // Check if drag data contains a WidgetType
        #pragma warning disable CS0618 // Type or member is obsolete
        if (e.Data.Contains("WidgetType"))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }
        #pragma warning restore CS0618 // Type or member is obsolete

        e.Handled = true;
    }

    /// <summary>
    /// Handles drop events to add a new widget at the drop position.
    /// </summary>
    private void OnDrop(object? sender, DragEventArgs e)
    {
        #pragma warning disable CS0618 // Type or member is obsolete
        if (!e.Data.Contains("WidgetType"))
        {
            return;
        }

        // Extract widget type from drag data
        var widgetTypeObj = e.Data.Get("WidgetType");
        if (widgetTypeObj is not WidgetType widgetType)
        {
            return;
        }
        #pragma warning restore CS0618 // Type or member is obsolete

        // Get drop position
        var dropPosition = e.GetPosition(this);

        // Clamp drop position to canvas bounds (assume min widget size 100x100)
        double clampedX = Math.Max(0, Math.Min(dropPosition.X, Bounds.Width - DefaultWidgetSize));
        double clampedY = Math.Max(0, Math.Min(dropPosition.Y, Bounds.Height - DefaultWidgetSize));
        dropPosition = new Point(clampedX, clampedY);

        // Execute AddWidgetCommand with type and position
        if (AddWidgetCommand?.CanExecute((widgetType, dropPosition)) == true)
        {
            AddWidgetCommand.Execute((widgetType, dropPosition));
        }

        e.Handled = true;
    }
}
