#if BURST_PRESENT
using Unity.Burst;
#endif
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs
{
    /// <summary>
    /// Tween job implementation for tweening Float values.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public struct FloatTweenJob : ITweenJob<float>
    {
        /// <inheritdoc/>
        public TweenJobData<float> jobData { get; set; }

        /// <summary>
        /// Perform work on a worker thread.
        /// </summary>
        /// <seealso cref="IJob.Execute"/>
        public void Execute()
        {
            var stateTransitionAmount = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
            var newTargetValue = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, stateTransitionAmount);

            var outputData = jobData.outputData;
            outputData[0] = Lerp(jobData.tweenStartValue, newTargetValue, jobData.tweenAmount);
        }

        /// <inheritdoc/>
        public float Lerp(float from, float to, float t)
        {
            if (IsNearlyEqual(from, to))
            {
                return to;
            }

            return math.lerp(from, to, t);
        }

        /// <inheritdoc/>
        public bool IsNearlyEqual(float from, float to)
        {
            return math.distancesq(from, to) < TweenJobData<float>.squareSnapDistanceThreshold;
        }
    }

    /// <summary>
    /// Tween job implementation for tweening float2 values.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public struct Float2TweenJob : ITweenJob<float2>
    {
        /// <inheritdoc/>
        public TweenJobData<float2> jobData { get; set; }

        /// <summary>
        /// Perform work on a worker thread.
        /// </summary>
        /// <seealso cref="IJob.Execute"/>
        public void Execute()
        {
            var stateTransitionAmount = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
            var newTargetValue = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, stateTransitionAmount);

            var outputData = jobData.outputData;
            outputData[0] = Lerp(jobData.tweenStartValue, newTargetValue, jobData.tweenAmount);
        }

        /// <inheritdoc/>
        public float2 Lerp(float2 from, float2 to, float t)
        {
            if (IsNearlyEqual(from, to))
            {
                return to;
            }

            return math.lerp(from, to, t);
        }

        /// <inheritdoc/>
        public bool IsNearlyEqual(float2 from, float2 to)
        {
            return math.distancesq(from, to) < TweenJobData<float2>.squareSnapDistanceThreshold;
        }
    }

    /// <summary>
    /// Tween job implementation for tweening float3 values.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public struct Float3TweenJob : ITweenJob<float3>
    {
        /// <inheritdoc/>
        public TweenJobData<float3> jobData { get; set; }

        /// <summary>
        /// Perform work on a worker thread.
        /// </summary>
        /// <seealso cref="IJob.Execute"/>
        public void Execute()
        {
            var stateTransitionAmount = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
            var newTargetValue = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, stateTransitionAmount);

            var outputData = jobData.outputData;
            outputData[0] = Lerp(jobData.tweenStartValue, newTargetValue, jobData.tweenAmount);
        }

        /// <inheritdoc/>
        public float3 Lerp(float3 from, float3 to, float t)
        {
            if (IsNearlyEqual(from, to))
            {
                return to;
            }

            return math.lerp(from, to, t);
        }

        /// <inheritdoc/>
        public bool IsNearlyEqual(float3 from, float3 to)
        {
            return math.distancesq(from, to) < TweenJobData<float3>.squareSnapDistanceThreshold;
        }
    }

    /// <summary>
    /// Tween job implementation for tweening float4 values.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public struct Float4TweenJob : ITweenJob<float4>
    {
        /// <inheritdoc/>
        public TweenJobData<float4> jobData { get; set; }

        /// <summary>
        /// Perform work on a worker thread.
        /// </summary>
        /// <seealso cref="IJob.Execute"/>
        public void Execute()
        {
            var stateTransitionAmount = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
            var newTargetValue = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, stateTransitionAmount);

            var outputData = jobData.outputData;
            outputData[0] = Lerp(jobData.tweenStartValue, newTargetValue, jobData.tweenAmount);
        }

        /// <inheritdoc/>
        public float4 Lerp(float4 from, float4 to, float t)
        {
            if (IsNearlyEqual(from, to))
            {
                return to;
            }

            return math.lerp(from, to, t);
        }

        /// <inheritdoc/>
        public bool IsNearlyEqual(float4 from, float4 to)
        {
            return math.distancesq(from, to) < TweenJobData<float4>.squareSnapDistanceThreshold;
        }
    }
}