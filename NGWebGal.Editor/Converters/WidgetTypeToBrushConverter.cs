using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using NGWebGal.Editor.Models;

namespace NGWebGal.Editor.Converters;

/// <summary>
/// Converts WidgetType enum values to SolidColorBrush colors for UI visualization.
/// </summary>
/// <remarks>
/// Each widget type is mapped to a distinct light color for visual differentiation in the editor.
/// </remarks>
public class WidgetTypeToBrushConverter : IValueConverter
{
    /// <summary>
    /// Converts a WidgetType value to a SolidColorBrush.
    /// </summary>
    /// <param name="value">The WidgetType value to convert.</param>
    /// <param name="targetType">The target type (SolidColorBrush).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">The culture info (unused).</param>
    /// <returns>A SolidColorBrush corresponding to the widget type.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
    {
        if (value is not WidgetType widgetType)
        {
            return new SolidColorBrush(Colors.Gray);
        }

        return widgetType switch
        {
            WidgetType.ImageBox => new SolidColorBrush(Colors.LightBlue),
            WidgetType.TextBox => new SolidColorBrush(Colors.LightGreen),
            WidgetType.ColorBox => new SolidColorBrush(Colors.LightCoral),
            WidgetType.ProgressBar => new SolidColorBrush(Colors.LightGoldenrodYellow),
            WidgetType.Button => new SolidColorBrush(Colors.LightSalmon),
            WidgetType.Toggle => new SolidColorBrush(Colors.LightSeaGreen),
            WidgetType.Checkbox => new SolidColorBrush(Colors.LightSkyBlue),
            WidgetType.SliderHorizontal => new SolidColorBrush(Colors.LightSteelBlue),
            WidgetType.SliderVertical => new SolidColorBrush(Colors.LightSteelBlue),
            WidgetType.InputField => new SolidColorBrush(Colors.LightYellow),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    /// <summary>
    /// Converts a SolidColorBrush back to a WidgetType value.
    /// </summary>
    /// <param name="value">The SolidColorBrush value to convert.</param>
    /// <param name="targetType">The target type (WidgetType).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">The culture info (unused).</param>
    /// <returns>Not implemented - throws NotImplementedException.</returns>
    /// <exception cref="NotImplementedException">This conversion is not supported.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
    {
        throw new NotImplementedException("WidgetTypeToBrushConverter does not support ConvertBack.");
    }
}
