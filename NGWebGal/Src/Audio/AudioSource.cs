using System;
using System.IO;
using NAudio.Wave;

namespace NGWebGal.Audio;

/// <summary>
/// Audio source node that loads and plays audio files.
/// Equivalent to AudioBufferSourceNode in Web Audio API.
/// </summary>
public class AudioSource : AudioNodeBase, IAudioSource
{
    private AudioFileReader? _audioReader;
    private LoopStream? _loopProvider;
    private bool _isPlaying;
    private bool _loop;

    public override int InputChannels => 0;
    public override int OutputChannels => 2;

    public bool IsPlaying => _isPlaying;

    public bool Loop
    {
        get => _loop;
        set
        {
            _loop = value;
            if (_loopProvider != null)
                _loopProvider.EnableLooping = value;
        }
    }

    public void LoadAudio(byte[] audioData)
    {
        using var ms = new MemoryStream(audioData);
        LoadAudio(ms);
    }

    public void LoadAudio(Stream audioStream)
    {
        Stop();
        _audioReader?.Dispose();

        var ms = new MemoryStream();
        audioStream.CopyTo(ms);
        ms.Position = 0;

        // Use WaveFileReader for stream-based loading, then wrap in SampleChannel
        var waveStream = new WaveFileReader(ms);
        var sampleProvider = waveStream.ToSampleProvider();
        _loopProvider = new LoopStream(sampleProvider) { EnableLooping = _loop };
    }

    public void Play()
    {
        if (_audioReader == null) return;
        _audioReader.Position = 0;
        _isPlaying = true;
    }

    public void Stop()
    {
        if (_audioReader == null) return;
        _audioReader.Position = 0;
        _isPlaying = false;
    }

    public void Pause()
    {
        _isPlaying = false;
    }

    public void Resume()
    {
        if (_audioReader != null)
            _isPlaying = true;
    }

    public override ISampleProvider? GetOutput()
    {
        return _isPlaying ? _loopProvider : null;
    }

    public override void Dispose()
    {
        if (_disposed) return;
        Stop();
        _audioReader?.Dispose();
        base.Dispose();
    }
}

/// <summary>
/// Helper class to enable looping of audio streams.
/// </summary>
internal class LoopStream : ISampleProvider
{
    private readonly ISampleProvider _source;

    public bool EnableLooping { get; set; }
    public WaveFormat WaveFormat => _source.WaveFormat;

    public LoopStream(ISampleProvider source)
    {
        _source = source;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = _source.Read(buffer, offset + totalRead, count - totalRead);
            if (read == 0)
            {
                if (!EnableLooping) break;

                // Reset position for looping
                if (_source is AudioFileReader reader)
                    reader.Position = 0;
                else
                    break;
            }
            totalRead += read;
        }
        return totalRead;
    }
}
