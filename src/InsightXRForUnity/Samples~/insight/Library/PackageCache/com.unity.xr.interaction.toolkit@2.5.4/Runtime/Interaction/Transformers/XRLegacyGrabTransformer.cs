using System;
using Unity.XR.CoreUtils;
using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Grab transformer used when <see cref="XRGrabInteractable.attachPointCompatibilityMode"/> is
    /// set to <see cref="XRGrabInteractable.AttachPointCompatibilityMode.Legacy"/>.
    /// This is for backwards compatibility purpose for old projects.
    /// Marked for deprecation, this component will be removed in a future version.
    /// </summary>
    /// <seealso cref="XRSingleGrabFreeTransformer"/>
    [AddComponentMenu("")]
    [HelpURL(XRHelpURLConstants.k_XRLegacyGrabTransformer)]
    [Obsolete("XRLegacyGrabTransformer has been deprecated, use XRSingleFreeGrabTransformer instead.")]
    public sealed class XRLegacyGrabTransformer : XRBaseGrabTransformer
    {
        Rigidbody m_Rigidbody;

        Pose m_OffsetPose;

        /// <inheritdoc />
        public override void OnLink(XRGrabInteractable grabInteractable)
        {
            base.OnLink(grabInteractable);
            m_Rigidbody = grabInteractable.GetComponent<Rigidbody>();
        }

        /// <inheritdoc />
        public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            base.OnGrabCountChanged(grabInteractable, targetPose, localScale);
            Assert.AreEqual(XRGrabInteractable.AttachPointCompatibilityMode.Legacy, grabInteractable.attachPointCompatibilityMode);

            if (grabInteractable.interactorsSelecting.Count == 1)
            {
                // For Legacy mode, this is the only time that the offsets are calculated while selected.
                m_OffsetPose = CalculateOffsetPoseLegacy(grabInteractable.interactorsSelecting[0], grabInteractable);
            }
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

        Pose CalculateOffsetPoseLegacy(IXRInteractor interactor, XRGrabInteractable grabInteractable)
        {
            var thisAttachTransform = grabInteractable.GetAttachTransform(interactor);
            var attachOffset = m_Rigidbody.worldCenterOfMass - thisAttachTransform.position;
            var localAttachOffset = thisAttachTransform.InverseTransformDirection(attachOffset);

            var inverseLocalScale = interactor.GetAttachTransform(grabInteractable).lossyScale;
            inverseLocalScale = new Vector3(1f / inverseLocalScale.x, 1f / inverseLocalScale.y, 1f / inverseLocalScale.z);
            localAttachOffset.Scale(inverseLocalScale);

            var offsetPosition = localAttachOffset;
            var offsetRotation = Quaternion.Inverse(Quaternion.Inverse(grabInteractable.transform.rotation) * thisAttachTransform.rotation);

            return new Pose(offsetPosition, offsetRotation);
        }

        void UpdateTarget(XRGrabInteractable grabInteractable, ref Pose targetPose)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            var interactorAttachPose = interactor.GetAttachTransform(grabInteractable).GetWorldPose();

            // Compute the new target world pose
            if (grabInteractable.trackRotation)
            {
                targetPose.position = interactorAttachPose.rotation * m_OffsetPose.position + interactorAttachPose.position;
                targetPose.rotation = interactorAttachPose.rotation * m_OffsetPose.rotation;
            }
            else
            {
                var thisAttachTransform = grabInteractable.GetAttachTransform(interactor);
                targetPose.position = thisAttachTransform.TransformDirection(m_OffsetPose.position) + interactorAttachPose.position;
            }
        }
    }
}
