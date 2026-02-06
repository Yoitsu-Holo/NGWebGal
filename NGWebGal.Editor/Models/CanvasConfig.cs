using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NGWebGal.Editor.Models;

/// <summary>
/// Configuration for the editor canvas including dimensions and zoom level.
/// </summary>
[Serializable]
public class CanvasConfig : INotifyPropertyChanged
{
    private int _width = 1280;
    private int _height = 720;
    private double _zoomLevel = 1.0;

    /// <summary>
    /// Gets or sets the canvas width in pixels.
    /// Must be greater than 0.
    /// </summary>
    public int Width
    {
        get => _width;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Width must be greater than 0", nameof(value));

            if (_width != value)
            {
                _width = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the canvas height in pixels.
    /// Must be greater than 0.
    /// </summary>
    public int Height
    {
        get => _height;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Height must be greater than 0", nameof(value));

            if (_height != value)
            {
                _height = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the zoom level for the canvas.
    /// Must be between 0.1 and 5.0 (inclusive).
    /// </summary>
    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (value < 0.1 || value > 5.0)
                throw new ArgumentException("ZoomLevel must be between 0.1 and 5.0", nameof(value));

            if (Math.Abs(_zoomLevel - value) > 0.0001)
            {
                _zoomLevel = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Event raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
