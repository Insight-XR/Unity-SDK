using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor used for interacting with interactables via gaze. This extends <see cref="XRRayInteractor"/> and
    /// uses the same ray cast technique to update a current set of valid targets.
    /// </summary>
    /// <seealso cref="XRBaseInteractable.allowGazeInteraction"/>
    /// <seealso cref="XRBaseInteractable.allowGazeSelect"/>
    /// <seealso cref="XRBaseInteractable.allowGazeAssistance"/>
    [AddComponentMenu("XR/XR Gaze Interactor", 11)]
    [HelpURL(XRHelpURLConstants.k_XRGazeInteractor)]
    public class XRGazeInteractor : XRRayInteractor
    {
        /// <summary>
        /// Defines the way the gaze assistance calculates and sizes the assistance area.
        /// </summary>
        /// <seealso cref="gazeAssistanceCalculation"/>
        public enum GazeAssistanceCalculation
        {
            ///<summary>
            /// Gaze assistance area will be a fixed size set in <see cref="gazeAssistanceColliderFixedSize"/>
            /// and scaled by <see cref="gazeAssistanceColliderScale"/>.
            /// </summary>
            FixedSize,

            ///<summary>
            /// Gaze assistance area will be sized based on the <see cref="Collider.bounds"/> of the <see cref="IXRInteractable"/>
            /// this <see cref="XRGazeInteractor"/> is hovering over and scaled by <see cref="gazeAssistanceColliderScale"/>.
            /// </summary>
            ColliderSize,
        }

        [SerializeField]
        GazeAssistanceCalculation m_GazeAssistanceCalculation;

        /// <summary>
        /// Defines the way the gaze assistance calculates and sizes the assistance area.
        /// </summary>
        /// <seealso cref="GazeAssistanceCalculation"/>
        public GazeAssistanceCalculation gazeAssistanceCalculation
        {
            get => m_GazeAssistanceCalculation;
            set => m_GazeAssistanceCalculation = value;
        }

        [SerializeField]
        float m_GazeAssistanceColliderFixedSize = 1f;

        /// <summary>
        /// The size of the <see cref="gazeAssistanceSnapVolume"/> collider when <see cref="gazeAssistanceCalculation"/> is <see cref="GazeAssistanceCalculation.FixedSize"/>.
        /// </summary>
        /// <seealso cref="GazeAssistanceCalculation"/>
        public float gazeAssistanceColliderFixedSize
        {
            get => m_GazeAssistanceColliderFixedSize;
            set => m_GazeAssistanceColliderFixedSize = value;
        }

        [SerializeField]
        float m_GazeAssistanceColliderScale = 1f;

        /// <summary>
        /// The scale of the <see cref="gazeAssistanceSnapVolume"/> when <see cref="gazeAssistanceCalculation"/> is <see cref="GazeAssistanceCalculation.FixedSize"/> or <see cref="GazeAssistanceCalculation.ColliderSize"/> .
        /// </summary>
        /// <seealso cref="GazeAssistanceCalculation"/>
        public float gazeAssistanceColliderScale
        {
            get => m_GazeAssistanceColliderScale;
            set => m_GazeAssistanceColliderScale = value;
        }

        [SerializeField]
        XRInteractableSnapVolume m_GazeAssistanceSnapVolume;

        /// <summary>
        /// The <see cref="XRInteractableSnapVolume"/> to place where this <see cref="XRGazeInteractor"/> hits a
        /// valid target for gaze assistance. If not set, Unity will create one by default.
        /// </summary>
        /// <remarks>
        /// Only <see cref="SphereCollider"/> and <see cref="BoxCollider"/> are supported
        /// for automatic dynamic scaling of the <see cref="XRInteractableSnapVolume.snapCollider"/>.
        /// </remarks>
        public XRInteractableSnapVolume gazeAssistanceSnapVolume
        {
            get => m_GazeAssistanceSnapVolume;
            set => m_GazeAssistanceSnapVolume = value;
        }

        [SerializeField]
        bool m_GazeAssistanceDistanceScaling;

        /// <summary>
        /// If true, the <see cref="gazeAssistanceSnapVolume"/> will also scale based on the distance from the <see cref="XRGazeInteractor"/>.
        /// </summary>
        /// <seealso cref="clampGazeAssistanceDistanceScaling"/>
        public bool gazeAssistanceDistanceScaling
        {
            get => m_GazeAssistanceDistanceScaling;
            set => m_GazeAssistanceDistanceScaling = value;
        }

        [SerializeField]
        bool m_ClampGazeAssistanceDistanceScaling;

        /// <summary>
        /// If true, the <see cref="gazeAssistanceSnapVolume"/> scale will be clamped at <see cref="gazeAssistanceDistanceScalingClampValue"/>.
        /// </summary>
        /// <seealso cref="GazeAssistanceCalculation"/>
        public bool clampGazeAssistanceDistanceScaling
        {
            get => m_ClampGazeAssistanceDistanceScaling;
            set => m_ClampGazeAssistanceDistanceScaling = value;
        }

        [SerializeField]
        float m_GazeAssistanceDistanceScalingClampValue = 1f;

        /// <summary>
        /// The value the assistance collider scale will be clamped to if <see cref="clampGazeAssistanceDistanceScaling"/> is true.
        /// </summary>
        /// <seealso cref="GazeAssistanceCalculation"/>
        public float gazeAssistanceDistanceScalingClampValue
        {
            get => m_GazeAssistanceDistanceScalingClampValue;
            set => m_GazeAssistanceDistanceScalingClampValue = value;
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            CreateGazeAssistanceSnapVolume();
        }

        void CreateGazeAssistanceSnapVolume()
        {
            // If we don't have a snap volume for gaze assistance, generate one.
            if (m_GazeAssistanceSnapVolume == null)
            {
                var snapVolumeGO = new GameObject("Gaze Snap Volume");
                var snapCollider = snapVolumeGO.AddComponent<SphereCollider>();
                snapCollider.isTrigger = true;
                m_GazeAssistanceSnapVolume = snapVolumeGO.AddComponent<XRInteractableSnapVolume>();
            }
            else if (m_GazeAssistanceSnapVolume.snapCollider != null)
            {
                if (!(m_GazeAssistanceSnapVolume.snapCollider is SphereCollider || m_GazeAssistanceSnapVolume.snapCollider is BoxCollider))
                    Debug.LogWarning("The Gaze Assistance Snap Volume is using a Snap Collider which does not support" +
                        " automatic dynamic scaling by the XR Gaze Interactor. It must be a Sphere Collider or Box Collider.", this);
            }
        }

        /// <inheritdoc />
        public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.PreprocessInteractor(updatePhase);
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Get the nearest valid interactable target that this can interact with,
                // and use it to update the gaze assistance snap volume.
                var gazeInteractable = CanInteract(currentNearestValidTarget) ? currentNearestValidTarget : null;
                UpdateSnapVolumeInteractable(gazeInteractable);
            }
        }

        /// <summary>
        /// Updates the <see cref="gazeAssistanceSnapVolume"/> based on a target interactable.
        /// </summary>
        /// <param name="interactable">The <see cref="IXRInteractable"/> this <see cref="XRGazeInteractor"/> is processing and using to update the <see cref="gazeAssistanceSnapVolume"/>.</param>
        protected virtual void UpdateSnapVolumeInteractable(IXRInteractable interactable)
        {
            if (m_GazeAssistanceSnapVolume == null)
                return;

            var snapVolumePosition = Vector3.zero;
            var snapVolumeScale = m_GazeAssistanceColliderScale;
            var snapColliderSize = 0f;
            IXRInteractable snapInteractable = null;
            Collider snapToCollider = null;

            // Currently assumes no gaze assistance for interactables that are not our abstract base class
            if (interactable is XRBaseInteractable baseInteractable && baseInteractable != null && baseInteractable.allowGazeAssistance)
            {
                snapInteractable = interactable;

                // Default to interactable, tries to grab collider position below
                snapVolumePosition = interactable.transform.position;

                if (TryGetHitInfo(out var pos, out _, out _, out _) &&
                    XRInteractableUtility.TryGetClosestCollider(interactable, pos, out var distanceInfo))
                {
                    snapToCollider = distanceInfo.collider;
                    snapVolumePosition = distanceInfo.collider.bounds.center;
                }

                snapColliderSize = CalculateSnapColliderSize(snapToCollider);
            }

            // Update position, size, and scale of the snap volume
            if (m_GazeAssistanceDistanceScaling)
            {
                snapVolumeScale *= Vector3.Distance(transform.position, snapVolumePosition);
                if (m_ClampGazeAssistanceDistanceScaling)
                    snapVolumeScale = Mathf.Clamp(snapVolumeScale, 0f, m_GazeAssistanceDistanceScalingClampValue);
            }

            var snapVolumeTransform = m_GazeAssistanceSnapVolume.transform;
            snapVolumeTransform.position = snapVolumePosition;
            snapVolumeTransform.localScale = new Vector3(snapVolumeScale, snapVolumeScale, snapVolumeScale);

            switch (m_GazeAssistanceSnapVolume.snapCollider)
            {
                case SphereCollider sphereCollider:
                    sphereCollider.radius = snapColliderSize;
                    break;
                case BoxCollider boxCollider:
                    boxCollider.size = new Vector3(snapColliderSize, snapColliderSize, snapColliderSize);
                    break;
            }

            // Update references
            m_GazeAssistanceSnapVolume.interactable = snapInteractable;
            m_GazeAssistanceSnapVolume.snapToCollider = snapToCollider;
        }

        float CalculateSnapColliderSize(Collider interactableCollider)
        {
            switch (m_GazeAssistanceCalculation)
            {
                case GazeAssistanceCalculation.FixedSize:
                    return m_GazeAssistanceColliderFixedSize;
                case GazeAssistanceCalculation.ColliderSize:
                    if (interactableCollider != null)
                        return interactableCollider.bounds.size.MaxComponent();
                    break;
                default:
                    Debug.Assert(false, $"Unhandled {nameof(GazeAssistanceCalculation)}={m_GazeAssistanceCalculation}", this);
                    break;
            }

            return 0f;
        }

        /// <summary>
        /// Checks to see if this <see cref="XRGazeInteractor"/> can interact with an <see cref="IXRInteractable"/>.
        /// </summary>
        /// <param name="interactable">The <see cref="IXRInteractable"/> to check if this <see cref="XRGazeInteractor"/> can interact with.</param>
        /// <returns>Returns <see langword="true"/> if this <see cref="XRGazeInteractor"/> can interact with <see cref="interactable"/>, otherwise returns <see langword="false"/>.</returns>
        bool CanInteract(IXRInteractable interactable)
        {
            return interactable is IXRHoverInteractable hoverInteractable && interactionManager.CanHover(this, hoverInteractable) ||
                interactable is IXRSelectInteractable selectInteractable && interactionManager.CanSelect(this, selectInteractable);
        }

        /// <inheritdoc />
        protected override float GetHoverTimeToSelect(IXRInteractable interactable)
        {
            if (interactable is IXROverridesGazeAutoSelect { overrideGazeTimeToSelect: true } overrideProvider)
                return overrideProvider.gazeTimeToSelect;

            return base.GetHoverTimeToSelect(interactable);
        }

        /// <inheritdoc />
        protected override float GetTimeToAutoDeselect(IXRInteractable interactable)
        {
            if (interactable is IXROverridesGazeAutoSelect { overrideTimeToAutoDeselectGaze: true } overrideProvider)
                return overrideProvider.timeToAutoDeselectGaze;

            return base.GetTimeToAutoDeselect(interactable);
        }
    }
}