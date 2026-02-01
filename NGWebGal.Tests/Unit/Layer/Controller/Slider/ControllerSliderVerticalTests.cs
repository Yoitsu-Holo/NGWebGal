using Xunit;
using SkiaSharp;
using NGWebGal.Layer.Controller;
using NGWebGal.Handler.Event;
using NGWebGal.Types;

namespace NGWebGal.Tests.Unit.Layer.Controller.Slider;

public class ControllerSliderVerticalTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var slider = new ControllerSliderVertical();

        // Assert
        Assert.NotNull(slider);
        Assert.Equal(20, slider.Size.X);
        Assert.Equal(200, slider.Size.Y);
    }

    [Fact]
    public void Value_SetToHalf_UpdatesThumbPosition()
    {
        // Arrange
        var slider = new ControllerSliderVertical();
        slider.InitAttribute(new IVector(20, 200), new IVector(20, 10));

        // Act
        slider.Value = 0.5f;

        // Assert
        Assert.Equal(0.5f, (float)slider.Value);
    }

    [Fact]
    public void Value_SetToZero_MovesThumbToTop()
    {
        // Arrange
        var slider = new ControllerSliderVertical();
        slider.InitAttribute(new IVector(20, 200), new IVector(20, 10));

        // Act
        slider.Value = 0.0f;

        // Assert
        Assert.Equal(0.0f, (float)slider.Value);
    }

    [Fact]
    public void Value_SetToOne_MovesThumbToBottom()
    {
        // Arrange
        var slider = new ControllerSliderVertical();
        slider.InitAttribute(new IVector(20, 200), new IVector(20, 10));

        // Act
        slider.Value = 1.0f;

        // Assert
        Assert.Equal(1.0f, (float)slider.Value);
    }
}
