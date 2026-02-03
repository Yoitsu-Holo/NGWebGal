using System;
using SkiaSharp;
using NGWebGal.Extensions;
using NGWebGal.Types;

namespace NGWebGal.Layer.Widget;

/// <summary>
/// Widget layer that renders an image with optional sub-rectangle support
/// </summary>
public class WidgetImageBox : WidgetLayerBase
{
	private SKBitmap _imageBuffer = new();
	private SKBitmap? _renderBuffer;

	public override void SetImage(SKBitmap image, int imageId, IRect? imageWindow = null)
	{
		if (imageWindow != null && (imageWindow.W != 0 || imageWindow.H != 0))
		{
			_imageBuffer = image.SubBitmap(imageWindow);
		}
		else
		{
			_imageBuffer = image;
		}
		_dirty = true;
	}

	public override int[] GetImageIds()
	{
		return _imageBuffer.IsNull() ? Array.Empty<int>() : new[] { 0 };
	}

	public override SKImage? GetImage(int imageId = 0)
	{
		if (_renderBuffer == null || _renderBuffer.IsNull())
			return null;

		return SKImage.FromBitmap(_renderBuffer);
	}

	public override void Render(SKCanvas canvas, bool force)
	{
		if (Status == LayerStatus.Unvisable || _imageBuffer.IsNull())
			return;

		if (_dirty || force || _renderBuffer is null)
		{
			_renderBuffer = _imageBuffer.Resize(Size, SKFilterQuality.High);
			_dirty = false;
		}

		SKMatrix matrix = SKMatrix.Identity;
		FVector pos = (FVector)Position + _animationData.PosOff;

		matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY));
		matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)pos.X, (float)pos.Y));
		matrix = SKMatrix.Concat(matrix, _animationData.Transform);
		matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(_offset.X, _offset.Y));

		canvas.Save();
		canvas.SetMatrix(matrix);
		canvas.DrawBitmap(_renderBuffer, new SKPoint(0, 0), _animationData.Paint);
		canvas.Restore();
	}
}
