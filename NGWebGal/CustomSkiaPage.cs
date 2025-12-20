using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Avalonia.Utilities;
using SkiaSharp;

namespace NGWebGal;

public class CustomSkiaPage : Control
{
    private UInt64 frameCount = 0;
    private int mtime = 0;
    private readonly GlyphRun _noSkia;
    public CustomSkiaPage()
    {
        ClipToBounds = true;
        var text = "Current rendering API is not Skia";
        var glyphs = text.Select(ch => Typeface.Default.GlyphTypeface.GetGlyph(ch)).ToArray();
        _noSkia = new GlyphRun(Typeface.Default.GlyphTypeface, 12, text.AsMemory(), glyphs);
    }

    class CustomDrawOp : ICustomDrawOperation
    {
        private readonly IImmutableGlyphRunReference? _noSkia;

        public CustomDrawOp(Rect bounds, GlyphRun noSkia)
        {
            _noSkia = noSkia.TryCreateImmutableGlyphRunReference();
            Bounds = bounds;
        }

        public void Dispose()
        {
            // No-op
        }

        public Rect Bounds { get; }
        public bool HitTest(Point p) => false;
        public bool Equals(ICustomDrawOperation? other) => false;
        static Stopwatch St = Stopwatch.StartNew();
        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (_noSkia == null)
                return;
            if (leaseFeature == null)
                context.DrawGlyphRun(Brushes.Black, _noSkia);
            else
            {
                using var lease = leaseFeature.Lease();
                var canvas = lease.SkCanvas;

                // 使用提取出的绘图函数
                SkiaRenderer.RenderAnimatedEffect(canvas, Bounds, St.Elapsed.TotalSeconds);
            }
        }
    }

    public override void Render(DrawingContext context)
    {
        frameCount++;
        if (DateTime.Now.Millisecond + DateTime.Now.Second * 1000 > mtime + 1000)
        {
            mtime = DateTime.Now.Millisecond + DateTime.Now.Second * 1000;
            Console.WriteLine(frameCount);
            frameCount = 0;
        }
        context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), _noSkia));
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }
}
