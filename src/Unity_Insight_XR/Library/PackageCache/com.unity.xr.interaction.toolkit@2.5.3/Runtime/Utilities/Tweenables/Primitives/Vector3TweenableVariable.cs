using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives
{
    /// <summary>
    /// Bindable variable that can tween over time towards a target float3 (Vector3) value.
    /// Uses an async implementation to tween using the job system.
    /// </summary>
    public class Vector3TweenableVariable : TweenableVariableAsyncBase<float3>
    {
        /// <inheritdoc />
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<float3> jobData)
        {
            var job = new Float3TweenJob { jobData = jobData };
            return job.Schedule();
        }
    }
}