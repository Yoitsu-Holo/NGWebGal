namespace NGWebGal.Editor.Services;

using NGWebGal.Editor.Models;

/// <summary>
/// Defines the contract for serializing and deserializing editor layouts.
/// </summary>
public interface ILayoutSerializer
{
    /// <summary>
    /// Saves an editor layout to a file.
    /// </summary>
    /// <param name="layout">The layout to save.</param>
    /// <param name="filePath">The file path where the layout will be saved.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when layout or filePath is null.</exception>
    /// <exception cref="System.IO.IOException">Thrown when file operations fail.</exception>
    void Save(EditorLayout layout, string filePath);

    /// <summary>
    /// Loads an editor layout from a file.
    /// </summary>
    /// <param name="filePath">The file path to load the layout from.</param>
    /// <returns>The loaded editor layout.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when filePath is null.</exception>
    /// <exception cref="System.IO.FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="System.InvalidOperationException">Thrown when deserialization fails.</exception>
    EditorLayout Load(string filePath);
}
