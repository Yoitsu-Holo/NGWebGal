using System;
using Xunit;
using NGWebGal.Handler;
using NGWebGal.Layer;
using NGWebGal.Types;
using Avalonia.Input;
using GameMouseButton = NGWebGal.Handler.Event.MouseButton;
using GameMouseStatus = NGWebGal.Handler.Event.MouseStatus;
using GameKeyModifiers = NGWebGal.Handler.Event.KeyModifiers;
using GameKeyStatus = NGWebGal.Handler.Event.KeyStatus;
using MouseEventData = NGWebGal.Handler.Event.MouseEventData;
using KeyboardEventData = NGWebGal.Handler.Event.KeyboardEventData;

namespace NGWebGal.Tests.Handler;

public class EventSystemTests
{
    [Fact]
    public void MouseEventData_ShouldStorePositionAndButton()
    {
        // Arrange & Act
        var eventData = new MouseEventData
        {
            Position = new IVector(100, 200),
            Move = new IVector(10, 20),
            Button = GameMouseButton.LButton,
            Status = GameMouseStatus.Down
        };

        // Assert
        Assert.Equal(100, eventData.Position.X);
        Assert.Equal(200, eventData.Position.Y);
        Assert.Equal(10, eventData.Move.X);
        Assert.Equal(20, eventData.Move.Y);
        Assert.Equal(GameMouseButton.LButton, eventData.Button);
        Assert.Equal(GameMouseStatus.Down, eventData.Status);
    }

    [Fact]
    public void KeyboardEventData_ShouldStoreKeyAndModifiers()
    {
        // Arrange & Act
        var eventData = new KeyboardEventData
        {
            Key = Key.A,
            Status = GameKeyStatus.Down,
            Modifiers = GameKeyModifiers.Control | GameKeyModifiers.Shift,
            Text = "A"
        };

        // Assert
        Assert.Equal(Key.A, eventData.Key);
        Assert.Equal(GameKeyStatus.Down, eventData.Status);
        Assert.True(eventData.Modifiers.HasFlag(GameKeyModifiers.Control));
        Assert.True(eventData.Modifiers.HasFlag(GameKeyModifiers.Shift));
        Assert.Equal("A", eventData.Text);
    }
}
