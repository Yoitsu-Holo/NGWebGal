using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace NGWebGal.Audio;

/// <summary>
/// Audio multiplexer node for mixing multiple audio inputs.
/// Equivalent to ChannelMergerNode in Web Audio API.
/// </summary>
public class AudioMultiplexer : AudioNodeBase, IAudioProcessor
{
    private readonly List<ISampleProvider?> _inputs = [];
    private int _inputChannelCount = 6;
    private int _outputChannelCount = 6;

    public override int InputChannels => _inputChannelCount;
    public override int OutputChannels => _outputChannelCount;

    public AudioMultiplexer(int inputChannels = 6, int outputChannels = 6)
    {
        _inputChannelCount = inputChannels;
        _outputChannelCount = outputChannels;

        for (int i = 0; i < inputChannels; i++)
            _inputs.Add(null);
    }

    public void SetInput(ISampleProvider input)
    {
        SetInput(input, 0);
    }

    public void SetInput(ISampleProvider input, int channel)
    {
        if (channel >= _inputChannelCount)
            throw new ArgumentException($"Channel {channel} exceeds input channels {_inputChannelCount}");

        while (_inputs.Count <= channel)
            _inputs.Add(null);

        _inputs[channel] = input;
    }

    public override ISampleProvider? GetOutput()
    {
        var activeInputs = _inputs.Where(i => i != null).ToList();
        return activeInputs.Count > 0 ? new MixingSampleProvider(activeInputs!) : null;
    }
}

/// <summary>
/// Sample provider that mixes multiple audio sources together.
/// </summary>
internal class MixingSampleProvider : ISampleProvider
{
    private readonly List<ISampleProvider> _sources;
    private readonly float[][] _sourceBuffers;

    public WaveFormat WaveFormat { get; }

    public MixingSampleProvider(List<ISampleProvider> sources)
    {
        if (sources.Count == 0)
            throw new ArgumentException("At least one source required");

        _sources = sources;
        WaveFormat = sources[0].WaveFormat;

        _sourceBuffers = new float[sources.Count][];
        for (int i = 0; i < sources.Count; i++)
            _sourceBuffers[i] = new float[WaveFormat.SampleRate];
    }

    public int Read(float[] buffer, int offset, int count)
    {
        Array.Clear(buffer, offset, count);

        int maxRead = 0;
        for (int i = 0; i < _sources.Count; i++)
        {
            int read = _sources[i].Read(_sourceBuffers[i], 0, count);
            maxRead = Math.Max(maxRead, read);

            for (int j = 0; j < read; j++)
                buffer[offset + j] += _sourceBuffers[i][j];
        }

        return maxRead;
    }
}
