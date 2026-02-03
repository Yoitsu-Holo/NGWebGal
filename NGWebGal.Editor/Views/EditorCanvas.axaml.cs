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
        // EditorCanvas should take all available space
        return availableSize;
    }

    /// <summary>
    /// Arranges the control's final size.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
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

            // Subscribe to new collection
            if (change.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnWidgetsCollectionChanged;
            }

            InvalidateVisual();
        }
        else if (change.Property == SelectedWidgetProperty)
        {
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Handles collection changed events to trigger re-render when widgets are added/removed.
    /// </summary>
    private void OnWidgetsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Console.WriteLine($"DEBUG [OnWidgetsCollectionChanged]: Collection changed. Action: {e.Action}");
        if (e.NewItems != null)
        {
            Console.WriteLine($"DEBUG [OnWidgetsCollectionChanged]: Added {e.NewItems.Count} items");
        }
        InvalidateVisual();
        Console.WriteLine($"DEBUG [OnWidgetsCollectionChanged]: InvalidateVisual called");
    }

    /// <summary>
    /// Renders the canvas and all widgets.
    /// </summary>
    /// <param name="context">The drawing context to render to.</param>
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Debug: Draw canvas background explicitly
        context.DrawRectangle(new SolidColorBrush(Color.FromRgb(245, 245, 245)), null, new Rect(0, 0, Bounds.Width, Bounds.Height));

        if (Widgets == null)
        {
            Console.WriteLine("DEBUG: Widgets is null");
            return;
        }

        // Sort widgets by ZIndex for proper layering
        var sortedWidgets = Widgets.OrderBy(w => w.ZIndex).ToList();

        Console.WriteLine($"DEBUG: Rendering {sortedWidgets.Count} widgets");

        foreach (var widget in sortedWidgets)
        {
            DrawWidget(context, widget);
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

        Console.WriteLine($"DEBUG: Drawing {widget.Type} at ({widget.X}, {widget.Y}) size ({widget.Width}, {widget.Height})");

        // Get fill color based on widget type
        var fillBrush = GetBrushForWidgetType(widget.Type);

        Console.WriteLine($"DEBUG: Color for {widget.Type}: {((SolidColorBrush)fillBrush).Color}");

        // Determine border: selected widgets get thicker blue border
        var isSelected = widget == SelectedWidget;
        var borderBrush = isSelected ? Brushes.Blue : Brushes.Black;
        var borderPen = new Pen(borderBrush, isSelected ? 3 : 2);

        // Draw filled rectangle
        context.DrawRectangle(fillBrush, borderPen, rect);

        // Draw type and name labels with better visibility
        var typeface = new Typeface("Inter");
        var fontSize = 12;
        var foreground = Brushes.Black;

        var typeText = new FormattedText(
            widget.Type.ToString(),
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize,
            foreground);

        var nameText = new FormattedText(
            widget.Name,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            fontSize - 1,
            foreground);

        // Position labels inside the widget
        var typePosition = new Point(widget.X + 4, widget.Y + 4);
        var namePosition = new Point(widget.X + 4, widget.Y + 4 + fontSize + 2);

        // Draw semi-transparent white background for text (better readability)
        var typeBgRect = new Rect(typePosition.X - 2, typePosition.Y - 1,
                                   typeText.Width + 4, typeText.Height + 2);
        var nameBgRect = new Rect(namePosition.X - 2, namePosition.Y - 1,
                                   nameText.Width + 4, nameText.Height + 2);

        var textBackgroundBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
        context.DrawRectangle(textBackgroundBrush, null, typeBgRect);
        context.DrawRectangle(textBackgroundBrush, null, nameBgRect);

        context.DrawText(typeText, typePosition);
        context.DrawText(nameText, namePosition);
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
        return new SolidColorBrush(Color.FromArgb(128, color.R, color.G, color.B));
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

        // Clamp to canvas bounds (prevent negative positions)
        _draggedWidget.X = Math.Max(0, _draggedWidget.X);
        _draggedWidget.Y = Math.Max(0, _draggedWidget.Y);

        InvalidateVisual();
        e.Handled = true;
    }

    /// <summary>
    /// Handles pointer released events to end dragging.
    /// </summary>
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _draggedWidget = null;
        e.Handled = true;
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
        Console.WriteLine("DEBUG [OnDrop]: Drop event triggered");

        #pragma warning disable CS0618 // Type or member is obsolete
        if (!e.Data.Contains("WidgetType"))
        {
            Console.WriteLine("DEBUG [OnDrop]: No WidgetType in drag data");
            return;
        }

        // Extract widget type from drag data
        var widgetType = (WidgetType)e.Data.Get("WidgetType")!;
        #pragma warning restore CS0618 // Type or member is obsolete

        // Get drop position
        var dropPosition = e.GetPosition(this);

        Console.WriteLine($"DEBUG [OnDrop]: Dropping {widgetType} at ({dropPosition.X}, {dropPosition.Y})");
        Console.WriteLine($"DEBUG [OnDrop]: AddWidgetCommand is {(AddWidgetCommand == null ? "NULL" : "not null")}");

        // Execute AddWidgetCommand with type and position
        if (AddWidgetCommand?.CanExecute((widgetType, dropPosition)) == true)
        {
            Console.WriteLine($"DEBUG [OnDrop]: Executing AddWidgetCommand");
            AddWidgetCommand.Execute((widgetType, dropPosition));
            Console.WriteLine($"DEBUG [OnDrop]: Command executed");
        }
        else
        {
            Console.WriteLine($"DEBUG [OnDrop]: Command CanExecute returned false or command is null");
        }

        e.Handled = true;
    }
}
