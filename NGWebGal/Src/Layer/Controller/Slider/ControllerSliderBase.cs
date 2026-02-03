using System;
using System.Collections.Generic;
using SkiaSharp;
using NGWebGal.Global;
using NGWebGal.Types;
using NGWebGal.Extensions;
using NGWebGal.Handler.Event;

namespace NGWebGal.Layer.Controller;

/// <summary>
/// Base class for slider controls with track and thumb components
/// </summary>
public abstract class ControllerSliderBase : LayerBase
{
    protected SortedDictionary<int, SKBitmap> _image = [];
    protected SortedDictionary<int, SKColor> _colors = []; // Store colors for each imageId

    protected IVector _mouseDelta = new(0, 0);
    protected new float _value = 0;
    protected IVector _thumbSize = new();
    protected IVector _thumbDelta;

    // Slider elements: thumb
    public bool ThumbVisible { get; set; } = true;

    // Common properties
    public IVector ThumbPosition
    {
        get { return Position + _thumbDelta; }
        set { _thumbDelta = value - Position; }
    }

    public IVector ThumbSize
    {
        get { return _thumbSize; }
        set { _thumbSize = value; }
    }

    public IRect ThumbWindow
    {
        get { return new(ThumbPosition, _thumbSize); }
    }

    public void InitBase()
    {
        Visible = true;
        Enable = true;
        Name = "slider";
        Text = "new slider";
    }

    public void InitAttribute(IVector trackSize, IVector thumbSize)
    {
        Size = trackSize;
        _thumbSize = thumbSize;
        _attributeChange = true;
    }

    public void InitImage(SKBitmap trackImage, SKBitmap sliderNormal, SKBitmap sliderHover, SKBitmap sliderPressed)
    {
        _image[-1] = trackImage;
        _image[(int)LayerStatus.Normal] = sliderNormal;
        _image[(int)LayerStatus.Hover] = sliderHover;
        _image[(int)LayerStatus.Pressed] = sliderPressed;
        _image[(int)LayerStatus.Focused] = sliderPressed;
    }

    public override void SetImage(SKBitmap image, int imageId, IRect? imageWindow = null)
    {
        if (imageWindow != null && imageWindow != default(IRect))
            _image[imageId] = image.SubBitmap(imageWindow);
        else
            _image[imageId] = image;
        _dirty = true;
    }

    public override int[] GetImageIds()
    {
        return System.Linq.Enumerable.ToArray(_image.Keys);
    }

    public override void SetColor(SKColor color, int imageId = 0)
    {
        // Store the color for GetColor retrieval
        _colors[imageId] = color;

        // Use ThumbSize for thumb images (imageId >= 0), Size for track (imageId = -1)
        IVector bitmapSize = imageId == -1 ? Size : _thumbSize;

        SKBitmap bitmap = new(bitmapSize.X, bitmapSize.Y, RenderConfig.DefaultColorType, RenderConfig.DefaultAlphaType);
        using SKCanvas canvas = new(bitmap);
        canvas.DrawRect(
            new SKRect(0, 0, bitmapSize.X, bitmapSize.Y),
            new SKPaint { Color = color }
        );
        canvas.Flush();
        _image[imageId] = bitmap;
    }

    public override SKColor GetColor(int imageId = 0)
    {
        if (_colors.TryGetValue(imageId, out SKColor color))
            return color;

        return SKColors.Transparent;
    }

    public override SKImage? GetImage(int imageId = 0)
    {
        if (_image.TryGetValue(imageId, out SKBitmap? bitmap) && bitmap != null && !bitmap.IsNull)
            return SKImage.FromBitmap(bitmap);

        return null;
    }

    public ControllerSliderBase()
    {
        Position = new IVector(100, 300);

        InitBase();
        InitAttribute(new IVector(200, 20), new IVector(10, 20));

        for (int i = -1; i <= 4; i++)
            _image[i] = new();
    }

    public override bool DoAction(EventArgs eventArgs)
    {
        if (eventArgs is not MouseEventData) return false;
        if (Status == LayerStatus.Disable) return false;

        MouseEventData mouseEvent = (MouseEventData)eventArgs;

        if (Status != LayerStatus.Focused && !(Status == LayerStatus.Pressed && mouseEvent.Status == MouseStatus.Hold))
        {
            if (RangeComp.OutRange(ThumbWindow, mouseEvent.Position))
                Status = LayerStatus.Normal;
            else if (mouseEvent.Button == MouseButton.Empty && mouseEvent.Status == MouseStatus.Release)
                Status = LayerStatus.Hover;
            else if (mouseEvent.Button == MouseButton.LButton && mouseEvent.Status == MouseStatus.Hold)
                Status = LayerStatus.Pressed;
        }

        // When pressed, update thumb position
        if (Status == LayerStatus.Pressed)
        {
            if (_mouseDelta.X == 0 && _mouseDelta.Y == 0)
                _mouseDelta = mouseEvent.Position - ThumbPosition;
            ThumbLimitSet(mouseEvent.Position - Position - _mouseDelta);
            return true;
        }
        else
        {
            _mouseDelta = new(0, 0);
            if (RangeComp.InRange(Window, mouseEvent.Position))
                return true;
        }
        return false;
    }

    public override void Render(SKCanvas canvas, bool force)
    {
        if (Status == LayerStatus.Unvisable || _image[(int)Status].IsNull || _image[-1].IsNull)
            return;

        // Render track
        SKMatrix matrix = SKMatrix.Identity;
        FVector pos = (FVector)Position + _animationData.PosOff;

        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)pos.X, (float)pos.Y));
        matrix = SKMatrix.Concat(matrix, _animationData.Transform);
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(_offset.X, _offset.Y));

        canvas.Save();
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(_image[-1], new SKPoint(0, 0), RenderConfig.DefaultPaint);
        canvas.Restore();

        // Render thumb
        matrix = SKMatrix.Identity;
        pos = (FVector)ThumbPosition + _animationData.PosOff;

        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)pos.X, (float)pos.Y));
        matrix = SKMatrix.Concat(matrix, _animationData.Transform);
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(_offset.X, _offset.Y));

        canvas.Save();
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(_image[(int)Status], new SKPoint(0, 0), _animationData.Paint);
        canvas.Restore();
    }

    // Value range [0,1]
    public override object Value
    {
        get => base.Value;
        set
        {
            if (value is float v)
            {
                base.Value = value;
                ThumbLimitSet((IVector)((FVector)(Size - ThumbSize) * v));
            }
        }
    }

    protected virtual void ThumbLimitSet(IVector thumbDelta) => _thumbDelta = new(0, 0);
}
