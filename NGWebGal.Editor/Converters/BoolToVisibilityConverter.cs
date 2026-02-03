using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace NGWebGal.Editor.Converters;

/// <summary>
/// Converts boolean values to boolean for Avalonia IsVisible binding.
/// </summary>
/// <remarks>
/// In Avalonia, IsVisible property uses bool directly, not Visibility enum.
/// true → visible
/// false → collapsed
/// </remarks>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a boolean for Avalonia IsVisible binding.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (bool).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">The culture info (unused).</param>
    /// <returns>true if visible, false if collapsed.</returns>
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }

        return false;
    }

    /// <summary>
    /// Converts a boolean value back to a boolean.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The target type (bool).</param>
    /// <param name="parameter">Optional converter parameter (unused).</param>
    /// <param name="culture">The culture info (unused).</param>
    /// <returns>The boolean value.</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }

        return false;
    }
}
