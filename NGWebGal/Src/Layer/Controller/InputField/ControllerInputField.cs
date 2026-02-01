using System;
using System.Collections.Generic;
using Avalonia.Input;
using SkiaSharp;
using NGWebGal.Global;
using NGWebGal.Types;
using NGWebGal.Extensions;
using NGWebGal.Handler.Event;

namespace NGWebGal.Layer.Controller;

/// <summary>
/// Interactive text input field controller with 4-state support (normal, hover, focused, disabled)
/// Supports keyboard input, cursor navigation, and text editing
/// </summary>
public class ControllerInputField : ControllerLayerBase
{
    protected List<SKBitmap> _imageBuffer = [];
    protected List<SKBitmap> _renderBuffer = [];

    private int _cursorPosition = 0;
    private long _lastBlinkTime = 0;
    private bool _cursorVisible = true;
    private const int BlinkIntervalMs = 500;

    /// <summary>
    /// Current cursor position (character index)
    /// </summary>
    public int CursorPosition
    {
        get => _cursorPosition;
        set
        {
            _cursorPosition = Math.Clamp(value, 0, _text.Length);
            _cursorVisible = true;
            _lastBlinkTime = NowTime.Minisecond;
            _dirty = true;
        }
    }

    /// <summary>
    /// Override Text property to ensure cursor stays valid
    /// </summary>
    public override string Text
    {
        get => _text;
        set
        {
            _text = value ?? "";
            _cursorPosition = Math.Clamp(_cursorPosition, 0, _text.Length);
            _dirty = true;
        }
    }

    /// <summary>
    /// Override Value to return Text content
    /// </summary>
    public override object Value
    {
        get => _text;
        set
        {
            Text = value?.ToString() ?? "";
        }
    }

    public ControllerInputField()
    {
        // Initialize buffers for 4 states: Normal, Hover, Focused, Disabled
        for (int i = 0; i < 4; i++)
        {
            _imageBuffer.Add(new());
            _renderBuffer.Add(new());
        }
    }

    public override void SetImage(SKBitmap image, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        _imageBuffer[imageId] = image;
        _dirty = true;
    }

    public override void SetImage(SKBitmap image, IRect imageWindow, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        if (imageWindow == default)
            SetImage(image, imageId);
        else
            _imageBuffer[imageId] = image.SubBitmap(imageWindow);
        _dirty = true;
    }

    public override void SetColor(SKColor color, int imageId = 0)
    {
        if (imageId >= _imageBuffer.Count)
            return;
        SKBitmap bitmap = new(Size.X, Size.Y, RenderConfig.DefaultColorType, RenderConfig.DefaultAlphaType);
        using SKCanvas canvas = new(bitmap);
        canvas.DrawRect(
            new SKRect(0, 0, Size.X, Size.Y),
            new SKPaint { Color = color }
        );
        canvas.Flush();
        _imageBuffer[imageId] = bitmap;
        _dirty = true;
    }

    public override bool DoAction(EventArgs eventArgs)
    {
        if (Status == LayerStatus.Disable) return false;

        if (eventArgs is MouseEventData mouseEvent)
            return HandleMouseEvent(mouseEvent);

        if (eventArgs is KeyboardEventData keyboardEvent)
            return HandleKeyboardEvent(keyboardEvent);

        return false;
    }

    private bool HandleMouseEvent(MouseEventData mouseEvent)
    {
        bool isInside = RangeComp.InRange(Window, mouseEvent.Position);
        LayerStatus nowStatus = Status;

        // Handle click to focus/unfocus
        if (mouseEvent.Button == Handler.Event.MouseButton.LButton && mouseEvent.Status == MouseStatus.Down)
        {
            if (isInside)
            {
                nowStatus = LayerStatus.Focused;
                // Position cursor based on click position
                PositionCursorFromClick(mouseEvent.Position);
            }
            else if (Status == LayerStatus.Focused)
            {
                // Click outside loses focus
                nowStatus = LayerStatus.Normal;
            }
        }
        else if (Status != LayerStatus.Focused)
        {
            // Hover handling when not focused
            if (!isInside)
                nowStatus = LayerStatus.Normal;
            else if (mouseEvent.Button == Handler.Event.MouseButton.Empty && mouseEvent.Status == MouseStatus.Release)
                nowStatus = LayerStatus.Hover;
        }

        if (nowStatus != Status)
        {
            Status = nowStatus;
            TriggerEvent(mouseEvent);
            return true;
        }

        return false;
    }

    private void PositionCursorFromClick(IVector clickPos)
    {
        if (string.IsNullOrEmpty(_text))
        {
            CursorPosition = 0;
            return;
        }

        // Calculate relative X position within the text area
        int relativeX = clickPos.X - Position.X - Offset.X - TextPaddingLeft;

        using var font = new SKFont(_typeface, _textSize > 0 ? _textSize : 16);
        using var paint = new SKPaint(font) { IsAntialias = true };

        // Find the character position closest to the click
        float totalWidth = 0;
        for (int i = 0; i < _text.Length; i++)
        {
            float charWidth = paint.MeasureText(_text[i].ToString());
            if (totalWidth + charWidth / 2 > relativeX)
            {
                CursorPosition = i;
                return;
            }
            totalWidth += charWidth;
        }

        CursorPosition = _text.Length;
    }

    private bool HandleKeyboardEvent(KeyboardEventData keyboardEvent)
    {
        // Only handle keyboard when focused
        if (Status != LayerStatus.Focused) return false;

        // Only handle key down/press events
        if (keyboardEvent.Status != KeyStatus.Down && keyboardEvent.Status != KeyStatus.Press)
            return false;

        bool handled = false;

        switch (keyboardEvent.Key)
        {
            case Key.Back:
                handled = HandleBackspace();
                break;
            case Key.Delete:
                handled = HandleDelete();
                break;
            case Key.Left:
                handled = HandleLeftArrow(keyboardEvent.Modifiers);
                break;
            case Key.Right:
                handled = HandleRightArrow(keyboardEvent.Modifiers);
                break;
            case Key.Home:
                CursorPosition = 0;
                handled = true;
                break;
            case Key.End:
                CursorPosition = _text.Length;
                handled = true;
                break;
            default:
                // Handle text input
                if (!string.IsNullOrEmpty(keyboardEvent.Text))
                {
                    handled = InsertText(keyboardEvent.Text);
                }
                break;
        }

        if (handled)
        {
            TriggerEvent(keyboardEvent);
            _dirty = true;
        }

        return handled;
    }

    private bool HandleBackspace()
    {
        if (_cursorPosition > 0)
        {
            _text = _text.Remove(_cursorPosition - 1, 1);
            _cursorPosition--;
            return true;
        }
        return false;
    }

    private bool HandleDelete()
    {
        if (_cursorPosition < _text.Length)
        {
            _text = _text.Remove(_cursorPosition, 1);
            return true;
        }
        return false;
    }

    private bool HandleLeftArrow(Handler.Event.KeyModifiers modifiers)
    {
        if (_cursorPosition > 0)
        {
            if ((modifiers & Handler.Event.KeyModifiers.Control) != 0)
            {
                // Move to previous word boundary
                _cursorPosition = FindPreviousWordBoundary();
            }
            else
            {
                _cursorPosition--;
            }
            _cursorVisible = true;
            _lastBlinkTime = NowTime.Minisecond;
            return true;
        }
        return false;
    }

    private bool HandleRightArrow(Handler.Event.KeyModifiers modifiers)
    {
        if (_cursorPosition < _text.Length)
        {
            if ((modifiers & Handler.Event.KeyModifiers.Control) != 0)
            {
                // Move to next word boundary
                _cursorPosition = FindNextWordBoundary();
            }
            else
            {
                _cursorPosition++;
            }
            _cursorVisible = true;
            _lastBlinkTime = NowTime.Minisecond;
            return true;
        }
        return false;
    }

    private int FindPreviousWordBoundary()
    {
        int pos = _cursorPosition - 1;
        // Skip whitespace
        while (pos > 0 && char.IsWhiteSpace(_text[pos]))
            pos--;
        // Skip word characters
        while (pos > 0 && !char.IsWhiteSpace(_text[pos - 1]))
            pos--;
        return pos;
    }

    private int FindNextWordBoundary()
    {
        int pos = _cursorPosition;
        // Skip word characters
        while (pos < _text.Length && !char.IsWhiteSpace(_text[pos]))
            pos++;
        // Skip whitespace
        while (pos < _text.Length && char.IsWhiteSpace(_text[pos]))
            pos++;
        return pos;
    }

    private bool InsertText(string text)
    {
        // Filter out control characters
        string filtered = "";
        foreach (char c in text)
        {
            if (!char.IsControl(c))
                filtered += c;
        }

        if (string.IsNullOrEmpty(filtered))
            return false;

        _text = _text.Insert(_cursorPosition, filtered);
        _cursorPosition += filtered.Length;
        return true;
    }

    // Text rendering padding
    private const int TextPaddingLeft = 8;
    private const int TextPaddingRight = 8;

    public override void Render(SKCanvas canvas, bool force)
    {
        if (Status == LayerStatus.Unvisable || _imageBuffer is null || _imageBuffer[0].IsNull)
            return;

        // Update cursor blink state
        UpdateCursorBlink();

        if (_dirty || force || _renderBuffer[GetStateIndex()].IsNull)
        {
            PrepareRenderBuffers();
            _dirty = false;
        }

        // Draw background
        DrawBackground(canvas);

        // Draw text and cursor
        DrawTextAndCursor(canvas);
    }

    private int GetStateIndex()
    {
        return Status switch
        {
            LayerStatus.Normal => 0,
            LayerStatus.Hover => 1,
            LayerStatus.Focused => 2,
            LayerStatus.Disable => 3,
            _ => 0
        };
    }

    private void PrepareRenderBuffers()
    {
        // Fill missing states with previous state images
        for (int i = 1; i < 4; i++)
            if (_imageBuffer[i].IsNull)
                _imageBuffer[i] = _imageBuffer[i - 1];

        // Resize all images to target size
        for (int i = 0; i < 4; i++)
            _renderBuffer[i] = _imageBuffer[i].Resize(Size);
    }

    private void UpdateCursorBlink()
    {
        if (Status != LayerStatus.Focused)
        {
            _cursorVisible = false;
            return;
        }

        long currentTime = NowTime.Minisecond;
        if (currentTime - _lastBlinkTime >= BlinkIntervalMs)
        {
            _cursorVisible = !_cursorVisible;
            _lastBlinkTime = currentTime;
        }
    }

    private void DrawBackground(SKCanvas canvas)
    {
        SKMatrix matrix = SKMatrix.Identity;
        FVector pos = (FVector)Position + _animationData.PosOff;

        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(canvas.TotalMatrix.ScaleX, canvas.TotalMatrix.ScaleY));
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(pos.X, pos.Y));
        matrix = SKMatrix.Concat(matrix, _animationData.Transform);
        matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(_offset.X, _offset.Y));

        canvas.Save();
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(_renderBuffer[GetStateIndex()], new SKPoint(0, 0), _animationData.Paint);
        canvas.Restore();
    }

    private void DrawTextAndCursor(SKCanvas canvas)
    {
        FVector pos = (FVector)Position + _animationData.PosOff + (FVector)_offset;

        int fontSize = _textSize > 0 ? _textSize : 16;
        using var font = new SKFont(_typeface, fontSize);
        using var paint = new SKPaint(font)
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        // Calculate text position (vertically centered)
        float textY = pos.Y + (Size.Y + fontSize) / 2f - fontSize * 0.15f;
        float textX = pos.X + TextPaddingLeft;

        // Draw text
        if (!string.IsNullOrEmpty(_text))
        {
            canvas.DrawText(_text, textX, textY, font, paint);
        }

        // Draw cursor when focused and visible
        if (Status == LayerStatus.Focused && _cursorVisible)
        {
            float cursorX = textX;
            if (_cursorPosition > 0 && !string.IsNullOrEmpty(_text))
            {
                string textBeforeCursor = _text.Substring(0, _cursorPosition);
                cursorX += paint.MeasureText(textBeforeCursor);
            }

            using var cursorPaint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1.5f,
                IsAntialias = true
            };

            float cursorTop = pos.Y + (Size.Y - fontSize) / 2f;
            float cursorBottom = cursorTop + fontSize;
            canvas.DrawLine(cursorX, cursorTop, cursorX, cursorBottom, cursorPaint);
        }
    }
}
