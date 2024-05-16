using System;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Evaluates the Interactor distance from the target Interactable.
    /// Targets close to the Interactor will receive a highest score and targets far way will receive a lower score.
    /// </summary>
    [Serializable]
    public class XRDistanceEvaluator : XRTargetEvaluator
    {
        [Tooltip("The maximum distance from the Interactor. Any target from this distance will receive a 0 normalized score.")]
        [SerializeField]
        float m_MaxDistance = 1f;

        /// <summary>
        /// The maximum distance from the Interactor.
        /// Any target from this distance will receive a <c>0</c> normalized score.
        /// </summary>
        public float maxDistance
        {
            get => m_MaxDistance;
            set => m_MaxDistance = value;
        }

        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();
            weight = new AnimationCurve(new Keyframe(0, 0, 0, 0.5f), new Keyframe(1, 1, 2, 2));
        }

        /// <inheritdoc />
        /// <remarks>
        /// This is similar to the implementation of the default algorithm to get valid targets in <see cref="XRDirectInteractor"/>.
        /// </remarks>
        protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
        {
            if (Mathf.Approximately(m_MaxDistance, 0f))
                return 0f;

            using (new XRInteractableUtility.AllowTriggerCollidersScope(true))
            {
                var baseInteractor = interactor as XRBaseInteractor;
                float distanceSqr;
                if (target is XRBaseInteractable baseInteractable && baseInteractor != null)
                {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                    distanceSqr = baseInteractable.GetDistanceSqrToInteractor(baseInteractor);
#pragma warning restore 618
                }
                else
                {
                    distanceSqr = target.GetDistanceSqrToInteractor(interactor);
                }

                return 1f - Mathf.Clamp01(distanceSqr / (m_MaxDistance * m_MaxDistance));
            }
        }
    }
}
