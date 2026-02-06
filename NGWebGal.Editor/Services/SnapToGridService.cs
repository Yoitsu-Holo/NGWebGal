using System;
using Avalonia;

namespace NGWebGal.Editor.Services;

/// <summary>
/// Provides snap-to-grid functionality for positioning elements on a canvas.
/// </summary>
public static class SnapToGridService
{
    /// <summary>
    /// Snaps a position to the nearest grid point on the canvas if within threshold.
    /// </summary>
    /// <param name="position">The position to snap.</param>
    /// <param name="canvasSize">The size of the canvas.</param>
    /// <param name="threshold">The distance threshold in pixels for snapping. Default is 10.</param>
    /// <returns>The snapped position, or the original position if no snap target is nearby.</returns>
    public static Point SnapPosition(Point position, Size canvasSize, int threshold = 10)
    {
        var snappedX = SnapCoordinate(position.X, canvasSize.Width, threshold);
        var snappedY = SnapCoordinate(position.Y, canvasSize.Height, threshold);

        return new Point(snappedX, snappedY);
    }

    /// <summary>
    /// Determines if a position is currently snapped to a grid point.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="canvasSize">The size of the canvas.</param>
    /// <param name="threshold">The distance threshold in pixels for snapping. Default is 10.</param>
    /// <returns>True if the position is snapped to a grid point, false otherwise.</returns>
    public static bool IsSnapped(Point position, Size canvasSize, int threshold = 10)
    {
        var snappedPosition = SnapPosition(position, canvasSize, threshold);
        return position.X == snappedPosition.X && position.Y == snappedPosition.Y;
    }

    /// <summary>
    /// Snaps a single coordinate to the nearest grid line if within threshold.
    /// </summary>
    /// <param name="coordinate">The coordinate value to snap.</param>
    /// <param name="dimension">The canvas dimension (width or height).</param>
    /// <param name="threshold">The distance threshold in pixels for snapping.</param>
    /// <returns>The snapped coordinate, or the original coordinate if no snap target is nearby.</returns>
    private static double SnapCoordinate(double coordinate, double dimension, int threshold)
    {
        // Define snap percentages: 0%, 25%, 50%, 75%, 100%
        var snapPercentages = new[] { 0.0, 0.25, 0.5, 0.75, 1.0 };

        foreach (var percentage in snapPercentages)
        {
            var snapTarget = dimension * percentage;
            var distance = Math.Abs(coordinate - snapTarget);

            if (distance <= threshold)
            {
                return snapTarget;
            }
        }

        // No snap target nearby, return original coordinate
        return coordinate;
    }
}
