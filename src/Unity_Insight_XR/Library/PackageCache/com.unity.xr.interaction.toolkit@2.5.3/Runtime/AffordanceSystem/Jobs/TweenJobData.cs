using Unity.Collections;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs
{
    /// <summary>
    /// Struct holding all data needed to compute a tween job.
    /// </summary>
    /// <typeparam name="T">Struct type of tween output.</typeparam>
    public struct TweenJobData<T> where T : struct
    {
        /// <summary>
        /// Square distance value used to evaluate if the tween should just snap to the target value.
        /// </summary>
        public const float squareSnapDistanceThreshold = 0.0005f * 0.0005f;

        /// <summary>
        /// Total number of supported increments for the affordance state transition amount float conversion.
        /// </summary>
        /// <seealso cref="AffordanceStateData.totalStateTransitionIncrements"/>
        public const byte totalStateTransitionIncrements = AffordanceStateData.totalStateTransitionIncrements;

        /// <summary>
        /// Initial value for variable or receiver being tweened.
        /// </summary>
        public T initialValue;

        /// <summary>
        /// Affordance state lower bound. Used with <see cref="stateTransitionAmountFloat"/> and <see cref="nativeCurve"/> to find tween target.
        /// </summary>
        public T stateOriginValue;

        /// <summary>
        /// Affordance state upper bound. Used with <see cref="stateTransitionAmountFloat"/> and <see cref="nativeCurve"/> to find tween target. 
        /// </summary>
        public T stateTargetValue;

        /// <summary>
        /// State transition amount represented as a byte. Converted to float by dividing over <see cref="totalStateTransitionIncrements"/>.
        /// </summary>
        /// <seealso cref="AffordanceStateData.stateTransitionIncrement"/>
        public byte stateTransitionIncrement;

        /// <summary>
        /// 0-1 Float representation of <see cref="stateTransitionIncrement"/>.
        /// </summary>
        /// <seealso cref="AffordanceStateData.stateTransitionAmountFloat"/>
        public float stateTransitionAmountFloat => (float)stateTransitionIncrement / totalStateTransitionIncrements;

        /// <summary>
        /// Native curve used to evaluate the tweens using the <see cref="stateOriginValue"/>, <see cref="stateTargetValue"/>, and <see cref="stateTransitionAmountFloat"/>.
        /// </summary>   
        public NativeCurve nativeCurve;

        /// <summary>
        /// Tween starting value. Used with computed tween target by evaluating the <see cref="stateTransitionAmountFloat"/> between origin and target values.
        /// </summary>
        public T tweenStartValue;

        /// <summary>
        /// Interpolation value between 0-1 used to interpolate between the tween start value and the computed target value.
        /// </summary>
        public float tweenAmount;

        /// <summary>
        /// Native array with 1 value used to store the tween output.
        /// </summary>
        public NativeArray<T> outputData;
    }
}