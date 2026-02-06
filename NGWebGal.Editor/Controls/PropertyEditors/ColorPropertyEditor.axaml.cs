using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NGWebGal.Editor.Services.PropertyReflection;
using SkiaSharp;

namespace NGWebGal.Editor.Controls.PropertyEditors;

/// <summary>
/// Property editor for SKColor with checkerboard alpha preview
/// </summary>
public partial class ColorPropertyEditor : UserControl, IPropertyEditor
{
    private object? _target;
    private bool _isUpdating;
    private static Bitmap? _checkerboardBitmap;
    private INotifyPropertyChanged? _observableTarget;

    public Services.PropertyReflection.PropertyDescriptor Descriptor { get; }
    public Control Control => this;
    public string PropertyName => Descriptor.Name;

    public event EventHandler? ValueChanged;

    public ColorPropertyEditor(Services.PropertyReflection.PropertyDescriptor descriptor)
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

        // Hook up value changed events
        var aNumeric = this.FindControl<NumericUpDown>("ANumeric");
        var rNumeric = this.FindControl<NumericUpDown>("RNumeric");
        var gNumeric = this.FindControl<NumericUpDown>("GNumeric");
        var bNumeric = this.FindControl<NumericUpDown>("BNumeric");

        if (aNumeric != null) aNumeric.ValueChanged += OnColorValueChanged;
        if (rNumeric != null) rNumeric.ValueChanged += OnColorValueChanged;
        if (gNumeric != null) gNumeric.ValueChanged += OnColorValueChanged;
        if (bNumeric != null) bNumeric.ValueChanged += OnColorValueChanged;
    }

    /// <summary>
    /// Creates a 16x16 checkerboard pattern bitmap with 8x8 pixel squares
    /// </summary>
    private static Bitmap CreateCheckerboardBitmap()
    {
        const int size = 16;
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
            if (value is SKColor color)
            {
                var aNumeric = this.FindControl<NumericUpDown>("ANumeric");
                var rNumeric = this.FindControl<NumericUpDown>("RNumeric");
                var gNumeric = this.FindControl<NumericUpDown>("GNumeric");
                var bNumeric = this.FindControl<NumericUpDown>("BNumeric");

                if (aNumeric != null) aNumeric.Value = color.Alpha;
                if (rNumeric != null) rNumeric.Value = color.Red;
                if (gNumeric != null) gNumeric.Value = color.Green;
                if (bNumeric != null) bNumeric.Value = color.Blue;

                UpdateColorPreview(color);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void OnColorValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_target == null || _isUpdating || Descriptor.Setter == null)
            return;

        UpdateTarget();
    }

    private void UpdateTarget()
    {
        if (_target == null || _isUpdating || Descriptor.Setter == null)
            return;

        _isUpdating = true;
        try
        {
            var aNumeric = this.FindControl<NumericUpDown>("ANumeric");
            var rNumeric = this.FindControl<NumericUpDown>("RNumeric");
            var gNumeric = this.FindControl<NumericUpDown>("GNumeric");
            var bNumeric = this.FindControl<NumericUpDown>("BNumeric");

            if (aNumeric?.Value == null || rNumeric?.Value == null ||
                gNumeric?.Value == null || bNumeric?.Value == null)
                return;

            var color = new SKColor(
                (byte)rNumeric.Value.Value,
                (byte)gNumeric.Value.Value,
                (byte)bNumeric.Value.Value,
                (byte)aNumeric.Value.Value
            );

            Descriptor.Setter(_target, color);
            UpdateColorPreview(color);

            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void UpdateColorPreview(SKColor color)
    {
        var colorPreview = this.FindControl<Border>("ColorPreview");
        if (colorPreview != null)
        {
            // Convert SKColor to Avalonia Color
            var avaloniaColor = new Avalonia.Media.Color(color.Alpha, color.Red, color.Green, color.Blue);
            colorPreview.Background = new Avalonia.Media.SolidColorBrush(avaloniaColor);
        }
    }
}
