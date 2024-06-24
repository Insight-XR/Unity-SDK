using System.Collections.Generic;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Transformer used when an interactable is snapped to a socket.
    /// Applies both when select is active and when hover socket snapping is active.
    /// </summary>
    [HelpURL(XRHelpURLConstants.k_XRSocketGrabTransformer)]
#if BURST_PRESENT
    [BurstCompile]
#endif
    public class XRSocketGrabTransformer : IXRGrabTransformer
    {
        /// <summary>
        /// The uniform tolerance on each axis of a float3 used to determine if the socket is in place.
        /// We default to 1 cm because if smoothing is enabled it can take time to reach the target position beyond 1 cm accuracy.
        /// </summary>
        const float k_SocketSnappingAxisTolerance = 0.01f;

        /// <inheritdoc />
        public bool canProcess { get; set; } = true;

        /// <summary>
        /// When socket snapping is enabled, this is the radius within which the interactable will snap to the socket's attach transform while hovering.
        /// </summary>
        public float socketSnappingRadius { get; set; }

        /// <summary>
        /// Scale mode used to calculate the scale factor applied to the interactable when hovering.
        /// </summary>
        public SocketScaleMode scaleMode { get; set; }

        /// <summary>
        /// Scale factor applied to the interactable when scale mode is set to Fixed.
        /// </summary>
        public float3 fixedScale { get; set; } = new float3(1f, 1f, 1f);

        /// <summary>
        /// Bounds size used to calculate the scale factor applied to the interactable when scale mode is set to Stretched To Fit Size.
        /// </summary>
        public float3 targetBoundsSize { get; set; } = new float3(1f, 1f, 1f);

        /// <summary>
        /// The current socket interactor.
        /// </summary>
        public IXRInteractor socketInteractor { get; set; }

        readonly Dictionary<IXRInteractable, float3> m_InitialScale = new Dictionary<IXRInteractable, float3>();

        readonly Dictionary<IXRInteractable, float3> m_InteractableBoundsSize = new Dictionary<IXRInteractable, float3>();

        /// <inheritdoc />
        public void OnLink(XRGrabInteractable grabInteractable)
        {
        }

        /// <inheritdoc />
        public void OnGrab(XRGrabInteractable grabInteractable)
        {
        }

        /// <inheritdoc />
        public void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            if (scaleMode != SocketScaleMode.None)
                RegisterInteractableScale(grabInteractable, localScale);
        }

        /// <inheritdoc />
        public void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                {
                    if (scaleMode == SocketScaleMode.None)
                    {
                        UpdateTargetWithoutScale(grabInteractable, socketInteractor, socketSnappingRadius, ref targetPose);
                    }
                    else
                    {
                        float3 initialScale = m_InitialScale[grabInteractable];
                        float3 initialBounds = m_InteractableBoundsSize[grabInteractable];
                        float3 targetScale = ComputeSocketTargetScale(grabInteractable, initialScale);

                        UpdateTargetWithScale(grabInteractable, socketInteractor, socketSnappingRadius, initialScale, initialBounds, targetScale, ref targetPose, ref localScale);
                    }

                    break;
                }
            }
        }

        static void UpdateTargetWithoutScale(XRGrabInteractable grabInteractable, IXRInteractor interactor, float snappingRadius, ref Pose targetPose)
        {
            var hasSocketPose = GetTargetPoseForInteractable(grabInteractable, interactor, out var socketTargetPose);
            if (!hasSocketPose)
                return;

            if (!IsWithinRadius(targetPose.position, socketTargetPose.position, snappingRadius))
                return;

            targetPose = socketTargetPose;
        }

        static void UpdateTargetWithScale(XRGrabInteractable grabInteractable, IXRInteractor interactor, float innerRadius, in float3 initialScale, in float3 initialBounds, in float3 targetScale, ref Pose targetPose, ref Vector3 localScale)
        {
            var hasSocketPose = GetTargetPoseForInteractable(grabInteractable, interactor, out var socketTargetPose);
            if (!hasSocketPose)
                return;

            // We do a fast estimate to ensure the socket is roughly in place with a tolerance of 1cm on each axis.
            // This allows us to account for slow transitions from smoothing, and mask out jitter from physics simulated movement or tracking noise.
            bool isSocketInPlace = BurstMathUtility.FastVectorEquals(grabInteractable.transform.position, socketTargetPose.position, k_SocketSnappingAxisTolerance);

            // Outer radius is larger to avoid flickering when removing the object from the socket
            float outerRadius = FastCalculateRadiusOffset(initialScale, targetScale, initialBounds, innerRadius);

            float targetRadius = isSocketInPlace ? outerRadius : innerRadius;

            if (!IsWithinRadius(targetPose.position, socketTargetPose.position, targetRadius))
            {
                localScale = initialScale;
                return;
            }

            targetPose = socketTargetPose;

            // Only apply scale target when object is firmly socketed in place
            if (isSocketInPlace)
                localScale = targetScale;
        }

        /// <inheritdoc />
        public void OnUnlink(XRGrabInteractable grabInteractable)
        {
            // Ends the socket interaction for the provided interactable, resetting its scale to its initial value.
            if (m_InitialScale.TryGetValue(grabInteractable, out var initialScale))
            {
                grabInteractable.SetTargetLocalScale(initialScale);
                m_InitialScale.Remove(grabInteractable);
                m_InteractableBoundsSize.Remove(grabInteractable);
            }
        }

        bool RegisterInteractableScale(IXRInteractable targetInteractable, Vector3 scale)
        {
            if (m_InitialScale.ContainsKey(targetInteractable))
                return false;

            m_InitialScale[targetInteractable] = scale;

            var targetTransform = targetInteractable.transform;

            // Capture position and rotation
            var currentPosition = targetTransform.position;
            var currentRotation = targetTransform.rotation;

            // Revert to identity to capture accurate bounds size
            targetTransform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Capture Bounds
            m_InteractableBoundsSize[targetInteractable] = BoundsUtils.GetBounds(targetInteractable.transform).size;

            // Revert to original position and rotation
            targetTransform.SetPositionAndRotation(currentPosition, currentRotation);

            return true;
        }

        /// <summary>
        /// Function used to compute the target local scale of the interactable when it is snapped to the socket.
        /// </summary>
        /// <param name="interactable">Interactable to place.</param>
        /// <param name="initialInteractableScale">Initial interactable local scale.</param>
        /// <returns>Returns the target local scale.</returns>
        float3 ComputeSocketTargetScale(IXRInteractable interactable, in float3 initialInteractableScale)
        {
            switch (scaleMode)
            {
                case SocketScaleMode.Fixed:
                {
                    BurstMathUtility.Scale(initialInteractableScale, fixedScale, out var result);
                    return result;
                }

                case SocketScaleMode.StretchedToFitSize:
                {
                    if (!m_InteractableBoundsSize.TryGetValue(interactable, out var interactableBounds))
                        return initialInteractableScale;

                    CalculateScaleToFit(interactableBounds, targetBoundsSize, initialInteractableScale, Mathf.Epsilon, out var newScale);
                    return newScale;
                }

                case SocketScaleMode.None:
                default:
                    return initialInteractableScale;
            }
        }

        static bool GetTargetPoseForInteractable(IXRInteractable interactable, IXRInteractor interactor, out Pose targetPose)
        {
            targetPose = Pose.identity;
            var grabInteractable = interactable as XRGrabInteractable;
            if (grabInteractable == null)
                return false;

            var interactorAttachTransform = interactor.GetAttachTransform(grabInteractable);
            var interactableTransform = grabInteractable.transform;
            var interactableAttachTransform = grabInteractable.GetAttachTransform(interactor);

            // Calculate offset of the grab interactable's position relative to its attach transform
            var attachOffset = interactableTransform.position - interactableAttachTransform.position;

            // Compute the new target world pose
            if (grabInteractable.trackRotation)
            {
                // Transform that offset direction from world space to local space of the transform it's relative to.
                // It will be applied to the interactor's attach position using the orientation of the Interactor's attach transform.
                var positionOffset = interactableAttachTransform.InverseTransformDirection(attachOffset);

                FastComputeNewTrackedPose(interactorAttachTransform.position, interactorAttachTransform.rotation,
                    positionOffset, interactableTransform.rotation, interactableAttachTransform.rotation,
                    out var targetPos, out var targetRot);

                targetPose.position = targetPos;
                targetPose.rotation = targetRot;
            }
            else
            {
                // When not using the rotation of the Interactor, the world offset direction can be directly
                // added to the Interactor's attach transform position.
                targetPose.position = attachOffset + interactorAttachTransform.position;
            }

            return true;
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static float FastCalculateRadiusOffset(in float3 initialScale, in float3 targetScale, in float3 initialBoundsSize, float innerRadius)
        {
            var maxInitialBoundsParam = math.max(math.max(initialBoundsSize.x, initialBoundsSize.y), initialBoundsSize.z);

            BurstMathUtility.FastSafeDivide(targetScale, initialScale, out float3 scaleRatio);

            var scaledBoundsX = scaleRatio.x * initialBoundsSize.x;
            var scaledBoundsY = scaleRatio.y * initialBoundsSize.y;
            var scaledBoundsZ = scaleRatio.z * initialBoundsSize.z;

            var maxScaledBoundsParam = math.max(math.max(scaledBoundsX, scaledBoundsY), scaledBoundsZ);
            var maxBoundsParam = math.max(maxInitialBoundsParam, maxScaledBoundsParam);

            // Divide max bounds parameter by 2 to get the radius offset
            return innerRadius + (maxBoundsParam / 2f);
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void FastComputeNewTrackedPose(in float3 interactorAttachPos, in quaternion interactorAttachRot,
            in float3 positionOffset, in quaternion interactableRot, in quaternion interactableAttachRot,
            out float3 targetPos, out quaternion targetRot)
        {
            quaternion rotationOffset = math.inverse(math.mul(math.inverse(interactableRot), interactableAttachRot));

            targetPos = math.mul(interactorAttachRot, positionOffset) + interactorAttachPos;
            targetRot = math.mul(interactorAttachRot, rotationOffset);
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static bool IsWithinRadius(in float3 a, in float3 b, float radius)
        {
            return math.lengthsq(a - b) < (radius * radius);
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void CalculateScaleToFit(in float3 boundsSize, in float3 fixedSize, in float3 initialScale, float epsilon, out float3 newScale)
        {
            // Find the ratio of the current size to the target size
            float sizeRatioX = boundsSize.x / (fixedSize.x + epsilon);
            float sizeRatioY = boundsSize.y / (fixedSize.y + epsilon);
            float sizeRatioZ = boundsSize.z / (fixedSize.z + epsilon);

            // Find the maximum ratio to ensure the object fits within the target size
            float maxRatio = math.max(math.max(sizeRatioX, sizeRatioY), sizeRatioZ);

            // Calculate the new scale based on the ratio
            newScale = initialScale / maxRatio;
        }
    }
}
