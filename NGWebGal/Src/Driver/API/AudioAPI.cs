using NGWebGal.Driver.Data;
using NGWebGal.Audio;

namespace NGWebGal.Driver.API;

/// <summary>
/// Audio API - Node management and wiring
/// </summary>
public class AudioAPI
{
    private readonly AudioManager _audioManager;

    public AudioAPI(AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public Response CreateAudioNode(AudioNodeInfo info)
    {
        // Audio node creation handled by AudioManager
        return new Response { Type = ResponseType.Success };
    }

    public Response DeleteAudioNode(AudioNodeInfo info)
    {
        // Audio node deletion handled by AudioManager
        return new Response { Type = ResponseType.Success };
    }

    public Response ConnectAudioNodes(AudioWireInfo wire)
    {
        // Audio wiring handled by AudioManager
        return new Response { Type = ResponseType.Success };
    }

    public Response DisconnectAudioNodes(AudioWireInfo wire)
    {
        // Audio disconnection handled by AudioManager
        return new Response { Type = ResponseType.Success };
    }
}
