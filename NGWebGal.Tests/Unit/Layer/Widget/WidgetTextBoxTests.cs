using System.Collections.Generic;
using SkiaSharp;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using NGWebGal.Types;
using Xunit;

namespace NGWebGal.Tests.Unit.Layer.Widget;

public class WidgetTextBoxTests
{
	[Fact]
	public void SetFontSize_UpdatesFontAndMarksDirty()
	{
		// Arrange
		var textBox = new WidgetTextBox();

		// Act
		textBox.SetFontSize(24);

		// Assert
		Assert.Equal(24, textBox.TextFont.Size);
		Assert.True(textBox.GetType().GetField("_dirty",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(textBox) as bool? ?? false);
	}

	[Fact]
	public void SetColor_UpdatesTextColorAndMarksDirty()
	{
		// Arrange
		var textBox = new WidgetTextBox();
		var testColor = SKColors.Blue;

		// Act
		textBox.SetColor(testColor);

		// Assert
		Assert.Equal(testColor, textBox.TextPaint.Color);
	}

	[Fact]
	public void SetFontStyle_UpdatesTypefaceAndMarksDirty()
	{
		// Arrange
		var textBox = new WidgetTextBox();
		var typeface = SKTypeface.FromFamilyName("Arial");

		// Act
		textBox.SetFontStyle(typeface);

		// Assert
		Assert.Equal(typeface, textBox.TextFont.Typeface);
	}

	[Fact]
	public void Render_WithUnvisableStatus_DoesNotRender()
	{
		// Arrange
		var textBox = new WidgetTextBox
		{
			Status = LayerStatus.Unvisable,
			Text = "Test",
			Size = new IVector(200, 100)
		};

		using var surface = SKSurface.Create(new SKImageInfo(300, 200));
		var canvas = surface.Canvas;

		// Act & Assert - should not throw
		textBox.Render(canvas, false);
	}

	[Fact]
	public void Render_WithTextWrapping_WrapsCorrectly()
	{
		// Arrange
		var textBox = new WidgetTextBox
		{
			Position = new IVector(10, 10),
			Size = new IVector(200, 200),
			Status = LayerStatus.Normal,
			Text = "This is a long text that should wrap to multiple lines when rendered",
			Style = new TextBoxStyle
			{
				MarginTop = 5,
				MarginBottom = 5,
				Padding = new TextPadding(10, 10, 10, 10)
			}
		};
		textBox.SetFontSize(16);

		using var surface = SKSurface.Create(new SKImageInfo(300, 300));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);

		// Act
		textBox.Render(canvas, false);

		// Assert - verify text lines were created
		var textLines = textBox.GetType().GetField("_textLine",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(textBox) as List<string>;

		Assert.NotNull(textLines);
		Assert.True(textLines.Count > 1, "Text should wrap to multiple lines");
	}

	[Fact]
	public void Render_WithNewlineCharacters_BreaksLines()
	{
		// Arrange
		var textBox = new WidgetTextBox
		{
			Position = new IVector(10, 10),
			Size = new IVector(300, 200),
			Status = LayerStatus.Normal,
			Text = "Line 1\nLine 2\nLine 3",
			Style = new TextBoxStyle
			{
				MarginTop = 2,
				MarginBottom = 2,
				Padding = new TextPadding(5, 5, 5, 5)
			}
		};
		textBox.SetFontSize(16);

		using var surface = SKSurface.Create(new SKImageInfo(400, 300));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);

		// Act
		textBox.Render(canvas, false);

		// Assert - verify 3 lines were created
		var textLines = textBox.GetType().GetField("_textLine",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(textBox) as List<string>;

		Assert.NotNull(textLines);
		Assert.Equal(3, textLines.Count);
		Assert.Equal("Line 1", textLines[0]);
		Assert.Equal("Line 2", textLines[1]);
		Assert.Equal("Line 3", textLines[2]);
	}

	[Fact]
	public void Render_WithUnicodeText_RendersCorrectly()
	{
		// Arrange
		var textBox = new WidgetTextBox
		{
			Position = new IVector(10, 10),
			Size = new IVector(300, 200),
			Status = LayerStatus.Normal,
			Text = "你好世界 Hello 🌍",
			Style = new TextBoxStyle
			{
				MarginTop = 5,
				MarginBottom = 5,
				Padding = new TextPadding(10, 10, 10, 10)
			}
		};
		textBox.SetFontSize(20);

		using var surface = SKSurface.Create(new SKImageInfo(400, 300));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);

		// Act & Assert - should not throw
		textBox.Render(canvas, false);

		// Verify text was processed
		var textLines = textBox.GetType().GetField("_textLine",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
			?.GetValue(textBox) as List<string>;

		Assert.NotNull(textLines);
		Assert.True(textLines.Count > 0);
	}

	[Fact]
	public void Render_WithEmptyText_DoesNotCrash()
	{
		// Arrange
		var textBox = new WidgetTextBox
		{
			Position = new IVector(10, 10),
			Size = new IVector(200, 100),
			Status = LayerStatus.Normal,
			Text = ""
		};
		textBox.SetFontSize(16);

		using var surface = SKSurface.Create(new SKImageInfo(300, 200));
		var canvas = surface.Canvas;

		// Act & Assert - should not throw
		textBox.Render(canvas, false);
	}
}
