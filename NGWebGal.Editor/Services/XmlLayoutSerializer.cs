using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Provides XML-based serialization and deserialization for editor layouts.
/// </summary>
public class XmlLayoutSerializer : ILayoutSerializer
{
    private readonly XmlSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlLayoutSerializer"/> class.
    /// </summary>
    public XmlLayoutSerializer()
    {
        _serializer = new XmlSerializer(typeof(EditorLayout));
    }

    /// <summary>
    /// Saves an editor layout to an XML file with UTF-8 encoding.
    /// </summary>
    /// <param name="layout">The layout to save.</param>
    /// <param name="filePath">The file path where the layout will be saved.</param>
    /// <exception cref="ArgumentNullException">Thrown when layout or filePath is null.</exception>
    /// <exception cref="IOException">Thrown when file operations fail.</exception>
    public void Save(EditorLayout layout, string filePath)
    {
        if (layout == null)
            throw new ArgumentNullException(nameof(layout));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            // Update modification timestamp
            layout.ModifiedAt = DateTime.UtcNow;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Serialize to XML with UTF-8 encoding
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
            _serializer.Serialize(writer, layout);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new IOException($"Failed to save layout to '{filePath}'.", ex);
        }
    }

    /// <summary>
    /// Loads an editor layout from an XML file.
    /// </summary>
    /// <param name="filePath">The file path to load the layout from.</param>
    /// <returns>The loaded editor layout.</returns>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
    public EditorLayout Load(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Layout file not found: '{filePath}'", filePath);

        try
        {
            using var reader = new StreamReader(filePath, Encoding.UTF8);
            var layout = _serializer.Deserialize(reader) as EditorLayout;

            if (layout == null)
                throw new InvalidOperationException("Deserialization returned null.");

            return layout;
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize layout from '{filePath}'. The file may be corrupted or in an invalid format.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while loading layout from '{filePath}'.", ex);
        }
    }
}
