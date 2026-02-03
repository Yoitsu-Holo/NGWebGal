using System;
using NGWebGal.Editor.Models;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using NGWebGal.Layer.Controller;
using NGWebGal.Types;
using SkiaSharp;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Factory for creating Core Layer instances from EditorWidget data
/// </summary>
public class CoreWidgetFactory
{
    /// <summary>
    /// Gets the SKColor for a widget type, matching the Toolbox colors with 50% alpha
    /// </summary>
    private SKColor GetWidgetColor(WidgetType type)
    {
        // Match colors from WidgetTypeToBrushConverter, with 50% alpha (128)
        return type switch
        {
            WidgetType.ImageBox => new SKColor(173, 216, 230, 128),       // LightBlue
            WidgetType.TextBox => new SKColor(144, 238, 144, 128),        // LightGreen
            WidgetType.ColorBox => new SKColor(240, 128, 128, 128),       // LightCoral
            WidgetType.ProgressBar => new SKColor(250, 250, 210, 128),    // LightGoldenrodYellow
            WidgetType.Button => new SKColor(255, 160, 122, 128),         // LightSalmon
            WidgetType.Toggle => new SKColor(32, 178, 170, 128),          // LightSeaGreen
            WidgetType.Checkbox => new SKColor(135, 206, 250, 128),       // LightSkyBlue
            WidgetType.SliderHorizontal => new SKColor(176, 196, 222, 128), // LightSteelBlue
            WidgetType.SliderVertical => new SKColor(176, 196, 222, 128),   // LightSteelBlue
            WidgetType.InputField => new SKColor(255, 255, 224, 128),     // LightYellow
            _ => new SKColor(128, 128, 128, 128)                          // Gray
        };
    }

    public ILayer CreateLayer(EditorWidget widget)
    {
        ILayer layer = widget.Type switch
        {
            WidgetType.TextBox => CreateTextBox(widget),
            WidgetType.ColorBox => CreateColorBox(widget),
            WidgetType.ImageBox => CreateImageBox(widget),
            WidgetType.ProgressBar => CreateProgressBar(widget),
            WidgetType.Button => CreateButton(widget),
            WidgetType.Toggle => CreateToggle(widget),
            WidgetType.Checkbox => CreateCheckbox(widget),
            WidgetType.SliderHorizontal => CreateSliderH(widget),
            WidgetType.SliderVertical => CreateSliderV(widget),
            WidgetType.InputField => CreateInputField(widget),
            _ => throw new ArgumentException($"Unknown widget type: {widget.Type}")
        };

        // Set common properties
        layer.Position = new IVector(widget.X, widget.Y);
        layer.Size = new IVector(widget.Width, widget.Height);
        layer.Name = widget.Name;
        layer.Visible = widget.Visible;
        layer.Enable = widget.Enable;

        // IMPORTANT: DoAnimation(0) initializes animation data and colors
        layer.DoAnimation(0);

        return layer;
    }

    private ILayer CreateTextBox(EditorWidget widget)
    {
        var layer = new WidgetTextBox();
        layer.Size = new IVector(widget.Width, widget.Height);
        layer.SetFontSize(24);
        layer.Text = widget.Text.Length > 0 ? widget.Text : "Text"; // Default text if empty

        // TextBox only renders text, not background
        // We need to create a background using ImageBox instead
        var color = GetWidgetColor(WidgetType.TextBox);
        var bgLayer = new WidgetImageBox();
        bgLayer.Size = new IVector(widget.Width, widget.Height);
        var bgBitmap = CreateSolidColorBitmap(widget.Width, widget.Height, color);
        bgLayer.SetImage(bgBitmap, 0);

        // For now, just return the background since we can't composite layers easily
        // In the future, we could create a composite layer
        return bgLayer;
    }

    private ILayer CreateColorBox(EditorWidget widget)
    {
        var layer = new WidgetColorBox();
        layer.Size = new IVector(widget.Width, widget.Height);
        var color = GetWidgetColor(WidgetType.ColorBox);
        layer.SetColor(color);

        // Initialize EditorWidget.Color shadow state so property panel can read it
        widget.Color = EditorColor.FromSKColor(color);

        return layer;
    }

    private ILayer CreateImageBox(EditorWidget widget)
    {
        var layer = new WidgetImageBox();
        layer.Size = new IVector(widget.Width, widget.Height);

        // Create solid color bitmap matching Toolbox color
        var color = GetWidgetColor(WidgetType.ImageBox);
        var bitmap = CreateSolidColorBitmap(widget.Width, widget.Height, color);
        layer.SetImage(bitmap, 0);
        return layer;
    }

    private ILayer CreateProgressBar(EditorWidget widget)
    {
        var layer = new WidgetProgressBar();
        layer.Size = new IVector(widget.Width, widget.Height);

        var color = GetWidgetColor(WidgetType.ProgressBar);
        // Track color (slightly darker)
        layer.SetColor(new SKColor(color.Red, color.Green, color.Blue, 180), 0);
        // Fill color (lighter)
        layer.SetColor(new SKColor(color.Red, color.Green, color.Blue, 255), 1);
        return layer;
    }

    private ILayer CreateButton(EditorWidget widget)
    {
        var layer = new ControllerButton();
        layer.Size = new IVector(widget.Width, widget.Height);
        layer.SetColor(GetWidgetColor(WidgetType.Button), 0);
        return layer;
    }

    private ILayer CreateToggle(EditorWidget widget)
    {
        var layer = new ControllerToggle();
        layer.Size = new IVector(widget.Width, widget.Height);

        var color = GetWidgetColor(WidgetType.Toggle);
        layer.SetColor(new SKColor(color.Red, color.Green, color.Blue, 180), 0); // Background
        layer.SetColor(color, 4); // Active color
        return layer;
    }

    private ILayer CreateCheckbox(EditorWidget widget)
    {
        var layer = new ControllerCheckbox();
        layer.Size = new IVector(widget.Width, widget.Height);

        var color = GetWidgetColor(WidgetType.Checkbox);
        layer.SetColor(new SKColor(color.Red, color.Green, color.Blue, 180), 0); // Background
        layer.SetColor(color, 4); // Check color
        return layer;
    }

    private ILayer CreateSliderH(EditorWidget widget)
    {
        var layer = new ControllerSliderHorizontal();
        layer.Size = new IVector(widget.Width, widget.Height);
        layer.ThumbSize = new IVector(20, widget.Height);

        var color = GetWidgetColor(WidgetType.SliderHorizontal);
        layer.SetColor(new SKColor(color.Red, color.Green, color.Blue, 180), -1); // Track
        layer.SetColor(color, 0); // Thumb
        return layer;
    }

    private ILayer CreateSliderV(EditorWidget widget)
    {
        var layer = new ControllerSliderVertical();
        layer.Size = new IVector(widget.Width, widget.Height);
        layer.ThumbSize = new IVector(widget.Width, 20);

        var color = GetWidgetColor(WidgetType.SliderVertical);
        layer.SetColor(new SKColor(color.Red, color.Green, color.Blue, 180), -1); // Track
        layer.SetColor(color, 0); // Thumb
        return layer;
    }

    private ILayer CreateInputField(EditorWidget widget)
    {
        var layer = new ControllerInputField();
        layer.Size = new IVector(widget.Width, widget.Height);
        layer.SetColor(GetWidgetColor(WidgetType.InputField), 0);
        return layer;
    }

    /// <summary>
    /// Creates a solid color bitmap with specified dimensions and color
    /// </summary>
    private SKBitmap CreateSolidColorBitmap(int width, int height, SKColor color)
    {
        var bitmap = new SKBitmap(Math.Max(1, width), Math.Max(1, height));
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint { Color = color };
        canvas.DrawRect(0, 0, width, height, paint);
        return bitmap;
    }
}
