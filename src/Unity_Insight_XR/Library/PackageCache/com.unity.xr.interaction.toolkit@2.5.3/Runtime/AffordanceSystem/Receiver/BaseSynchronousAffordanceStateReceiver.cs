using System;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver
{
    /// <summary>
    /// Base implementation of a synchronous affordance state receiver to be used with affordance types
    /// that might not be possible to tween using the job system.
    /// </summary>
    /// <typeparam name="T">The type of the value struct.</typeparam>
    /// <seealso cref="BaseAsyncAffordanceStateReceiver{T}"/>
    public abstract class BaseSynchronousAffordanceStateReceiver<T> : BaseAffordanceStateReceiver<T>, ISynchronousAffordanceStateReceiver where T : struct, IEquatable<T>
    {
        /// <inheritdoc />
        public virtual void HandleTween(float tweenTarget)
        {
            CaptureInitialValue();
            
            var stateData = currentAffordanceStateData.Value;

            // Grab affordance theme data matching the target state index.
            var themeData = affordanceTheme.GetAffordanceThemeDataForIndex(stateData.stateIndex);
            if (themeData == null)
            {
                // If we cannot process the desired state index, return
                var stateName = AffordanceStateShortcuts.GetNameForIndex(stateData.stateIndex);
                XRLoggingUtils.LogError($"Missing theme data for affordance state index {stateData.stateIndex} \"{stateName}\" with {this}.", this);
                return;
            }

            // Evaluate the state transition amount according to the theme's animation curve.
            var curveAdjustedTarget = affordanceTheme.animationCurve.Evaluate(stateData.stateTransitionAmountFloat);

            // Determine if we should use the initial value as the target.
            bool useInitialValueAsTarget = replaceIdleStateValueWithInitialValue && stateData.stateIndex == AffordanceStateShortcuts.idle;
            
            // Determine a new target value using the curve adjusted transition target.
            T targetValue = useInitialValueAsTarget ? initialValue : Interpolate(themeData.animationStateStartValue, themeData.animationStateEndValue, curveAdjustedTarget);

            // Add processing to target value before using it.
            T processedTargetValue = ProcessTargetAffordanceValue(targetValue);

            // Compute the new affordance affordance value based on the current value and the newly computed target, along with the tween target parameter.
            T newAffordanceValue = Interpolate(currentAffordanceValue.Value, processedTargetValue, tweenTarget);

            // Update the affordance state with the new affordance value.
            ConsumeAffordance(newAffordanceValue);
        }

        /// <summary>
        /// Function used to interpolate between a tween's start value and target value.
        /// </summary>
        /// <param name="startValue">Tween start value.</param>
        /// <param name="targetValue">Tween target value.</param>
        /// <param name="interpolationAmount">Interpolation parameter value between 0-1 used to evaluate the output between the start and target values.</param>
        /// <returns>Returns the interpolation from <paramref name="startValue"/> to <paramref name="targetValue"/>.</returns>
        protected abstract T Interpolate(T startValue, T targetValue, float interpolationAmount);
    }
}