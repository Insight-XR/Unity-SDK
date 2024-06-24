using Unity.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives
{
    /// <summary>
    /// Bindable variable that can tween over time towards a target color value.
    /// Uses an async implementation to tween using the job system.
    /// </summary>
    public class ColorTweenableVariable : TweenableVariableAsyncBase<Color>
    {
        /// <summary>
        /// Blend mode used by the color affordance receiver when applying the new color.
        /// </summary>
        ColorBlendMode colorBlendMode { get; set; } = ColorBlendMode.Solid;

        /// <summary>
        /// Value between 0 and 1 used to compute color blend modes.
        /// </summary>
        float colorBlendAmount { get; set; } = 1f;
        
        /// <inheritdoc />
        protected override JobHandle ScheduleTweenJob(ref TweenJobData<Color> jobData)
        {
            var job = new ColorTweenJob
            {
                jobData = jobData,
                colorBlendAmount = colorBlendAmount,
                colorBlendMode = (byte)colorBlendMode
            };
            return job.Schedule();
        }
    }
}