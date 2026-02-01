using NGWebGal.Driver.Data;
using NGWebGal.Services;

namespace NGWebGal.Driver.API;

/// <summary>
/// Widget API - ColorBox, TextBox, ImageBox
/// </summary>
public class WidgetAPI
{
    private readonly LayoutManager _layoutManager;

    public WidgetAPI(LayoutManager layoutManager)
    {
        _layoutManager = layoutManager;
    }

    #region ColorBox

    public Response SetColorBox(ColorBoxInfo info)
    {
        var layout = _layoutManager.GetLayout(info.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {info.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(info.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {info.ID.LayerID} not found in layout {info.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetColorBox colorBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {info.ID.LayerID} is not a WidgetColorBox" };

        colorBox.SetColor(new SkiaSharp.SKColor(info.R, info.G, info.B, info.A));
        return new Response { Type = ResponseType.Success };
    }

    public Response SetColorBoxColor(ColorBoxColor color)
    {
        var layout = _layoutManager.GetLayout(color.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {color.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(color.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {color.ID.LayerID} not found in layout {color.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetColorBox colorBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {color.ID.LayerID} is not a WidgetColorBox" };

        colorBox.SetColor(new SkiaSharp.SKColor(color.R, color.G, color.B, color.A));
        return new Response { Type = ResponseType.Success };
    }

    #endregion

    #region TextBox

    public Response SetTextBox(TextBoxInfo info)
    {
        var layout = _layoutManager.GetLayout(info.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {info.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(info.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {info.ID.LayerID} not found in layout {info.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetTextBox textBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {info.ID.LayerID} is not a WidgetTextBox" };

        textBox.Text = info.Text;
        if (info.FontSize > 0)
            textBox.SetFontSize(info.FontSize);
        // Note: Font loading would require ResourceManager integration
        return new Response { Type = ResponseType.Success };
    }

    public Response SetTextBoxText(TextBoxText text)
    {
        var layout = _layoutManager.GetLayout(text.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {text.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(text.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {text.ID.LayerID} not found in layout {text.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetTextBox textBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {text.ID.LayerID} is not a WidgetTextBox" };

        textBox.Text = text.Text;
        return new Response { Type = ResponseType.Success };
    }

    public Response SetTextBoxFont(TextBoxFont font)
    {
        var layout = _layoutManager.GetLayout(font.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {font.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(font.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {font.ID.LayerID} not found in layout {font.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetTextBox textBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {font.ID.LayerID} is not a WidgetTextBox" };

        // TODO: Font loading requires ResourceManager integration
        return new Response { Type = ResponseType.Success, Message = "Font setting requires ResourceManager" };
    }

    public Response SetTextBoxFontSize(TextBoxFontSize fontSize)
    {
        var layout = _layoutManager.GetLayout(fontSize.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {fontSize.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(fontSize.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {fontSize.ID.LayerID} not found in layout {fontSize.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetTextBox textBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {fontSize.ID.LayerID} is not a WidgetTextBox" };

        textBox.SetFontSize(fontSize.FontSize);
        return new Response { Type = ResponseType.Success };
    }

    #endregion

    #region ImageBox

    public Response SetImageBox(ImageBoxInfo info)
    {
        var layout = _layoutManager.GetLayout(info.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {info.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(info.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {info.ID.LayerID} not found in layout {info.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetImageBox imageBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {info.ID.LayerID} is not a WidgetImageBox" };

        // TODO: Image loading requires ResourceManager integration
        return new Response { Type = ResponseType.Success, Message = "Image setting requires ResourceManager" };
    }

    public Response SetImageBoxImage(ImageBoxImage image)
    {
        var layout = _layoutManager.GetLayout(image.ID.LayoutID);
        if (layout == null)
            return new Response { Type = ResponseType.Fail, Message = $"Layout {image.ID.LayoutID} not found" };

        if (!layout.Layers.TryGetValue(image.ID.LayerID, out var layer))
            return new Response { Type = ResponseType.Fail, Message = $"Layer {image.ID.LayerID} not found in layout {image.ID.LayoutID}" };

        if (layer is not Layer.Widget.WidgetImageBox imageBox)
            return new Response { Type = ResponseType.Fail, Message = $"Layer {image.ID.LayerID} is not a WidgetImageBox" };

        // TODO: Image loading requires ResourceManager integration
        return new Response { Type = ResponseType.Success, Message = "Image setting requires ResourceManager" };
    }

    #endregion
}
