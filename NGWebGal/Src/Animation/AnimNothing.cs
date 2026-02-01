using NGWebGal.Types;

namespace NGWebGal.Animations;

/// <summary>
/// No-op animation that does nothing
/// </summary>
public class AnimationNothing : AnimationBase
{
	public override void DoAnimation(ref AnimationData data, long timeOff) { }
	public override void SetParama(object parama) { }
}

public record struct AnimationNothingData
{
}
