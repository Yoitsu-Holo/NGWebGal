using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using NGWebGal.Animations;
using NGWebGal.Extend.SkiaSharp;
using NGWebGal.Handler;
using NGWebGal.Types;

namespace NGWebGal.Layer;

/// <summary>
/// 包含最基本的控制组件方法.
/// 其行为是一个可以被拖动的简单方块
///
/// 1. 多图层（惰性叠加）
/// 2. 基础属性设置
/// 	- 文本
/// 	- 位置
/// 	- 大小
/// 	- 可见性
/// 3. 控制
/// 	- 跟踪事件
/// 	- 传入事件自动处理
///
/// 目前没有区分 visiable 和 disable。
/// 原则上，visiable 控制是否显示。
/// 而 diasble 会显示，但是不能操作。
/// </summary>
public class LayerBase : HandlerBase, ILayer
{
	protected bool _dirty = true;
	public LayerStatus Status = LayerStatus.Normal;
	protected bool _attributeChange = true;
	protected AnimationData _animationData = new();
	private readonly SortedDictionary<int, IAnimation> _animation = [];


	#region 外界交互
	// 渲染图像
	public virtual void Render(SKCanvas canvas, bool force) => throw new NotImplementedException();
	public virtual bool HasAnimation(long timeOff) => true;
	#endregion


	#region 自身设置
	// 设置图片属性 (不作为基本属性设置)

	/// <summary>
	/// Sets image from bitmap with optional image window
	/// </summary>
	public virtual void SetImage(SKBitmap image, int imageId, IRect? imageWindow = null)
		=> throw new NotImplementedException();

	public virtual void SetColor(SKColor color, int imageId = 0) => throw new NotImplementedException();

	/// <summary>
	/// Gets a copy of the color at the specified image ID.
	/// Default implementation returns Transparent.
	/// Override in subclasses that support color.
	/// </summary>
	public virtual SKColor GetColor(int imageId = 0) => SKColors.Transparent;

	/// <summary>
	/// Gets the image at the specified image ID.
	/// Default implementation returns null.
	/// Override in subclasses that support images.
	/// </summary>
	public virtual SKImage? GetImage(int imageId = 0) => null;

	/// <summary>
	/// Gets all image IDs currently stored
	/// </summary>
	public virtual int[] GetImageIds() => Array.Empty<int>();

	public virtual void ResetAnimationData() { _animationData = new(); }
	public virtual void AddAnimation(int id, IAnimation animation) => _animation[id] = animation;
	public virtual void ClearAnimation() => _animation.Clear();
	public virtual void DoAnimation(long timeOff)
	{
		ResetAnimationData();
		foreach (var animation in _animation) animation.Value.DoAnimation(ref _animationData, timeOff);
	}

	public void SetAnimationData(int id, object animationData) => _animation[id].SetParama(animationData);


	// 设置位置属性
	protected IVector _position = new();
	protected IVector _offset = new();
	protected IVector _size = new();
	public virtual IVector Position { get => _position; set => (_position, _dirty) = (value, true); }
	public IVector Offset { get => _offset; set => (_offset, _dirty) = (value, true); }

	public virtual IVector Size { get => _size; set => (_size, _dirty) = (value, true); }
	public virtual IRect Window { get { return new(Position + Offset, Size); } }


	// 设置文本属性
	protected string _text = "";
	protected int _textSize = 0;
	protected SKTypeface _typeface = SKTypeface.FromFamilyName("Microsoft YaHei");
	public virtual string Text { get => _text; set => (_text, _dirty) = (value, true); }
	public virtual int TextSize { get => _textSize; set => (_textSize, _dirty) = (value, true); }
	public virtual SKTypeface Typeface { get => _typeface; set => (_typeface, _dirty) = (value, true); }


	public virtual bool Enable { get => Status != LayerStatus.Disable; set => (Status, _dirty) = (value ? LayerStatus.Normal : LayerStatus.Disable, true); }
	public virtual bool Visible { get => Status != LayerStatus.Unvisable; set => (Status, _dirty) = (value ? LayerStatus.Normal : LayerStatus.Unvisable, true); }

	protected string _name = "";
	protected object _value = new();

	public virtual string Name { get => _name; set => (_name, _dirty) = (value, true); }
	public virtual object Value { get => _value; set => (_value, _dirty) = (value, true); }
	#endregion
}

public class WidgetLayerBase : LayerBase { }
public class ControllerLayerBase : LayerBase { }