using System;
using NAudio.Wave;

namespace NGWebGal.Audio;

/// <summary>
/// Audio speaker (output) node that plays audio to the system audio device.
/// Equivalent to AudioDestinationNode in Web Audio API.
/// </summary>
public class AudioSpeaker : AudioNodeBase, IAudioOutput
{
    private IWavePlayer? _waveOut;
    private ISampleProvider? _input;
    private bool _isStarted;

    public override int InputChannels => 2;
    public override int OutputChannels => 0;

    public void SetInput(ISampleProvider input)
    {
        _input = input;
    }

    public void Start()
    {
        if (_isStarted || _input == null) return;

        _waveOut = new WaveOutEvent();
        _waveOut.Init(_input);
        _waveOut.Play();
        _isStarted = true;
    }

    public void Stop()
    {
        if (!_isStarted) return;

        _waveOut?.Stop();
        _waveOut?.Dispose();
        _waveOut = null;
        _isStarted = false;
    }

    public override ISampleProvider? GetOutput()
    {
        return null; // Speaker is the final destination
    }

    public override void Dispose()
    {
        if (_disposed) return;
        Stop();
        base.Dispose();
    }
}
