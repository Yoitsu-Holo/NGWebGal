using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.ViewModels;

/// <summary>
/// Observable wrapper for EditorWidget that provides property change notifications.
/// </summary>
public partial class WidgetViewModel : ObservableObject, IDisposable
{
    private readonly MainViewModel _mainViewModel;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the unique identifier for this widget.
    /// </summary>
    [ObservableProperty]
    private string _id;

    /// <summary>
    /// Gets or sets the type of widget.
    /// </summary>
    [ObservableProperty]
    private WidgetType _type;

    /// <summary>
    /// Gets or sets the display name of the widget.
    /// </summary>
    [ObservableProperty]
    private string _name;

    /// <summary>
    /// Gets or sets the X coordinate position on the canvas.
    /// </summary>
    [ObservableProperty]
    private int _x;

    /// <summary>
    /// Gets or sets the Y coordinate position on the canvas.
    /// </summary>
    [ObservableProperty]
    private int _y;

    /// <summary>
    /// Gets or sets the width of the widget.
    /// </summary>
    [ObservableProperty]
    private int _width;

    /// <summary>
    /// Gets or sets the height of the widget.
    /// </summary>
    [ObservableProperty]
    private int _height;

    /// <summary>
    /// Gets or sets whether the widget is visible.
    /// </summary>
    [ObservableProperty]
    private bool _visible;

    /// <summary>
    /// Gets or sets whether the widget is enabled for interaction.
    /// </summary>
    [ObservableProperty]
    private bool _enable;

    /// <summary>
    /// Gets or sets the text content of the widget.
    /// </summary>
    [ObservableProperty]
    private string _text;

    /// <summary>
    /// Gets or sets the Z-index for layering order.
    /// </summary>
    [ObservableProperty]
    private int _zIndex;

    private readonly PropertyChangedEventHandler? _propertyChangedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="WidgetViewModel"/> class.
    /// </summary>
    /// <param name="widget">The underlying EditorWidget model.</param>
    /// <param name="mainViewModel">Reference to the main view model for dirty state tracking.</param>
    public WidgetViewModel(EditorWidget widget, MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;

        _id = widget.Id;
        _type = widget.Type;
        _name = widget.Name;
        _x = widget.X;
        _y = widget.Y;
        _width = widget.Width;
        _height = widget.Height;
        _visible = widget.Visible;
        _enable = widget.Enable;
        _text = widget.Text;
        _zIndex = widget.ZIndex;

        // Subscribe to property changes to mark layout as dirty
        _propertyChangedHandler = (_, _) => _mainViewModel.MarkDirty();
        PropertyChanged += _propertyChangedHandler;
    }

    /// <summary>
    /// Converts this ViewModel back to an EditorWidget model.
    /// </summary>
    /// <returns>A new EditorWidget instance with current property values.</returns>
    public EditorWidget ToModel()
    {
        return new EditorWidget
        {
            Id = Id,
            Type = Type,
            Name = Name,
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Visible = Visible,
            Enable = Enable,
            Text = Text,
            ZIndex = ZIndex
        };
    }

    /// <summary>
    /// Disposes resources and unsubscribes from events to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            if (_propertyChangedHandler != null)
            {
                PropertyChanged -= _propertyChangedHandler;
            }
            _disposed = true;
        }
    }
}
