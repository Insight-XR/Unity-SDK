using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver
{
    /// <summary>
    /// Base implementation of an asynchronous affordance state receiver to be used with affordance types to tween using the job system.
    /// </summary>
    /// <typeparam name="T">The type of the value struct.</typeparam>
    public abstract class BaseAsyncAffordanceStateReceiver<T> : BaseAffordanceStateReceiver<T>, IAsyncAffordanceStateReceiver where T : struct, IEquatable<T>
    {
        NativeArray<T> m_JobOutputStore;
        NativeCurve m_NativeCurve;
        JobHandle m_LastJobHandle;

        bool m_OutputInitialized;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDestroy()
        {
            m_LastJobHandle.Complete();
            
            if (m_JobOutputStore.IsCreated)
                m_JobOutputStore.Dispose();

            if (m_NativeCurve.isCreated)
                m_NativeCurve.Dispose();
        }

        /// <inheritdoc/>
        public JobHandle HandleTween(float tweenTarget)
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
                return default;
            }

            var originValue = themeData.animationStateStartValue;
            var targetValue = themeData.animationStateEndValue;
            
            // Idle state and we want to replace idle state with initial
            if (replaceIdleStateValueWithInitialValue && stateData.stateIndex == AffordanceStateShortcuts.idle)
            {
                originValue = initialValue;
                targetValue = initialValue;
            }
            
            var jobData = new TweenJobData<T>
            {
                initialValue = this.initialValue,
                stateOriginValue = ProcessTargetAffordanceValue(originValue),
                stateTargetValue = ProcessTargetAffordanceValue(targetValue),
                stateTransitionIncrement = stateData.stateTransitionIncrement,
                nativeCurve = m_NativeCurve,
                tweenStartValue = currentAffordanceValue.Value,
                tweenAmount = tweenTarget,
                outputData = GetJobOutputStore(),
            };

            m_LastJobHandle = ScheduleTweenJob(ref jobData);
            return m_LastJobHandle;
        }

        /// <inheritdoc/>
        public void UpdateStateFromCompletedJob()
        {
            if (!m_OutputInitialized)
                return;

            ConsumeAffordance(GetJobOutputStore()[0]);
        }

        /// <summary>
        /// Generate the tween job from the given job data and schedule the job for execution on a worker thread.
        /// </summary>
        /// <param name="jobData">Typed job data used in tween job.</param>
        /// <returns>Returns the handle identifying the scheduled job.</returns>
        protected abstract JobHandle ScheduleTweenJob(ref TweenJobData<T> jobData);

        /// <inheritdoc/>
        protected override void OnAffordanceThemeChanged(BaseAffordanceTheme<T> newValue)
        {
            base.OnAffordanceThemeChanged(newValue);

            // Update internal native curve with new theme curve value.
            m_NativeCurve.Update(newValue.animationCurve, 1024);
        }

        NativeArray<T> GetJobOutputStore()
        {
            if (!m_OutputInitialized && enabled)
            {
                m_JobOutputStore = new NativeArray<T>(1, Allocator.Persistent);
                m_OutputInitialized = true;
            }

            return m_JobOutputStore;
        }
    }
}