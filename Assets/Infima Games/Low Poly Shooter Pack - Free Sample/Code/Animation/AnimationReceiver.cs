// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
	/// <summary>
	/// This class is helpful when adding weapons alone in the scene that are playing animations.
	/// As, without it, the animation events would not have a receiver, and thus create errors!
	/// </summary>
	public class AnimationReceiver : MonoBehaviour
	{
		#region ANIMATION

		private void OnAmmunitionFill(int amount = 0)
		{
		}

		private void OnGrenade()
		{
		}
		private void OnSetActiveMagazine(int active)
		{
		}
		
		private void OnAnimationEndedBolt()
		{
		}
		private void OnAnimationEndedReload()
		{
		}

		private void OnAnimationEndedGrenadeThrow()
		{
		}
		private void OnAnimationEndedMelee()
		{
		}

		private void OnAnimationEndedInspect()
		{
		}
		private void OnAnimationEndedHolster()
		{
		}
		
		private void OnEjectCasing()
		{
		}

		private void OnSlideBack()
		{
		}

		private void OnSetActiveKnife()
		{
		}

		#endregion
	}   
}