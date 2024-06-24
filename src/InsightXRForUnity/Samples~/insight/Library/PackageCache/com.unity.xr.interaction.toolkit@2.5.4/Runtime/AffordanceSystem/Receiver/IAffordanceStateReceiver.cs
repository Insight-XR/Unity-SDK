using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver
{
    /// <summary>
    /// An interface that represents the core capabilities of an affordance receiver.
    /// Its job is to receive updates from an affordance state provider and generate tween jobs to be scheduled,
    /// then update the affordance state according to the tween job output.
    /// </summary>
    /// <seealso cref="IAffordanceStateReceiver{T}"/>
    /// <seealso cref="IAsyncAffordanceStateReceiver"/>
    /// <seealso cref="ISynchronousAffordanceStateReceiver"/>
    public interface IAffordanceStateReceiver
    {
        /// <summary>
        /// Bindable variable holding the last affordance state passed in by the affordance state provider.
        /// </summary>
        IReadOnlyBindableVariable<AffordanceStateData> currentAffordanceStateData { get; }

        /// <summary>
        /// Called by the affordance state provider to inform the receiver of the previous state and new state.
        /// </summary>
        /// <param name="previousState">Previous affordance state.</param>
        /// <param name="newState">New Affordance state.</param>
        void OnAffordanceStateUpdated(AffordanceStateData previousState, AffordanceStateData newState);
    }

    /// <summary>
    /// Typed interface for affordance state receivers used to expose the typed functions and properties necessary
    /// for an affordance state receiver to work.
    /// </summary>
    /// <typeparam name="T">The type of the value struct.</typeparam>
    public interface IAffordanceStateReceiver<T> : IAffordanceStateReceiver where T : struct
    {
        /// <summary>
        /// Affordance theme, used to map affordance state to a typed affordance value.
        /// </summary>
        BaseAffordanceTheme<T> affordanceTheme { get; }

        /// <summary>
        /// Bindable variable for current typed affordance value. Updated as scheduled tween jobs complete.
        /// </summary>
        IReadOnlyBindableVariable<T> currentAffordanceValue { get; }
    }
}