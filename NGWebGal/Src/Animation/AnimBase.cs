using NGWebGal.Types;

namespace NGWebGal.Animations;

/// <summary>
/// Base animation class - does nothing by default
/// </summary>
public class AnimationBase : IAnimation
{
	public virtual void DoAnimation(ref AnimationData aniData, long timeOff) { }
	public virtual void SetParama(object parama) { }
}
