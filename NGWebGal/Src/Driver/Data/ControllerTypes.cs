using NGWebGal.Types;

namespace NGWebGal.Driver.Data;

/// <summary>
/// Button box settings
/// </summary>
public record struct ButtonBoxInfo
{
    public LayerIdInfo ID { get; set; }
    public ImageInfo NormalImage { get; set; }
    public ImageInfo HoverImage { get; set; }
    public ImageInfo PressedImage { get; set; }
    public ImageInfo FocusedImage { get; set; }
    public IVector ButtonSize { get; set; }
}

public record struct ButtonBoxImage
{
    public LayerIdInfo ID { get; set; }
    public ControllerStatus Status { get; set; }
    public ImageInfo Image { get; set; }
}

/// <summary>
/// Slider box settings
/// </summary>
public record struct SliderBoxInfo
{
    public LayerIdInfo ID { get; set; }
    public ImageInfo TrackImage { get; set; }
    public ImageInfo NormalImage { get; set; }
    public ImageInfo HoverImage { get; set; }
    public ImageInfo PressedImage { get; set; }
    public ImageInfo FocusedImage { get; set; }
    public IVector ThumbSize { get; set; }
    public IVector TrackSize { get; set; }
}

public record struct SliderBoxImage
{
    public LayerIdInfo ID { get; set; }
    public ControllerStatus Status { get; set; }
    public ImageInfo Image { get; set; }
}

public record struct SliderBoxTrackImage
{
    public LayerIdInfo ID { get; set; }
    public ImageInfo Image { get; set; }
}
