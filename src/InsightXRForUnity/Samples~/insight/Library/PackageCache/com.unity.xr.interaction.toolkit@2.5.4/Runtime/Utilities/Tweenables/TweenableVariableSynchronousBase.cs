using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables
{
    /// <summary>
    /// Synchronous implementation of tweenable variable used for types for which it may not be possible to create tween jobs.
    /// </summary>
    /// <typeparam name="T">BindableVariable type.</typeparam>
    /// <seealso cref="TweenableVariableAsyncBase{T}"/>
    public abstract class TweenableVariableSynchronousBase<T> : TweenableVariableBase<T> where T : IEquatable<T>
    {
        /// <inheritdoc />
        protected override void ExecuteTween(T startValue, T targetValue, float tweenAmount, bool useCurve = false)
        {
            if (tweenAmount > k_NearlyOne || IsNearlyEqual(startValue, targetValue))
            {
                Value = targetValue;
                return;
            }

            var adjustedTweenAmount = useCurve ? animationCurve.Evaluate(tweenAmount) : tweenAmount;

            Value = Lerp(startValue, targetValue, adjustedTweenAmount);
        }

        /// <summary>
        /// Function used to interpolate between a tween's start value and target value.
        /// </summary>
        /// <param name="from">Tween start value.</param>
        /// <param name="to">Tween target value.</param>
        /// <param name="t">Value between 0-1 used to evaluate the output between the from and to values.</param>
        /// <returns>Returns the interpolation from <paramref name="from"/> to <paramref name="to"/>.</returns>
        protected abstract T Lerp(T from, T to, float t);

        /// <summary>
        /// Evaluates if the value is nearly equal to target.
        /// </summary>
        /// <param name="startValue">First value in equality comparison.</param>
        /// <param name="targetValue">Second value in equality comparison.</param>
        /// <returns>Returns <see langword="true"/> if the values are nearly equal.</returns>
        protected abstract bool IsNearlyEqual(T startValue, T targetValue);
    }
}