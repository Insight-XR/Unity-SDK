namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives
{
    /// <summary>
    /// Bindable variable that can tween over time towards a target Quaternion value.
    /// Uses an synchronous implementation so the tween does not use the job system.
    /// </summary>
    public class QuaternionTweenableVariable : TweenableVariableSynchronousBase<Quaternion>
    {
        /// <summary>
        /// Angle threshold in degrees, under which two quaternions are considered equal.
        /// </summary>
        public float angleEqualityThreshold { get; set; } = 0.01f;

        /// <inheritdoc />
        protected override Quaternion Lerp(Quaternion from, Quaternion to, float t)
        {
            return Quaternion.Slerp(from, to, t);
        }

        /// <inheritdoc />
        protected override bool IsNearlyEqual(Quaternion startValue, Quaternion targetValue)
        {
            return Quaternion.Angle(startValue, targetValue) < angleEqualityThreshold;
        }
    }
}