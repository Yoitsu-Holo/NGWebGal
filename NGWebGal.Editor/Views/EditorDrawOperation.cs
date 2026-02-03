using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using NGWebGal.Editor.ViewModels;
using NGWebGal.Layer;
using SkiaSharp;

namespace NGWebGal.Editor.Views;

public class EditorDrawOperation : ICustomDrawOperation
{
    private readonly Rect _bounds;
    private readonly List<(WidgetViewModel vm, ILayer? layer)> _widgets;
    private readonly WidgetViewModel? _selectedWidget;
    private readonly int _hashCode; // Cache hash code

    public EditorDrawOperation(
        Rect bounds,
        List<(WidgetViewModel, ILayer?)> widgets,
        WidgetViewModel? selectedWidget)
    {
        _bounds = bounds;
        _widgets = widgets;
        _selectedWidget = selectedWidget;

        // Calculate hash code once during construction
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + _bounds.GetHashCode();
            hash = hash * 31 + (_selectedWidget?.GetHashCode() ?? 0);
            foreach (var (vm, _) in _widgets)
            {
                hash = hash * 31 + vm.GetHashCode();
                hash = hash * 31 + vm.X;
                hash = hash * 31 + vm.Y;
                hash = hash * 31 + vm.Width;
                hash = hash * 31 + vm.Height;
            }
            _hashCode = hash;
        }
    }

    public Rect Bounds => _bounds;

    public bool HitTest(Point p) => false;

    public bool Equals(ICustomDrawOperation? other)
    {
        // If it's the exact same instance, return true
        if (ReferenceEquals(this, other))
            return true;

        // If it's not an EditorDrawOperation, return false
        if (other is not EditorDrawOperation otherOp)
            return false;

        // Compare hash codes first (fast check)
        if (_hashCode != otherOp._hashCode)
            return false;

        // Compare bounds
        if (_bounds != otherOp._bounds)
            return false;

        // Compare selected widget
        if (_selectedWidget != otherOp._selectedWidget)
            return false;

        // Compare widget count
        if (_widgets.Count != otherOp._widgets.Count)
            return false;

        // Compare each widget's position and size
        for (int i = 0; i < _widgets.Count; i++)
        {
            var (vm1, _) = _widgets[i];
            var (vm2, _) = otherOp._widgets[i];

            if (vm1 != vm2 || vm1.X != vm2.X || vm1.Y != vm2.Y ||
                vm1.Width != vm2.Width || vm1.Height != vm2.Height)
            {
                return false;
            }
        }

        System.Console.WriteLine("[EditorDrawOperation.Equals] Operations are EQUAL - skipping render");
        return true;
    }

    public void Dispose() { }

    public void Render(ImmediateDrawingContext context)
    {
        System.Console.WriteLine($"[EditorDrawOperation.Render] START - {_widgets.Count} widgets");

        var leaseFeature = context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature));
        if (leaseFeature == null)
        {
            System.Console.WriteLine($"[EditorDrawOperation.Render] ERROR: leaseFeature is NULL!");
            return;
        }

        System.Console.WriteLine($"[EditorDrawOperation.Render] Got leaseFeature, getting lease");
        var lease = ((ISkiaSharpApiLeaseFeature)leaseFeature).Lease();
        var canvas = lease.SkCanvas;
        System.Console.WriteLine($"[EditorDrawOperation.Render] Got SKCanvas");

        // Render each widget via Core Layer
        int rendered = 0;
        foreach (var (vm, layer) in _widgets)
        {
            if (layer == null)
            {
                System.Console.WriteLine($"[EditorDrawOperation.Render] Widget {vm.Name} has null layer, skipping");
                continue;
            }

            System.Console.WriteLine($"[EditorDrawOperation.Render] Rendering widget {vm.Name} ({vm.Type}) at ({vm.X}, {vm.Y})");

            try
            {
                // DON'T modify layer properties here - they should be set when the widget moves
                // Only update position/size if they've actually changed
                var currentPos = layer.Position;
                var currentSize = layer.Size;

                if (currentPos.X != vm.X || currentPos.Y != vm.Y)
                {
                    System.Console.WriteLine($"[EditorDrawOperation.Render] Position changed, updating");
                    layer.Position = new NGWebGal.Types.IVector(vm.X, vm.Y);
                }

                if (currentSize.X != vm.Width || currentSize.Y != vm.Height)
                {
                    System.Console.WriteLine($"[EditorDrawOperation.Render] Size changed, updating");
                    layer.Size = new NGWebGal.Types.IVector(vm.Width, vm.Height);
                }

                System.Console.WriteLine($"[EditorDrawOperation.Render] Calling layer.Render()");
                layer.Render(canvas, force: true);

                System.Console.WriteLine($"[EditorDrawOperation.Render] Widget {vm.Name} rendered successfully");
                rendered++;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[EditorDrawOperation.Render] ERROR rendering {vm.Name}: {ex.Message}");
                System.Console.WriteLine($"[EditorDrawOperation.Render] Stack: {ex.StackTrace}");
            }
        }

        System.Console.WriteLine($"[EditorDrawOperation.Render] Rendered {rendered} widgets, now drawing selection border");

        // Draw selection border if needed
        if (_selectedWidget != null)
        {
            try
            {
                using var borderPaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3,
                    IsAntialias = true
                };
                canvas.DrawRect(_selectedWidget.X, _selectedWidget.Y,
                              _selectedWidget.Width, _selectedWidget.Height, borderPaint);
                System.Console.WriteLine($"[EditorDrawOperation.Render] Selection border drawn");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[EditorDrawOperation.Render] ERROR drawing selection: {ex.Message}");
            }
        }

        System.Console.WriteLine($"[EditorDrawOperation.Render] COMPLETE");
    }
}
