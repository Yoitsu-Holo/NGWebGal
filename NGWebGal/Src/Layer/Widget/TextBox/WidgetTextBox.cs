using System.Collections.Generic;
using SkiaSharp;
using NGWebGal.Global;
using NGWebGal.Types;

namespace NGWebGal.Layer.Widget;

/// <summary>
/// Widget layer that renders text with wrapping and Unicode support
/// </summary>
public class WidgetTextBox : WidgetLayerBase
{
	public SKPaint TextPaint { get; set; } = RenderConfig.DefaultTextPaint;
	public SKFont TextFont { get; set; } = new();
	public TextBoxStyle Style { get; set; } = new();
	private readonly List<string> _textLine = [];

	public virtual void SetFontSize(int fontSize)
	{
		TextFont.Size = fontSize;
		_dirty = true;
	}

	public override void SetColor(SKColor color, int imageId = 0)
	{
		TextPaint.Color = color;
		_dirty = true;
	}

	public override SKColor GetColor(int imageId = 0)
	{
		return TextPaint.Color;
	}

	public virtual void SetFontStyle(SKTypeface typeFace)
	{
		TextFont.Typeface = typeFace;
		_dirty = true;
	}

	public override void Render(SKCanvas canvas, bool force)
	{
		if (Status == LayerStatus.Unvisable)
			return;

		if (_dirty)
		{
			_textLine.Clear();
			int lineWidth = Size.X - (Style.Padding.Left + Style.Padding.Right);

			string s = "";
			foreach (var c in Text)
			{
				float length = TextPaint.MeasureText(s + c);
				if (length > lineWidth || c == '\n')
				{
					_textLine.Add(s);
					s = "";
				}
				if (c != '\n')
					s += c;
			}

			if (s.Length != 0)
				_textLine.Add(s);
			_dirty = false;
		}

		IVector startPos = Position + new IVector(Style.Padding.Top, Style.Padding.Left);
		for (int i = 0; i < _textLine.Count; i++)
		{
			startPos.Y += Style.MarginTop;
			startPos.Y += (int)TextFont.Size;
			canvas.DrawText(_textLine[i], startPos.X, startPos.Y, TextPaint);
			startPos.Y += Style.MarginBottom;
		}
	}
}
