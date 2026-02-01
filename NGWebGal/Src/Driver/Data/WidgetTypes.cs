using NGWebGal.Types;

namespace NGWebGal.Driver.Data;

/// <summary>
/// Color box settings
/// </summary>
public record struct ColorBoxInfo
{
    public LayerIdInfo ID { get; set; }

    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }
}

public record struct ColorBoxColor
{
    public LayerIdInfo ID { get; set; }
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }
    public byte A { get; set; }
}

/// <summary>
/// Text box settings
/// </summary>
public record struct TextBoxInfo
{
    public LayerIdInfo ID { get; set; }

    public string Text { get; set; }
    public string Font { get; set; }
    public int FontSize { get; set; }
}

public record struct TextBoxText
{
    public LayerIdInfo ID { get; set; }
    public string Text { get; set; }
}

public record struct TextBoxFont
{
    public LayerIdInfo ID { get; set; }
    public string Font { get; set; }
}

public record struct TextBoxFontSize
{
    public LayerIdInfo ID { get; set; }
    public int FontSize { get; set; }
}

/// <summary>
/// Image box settings
/// </summary>
public record struct ImageBoxInfo
{
    public LayerIdInfo ID { get; set; }
    public ImageInfo Image { get; set; }
}

public record struct ImageBoxImage
{
    public LayerIdInfo ID { get; set; }
    public ImageInfo Image { get; set; }
}
