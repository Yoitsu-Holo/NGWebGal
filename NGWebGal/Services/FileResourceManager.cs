using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace NGWebGal.Services;

/// <summary>
/// File-based resource manager with LRU caching
/// </summary>
public class FileResourceManager : IResourceManager
{
    private readonly string _basePath;
    private readonly int _maxCacheSize;
    private readonly Dictionary<string, CacheEntry> _cache;
    private readonly LinkedList<string> _lruList;

    private class CacheEntry
    {
        public object? Resource { get; set; }
        public LinkedListNode<string>? LruNode { get; set; }
    }

    public FileResourceManager(string basePath, int maxCacheSize = 100)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        _maxCacheSize = maxCacheSize;
        _cache = new Dictionary<string, CacheEntry>();
        _lruList = new LinkedList<string>();
    }

    public SKBitmap? LoadImage(string path)
    {
        return LoadResource(path, p =>
        {
            var fullPath = GetFullPath(p);
            if (!File.Exists(fullPath))
                return null;

            return SKBitmap.Decode(fullPath);
        });
    }

    public SKTypeface? LoadFont(string path)
    {
        return LoadResource(path, p =>
        {
            var fullPath = GetFullPath(p);
            if (!File.Exists(fullPath))
                return null;

            return SKTypeface.FromFile(fullPath);
        });
    }

    public byte[]? LoadAudio(string path)
    {
        return LoadResource(path, p =>
        {
            var fullPath = GetFullPath(p);
            if (!File.Exists(fullPath))
                return null;

            return File.ReadAllBytes(fullPath);
        });
    }

    public string? LoadScript(string path)
    {
        return LoadResource(path, p =>
        {
            var fullPath = GetFullPath(p);
            if (!File.Exists(fullPath))
                return null;

            return File.ReadAllText(fullPath);
        });
    }

    public void ClearCache()
    {
        _cache.Clear();
        _lruList.Clear();
    }

    public void RemoveFromCache(string path)
    {
        if (_cache.TryGetValue(path, out var entry))
        {
            if (entry.LruNode != null)
                _lruList.Remove(entry.LruNode);
            _cache.Remove(path);
        }
    }

    private T? LoadResource<T>(string path, Func<string, T?> loader) where T : class
    {
        // Check cache first
        if (_cache.TryGetValue(path, out var entry))
        {
            UpdateLru(path, entry);
            return entry.Resource as T;
        }

        // Load resource
        try
        {
            var resource = loader(path);
            if (resource != null)
            {
                AddToCache(path, resource);
            }
            return resource;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void AddToCache(string path, object resource)
    {
        // Evict oldest if cache is full
        if (_cache.Count >= _maxCacheSize && _lruList.Last != null)
        {
            var oldestPath = _lruList.Last.Value;
            _cache.Remove(oldestPath);
            _lruList.RemoveLast();
        }

        var node = _lruList.AddFirst(path);
        _cache[path] = new CacheEntry
        {
            Resource = resource,
            LruNode = node
        };
    }

    private void UpdateLru(string path, CacheEntry entry)
    {
        if (entry.LruNode != null)
        {
            _lruList.Remove(entry.LruNode);
            entry.LruNode = _lruList.AddFirst(path);
        }
    }

    private string GetFullPath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.Combine(_basePath, path);
    }
}
