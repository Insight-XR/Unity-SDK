// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Muzzle Abstract Class.
    /// </summary>
    public abstract class MuzzleBehaviour : MonoBehaviour
    {
        #region GETTERS

        /// <summary>
        /// Returns the firing socket. This is the point that we use to fire the bullets.
        /// </summary>
        public abstract Transform GetSocket();

        /// <summary>
        /// Returns the Sprite used on the Character's Interface.
        /// </summary>
        public abstract Sprite GetSprite();
        /// <summary>
        /// Returns the AudioClip to play when firing.
        /// </summary>
        public abstract AudioClip GetAudioClipFire();
        
        /// <summary>
        /// Returns the particle system to use when firing.
        /// </summary>
        public abstract ParticleSystem GetParticlesFire();
        /// <summary>
        /// Returns the number of particles to emit when firing.
        /// </summary>
        public abstract int GetParticlesFireCount();

        /// <summary>
        /// Returns the light component used when firing..
        /// </summary>
        public abstract Light GetFlashLight();
        /// <summary>
        /// Returns the time it takes for the light flash to be hidden.
        /// </summary>
        public abstract float GetFlashLightDuration();

        #endregion

        #region METHODS

        /// <summary>
        /// Plays all of the muzzle effects.
        /// </summary>
        public abstract void Effect(); 

        #endregion
    }
}