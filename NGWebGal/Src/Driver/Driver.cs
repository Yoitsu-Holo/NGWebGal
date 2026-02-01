using NGWebGal.Driver.Data;
using NGWebGal.Services;
using NGWebGal.Audio;

namespace NGWebGal.Driver;

/// <summary>
/// Global driver and engine API facade
/// </summary>
public class Driver
{
    private readonly LayoutManager _layoutManager;
    private readonly IResourceManager _resourceManager;
    private readonly AudioManager _audioManager;

    public Driver(
        LayoutManager layoutManager,
        IResourceManager resourceManager,
        AudioManager audioManager)
    {
        _layoutManager = layoutManager;
        _resourceManager = resourceManager;
        _audioManager = audioManager;
    }

    public Response CheckInit()
    {
        Response response = new() { Type = ResponseType.Fail };

        if (_layoutManager is null)
        {
            response.Message = "LayoutManager not set OR Game not loading";
            return response;
        }
        if (_resourceManager is null)
        {
            response.Message = "ResourceManager not set OR Game not loading";
            return response;
        }
        if (_audioManager is null)
        {
            response.Message = "AudioManager not set OR Game not loading";
            return response;
        }

        response.Type = ResponseType.Success;
        return response;
    }

    public LayoutManager Layout => _layoutManager;
    public IResourceManager Resources => _resourceManager;
    public AudioManager Audio => _audioManager;
}
