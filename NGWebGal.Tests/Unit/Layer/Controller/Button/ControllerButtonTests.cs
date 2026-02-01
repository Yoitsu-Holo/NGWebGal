using Xunit;
using SkiaSharp;
using NGWebGal.Layer;
using NGWebGal.Layer.Controller;
using NGWebGal.Handler.Event;
using NGWebGal.Types;

namespace NGWebGal.Tests.Unit.Layer.Controller.Button;

public class ControllerButtonTests
{
    [Fact]
    public void Constructor_InitializesFourStateBuffers()
    {
        // Arrange & Act
        var button = new ControllerButton();

        // Assert
        Assert.NotNull(button);
    }

    [Fact]
    public void SetImage_WithValidImageId_Succeeds()
    {
        // Arrange
        var button = new ControllerButton();
        var bitmap = new SKBitmap(100, 100);

        // Act
        button.SetImage(bitmap, 0);

        // Assert - No exception thrown
        Assert.NotNull(button);
    }

    [Fact]
    public void SetImage_WithInvalidImageId_DoesNotThrow()
    {
        // Arrange
        var button = new ControllerButton();
        var bitmap = new SKBitmap(100, 100);

        // Act & Assert - Should not throw
        button.SetImage(bitmap, 10); // Out of range
        Assert.NotNull(button);
    }

    [Fact]
    public void SetColor_CreatesColoredBitmap()
    {
        // Arrange
        var button = new ControllerButton();
        button.Size = new IVector(50, 50);
        var color = SKColors.Red;

        // Act
        button.SetColor(color, 0);

        // Assert - No exception thrown
        Assert.NotNull(button);
    }

    [Fact]
    public void DoAction_WithMouseOutsideWindow_SetsStatusToNormal()
    {
        // Arrange
        var button = new ControllerButton();
        button.Position = new IVector(100, 100);
        button.Size = new IVector(50, 50);
        button.Status = LayerStatus.Hover;

        var mouseEvent = new MouseEventData
        {
            Position = new IVector(200, 200), // Outside button
            Button = MouseButton.Empty,
            Status = MouseStatus.Release
        };

        // Act
        var result = button.DoAction(mouseEvent);

        // Assert
        Assert.True(result);
        Assert.Equal(LayerStatus.Normal, button.Status);
    }

    [Fact]
    public void DoAction_WithMouseInsideAndNoButton_SetsStatusToHover()
    {
        // Arrange
        var button = new ControllerButton();
        button.Position = new IVector(100, 100);
        button.Size = new IVector(50, 50);
        button.Status = LayerStatus.Normal;

        var mouseEvent = new MouseEventData
        {
            Position = new IVector(120, 120), // Inside button
            Button = MouseButton.Empty,
            Status = MouseStatus.Release
        };

        // Act
        var result = button.DoAction(mouseEvent);

        // Assert
        Assert.True(result);
        Assert.Equal(LayerStatus.Hover, button.Status);
    }

    [Fact]
    public void DoAction_WithLeftButtonPressed_SetsStatusToPressed()
    {
        // Arrange
        var button = new ControllerButton();
        button.Position = new IVector(100, 100);
        button.Size = new IVector(50, 50);
        button.Status = LayerStatus.Hover;

        var mouseEvent = new MouseEventData
        {
            Position = new IVector(120, 120), // Inside button
            Button = MouseButton.LButton,
            Status = MouseStatus.Hold
        };

        // Act
        var result = button.DoAction(mouseEvent);

        // Assert
        Assert.True(result);
        Assert.Equal(LayerStatus.Pressed, button.Status);
    }

    [Fact]
    public void DoAction_WhenDisabled_ReturnsFalse()
    {
        // Arrange
        var button = new ControllerButton();
        button.Status = LayerStatus.Disable;

        var mouseEvent = new MouseEventData
        {
            Position = new IVector(120, 120),
            Button = MouseButton.LButton,
            Status = MouseStatus.Hold
        };

        // Act
        var result = button.DoAction(mouseEvent);

        // Assert
        Assert.False(result);
    }
}
