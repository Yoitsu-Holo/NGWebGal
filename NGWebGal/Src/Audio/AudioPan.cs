using System;
using NAudio.Wave;

namespace NGWebGal.Audio;

/// <summary>
/// Audio panning node for stereo positioning.
/// Equivalent to StereoPannerNode in Web Audio API.
/// </summary>
public class AudioPan : AudioNodeBase, IAudioProcessor
{
    private ISampleProvider? _input;
    private float _pan = 0.0f;

    public override int InputChannels => 2;
    public override int OutputChannels => 2;

    /// <summary>
    /// Gets or sets the pan value (-1.0 = full left, 0.0 = center, 1.0 = full right).
    /// </summary>
    public float Pan
    {
        get => _pan;
        set => _pan = Math.Clamp(value, -1.0f, 1.0f);
    }

    public void SetInput(ISampleProvider input)
    {
        _input = input;
    }

    public override ISampleProvider? GetOutput()
    {
        return _input != null ? new PanningSampleProvider(_input, _pan) : null;
    }
}

/// <summary>
/// Sample provider that applies stereo panning to audio samples.
/// </summary>
internal class PanningSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly float _pan;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public PanningSampleProvider(ISampleProvider source, float pan)
    {
        _source = source;
        _pan = Math.Clamp(pan, -1.0f, 1.0f);
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);

        if (WaveFormat.Channels == 2 && Math.Abs(_pan) > 0.001f)
        {
            float leftGain = _pan <= 0 ? 1.0f : 1.0f - _pan;
            float rightGain = _pan >= 0 ? 1.0f : 1.0f + _pan;

            for (int i = 0; i < samplesRead; i += 2)
            {
                buffer[offset + i] *= leftGain;         // Left channel
                buffer[offset + i + 1] *= rightGain;    // Right channel
            }
        }

        return samplesRead;
    }
}
