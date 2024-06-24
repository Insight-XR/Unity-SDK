using System;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Grab transformer which supports moving and rotating unconstrained with one or two interactors.
    /// Also allows clamped or unclamped scaling when using two interactors.
    /// Allows axis constraints on translation.
    /// This is the default grab transformer.
    /// </summary>
    /// <seealso cref="XRGrabInteractable"/>
    [AddComponentMenu("XR/Transformers/XR General Grab Transformer", 11)]
    [HelpURL(XRHelpURLConstants.k_XRGeneralGrabTransformer)]
#if BURST_PRESENT
    [BurstCompile]
#endif
    public class XRGeneralGrabTransformer : XRBaseGrabTransformer
    {
        /// <summary>
        /// Axis constraint enum.
        /// </summary>
        /// <seealso cref="permittedDisplacementAxes"/>
        [Flags]
        public enum ManipulationAxes
        {
            /// <summary>
            /// X-axis movement is permitted.
            /// </summary>
            X = 1 << 0,

            /// <summary>
            /// Y-axis movement is permitted.
            /// </summary>
            Y = 1 << 1,

            /// <summary>
            /// Z-axis movement is permitted.
            /// </summary>
            Z = 1 << 2,

            /// <summary>
            /// All axes movement is permitted.
            /// Shortcut for <c>ManipulationAxes.X | ManipulationAxes.Y | ManipulationAxes.Z</c>.
            /// </summary>
            All = X | Y | Z,
        }

        /// <summary>
        /// Constrained Axis Displacement Mode
        /// </summary>
        /// <seealso cref="constrainedAxisDisplacementMode"/>
        public enum ConstrainedAxisDisplacementMode
        {
            /// <summary>
            /// Determines the permitted axes based on the initial object rotation in world space.
            /// </summary>
            ObjectRelative,

            /// <summary>
            /// Determines the permitted axes based on the initial object rotation in world space, but also locks the up axis to be the world up.
            /// </summary>
            ObjectRelativeWithLockedWorldUp,

            /// <summary>
            /// Uses the world axes to project all displacement against.
            /// </summary>
            WorldAxisRelative,
        }

        /// <summary>
        /// Two handed rotation mode.
        /// </summary>
        public enum TwoHandedRotationMode
        {
            /// <summary>
            /// Determines rotation using only first hand.
            /// </summary>
            FirstHandOnly,

            /// <summary>
            /// Determines two handed rotation using first hand and then directing the object towards the second one.
            /// </summary>
            FirstHandDirectedTowardsSecondHand,

            /// <summary>
            /// Directs first hand towards second hand, but uses the two handed average to determine the base rotation.
            /// </summary>
            TwoHandedAverage,
        }

        [Header("Translation Constraints")]
        [SerializeField]
        [Tooltip("Permitted axes for translation displacement relative to the object's initial rotation.")]
        ManipulationAxes m_PermittedDisplacementAxes = ManipulationAxes.All;

        /// <summary>
        /// Permitted axes for translation displacement relative to the object's initial rotation.
        /// </summary>
        /// <seealso cref="ManipulationAxes"/>
        public ManipulationAxes permittedDisplacementAxes
        {
            get => m_PermittedDisplacementAxes;
            set => m_PermittedDisplacementAxes = value;
        }

        [SerializeField]
        [Tooltip("Determines how the constrained axis displacement mode is computed.")]
        ConstrainedAxisDisplacementMode m_ConstrainedAxisDisplacementMode = ConstrainedAxisDisplacementMode.ObjectRelativeWithLockedWorldUp;

        /// <summary>
        /// Determines how the constrained axis displacement mode is computed.
        /// </summary>
        /// <seealso cref="ConstrainedAxisDisplacementMode"/>
        public ConstrainedAxisDisplacementMode constrainedAxisDisplacementMode
        {
            get => m_ConstrainedAxisDisplacementMode;
            set => m_ConstrainedAxisDisplacementMode = value;
        }

        [Header("Rotation Constraints")]
        [SerializeField]
        [Tooltip("Determines how rotation is calculated when using two hands for the grab interaction.")]
        TwoHandedRotationMode m_TwoHandedRotationMode = TwoHandedRotationMode.FirstHandDirectedTowardsSecondHand;

        /// <summary>
        /// Determines how rotation is calculated when using two hands for the grab interaction.
        /// </summary>
        /// <seealso cref="TwoHandedRotationMode"/>
        public TwoHandedRotationMode allowTwoHandedRotation
        {
            get => m_TwoHandedRotationMode;
            set => m_TwoHandedRotationMode = value;
        }

        [Header("Scaling Constraints")]
        [SerializeField]
        [Tooltip("Allow one handed scaling using the scale value provider if available.")]
        bool m_AllowOneHandedScaling = true;
        
        /// <summary>
        /// Allow one handed scaling using the scale value provider if available.
        /// </summary>
        public bool allowOneHandedScaling
        {
            get => m_AllowOneHandedScaling;
            set => m_AllowOneHandedScaling = value;
        }
        
        [SerializeField]
        [Tooltip("Allow scaling when using multi-grab interaction.")]
        bool m_AllowTwoHandedScaling;

        /// <summary>
        /// Allow scaling when using multi-grab interaction.
        /// </summary>
        public bool allowTwoHandedScaling
        {
            get => m_AllowTwoHandedScaling;
            set => m_AllowTwoHandedScaling = value;
        }
        
        [SerializeField]
        [Tooltip("Scaling speed over time for one handed scaling based on the scale value provider.")]
        [Range(0f, 32f)]
        float m_OneHandedScaleSpeed = 0.5f;
        
        /// <summary>
        /// Scaling speed over time for one handed scaling based on the <see cref="IXRScaleValueProvider"/>
        /// </summary>
        public float oneHandedScaleSpeed
        {
            get => m_OneHandedScaleSpeed;
            set => m_OneHandedScaleSpeed = Mathf.Max(value, 0f);
        }

        [SerializeField]
        [Tooltip("(Two Handed Scaling) Percentage as a measure of 0 to 1 of scaled relative hand displacement required to trigger scale operation." +
                 "\nIf this value is 0f, scaling happens the moment both grab interactors move closer or further away from each other." +
                 "\nOtherwise, this percentage is used as a threshold before any scaling happens.")]
        [Range(0f, 1f)]
        float m_ThresholdMoveRatioForScale = 0.05f;

        /// <summary>
        /// (Two Handed Scaling) Percentage as a measure of 0 to 1 of scaled relative hand displacement required to trigger scale operation.
        /// If this value is 0f, scaling happens the moment both grab interactors move closer or further away from each other.
        /// Otherwise, this percentage is used as a threshold before any scaling happens.
        /// </summary>
        public float thresholdMoveRatioForScale
        {
            get => m_ThresholdMoveRatioForScale;
            set => m_ThresholdMoveRatioForScale = value;
        }

        [Space]
        [SerializeField]
        [Tooltip("If enabled, scaling will abide by ratio ranges defined below.")]
        bool m_ClampScaling = true;

        /// <summary>
        /// If enabled, scaling will abide by ratio ranges defined by <see cref="minimumScaleRatio"/> and <see cref="maximumScaleRatio"/>.
        /// </summary>
        public bool clampScaling
        {
            get => m_ClampScaling;
            set => m_ClampScaling = value;
        }

        [SerializeField]
        [Tooltip("Minimum scale multiplier applied to the initial scale captured on start.")]
        [Range(0.01f, 1f)]
        float m_MinimumScaleRatio = 0.25f;

        /// <summary>
        /// Minimum scale multiplier applied to the initial scale captured on start.
        /// </summary>
        public float minimumScaleRatio
        {
            get => m_MinimumScaleRatio;
            set
            {
                m_MinimumScaleRatio = Mathf.Min(1f, value);
                m_MinimumScale = m_InitialScale * m_MinimumScaleRatio;
            }
        }

        [SerializeField]
        [Tooltip("Maximum scale multiplier applied to the initial scale captured on start.")]
        [Range(1f, 10f)]
        float m_MaximumScaleRatio = 2f;

        /// <summary>
        /// Maximum scale multiplier applied to the initial scale captured on start.
        /// </summary>
        public float maximumScaleRatio
        {
            get => m_MaximumScaleRatio;
            set
            {
                m_MaximumScaleRatio = Mathf.Max(1f, value);
                m_MaximumScale = m_InitialScale * m_MaximumScaleRatio;
            }
        }

        [Space]
        [SerializeField]
        [Range(0.1f, 5f)]
        [Tooltip("Scales the distance of displacement between interactors needed to modify the scale interactable.")]
        float m_ScaleMultiplier = 0.25f;

        /// <summary>
        /// Scales the distance of displacement between interactors needed to modify the scale interactable.
        /// </summary>
        public float scaleMultiplier
        {
            get => m_ScaleMultiplier;
            set => m_ScaleMultiplier = value;
        }

        /// <inheritdoc />
        protected override RegistrationMode registrationMode => RegistrationMode.SingleAndMultiple;

        Pose m_OriginalObjectPose;
        Pose m_OffsetPose;
        Pose m_OriginalInteractorPose;
        Vector3 m_InteractorLocalGrabPoint;
        Vector3 m_ObjectLocalGrabPoint;
        IXRInteractor m_OriginalInteractor;

        // Two handed grab start cached values
        int m_LastGrabCount;
        Vector3 m_StartHandleBar;
        Vector3 m_StartHandleBarNormalized;
        Quaternion m_StartHandleBarLookRotation;
        Quaternion m_InverseStartHandleBarLookRotation;
        Quaternion m_LastHandleBarLocalRotation;
        Vector3 m_ScaleAtGrabStart;

        bool m_FirstFrameSinceTwoHandedGrab;
        Vector3 m_LastTwoHandedUp;

        Vector3 m_InitialScale;
        Vector3 m_InitialScaleProportions;
        Vector3 m_MinimumScale;
        Vector3 m_MaximumScale;

        ConstrainedAxisDisplacementMode m_ConstrainedAxisDisplacementModeOnGrab;
        ManipulationAxes m_PermittedDisplacementAxesOnGrab;
        
        IXRScaleValueProvider m_ScaleValueProvider;
        bool m_HasScaleValueProvider;
        
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_InitialScale = transform.localScale;
            var maxComponent = Mathf.Max(Mathf.Abs(m_InitialScale.x), Mathf.Abs(m_InitialScale.y), Mathf.Abs(m_InitialScale.z));
            m_InitialScaleProportions = m_InitialScale.SafeDivide(new Vector3(maxComponent, maxComponent, maxComponent));
        }

        /// <inheritdoc />
        public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                {
                    UpdateTarget(grabInteractable, ref targetPose, ref localScale);
                    break;
                }
            }
        }

        /// <inheritdoc />
        public override void OnGrab(XRGrabInteractable grabInteractable)
        {
            base.OnGrab(grabInteractable);

            var interactor = grabInteractable.interactorsSelecting[0];
            var grabInteractableTransform = grabInteractable.transform;
            var grabAttachTransform = grabInteractable.GetAttachTransform(interactor);

            m_ScaleValueProvider = interactor as IXRScaleValueProvider;
            m_HasScaleValueProvider = m_ScaleValueProvider != null;

            m_OriginalObjectPose = grabInteractableTransform.GetWorldPose();
            m_OriginalInteractorPose = interactor.GetAttachTransform(grabInteractable).GetWorldPose();
            m_OriginalInteractor = interactor;
            m_LastGrabCount = 1;

            Vector3 offsetTargetPosition = Vector3.zero;
            Quaternion offsetTargetRotation = Quaternion.identity;

            Quaternion capturedRotation = m_OriginalObjectPose.rotation;
            if (grabInteractable.trackRotation)
            {
                capturedRotation = m_OriginalInteractorPose.rotation;

                offsetTargetRotation = Quaternion.Inverse(Quaternion.Inverse(m_OriginalObjectPose.rotation) * grabAttachTransform.rotation);
            }

            Vector3 capturedPosition = m_OriginalObjectPose.position;
            if (grabInteractable.trackPosition)
            {
                capturedPosition = m_OriginalInteractorPose.position;

                // Calculate offset of the grab interactable's position relative to its attach transform
                var attachOffset = m_OriginalObjectPose.position - grabAttachTransform.position;

                offsetTargetPosition = grabInteractable.trackRotation ? grabAttachTransform.InverseTransformDirection(attachOffset) : attachOffset;
            }

            // Cache axis settings on grab because changing them while grab is in progress can lead to undesired results.
            m_ConstrainedAxisDisplacementModeOnGrab = m_ConstrainedAxisDisplacementMode;
            m_PermittedDisplacementAxesOnGrab = m_PermittedDisplacementAxes;

            // Adjust capture position according to permitted axes
            capturedPosition = AdjustPositionForPermittedAxes(capturedPosition, m_OriginalObjectPose, m_PermittedDisplacementAxesOnGrab, m_ConstrainedAxisDisplacementModeOnGrab);

            // Store adjusted transform pose
            m_OriginalObjectPose = new Pose(capturedPosition, capturedRotation);

            Vector3 localScale = grabInteractableTransform.localScale;
            TranslateSetup(m_OriginalInteractorPose, m_OriginalInteractorPose.position, m_OriginalObjectPose, localScale);

            Quaternion worldToGripRotation = offsetTargetRotation * Quaternion.Inverse(m_OriginalInteractorPose.rotation);
            Quaternion relativeCaptureRotation = worldToGripRotation * m_OriginalObjectPose.rotation;

            // Scale offset target position to match new local scale
            Vector3 scaledOffsetTargetPosition = offsetTargetPosition.Divide(localScale);

            m_OffsetPose = new Pose(scaledOffsetTargetPosition, relativeCaptureRotation);
        }

        /// <inheritdoc />
        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            base.OnGrabCountChanged(grabInteractable, targetPose, localScale);

            var newGrabCount = grabInteractable.interactorsSelecting.Count;
            if (newGrabCount == 1)
            {
                // If the initial grab interactor changes, or we reduce the grab count, we need to recompute initial grab parameters. 
                var interactor0 = grabInteractable.interactorsSelecting[0];
                if (interactor0 != m_OriginalInteractor || newGrabCount < m_LastGrabCount)
                {
                    OnGrab(grabInteractable);
                }
            }
            else if (newGrabCount > 1)
            {
                var interactor0 = grabInteractable.interactorsSelecting[0];
                var interactor1 = grabInteractable.interactorsSelecting[1];

                var interactor0Transform = interactor0.GetAttachTransform(grabInteractable);
                var grabAttachTransform1 = grabInteractable.GetAttachTransform(interactor1);

                m_ScaleAtGrabStart = localScale;

                m_StartHandleBar = interactor0Transform.InverseTransformPoint(grabAttachTransform1.position);
                m_StartHandleBarNormalized = m_StartHandleBar.normalized;
                
                m_StartHandleBarLookRotation = Quaternion.LookRotation(m_StartHandleBarNormalized, BurstMathUtility.Orthogonal(m_StartHandleBarNormalized));
                m_InverseStartHandleBarLookRotation = Quaternion.Inverse(m_StartHandleBarLookRotation);
                m_LastHandleBarLocalRotation = m_StartHandleBarLookRotation;

                m_FirstFrameSinceTwoHandedGrab = true;
            }

            m_LastGrabCount = newGrabCount;

            // Precompute scale range to support modifying the values in the Inspector window at runtime without needing to multiply every frame.
            m_MinimumScale = m_InitialScale * m_MinimumScaleRatio;
            m_MaximumScale = m_InitialScale * m_MaximumScaleRatio;
        }

        void ComputeAdjustedInteractorPose(XRGrabInteractable grabInteractable, out Vector3 newHandleBar, out Vector3 adjustedInteractorPosition, out Quaternion adjustedInteractorRotation)
        {
            if (grabInteractable.interactorsSelecting.Count == 1 || m_TwoHandedRotationMode == TwoHandedRotationMode.FirstHandOnly)
            {
                newHandleBar = m_StartHandleBar;
                var attachTransform = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable);
                adjustedInteractorPosition = attachTransform.position;
                adjustedInteractorRotation = attachTransform.rotation;
                return;
            }

            if (grabInteractable.interactorsSelecting.Count > 1)
            {
                var interactor0 = grabInteractable.interactorsSelecting[0];
                var interactor1 = grabInteractable.interactorsSelecting[1];

                var interactor0Transform = interactor0.GetAttachTransform(grabInteractable);
                var interactor1Transform = interactor1.GetAttachTransform(grabInteractable);

                newHandleBar = interactor0Transform.InverseTransformPoint(interactor1Transform.position);

                Quaternion newRotation;
                if (m_TwoHandedRotationMode == TwoHandedRotationMode.FirstHandDirectedTowardsSecondHand)
                {
                    // Use the fallback axis as the 'up' direction for the LookRotation
                    Vector3 newHandleBarNormalized = newHandleBar.normalized;

                    // Use the last calculated rotation to compute a temporally coherent up vector
                    Vector3 newUpVector = m_LastHandleBarLocalRotation * Vector3.up;
                    
                    Quaternion newHandleBarLocalRotation = Quaternion.LookRotation(newHandleBarNormalized, newUpVector);
                    
                    // Store the last handle bar rotation for the next frame
                    m_LastHandleBarLocalRotation = newHandleBarLocalRotation;

                    // Compute the rotation difference
                    Quaternion rotationDiff = newHandleBarLocalRotation * m_InverseStartHandleBarLookRotation;

                    // Update the rotation
                    newRotation = interactor0Transform.rotation * rotationDiff;
                }
                else if (m_TwoHandedRotationMode == TwoHandedRotationMode.TwoHandedAverage)
                {
                    var forward = (interactor1Transform.position - interactor0Transform.position).normalized;

                    var averageRight = Vector3.Slerp(interactor0Transform.right, interactor1Transform.right, 0.5f);
                    var up = Vector3.Slerp(interactor0Transform.up, interactor1Transform.up, 0.5f);

                    var crossUp = Vector3.Cross(forward, averageRight);
                    var angleDiff = Mathf.PingPong(Vector3.Angle(up, forward), 90f);
                    up = Vector3.Slerp(crossUp, up, angleDiff / 90f);

                    var crossRight = Vector3.Cross(up, forward);
                    up = Vector3.Cross(forward, crossRight);

                    if (m_FirstFrameSinceTwoHandedGrab)
                    {
                        m_FirstFrameSinceTwoHandedGrab = false;
                    }
                    else
                    {
                        // We also keep track of whether the up vector was pointing up or down previously, to allow for objects to be flipped through a series of rotations
                        // Such as a 180 degree rotation on the y, followed by a 180 degree rotation on the x
                        if (Vector3.Dot(up, m_LastTwoHandedUp) <= 0f)
                        {
                            up = -up;
                        }
                    }

                    m_LastTwoHandedUp = up;

                    var twoHandedRotation = Quaternion.LookRotation(forward, up);
                    
                    // Given that this rotation method doesn't really consider the first interactor's start rotation, we have to remove the offset pose computed on grab. 
                    newRotation = twoHandedRotation * Quaternion.Inverse(m_OffsetPose.rotation);
                }
                else
                {
                    newRotation = interactor0Transform.rotation;
                }

                adjustedInteractorPosition = interactor0Transform.position;
                adjustedInteractorRotation = newRotation;
                return;
            }

            newHandleBar = m_StartHandleBar;
            adjustedInteractorPosition = Vector3.zero;
            adjustedInteractorRotation = Quaternion.identity;
        }

        void TranslateSetup(Pose interactorCentroidPose, Vector3 grabCentroid, Pose objectPose, Vector3 objectScale)
        {
            Quaternion worldToInteractorRotation = Quaternion.Inverse(interactorCentroidPose.rotation);
            m_InteractorLocalGrabPoint = worldToInteractorRotation * (grabCentroid - interactorCentroidPose.position);

            m_ObjectLocalGrabPoint = Quaternion.Inverse(objectPose.rotation) * (grabCentroid - objectPose.position);
            m_ObjectLocalGrabPoint = m_ObjectLocalGrabPoint.Divide(objectScale);
        }
        

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void ComputeNewObjectPosition(in float3 interactorPosition, in quaternion interactorRotation, in quaternion objectRotation, in float3 objectScale, bool trackRotation, in float3 offsetPosition, in float3 objectLocalGrabPoint, in float3 interactorLocalGrabPoint, out Vector3 newPosition)
        {
            // Scale up offset pose with new object scale
            float3 scaledOffsetPose = Scale(offsetPosition, objectScale);

            // Adjust computed offset with current source rotation
            float3 rotationAdjustedOffset = math.mul(interactorRotation, scaledOffsetPose);
            float3 rotationAdjustedTargetOffset = trackRotation ? rotationAdjustedOffset : scaledOffsetPose;
            float3 newTargetPosition = interactorPosition + rotationAdjustedTargetOffset;

            float3 scaledGrabToObject = Scale(objectLocalGrabPoint, objectScale);
            float3 adjustedInteractorToGrab = interactorLocalGrabPoint;

            adjustedInteractorToGrab = math.mul(interactorRotation, adjustedInteractorToGrab);
            var rotatedScaledGrabToObject = math.mul(objectRotation, scaledGrabToObject);
            
            newPosition = adjustedInteractorToGrab - rotatedScaledGrabToObject + newTargetPosition;
        }
        
        static float3 Scale(float3 a, float3 b) => new float3(a.x * b.x, a.y * b.y, a.z * b.z);

        Quaternion ComputeNewObjectRotation(in Quaternion interactorRotation, bool trackRotation)
        {
            if (!trackRotation)
                return m_OriginalObjectPose.rotation;
            return interactorRotation * m_OffsetPose.rotation;
        }

        static Vector3 AdjustPositionForPermittedAxes(in Vector3 targetPosition, in Pose originalObjectPose, ManipulationAxes permittedAxes, ConstrainedAxisDisplacementMode axisDisplacementMode)
        {
            bool hasX = (permittedAxes & ManipulationAxes.X) != 0;
            bool hasY = (permittedAxes & ManipulationAxes.Y) != 0;
            bool hasZ = (permittedAxes & ManipulationAxes.Z) != 0;

            if (hasX && hasY && hasZ)
                return targetPosition;

            if (!hasX && !hasY && !hasZ)
                return originalObjectPose.position;
        
            AdjustPositionForPermittedAxesBurst(targetPosition, originalObjectPose, axisDisplacementMode, hasX, hasY, hasZ, out Vector3 adjustedTargetPosition);
            return adjustedTargetPosition;
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void AdjustPositionForPermittedAxesBurst(in Vector3 targetPosition, in Pose originalObjectPose, ConstrainedAxisDisplacementMode axisDisplacementMode, bool hasX, bool hasY, bool hasZ, out Vector3 adjustedTargetPosition)
        {
            float3 xComponent = float3.zero;
            float3 yComponent = float3.zero;
            float3 zComponent = float3.zero;

            float3 right = new float3(1f, 0f, 0f);
            float3 up = new float3(0f, 1f, 0f);
            float3 forward = new float3(0f, 0f, 1f);
            
            float3 translationVector = targetPosition - originalObjectPose.position;
            float3 sumTranslationVector = float3.zero;
            float3 originalObjectPosition = originalObjectPose.position; 
            quaternion objectRotation = originalObjectPose.rotation;

            if (axisDisplacementMode == ConstrainedAxisDisplacementMode.WorldAxisRelative)
            {
                if (hasX)
                    xComponent = math.project(translationVector, right);

                if (hasY)
                    yComponent = math.project(translationVector, up);

                if (hasZ)
                    zComponent = math.project(translationVector, forward);

                sumTranslationVector = (xComponent + yComponent + zComponent);
            }
            else if (axisDisplacementMode == ConstrainedAxisDisplacementMode.ObjectRelative)
            {
                if (hasX)
                {
                    float3 rotatedRight = math.mul(objectRotation, right);
                    xComponent = math.project(translationVector, rotatedRight);
                }
                
                if (hasY)
                {
                    float3 rotatedUp = math.mul(objectRotation, up);
                    yComponent = math.project(translationVector, rotatedUp);
                }
                
                if (hasZ)
                {
                    float3 rotatedForward = math.mul(objectRotation, forward);
                    zComponent = math.project(translationVector, rotatedForward);
                }

                sumTranslationVector = (xComponent + yComponent + zComponent);
            }
            else if (axisDisplacementMode == ConstrainedAxisDisplacementMode.ObjectRelativeWithLockedWorldUp)
            {
                if (hasX && hasZ)
                {
                    BurstMathUtility.ProjectOnPlane(translationVector, up, out sumTranslationVector);
                }
                else
                {
                    float3 upComponent = Vector3.zero;

                    if (hasX)
                    {
                        float3 rotatedRight = math.mul(objectRotation, right);
                        xComponent = math.project(translationVector, rotatedRight);
                    }

                    if (hasY)
                    {
                        float3 rotatedUp = math.mul(objectRotation, up);
                        yComponent = math.project(translationVector, rotatedUp);
                        upComponent = math.project(translationVector, up);
                    }

                    if (hasZ)
                    {
                        float3 rotatedForward = math.mul(objectRotation, forward);
                        zComponent = math.project(translationVector, rotatedForward);
                    }

                    BurstMathUtility.ProjectOnPlane(xComponent + yComponent + zComponent, up, out var projectedSum);
                    sumTranslationVector = projectedSum + upComponent;
                }
            }

            adjustedTargetPosition = originalObjectPosition + sumTranslationVector;
        }
        
        Vector3 ComputeNewScale(in XRGrabInteractable grabInteractable, in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool trackScale)
        {
            var interactorsCount = grabInteractable.interactorsSelecting.Count;
            if (trackScale && interactorsCount == 1 && m_AllowOneHandedScaling && m_HasScaleValueProvider && m_ScaleValueProvider.scaleMode == ScaleMode.Input)
            {
                var scaleDelta = m_ScaleValueProvider.scaleValue;
                if (Mathf.Approximately(scaleDelta, 0f))
                    return currentScale;
                
                ComputeNewOneHandedScale(currentScale, m_InitialScaleProportions, m_ClampScaling, m_MinimumScale, m_MaximumScale, scaleDelta, Time.deltaTime, m_OneHandedScaleSpeed, out var newOneHandedScale);
                return newOneHandedScale;
            }

            if (trackScale && interactorsCount > 1 && m_AllowTwoHandedScaling)
            {
                ComputeNewTwoHandedScale(startScale, currentScale, startHandleBar, newHandleBar, m_ClampScaling, m_ScaleMultiplier, m_ThresholdMoveRatioForScale, m_MinimumScale, m_MaximumScale, out var newTwoHandedScale);
                return newTwoHandedScale;
            }

            return currentScale;
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void ComputeNewOneHandedScale(in Vector3 currentScale, in Vector3 initialScaleProportions, bool clampScale, in Vector3 minScale, in Vector3 maxScale, float scaleDelta, float deltaTime, float scaleSpeed, out Vector3 newScale)
        {
            newScale = currentScale;

            var scaleAmount = scaleDelta * deltaTime * scaleSpeed;
            var scaleAmount3 = new float3(scaleAmount, scaleAmount, scaleAmount);
            BurstMathUtility.Scale(scaleAmount3, (float3)initialScaleProportions, out var proportionedScaleAmount);
            float3 targetScale = (float3)currentScale + proportionedScaleAmount;
            if (!clampScale)
            {
                newScale = math.max(targetScale, float3.zero);
                return;
            }

            if (scaleAmount > 0f)
            {
                var isOverMaximum =
                    math.abs(targetScale.x) > math.abs(maxScale.x) ||
                    math.abs(targetScale.y) > math.abs(maxScale.y) ||
                    math.abs(targetScale.z) > math.abs(maxScale.z);
                newScale = isOverMaximum ? maxScale : (Vector3)targetScale;
            }
            else if (scaleAmount < 0f)
            {
                var isUnderMinimum =
                    math.abs(targetScale.x) < math.abs(minScale.x) ||
                    math.abs(targetScale.y) < math.abs(minScale.y) ||
                    math.abs(targetScale.z) < math.abs(minScale.z);
                newScale = isUnderMinimum ? minScale : (Vector3)targetScale;
            }
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void ComputeNewTwoHandedScale(in Vector3 startScale, in Vector3 currentScale, in Vector3 startHandleBar, in Vector3 newHandleBar, bool clampScale, float scaleMultiplier, float thresholdMoveRatioForScale, in Vector3 minScale, in Vector3 maxScale,  out Vector3 newScale)
        {
            newScale = currentScale;

            var scaleRatio = math.length(newHandleBar) / math.length(startHandleBar);
            if (scaleRatio > 1)
            {
                var amountOver1 = (scaleRatio - 1f);
                var multipliedAmountOver1 = amountOver1 * scaleMultiplier;

                var multipliedAmountOver1WithoutThreshold = multipliedAmountOver1 - thresholdMoveRatioForScale;
                if (multipliedAmountOver1WithoutThreshold < 0f)
                    return;

                var targetScaleRatio = 1f + multipliedAmountOver1WithoutThreshold;
                var targetScale = targetScaleRatio * startScale;

                var isOverMaximum =
                    math.abs(targetScale.x) > math.abs(maxScale.x) ||
                    math.abs(targetScale.y) > math.abs(maxScale.y) ||
                    math.abs(targetScale.z) > math.abs(maxScale.z);
                newScale = isOverMaximum && clampScale ? maxScale : targetScale;
            }
            else if (scaleRatio < 1f)
            {
                var invertedScaleRatio = 1f / scaleRatio;
                var amountOver1 = invertedScaleRatio - 1f;
                var multipliedAmountOver1 = amountOver1 * scaleMultiplier;

                var multipliedAmountOver1WithoutThreshold = multipliedAmountOver1 - thresholdMoveRatioForScale;
                if (multipliedAmountOver1WithoutThreshold < 0f)
                    return;

                var invertedTargetScaleRatio = 1f + multipliedAmountOver1WithoutThreshold;
                var targetScale = 1f / invertedTargetScaleRatio * startScale;

                var isUnderMinimum =
                    math.abs(targetScale.x) < math.abs(minScale.x) ||
                    math.abs(targetScale.y) < math.abs(minScale.y) ||
                    math.abs(targetScale.z) < math.abs(minScale.z);
                newScale = isUnderMinimum && clampScale ? minScale : targetScale;
            }
        }

        void UpdateTarget(XRGrabInteractable grabInteractable, ref Pose targetPose, ref Vector3 localScale)
        {
            ComputeAdjustedInteractorPose(grabInteractable, out Vector3 newHandleBar, out Vector3 adjustedInteractorPosition, out  Quaternion adjustedInteractorRotation);

            localScale = ComputeNewScale(grabInteractable, m_ScaleAtGrabStart, localScale, m_StartHandleBar, newHandleBar, grabInteractable.trackScale);

            targetPose.rotation = ComputeNewObjectRotation(adjustedInteractorRotation, grabInteractable.trackRotation);

            ComputeNewObjectPosition(adjustedInteractorPosition,  adjustedInteractorRotation, 
                targetPose.rotation, localScale, grabInteractable.trackRotation, 
                m_OffsetPose.position, m_ObjectLocalGrabPoint, m_InteractorLocalGrabPoint,
                out Vector3 targetObjectPosition);
            
            targetPose.position = AdjustPositionForPermittedAxes(targetObjectPosition, m_OriginalObjectPose, m_PermittedDisplacementAxesOnGrab, m_ConstrainedAxisDisplacementModeOnGrab);
        }
    }
}