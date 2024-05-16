using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Allow specified ray interactors to fallback to eye-gaze when they are off screen or pointing off screen.
    /// This component enables split interaction functionality to allow the user to aim with eye gaze and select with a controller.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/XR Gaze Assistance", 11)]
    [HelpURL(XRHelpURLConstants.k_XRGazeAssistance)]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_GazeAssistance)]
#if BURST_PRESENT
    [BurstCompile]
#endif
    public class XRGazeAssistance : MonoBehaviour, IXRAimAssist
    {
        const float k_MinAttachDistance = 0.5f;
        const float k_MinFallbackDivergence = 0f;
        const float k_MaxFallbackDivergence = 90f;
        const float k_MinAimAssistRequiredAngle = 0f;
        const float k_MaxAimAssistRequiredAngle = 90f;

        /// <summary>
        /// Contains all the references to objects needed to mediate gaze fallback for a particular ray interactor.
        /// </summary>
        [Serializable]
        public sealed class InteractorData
        {
            [SerializeField]
            [RequireInterface(typeof(IXRRayProvider))]
            [Tooltip("The interactor that can fall back to gaze data.")]
            Object m_Interactor;

            /// <summary>
            /// The interactor that can fall back to gaze data.
            /// </summary>
            public Object interactor
            {
                get => m_Interactor;
                set => m_Interactor = value;
            }

            [SerializeField]
            [Tooltip("Changes mediation behavior to account for teleportation controls.")]
            bool m_TeleportRay;

            /// <summary>
            /// Changes mediation behavior to account for teleportation controls.
            /// </summary>
            public bool teleportRay
            {
                get => m_TeleportRay;
                set => m_TeleportRay = value;
            }

            /// <summary>
            /// If this interactor is currently having its ray data modified to the gaze fallback.
            /// </summary>
            public bool fallback { get; private set; }

            bool m_Initialized;

            IXRRayProvider m_RayProvider;
            IXRSelectInteractor m_SelectInteractor;

            bool m_RestoreVisuals;
            XRInteractorLineVisual m_LineVisual;
            bool m_HasLineVisual;

            Transform m_OriginalRayOrigin;
            Transform m_OriginalAttach;
            Transform m_OriginalVisualLineOrigin;
            bool m_OriginalOverrideVisualLineOrigin;
            Transform m_FallbackRayOrigin;
            Transform m_FallbackAttach;
            Transform m_FallbackVisualLineOrigin;

            /// <summary>
            /// Hooks up all possible mediated components attached to the interactor.
            /// </summary>
            internal void Initialize()
            {
                if (m_Initialized)
                    return;

                m_RayProvider = m_Interactor as IXRRayProvider;
                m_SelectInteractor = m_Interactor as IXRSelectInteractor;
                if (m_RayProvider == null || m_SelectInteractor == null)
                {
                    Debug.LogWarning("No ray and select interactor found!");
                    return;
                }

                m_OriginalRayOrigin = m_RayProvider.GetOrCreateRayOrigin();
                m_OriginalAttach = m_RayProvider.GetOrCreateAttachTransform();

                var rayTransform = m_SelectInteractor.transform;
                var rayName = rayTransform.gameObject.name;
                m_FallbackRayOrigin = new GameObject($"Gaze Assistance [{rayName}] Ray Origin").transform;
                m_FallbackAttach = new GameObject($"Gaze Assistance [{rayName}] Attach").transform;
                m_FallbackRayOrigin.parent = m_OriginalRayOrigin.parent;
                m_FallbackAttach.parent = m_FallbackRayOrigin;

                m_HasLineVisual = rayTransform.TryGetComponent(out m_LineVisual);
                if (m_HasLineVisual)
                {
                    m_FallbackVisualLineOrigin = new GameObject($"Gaze Assistance [{rayName}] Visual Origin").transform;
                    m_FallbackVisualLineOrigin.parent = m_FallbackRayOrigin.parent;
                }

                m_Initialized = true;
            }

            /// <summary>
            /// Update the fallback ray pose (copying gaze) if we are using it.
            /// </summary>
            /// <param name="gazeTransform">The Transform representing eye gaze origin.</param>
            internal void UpdateFallbackRayOrigin(Transform gazeTransform)
            {
                if (!m_Initialized)
                    return;

                if (fallback)
                {
                    var gazePosition = gazeTransform.position;
                    var gazeRotation = gazeTransform.rotation;
                    m_FallbackRayOrigin.SetPositionAndRotation(gazePosition, gazeRotation);
                }
            }

            /// <summary>
            /// Update the line visual origin pose if we are using it.
            /// </summary>
            internal void UpdateLineVisualOrigin()
            {
                if (!m_Initialized)
                    return;

                if (m_HasLineVisual && fallback)
                {
                    Vector3 position;
                    Quaternion rotation;
                    // The pose for the line visual is copied from the original.
                    // The rotation uses the gaze direction when it is a teleport projectile since it feels better.
                    if (m_OriginalOverrideVisualLineOrigin && m_OriginalVisualLineOrigin != null)
                    {
                        position = m_OriginalVisualLineOrigin.position;
                        rotation = !m_TeleportRay ? m_OriginalVisualLineOrigin.rotation : m_FallbackRayOrigin.rotation;
                    }
                    else
                    {
                        position = m_OriginalRayOrigin.position;
                        rotation = !m_TeleportRay ? m_OriginalRayOrigin.rotation : m_FallbackRayOrigin.rotation;
                    }

                    m_FallbackVisualLineOrigin.SetPositionAndRotation(position, rotation);
                }
            }

            /// <summary>
            /// Determines if this interactor should be using fallback data or not.
            /// </summary>
            /// <param name="gazeTransform">The Transform representing eye gaze origin.</param>
            /// <param name="fallbackDivergence">At what angle the fallback data should be used.</param>
            /// <param name="selectionLocked">If another interactor is already using the fallback data.</param>
            /// <returns>Returns <see langword="true"/> if the interactor is using the eye gaze for ray origin, <see langword="false"/> if it is using its original data.</returns>
            internal bool UpdateFallbackState(Transform gazeTransform, float fallbackDivergence, bool selectionLocked)
            {
                if (!m_Initialized)
                    return false;

                var shouldFallback = !selectionLocked && (Vector3.Angle(gazeTransform.forward, m_OriginalRayOrigin.forward) > fallbackDivergence);

                // Only allow state transitions when selecting is not occurring
                if (!m_SelectInteractor.isSelectActive)
                {
                    // If the ray is out of view, switch to using the fallback data
                    if (shouldFallback && !fallback)
                    {
                        // Set to the Transforms managed by this component
                        if (m_HasLineVisual)
                        {
                            m_OriginalOverrideVisualLineOrigin = m_LineVisual.overrideInteractorLineOrigin;
                            m_OriginalVisualLineOrigin = m_LineVisual.lineOriginTransform;

                            m_LineVisual.overrideInteractorLineOrigin = true;
                            m_LineVisual.lineOriginTransform = m_FallbackVisualLineOrigin;
                        }

                        m_RayProvider.SetRayOrigin(m_FallbackRayOrigin);
                        m_RayProvider.SetAttachTransform(m_FallbackAttach);
                    }
                    else if (!shouldFallback && fallback)
                    {
                        // Restore the original values from before
                        if (m_HasLineVisual)
                        {
                            m_LineVisual.overrideInteractorLineOrigin = m_OriginalOverrideVisualLineOrigin;
                            m_LineVisual.lineOriginTransform = m_OriginalVisualLineOrigin;
                        }

                        m_RayProvider.SetRayOrigin(m_OriginalRayOrigin);
                        m_RayProvider.SetAttachTransform(m_OriginalAttach);

                        if (!m_TeleportRay)
                            m_RestoreVisuals = true;
                    }

                    fallback = shouldFallback;
                }

                if (fallback)
                {
                    var gazePosition = gazeTransform.position;
                    var gazeRotation = gazeTransform.rotation;

                    if (!m_TeleportRay && m_SelectInteractor.isSelectActive && m_SelectInteractor.hasSelection)
                    {
                        // Lerp the fallback ray to the original ray
                        var anchorDistance = (m_FallbackAttach.position - gazePosition).magnitude;
                        var distancePercent = Mathf.Clamp01(anchorDistance / k_MinAttachDistance);
                        m_FallbackRayOrigin.SetPositionAndRotation(
                            Vector3.Lerp(m_OriginalRayOrigin.position, gazePosition, distancePercent),
                            Quaternion.Lerp(m_OriginalRayOrigin.rotation, gazeRotation, distancePercent));

                        if (m_HasLineVisual)
                            m_LineVisual.enabled = true;

                        return true;
                    }

                    if (m_HasLineVisual && !m_TeleportRay)
                        m_LineVisual.enabled = false;
                }

                return false;
            }

            /// <summary>
            /// Restores the visuals of the <see cref="XRInteractorLineVisual" /> if they were hidden.
            /// </summary>
            internal void RestoreVisuals()
            {
                if (m_RestoreVisuals && m_HasLineVisual && !fallback)
                    m_LineVisual.enabled = true;

                m_RestoreVisuals = false;
            }
        }

        [SerializeField]
        [Tooltip("Eye data source used as fallback data and to determine if fallback data should be used.")]
        XRGazeInteractor m_GazeInteractor;

        /// <summary>
        /// Eye data source used as fallback data and to determine if fallback data should be used.
        /// </summary>
        public XRGazeInteractor gazeInteractor
        {
            get => m_GazeInteractor;
            set => m_GazeInteractor = value;
        }

        [SerializeField]
        [Range(k_MinFallbackDivergence, k_MaxFallbackDivergence)]
        [Tooltip("How far an interactor must point away from the user's view area before eye gaze will be used instead.")]
        float m_FallbackDivergence = 60f;

        /// <summary>
        /// How far an interactor must point away from the user's view area before eye gaze will be used instead.
        /// </summary>
        public float fallbackDivergence
        {
            get => m_FallbackDivergence;
            set => m_FallbackDivergence = Mathf.Clamp(value, k_MinFallbackDivergence, k_MaxFallbackDivergence);
        }

        [SerializeField]
        [Tooltip("If the eye reticle should be hidden when all interactors are using their original data.")]
        bool m_HideCursorWithNoActiveRays = true;

        /// <summary>
        /// If the eye reticle should be hidden when all interactors are using their original data.
        /// </summary>
        public bool hideCursorWithNoActiveRays
        {
            get => m_HideCursorWithNoActiveRays;
            set => m_HideCursorWithNoActiveRays = value;
        }

        [SerializeField]
        [Tooltip("Interactors that can fall back to gaze data.")]
        List<InteractorData> m_RayInteractors = new List<InteractorData>();

        /// <summary>
        /// Interactors that can fall back to gaze data.
        /// </summary>
        public List<InteractorData> rayInteractors
        {
            get => m_RayInteractors;
            set => m_RayInteractors = value;
        }

        [SerializeField]
        [Tooltip("How far projectiles can aim outside of eye gaze and still be considered for aim assist.")]
        [Range(k_MinAimAssistRequiredAngle, k_MaxAimAssistRequiredAngle)]
        float m_AimAssistRequiredAngle = 30f;

        /// <summary>
        /// How far projectiles can aim outside of eye gaze and still be considered for aim assist.
        /// </summary>
        public float aimAssistRequiredAngle
        {
            get => m_AimAssistRequiredAngle;
            set => m_AimAssistRequiredAngle = Mathf.Clamp(value, k_MinAimAssistRequiredAngle, k_MaxAimAssistRequiredAngle);
        }

        [SerializeField]
        [Tooltip("How fast a projectile must be moving to be considered for aim assist.")]
        float m_AimAssistRequiredSpeed = 0.25f;

        /// <summary>
        /// How fast a projectile must be moving to be considered for aim assist.
        /// </summary>
        public float aimAssistRequiredSpeed
        {
            get => m_AimAssistRequiredSpeed;
            set => m_AimAssistRequiredSpeed = value;
        }

        [SerializeField]
        [Tooltip("How much of the corrected aim velocity to use, as a percentage.")]
        [Range(0f, 1f)]
        float m_AimAssistPercent = 0.8f;

        /// <summary>
        /// How much of the corrected aim velocity to use, as a percentage.
        /// </summary>
        public float aimAssistPercent
        {
            get => m_AimAssistPercent;
            set => m_AimAssistPercent = Mathf.Clamp01(value);
        }

        [SerializeField]
        [Tooltip("How much additional speed a projectile can receive from aim assistance, as a percentage.")]
        float m_AimAssistMaxSpeedPercent = 10f;

        /// <summary>
        /// How much additional speed a projectile can receive from aim assistance, as a percentage.
        /// </summary>
        public float aimAssistMaxSpeedPercent
        {
            get => m_AimAssistMaxSpeedPercent;
            set => m_AimAssistMaxSpeedPercent = value;
        }

        InteractorData m_SelectingInteractorData;
        XRInteractorReticleVisual m_GazeReticleVisual;
        bool m_HasGazeReticleVisual;

        void Initialize()
        {
            if (m_GazeInteractor != null)
            {
                m_HasGazeReticleVisual = m_GazeInteractor.TryGetComponent(out m_GazeReticleVisual);
            }
            else
            {
                Debug.LogError($"Gaze Interactor not set or missing on {this}. Disabling this XR Gaze Assistance component.", this);
                enabled = false;
                return;
            }

            for (var index = 0; index < m_RayInteractors.Count; ++index)
            {
                var interactorData = m_RayInteractors[index];
                interactorData.Initialize();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Start()
        {
            Initialize();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            var gazeTransform = m_GazeInteractor.rayOriginTransform;

            for (var index = 0; index < m_RayInteractors.Count; ++index)
            {
                var interactorData = m_RayInteractors[index];

                interactorData.RestoreVisuals();
                interactorData.UpdateFallbackRayOrigin(gazeTransform);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void LateUpdate()
        {
            if (!m_GazeInteractor.isActiveAndEnabled)
                return;

            var gazeTransform = m_GazeInteractor.rayOriginTransform;

            if (m_SelectingInteractorData != null)
            {
                if (!m_SelectingInteractorData.UpdateFallbackState(gazeTransform, m_FallbackDivergence, false))
                    m_SelectingInteractorData = null;
            }

            // Go through each interactor
            // If one is selecting, it takes priority and all others just revert
            var anyFallback = false;
            for (var index = 0; index < m_RayInteractors.Count; ++index)
            {
                var interactorData = m_RayInteractors[index];

                if (interactorData.fallback)
                    anyFallback = true;

                if (interactorData == m_SelectingInteractorData)
                    continue;

                if (interactorData.UpdateFallbackState(gazeTransform, m_FallbackDivergence, m_SelectingInteractorData != null))
                    m_SelectingInteractorData = interactorData;
            }

            if (m_HideCursorWithNoActiveRays && m_HasGazeReticleVisual)
            {
                var selecting = m_SelectingInteractorData != null;
                m_GazeReticleVisual.enabled = anyFallback && !selecting;
            }
        }

        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderGazeAssistance)]
        void OnBeforeRender()
        {
            for (var index = 0; index < m_RayInteractors.Count; ++index)
            {
                var interactorData = m_RayInteractors[index];

                interactorData.UpdateLineVisualOrigin();
            }
        }

        /// <inheritdoc />
        public Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity)
        {
            GetAssistedVelocityInternal(source, m_GazeInteractor.rayEndPoint, velocity, gravity,
                m_AimAssistRequiredAngle, m_AimAssistRequiredSpeed, m_AimAssistMaxSpeedPercent, m_AimAssistPercent, Mathf.Epsilon, out var adjustedVelocity);
            return adjustedVelocity;
        }

        /// <inheritdoc />
        public Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity, float maxAngle)
        {
            GetAssistedVelocityInternal(source, m_GazeInteractor.rayEndPoint, velocity, gravity,
                maxAngle, m_AimAssistRequiredSpeed, m_AimAssistMaxSpeedPercent, m_AimAssistPercent, Mathf.Epsilon, out var adjustedVelocity);
            return adjustedVelocity;
        }

#if BURST_PRESENT
    [BurstCompile]
#endif
        static void GetAssistedVelocityInternal(in Vector3 source, in Vector3 target, in Vector3 velocity, float gravity,
            float maxAngle, float requiredSpeed, float maxSpeedPercent, float assistPercent, float epsilon, out Vector3 adjustedVelocity)
        {
            var toTarget = (target - source);
            var speed = math.length(velocity);

            var originalDirection = math.normalize(velocity);
            var targetDirection = math.normalize(toTarget);
            
            // If too far out, no aim assistance occurs
            if (Vector3.Angle(originalDirection, targetDirection) > maxAngle)
            {
                adjustedVelocity = velocity;
                return;
            }

            // If there is no gravity, then just go straight to the eye point
            if (gravity < epsilon)
            {
                adjustedVelocity = targetDirection * speed;
                return;
            }

            // If the speed is too low, we don't change anything
            if (speed < requiredSpeed)
            {
                adjustedVelocity = velocity;
                return;
            }

            // We solve the trajectory in 2D and then apply to the XZ angle
            float3 xzFacing = toTarget;
            xzFacing.y = 0f;
            var xzDistance = math.length(xzFacing);

            if (xzDistance < epsilon)
            {
                adjustedVelocity = velocity;
                return;
            }

            // To find the best angle, we solve for 45 degrees (a perfect parabolic arc) and 0 degrees or as low of an arc as we can
            var parabolicSolve = new float2(math.sqrt((0.5f * gravity * (xzDistance * xzDistance)) / (xzDistance - toTarget.y)), 0f);

            parabolicSolve.y = parabolicSolve.x;

            // Solve for a low of a degrees as possible
            var lowSolve = new float2(parabolicSolve.x, 0f);

            // If the target point is not lower than the starting point, we can't do the 0 degree solve
            if (toTarget.y < 0f)
            {
                lowSolve.x = math.sqrt((0.5f * gravity * xzDistance * xzDistance / -toTarget.y));
            }
            else
            {
                // Instead, we just double the horizontal speed of the parabolic solve to lower the height
                lowSolve.x *= 2f;
                lowSolve.y = lowSolve.x * (toTarget.y + (0.5f * gravity * (xzDistance / lowSolve.x) * (xzDistance / lowSolve.x))) / xzDistance;
            }

            // See which one is closer to our target speed
            var parabolicSpeed = math.length(parabolicSolve);
            var lowSpeed = math.length(lowSolve);

            var parabolicDif = math.abs(parabolicSpeed - speed);
            var lowDif = math.abs(lowSpeed - speed);

            // If the original user-supplied velocity was heading down, we give the low angle priority as parabolic would look weird
            if (velocity.y <= 0f)
                lowDif *= 0.25f;

            var chosenSolve = parabolicDif < lowDif ? parabolicSolve : lowSolve;

            // Cap to the assisted speed
            chosenSolve = math.normalize(chosenSolve) * math.min(math.length(chosenSolve), maxSpeedPercent * speed);

            float3 assistVelocity = math.normalize(xzFacing) * chosenSolve.x;
            assistVelocity.y = chosenSolve.y;

            // Lerp direction and speed for the final velocity
            adjustedVelocity = Vector3.Slerp(originalDirection, math.normalize(assistVelocity), assistPercent) * math.lerp(speed, math.length(assistVelocity), assistPercent);
        }
    }
}