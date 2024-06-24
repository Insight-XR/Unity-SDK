using Unity.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives
{
    /// <summary>
    /// Bindable variable that can tween over time towards a target float value.
    /// Uses an async implementation to tween using the job system.
    /// </summary>
    public class FloatTweenableVariable : TweenableVariableAsyncBase<float>
    {
        /// <inheritdoc />
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<float> jobData)
        {
            var job = new FloatTweenJob { jobData = jobData };
            return job.Schedule();
        }
    }
}