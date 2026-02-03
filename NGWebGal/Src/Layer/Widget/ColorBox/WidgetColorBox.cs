using SkiaSharp;
using NGWebGal.Types;

namespace NGWebGal.Layer.Widget;

/// <summary>
/// Widget layer that renders a solid color rectangle
/// </summary>
public class WidgetColorBox : WidgetLayerBase
{
	private SKColor _color = new();

	public override void SetColor(SKColor color, int imageId = 0)
	{
		_color = color;
		_dirty = true;
	}

	public override SKColor GetColor(int imageId = 0)
	{
		return _color; // SKColor is a struct, so this is already a copy
	}

	public override void Render(SKCanvas canvas, bool force)
	{
		if (Status == LayerStatus.Unvisable)
			return;

		_dirty = false;

		SKMatrix matrix = SKMatrix.Identity;
		FVector pos = (FVector)Position + _animationData.PosOff;

		matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY));
		matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)pos.X, (float)pos.Y));
		matrix = SKMatrix.Concat(matrix, _animationData.Transform);
		matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(_offset.X, _offset.Y));

		canvas.Save();
		canvas.SetMatrix(matrix);
		canvas.DrawRect(new SKRect(0, 0, Size.Width, Size.Height), _animationData.Paint);
		canvas.Restore();
	}

	public override void ResetAnimationData()
	{
		_animationData = new()
		{
			Paint = new() { Color = _color, IsAntialias = true, Style = SKPaintStyle.Fill }
		};
	}
}
