using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Use this class to maintain a list of Interactors that are potentially influenced by teleportation
    /// and subscribe to the event when teleportation occurs. Uses the events invoked by
    /// <see cref="TeleportationProvider"/> to detect teleportation.
    /// </summary>
    /// <remarks>
    /// Used by the XR Grab Interactable to cancel out the effect of the teleportation from its tracked velocity
    /// so it does not release at unintentionally high energy.
    /// </remarks>
    /// <seealso cref="XRGrabInteractable"/>
    class TeleportationMonitor
    {
        /// <summary>
        /// Calls the methods in its invocation list when one of the Interactors monitored has been influenced by teleportation.
        /// The <see cref="Pose"/> event args represents the amount the <see cref="XROrigin"/> rig was translated and rotated.
        /// </summary>
        public event Action<Pose> teleported;

        /// <summary>
        /// The list of interactors monitored that are influenced by teleportation.
        /// Consists of those that are a child GameObject of the <see cref="XROrigin"/> rig.
        /// </summary>
        /// <remarks>
        /// There will typically only ever be one <see cref="TeleportationProvider"/> in the scene.
        /// </remarks>
        Dictionary<TeleportationProvider, List<IXRInteractor>> m_TeleportInteractors;

        /// <summary>
        /// The <see cref="Pose"/> of the <see cref="XROrigin"/> rig before teleportation.
        /// Used to calculate the teleportation delta using this as reference.
        /// </summary>
        Dictionary<LocomotionSystem, Pose> m_OriginPosesBeforeTeleport;

        static readonly LinkedPool<Dictionary<TeleportationProvider, List<IXRInteractor>>> s_TeleportInteractorsPool =
            new LinkedPool<Dictionary<TeleportationProvider, List<IXRInteractor>>>(() => new Dictionary<TeleportationProvider, List<IXRInteractor>>());

        static readonly LinkedPool<Dictionary<LocomotionSystem, Pose>> s_OriginPosesBeforeTeleportPool =
            new LinkedPool<Dictionary<LocomotionSystem, Pose>>(() => new Dictionary<LocomotionSystem, Pose>());

        /// <summary>
        /// Cached reference to <see cref="TeleportationProvider"/> instances found.
        /// </summary>
        static TeleportationProvider[] s_TeleportationProvidersCache;

        /// <summary>
        /// Adds <paramref name="interactor"/> to monitor. If it is a child of the XR Origin, <see cref="teleported"/>
        /// will be invoked when the player teleports.
        /// </summary>
        /// <param name="interactor">The Interactor to add.</param>
        /// <seealso cref="RemoveInteractor"/>
        public void AddInteractor(IXRInteractor interactor)
        {
            if (interactor == null)
                throw new ArgumentNullException(nameof(interactor));

            var interactorTransform = interactor.transform;
            if (interactorTransform == null)
                return;

            if (!FindTeleportationProviders())
                return;

            foreach (var teleportationProvider in s_TeleportationProvidersCache)
            {
                if (!TryGetOriginTransform(teleportationProvider, out var originTransform))
                    continue;

                if (!interactorTransform.IsChildOf(originTransform))
                    continue;

                if (m_TeleportInteractors == null)
                    m_TeleportInteractors = s_TeleportInteractorsPool.Get();

                if (!m_TeleportInteractors.TryGetValue(teleportationProvider, out var interactors))
                {
                    interactors = new List<IXRInteractor>();
                    m_TeleportInteractors.Add(teleportationProvider, interactors);
                }

                Debug.Assert(!interactors.Contains(interactor));
                interactors.Add(interactor);

                if (interactors.Count == 1)
                {
                    teleportationProvider.beginLocomotion += OnBeginTeleportation;
                    teleportationProvider.endLocomotion += OnEndTeleportation;
                }
            }
        }

        /// <summary>
        /// Removes <paramref name="interactor"/> from monitor.
        /// </summary>
        /// <param name="interactor">The Interactor to remove.</param>
        /// <seealso cref="AddInteractor"/>
        public void RemoveInteractor(IXRInteractor interactor)
        {
            if (interactor == null)
                throw new ArgumentNullException(nameof(interactor));

            var totalInteractors = 0;
            if (m_TeleportInteractors != null)
            {
                foreach (var kvp in m_TeleportInteractors)
                {
                    var teleportationProvider = kvp.Key;
                    var interactors = kvp.Value;

                    if (interactors.Remove(interactor) && interactors.Count == 0 && teleportationProvider != null)
                    {
                        teleportationProvider.beginLocomotion -= OnBeginTeleportation;
                        teleportationProvider.endLocomotion -= OnEndTeleportation;
                    }

                    totalInteractors += interactors.Count;
                }
            }

            // Release back to the pool
            if (totalInteractors == 0)
            {
                if (m_TeleportInteractors != null)
                {
                    s_TeleportInteractorsPool.Release(m_TeleportInteractors);
                    m_TeleportInteractors = null;
                }

                if (m_OriginPosesBeforeTeleport != null)
                {
                    s_OriginPosesBeforeTeleportPool.Release(m_OriginPosesBeforeTeleport);
                    m_OriginPosesBeforeTeleport = null;
                }
            }
        }

        static bool TryGetOriginTransform(LocomotionProvider locomotionProvider, out Transform originTransform)
        {
            // Correct version of locomotionProvider?.system?.xrOrigin?.Origin?.transform
            if (locomotionProvider != null)
            {
                var system = locomotionProvider.system;
                return TryGetOriginTransform(system, out originTransform);
            }

            originTransform = null;
            return false;
        }

        static bool TryGetOriginTransform(LocomotionSystem system, out Transform originTransform)
        {
            // Correct version of system?.xrOrigin?.Origin?.transform
            if (system != null)
            {
                var xrOrigin = system.xrOrigin;
                if (xrOrigin != null)
                {
                    var origin = xrOrigin.Origin;
                    if (origin != null)
                    {
                        originTransform = origin.transform;
                        return true;
                    }
                }
            }

            originTransform = null;
            return false;
        }

        static bool FindTeleportationProviders()
        {
            if (s_TeleportationProvidersCache == null)
#if UNITY_2023_1_OR_NEWER
                s_TeleportationProvidersCache = Object.FindObjectsByType<TeleportationProvider>(FindObjectsSortMode.None);
#else
                s_TeleportationProvidersCache = Object.FindObjectsOfType<TeleportationProvider>();
#endif

            return s_TeleportationProvidersCache.Length > 0;
        }

        void OnBeginTeleportation(LocomotionSystem locomotionSystem)
        {
            if (!TryGetOriginTransform(locomotionSystem, out var originTransform))
                return;

            if (m_OriginPosesBeforeTeleport == null)
                m_OriginPosesBeforeTeleport = s_OriginPosesBeforeTeleportPool.Get();

            m_OriginPosesBeforeTeleport[locomotionSystem] = originTransform.GetWorldPose();
        }

        void OnEndTeleportation(LocomotionSystem locomotionSystem)
        {
            if (!TryGetOriginTransform(locomotionSystem, out var originTransform))
                return;

            if (m_OriginPosesBeforeTeleport == null)
                return;

            if (!m_OriginPosesBeforeTeleport.TryGetValue(locomotionSystem, out var originPoseBeforeTeleport))
                return;

            var translated = originTransform.position - originPoseBeforeTeleport.position;
            var rotated = originTransform.rotation * Quaternion.Inverse(originPoseBeforeTeleport.rotation);

            teleported?.Invoke(new Pose(translated, rotated));
        }
    }
}