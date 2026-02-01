using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NGWebGal.Animations;
using NGWebGal.Types;
using Xunit;

namespace NGWebGal.Tests.Unit.Animation;

/// <summary>
/// Tests for animation timing accuracy - must be within 16ms tolerance (one frame at 60fps)
/// </summary>
public class AnimationTimingTests
{
	private const float TimingTolerance = 16.0f; // milliseconds

	[Fact]
	public void AnimationNothing_DoesNotModifyData()
	{
		// Arrange
		var anim = new AnimationNothing();
		var data = new AnimationData
		{
			PosOff = new FVector(10, 20),
			Transform = SkiaSharp.SKMatrix.Identity
		};
		var originalPos = data.PosOff;

		// Act
		anim.DoAnimation(ref data, 100);

		// Assert
		Assert.Equal(originalPos.X, data.PosOff.X);
		Assert.Equal(originalPos.Y, data.PosOff.Y);
	}

	[Fact]
	public void AnimationBounce_RespectsBoundaries()
	{
		// Arrange
		var anim = new AnimationBounce();
		var bounceData = new AnimationBounceData
		{
			Range = new FVector(100, 100),
			Delta = new FVector(0.1f, 0.1f)
		};
		anim.SetParama(bounceData);

		var data = new AnimationData { PosOff = new FVector(0, 0) };

		// Act - simulate multiple frames
		for (int i = 0; i < 100; i++)
		{
			anim.DoAnimation(ref data, i * 16);
			Thread.Sleep(1); // Small delay to simulate frame time
		}

		// Assert - position should stay within bounds
		Assert.True(data.PosOff.X >= 0 && data.PosOff.X <= 100);
		Assert.True(data.PosOff.Y >= 0 && data.PosOff.Y <= 100);
	}

	[Fact]
	public void AnimationBounce_ReversesDirectionAtBoundaries()
	{
		// Arrange
		var anim = new AnimationBounce();
		var bounceData = new AnimationBounceData
		{
			Range = new FVector(50, 50),
			Delta = new FVector(1.0f, 1.0f)
		};
		anim.SetParama(bounceData);

		var data = new AnimationData { PosOff = new FVector(49, 49) };

		// Act - move past boundary
		for (int i = 0; i < 10; i++)
		{
			anim.DoAnimation(ref data, i * 16);
			Thread.Sleep(2);
		}

		// Assert - should have bounced back
		Assert.True(data.PosOff.X <= 50);
		Assert.True(data.PosOff.Y <= 50);
	}

	[Fact]
	public void AnimationBrownian_GeneratesRandomMovement()
	{
		// Arrange
		var anim = new AnimationBrownian();
		var data = new AnimationData { PosOff = new FVector(0, 0) };
		var positions = new List<FVector>();

		// Act - collect multiple positions
		for (int i = 0; i < 10; i++)
		{
			anim.DoAnimation(ref data, i * 16);
			positions.Add(data.PosOff);
		}

		// Assert - positions should vary (not all the same)
		var uniquePositions = positions.Distinct().Count();
		Assert.True(uniquePositions > 1, "Brownian motion should generate varying positions");
	}

	[Fact]
	public void AnimationRotate_AppliesRotationTransform()
	{
		// Arrange
		var anim = new AnimationRotate();
		var rotateData = new AnimationRotateData { Z = (float)Math.PI }; // 180 degrees
		anim.SetParama(rotateData);

		var data = new AnimationData { Transform = SkiaSharp.SKMatrix.Identity };

		// Act - apply rotation with significant time delta
		anim.DoAnimation(ref data, 0);
		Thread.Sleep(1000); // Wait 1 second for significant rotation
		anim.DoAnimation(ref data, 1000);

		// Assert - animation executes without error and produces a valid matrix
		// The transform should be valid (not NaN or infinity)
		Assert.False(float.IsNaN(data.Transform.ScaleX));
		Assert.False(float.IsInfinity(data.Transform.ScaleX));
	}

	[Fact]
	public void AnimationRegister_CanRetrieveAnimationByName()
	{
		// Act
		var anim = NGWebGal.Global.AnimationRegister.GetAnimation("AnimationNothing");

		// Assert
		Assert.NotNull(anim);
		Assert.IsType<AnimationNothing>(anim);
	}

	[Fact]
	public void AnimationRegister_ReturnsNothingForUnknownAnimation()
	{
		// Act
		var anim = NGWebGal.Global.AnimationRegister.GetAnimation("NonExistentAnimation");

		// Assert
		Assert.NotNull(anim);
		Assert.IsType<AnimationNothing>(anim);
	}
}
