using NGWebGal.Types;

namespace NGWebGal.Animations;

public interface IAnimation
{
	/// <summary>
	/// Execute animation and modify animation data
	/// </summary>
	/// <param name="aniData">Animation data to modify</param>
	/// <param name="timeOff">Time offset in milliseconds</param>
	void DoAnimation(ref AnimationData aniData, long timeOff);

	/// <summary>
	/// Set animation parameters
	/// </summary>
	/// <param name="parama">Animation-specific parameter object</param>
	void SetParama(object parama);
}
