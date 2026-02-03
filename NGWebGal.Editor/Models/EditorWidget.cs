using System;
using System.Xml.Serialization;

namespace NGWebGal.Editor.Models;

/// <summary>
/// Represents a widget in the editor canvas with its properties and state.
/// </summary>
[Serializable]
public class EditorWidget
{
    /// <summary>
    /// Gets or sets the unique identifier for this widget.
    /// </summary>
    [XmlAttribute]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Gets or sets the type of widget.
    /// </summary>
    [XmlAttribute]
    public WidgetType Type { get; set; }

    /// <summary>
    /// Gets or sets the display name of the widget.
    /// </summary>
    [XmlAttribute]
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the X coordinate position on the canvas.
    /// </summary>
    [XmlAttribute]
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate position on the canvas.
    /// </summary>
    [XmlAttribute]
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the widget.
    /// </summary>
    [XmlAttribute]
    public int Width { get; set; } = 100;

    /// <summary>
    /// Gets or sets the height of the widget.
    /// </summary>
    [XmlAttribute]
    public int Height { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether the widget is visible.
    /// </summary>
    [XmlAttribute]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the widget is enabled for interaction.
    /// </summary>
    [XmlAttribute]
    public bool Enable { get; set; } = true;

    /// <summary>
    /// Gets or sets the text content of the widget.
    /// </summary>
    [XmlAttribute]
    public string Text { get; set; } = "";

    /// <summary>
    /// Gets or sets the Z-index for layering order.
    /// </summary>
    [XmlAttribute]
    public int ZIndex { get; set; }
}
