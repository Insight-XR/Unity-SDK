// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Sound Manager Service Interface.
    /// </summary>
    public interface IAudioManagerService : IGameService
    {
        /// <summary>
        /// Plays a one shot of the AudioClip.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="settings">Audio Settings.</param>
        void PlayOneShot(AudioClip clip, AudioSettings settings = default);

        /// <summary>
        /// Plays a one shot of the AudioClip, but waits for <paramref name="delay"/> before doing so.
        /// </summary>
        /// <param name="clip">Clip to play.</param>
        /// <param name="settings">Audio settings to use for this sound.</param>
        /// <param name="delay">Time to wait before we start playing this AudioClip.</param>
        void PlayOneShotDelayed(AudioClip clip, AudioSettings settings = default, float delay = 1.0f);
    }
}