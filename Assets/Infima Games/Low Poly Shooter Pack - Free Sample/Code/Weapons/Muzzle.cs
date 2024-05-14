// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;
using System.Collections;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Muzzle.
    /// </summary>
    public class Muzzle : MuzzleBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Settings")]
        
        [Tooltip("Socket at the tip of the Muzzle. Commonly used as a firing point.")]
        [SerializeField]
        private Transform socket;

        [Tooltip("Sprite. Displayed on the player's interface.")]
        [SerializeField]
        private Sprite sprite;

        [Tooltip("Audio clip played when firing through this muzzle.")]
        [SerializeField]
        private AudioClip audioClipFire;
        
        [Header("Particles")]
        
        [Tooltip("Firing Particles.")]
        [SerializeField]
        private GameObject prefabFlashParticles;

        [Tooltip("Number of particles to emit when firing.")]
        [SerializeField]
        private int flashParticlesCount = 5;

        [Header("Flash Light")]

        [Tooltip("Muzzle Flash Prefab. A small light we use when firing.")]
        [SerializeField]
        private GameObject prefabFlashLight;

        [Tooltip("Time that the light flashed stays active. After this time, it is disabled.")]
        [SerializeField]
        private float flashLightDuration;

        [Tooltip("Local offset applied to the light.")]
        [SerializeField]
        private Vector3 flashLightOffset;

        #endregion
        
        #region FIELDS

        /// <summary>
        /// Instantiated Particle System.
        /// </summary>
        private ParticleSystem particles;
        /// <summary>
        /// Instantiated light.
        /// </summary>
        private Light flashLight;

        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            //Null Check.
            if(prefabFlashParticles != null)
            {
                //Instantiate Particles.
                GameObject spawnedParticlesPrefab = Instantiate(prefabFlashParticles, socket);
                //Reset the position.
                spawnedParticlesPrefab.transform.localPosition = default;
                //Reset the rotation.
                spawnedParticlesPrefab.transform.localEulerAngles = default;
                
                //Get Reference.
                particles = spawnedParticlesPrefab.GetComponent<ParticleSystem>();
            }

            //Null Check.
            if (prefabFlashLight)
            {
                //Instantiate.
                GameObject spawnedFlashLightPrefab = Instantiate(prefabFlashLight, socket);
                //Reset the position.
                spawnedFlashLightPrefab.transform.localPosition = flashLightOffset;
                //Reset the rotation.
                spawnedFlashLightPrefab.transform.localEulerAngles = default;
                
                //Get reference.
                flashLight = spawnedFlashLightPrefab.GetComponent<Light>();
                //Disable.
                flashLight.enabled = false;
            }
        }

        #endregion

        #region GETTERS

        public override void Effect()
        {
            //Try to play the fire particles from the muzzle!
            if(particles != null)
                particles.Emit(flashParticlesCount);

            //Make sure that we have a light to flash!
            if (flashLight != null)
            {
                //Enable the light.
                flashLight.enabled = true;
                //Disable the light after a few seconds.
                StartCoroutine(nameof(DisableLight));
            }
        }

        public override Transform GetSocket() => socket;

        public override Sprite GetSprite() => sprite;
        public override AudioClip GetAudioClipFire() => audioClipFire;
        
        public override ParticleSystem GetParticlesFire() => particles;
        public override int GetParticlesFireCount() => flashParticlesCount;
        
        public override Light GetFlashLight() => flashLight;
        public override float GetFlashLightDuration() => flashLightDuration;

        #endregion

        #region METHODS

        private IEnumerator DisableLight()
        {
            //Wait.
            yield return new WaitForSeconds(flashLightDuration);
            //Disable.
            flashLight.enabled = false;
        }

        #endregion
    }
}