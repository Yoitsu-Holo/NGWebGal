using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SkiaSharp;

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

    /// <summary>
    /// Shadow state for image file paths (not serialized).
    /// Key is the property name (e.g., "Image"), value is the file path.
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, string> ImagePaths { get; set; } = new();

    /// <summary>
    /// Shadow state for image source information with crop rectangles (not serialized).
    /// Key is imageId, value is (originalPath, subRect).
    /// Used by multi-image editors to track file source and cropping metadata.
    /// </summary>
    [XmlIgnore]
    public Dictionary<int, (string path, SKRectI subRect)> ImageInfos { get; set; } = new();

    /// <summary>
    /// Shadow state for color properties (not serialized).
    /// </summary>
    [XmlIgnore]
    public EditorColor? Color { get; set; }
}

/// <summary>
/// Serializable representation of SKColor for editor shadow state.
/// </summary>
public struct EditorColor
{
    public byte A { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    /// <summary>
    /// Converts this EditorColor to an SKColor.
    /// </summary>
    public SKColor ToSKColor() => new(R, G, B, A);

    /// <summary>
    /// Creates an EditorColor from an SKColor.
    /// </summary>
    public static EditorColor FromSKColor(SKColor color) => new()
    {
        A = color.Alpha,
        R = color.Red,
        G = color.Green,
        B = color.Blue
    };
}
