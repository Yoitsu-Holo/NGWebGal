using System;
using System.IO;

namespace NGWebGal.Audio;

/// <summary>
/// Connection wire between audio nodes specifying source and destination channels.
/// </summary>
public readonly record struct AudioWire(int SourceChannel, int DestinationChannel);

/// <summary>
/// Base interface for all audio processing nodes in the audio graph.
/// Implements a node-based architecture similar to Web Audio API but using NAudio.
/// </summary>
public interface IAudioNode : IDisposable
{
    /// <summary>Number of input channels this node accepts.</summary>
    int InputChannels { get; }

    /// <summary>Number of output channels this node produces.</summary>
    int OutputChannels { get; }

    /// <summary>Connect this node's output to another node's input.</summary>
    void ConnectTo(IAudioNode target, AudioWire wire);

    /// <summary>Disconnect from a target node.</summary>
    void Disconnect(IAudioNode? target = null);

    /// <summary>Get the NAudio sample provider for this node's output.</summary>
    NAudio.Wave.ISampleProvider? GetOutput();
}

/// <summary>
/// Interface for audio nodes that can play/stop audio.
/// </summary>
public interface IAudioSource : IAudioNode
{
    bool IsPlaying { get; }
    bool Loop { get; set; }

    void LoadAudio(byte[] audioData);
    void LoadAudio(Stream audioStream);
    void Play();
    void Stop();
    void Pause();
    void Resume();
}

/// <summary>
/// Interface for audio nodes that process/transform audio.
/// </summary>
public interface IAudioProcessor : IAudioNode
{
    void SetInput(NAudio.Wave.ISampleProvider input);
}

/// <summary>
/// Interface for the final audio output destination.
/// </summary>
public interface IAudioOutput : IAudioNode
{
    void Start();
    void Stop();
}
