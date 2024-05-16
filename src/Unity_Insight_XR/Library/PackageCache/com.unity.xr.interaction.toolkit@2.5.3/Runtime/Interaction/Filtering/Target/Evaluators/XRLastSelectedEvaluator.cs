using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// The last selected target Interactable will receive the highest normalized score.
    /// In the moment that an Interactable is selected by any of the linked Interactors, it'll have the highest normalized
    /// score of <c>1</c>. Its normalized score will linearly decrease with time until the score reaches <c>0</c> after
    /// <see cref="maxTime"/> seconds.
    /// </summary>
    [Serializable]
    public class XRLastSelectedEvaluator : XRTargetEvaluator, IXRTargetEvaluatorLinkable
    {
        readonly Dictionary<IXRInteractable, float> m_InteractableSelectionTimeMap =
            new Dictionary<IXRInteractable, float>();

        [Tooltip("Any Interactable which was last selected over Max Time seconds ago will receive a normalized score of 0.")]
        [SerializeField]
        float m_MaxTime = 10f;

        /// <summary>
        /// Any Interactable which was last selected over Max Time seconds ago will receive a normalized score of <c>0</c>.
        /// </summary>
        public float maxTime
        {
            get => m_MaxTime;
            set => m_MaxTime = value;
        }

        void OnSelect(SelectEnterEventArgs args)
        {
            if (enabled && args.interactableObject is IXRInteractable interactable)
                m_InteractableSelectionTimeMap[interactable] = Time.time;
        }

        /// <inheritdoc />
        public virtual void OnLink(IXRInteractor interactor)
        {
            if (interactor is IXRSelectInteractor selectInteractor)
                selectInteractor.selectEntered.AddListener(OnSelect);
        }

        /// <inheritdoc />
        public virtual void OnUnlink(IXRInteractor interactor)
        {
            if (interactor is IXRSelectInteractor selectInteractor)
                selectInteractor.selectEntered.RemoveListener(OnSelect);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            m_InteractableSelectionTimeMap.Clear();
        }

        /// <inheritdoc />
        protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
        {
            // We return .5 as the lowest value - zeroing out the score will flatten out the value, messing with other evaluators
            if (!m_InteractableSelectionTimeMap.TryGetValue(target, out var time) || m_MaxTime <= 0f)
                return 0.5f;

            return (1f - Mathf.Clamp01((Time.time - time) / m_MaxTime)) * 0.5f + 0.5f;
        }
    }
}
