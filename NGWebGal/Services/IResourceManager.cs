using SkiaSharp;

namespace NGWebGal.Services;

/// <summary>
/// Interface for managing game resources (images, fonts, audio, scripts)
/// </summary>
public interface IResourceManager
{
    /// <summary>
    /// Load an image from file
    /// </summary>
    SKBitmap? LoadImage(string path);

    /// <summary>
    /// Load a font from file
    /// </summary>
    SKTypeface? LoadFont(string path);

    /// <summary>
    /// Load audio data from file
    /// </summary>
    byte[]? LoadAudio(string path);

    /// <summary>
    /// Load script text from file
    /// </summary>
    string? LoadScript(string path);

    /// <summary>
    /// Clear all cached resources
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Remove a specific resource from cache
    /// </summary>
    void RemoveFromCache(string path);
}
