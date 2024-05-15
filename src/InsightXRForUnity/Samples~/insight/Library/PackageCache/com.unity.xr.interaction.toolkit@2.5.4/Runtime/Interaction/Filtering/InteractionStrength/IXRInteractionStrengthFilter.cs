using System;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Instances that implement this interface are called interaction strength filters. Interaction strength filters
    /// are used to adjust or set the interaction strength between an Interactor and Interactable.
    /// </summary>
    /// <remarks>
    /// Add an interaction strength filter to the following objects to extend its interaction strength computation:
    /// <list type="bullet">
    /// <item>
    /// <description><see cref="XRBaseInteractable"/>: to add an Interactable interaction strength filter used to modify
    /// interaction strength in the Interactable for a hovering or selecting Interactor.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public interface IXRInteractionStrengthFilter
    {
        /// <summary>
        /// Whether this interaction strength filter can process.
        /// Interaction strength filters that can process receive calls to <see cref="Process"/>, interaction strength filters that
        /// cannot process do not.
        /// </summary>
        /// <remarks>
        /// It's recommended to return <see cref="Behaviour.isActiveAndEnabled"/> when implementing this interface
        /// in a <see cref="MonoBehaviour"/>.
        /// </remarks>
        bool canProcess { get; }

        /// <summary>
        /// Called by the host object (<see cref="XRBaseInteractable"/>) to calculate the interaction strength
        /// between the given Interactor and Interactable.
        /// </summary>
        /// <param name="interactor">The Interactor interacting.</param>
        /// <param name="interactable">The Interactable interacting with the interactor.</param>
        /// <param name="interactionStrength">The input interaction strength.</param>
        /// <returns>Returns the modified interaction strength that is the result of passing the interaction strength through the filter.</returns>
        float Process(IXRInteractor interactor, IXRInteractable interactable, float interactionStrength);
    }

    /// <summary>
    /// An interaction strength filter that forwards its processing to a delegate (<see cref="delegateToProcess"/>).
    /// Useful to create custom filters by code without needing to create new classes.
    /// </summary>
    /// <seealso cref="IXRInteractionStrengthFilter"/>
    public sealed class XRInteractionStrengthFilterDelegate : IXRInteractionStrengthFilter
    {
        /// <summary>
        /// The delegate to be invoked when processing this filter.
        /// </summary>
        public Func<IXRInteractor, IXRInteractable, float, float> delegateToProcess { get; set; }

        /// <inheritdoc />
        public bool canProcess { get; set; } = true;

        /// <summary>
        /// Creates a new interaction strength filter delegate.
        /// </summary>
        /// <param name="delegateToProcess">The delegate to be invoked when processing this filter.</param>
        public XRInteractionStrengthFilterDelegate(Func<IXRInteractor, IXRInteractable, float, float> delegateToProcess)
        {
            if (delegateToProcess == null)
                throw new ArgumentException(nameof(delegateToProcess));

            this.delegateToProcess = delegateToProcess;
        }

        /// <inheritdoc />
        public float Process(IXRInteractor interactor, IXRInteractable interactable, float interactionStrength)
        {
            return delegateToProcess.Invoke(interactor, interactable, interactionStrength);
        }
    }
}
