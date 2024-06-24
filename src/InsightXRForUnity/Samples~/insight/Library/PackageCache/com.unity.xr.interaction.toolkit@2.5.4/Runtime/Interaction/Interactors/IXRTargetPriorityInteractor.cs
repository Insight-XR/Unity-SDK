using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Options for how many Targets (or Interactables with priority for selection) to monitor.
    /// </summary>
    /// <remarks>
    /// The options are in order of best performance.
    /// </remarks>
    /// <seealso cref="IXRTargetPriorityInteractor"/>
    public enum TargetPriorityMode
    {
        /// <summary>
        /// Monitors no Target, the <see cref="IXRTargetPriorityInteractor.targetsForSelection"/> list will not be updated.
        /// This option has very low performance cost.
        /// </summary>
        None,

        /// <summary>
        /// Only monitors the highest priority Target, the <see cref="IXRTargetPriorityInteractor.targetsForSelection"/> list
        /// will only be updated with the highest priority Target, or it'll be empty if there is no Target that can be
        /// selected in the current frame.
        /// This option has moderate performance cost.
        /// </summary>
        HighestPriorityOnly,

        /// <summary>
        /// Tracks all Targets, the <see cref="IXRTargetPriorityInteractor.targetsForSelection"/> list will be updated with
        /// all Interactables that can be selected in the current frame.
        /// This option has high performance cost.
        /// </summary>
        All,
    }

    /// <summary>
    /// An interface that represents an Interactor component that monitors the Interactables with priority for selection
    /// in a frame (called Targets), useful for custom feedback.
    /// </summary>
    /// <seealso cref="XRInteractionManager.InteractorSelectValidTargets(IXRSelectInteractor, List{IXRInteractable})"/>
    /// <seealso cref="XRInteractionManager.IsHighestPriorityTarget"/>
    /// <seealso cref="IXRInteractor.GetValidTargets"/>
    public interface IXRTargetPriorityInteractor : IXRInteractor
    {
        /// <summary>
        /// Specifies how many Interactables should be monitored in the <see cref="targetsForSelection"/>
        /// property.
        /// </summary>
        TargetPriorityMode targetPriorityMode { get; }

        /// <summary>
        /// The Interactables with priority for selection in the current frame, some Interactables might be already selected.
        /// This list is sorted by priority (with highest priority first).
        /// How many Interactables appear in this list is configured by the <see cref="targetPriorityMode"/> property.
        /// </summary>
        /// <remarks>
        /// Unity automatically clears and updates this list every frame if <see cref="targetPriorityMode"/> has a
        /// value different from <see cref="TargetPriorityMode.None"/>, in this case a valid list must be returned.
        /// </remarks>
        List<IXRSelectInteractable> targetsForSelection { get; }
    }
}
