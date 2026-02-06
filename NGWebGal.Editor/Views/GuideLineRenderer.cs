using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace NGWebGal.Editor.Views;

/// <summary>
/// Static helper class for rendering guide lines on canvas
/// </summary>
public static class GuideLineRenderer
{
    private static readonly Pen PercentageLinePen = new Pen(
        Brushes.LightGray,
        1,
        new DashStyle(new double[] { 4, 4 }, 0)
    );

    private static readonly Pen GridLinePen = new Pen(
        new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0)),
        1
    );

    private static readonly Pen CrosshairPen = new Pen(
        new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00)),
        2,
        new DashStyle(new double[] { 4, 4 }, 0)
    );

    /// <summary>
    /// Renders percentage-based guide lines at specified positions
    /// </summary>
    /// <param name="context">Drawing context</param>
    /// <param name="canvasSize">Size of the canvas</param>
    /// <param name="positions">List of percentage positions (0.0-1.0, e.g., 0.5 for 50%)</param>
    public static void RenderPercentageLines(DrawingContext context, Size canvasSize, List<double> positions)
    {
        if (context == null || positions == null || positions.Count == 0)
            return;

        foreach (var percentage in positions)
        {
            if (percentage < 0 || percentage > 1.0)
                continue;

            // Horizontal line
            var y = canvasSize.Height * percentage;
            context.DrawLine(
                PercentageLinePen,
                new Point(0, y),
                new Point(canvasSize.Width, y)
            );

            // Vertical line
            var x = canvasSize.Width * percentage;
            context.DrawLine(
                PercentageLinePen,
                new Point(x, 0),
                new Point(x, canvasSize.Height)
            );
        }
    }

    /// <summary>
    /// Renders grid lines at specified spacing intervals
    /// </summary>
    /// <param name="context">Drawing context</param>
    /// <param name="canvasSize">Size of the canvas</param>
    /// <param name="spacing">Spacing between grid lines in pixels</param>
    public static void RenderGridLines(DrawingContext context, Size canvasSize, int spacing)
    {
        if (context == null || spacing <= 0)
            return;

        // Draw vertical grid lines
        for (double x = spacing; x < canvasSize.Width; x += spacing)
        {
            context.DrawLine(
                GridLinePen,
                new Point(x, 0),
                new Point(x, canvasSize.Height)
            );
        }

        // Draw horizontal grid lines
        for (double y = spacing; y < canvasSize.Height; y += spacing)
        {
            context.DrawLine(
                GridLinePen,
                new Point(0, y),
                new Point(canvasSize.Width, y)
            );
        }
    }

    /// <summary>
    /// Renders a dynamic crosshair at the specified center point
    /// </summary>
    /// <param name="context">Drawing context</param>
    /// <param name="center">Center point of the crosshair</param>
    /// <param name="canvasSize">Size of the canvas</param>
    public static void RenderDynamicCrosshair(DrawingContext context, Point center, Size canvasSize)
    {
        if (context == null)
            return;

        // Draw horizontal line through center
        context.DrawLine(
            CrosshairPen,
            new Point(0, center.Y),
            new Point(canvasSize.Width, center.Y)
        );

        // Draw vertical line through center
        context.DrawLine(
            CrosshairPen,
            new Point(center.X, 0),
            new Point(center.X, canvasSize.Height)
        );
    }
}
