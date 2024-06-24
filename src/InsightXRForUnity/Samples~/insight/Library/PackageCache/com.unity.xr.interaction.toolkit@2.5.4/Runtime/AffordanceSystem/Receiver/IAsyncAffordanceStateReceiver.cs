using Unity.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver
{
    /// <summary>
    /// An interface that represents an affordance receiver that generates asynchronous tween jobs to be scheduled
    /// with the job system, then updates the affordance state according to the tween job output.
    /// </summary>
    /// <seealso cref="ISynchronousAffordanceStateReceiver"/>
    /// <seealso cref="BaseAsyncAffordanceStateReceiver{T}"/>
    public interface IAsyncAffordanceStateReceiver : IAffordanceStateReceiver
    {
        /// <summary>
        /// Called to generate and schedule a tween job with a given tween target.
        /// </summary>
        /// <param name="tweenTarget">Tween target parameter for the tween job.
        /// Used as a parameter in the theme's animation curve to find the value between the 0-1 animation state in the associated theme.</param>
        /// <returns>Returns <c>JobHandle</c> used by affordance state provider.</returns>
        JobHandle HandleTween(float tweenTarget);

        /// <summary>
        /// Read the affordance value written by the last completed affordance job and propagate that to listeners.
        /// Called by the affordance state provider to propagate the tween job's output.
        /// </summary>
        void UpdateStateFromCompletedJob();
    }
}