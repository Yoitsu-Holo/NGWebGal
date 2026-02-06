using System;
using System.IO;
using System.Text.Json;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Service for managing canvas configuration persistence.
/// Implements singleton pattern for global access.
/// </summary>
public class CanvasConfigService
{
    private static CanvasConfigService? _instance;
    private static readonly object _lock = new();
    private readonly string _configFilePath;
    private CanvasConfig _config;

    /// <summary>
    /// Gets the singleton instance of the CanvasConfigService.
    /// </summary>
    public static CanvasConfigService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new CanvasConfigService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the current canvas configuration.
    /// </summary>
    public CanvasConfig Config => _config;

    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private CanvasConfigService()
    {
        // Determine config file path: ~/.ngwebgal/canvas-config.json
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configDir = Path.Combine(homeDir, ".ngwebgal");
        _configFilePath = Path.Combine(configDir, "canvas-config.json");

        // Load or create default config
        _config = LoadConfig();
    }

    /// <summary>
    /// Loads the canvas configuration from disk.
    /// Returns default configuration if file doesn't exist or is invalid.
    /// </summary>
    private CanvasConfig LoadConfig()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<CanvasConfig>(json);
                if (config != null)
                {
                    return config;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load canvas config: {ex.Message}");
        }

        // Return default configuration
        return new CanvasConfig();
    }

    /// <summary>
    /// Saves the current canvas configuration to disk.
    /// Creates the config directory if it doesn't exist.
    /// </summary>
    public void SaveConfig()
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize and save
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(_config, options);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save canvas config: {ex.Message}");
            throw;
        }
    }
}
