using SkiaSharp;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using NGWebGal.Types;
using Xunit;

namespace NGWebGal.Tests.Unit.Layer.Widget;

public class WidgetColorBoxTests
{
	[Fact]
	public void SetColor_UpdatesColorAndMarksDirty()
	{
		// Arrange
		var colorBox = new WidgetColorBox();
		var testColor = SKColors.Red;

		// Act
		colorBox.SetColor(testColor);

		// Assert
		Assert.True(colorBox.GetType().GetField("_dirty",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(colorBox) as bool? ?? false);
	}

	[Fact]
	public void Render_WithUnvisableStatus_DoesNotRender()
	{
		// Arrange
		var colorBox = new WidgetColorBox
		{
			Status = LayerStatus.Unvisable,
			Size = new IVector(100, 100)
		};
		colorBox.SetColor(SKColors.Blue);

		using var surface = SKSurface.Create(new SKImageInfo(200, 200));
		var canvas = surface.Canvas;

		// Act
		colorBox.Render(canvas, false);

		// Assert - should not throw and should complete without rendering
		Assert.Equal(LayerStatus.Unvisable, colorBox.Status);
	}

	[Fact]
	public void Render_WithValidColor_RendersRectangle()
	{
		// Arrange
		var colorBox = new WidgetColorBox
		{
			Position = new IVector(10, 10),
			Size = new IVector(50, 50),
			Status = LayerStatus.Normal
		};
		colorBox.SetColor(SKColors.Green);
		colorBox.ResetAnimationData();

		using var surface = SKSurface.Create(new SKImageInfo(200, 200));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);

		// Act
		colorBox.Render(canvas, false);

		// Assert - verify pixel at center of rectangle
		using var image = surface.Snapshot();
		using var bitmap = SKBitmap.FromImage(image);
		var centerPixel = bitmap.GetPixel(35, 35);

		// Should be green (SKColors.Green is 0,128,0)
		System.Console.WriteLine($"Pixel at (35,35): R={centerPixel.Red}, G={centerPixel.Green}, B={centerPixel.Blue}, A={centerPixel.Alpha}");
		Assert.True(centerPixel.Green > 100, $"Expected green > 100, got {centerPixel.Green}");
		Assert.True(centerPixel.Red < 50, $"Expected red < 50, got {centerPixel.Red}");
		Assert.True(centerPixel.Blue < 50, $"Expected blue < 50, got {centerPixel.Blue}");
	}
}
