using System;
using NGWebGal.Driver.Data;
using NGWebGal.Services;

namespace NGWebGal.Driver.API;

/// <summary>
/// File API - Resource loading
/// </summary>
public class FileAPI
{
    private readonly IResourceManager _resourceManager;

    public FileAPI(IResourceManager resourceManager)
    {
        _resourceManager = resourceManager;
    }

    public Response LoadFile(Data.FileInfo fileInfo)
    {
        try
        {
            switch (fileInfo.Type)
            {
                case FileType.Image:
                    _resourceManager.LoadImage(fileInfo.URL);
                    break;
                case FileType.Audio:
                    _resourceManager.LoadAudio(fileInfo.URL);
                    break;
                case FileType.Font:
                    _resourceManager.LoadFont(fileInfo.URL);
                    break;
                case FileType.Script:
                    _resourceManager.LoadScript(fileInfo.URL);
                    break;
                default:
                    return new Response($"Unsupported file type: {fileInfo.Type}", ResponseType.Fail);
            }

            return new Response { Type = ResponseType.Success };
        }
        catch (Exception ex)
        {
            return new Response($"Failed to load file: {ex.Message}", ResponseType.Fail);
        }
    }

    public Response UnloadFile(Data.FileInfo fileInfo)
    {
        _resourceManager.RemoveFromCache(fileInfo.URL);
        return new Response { Type = ResponseType.Success };
    }
}
