#if BURST_PRESENT
using Unity.Burst;
#endif
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables
{
    /// <summary>
    /// This class expands on the Quaternion tweenable variable to introduce two concepts:
    /// <list type="bullet">
    /// <item>
    /// <description>A dynamic threshold angle that grows over time in a range,
    /// that prevents updating the target so long as the value being assigned to the target is within that threshold.
    /// </description>
    /// </item>
    /// <item>
    /// <description>A variable speed tween (<see cref="HandleSmartTween"/>) that inputs a lower and upper range speed for tweening.
    /// The closer the value is to the target, the faster the tween.
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    /// <seealso cref="SmartFollowVector3TweenableVariable"/>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public class SmartFollowQuaternionTweenableVariable : QuaternionTweenableVariable
    {
        /// <summary>
        /// Minimum angle offset allowed in degrees. 
        /// </summary>
        public float minAngleAllowed { get; set; }
        
        /// <summary>
        /// Maximum angle offset allowed in degrees.
        /// </summary>
        public float maxAngleAllowed { get; set; }
        
        /// <summary>
        /// Time required to elapse before the max angle offset allowed goes from the min angle to the max.
        /// </summary>
        public float minToMaxDelaySeconds { get; set; }

        float m_LastUpdateTime;

        /// <summary>
        /// Constructor for SmartFollowQuaternionTweenableVariable.
        /// </summary>
        /// <param name="minAngleAllowed">Minimum angle offset (in degrees) from target before which tween starts.</param>
        /// <param name="maxAngleAllowed">Maximum angle offset (in degrees) from target before tween targets, when time threshold is reached.</param>
        /// <param name="minToMaxDelaySeconds">Time required to elapse (in seconds) before the max angle offset allowed goes from the min angle offset to the max.</param>
        public SmartFollowQuaternionTweenableVariable(
            float minAngleAllowed = 0.1f,
            float maxAngleAllowed = 5,
            float minToMaxDelaySeconds = 3f)
        {
            this.minAngleAllowed = minAngleAllowed;
            this.maxAngleAllowed = maxAngleAllowed;
            this.minToMaxDelaySeconds = minToMaxDelaySeconds;
        }

        /// <summary>
        /// Checks if the angle difference between the current target rotation and a new target rotation is within a dynamically determined threshold,
        /// based on the time since the last update.
        /// </summary>
        /// <param name="newTarget">The new target rotation as a Quaternion.</param>
        /// <returns>Returns <see langword="true"/> if the angle difference between the current and new targets is within the allowed threshold, <see langword="false"/> otherwise.</returns>
        public bool IsNewTargetWithinThreshold(Quaternion newTarget)
        {
            float newAngleTargetOffset = Quaternion.Angle(target, newTarget);
            float timeSinceLastUpdate = Time.unscaledTime - m_LastUpdateTime;

            // Widen tolerance zone over time 
            float allowedTargetAngleOffset = Mathf.Lerp(minAngleAllowed, maxAngleAllowed, Mathf.Clamp01(timeSinceLastUpdate / minToMaxDelaySeconds));
            return newAngleTargetOffset > allowedTargetAngleOffset;
        }

        /// <summary>
        /// Updates the target rotation to a new value if it is within a dynamically determined threshold,
        /// based on the time since the last update.
        /// </summary>
        /// <param name="newTarget">The new target rotation as a Quaternion.</param>
        /// <returns>Returns <see langword="true"/> if the target rotation is updated, <see langword="false"/> otherwise.</returns>
        public bool SetTargetWithinThreshold(Quaternion newTarget)
        {
            bool isWithinThreshold = IsNewTargetWithinThreshold(newTarget);
            if (isWithinThreshold)
                target = newTarget;
            return isWithinThreshold;
        }

        /// <inheritdoc />
        protected override void OnTargetChanged(Quaternion newTarget)
        {
            m_LastUpdateTime = Time.unscaledTime;
        }

        /// <summary>
        /// Tween to new target with variable speed according to distance from target.
        /// The closer the target is to the current value, the faster the tween.
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last tween.</param>
        /// <param name="lowerSpeed">Lower speed limit new tween target will clamp to.</param>
        /// <param name="upperSpeed">Upper speed limit new tween target will clamp to.</param>
        public void HandleSmartTween(float deltaTime, float lowerSpeed, float upperSpeed)
        {
            float angleOffsetDeg = Quaternion.Angle(target, Value);
            ComputeNewTweenTarget(deltaTime, angleOffsetDeg, maxAngleAllowed, lowerSpeed, upperSpeed, out float newTweenTarget);
            HandleTween(newTweenTarget);
        }
        
#if BURST_PRESENT
        [BurstCompile]
#endif
        static void ComputeNewTweenTarget(float deltaTime, float angleOffsetDeg, float maxAngleAllowed, float lowerSpeed, float upperSpeed, out float newTweenTarget)
        {
            float speedMultiplier = (1f - math.clamp(angleOffsetDeg / maxAngleAllowed, 0f, 1f));
            newTweenTarget = deltaTime * math.clamp(speedMultiplier * upperSpeed, lowerSpeed, upperSpeed);
        }
    }
}