using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using NGWebGal.Extend.SkiaSharp;
using SkiaSharp;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Dialog for cropping an image with X/Y/W/H parameters
/// </summary>
public partial class ImageCropDialog : Window
{
    private SKBitmap? _originalBitmap;
    private bool _isUpdating;
    private CancellationTokenSource? _updateCancellation;
    private const int DebounceDelayMs = 150; // 150ms debounce delay
    private readonly string _imagePath;

    public ImageCropDialog(string imagePath)
    {
        _imagePath = imagePath;
        InitializeComponent();

        // Hook up events
        var xNumeric = this.FindControl<NumericUpDown>("XNumeric");
        var yNumeric = this.FindControl<NumericUpDown>("YNumeric");
        var widthNumeric = this.FindControl<NumericUpDown>("WidthNumeric");
        var heightNumeric = this.FindControl<NumericUpDown>("HeightNumeric");
        var okButton = this.FindControl<Button>("OkButton");
        var cancelButton = this.FindControl<Button>("CancelButton");

        if (xNumeric != null)
            xNumeric.ValueChanged += OnCropParamChanged;
        if (yNumeric != null)
            yNumeric.ValueChanged += OnCropParamChanged;
        if (widthNumeric != null)
            widthNumeric.ValueChanged += OnCropParamChanged;
        if (heightNumeric != null)
            heightNumeric.ValueChanged += OnCropParamChanged;
        if (okButton != null)
            okButton.Click += OnOkClick;
        if (cancelButton != null)
            cancelButton.Click += OnCancelClick;
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        // Load image asynchronously after dialog is shown
        await LoadImageAsync();
    }

    private async Task LoadImageAsync()
    {
        try
        {
            // Load bitmap on background thread
            var bitmap = await Task.Run(() => SKBitmap.Decode(_imagePath));

            if (bitmap == null)
                return;

            _originalBitmap = bitmap;

            // Initialize crop params to full image
            _isUpdating = true;
            this.FindControl<NumericUpDown>("XNumeric")!.Maximum = _originalBitmap.Width;
            this.FindControl<NumericUpDown>("YNumeric")!.Maximum = _originalBitmap.Height;
            this.FindControl<NumericUpDown>("WidthNumeric")!.Maximum = _originalBitmap.Width;
            this.FindControl<NumericUpDown>("HeightNumeric")!.Maximum = _originalBitmap.Height;

            this.FindControl<NumericUpDown>("WidthNumeric")!.Value = _originalBitmap.Width;
            this.FindControl<NumericUpDown>("HeightNumeric")!.Value = _originalBitmap.Height;
            _isUpdating = false;

            // Generate initial previews asynchronously
            await UpdatePreviewsAsync(CancellationToken.None);

            // Hide loading overlay, show main content
            var loadingOverlay = this.FindControl<Border>("LoadingOverlay")!;
            var mainContent = this.FindControl<Grid>("MainContent")!;
            loadingOverlay.IsVisible = false;
            mainContent.IsVisible = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
            Close((false, new SKRectI()));
        }
    }

    private async void OnCropParamChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdating)
            return;

        // Cancel previous update task
        _updateCancellation?.Cancel();
        _updateCancellation = new CancellationTokenSource();
        var token = _updateCancellation.Token;

        try
        {
            // Debounce: wait for user to stop rapid adjustments
            await Task.Delay(DebounceDelayMs, token);

            // Async update previews
            await UpdatePreviewsAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, ignore
        }
    }

    private async Task UpdatePreviewsAsync(CancellationToken cancellationToken)
    {
        if (_originalBitmap == null)
            return;

        // Read parameters on UI thread
        var (x, y, w, h) = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var xVal = (int)(this.FindControl<NumericUpDown>("XNumeric")!.Value ?? 0);
            var yVal = (int)(this.FindControl<NumericUpDown>("YNumeric")!.Value ?? 0);
            var wVal = (int)(this.FindControl<NumericUpDown>("WidthNumeric")!.Value ?? 0);
            var hVal = (int)(this.FindControl<NumericUpDown>("HeightNumeric")!.Value ?? 0);
            return (xVal, yVal, wVal, hVal);
        });

        // Clamp to bounds
        x = Math.Max(0, Math.Min(x, _originalBitmap.Width - 1));
        y = Math.Max(0, Math.Min(y, _originalBitmap.Height - 1));
        w = Math.Max(1, Math.Min(w, _originalBitmap.Width - x));
        h = Math.Max(1, Math.Min(h, _originalBitmap.Height - y));

        if (w == 0 || h == 0)
        {
            w = _originalBitmap.Width;
            h = _originalBitmap.Height;
            x = 0;
            y = 0;
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Generate previews on background thread
        var (originalPreview, cropPreview) = await Task.Run(() =>
        {
            // 1. Original thumbnail (max 400px) with red rectangle
            const int maxPreviewSize = 400;
            var scale = Math.Min(1.0, (double)maxPreviewSize / Math.Max(_originalBitmap.Width, _originalBitmap.Height));
            var thumbWidth = (int)(_originalBitmap.Width * scale);
            var thumbHeight = (int)(_originalBitmap.Height * scale);

            var thumbnail = new SKBitmap(thumbWidth, thumbHeight);
            using (var canvas = new SKCanvas(thumbnail))
            {
                // Use low quality for fast preview rendering
                using var paint = new SKPaint { FilterQuality = SKFilterQuality.Low };
                canvas.DrawBitmap(_originalBitmap, new SKRect(0, 0, thumbWidth, thumbHeight), paint);

                // Draw red crop rectangle (scaled)
                var scaledX = (float)(x * scale);
                var scaledY = (float)(y * scale);
                var scaledW = (float)(w * scale);
                var scaledH = (float)(h * scale);

                using var rectPaint = new SKPaint
                {
                    Color = SKColors.Red,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1,
                    IsAntialias = false
                };
                canvas.DrawRect(scaledX, scaledY, scaledW, scaledH, rectPaint);
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 2. Crop preview with low quality
            var subRect = new SKRectI(x, y, x + w, y + h);
            var cropBitmap = _originalBitmap.SubBitmap(subRect, SKFilterQuality.Low);

            // If crop result is too large, also generate thumbnail
            if (cropBitmap.Width > maxPreviewSize || cropBitmap.Height > maxPreviewSize)
            {
                var cropScale = (double)maxPreviewSize / Math.Max(cropBitmap.Width, cropBitmap.Height);
                var cropThumbWidth = (int)(cropBitmap.Width * cropScale);
                var cropThumbHeight = (int)(cropBitmap.Height * cropScale);

                var cropThumb = new SKBitmap(cropThumbWidth, cropThumbHeight);
                using (var canvas = new SKCanvas(cropThumb))
                {
                    using var paint = new SKPaint { FilterQuality = SKFilterQuality.Low };
                    canvas.DrawBitmap(cropBitmap, new SKRect(0, 0, cropThumbWidth, cropThumbHeight), paint);
                }
                cropBitmap.Dispose();
                cropBitmap = cropThumb;
            }

            return (thumbnail, cropBitmap);
        }, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Update images on UI thread with low-quality JPEG encoding
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                var originalImage = this.FindControl<Image>("OriginalImage")!;
                // Use JPEG 75% quality for fast encoding
                using (var data = originalPreview.Encode(SKEncodedImageFormat.Jpeg, 75))
                using (var stream = data.AsStream())
                {
                    originalImage.Source = new Bitmap(stream);
                }
                originalPreview.Dispose();

                var croppedImage = this.FindControl<Image>("CroppedImage")!;
                using (var data = cropPreview.Encode(SKEncodedImageFormat.Jpeg, 75))
                using (var stream = data.AsStream())
                {
                    croppedImage.Source = new Bitmap(stream);
                }
                cropPreview.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview: {ex.Message}");
            }
        });
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        var x = (int)(this.FindControl<NumericUpDown>("XNumeric")!.Value ?? 0);
        var y = (int)(this.FindControl<NumericUpDown>("YNumeric")!.Value ?? 0);
        var w = (int)(this.FindControl<NumericUpDown>("WidthNumeric")!.Value ?? 0);
        var h = (int)(this.FindControl<NumericUpDown>("HeightNumeric")!.Value ?? 0);

        var subRect = new SKRectI(x, y, x + w, y + h);

        Close((true, subRect));
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close((false, new SKRectI()));
    }

    protected override void OnClosed(EventArgs e)
    {
        _updateCancellation?.Cancel();
        _updateCancellation?.Dispose();
        _originalBitmap?.Dispose();
        base.OnClosed(e);
    }
}
