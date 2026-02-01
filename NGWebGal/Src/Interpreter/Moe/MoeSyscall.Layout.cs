using System;

namespace NGWebGal.Interpreter.Moe;

/// <summary>
/// Layout-related syscalls
/// </summary>
public static partial class MoeSyscall
{
    public static void RegLayout(MoeVariable layout)
        => RawRegLayout(layout);

    public static void SetLayout(MoeVariable layout)
        => RawSetLayout(layout);

    public static void RegLayer(MoeVariable layout, MoeVariable layer, MoeVariable type,
        MoeVariable posx, MoeVariable posy, MoeVariable width, MoeVariable height)
        => RawRegLayer(layout, layer, type, posx, posy, width, height);

    public static void SetImageBox(MoeVariable layout, MoeVariable layer, MoeVariable image,
        MoeVariable subx, MoeVariable suby, MoeVariable width, MoeVariable height)
        => RawSetImageBox(layout, layer, image, subx, suby, width, height);

    public static void SetTextBox(MoeVariable layout, MoeVariable layer, MoeVariable text,
        MoeVariable font, MoeVariable size)
        => RawSetTextBox(layout, layer, text, font, size);

    public static void SetLatexBox(MoeVariable layout, MoeVariable layer, MoeVariable text,
        MoeVariable size)
        => RawSetLatexBox(layout, layer, text, size);

    public static void SetColorBox(MoeVariable layout, MoeVariable layer, MoeVariable a,
        MoeVariable r, MoeVariable g, MoeVariable b)
        => RawSetColorBox(layout, layer, a, r, g, b);

    public static void SetButtonBox(MoeVariable layout, MoeVariable layer, MoeVariable normal,
        MoeVariable hover, MoeVariable pressed, MoeVariable focused)
        => RawSetButtonBox(layout, layer, normal, hover, pressed, focused);

    public static void SetSliderBox(MoeVariable layout, MoeVariable layer, MoeVariable track,
        MoeVariable normal, MoeVariable hover, MoeVariable pressed, MoeVariable focused)
        => RawSetSliderBox(layout, layer, track, normal, hover, pressed, focused);

    private static void RawRegLayout(int layout)
    {
        if (_interpreter?.Driver == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        layoutMgr.GetOrCreateLayout(layout);
    }

    private static void RawSetLayout(int layout)
    {
        if (_interpreter?.Driver == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        layoutMgr.ActiveLayout = layout;
    }

    private static void RawRegLayer(int layout, int layer, string type, int posx, int posy, int width, int height)
    {
        if (_interpreter?.Driver == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetOrCreateLayout(layout);

        // Create layer using LayerBoxRegister
        var layerObj = NGWebGal.Global.LayerBoxRegister.GetLayerBox(type);
        layerObj.Position = new NGWebGal.Types.IVector { X = posx, Y = posy };
        layerObj.Size = new NGWebGal.Types.IVector { X = width, Y = height };

        layoutObj.Layers[layer] = layerObj;
    }

    private static void RawSetImageBox(int layout, int layer, string image, int subx, int suby, int width, int height)
    {
        if (_interpreter?.Driver == null || _elfHeader == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetLayout(layout);
        if (layoutObj == null || !layoutObj.Layers.TryGetValue(layer, out var layerObj)) return;

        // Load image resource if needed
        if (_elfHeader.ImageFiles.TryGetValue(image, out var imageFile))
        {
            var resourceMgr = _interpreter.Driver.Resources;
            resourceMgr.LoadImage(imageFile.URL);

            // TODO: Set image properties on layer when ImageBox type is implemented
            // For now, just ensure the resource is loaded
        }
    }

    private static void RawSetTextBox(int layout, int layer, string text, string font, int size)
    {
        if (_interpreter?.Driver == null || _elfHeader == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetLayout(layout);
        if (layoutObj == null || !layoutObj.Layers.TryGetValue(layer, out var layerObj)) return;

        // Load font resource if needed
        if (_elfHeader.BinFiles.TryGetValue(font, out var fontFile))
        {
            var resourceMgr = _interpreter.Driver.Resources;
            resourceMgr.LoadFont(fontFile.URL);

            // TODO: Set text properties on layer when TextBox type is implemented
        }
    }

    private static void RawSetLatexBox(int layout, int layer, string text, int size)
    {
        if (_interpreter?.Driver == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetLayout(layout);
        if (layoutObj == null || !layoutObj.Layers.TryGetValue(layer, out var layerObj)) return;

        // TODO: Set latex text on layer when LatexBox type is implemented
    }

    private static void RawSetColorBox(int layout, int layer, int a, int r, int g, int b)
    {
        if (_interpreter?.Driver == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetLayout(layout);
        if (layoutObj == null || !layoutObj.Layers.TryGetValue(layer, out var layerObj)) return;

        // TODO: Set color on layer when ColorBox type is implemented
    }

    private static void RawSetButtonBox(int layout, int layer, string normal, string hover, string pressed, string focused)
    {
        if (_interpreter?.Driver == null || _elfHeader == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetLayout(layout);
        if (layoutObj == null || !layoutObj.Layers.TryGetValue(layer, out var layerObj)) return;

        var resourceMgr = _interpreter.Driver.Resources;

        // Load all button state images
        if (_elfHeader.ImageFiles.TryGetValue(normal, out var normalFile))
            resourceMgr.LoadImage(normalFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(hover, out var hoverFile))
            resourceMgr.LoadImage(hoverFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(pressed, out var pressedFile))
            resourceMgr.LoadImage(pressedFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(focused, out var focusedFile))
            resourceMgr.LoadImage(focusedFile.URL);

        // TODO: Set button images on layer when ButtonBox type is implemented
    }

    private static void RawSetSliderBox(int layout, int layer, string track, string normal, string hover, string pressed, string focused)
    {
        if (_interpreter?.Driver == null || _elfHeader == null) return;

        var layoutMgr = _interpreter.Driver.Layout;
        var layoutObj = layoutMgr.GetLayout(layout);
        if (layoutObj == null || !layoutObj.Layers.TryGetValue(layer, out var layerObj)) return;

        var resourceMgr = _interpreter.Driver.Resources;

        // Load all slider images
        if (_elfHeader.ImageFiles.TryGetValue(track, out var trackFile))
            resourceMgr.LoadImage(trackFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(normal, out var normalFile))
            resourceMgr.LoadImage(normalFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(hover, out var hoverFile))
            resourceMgr.LoadImage(hoverFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(pressed, out var pressedFile))
            resourceMgr.LoadImage(pressedFile.URL);
        if (_elfHeader.ImageFiles.TryGetValue(focused, out var focusedFile))
            resourceMgr.LoadImage(focusedFile.URL);

        // TODO: Set slider images on layer when SliderBox type is implemented
    }
}
