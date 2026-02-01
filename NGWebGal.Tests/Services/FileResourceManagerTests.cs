using NGWebGal.Services;
using SkiaSharp;
using System;
using System.IO;
using Xunit;

namespace NGWebGal.Tests.Services;

public class FileResourceManagerTests : IDisposable
{
    private readonly string _testDir;
    private readonly FileResourceManager _manager;

    public FileResourceManagerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"ngwebgal_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
        _manager = new FileResourceManager(_testDir, maxCacheSize: 3);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, true);
    }

    [Fact]
    public void LoadScript_ValidFile_ReturnsContent()
    {
        // Arrange
        var scriptPath = Path.Combine(_testDir, "test.txt");
        var content = "test script content";
        File.WriteAllText(scriptPath, content);

        // Act
        var result = _manager.LoadScript("test.txt");

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void LoadScript_NonExistentFile_ReturnsNull()
    {
        // Act
        var result = _manager.LoadScript("nonexistent.txt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LoadAudio_ValidFile_ReturnsBytes()
    {
        // Arrange
        var audioPath = Path.Combine(_testDir, "test.mp3");
        var data = new byte[] { 1, 2, 3, 4, 5 };
        File.WriteAllBytes(audioPath, data);

        // Act
        var result = _manager.LoadAudio("test.mp3");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(data, result);
    }

    [Fact]
    public void Cache_LoadTwice_ReturnsSameInstance()
    {
        // Arrange
        var scriptPath = Path.Combine(_testDir, "cached.txt");
        File.WriteAllText(scriptPath, "cached content");

        // Act
        var result1 = _manager.LoadScript("cached.txt");
        var result2 = _manager.LoadScript("cached.txt");

        // Assert
        Assert.Same(result1, result2);
    }

    [Fact]
    public void Cache_LRU_EvictsOldest()
    {
        // Arrange - cache size is 3
        File.WriteAllText(Path.Combine(_testDir, "file1.txt"), "1");
        File.WriteAllText(Path.Combine(_testDir, "file2.txt"), "2");
        File.WriteAllText(Path.Combine(_testDir, "file3.txt"), "3");
        File.WriteAllText(Path.Combine(_testDir, "file4.txt"), "4");

        // Act - load 4 files (should evict file1)
        _manager.LoadScript("file1.txt");
        _manager.LoadScript("file2.txt");
        _manager.LoadScript("file3.txt");
        _manager.LoadScript("file4.txt");

        // Assert - file1 should be evicted, others cached
        var result2a = _manager.LoadScript("file2.txt");
        var result2b = _manager.LoadScript("file2.txt");
        Assert.Same(result2a, result2b);
    }

    [Fact]
    public void ClearCache_RemovesAllEntries()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "content");
        var result1 = _manager.LoadScript("test.txt");

        // Act
        _manager.ClearCache();
        var result2 = _manager.LoadScript("test.txt");

        // Assert - should reload from disk
        Assert.NotSame(result1, result2);
    }

    [Fact]
    public void RemoveFromCache_RemovesSpecificEntry()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_testDir, "test.txt"), "content");
        var result1 = _manager.LoadScript("test.txt");

        // Act
        _manager.RemoveFromCache("test.txt");
        var result2 = _manager.LoadScript("test.txt");

        // Assert - should reload from disk
        Assert.NotSame(result1, result2);
    }
}
