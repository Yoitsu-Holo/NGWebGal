using System;
using System.Collections.Generic;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Factory class for creating editor widgets with default configurations.
/// </summary>
public class WidgetFactory
{
    private readonly Dictionary<WidgetType, int> _widgetCounters = new();

    /// <summary>
    /// Creates a new widget of the specified type at the given position.
    /// </summary>
    /// <param name="type">The type of widget to create.</param>
    /// <param name="x">The X coordinate position on the canvas.</param>
    /// <param name="y">The Y coordinate position on the canvas.</param>
    /// <returns>A new editor widget with default properties.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid widget type is provided.</exception>
    public EditorWidget CreateWidget(WidgetType type, int x, int y)
    {
        // Increment counter for this widget type
        if (!_widgetCounters.ContainsKey(type))
        {
            _widgetCounters[type] = 0;
        }
        _widgetCounters[type]++;

        var widget = new EditorWidget
        {
            Type = type,
            X = x,
            Y = y,
            Name = $"{type}{_widgetCounters[type]}",
            Visible = true,
            Enable = true,
            ZIndex = 0
        };

        // Set default size based on widget type
        switch (type)
        {
            case WidgetType.ImageBox:
                widget.Width = 200;
                widget.Height = 200;
                break;

            case WidgetType.TextBox:
                widget.Width = 300;
                widget.Height = 100;
                break;

            case WidgetType.ColorBox:
                widget.Width = 100;
                widget.Height = 100;
                break;

            case WidgetType.ProgressBar:
                widget.Width = 200;
                widget.Height = 30;
                break;

            case WidgetType.Button:
                widget.Width = 120;
                widget.Height = 40;
                widget.Text = "Button";
                break;

            case WidgetType.Toggle:
                widget.Width = 60;
                widget.Height = 30;
                break;

            case WidgetType.Checkbox:
                widget.Width = 30;
                widget.Height = 30;
                break;

            case WidgetType.SliderHorizontal:
                widget.Width = 200;
                widget.Height = 20;
                break;

            case WidgetType.SliderVertical:
                widget.Width = 20;
                widget.Height = 200;
                break;

            case WidgetType.InputField:
                widget.Width = 200;
                widget.Height = 30;
                widget.Text = "";
                break;

            default:
                throw new ArgumentException($"Unknown widget type: {type}", nameof(type));
        }

        return widget;
    }

    /// <summary>
    /// Resets the widget counters for all types.
    /// </summary>
    public void ResetCounters()
    {
        _widgetCounters.Clear();
    }

    /// <summary>
    /// Resets the counter for a specific widget type.
    /// </summary>
    /// <param name="type">The widget type to reset the counter for.</param>
    public void ResetCounter(WidgetType type)
    {
        _widgetCounters.Remove(type);
    }
}
