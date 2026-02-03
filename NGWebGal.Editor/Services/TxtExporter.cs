using System;
using System.IO;
using System.Text;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Provides text-based export functionality for editor layouts.
/// </summary>
public class TxtExporter
{
    /// <summary>
    /// Exports an editor layout to a human-readable text file.
    /// </summary>
    /// <param name="layout">The layout to export.</param>
    /// <param name="filePath">The file path where the text export will be saved.</param>
    /// <exception cref="ArgumentNullException">Thrown when layout or filePath is null.</exception>
    /// <exception cref="IOException">Thrown when file operations fail.</exception>
    public void Export(EditorLayout layout, string filePath)
    {
        if (layout == null)
            throw new ArgumentNullException(nameof(layout));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("# WebGal Layout Export");
            sb.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"# Layout: {layout.Name}");
            sb.AppendLine($"# Canvas: {layout.CanvasWidth}x{layout.CanvasHeight}");
            sb.AppendLine();
            sb.AppendLine($"Widget Count: {layout.Widgets.Count}");
            sb.AppendLine("============================================================");
            sb.AppendLine();

            // Widgets
            foreach (var widget in layout.Widgets)
            {
                sb.AppendLine($"[{widget.Type}] {widget.Name}");
                sb.AppendLine($"  ID:       {widget.Id}");
                sb.AppendLine($"  Position: ({widget.X}, {widget.Y})");
                sb.AppendLine($"  Size:     {widget.Width} x {widget.Height}");
                sb.AppendLine($"  Visible:  {widget.Visible}");
                sb.AppendLine($"  Enable:   {widget.Enable}");
                sb.AppendLine($"  ZIndex:   {widget.ZIndex}");

                if (!string.IsNullOrEmpty(widget.Text))
                {
                    sb.AppendLine($"  Text:     \"{widget.Text}\"");
                }

                sb.AppendLine();
            }

            // Write to file
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new IOException($"Failed to export layout to '{filePath}'.", ex);
        }
    }
}
