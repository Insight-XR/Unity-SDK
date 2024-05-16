using Unity.XR.CoreUtils.Bindings.Variables;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents an Interactable component which
    /// can express an interaction strength amount, which is a normalized value <c>[0.0, 1.0]</c>
    /// that describes the strength of selection.
    /// </summary>
    /// <remarks>
    /// For interactors that use motion controller input, this is typically based on the analog trigger or grip press amount.
    /// It can also be based on a poke amount for how deep a poke interactor has pressed into an interactable.
    /// </remarks>
    /// <seealso cref="IXRInteractionStrengthInteractor"/>
    public interface IXRInteractionStrengthInteractable : IXRInteractable
    {
        /// <summary>
        /// The largest interaction strength value of all interactors hovering or selecting this interactable.
        /// </summary>
        IReadOnlyBindableVariable<float> largestInteractionStrength { get; }

        /// <summary>
        /// Gets the interaction strength between the given interactor and this interactable.
        /// </summary>
        /// <param name="interactor">The specific interactor to get the interaction strength between.</param>
        /// <returns>Returns a value <c>[0.0, 1.0]</c> of the interaction strength.</returns>
        float GetInteractionStrength(IXRInteractor interactor);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method to signal to update the interaction strength.
        /// </summary>
        /// <param name="updatePhase">The update phase during which this method is called.</param>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase);
    }
}
