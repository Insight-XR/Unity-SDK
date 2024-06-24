using Unity.XR.CoreUtils;
using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Grab transformer which supports moving and rotating unconstrained with multiple Interactors.
    /// Maintains the offset from the attachment points used for each Interactor and points in the
    /// direction made by each grab.
    /// </summary>
    /// <remarks>
    /// When there is a single Interactor, this has identical behavior to <see cref="XRSingleGrabFreeTransformer"/>.
    /// </remarks>
    /// <seealso cref="XRGrabInteractable"/>
    [AddComponentMenu("XR/Transformers/XR Dual Grab Free Transformer", 11)]
    [HelpURL(XRHelpURLConstants.k_XRDualGrabFreeTransformer)]
    public class XRDualGrabFreeTransformer : XRBaseGrabTransformer
    {
        /// <summary>
        /// Describes which combination of interactors influences a pose.
        /// </summary>
        public enum PoseContributor
        {
            /// <summary>
            /// Use the first interactor's data.
            /// </summary>
            First,

            /// <summary>
            /// Use the second interactor's data.
            /// </summary>
            Second,

            /// <summary>
            /// Use an average of the first and second interactor's data.
            /// </summary>
            Average,
        }

        [SerializeField]
        PoseContributor m_MultiSelectPosition = PoseContributor.First;

        /// <summary>
        /// Controls how multiple interactors combine to drive this interactable's position
        /// </summary>
        /// <seealso cref="PoseContributor"/>
        public PoseContributor multiSelectPosition
        {
            get => m_MultiSelectPosition;
            set => m_MultiSelectPosition = value;
        }

        [SerializeField]
        PoseContributor m_MultiSelectRotation = PoseContributor.Average;

        /// <summary>
        /// Controls how multiple interactors combine to drive this interactable's rotation
        /// </summary>
        /// <seealso cref="PoseContributor"/>
        public PoseContributor multiSelectRotation
        {
            get => m_MultiSelectRotation;
            set => m_MultiSelectRotation = value;
        }

        /// <inheritdoc />
        protected override RegistrationMode registrationMode => RegistrationMode.Multiple;

        // For Gizmo
        internal Pose lastInteractorAttachPose { get; private set; }

        Vector3 m_LastUp;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        // ReSharper disable once Unity.RedundantEventFunction -- See comment in method
        protected virtual void OnDrawGizmosSelected()
        {
            // Empty method, but needed to allow the user to toggle it by folding the component in the Inspector window
            // and making it visible in the Gizmos dropdown in the Scene view.
        }

        /// <inheritdoc />
        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
            if (grabInteractable.interactorsSelecting.Count == 2)
                m_LastUp = grabInteractable.transform.up;
        }

        /// <inheritdoc />
        public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                {
                    UpdateTarget(grabInteractable, ref targetPose);

                    break;
                }
            }
        }

        void UpdateTarget(XRGrabInteractable grabInteractable, ref Pose targetPose)
        {
            if (grabInteractable.interactorsSelecting.Count == 1)
                XRSingleGrabFreeTransformer.UpdateTarget(grabInteractable, ref targetPose);
            else
                UpdateTargetMulti(grabInteractable, ref targetPose);
        }

        void UpdateTargetMulti(XRGrabInteractable grabInteractable, ref Pose targetPose)
        {
            Debug.Assert(grabInteractable.interactorsSelecting.Count > 1, this);

            var primaryAttachPose = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).GetWorldPose();
            var secondaryAttachPose = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable).GetWorldPose();

            // When multi-selecting, adjust the effective interactorAttachPose with our default 2-hand algorithm.
            // Default to the primary interactor.
            var interactorAttachPose = primaryAttachPose;

            switch (m_MultiSelectPosition)
            {
                case PoseContributor.First:
                    interactorAttachPose.position = primaryAttachPose.position;
                    break;
                case PoseContributor.Second:
                    interactorAttachPose.position = secondaryAttachPose.position;
                    break;
                case PoseContributor.Average:
                    interactorAttachPose.position = (primaryAttachPose.position + secondaryAttachPose.position) * 0.5f;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(PoseContributor)}={m_MultiSelectPosition}.");
                    goto case PoseContributor.First;
            }

            // For rotation, we match the anchor's forward to the vector made by the two interactor positions - imagine a hammer handle.
            // We use the interactor's up as the base of the combined multi-select up, unless it is too similar to the forward vector
            // In that case, we will gradually fall back to the right vector and calculate the final 'up' from that
            var forward = (secondaryAttachPose.position - primaryAttachPose.position).normalized;

            Vector3 up;
            Vector3 right;
            switch (m_MultiSelectRotation)
            {
                case PoseContributor.First:
                    up = primaryAttachPose.up;
                    right = primaryAttachPose.right;
                    if (forward == Vector3.zero)
                        forward = primaryAttachPose.forward;
                    break;
                case PoseContributor.Second:
                    up = secondaryAttachPose.up;
                    right = secondaryAttachPose.right;
                    if (forward == Vector3.zero)
                        forward = secondaryAttachPose.forward;
                    break;
                case PoseContributor.Average:
                    up = Vector3.Slerp(primaryAttachPose.up, secondaryAttachPose.up, 0.5f);
                    right = Vector3.Slerp(primaryAttachPose.right, secondaryAttachPose.right, 0.5f);
                    if (forward == Vector3.zero)
                        forward = primaryAttachPose.forward;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(PoseContributor)}={m_MultiSelectRotation}.");
                    goto case PoseContributor.First;
            }

            var crossUp = Vector3.Cross(forward, right);

            var angleDiff = Mathf.PingPong(Vector3.Angle(up, forward), 90f);
            up = Vector3.Slerp(crossUp, up, angleDiff / 90f);

            var crossRight = Vector3.Cross(up, forward);
            up = Vector3.Cross(forward, crossRight);

            // We also keep track of whether the up vector was pointing up or down previously, to allow for objects to be flipped through a series of rotations
            // Such as a 180 degree rotation on the y, followed by a 180 degree rotation on the x
            if (Vector3.Dot(up, m_LastUp) <= 0f)
            {
                up = -up;
            }

            m_LastUp = up;

            interactorAttachPose.rotation = Quaternion.LookRotation(forward, up);

            lastInteractorAttachPose = interactorAttachPose;

            if (!grabInteractable.trackRotation)
            {
                // When not using the rotation of the Interactor we apply the position without an offset
                targetPose.position = interactorAttachPose.position;
                return;
            }

            // Compute the new target world pose
            if (m_MultiSelectRotation == PoseContributor.First || m_MultiSelectRotation == PoseContributor.Second)
            {
                var controllerIndex = m_MultiSelectRotation == PoseContributor.First ? 0 : 1;
                var thisAttachTransform = grabInteractable.GetAttachTransform(grabInteractable.interactorsSelecting[controllerIndex]);
                var thisTransformPose = grabInteractable.transform.GetWorldPose();

                // Calculate offset of the grab interactable's position relative to its attach transform.
                // Transform that offset direction from world space to local space of the transform it's relative to.
                // It will be applied to the interactor's attach position using the orientation of the Interactor's attach transform.
                var attachOffset = thisTransformPose.position - thisAttachTransform.position;
                var positionOffset = thisAttachTransform.InverseTransformDirection(attachOffset);
                targetPose.position = (interactorAttachPose.rotation * positionOffset) + interactorAttachPose.position;
            }
            else if (m_MultiSelectRotation == PoseContributor.Average)
            {
                // Average rotation does not use offset and keeps objects between two attach points (controllers).
                targetPose.position = interactorAttachPose.position;
            }
            else
            {
                Assert.IsTrue(false, $"Unhandled {nameof(PoseContributor)}={m_MultiSelectRotation}.");
            }

            targetPose.rotation = interactorAttachPose.rotation;
        }
    }
}
