using Xunit;
using SkiaSharp;
using NGWebGal.Layer.Controller;
using NGWebGal.Handler.Event;
using NGWebGal.Types;

namespace NGWebGal.Tests.Unit.Layer.Controller.Slider;

public class ControllerSliderHorizontalTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var slider = new ControllerSliderHorizontal();

        // Assert
        Assert.NotNull(slider);
        Assert.Equal(200, slider.Size.X);
        Assert.Equal(20, slider.Size.Y);
    }

    [Fact]
    public void ThumbLimitSet_WithValidDelta_UpdatesThumbPositionAndValue()
    {
        // Arrange
        var slider = new ControllerSliderHorizontal();
        slider.InitAttribute(new IVector(200, 20), new IVector(10, 20));

        // Act
        slider.Value = 0.5f;

        // Assert
        Assert.Equal(0.5f, (float)slider.Value);
    }

    [Fact]
    public void Value_SetToZero_MovesThumbToStart()
    {
        // Arrange
        var slider = new ControllerSliderHorizontal();
        slider.InitAttribute(new IVector(200, 20), new IVector(10, 20));

        // Act
        slider.Value = 0.0f;

        // Assert
        Assert.Equal(0.0f, (float)slider.Value);
    }

    [Fact]
    public void Value_SetToOne_MovesThumbToEnd()
    {
        // Arrange
        var slider = new ControllerSliderHorizontal();
        slider.InitAttribute(new IVector(200, 20), new IVector(10, 20));

        // Act
        slider.Value = 1.0f;

        // Assert
        Assert.Equal(1.0f, (float)slider.Value);
    }
}
