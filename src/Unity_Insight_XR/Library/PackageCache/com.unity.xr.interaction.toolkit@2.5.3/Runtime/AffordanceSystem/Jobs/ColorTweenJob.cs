#if BURST_PRESENT
using Unity.Burst;
#endif
using JetBrains.Annotations;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs
{
    /// <summary>
    /// Tween job implementation for tweening Color values.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public struct ColorTweenJob : ITweenJob<Color>
    {
        /// <inheritdoc/>
        public TweenJobData<Color> jobData { get; set; }
        
        /// <summary>
        /// Color BlendMode enum represented as a byte
        /// </summary>
        public byte colorBlendMode { get; set; }
  
        /// <summary>
        /// Value between 0-1 used when determining how much to apply the blend processing.
        /// </summary>
        public float colorBlendAmount { get; set; }

        /// <summary>
        /// Perform work on a worker thread.
        /// </summary>
        /// <seealso cref="IJob.Execute"/>
        public void Execute()
        {
            var stateTransitionAmount = jobData.nativeCurve.Evaluate(jobData.stateTransitionAmountFloat);
            var newTargetValue = Lerp(jobData.stateOriginValue, jobData.stateTargetValue, stateTransitionAmount);
            var processedTargetValue = ProcessTargetAffordanceValue(jobData.initialValue, newTargetValue);

            var outputData = jobData.outputData;
            outputData[0] = Lerp(jobData.tweenStartValue, processedTargetValue, jobData.tweenAmount);
        }
        
        Color ProcessTargetAffordanceValue(Color initialValue, Color newValue)
        {
            Color blendedColor = newValue;
            switch (colorBlendMode)
            {
                // Solid
                case 0:
                    break;
                // Add
                case 1:
                    float blendAmt = colorBlendAmount;
                    blendedColor = new Color(initialValue.r + newValue.r * blendAmt, initialValue.g + newValue.g * blendAmt, initialValue.b + newValue.b * blendAmt, initialValue.a + newValue.a * blendAmt);
                    break;
                // Mix
                case 2:
                    blendedColor = Lerp(initialValue, newValue, colorBlendAmount);
                    break;
            }
            return blendedColor;
        }

        /// <inheritdoc/>
        public Color Lerp(Color from, Color to, float t)
        {
            if (IsNearlyEqual(from, to) )
            {
                return to;
            }

            return (Vector4)math.lerp((Vector4)from, (Vector4)to, t);
        }

        /// <inheritdoc/>
        public bool IsNearlyEqual(Color from, Color to)
        {
            return math.distancesq((Vector4)from, (Vector4)to) < TweenJobData<Color>.squareSnapDistanceThreshold;
        }
    }
}