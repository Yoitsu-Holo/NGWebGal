using System;
using System.IO;
using Xunit;
using NGWebGal.Audio;

namespace NGWebGal.Tests.Unit;

/// <summary>
/// Integration tests for the NAudio-based audio system.
/// Tests audio nodes, connections, and the AudioManager.
/// </summary>
public class AudioSystemTests : IDisposable
{
    private readonly AudioManager _audioManager;

    public AudioSystemTests()
    {
        _audioManager = new AudioManager();
    }

    [Fact]
    public void AudioSource_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var source = new AudioSource();

        // Assert
        Assert.Equal(0, source.InputChannels);
        Assert.Equal(2, source.OutputChannels);
        Assert.False(source.IsPlaying);
        Assert.False(source.Loop);
    }

    [Fact]
    public void AudioGain_ShouldControlVolume()
    {
        // Arrange
        using var gain = new AudioGain();

        // Act
        gain.Gain = 0.5f;

        // Assert
        Assert.Equal(0.5f, gain.Gain);
        Assert.Equal(2, gain.InputChannels);
        Assert.Equal(2, gain.OutputChannels);
    }

    [Fact]
    public void AudioPan_ShouldControlStereoPosition()
    {
        // Arrange
        using var pan = new AudioPan();

        // Act
        pan.Pan = -0.5f;

        // Assert
        Assert.Equal(-0.5f, pan.Pan);
        Assert.Equal(2, pan.InputChannels);
        Assert.Equal(2, pan.OutputChannels);
    }

    [Fact]
    public void AudioMultiplexer_ShouldSupportMultipleChannels()
    {
        // Arrange & Act
        using var multiplexer = new AudioMultiplexer(inputChannels: 4, outputChannels: 2);

        // Assert
        Assert.Equal(4, multiplexer.InputChannels);
        Assert.Equal(2, multiplexer.OutputChannels);
    }

    [Fact]
    public void AudioSpeaker_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        using var speaker = new AudioSpeaker();

        // Assert
        Assert.Equal(2, speaker.InputChannels);
        Assert.Equal(0, speaker.OutputChannels);
    }

    [Fact]
    public void AudioNodes_ShouldConnectCorrectly()
    {
        // Arrange
        using var source = new AudioSource();
        using var gain = new AudioGain();

        // Act
        source.ConnectTo(gain, new AudioWire(0, 0));

        // Assert - No exception means connection succeeded
        Assert.NotNull(source);
        Assert.NotNull(gain);
    }

    [Fact]
    public void AudioManager_ShouldInitializeWithDefaultVolume()
    {
        // Assert
        Assert.Equal(1.0f, _audioManager.MasterVolume);
    }

    [Fact]
    public void AudioManager_ShouldSetMasterVolume()
    {
        // Act
        _audioManager.MasterVolume = 0.7f;

        // Assert
        Assert.Equal(0.7f, _audioManager.MasterVolume);
    }

    [Fact]
    public void AudioManager_ShouldStopAllAudio()
    {
        // Act
        _audioManager.StopAll();

        // Assert - No exception means operation succeeded
        Assert.NotNull(_audioManager);
    }

    public void Dispose()
    {
        _audioManager?.Dispose();
    }
}
