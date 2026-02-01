using System;
using Avalonia;
using SkiaSharp;
using NGWebGal.Layer;
using NGWebGal.Layer.Widget;
using NGWebGal.Layer.Controller;
using NGWebGal.Handler.Event;
using NGWebGal.Types;

namespace NGWebGal.Example;

/// <summary>
/// Example UI page demonstrating various rendering capabilities.
/// This class handles all example UI rendering independently from GameView.
/// </summary>
public class ExamplePage
{
    private readonly Layout _layout = new();

    public ExamplePage()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Layer key scheme for z-ordering:
        // 10-17: WidgetColorBox (8 color blocks)
        // 20-22: ControllerSliderHorizontal (3 sliders)
        // 30-33: ControllerButton (4 buttons)
        // 40-42: WidgetProgressBar (3 progress bars)
        // 50-53: ControllerCheckbox (4 checkboxes)
        // 60-61: ControllerToggle (2 toggles)

        // Color blocks (8 blocks)
        var colors = new[]
        {
            new SKColor(255, 100, 100), // Red
            new SKColor(100, 255, 100), // Green
            new SKColor(100, 100, 255), // Blue
            new SKColor(255, 255, 100), // Yellow
            new SKColor(255, 100, 255), // Magenta
            new SKColor(100, 255, 255), // Cyan
            new SKColor(255, 150, 50),  // Orange
            new SKColor(150, 100, 255)  // Purple
        };

        for (int i = 0; i < colors.Length; i++)
        {
            var colorBlock = new WidgetColorBox();
            colorBlock.Size = new IVector(80, 60);  // MUST be before SetColor
            colorBlock.Position = new IVector(20 + (i % 4) * 90, 230 + (i / 4) * 70);
            colorBlock.SetColor(colors[i]);
            _layout.Layers[10 + i] = colorBlock;
        }

        // Alpha overlay block
        var alphaBlock = new WidgetColorBox();
        alphaBlock.Size = new IVector(100, 130);
        alphaBlock.Position = new IVector(380, 230);
        alphaBlock.SetColor(new SKColor(255, 255, 255, 100));
        _layout.Layers[18] = alphaBlock;

        // Buttons (4 buttons)
        var buttonData = new[]
        {
            new { Label = "Start", X = 20, Color = new SKColor(100, 200, 100) },
            new { Label = "Stop", X = 130, Color = new SKColor(255, 100, 100) },
            new { Label = "Pause", X = 240, Color = new SKColor(255, 200, 100) },
            new { Label = "Reset", X = 350, Color = new SKColor(150, 150, 200) }
        };

        for (int i = 0; i < buttonData.Length; i++)
        {
            var button = new ControllerButton();
            button.Size = new IVector(100, 40);
            button.Position = new IVector(buttonData[i].X, 430);
            button.Name = buttonData[i].Label;

            // Normal state
            button.SetColor(buttonData[i].Color, 0);
            // Hover state (slightly brighter)
            var hoverColor = new SKColor(
                (byte)Math.Min(255, buttonData[i].Color.Red * 1.2),
                (byte)Math.Min(255, buttonData[i].Color.Green * 1.2),
                (byte)Math.Min(255, buttonData[i].Color.Blue * 1.2)
            );
            button.SetColor(hoverColor, 1);
            // Pressed state (darker)
            var pressedColor = new SKColor(
                (byte)(buttonData[i].Color.Red * 0.6),
                (byte)(buttonData[i].Color.Green * 0.6),
                (byte)(buttonData[i].Color.Blue * 0.6)
            );
            button.SetColor(pressedColor, 2);

            _layout.Layers[30 + i] = button;
        }

        // Checkboxes (4 checkboxes)
        var checkboxData = new[]
        {
            new { Label = "Enable Sound", Y = 530, Checked = true },
            new { Label = "Show FPS", Y = 560, Checked = true },
            new { Label = "Fullscreen", Y = 590, Checked = false },
            new { Label = "V-Sync", Y = 620, Checked = true }
        };

        for (int i = 0; i < checkboxData.Length; i++)
        {
            var checkbox = new ControllerCheckbox();
            checkbox.Size = new IVector(20, 20);
            checkbox.Position = new IVector(20, checkboxData[i].Y);
            checkbox.Name = checkboxData[i].Label;
            checkbox.IsChecked = checkboxData[i].Checked;

            // Unchecked normal state
            checkbox.SetColor(new SKColor(60, 60, 70), 0);
            // Checked normal state
            checkbox.SetColor(new SKColor(100, 200, 255), 4);

            _layout.Layers[50 + i] = checkbox;
        }

        // Toggles (2 toggles)
        var toggleData = new[]
        {
            new { Label = "Auto-Save", Y = 532, Checked = true },
            new { Label = "Notifications", Y = 562, Checked = false }
        };

        for (int i = 0; i < toggleData.Length; i++)
        {
            var toggle = new ControllerToggle();
            toggle.Size = new IVector(40, 16);
            toggle.Position = new IVector(250, toggleData[i].Y);
            toggle.Name = toggleData[i].Label;
            toggle.IsChecked = toggleData[i].Checked;

            // Off state (normal)
            toggle.SetColor(new SKColor(60, 60, 70), 0);
            // On state (normal)
            toggle.SetColor(new SKColor(100, 200, 100), 4);

            _layout.Layers[60 + i] = toggle;
        }

        // Sliders (3 sliders)
        var sliderData = new[]
        {
            new { Label = "Volume", Y = 245, Value = 0.75f, Color = new SKColor(100, 200, 255) },
            new { Label = "Brightness", Y = 295, Value = 0.50f, Color = new SKColor(255, 200, 100) },
            new { Label = "Speed", Y = 345, Value = 0.30f, Color = new SKColor(150, 255, 150) }
        };

        for (int i = 0; i < sliderData.Length; i++)
        {
            var slider = new ControllerSliderHorizontal();
            slider.Size = new IVector(200, 10);  // Track size
            slider.ThumbSize = new IVector(16, 16);  // Thumb size
            slider.Position = new IVector(520, sliderData[i].Y);
            slider.Name = sliderData[i].Label;
            slider.Value = sliderData[i].Value;

            // Track background
            slider.SetColor(new SKColor(60, 60, 70), -1);
            // Thumb normal state
            slider.SetColor(SKColors.White, 0);

            _layout.Layers[20 + i] = slider;
        }

        // Progress bars (3 progress bars)
        var progressBarData = new[]
        {
            new { Label = "Loading", Y = 425, FillColor = new SKColor(100, 200, 255), Progress = 0.5f },
            new { Label = "Processing", Y = 465, FillColor = new SKColor(255, 150, 100), Progress = 0.5f },
            new { Label = "Complete", Y = 505, FillColor = new SKColor(100, 255, 100), Progress = 1.0f }
        };

        for (int i = 0; i < progressBarData.Length; i++)
        {
            var progressBar = new WidgetProgressBar();
            progressBar.Size = new IVector(150, 20);
            progressBar.Position = new IVector(620, progressBarData[i].Y);
            progressBar.Name = progressBarData[i].Label;

            // Background track
            progressBar.SetColor(new SKColor(60, 60, 70), 0);
            // Fill bar
            progressBar.SetColor(progressBarData[i].FillColor, 1);
            progressBar.Progress = progressBarData[i].Progress;

            _layout.Layers[40 + i] = progressBar;
        }
    }

    /// <summary>
    /// Renders the complete example UI to the provided canvas.
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to render to</param>
    /// <param name="bounds">The bounds of the rendering area</param>
    /// <param name="elapsedSeconds">Total elapsed time in seconds</param>
    /// <param name="fps">Current frames per second</param>
    /// <param name="deltaTime">Time since last frame in milliseconds</param>
    /// <param name="mousePosition">Current mouse position</param>
    public void Render(SKCanvas canvas, Rect bounds, double elapsedSeconds, double fps, double deltaTime, Point mousePosition, bool isLeftButtonPressed, bool isRightButtonPressed)
    {
        // Clear canvas with dark background
        canvas.Clear(new SKColor(20, 20, 30));

        // Update animated progress bars
        UpdateAnimatedProgressBars(elapsedSeconds);

        // Render all layer components
        _layout.Render(canvas, false);

        // === DIRECT DRAWS (not components) ===
        DrawTitle(canvas);
        DrawStatusInfo(canvas, fps, deltaTime, mousePosition, elapsedSeconds);
        DrawInputFields(canvas, elapsedSeconds);
        DrawMouseCursor(canvas, mousePosition, isLeftButtonPressed, isRightButtonPressed);
    }

    private void UpdateAnimatedProgressBars(double elapsedSeconds)
    {
        // Animated progress based on elapsed time
        float progress1 = (float)((Math.Sin(elapsedSeconds * 2) + 1) / 2);
        float progress2 = (float)((Math.Cos(elapsedSeconds * 1.5) + 1) / 2);

        if (_layout.Layers.TryGetValue(40, out var layer1) && layer1 is WidgetProgressBar bar1)
            bar1.Progress = progress1;
        if (_layout.Layers.TryGetValue(41, out var layer2) && layer2 is WidgetProgressBar bar2)
            bar2.Progress = progress2;
    }

    private void DrawTitle(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(100, 200, 255),
            TextSize = 36,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };
        canvas.DrawText("NGWebGal Test Interface", 20, 50, paint);

        // Underline
        using var linePaint = new SKPaint
        {
            Color = new SKColor(100, 200, 255),
            StrokeWidth = 2,
            IsAntialias = true
        };
        canvas.DrawLine(20, 60, 400, 60, linePaint);
    }

    private void DrawStatusInfo(SKCanvas canvas, double fps, double deltaTime, Point mousePosition, double elapsedSeconds)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 18,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial")
        };

        canvas.DrawText($"FPS: {fps:F1}", 20, 90, paint);
        canvas.DrawText($"Frame Time: {deltaTime:F2}ms", 20, 115, paint);
        canvas.DrawText($"Mouse Position: ({mousePosition.X:F0}, {mousePosition.Y:F0})", 20, 140, paint);
        canvas.DrawText($"Elapsed: {elapsedSeconds:F1}s", 20, 165, paint);
    }




    private void DrawInputFields(SKCanvas canvas, double elapsedSeconds)
    {
        // Section title
        using (var titlePaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            TextSize = 20,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        })
        {
            canvas.DrawText("Input Fields", 520, 510, titlePaint);
        }

        var inputs = new[]
        {
            new { Label = "Username:", Value = "Player1" },
            new { Label = "Password:", Value = "********" },
            new { Label = "Score:", Value = "12345" }
        };

        for (int i = 0; i < inputs.Length; i++)
        {
            var input = inputs[i];
            int y = 530 + i * 40;

            // Label
            using (var labelPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            })
            {
                canvas.DrawText(input.Label, 520, y + 20, labelPaint);
            }

            // Input box background
            using (var boxPaint = new SKPaint
            {
                Color = new SKColor(40, 40, 50),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawRoundRect(620, y, 150, 30, 6, 6, boxPaint);
            }

            // Input box border
            using (var borderPaint = new SKPaint
            {
                Color = new SKColor(100, 150, 200),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                IsAntialias = true
            })
            {
                canvas.DrawRoundRect(620, y, 150, 30, 6, 6, borderPaint);
            }

            // Input text
            using (var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 16,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial")
            })
            {
                canvas.DrawText(input.Value, 630, y + 20, textPaint);
            }

            // Blinking cursor on first field
            if (i == 0 && (int)(elapsedSeconds * 2) % 2 == 0)
            {
                using var cursorPaint = new SKPaint
                {
                    Color = SKColors.White,
                    StrokeWidth = 2,
                    IsAntialias = true
                };
                using var measurePaint = new SKPaint
                {
                    TextSize = 16,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };
                float cursorX = 630 + measurePaint.MeasureText(input.Value);
                canvas.DrawLine(cursorX, y + 8, cursorX, y + 22, cursorPaint);
            }
        }
    }

    private void DrawMouseCursor(SKCanvas canvas, Point mousePosition, bool isLeftButtonPressed, bool isRightButtonPressed)
    {
        // Determine cursor color based on button state
        SKColor cursorColor;
        if (isLeftButtonPressed && isRightButtonPressed)
        {
            // Both buttons pressed: Yellow
            cursorColor = new SKColor(255, 255, 0, 150);
        }
        else if (isLeftButtonPressed)
        {
            // Left button pressed: Red
            cursorColor = new SKColor(255, 0, 0, 150);
        }
        else if (isRightButtonPressed)
        {
            // Right button pressed: Blue
            cursorColor = new SKColor(0, 100, 255, 150);
        }
        else
        {
            // No buttons pressed: Magenta (default)
            cursorColor = new SKColor(255, 100, 255, 150);
        }

        // Draw circle at mouse position
        using (var paint = new SKPaint
        {
            Color = cursorColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        })
        {
            canvas.DrawCircle((float)mousePosition.X, (float)mousePosition.Y, 12, paint);
        }

        // Outer ring with matching color (full opacity)
        SKColor ringColor = new SKColor(cursorColor.Red, cursorColor.Green, cursorColor.Blue);
        using (var ringPaint = new SKPaint
        {
            Color = ringColor,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        })
        {
            canvas.DrawCircle((float)mousePosition.X, (float)mousePosition.Y, 15, ringPaint);
        }
    }

    /// <summary>
    /// Processes mouse events and routes them to layer components.
    /// </summary>
    public void ProcessEvent(MouseEventData eventData)
    {
        _layout.ProcessEvent(eventData);
    }
}
