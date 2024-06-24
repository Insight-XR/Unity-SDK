using Unity.XR.CoreUtils.Bindings.Variables;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an Interactor component which
    /// can express an interaction strength amount, which is a normalized value <c>[0.0, 1.0]</c>
    /// that describes the strength of selection.
    /// </summary>
    /// <remarks>
    /// For interactors that use motion controller input, this is typically based on the analog trigger or grip press amount.
    /// It can also be based on a poke amount for how deep a poke interactor has pressed into an interactable.
    /// </remarks>
    /// <seealso cref="IXRInteractionStrengthInteractable"/>
    public interface IXRInteractionStrengthInteractor : IXRInteractor
    {
        /// <summary>
        /// The largest interaction strength value of all interactables this interactor is hovering or selecting.
        /// </summary>
        IReadOnlyBindableVariable<float> largestInteractionStrength { get; }

        /// <summary>
        /// Gets the interaction strength between the given interactable and this interactor.
        /// </summary>
        /// <param name="interactable">The specific interactable to get the interaction strength between.</param>
        /// <returns>Returns a value <c>[0.0, 1.0]</c> of the interaction strength.</returns>
        float GetInteractionStrength(IXRInteractable interactable);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method to signal to update the interaction strength.
        /// </summary>
        /// <param name="updatePhase">The update phase during which this method is called.</param>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase);
    }
}
