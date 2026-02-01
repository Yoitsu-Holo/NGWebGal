using System;
using Xunit;
using NGWebGal.Handler;
using NGWebGal.Layer;
using NGWebGal.Types;
using GameMouseButton = NGWebGal.Handler.Event.MouseButton;
using GameMouseStatus = NGWebGal.Handler.Event.MouseStatus;
using MouseEventData = NGWebGal.Handler.Event.MouseEventData;

namespace NGWebGal.Tests.Handler;

public class EventBubblingTests_Part2
{
    private class TestLayer : LayerBase
    {
        public bool ShouldHandle { get; set; } = false;
        public bool EventReceived { get; private set; }

        public override bool DoAction(EventArgs eventArgs)
        {
            EventReceived = true;
            return ShouldHandle;
        }
    }

    [Fact]
    public void Layout_ProcessEvent_ShouldStopBubblingWhenHandled()
    {
        // Arrange
        var layout = new Layout();
        var layer1 = new TestLayer { ShouldHandle = true };
        var layer2 = new TestLayer { ShouldHandle = false };

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

        // Assert
        Assert.True(layer1.EventReceived);
        Assert.False(layer2.EventReceived); // Should not receive event
    }
}
