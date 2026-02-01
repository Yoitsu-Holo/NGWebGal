using System;
using System.Collections.Generic;
using SkiaSharp;
using NGWebGal.Global;
using NGWebGal.Types;
using NGWebGal.Extensions;

namespace NGWebGal.Layer.Widget;

/// <summary>
/// Progress bar widget with background track and fill bar
/// </summary>
public class WidgetProgressBar : WidgetLayerBase
{
    protected List<SKBitmap> _imageBuffer = [];
    protected List<SKBitmap> _renderBuffer = [];

    private float _progress = 0.0f;

    /// <summary>
    /// Progress value (0.0 to 1.0)
    /// </summary>
    public float Progress
    {
        get => _progress;
        set
        {
            _progress = Math.Clamp(value, 0.0f, 1.0f);
            _dirty = true;
        }
    }

    /// <summary>
    /// Override Value property to return Progress
    /// </summary>
    public override object Value
    {
        get => Progress;
        set
        {
            if (value is float floatValue)
                Progress = floatValue;
            else if (value is double doubleValue)
                Progress = (float)doubleValue;
            else if (value is int intValue)
                Progress = intValue;
        }
    }

    public WidgetProgressBar()
    {
        // Initialize buffers for 2 images: [0] = background track, [1] = fill bar
        for (int i = 0; i < 2; i++)
        {
            _imageBuffer.Add(new());
            _renderBuffer.Add(new());
        }
    }

    public override void SetImage(SKBitmap image, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        _imageBuffer[imageId] = image;
        _dirty = true;
    }

    public override void SetImage(SKBitmap image, IRect imageWindow, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        if (imageWindow == default)
            SetImage(image, imageId);
        else
            _imageBuffer[imageId] = image.SubBitmap(imageWindow);
        _dirty = true;
    }

    public override void SetColor(SKColor color, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        SKBitmap bitmap = new(Size.X, Size.Y, RenderConfig.DefaultColorType, RenderConfig.DefaultAlphaType);
        using SKCanvas canvas = new(bitmap);
        canvas.DrawRect(
            new SKRect(0, 0, Size.X, Size.Y),
            new SKPaint { Color = color }
        );
        canvas.Flush();
        _imageBuffer[imageId] = bitmap;
        _dirty = true;
    }

    public override void Render(SKCanvas canvas, bool force)
    {
        if (Status == LayerStatus.Unvisable || _imageBuffer is null || _imageBuffer[0].IsNull)
            return;

        if (_dirty || force || _renderBuffer[0].IsNull)
        {
            // Fill missing fill bar with background if not provided
            if (_imageBuffer[1].IsNull)
                _imageBuffer[1] = _imageBuffer[0];

            // Resize both images to target size
            for (int i = 0; i < 2; i++)
                _renderBuffer[i] = _imageBuffer[i].Resize(Size);
            _dirty = false;
        }

        SKMatrix matrix = SKMatrix.Identity;
        FVector pos = (FVector)Position + _animationData.PosOff;

        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(pos.X, pos.Y));
        matrix = SKMatrix.Concat(matrix, _animationData.Transform);
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(_offset.X, _offset.Y));

        canvas.Save();
        canvas.SetMatrix(matrix);

        // Draw background track at full size
        canvas.DrawBitmap(_renderBuffer[0], new SKPoint(0, 0), _animationData.Paint);

        // Draw fill bar clipped to progress width
        if (Progress > 0.0f)
        {
            int fillWidth = (int)(Size.X * Progress);
            SKRect clipRect = new(0, 0, fillWidth, Size.Y);

            canvas.Save();
            canvas.ClipRect(clipRect);
            canvas.DrawBitmap(_renderBuffer[1], new SKPoint(0, 0), _animationData.Paint);
            canvas.Restore();
        }

        canvas.Restore();
    }
}
