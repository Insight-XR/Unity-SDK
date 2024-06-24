#if AR_FOUNDATION_PRESENT || PACKAGE_DOCS_GENERATION
using Unity.XR.CoreUtils;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Grab transformer which supports translation, rotation and scaling while
    /// respecting the AR environment. This transformer constrains the interactable
    /// to the <see cref="ARPlane"/> during translation.
    /// </summary>
    /// <remarks>
    /// Assumes that AR raycast hits are enabled in the corresponding <see cref="IARInteractor"/>.
    /// </remarks>
    /// <seealso cref="XRRayInteractor"/>
    [AddComponentMenu("XR/Transformers/AR Transformer", 11)]
    [HelpURL(XRHelpURLConstants.k_ARTransformer)]
    public class ARTransformer : XRBaseGrabTransformer, IXRDropTransformer
    {
        /// <summary>
        /// Threshold to compare lerp amount outside 0 or 1 where the lerp is short-circuited to the normalized target value.
        /// </summary>
        const float k_NearlyEqual = 1e-5f;

        /// <summary>
        /// Absolute minimum scale when elastic scaling below the min scale.
        /// Ensures that the scale factor is never infinity since we need to scale the offset from the
        /// attach transform by the scale factor to keep the attach transform point fixed when scaling.
        /// </summary>
        const float k_MinElasticScale = 0.001f; // 1 mm

        const float k_DiffThreshold = 0.0015f;

        /// <summary>
        /// Represents the alignment of a plane where translation is allowed.
        /// </summary>
        /// <seealso cref="objectPlaneTranslationMode"/>
        /// <seealso cref="PlaneAlignment"/>
        public enum PlaneTranslationMode
        {
            /// <summary>
            /// Allow translation when the plane is horizontal.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Allow translation when the plane is vertical.
            /// </summary>
            Vertical,

            /// <summary>
            /// Allow translation on any plane.
            /// </summary>
            Any,
        }

        [SerializeField]
        [Tooltip("Controls whether Unity constrains the object vertically, horizontally, or free to move in all axes.")]
        PlaneTranslationMode m_ObjectPlaneTranslationMode = PlaneTranslationMode.Any;

        /// <summary>
        /// Controls whether the grab interactable will be constrained vertically, horizontally, or free to move in all axes.
        /// </summary>
        /// <seealso cref="PlaneTranslationMode"/>
        public PlaneTranslationMode objectPlaneTranslationMode
        {
            get => m_ObjectPlaneTranslationMode;
            set => m_ObjectPlaneTranslationMode = value;
        }

        [SerializeField]
        [Tooltip("Controls whether the interactable will use the orientation of the interactor, or not.")]
        bool m_UseInteractorOrientation;

        /// <summary>
        /// Controls whether the interactable will use the orientation of the interactor, or not.
        /// </summary>
        public bool useInteractorOrientation
        {
            get => m_UseInteractorOrientation;
            set => m_UseInteractorOrientation = value;
        }

        [Header("Scaling")]
        [SerializeField]
        [Tooltip("The minimum scale of the object.")]
        float m_MinScale = 0.25f;

        /// <summary>
        /// The minimum scale of the object.
        /// </summary>
        public float minScale
        {
            get => m_MinScale;
            set => m_MinScale = value;
        }

        [SerializeField]
        [Tooltip("The maximum scale of the object.")]
        float m_MaxScale = 2f;

        /// <summary>
        /// The maximum scale of the object.
        /// </summary>
        public float maxScale
        {
            get => m_MaxScale;
            set => m_MaxScale = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity to movement being translated into scale.")]
        float m_ScaleSensitivity = 0.75f;

        /// <summary>
        /// Sensitivity to movement being translated into scale.
        /// </summary>
        public float scaleSensitivity
        {
            get => m_ScaleSensitivity;
            set => m_ScaleSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Amount of over scale allowed after hitting min/max of range.")]
        float m_Elasticity = 0.15f;

        /// <summary>
        /// Amount of over scale allowed after hitting min/max of range.
        /// </summary>
        public float elasticity
        {
            get => m_Elasticity;
            set => m_Elasticity = value;
        }

        [SerializeField]
        [Tooltip("Whether to enable the elastic break limit when scaling the object beyond range.")]
        bool m_EnableElasticBreakLimit;

        /// <summary>
        /// Whether to enable the elastic break limit when scaling the object beyond range.
        /// </summary>
        /// <seealso cref="elasticBreakLimit"/>
        public bool enableElasticBreakLimit
        {
            get => m_EnableElasticBreakLimit;
            set => m_EnableElasticBreakLimit = value;
        }

        [SerializeField]
        [Tooltip("The break limit of the elastic ratio used when scaling the object. Returns to min/max range over time after scaling beyond this limit.")]
        float m_ElasticBreakLimit = 0.5f;

        /// <summary>
        /// The break limit of the elastic ratio used when scaling the object. Returns to min/max range over time after scaling beyond this limit.
        /// </summary>
        /// <seealso cref="enableElasticBreakLimit"/>
        public float elasticBreakLimit
        {
            get => m_ElasticBreakLimit;
            set => m_ElasticBreakLimit = value;
        }

        /// <inheritdoc />
        public bool canProcessOnDrop => true;

        XROrigin m_XROrigin;
        ARPlaneManager m_ARPlaneManager;

        bool m_CalculateAttachOffset;
        Vector3 m_AttachOffset;
        Vector3 m_LocalAttachOffset;

        bool m_CalculatePlacementOffset;
        Vector3 m_PlacementOffset;
        Vector3 m_LocalPlacementOffset;

        Vector3 m_DesiredPosition;
        Quaternion m_DesiredRotation;
        Vector3 m_LastEulerRotation;
        Pose m_OriginalObjectPose;

        float m_CurrentScaleRatio;
        float m_CapturedMinScale;
        float m_CapturedMaxScale;

        bool m_ElasticBreakLimitReached;

        IXRScaleValueProvider m_ScaleValueProvider;
        bool m_HasScaleValueProvider;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            FindXROrigin();
            FindCreateARPlaneManager();
        }

        /// <inheritdoc />
        public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                {
                    var interactor = grabInteractable.GetOldestInteractorSelecting();
                    UpdateTarget(interactor, grabInteractable, ref targetPose);

                    // Only update during Dynamic since it reads input or steps the lerp when the gesture is released,
                    // and either of those should only be done once per frame.
                    if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                        UpdateCurrentScaleRatio(grabInteractable, localScale);

                    UpdateTargetScale(interactor, grabInteractable, ref targetPose, ref localScale);
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override void OnGrab(XRGrabInteractable grabInteractable)
        {
            base.OnGrab(grabInteractable);
            m_OriginalObjectPose = grabInteractable.transform.GetWorldPose();
        }

        /// <inheritdoc />
        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
            var interactor = grabInteractable.interactorsSelecting[0];

            m_ScaleValueProvider = interactor as IXRScaleValueProvider;
            m_HasScaleValueProvider = m_ScaleValueProvider != null;
            
            InitializeCurrentScaleRatio(localScale);
            m_ElasticBreakLimitReached = false;

            var interactorAttachTransform = interactor.GetAttachTransform(grabInteractable);
            m_LastEulerRotation = interactorAttachTransform.localEulerAngles;

            m_CalculateAttachOffset = true;
            m_CalculatePlacementOffset = true;
        }

        /// <inheritdoc />
        public void OnDrop(XRGrabInteractable grabInteractable, DropEventArgs args)
        {
            m_CurrentScaleRatio = Mathf.Clamp01(m_CurrentScaleRatio);
        }

        bool TryGetBestPlacementPose(IXRInteractor interactor, out Pose placementPose)
        {
            placementPose = Pose.identity;

            var arInteractor = interactor as IARInteractor;
            if (arInteractor == null)
                return false;

            var mainCamera = m_XROrigin != null ? m_XROrigin.Camera : Camera.main;
            if (mainCamera == null)
                return false;

            var cameraTransform = mainCamera.transform;

            if (arInteractor.TryGetCurrentARRaycastHit(out var firstHit))
            {
                var plane = m_ARPlaneManager.GetPlane(firstHit.trackableId);
                if (plane == null || IsPlaneTypeAllowed(m_ObjectPlaneTranslationMode, plane.alignment))
                {
                    var firstHitPose = firstHit.pose;

                    // Avoid detecting the back of existing planes.
                    if (Vector3.Dot(cameraTransform.position - firstHitPose.position,
                            firstHitPose.rotation * Vector3.up) < 0f)
                        return false;

                    // Don't allow hovering for vertical or horizontal downward facing planes.
                    if (plane == null ||
                        plane.alignment == PlaneAlignment.Vertical ||
                        plane.alignment == PlaneAlignment.HorizontalDown ||
                        plane.alignment == PlaneAlignment.HorizontalUp)
                    {
                        // The best estimate of the point in the plane where the object will be placed.
                        placementPose = firstHitPose;
                        return true;
                    }
                }
            }

            return false;
        }

        static bool IsPlaneTypeAllowed(PlaneTranslationMode planeTranslationMode, PlaneAlignment planeAlignment)
        {
            if (planeTranslationMode == PlaneTranslationMode.Any)
            {
                return true;
            }

            if (planeTranslationMode == PlaneTranslationMode.Horizontal &&
                (planeAlignment == PlaneAlignment.HorizontalDown ||
                    planeAlignment == PlaneAlignment.HorizontalUp))
            {
                return true;
            }

            if (planeTranslationMode == PlaneTranslationMode.Vertical &&
                planeAlignment == PlaneAlignment.Vertical)
            {
                return true;
            }

            return false;
        }

        bool ComputeNewDesiredPose(IXRInteractor interactor, XRGrabInteractable grabInteractable)
        {
            if (TryGetBestPlacementPose(interactor, out var desiredPlacement))
            {
                m_DesiredPosition =  desiredPlacement.position;

                if (m_CalculatePlacementOffset)
                {
                    CalculatePlacementOffset(grabInteractable);
                    m_CalculatePlacementOffset = false;
                }

                // Rotate if the plane direction has changed (for example, dragging from a horizontal surface to a vertical surface).
                var interactableAttachTransform = grabInteractable.GetAttachTransform(interactor);
                if (((desiredPlacement.rotation * Vector3.up) - interactableAttachTransform.up).magnitude > k_DiffThreshold)
                {
                    m_DesiredRotation = desiredPlacement.rotation;

                    // Update offset since it's in world coordinates.
                    if (grabInteractable.trackRotation)
                    {
                        m_AttachOffset = m_DesiredRotation * m_LocalAttachOffset;
                        m_PlacementOffset = m_DesiredRotation * m_LocalPlacementOffset;
                    }
                }
                else
                {
                    m_DesiredRotation = grabInteractable.transform.rotation;
                }

                return true;
            }

            return false;
        }

        void UpdateTarget(IXRInteractor interactor, XRGrabInteractable grabInteractable, ref Pose targetPose)
        {
            if (!grabInteractable.isSelected)
                return;

            if (m_CalculateAttachOffset)
            {
                CalculateAttachOffset(interactor, grabInteractable);
                m_CalculateAttachOffset = false;
            }

            var hasPlacement = ComputeNewDesiredPose(interactor, grabInteractable);
            if (hasPlacement)
            {
                // When Match Position is enabled, use that to indicate that the object should not be moved
                // initially upon first selection. There will typically be an offset from the hit point on the object
                // (the Ray Interactor's attach transform, which would be copied when Match Position is enabled)
                // and the desired placement position on the ARPlane, so the attach transform of the interactable isn't used
                // and instead the offset from placement is used to keep the object in the same position upon select.
                if (grabInteractable.useDynamicAttach && grabInteractable.matchAttachPosition)
                    targetPose.position = m_DesiredPosition + m_PlacementOffset;
                else
                    targetPose.position = m_DesiredPosition + m_AttachOffset;

                if (m_UseInteractorOrientation)
                {
                    targetPose.rotation = grabInteractable.trackRotation
                        ? interactor.GetAttachTransform(grabInteractable).rotation
                        : m_OriginalObjectPose.rotation;
                }
                else
                {
                    // This is using only the y-axis, but other axes can be included if need be.
                    var angle = ComputeRotationDelta(interactor, grabInteractable);
                    var currentRotation = m_DesiredRotation * Quaternion.Euler(0f, angle, 0f);
                    targetPose.rotation = grabInteractable.trackRotation ? currentRotation : m_OriginalObjectPose.rotation;
                }
            }
        }
        void UpdateTargetScale(IXRInteractor interactor, XRGrabInteractable grabInteractable, ref Pose targetPose, ref Vector3 localScale)
        {
            var oldLocalScale = localScale;
            localScale = CalculateCurrentScale();
            var scaleFactor = new Vector3(
                localScale.x / oldLocalScale.x,
                localScale.y / oldLocalScale.y,
                localScale.z / oldLocalScale.z);

            if (scaleFactor == Vector3.one)
                return;

            m_CalculateAttachOffset = true;
            m_CalculatePlacementOffset = true;

            // Scale the object while keeping the attach transform fixed.
            // If the attach transform is at the bottom of the object, this will scale it up and out from the floor.
            // We attempt to get the static attach transform rather than the dynamic one since the dynamic would be
            // based on the raycast hit point likely at the top surface of the object rather than at the base of the
            // object where we want to scale out from.
            Transform thisAttachTransform;
            if (grabInteractable.useDynamicAttach && grabInteractable.matchAttachPosition)
            {
                grabInteractable.useDynamicAttach = false;
                thisAttachTransform = grabInteractable.GetAttachTransform(interactor);
                grabInteractable.useDynamicAttach = true;
            }
            else
            {
                thisAttachTransform = grabInteractable.GetAttachTransform(interactor);
            }

            var pivot = thisAttachTransform.position;
            var pivotDelta = targetPose.position - pivot;
            pivotDelta.Scale(scaleFactor);
            targetPose.position = pivot + pivotDelta;
        }

        void UpdateCurrentScaleRatio(XRGrabInteractable grabInteractable, in Vector3 localScale)
        {
            if (grabInteractable.isSelected && !m_ElasticBreakLimitReached)
            {
                if (m_HasScaleValueProvider && m_ScaleValueProvider.scaleMode == ScaleMode.Distance)
                {
                    m_CurrentScaleRatio += m_ScaleSensitivity * m_ScaleValueProvider.scaleValue;

                    // If the user tried to scale too far beyond the limit, then lerp back within the scale range.
                    // The desired behavior is that after hitting the break limit, the user's scale gesture is ignored
                    // while the lerp is happening, until the ratio is back within min/max range.
                    if (m_EnableElasticBreakLimit &&
                        (m_CurrentScaleRatio < -m_ElasticBreakLimit ||
                            m_CurrentScaleRatio > (1f + m_ElasticBreakLimit)))
                    {
                        m_ElasticBreakLimitReached = true;
                    }
                }
            }
            else
            {
                // Re-adjust ratio in case of min and max scale change.
                InitializeCurrentScaleRatioIfNecessary(localScale);

                // When the pinch gesture is released, lerp the scale into range if elastic scaling caused an over/under scale.
                m_CurrentScaleRatio = Mathf.Lerp(m_CurrentScaleRatio, Mathf.Clamp01(m_CurrentScaleRatio), Time.deltaTime * 8f);

                // Clamp the scale ratio to 0 or 1 if it's close enough outside 0 or 1.
                m_CurrentScaleRatio = SqueezeNearly01(m_CurrentScaleRatio);

                // Once the ratio is back within range, allow the user to scale again.
                if (m_ElasticBreakLimitReached && m_CurrentScaleRatio >= 0f && m_CurrentScaleRatio <= 1f)
                {
                    m_ElasticBreakLimitReached = false;
                }
            }
        }

        /// <summary>
        /// Clamp the value to 0 or 1 if it's close enough outside 0 or 1.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The sanitized value.</returns>
        static float SqueezeNearly01(float value)
        {
            if (value < 0f && value >= -k_NearlyEqual ||
                value > 1f && value <= k_NearlyEqual + 1f)
            {
                return Mathf.Clamp01(value);
            }

            return value;
        }

        void InitializeCurrentScaleRatio(in Vector3 localScale)
        {
            var scaleDelta = Mathf.Max(m_MaxScale - m_MinScale, 0f);
            m_CurrentScaleRatio = scaleDelta != 0f ? SqueezeNearly01((localScale.x - m_MinScale) / scaleDelta) : 1f;
            m_CapturedMinScale = m_MinScale;
            m_CapturedMaxScale = m_MaxScale;
        }

        void InitializeCurrentScaleRatioIfNecessary(in Vector3 localScale)
        {
            if (!Mathf.Approximately(m_CapturedMinScale, m_MinScale) ||
                !Mathf.Approximately(m_CapturedMaxScale, m_MaxScale))
            {
                InitializeCurrentScaleRatio(localScale);
            }
        }

        float CalculateElasticDelta()
        {
            float overRatio;
            if (m_CurrentScaleRatio > 1f)
                overRatio = m_CurrentScaleRatio - 1f;
            else if (m_CurrentScaleRatio < 0f)
                overRatio = m_CurrentScaleRatio;
            else
                return 0f;

            return (1f - 1f / (Mathf.Abs(overRatio) * m_Elasticity + 1f)) * Mathf.Sign(overRatio);
        }

        Vector3 CalculateCurrentScale()
        {
            var elasticScaleRatio = Mathf.Clamp01(m_CurrentScaleRatio) + CalculateElasticDelta();
            var scaleDelta = Mathf.Max(m_MaxScale - m_MinScale, 0f);
            var elasticScale = m_MinScale + (elasticScaleRatio * scaleDelta);
            elasticScale = Mathf.Max(elasticScale, k_MinElasticScale);
            return new Vector3(elasticScale, elasticScale, elasticScale);
        }

        float ComputeRotationDelta(IXRInteractor interactor, XRGrabInteractable grabInteractable)
        {
            // Right now, default is y, but we can probably add the other axes if need be.
            var interactorAttachTransform = interactor.GetAttachTransform(grabInteractable);
            var angle = interactorAttachTransform.localEulerAngles.y - m_LastEulerRotation.y;

            if (angle > 180)
            {
                angle -= 360;
            }

            if (angle < -180)
            {
                angle += 360;
            }

            m_LastEulerRotation = interactorAttachTransform.localEulerAngles;
            return angle;
        }

        void CalculateAttachOffset(IXRInteractor interactor, XRGrabInteractable grabInteractable)
        {
            // Calculate offset to the interactable transform from the attach transform.
            var thisAttachTransform = grabInteractable.GetAttachTransform(interactor);
            var attachPosition = thisAttachTransform.position;
            var objectTransform = grabInteractable.transform;
            var objectPosition = objectTransform.position;

            m_AttachOffset = objectPosition - attachPosition;
            m_LocalAttachOffset = Quaternion.Inverse(objectTransform.rotation) * m_AttachOffset;
        }

        void CalculatePlacementOffset(XRGrabInteractable grabInteractable)
        {
            var objectTransform = grabInteractable.transform;

            m_PlacementOffset = objectTransform.position - m_DesiredPosition;
            m_LocalPlacementOffset = Quaternion.Inverse(objectTransform.rotation) * m_PlacementOffset;
        }

        void FindXROrigin()
        {
            if (m_XROrigin == null)
                ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin);
        }

        void FindCreateARPlaneManager()
        {
            if (m_ARPlaneManager != null || ComponentLocatorUtility<ARPlaneManager>.TryFindComponent(out m_ARPlaneManager))
                return;

            if (m_XROrigin != null)
            {
                // Add to the GameObject with the XR Origin component itself, not its potentially different Origin GameObject reference.
                m_ARPlaneManager = m_XROrigin.gameObject.AddComponent<ARPlaneManager>();
            }
            else
            {
                Debug.LogWarning($"{nameof(XROrigin)} not found, cannot add the {nameof(ARPlaneManager)} automatically. Cannot translate along surfaces in the AR environment. This component will be disabled.", this);
                enabled = false;
            }
        }
    }
}
#endif
