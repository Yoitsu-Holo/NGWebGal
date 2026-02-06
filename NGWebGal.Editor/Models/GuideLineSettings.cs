using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NGWebGal.Editor.Models;

/// <summary>
/// Configuration for guide lines displayed on the editor canvas.
/// </summary>
[Serializable]
public class GuideLineSettings : INotifyPropertyChanged
{
    private bool _showPercentageLines = true;
    private bool _showGridLines = false;
    private int _gridSpacing = 50;
    private List<double> _percentagePositions = new() { 0.1, 0.25, 0.5, 0.75, 0.9 };
    private bool _showDynamicCrosshair = true;

    /// <summary>
    /// Gets or sets whether percentage-based guide lines are visible.
    /// </summary>
    public bool ShowPercentageLines
    {
        get => _showPercentageLines;
        set
        {
            if (_showPercentageLines != value)
            {
                _showPercentageLines = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether grid lines are visible.
    /// </summary>
    public bool ShowGridLines
    {
        get => _showGridLines;
        set
        {
            if (_showGridLines != value)
            {
                _showGridLines = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the spacing between grid lines in pixels.
    /// Must be greater than 0.
    /// </summary>
    public int GridSpacing
    {
        get => _gridSpacing;
        set
        {
            if (value <= 0)
                throw new ArgumentException("GridSpacing must be greater than 0", nameof(value));

            if (_gridSpacing != value)
            {
                _gridSpacing = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the percentage positions for guide lines (0.0 to 1.0).
    /// </summary>
    public List<double> PercentagePositions
    {
        get => _percentagePositions;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (_percentagePositions != value)
            {
                _percentagePositions = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the dynamic crosshair is visible when hovering.
    /// </summary>
    public bool ShowDynamicCrosshair
    {
        get => _showDynamicCrosshair;
        set
        {
            if (_showDynamicCrosshair != value)
            {
                _showDynamicCrosshair = value;
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
