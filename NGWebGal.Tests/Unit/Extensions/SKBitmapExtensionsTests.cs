using SkiaSharp;
using NGWebGal.Extend.SkiaSharp;
using Xunit;

namespace NGWebGal.Tests.Unit.Extensions;

public class SKBitmapExtensionsTests
{
	private SKBitmap CreateTestBitmap(int width, int height, SKColor color)
	{
		var bitmap = new SKBitmap(width, height);
		using var canvas = new SKCanvas(bitmap);
		canvas.Clear(color);
		return bitmap;
	}

	[Fact]
	public void SubBitmap_WithValidRect_ExtractsCorrectRegion()
	{
		// Arrange
		using var source = CreateTestBitmap(200, 200, SKColors.Blue);
		var subRect = new SKRectI(50, 50, 150, 150);

		// Act
		using var result = source.SubBitmap(subRect);

		// Assert
		Assert.Equal(100, result.Width);
		Assert.Equal(100, result.Height);
		Assert.Equal(SKColors.Blue, result.GetPixel(50, 50));
	}

	[Fact]
	public void SubBitmap_WithZeroRect_ReturnsFullImage()
	{
		// Arrange
		using var source = CreateTestBitmap(100, 100, SKColors.Red);
		var subRect = new SKRectI(0, 0, 0, 0);

		// Act
		using var result = source.SubBitmap(subRect);

		// Assert
		Assert.Equal(100, result.Width);
		Assert.Equal(100, result.Height);
	}

	[Fact]
	public void IsNull_WithNullBitmap_ReturnsTrue()
	{
		// Arrange
		SKBitmap? bitmap = null;

		// Act
		var result = bitmap.IsNull();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsNull_WithZeroDimensions_ReturnsTrue()
	{
		// Arrange
		using var bitmap = new SKBitmap(0, 0);

		// Act
		var result = bitmap.IsNull();

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void IsNull_WithValidBitmap_ReturnsFalse()
	{
		// Arrange
		using var bitmap = CreateTestBitmap(100, 100, SKColors.Green);

		// Act
		var result = bitmap.IsNull();

		// Assert
		Assert.False(result);
	}
}
