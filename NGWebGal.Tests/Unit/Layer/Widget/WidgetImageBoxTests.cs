using SkiaSharp;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using NGWebGal.Types;
using NGWebGal.Extend.SkiaSharp;
using Xunit;

namespace NGWebGal.Tests.Unit.Layer.Widget;

public class WidgetImageBoxTests
{
	private SKBitmap CreateTestBitmap(int width, int height, SKColor color)
	{
		var bitmap = new SKBitmap(width, height);
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear(color);
		return bitmap;
	}

	[Fact]
	public void SetImage_UpdatesImageAndMarksDirty()
	{
		// Arrange
		var imageBox = new WidgetImageBox();
		using var testImage = CreateTestBitmap(100, 100, SKColors.Red);

		// Act
		imageBox.SetImage(testImage, 0);

		// Assert
		Assert.True(imageBox.GetType().GetField("_dirty",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(imageBox) as bool? ?? false);
	}

	[Fact]
	public void SetImage_WithSubRectangle_ExtractsCorrectRegion()
	{
		// Arrange
		var imageBox = new WidgetImageBox();
		using var testImage = CreateTestBitmap(200, 200, SKColors.Blue);
		var subRect = new IRect(50, 50, 100, 100);

		// Act
		imageBox.SetImage(testImage, 0, subRect);

		// Assert - verify the image buffer was updated
		var imageBuffer = imageBox.GetType().GetField("_imageBuffer",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(imageBox) as SKBitmap;

		Assert.NotNull(imageBuffer);
		Assert.Equal(100, imageBuffer.Width);
		Assert.Equal(100, imageBuffer.Height);
	}

	[Fact]
	public void Render_WithNullImage_DoesNotRender()
	{
		// Arrange
		var imageBox = new WidgetImageBox
		{
			Size = new IVector(100, 100),
			Status = LayerStatus.Normal
		};

		using var surface = SKSurface.Create(new SKImageInfo(200, 200));
		var canvas = surface.Canvas;

		// Act & Assert - should not throw
		imageBox.Render(canvas, false);
	}

	[Fact]
	public void Render_WithValidImage_RendersImage()
	{
		// Arrange
		var imageBox = new WidgetImageBox
		{
			Position = new IVector(10, 10),
			Size = new IVector(50, 50),
			Status = LayerStatus.Normal
		};

		using var testImage = CreateTestBitmap(100, 100, SKColors.Red);
		imageBox.SetImage(testImage, 0);
		imageBox.ResetAnimationData();

		using var surface = SKSurface.Create(new SKImageInfo(200, 200));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);

		// Act
		imageBox.Render(canvas, false);

		// Assert - verify pixel at center of image
		using var image = surface.Snapshot();
		using var bitmap = SKBitmap.FromImage(image);
		var centerPixel = bitmap.GetPixel(35, 35);

		// Should be red (allowing for anti-aliasing)
		Assert.True(centerPixel.Red > 200);
	}
}
