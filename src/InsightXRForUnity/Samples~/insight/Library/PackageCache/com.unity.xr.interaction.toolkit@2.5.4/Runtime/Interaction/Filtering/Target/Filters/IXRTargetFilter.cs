using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// An interface responsible to filter a list of Interactable targets (candidates) for interaction with a linked Interactor.
    /// </summary>
    /// <remarks>
    /// An Interactor and an implementation of this interface are linked when a call to <see cref="Link"/> happens and
    /// they are unlinked when a call to <see cref="Unlink"/> happens. A linked Interactor can forward its Interactable
    /// target filtering logic to this interface implementation by calling <see cref="Process"/>.
    /// <br />
    /// An <see cref="XRBaseInteractor"/> and a Target Filter can be linked when an implementation of this interface
    /// is assigned to <see cref="XRBaseInteractor.targetFilter"/>.
    /// <br />
    /// It's possible to have multiple Interactors linked to the same Target Filter.
    /// </remarks>
    /// <seealso cref="XRBaseTargetFilter"/>
    /// <seealso cref="XRTargetFilter"/>
    /// <seealso cref="IXRInteractor.GetValidTargets"/>
    public interface IXRTargetFilter
    {
        /// <summary>
        /// Whether this Target Filter can process and filter targets.
        /// Filters that can process targets receive calls to <see cref="Process"/>, filters that cannot process do not.
        /// </summary>
        bool canProcess { get; }

        /// <summary>
        /// Called by Unity when the given Interactor links to this filter.
        /// Use this to do any code initialization for the given Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor being linked to this filter.</param>
        void Link(IXRInteractor interactor);

        /// <summary>
        /// Called by Unity when the given Interactor unlinks from this filter.
        /// Use this to do any code cleanup for the given Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor being unlinked from this filter.</param>
        void Unlink(IXRInteractor interactor);

        /// <summary>
        /// Called by the linked Interactor to filter the Interactables that it could possibly interact with this frame.
        /// Implement your custom logic to filter the Interactable candidates in this method.
        /// </summary>
        /// <param name="interactor">The linked Interactor whose Interactable candidates (or targets) are being filtered.</param>
        /// <param name="targets">The read only list of candidate Interactables to filter. This list should not be modified.</param>
        /// <param name="results">The results list to populate with the filtered results. This list should be sorted by priority (with highest priority first).</param>
        /// <remarks>
        /// It's recommended to call this from an implementation of <see cref="IXRInteractor.GetValidTargets"/>.
        /// </remarks>>
        void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results);
    }
}
