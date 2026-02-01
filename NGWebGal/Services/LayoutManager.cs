using System;
using System.Collections.Generic;
using SkiaSharp;
using NGWebGal.Layer;

namespace NGWebGal.Services;

/// <summary>
/// Manages all game layouts (main menu, game scene, etc.)
/// Event flow: LayoutManager -> Layout -> Scene -> Layer -> Scene -> Layout -> LayoutManager
/// </summary>
public class LayoutManager
{
    public readonly Dictionary<int, Layout> Layouts = [];
    public int ActiveLayout = 0; // 0: main menu, -1: test layout

    public void Clear() => Layouts.Clear();

    public bool ShouldRender()
    {
        if (Layouts.TryGetValue(ActiveLayout, out Layout? layout))
            if (layout.ShouldRender())
                return true;
        return false;
    }

    public void Render(SKCanvas canvas, bool force)
    {
        if (Layouts.TryGetValue(ActiveLayout, out Layout? layout))
            layout.Render(canvas, force);
    }

    public void ProcessEvent(EventArgs eventdata)
    {
        if (Layouts.TryGetValue(ActiveLayout, out Layout? value))
            value.ProcessEvent(eventdata);
    }

    public Layout? GetLayout(int layoutId)
    {
        return Layouts.TryGetValue(layoutId, out Layout? layout) ? layout : null;
    }

    public Layout GetOrCreateLayout(int layoutId)
    {
        if (!Layouts.TryGetValue(layoutId, out Layout? layout))
        {
            layout = new Layout();
            Layouts[layoutId] = layout;
        }
        return layout;
    }
}
