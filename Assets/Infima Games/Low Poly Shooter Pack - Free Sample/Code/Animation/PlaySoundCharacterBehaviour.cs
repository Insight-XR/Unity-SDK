// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Helper StateMachineBehaviour that allows us to more easily play a specific weapon sound.
    /// </summary>
    public class PlaySoundCharacterBehaviour : StateMachineBehaviour
    {
        /// <summary>
        /// Type of weapon sound.
        /// </summary>
        private enum SoundType
        {
            //Holsters.
            Holster, Unholster,
            //Normal Reloads.
            Reload, ReloadEmpty,
            //Firing.
            Fire, FireEmpty,
        }

        #region FIELDS SERIALIZED

        [Header("Setup")]
        
        [Tooltip("Delay at which the audio is played.")]
        [SerializeField]
        private float delay;
        
        [Tooltip("Type of weapon sound to play.")]
        [SerializeField]
        private SoundType soundType;
        
        [Header("Audio Settings")]

        [Tooltip("Audio Settings.")]
        [SerializeField]
        private AudioSettings audioSettings = new AudioSettings(1.0f, 0.0f, true);

        #endregion

        #region FIELDS

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;

        /// <summary>
        /// Player Inventory.
        /// </summary>
        private InventoryBehaviour playerInventory;

        /// <summary>
        /// The service that handles sounds.
        /// </summary>
        private IAudioManagerService audioManagerService;

        #endregion
        
        #region UNITY

        /// <summary>
        /// On State Enter.
        /// </summary>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //We need to get the character component.
            playerCharacter ??= ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();

            //Get Inventory.
            playerInventory ??= playerCharacter.GetInventory();

            //Try to get the equipped weapon's Weapon component.
            if (!(playerInventory.GetEquipped() is { } weaponBehaviour))
                return;
            
            //Try grab a reference to the sound managing service.
            audioManagerService ??= ServiceLocator.Current.Get<IAudioManagerService>();

            #region Select Correct Clip To Play

            //Switch.
            AudioClip clip = soundType switch
            {
                //Holster.
                SoundType.Holster => weaponBehaviour.GetAudioClipHolster(),
                //Unholster.
                SoundType.Unholster => weaponBehaviour.GetAudioClipUnholster(),
                
                //Reload.
                SoundType.Reload => weaponBehaviour.GetAudioClipReload(),
                //Reload Empty.
                SoundType.ReloadEmpty => weaponBehaviour.GetAudioClipReloadEmpty(),
                
                //Fire.
                SoundType.Fire => weaponBehaviour.GetAudioClipFire(),
                //Fire Empty.
                SoundType.FireEmpty => weaponBehaviour.GetAudioClipFireEmpty(),
                
                //Default.
                _ => default
            };

            #endregion

            //Play with some delay. Granted, if the delay is set to zero, this will just straight-up play!
            audioManagerService.PlayOneShotDelayed(clip, audioSettings, delay);
        }
        
        #endregion
    }
}