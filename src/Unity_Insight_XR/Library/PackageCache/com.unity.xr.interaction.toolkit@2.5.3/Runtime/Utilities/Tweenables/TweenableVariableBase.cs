using System;
using System.Collections;
using Unity.XR.CoreUtils.Bindings.Variables;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables
{
    /// <summary>
    /// Tweenable variable uses bindable variable and target value to tween over time towards a target value.
    /// </summary>
    /// <typeparam name="T">BindableVariable type.</typeparam>
    public abstract class TweenableVariableBase<T> : BindableVariable<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Threshold to compare tween amount above which the tween is short-circuited to the target value.
        /// </summary>
        protected const float k_NearlyOne = 0.99999f;

        AnimationCurve m_AnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Animation curve used for sequence animations.
        /// </summary>
        public AnimationCurve animationCurve
        {
            get => m_AnimationCurve;
            set
            {
                m_AnimationCurve = value;
                OnAnimationCurveChanged(value);
            }
        }

        T m_Target;

        /// <summary>
        /// Target value used when tweening variable value.
        /// </summary>
        /// <seealso cref="BindableVariableBase{T}.Value"/>
        public T target
        {
            get => m_Target;
            set
            {
                if (m_Target.Equals(value))
                    return;
                m_Target = value;
                OnTargetChanged(m_Target);
            }
        }

        /// <summary>
        /// Initial value used for certain tween jobs that need to process from the initial state. 
        /// </summary>
        public T initialValue { get; set; } = default;

        /// <summary>
        /// Tween from current value to target using tween target.
        /// </summary>
        /// <param name="tweenTarget">Value between 0-1 used in tween evaluation.</param>
        public void HandleTween(float tweenTarget)
        {
            if (ValueEquals(target))
                return;

            PreprocessTween();
            ExecuteTween(Value, target, tweenTarget);
        }

        /// <summary>
        /// Tween from current value to target using tween target.
        /// </summary>
        /// <param name="startValue">Tween starting value.</param>
        /// <param name="targetValue">Tween target value.</param>
        /// <param name="tweenAmount">Value between 0-1 used in tween evaluation.</param>
        /// <param name="useCurve">Whether the animation curve should be used in the tween evaluation.</param>
        /// <seealso cref="HandleTween"/>
        protected abstract void ExecuteTween(T startValue, T targetValue, float tweenAmount, bool useCurve = false);

        /// <summary>
        /// Coroutine used to automatically tween every frame.
        /// </summary>
        /// <param name="deltaTimeMultiplier">Multiplier used to scale deltaTime for tweens.</param>
        /// <returns>Returns enumerator used for coroutine.</returns>
        public IEnumerator StartAutoTween(float deltaTimeMultiplier)
        {
            while (true)
            {
                HandleTween(Time.deltaTime * deltaTimeMultiplier);
                yield return null;
            }
        }

        /// <summary>
        /// Play sequence to animate value from start to finish over given duration.
        /// </summary>
        /// <param name="start">Value to start animation at.</param>
        /// <param name="finish">Target Value to end animation at.</param>
        /// <param name="duration">Duration of animation.</param>
        /// <param name="onComplete">Optional callback when animation completes.</param>
        /// <returns>Returns enumerator used for coroutine.</returns>
        public IEnumerator PlaySequence(T start, T finish, float duration, Action onComplete = null)
        {
            var timeElapsed = 0f;
            while (timeElapsed < duration)
            {
                PreprocessTween();
                var completionPercent = Mathf.Clamp01(timeElapsed / duration);
                ExecuteTween(start, finish, completionPercent, useCurve: true);
                yield return null;
                timeElapsed += Time.deltaTime;
            }

            PreprocessTween();
            ExecuteTween(start, finish, 1f);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Called when the animation curve reference used for sequence animations changed.
        /// </summary>
        /// <param name="value">The new value of the property.</param>
        /// <seealso cref="animationCurve"/>
        protected virtual void OnAnimationCurveChanged(AnimationCurve value)
        {
        }

        /// <summary>
        /// Callback when new tween target value is assigned.
        /// </summary>
        /// <param name="newTarget">New target value.</param>
        /// <seealso cref="target"/>
        protected virtual void OnTargetChanged(T newTarget)
        {
        }

        /// <summary>
        /// Logic to execute before a tween can be processed.
        /// </summary>
        protected virtual void PreprocessTween()
        {
        }
    }
}