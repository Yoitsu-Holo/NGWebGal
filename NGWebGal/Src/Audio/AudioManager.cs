using System;
using System.Collections.Generic;
using System.IO;

namespace NGWebGal.Audio;

/// <summary>
/// Manages audio playback and audio graph connections.
/// Provides high-level API for game audio operations.
/// </summary>
public class AudioManager : IDisposable
{
    private readonly Dictionary<string, AudioSource> _audioSources = [];
    private readonly AudioMultiplexer _mixer;
    private readonly AudioGain _masterGain;
    private readonly AudioSpeaker _speaker;
    private bool _disposed;

    public float MasterVolume
    {
        get => _masterGain.Gain;
        set => _masterGain.Gain = value;
    }

    public AudioManager()
    {
        // Create audio graph: Sources -> Mixer -> MasterGain -> Speaker
        _mixer = new AudioMultiplexer(inputChannels: 8, outputChannels: 2);
        _masterGain = new AudioGain { Gain = 1.0f };
        _speaker = new AudioSpeaker();

        // Connect nodes
        _mixer.ConnectTo(_masterGain, new AudioWire(0, 0));
        _masterGain.ConnectTo(_speaker, new AudioWire(0, 0));
    }

    /// <summary>
    /// Load audio from byte array and assign it a unique ID.
    /// </summary>
    public void LoadAudio(string id, byte[] audioData)
    {
        if (_audioSources.ContainsKey(id))
        {
            _audioSources[id].Dispose();
            _audioSources.Remove(id);
        }

        var source = new AudioSource();
        source.LoadAudio(audioData);
        _audioSources[id] = source;
    }

    /// <summary>
    /// Load audio from stream and assign it a unique ID.
    /// </summary>
    public void LoadAudio(string id, Stream audioStream)
    {
        if (_audioSources.ContainsKey(id))
        {
            _audioSources[id].Dispose();
            _audioSources.Remove(id);
        }

        var source = new AudioSource();
        source.LoadAudio(audioStream);
        _audioSources[id] = source;
    }

    /// <summary>
    /// Play audio by ID with optional looping.
    /// </summary>
    public void Play(string id, bool loop = false, float volume = 1.0f)
    {
        if (!_audioSources.TryGetValue(id, out var source))
            return;

        source.Loop = loop;

        // Create gain node for individual volume control
        var gain = new AudioGain { Gain = volume };
        source.ConnectTo(gain, new AudioWire(0, 0));
        gain.ConnectTo(_mixer, new AudioWire(0, 0));

        source.Play();
        _speaker.Start();
    }

    /// <summary>
    /// Stop audio playback by ID.
    /// </summary>
    public void Stop(string id)
    {
        if (_audioSources.TryGetValue(id, out var source))
            source.Stop();
    }

    /// <summary>
    /// Pause audio playback by ID.
    /// </summary>
    public void Pause(string id)
    {
        if (_audioSources.TryGetValue(id, out var source))
            source.Pause();
    }

    /// <summary>
    /// Resume audio playback by ID.
    /// </summary>
    public void Resume(string id)
    {
        if (_audioSources.TryGetValue(id, out var source))
            source.Resume();
    }

    /// <summary>
    /// Stop all audio playback.
    /// </summary>
    public void StopAll()
    {
        foreach (var source in _audioSources.Values)
            source.Stop();
    }

    /// <summary>
    /// Unload audio by ID and free resources.
    /// </summary>
    public void UnloadAudio(string id)
    {
        if (_audioSources.TryGetValue(id, out var source))
        {
            source.Dispose();
            _audioSources.Remove(id);
        }
    }

    /// <summary>
    /// Check if audio is currently playing.
    /// </summary>
    public bool IsPlaying(string id)
    {
        return _audioSources.TryGetValue(id, out var source) && source.IsPlaying;
    }

    public void Dispose()
    {
        if (_disposed) return;

        StopAll();
        foreach (var source in _audioSources.Values)
            source.Dispose();

        _audioSources.Clear();
        _mixer.Dispose();
        _masterGain.Dispose();
        _speaker.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
