using System;
using System.Threading.Tasks;
using SkiaSharp;
using NGWebGal.Audio;
using NGWebGal.Driver;

namespace NGWebGal.Services;

public class GameManager
{
    private readonly LayoutManager _layoutManager;
    private readonly AudioManager _audioManager;
    private readonly IResourceManager _resourceManager;
    private readonly Driver.Driver _driver;

    public GameManager(
        LayoutManager layoutManager,
        AudioManager audioManager,
        IResourceManager resourceManager,
        Driver.Driver driver)
    {
        _layoutManager = layoutManager;
        _audioManager = audioManager;
        _resourceManager = resourceManager;
        _driver = driver;
    }

    public async Task Clear()
    {
        _resourceManager.ClearCache();
        _layoutManager.Clear();
        // AudioManager doesn't have Clear method yet
        await Task.CompletedTask;
    }

    public async Task Init(string gameName)
    {
        await Clear();
        // BasePath is not part of IResourceManager interface
        // TODO: Handle game-specific resource paths
    }

    public bool ShouldRender() => _layoutManager.ShouldRender();

    public void Render(SKCanvas canvas, bool force = false) =>
        _layoutManager.Render(canvas, force);

    public void ProcessEvent(EventArgs eventArgs) =>
        _layoutManager.ProcessEvent(eventArgs);

    public async Task LoadMedia()
    {
        // TODO: Implement media loading
        await Task.CompletedTask;
    }
}
