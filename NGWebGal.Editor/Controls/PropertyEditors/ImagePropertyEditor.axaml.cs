using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using NGWebGal.Editor.Services.PropertyReflection;
using SkiaSharp;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for image file paths with thumbnail preview
/// </summary>
public partial class ImagePropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private bool _isUpdating;
    private static Bitmap? _checkerboardBitmap;
    private string? _currentPath;
    private INotifyPropertyChanged? _observableTarget;

    public Services.PropertyReflection.PropertyDescriptor Descriptor { get; }
    public Control Control => this;
    public string PropertyName => Descriptor.Name;

    public event EventHandler? ValueChanged;

    public ImagePropertyEditor(Services.PropertyReflection.PropertyDescriptor descriptor)
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

        // Create checkerboard pattern if not already created
        if (_checkerboardBitmap == null)
        {
            _checkerboardBitmap = CreateCheckerboardBitmap();
        }

        // Set checkerboard background
        var checkerboardImage = this.FindControl<Image>("CheckerboardImage");
        if (checkerboardImage != null)
        {
            checkerboardImage.Source = _checkerboardBitmap;
        }

        // Hook up button events
        var browseButton = this.FindControl<Button>("BrowseButton");
        var clearButton = this.FindControl<Button>("ClearButton");

        if (browseButton != null)
            browseButton.Click += OnBrowseClick;
        if (clearButton != null)
            clearButton.Click += OnClearClick;
    }

    /// <summary>
    /// Creates a 64x64 checkerboard pattern bitmap with 8x8 pixel squares
    /// </summary>
    private static Bitmap CreateCheckerboardBitmap()
    {
        const int size = 64;
        const int squareSize = 8;

        using var surface = SKSurface.Create(new SKImageInfo(size, size));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        using var grayPaint = new SKPaint { Color = new SKColor(0xCC, 0xCC, 0xCC), Style = SKPaintStyle.Fill };

        // Draw light gray squares (checkerboard pattern)
        for (int y = 0; y < size; y += squareSize)
        {
            for (int x = 0; x < size; x += squareSize)
            {
                // Alternate pattern
                if ((x / squareSize + y / squareSize) % 2 == 1)
                {
                    canvas.DrawRect(x, y, squareSize, squareSize, grayPaint);
                }
            }
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = data.AsStream();

        return new Bitmap(stream);
    }

    public void Bind(object target)
    {
        _target = target ?? throw new ArgumentNullException(nameof(target));

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
        _target = null;
    }

    private void OnTargetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == Descriptor.Name)
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
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                _currentPath = path;
                UpdateDisplay(path);
            }
            else
            {
                _currentPath = null;
                ClearDisplay();
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private async void OnBrowseClick(object? sender, RoutedEventArgs e)
    {
        if (_target == null || Descriptor.Setter == null)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;

        var storageProvider = topLevel.StorageProvider;
        if (storageProvider == null)
            return;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Image",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Image Files")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" }
                },
                FilePickerFileTypes.All
            }
        });

        if (files.Count == 0)
            return;

        var file = files[0];
        var filePath = file.Path.LocalPath;

        _isUpdating = true;
        try
        {
            _currentPath = filePath;
            Descriptor.Setter(_target, filePath);
            UpdateDisplay(filePath);

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        if (_target == null || Descriptor.Setter == null)
            return;

        _isUpdating = true;
        try
        {
            _currentPath = null;
            Descriptor.Setter(_target, string.Empty);
            ClearDisplay();

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateDisplay(string path)
    {
        var filePathText = this.FindControl<TextBlock>("FilePathText");
        var thumbnailImage = this.FindControl<Image>("ThumbnailImage");

        if (filePathText != null)
        {
            filePathText.Text = Path.GetFileName(path);
            ToolTip.SetTip(filePathText, path);
        }

        if (thumbnailImage != null && File.Exists(path))
        {
            try
            {
                using var skBitmap = SKBitmap.Decode(path);
                if (skBitmap != null)
                {
                    // Create thumbnail
                    using var surface = SKSurface.Create(new SKImageInfo(64, 64));
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Transparent);

                    // Calculate aspect-fit scaling
                    float scale = Math.Min(64f / skBitmap.Width, 64f / skBitmap.Height);
                    int scaledWidth = (int)(skBitmap.Width * scale);
                    int scaledHeight = (int)(skBitmap.Height * scale);
                    int offsetX = (64 - scaledWidth) / 2;
                    int offsetY = (64 - scaledHeight) / 2;

                    var destRect = new SKRect(offsetX, offsetY, offsetX + scaledWidth, offsetY + scaledHeight);
                    canvas.DrawBitmap(skBitmap, destRect);

                    using var snapshot = surface.Snapshot();
                    using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
                    using var stream = data.AsStream();

                    thumbnailImage.Source = new Bitmap(stream);
                }
            }
            catch
            {
                // If thumbnail generation fails, clear the image
                thumbnailImage.Source = null;
            }
        }
    }

    private void ClearDisplay()
    {
        var filePathText = this.FindControl<TextBlock>("FilePathText");
        var thumbnailImage = this.FindControl<Image>("ThumbnailImage");

        if (filePathText != null)
        {
            filePathText.Text = "(No image)";
            ToolTip.SetTip(filePathText, null);
        }

        if (thumbnailImage != null)
        {
            thumbnailImage.Source = null;
        }
    }
}
