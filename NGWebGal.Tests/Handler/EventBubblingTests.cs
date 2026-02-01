using System;
using Xunit;
using NGWebGal.Handler;
using NGWebGal.Layer;
using NGWebGal.Types;
using GameMouseButton = NGWebGal.Handler.Event.MouseButton;
using GameMouseStatus = NGWebGal.Handler.Event.MouseStatus;
using MouseEventData = NGWebGal.Handler.Event.MouseEventData;

namespace NGWebGal.Tests.Handler;

public class EventBubblingTests
{
    private class TestLayer : LayerBase
    {
        public bool EventReceived { get; private set; }
        public MouseEventData? LastMouseEvent { get; private set; }

        public override bool DoAction(EventArgs eventArgs)
        {
            if (eventArgs is MouseEventData mouseEvent)
            {
                EventReceived = true;
                LastMouseEvent = mouseEvent;
                return true; // Event handled
            }
            return false;
        }
    }

    [Fact]
    public void Layout_ProcessEvent_ShouldBubbleThroughLayers()
    {
        // Arrange
        var layout = new Layout();
        var layer1 = new TestLayer();
        var layer2 = new TestLayer();

        layout.Layers[0] = layer1;
        layout.Layers[1] = layer2;

        var mouseEvent = new MouseEventData
        {
            Position = new IVector(100, 100),
            Move = new IVector(0, 0),
            Button = GameMouseButton.LButton,
            Status = GameMouseStatus.Down
        };

        // Act
        layout.ProcessEvent(mouseEvent);

        // Assert
        Assert.True(layer1.EventReceived);
        Assert.NotNull(layer1.LastMouseEvent);
        Assert.Equal(100, layer1.LastMouseEvent.Position.X);
    }

    [Fact]
    public void Layout_ProcessEvent_ShouldPropagateToAllLayersIfNotHandled()
    {
        // Arrange
        var layout = new Layout();
        var layer1 = new TestLayer();
        var layer2 = new TestLayer();

        // Override to not handle event
        layer1 = new TestLayer();
        layer2 = new TestLayer();

        layout.Layers[0] = layer1;
        layout.Layers[1] = layer2;

        var mouseEvent = new MouseEventData
        {
            Position = new IVector(50, 50),
            Button = GameMouseButton.RButton,
            Status = GameMouseStatus.Click
        };

        // Act
        layout.ProcessEvent(mouseEvent);

        // Assert - first layer handles it, second doesn't receive
        Assert.True(layer1.EventReceived);
    }
}
