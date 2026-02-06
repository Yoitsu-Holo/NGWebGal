using SkiaSharp;

namespace NGWebGal.Extend.SkiaSharp;

public static class SKBitmapExtensions
{
	/// <summary>
	/// Extracts a sub-rectangle from a bitmap
	/// </summary>
	/// <param name="source">Source bitmap to extract from</param>
	/// <param name="subRect">Rectangle to extract</param>
	/// <param name="quality">Filter quality for the drawing operation (default: Medium)</param>
	public static SKBitmap SubBitmap(this SKBitmap source, SKRectI subRect, SKFilterQuality quality = SKFilterQuality.Medium)
	{
		if (subRect.Width == 0 || subRect.Height == 0)
			subRect = new(0, 0, source.Width, source.Height);

		// Create a new SKBitmap for the cropped image
		var croppedBitmap = new SKBitmap(subRect.Width, subRect.Height);

		// Create a canvas and bind it to the cropped bitmap
		using (var canvas = new SKCanvas(croppedBitmap))
		{
			// Use specified quality for drawing
			using var paint = new SKPaint { FilterQuality = quality };
			canvas.DrawBitmap(source, subRect, new SKRect(0, 0, subRect.Width, subRect.Height), paint);
		}

		return croppedBitmap;
	}

	/// <summary>
	/// Checks if a bitmap is null or has zero dimensions
	/// </summary>
	public static bool IsNull(this SKBitmap? bitmap)
	{
		return bitmap == null || bitmap.Width == 0 || bitmap.Height == 0;
	}

	/// <summary>
	/// Resizes a bitmap to the specified dimensions
	/// </summary>
	public static SKBitmap Resize(this SKBitmap source, NGWebGal.Types.IVector size, SKFilterQuality quality = SKFilterQuality.Medium)
	{
		var resized = new SKBitmap(size.X, size.Y);
		using (var canvas = new SKCanvas(resized))
		{
			var paint = new SKPaint { FilterQuality = quality };
			canvas.DrawBitmap(source, new SKRect(0, 0, size.X, size.Y), paint);
		}
		return resized;
	}
}
