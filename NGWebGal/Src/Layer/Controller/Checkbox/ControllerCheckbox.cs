using System;
using System.Collections.Generic;
using SkiaSharp;
using NGWebGal.Global;
using NGWebGal.Types;
using NGWebGal.Extensions;
using NGWebGal.Handler.Event;

namespace NGWebGal.Layer.Controller;

/// <summary>
/// Interactive checkbox controller with 8-state support (4 unchecked + 4 checked states)
/// States: Normal, Hover, Pressed, Disabled for both checked and unchecked
/// </summary>
public class ControllerCheckbox : ControllerLayerBase
{
    protected List<SKBitmap> _imageBuffer = [];
    protected List<SKBitmap> _renderBuffer = [];
    protected List<SKColor?> _colors = []; // Store colors for each state

    /// <summary>
    /// Gets or sets whether the checkbox is checked
    /// </summary>
    public bool IsChecked { get; set; } = false;

    /// <summary>
    /// Returns the IsChecked value as a boxed object
    /// </summary>
    public override object Value
    {
        get => IsChecked;
        set
        {
            if (value is bool boolValue)
            {
                IsChecked = boolValue;
                _dirty = true;
            }
        }
    }

    public ControllerCheckbox()
    {
        // Initialize buffers for 8 states:
        // 0-3: Unchecked (Normal, Hover, Pressed, Disabled)
        // 4-7: Checked (Normal, Hover, Pressed, Disabled)
        for (int i = 0; i < 8; i++)
        {
            _imageBuffer.Add(new());
            _renderBuffer.Add(new());
            _colors.Add(null);
        }
    }

    public override void SetImage(SKBitmap image, int imageId, IRect? imageWindow = null)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        if (imageWindow != null && imageWindow != default(IRect))
            _imageBuffer[imageId] = image.SubBitmap(imageWindow);
        else
            _imageBuffer[imageId] = image;
        _dirty = true;
    }

    public override int[] GetImageIds()
    {
        var ids = new List<int>();
        for (int i = 0; i < _imageBuffer.Count; i++)
        {
            if (!_imageBuffer[i].IsNull())
                ids.Add(i);
        }
        return ids.ToArray();
    }

    public override void SetColor(SKColor color, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;

        // Store the color for GetColor retrieval
        if (imageId < _colors.Count)
            _colors[imageId] = color;

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

    public override SKColor GetColor(int imageId = 0)
    {
        if (imageId >= 0 && imageId < _colors.Count && _colors[imageId].HasValue)
            return _colors[imageId]!.Value;

        return SKColors.Transparent;
    }

    public override SKImage? GetImage(int imageId = 0)
    {
        if (imageId < 0 || imageId >= _renderBuffer.Count || _renderBuffer[imageId].IsNull)
            return null;

        return SKImage.FromBitmap(_renderBuffer[imageId]);
    }

    public override bool DoAction(EventArgs eventArgs)
    {
        if (eventArgs is not MouseEventData) return false;
        if (Status == LayerStatus.Disable) return false;

        MouseEventData mouseEvent = (MouseEventData)eventArgs;

        LayerStatus nowStatus = Status;
        bool stateChanged = false;

        if (Status != LayerStatus.Focused)
        {
            if (RangeComp.OutRange(Window, mouseEvent.Position))
            {
                nowStatus = LayerStatus.Normal;
            }
            else if (mouseEvent.Button == MouseButton.Empty && mouseEvent.Status == MouseStatus.Release)
            {
                nowStatus = LayerStatus.Hover;
            }
            else if (mouseEvent.Button == MouseButton.LButton && mouseEvent.Status == MouseStatus.Hold)
            {
                nowStatus = LayerStatus.Pressed;
            }
            else if (mouseEvent.Button == MouseButton.LButton && mouseEvent.Status == MouseStatus.Release
                     && !RangeComp.OutRange(Window, mouseEvent.Position))
            {
                // Toggle the checked state on click release
                IsChecked = !IsChecked;
                stateChanged = true;
                nowStatus = LayerStatus.Hover;
            }
        }

        if (nowStatus != Status || stateChanged)
        {
            Status = nowStatus;
            TriggerEvent(mouseEvent);
            return true;
        }

        return false;
    }

    public override void Render(SKCanvas canvas, bool force)
    {
        if (Status == LayerStatus.Unvisable || _imageBuffer is null || _imageBuffer[0].IsNull)
            return;

        if (_dirty || force || _renderBuffer[GetImageIndex()].IsNull)
        {
            // Fill missing unchecked states with previous state images
            for (int i = 1; i < 4; i++)
                if (_imageBuffer[i].IsNull)
                    _imageBuffer[i] = _imageBuffer[i - 1];

            // Fill missing checked states with previous state images
            for (int i = 5; i < 8; i++)
                if (_imageBuffer[i].IsNull)
                    _imageBuffer[i] = _imageBuffer[i - 1];

            // If checked states are missing, copy from unchecked states
            if (_imageBuffer[4].IsNull)
                _imageBuffer[4] = _imageBuffer[0];

            // Resize all images to target size
            for (int i = 0; i < 8; i++)
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
        canvas.DrawBitmap(_renderBuffer[GetImageIndex()], new SKPoint(0, 0), _animationData.Paint);
        canvas.Restore();
    }

    /// <summary>
    /// Calculate the image index based on IsChecked state and current Status
    /// </summary>
    private int GetImageIndex()
    {
        int baseIndex = IsChecked ? 4 : 0;
        return baseIndex + (int)Status;
    }
}
