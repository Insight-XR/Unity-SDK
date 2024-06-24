namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// An interface that an <see cref="XRTargetFilter"/> can implement to receive calls whenever an Interactor links to or
    /// unlinks from its filter.
    /// <br />
    /// Implement this interface if the evaluator needs to subscribe to events or cache data from the linked Interactors.
    /// </summary>
    /// <seealso cref="XRLastSelectedEvaluator"/>
    /// <seealso cref="XRTargetEvaluator"/>
    /// <seealso cref="XRTargetFilter"/>
    public interface IXRTargetEvaluatorLinkable
    {
        /// <summary>
        /// Called by the Target Filter when it links to the given Interactor.
        /// This is also called after the evaluator's <see cref="XRTargetEvaluator.Awake"/> for each already linked Interactor.
        /// <br />
        /// Use this only for code initialization for the given Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor being linked to the filter.</param>
        /// <remarks>
        /// This is called even if the evaluator is disabled. You can check if the evaluator is enabled using the
        /// <see cref="XRTargetEvaluator.enabled"/> property.
        /// <br />
        /// You should not update the linked interactor list or the evaluator list in the filter, nor should you
        /// disable or enable evaluators. This can lead to out-of-order calls to <see cref="OnLink"/> and <see cref="OnUnlink"/>.
        /// </remarks>
        /// <seealso cref="OnUnlink"/>
        /// <seealso cref="IXRTargetFilter.Link"/>
        void OnLink(IXRInteractor interactor);

        /// <summary>
        /// Called by the Target Filter when it unlinks from the given Interactor.
        /// This is also called before the evaluator's <see cref="XRTargetEvaluator.OnDispose"/> for each linked Interactor.
        /// <br />
        /// Use this for any code cleanup for the given Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor being unlinked from this filter.</param>
        /// <remarks>
        /// This is called even if the evaluator is disabled. You can check if the evaluator is enabled using the
        /// <see cref="XRTargetEvaluator.enabled"/> property.
        /// <br />
        /// You should not update the linked interactor list or the evaluator list in the filter, nor should you
        /// disable or enable evaluators. This can lead to out-of-order calls to <see cref="OnLink"/> and <see cref="OnUnlink"/>.
        /// </remarks>
        /// <seealso cref="OnLink"/>
        /// <seealso cref="IXRTargetFilter.Unlink"/>
        void OnUnlink(IXRInteractor interactor);
    }
}
