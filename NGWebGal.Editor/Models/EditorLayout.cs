using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NGWebGal.Editor.Models;

/// <summary>
/// Represents the root container for a complete editor layout with all widgets and metadata.
/// </summary>
[Serializable]
[XmlRoot("EditorLayout")]
public class EditorLayout
{
    /// <summary>
    /// Gets or sets the layout file format version.
    /// </summary>
    [XmlAttribute]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets the name of the layout.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; } = "Untitled";

    /// <summary>
    /// Gets or sets the canvas width in pixels.
    /// </summary>
    [XmlAttribute]
    public int CanvasWidth { get; set; } = 1920;

    /// <summary>
    /// Gets or sets the canvas height in pixels.
    /// </summary>
    [XmlAttribute]
    public int CanvasHeight { get; set; } = 1080;

    /// <summary>
    /// Gets or sets the collection of widgets in this layout.
    /// </summary>
    [XmlArray("Widgets")]
    [XmlArrayItem("Widget")]
    public List<EditorWidget> Widgets { get; set; } = new();

    /// <summary>
    /// Gets or sets an optional description of the layout.
    /// </summary>
    [XmlElement]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [XmlAttribute]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    [XmlAttribute]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
