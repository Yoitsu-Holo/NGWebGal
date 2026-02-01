using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SkiaSharp;
using NGWebGal.Services;
using NGWebGal.Audio;
using NGWebGal.Driver;
using NGWebGal.Layer;

namespace NGWebGal.Tests.Integration;

public class BasicIntegrationTests
{
    private GameManager CreateGameManager()
    {
        var resourceManager = new FileResourceManager("./test-resources");
        var layoutManager = new LayoutManager();
        var audioManager = new AudioManager();
        var driver = new NGWebGal.Driver.Driver(layoutManager, resourceManager, audioManager);

        return new GameManager(layoutManager, audioManager, resourceManager, driver);
    }

    [Fact]
    public void GameManager_Initialization_ShouldSucceed()
    {
        // Arrange & Act
        var gameManager = CreateGameManager();

        // Assert
        gameManager.Should().NotBeNull();
        gameManager.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public async Task GameManager_Init_ShouldClearResources()
    {
        // Arrange
        var gameManager = CreateGameManager();

        // Act
        await gameManager.Init("test-game");

        // Assert
        gameManager.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public void Layout_Creation_ShouldSucceed()
    {
        // Arrange & Act
        var layout = new Layout();

        // Assert
        layout.Should().NotBeNull();
        layout.Layers.Should().BeEmpty();
        layout.SceneId.Should().Be(-1);
    }

    [Fact]
    public void Layout_Rendering_WithNoLayers_ShouldNotCrash()
    {
        // Arrange
        var layout = new Layout();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var canvas = surface.Canvas;

        // Act
        Action act = () => layout.Render(canvas, force: false);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Layout_ShouldRender_WithNoLayers_ReturnsFalse()
    {
        // Arrange
        var layout = new Layout();

        // Act
        var shouldRender = layout.ShouldRender();

        // Assert
        shouldRender.Should().BeFalse();
    }

    [Fact]
    public void LayoutManager_Creation_ShouldSucceed()
    {
        // Arrange & Act
        var layoutManager = new LayoutManager();

        // Assert
        layoutManager.Should().NotBeNull();
        layoutManager.ShouldRender().Should().BeFalse();
    }

    [Fact]
    public void LayoutManager_Render_WithNoLayouts_ShouldNotCrash()
    {
        // Arrange
        var layoutManager = new LayoutManager();
        using var surface = SKSurface.Create(new SKImageInfo(800, 600));
        var canvas = surface.Canvas;

        // Act
        Action act = () => layoutManager.Render(canvas, force: false);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ResourceManager_Creation_ShouldSucceed()
    {
        // Arrange & Act
        var resourceManager = new FileResourceManager("./test-resources");

        // Assert
        resourceManager.Should().NotBeNull();
    }

    [Fact]
    public void ResourceManager_ClearCache_ShouldNotCrash()
    {
        // Arrange
        var resourceManager = new FileResourceManager("./test-resources");

        // Act
        Action act = () => resourceManager.ClearCache();

        // Assert
        act.Should().NotThrow();
    }
}
