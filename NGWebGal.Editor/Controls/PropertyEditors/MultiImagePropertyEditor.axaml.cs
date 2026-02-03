using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using NGWebGal.Editor.Models;
using NGWebGal.Editor.Services.PropertyReflection;
using NGWebGal.Extensions;
using NGWebGal.Layer;
using SkiaSharp;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for multi-image support with sub-rectangle cropping
/// </summary>
public partial class MultiImagePropertyEditor : UserControl, IPropertyEditor
{
    private ILayer? _layer;
    private EditorWidget? _widget;
    private int _currentImageId;
    private bool _isUpdating;
    private readonly Func<ILayer, EditorWidget?>? _widgetLookup;

    public PropertyDescriptor Descriptor { get; }
    public Control Control => this;
    public string PropertyName => Descriptor.Name;

    public event EventHandler? ValueChanged;

    public MultiImagePropertyEditor(PropertyDescriptor descriptor, Func<ILayer, EditorWidget?>? widgetLookup = null)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _widgetLookup = widgetLookup;
        InitializeComponent();

        // Set property name label
        var nameLabel = this.FindControl<TextBlock>("PropertyNameLabel");
        if (nameLabel != null)
        {
            nameLabel.Text = descriptor.Name + ":";
        }

        // Hook up events
        var imageIdComboBox = this.FindControl<ComboBox>("ImageIdComboBox");
        var addButton = this.FindControl<Button>("AddButton");
        var deleteButton = this.FindControl<Button>("DeleteButton");
        var selectButton = this.FindControl<Button>("SelectButton");

        if (imageIdComboBox != null)
            imageIdComboBox.SelectionChanged += OnImageIdSelectionChanged;
        if (addButton != null)
            addButton.Click += OnAddImageClick;
        if (deleteButton != null)
            deleteButton.Click += OnDeleteImageClick;
        if (selectButton != null)
            selectButton.Click += OnSelectFileClick;
    }

    public void Bind(object target)
    {
        if (target is not ILayer layer)
            throw new ArgumentException("Target must be an ILayer", nameof(target));

        _layer = layer;
        _widget = _widgetLookup?.Invoke(layer);

        RefreshImageIdList();
        UpdateDisplay();
    }

    public void Unbind()
    {
        _layer = null;
        _widget = null;
    }

    private void RefreshImageIdList()
    {
        if (_layer == null)
            return;

        _isUpdating = true;

        var comboBox = this.FindControl<ComboBox>("ImageIdComboBox");
        if (comboBox == null)
        {
            _isUpdating = false;
            return;
        }

        var ids = _layer.GetImageIds();

        // If no images, default to 0
        if (ids.Length == 0)
        {
            ids = new[] { 0 };
        }

        comboBox.ItemsSource = ids.Select(id => id.ToString()).ToList();

        // Select current ID or first available
        if (ids.Contains(_currentImageId))
        {
            comboBox.SelectedItem = _currentImageId.ToString();
        }
        else
        {
            _currentImageId = ids[0];
            comboBox.SelectedItem = _currentImageId.ToString();
        }

        _isUpdating = false;
    }

    private void OnImageIdSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdating)
            return;

        var comboBox = (ComboBox)sender!;
        if (comboBox.SelectedItem is string selectedId && int.TryParse(selectedId, out var id))
        {
            _currentImageId = id;
            UpdateDisplay();
        }
    }

    private async Task SelectImageFile()
    {
        if (_layer == null || _widget == null)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择图像",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("图像文件")
                {
                    Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                }
            }
        });

        if (files.Count == 0)
            return;

        var filePath = files[0].Path.LocalPath;

        // Open crop dialog
        var cropDialog = new ImageCropDialog(filePath);
        var result = await cropDialog.ShowDialog<(bool accepted, SKRectI subRect)>(
            topLevel as Window ?? throw new InvalidOperationException("TopLevel must be a Window"));

        if (result.accepted)
        {
            await ApplyImage(filePath, result.subRect);
        }
    }

    private async Task ApplyImage(string filePath, SKRectI subRect)
    {
        if (_layer == null || _widget == null)
            return;

        try
        {
            // Load bitmap
            var bitmap = SKBitmap.Decode(filePath);
            if (bitmap != null)
            {
                // Crop if needed
                SKBitmap finalBitmap;
                if (subRect.Width > 0 && subRect.Height > 0)
                {
                    // Use high quality for final application
                    finalBitmap = bitmap.SubBitmap(subRect, SKFilterQuality.High);
                    bitmap.Dispose();
                }
                else
                {
                    finalBitmap = bitmap;
                }

                // Store metadata in EditorWidget FIRST
                _widget.ImageInfos[_currentImageId] = (filePath, subRect);

                System.Diagnostics.Debug.WriteLine($"[MultiImagePropertyEditor] Stored metadata for ID {_currentImageId}: path={filePath}, subRect={subRect}");
                System.Diagnostics.Debug.WriteLine($"[MultiImagePropertyEditor] Widget instance: {_widget.GetHashCode()}, ImageInfos count: {_widget.ImageInfos.Count}");

                // Then call SetImage
                _layer.SetImage(finalBitmap, _currentImageId);

                // Refresh ID list (in case this was a new ID)
                RefreshImageIdList();

                // Update display
                UpdateDisplay();

                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
        }
    }

    private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
    {
        await SelectImageFile();
    }

    private async void OnAddImageClick(object? sender, RoutedEventArgs e)
    {
        if (_layer == null)
            return;

        // Ask user for new ID
        var dialog = new AddImageIdDialog();
        var topLevel = TopLevel.GetTopLevel(this);
        var parentWindow = topLevel as Window;
        if (parentWindow == null)
            return;

        var newId = await dialog.ShowDialog<int?>(parentWindow);

        if (newId.HasValue)
        {
            _currentImageId = newId.Value;
            RefreshImageIdList();

            // 立即进入文件选择流程
            await SelectImageFile();
        }
    }

    private void OnDeleteImageClick(object? sender, RoutedEventArgs e)
    {
        if (_layer == null || _widget == null)
            return;

        // Remove metadata
        _widget.ImageInfos.Remove(_currentImageId);

        // Refresh ID list
        RefreshImageIdList();
        UpdateDisplay();

        ValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateDisplay()
    {
        if (_layer == null || _widget == null)
        {
            System.Diagnostics.Debug.WriteLine("[UpdateDisplay] Layer or widget is null");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Current ID: {_currentImageId}");
        System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Widget instance: {_widget.GetHashCode()}, ImageInfos count: {_widget.ImageInfos.Count}");

        var thumbnailImage = this.FindControl<Image>("ThumbnailImage")!;
        var pathLabel = this.FindControl<TextBlock>("PathLabel")!;
        var sizeLabel = this.FindControl<TextBlock>("SizeLabel")!;
        var cropLabel = this.FindControl<TextBlock>("CropLabel")!;

        // Try to get metadata
        if (!_widget.ImageInfos.TryGetValue(_currentImageId, out var info))
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] No metadata for ID {_currentImageId}");

            // No metadata, but check if image exists in layer
            var skImage = _layer.GetImage(_currentImageId);
            if (skImage != null)
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Image exists but no metadata");
                // Image exists but no metadata
                pathLabel.Text = "路径: (直接设置的bitmap)";
                sizeLabel.Text = $"尺寸: {skImage.Width} × {skImage.Height}";
                cropLabel.Text = "裁剪: -";

                // Display thumbnail
                try
                {
                    using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
                    using var stream = data.AsStream();
                    thumbnailImage.Source = new Bitmap(stream);
                }
                catch
                {
                    thumbnailImage.Source = null;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] No image at all");
                // No image at all
                thumbnailImage.Source = null;
                pathLabel.Text = "路径: (未设置)";
                sizeLabel.Text = "尺寸: -";
                cropLabel.Text = "裁剪: -";
            }
            return;
        }

        var (path, subRect) = info;

        System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Found metadata: path={path}, subRect={subRect}");

        // Display path
        pathLabel.Text = $"路径: {Path.GetFileName(path)}";

        // Load thumbnail directly from file (more reliable than GetImage)
        try
        {
            if (File.Exists(path))
            {
                // Load original bitmap from file
                var originalBitmap = SKBitmap.Decode(path);
                if (originalBitmap != null)
                {
                    SKBitmap displayBitmap;

                    // Apply crop if specified
                    if (subRect.Width > 0 && subRect.Height > 0)
                    {
                        displayBitmap = originalBitmap.SubBitmap(subRect, SKFilterQuality.Low);
                        originalBitmap.Dispose();
                        sizeLabel.Text = $"尺寸: {subRect.Width} × {subRect.Height}";
                    }
                    else
                    {
                        displayBitmap = originalBitmap;
                        sizeLabel.Text = $"尺寸: {originalBitmap.Width} × {originalBitmap.Height}";
                    }

                    // Generate thumbnail (max 64px for preview)
                    const int thumbSize = 64;
                    var scale = Math.Min(1.0, (double)thumbSize / Math.Max(displayBitmap.Width, displayBitmap.Height));
                    if (scale < 1.0)
                    {
                        var thumbWidth = (int)(displayBitmap.Width * scale);
                        var thumbHeight = (int)(displayBitmap.Height * scale);
                        var thumbnail = new SKBitmap(thumbWidth, thumbHeight);
                        using (var canvas = new SKCanvas(thumbnail))
                        {
                            using var paint = new SKPaint { FilterQuality = SKFilterQuality.Low };
                            canvas.DrawBitmap(displayBitmap, new SKRect(0, 0, thumbWidth, thumbHeight), paint);
                        }
                        displayBitmap.Dispose();
                        displayBitmap = thumbnail;
                    }

                    // Display thumbnail
                    using (var data = displayBitmap.Encode(SKEncodedImageFormat.Jpeg, 75))
                    using (var stream = data.AsStream())
                    {
                        thumbnailImage.Source = new Bitmap(stream);
                    }
                    displayBitmap.Dispose();

                    System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Thumbnail loaded from file successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Failed to decode bitmap from file");
                    thumbnailImage.Source = null;
                    sizeLabel.Text = "尺寸: 解码失败";
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] File does not exist: {path}");
                // Fallback: try to get from layer
                var skImage = _layer.GetImage(_currentImageId);
                if (skImage != null)
                {
                    using var data = skImage.Encode(SKEncodedImageFormat.Png, 100);
                    using var stream = data.AsStream();
                    thumbnailImage.Source = new Bitmap(stream);

                    sizeLabel.Text = $"尺寸: {skImage.Width} × {skImage.Height}";
                }
                else
                {
                    thumbnailImage.Source = null;
                    sizeLabel.Text = "尺寸: 文件不存在";
                }
            }

            // Display crop info
            if (subRect.Width > 0 && subRect.Height > 0)
            {
                cropLabel.Text = $"裁剪: X:{subRect.Left} Y:{subRect.Top} W:{subRect.Width} H:{subRect.Height}";
                System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Displayed crop info");
            }
            else
            {
                cropLabel.Text = "裁剪: 完整图像";
                System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] No cropping");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateDisplay] Error: {ex.Message}");
            thumbnailImage.Source = null;
            sizeLabel.Text = "尺寸: 加载失败";
        }
    }
}
