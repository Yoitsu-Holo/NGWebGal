using System;
using NAudio.Wave;

namespace NGWebGal.Audio;

/// <summary>
/// Audio gain (volume) control node.
/// Equivalent to GainNode in Web Audio API.
/// </summary>
public class AudioGain : AudioNodeBase, IAudioProcessor
{
    private ISampleProvider? _input;
    private float _gain = 1.0f;

    public override int InputChannels => 2;
    public override int OutputChannels => 2;

    /// <summary>
    /// Gets or sets the gain value (0.0 = silent, 1.0 = original volume, >1.0 = amplified).
    /// </summary>
    public float Gain
    {
        get => _gain;
        set => _gain = Math.Max(0, value);
    }

    public void SetInput(ISampleProvider input)
    {
        _input = input;
    }

    public override ISampleProvider? GetOutput()
    {
        return _input != null ? new GainSampleProvider(_input, _gain) : null;
    }
}

/// <summary>
/// Sample provider that applies gain to audio samples.
/// </summary>
internal class GainSampleProvider : ISampleProvider
{
    private readonly ISampleProvider _source;
    private readonly float _gain;

    public WaveFormat WaveFormat => _source.WaveFormat;

    public GainSampleProvider(ISampleProvider source, float gain)
    {
        _source = source;
        _gain = gain;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = _source.Read(buffer, offset, count);

        if (Math.Abs(_gain - 1.0f) > 0.001f)
        {
            for (int i = 0; i < samplesRead; i++)
            {
                buffer[offset + i] *= _gain;
            }
        }

        return samplesRead;
    }
}
