using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace NGWebGal.Audio;

/// <summary>
/// Base class for all audio nodes providing common functionality.
/// </summary>
public abstract class AudioNodeBase : IAudioNode
{
    protected readonly List<IAudioNode> _connectedNodes = [];
    protected bool _disposed;

    public abstract int InputChannels { get; }
    public abstract int OutputChannels { get; }

    public virtual void ConnectTo(IAudioNode target, AudioWire wire)
    {
        if (wire.SourceChannel >= OutputChannels)
            throw new ArgumentException($"Source channel {wire.SourceChannel} exceeds output channels {OutputChannels}");
        if (wire.DestinationChannel >= target.InputChannels)
            throw new ArgumentException($"Destination channel {wire.DestinationChannel} exceeds input channels {target.InputChannels}");

        if (!_connectedNodes.Contains(target))
            _connectedNodes.Add(target);

        if (target is IAudioProcessor processor)
        {
            var output = GetOutput();
            if (output != null)
                processor.SetInput(output);
        }
    }

    public virtual void Disconnect(IAudioNode? target = null)
    {
        if (target == null)
            _connectedNodes.Clear();
        else
            _connectedNodes.Remove(target);
    }

    public abstract ISampleProvider? GetOutput();

    public virtual void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connectedNodes.Clear();
        GC.SuppressFinalize(this);
    }
}
