using System;
using Avalonia;
using SkiaSharp;

namespace NGWebGal;

/// <summary>
/// 提供 Skia 绘图功能的静态类
/// </summary>
public static class SkiaRenderer
{
    private static readonly DateTime StartTime = DateTime.Now;

    /// <summary>
    /// 在指定的 SKCanvas 上绘制动画效果
    /// </summary>
    /// <param name="canvas">Skia 画布</param>
    /// <param name="bounds">绘制区域的边界</param>
    /// <param name="elapsedSeconds">已经过的秒数（用于动画）</param>
    public static void RenderAnimatedEffect(SKCanvas canvas, Rect bounds, double elapsedSeconds)
    {
        canvas.Save();

        // 创建颜色渐变
        var colors = new SKColor[] {
            new SKColor(0, 255, 255),
            new SKColor(255, 0, 255),
            new SKColor(255, 255, 0),
            new SKColor(0, 255, 255)
        };

        // 计算光源位置
        var lightPosition = new SKPoint(
            (float)(bounds.Width / 2 + Math.Cos(elapsedSeconds) * bounds.Width / 4),
            (float)(bounds.Height / 2 + Math.Sin(elapsedSeconds) * bounds.Height / 4));

        // 计算动画参数
        var elapsedMs = (long)(elapsedSeconds * 1000);
        var blurX = Animate(elapsedMs, 100, 2, 10);
        var blurY = Animate(elapsedMs, 100, 5, 15);
        var alphaValue = Animate(elapsedMs, 100, 200, 220);

        // 绘制渐变和噪声效果
        using (var sweep = SKShader.CreateSweepGradient(
            new SKPoint((int)bounds.Width / 2, (int)bounds.Height / 2), colors, null))
        using (var turbulence = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0))
        using (var shader = SKShader.CreateCompose(sweep, turbulence, SKBlendMode.SrcATop))
        using (var blur = SKImageFilter.CreateBlur(blurX, blurY))
        using (var paint = new SKPaint
        {
            Shader = shader,
            ImageFilter = blur
        })
            canvas.DrawPaint(paint);

        // 绘制伪光照效果
        using (var pseudoLight = SKShader.CreateRadialGradient(
            lightPosition,
            (float)(bounds.Width / 3),
            new[] {
                new SKColor(255, 200, 200, 100),
                SKColors.Transparent,
                new SKColor(40, 40, 40, 220),
                new SKColor(20, 20, 20, (byte)alphaValue)
            },
            new float[] { 0.3f, 0.3f, 0.8f, 1 },
            SKShaderTileMode.Clamp))
        using (var paint = new SKPaint
        {
            Shader = pseudoLight
        })
            canvas.DrawPaint(paint);

        canvas.Restore();
    }

    /// <summary>
    /// 在指定的 SKCanvas 上绘制动画效果（使用默认计时）
    /// </summary>
    /// <param name="canvas">Skia 画布</param>
    /// <param name="bounds">绘制区域的边界</param>
    public static void RenderAnimatedEffect(SKCanvas canvas, Rect bounds)
    {
        var elapsed = (DateTime.Now - StartTime).TotalSeconds;
        RenderAnimatedEffect(canvas, bounds, elapsed);
    }

    /// <summary>
    /// 动画辅助函数，在指定范围内循环往返
    /// </summary>
    /// <param name="elapsedMs">已经过的毫秒数</param>
    /// <param name="duration">动画周期（毫秒）</param>
    /// <param name="from">起始值</param>
    /// <param name="to">结束值</param>
    /// <returns>当前动画值</returns>
    private static int Animate(long elapsedMs, int duration, int from, int to)
    {
        var ms = (int)(elapsedMs / duration);
        var diff = to - from;
        var range = diff * 2;
        var v = ms % range;
        if (v > diff)
            v = range - v;
        var rv = v + from;
        if (rv < from || rv > to)
            throw new Exception("Animation value out of range");
        return rv;
    }
}
