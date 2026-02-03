using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NGWebGal.Editor.Models;
using NGWebGal.Layer;
using SkiaSharp;

namespace NGWebGal.Editor.ViewModels;

/// <summary>
/// Observable wrapper for EditorWidget that provides property change notifications.
/// </summary>
public partial class WidgetViewModel : ObservableObject, IDisposable
{
    private readonly MainViewModel _mainViewModel;
    private readonly EditorWidget _model; // Store original EditorWidget instance
    private bool _disposed;

    /// <summary>
    /// Gets or sets the Core Layer instance for rendering.
    /// </summary>
    public ILayer? CoreLayer { get; set; }

    /// <summary>
    /// Cached bitmap rendered from CoreLayer
    /// </summary>
    private SKBitmap? _cachedBitmap;
    private bool _bitmapDirty = true;

    /// <summary>
    /// Dummy property to trigger PropertyChanged notifications for bitmap updates
    /// </summary>
    [ObservableProperty]
    private int _bitmapVersion;

    /// <summary>
    /// Gets the cached bitmap, rendering it if necessary
    /// </summary>
    public SKBitmap? GetCachedBitmap()
    {
        if (CoreLayer == null)
            return null;

        // Check if we need to re-render
        if (_bitmapDirty || _cachedBitmap == null ||
            _cachedBitmap.Width != Width || _cachedBitmap.Height != Height)
        {
            // Dispose old bitmap
            _cachedBitmap?.Dispose();

            // Create new bitmap
            _cachedBitmap = new SKBitmap(Math.Max(1, Width), Math.Max(1, Height));
            using var canvas = new SKCanvas(_cachedBitmap);
            canvas.Clear(SKColors.Transparent);

            // Update layer properties
            CoreLayer.Position = new NGWebGal.Types.IVector(0, 0); // Render at 0,0 in the bitmap
            CoreLayer.Size = new NGWebGal.Types.IVector(Width, Height);

            // IMPORTANT: DoAnimation initializes colors and animation data
            CoreLayer.DoAnimation(0);

            // Render to bitmap
            CoreLayer.Render(canvas, force: true);

            _bitmapDirty = false;
        }

        return _cachedBitmap;
    }

    /// <summary>
    /// Marks the cached bitmap as dirty (needs re-render)
    /// </summary>
    public void InvalidateBitmap()
    {
        System.Diagnostics.Debug.WriteLine($"[WidgetViewModel] InvalidateBitmap called for widget {Name} (ID: {Id})");
        _bitmapDirty = true;
        BitmapVersion++; // Trigger PropertyChanged via ObservableProperty
        System.Diagnostics.Debug.WriteLine($"[WidgetViewModel] Bitmap invalidated and property change notified (version: {BitmapVersion})");
    }

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
        _model = widget; // Store reference to original EditorWidget

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

        // Subscribe to property changes to sync back to model and mark dirty
        _propertyChangedHandler = (_, e) =>
        {
            if (e.PropertyName != null)
            {
                SyncPropertyToModel(e.PropertyName);
            }
            _mainViewModel.MarkDirty();
        };
        PropertyChanged += _propertyChangedHandler;
    }

    /// <summary>
    /// Converts this ViewModel back to an EditorWidget model.
    /// </summary>
    /// <returns>A new EditorWidget instance with current property values.</returns>
    public EditorWidget ToModel()
    {
        // Sync all properties to ensure model is up-to-date
        _model.Id = Id;
        _model.Type = Type;
        _model.Name = Name;
        _model.X = X;
        _model.Y = Y;
        _model.Width = Width;
        _model.Height = Height;
        _model.Visible = Visible;
        _model.Enable = Enable;
        _model.Text = Text;
        _model.ZIndex = ZIndex;

        // Return the original instance, preserving shadow state (ImageInfos, Color, etc.)
        return _model;
    }

    /// <summary>
    /// Syncs a single property change back to the EditorWidget model.
    /// </summary>
    private void SyncPropertyToModel(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Id): _model.Id = Id; break;
            case nameof(Type): _model.Type = Type; break;
            case nameof(Name): _model.Name = Name; break;
            case nameof(X): _model.X = X; break;
            case nameof(Y): _model.Y = Y; break;
            case nameof(Width): _model.Width = Width; break;
            case nameof(Height): _model.Height = Height; break;
            case nameof(Visible): _model.Visible = Visible; break;
            case nameof(Enable): _model.Enable = Enable; break;
            case nameof(Text): _model.Text = Text; break;
            case nameof(ZIndex): _model.ZIndex = ZIndex; break;
        }
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
            _cachedBitmap?.Dispose();
            _cachedBitmap = null;
            CoreLayer = null;
            _disposed = true;
        }
    }
}
