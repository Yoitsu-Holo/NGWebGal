using System;
using System.IO;
using System.Text.Json;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Service for managing guide line settings persistence.
/// Implements singleton pattern for global access.
/// </summary>
public class GuideLineService
{
    private static GuideLineService? _instance;
    private static readonly object _lock = new();
    private readonly string _configFilePath;
    private GuideLineSettings _settings;

    /// <summary>
    /// Gets the singleton instance of the GuideLineService.
    /// </summary>
    public static GuideLineService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new GuideLineService();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Gets the current guide line settings.
    /// </summary>
    public GuideLineSettings Settings => _settings;

    /// <summary>
    /// Private constructor to enforce singleton pattern.
    /// </summary>
    private GuideLineService()
    {
        // Determine config file path: ~/.ngwebgal/guideline-settings.json
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configDir = Path.Combine(homeDir, ".ngwebgal");
        _configFilePath = Path.Combine(configDir, "guideline-settings.json");

        // Load or create default settings
        _settings = LoadSettings();
    }

    /// <summary>
    /// Loads the guide line settings from disk.
    /// Returns default settings if file doesn't exist or is invalid.
    /// </summary>
    private GuideLineSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = File.ReadAllText(_configFilePath);
                var settings = JsonSerializer.Deserialize<GuideLineSettings>(json);
                if (settings != null)
                {
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load guide line settings: {ex.Message}");
        }

        // Return default settings
        return new GuideLineSettings();
    }

    /// <summary>
    /// Saves the current guide line settings to disk.
    /// Creates the config directory if it doesn't exist.
    /// </summary>
    public void SaveSettings()
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
            var json = JsonSerializer.Serialize(_settings, options);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save guide line settings: {ex.Message}");
            throw;
        }
    }
}
