using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Jobs;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables
{
    /// <summary>
    /// Async implementation of base TweenableVariable.
    /// Uses affordance system jobs to asynchronously tween towards a target value.
    /// </summary>
    /// <typeparam name="T">BindableVariable type.</typeparam>
    /// <seealso cref="TweenableVariableSynchronousBase{T}"/>
    /// <remarks>
    /// While the destructor of this class should be able to automatically dispose of allocated resources, it is best practice
    /// to manually call dispose when instances are no longer needed.
    /// </remarks>
    public abstract class TweenableVariableAsyncBase<T> : TweenableVariableBase<T>, IDisposable where T : struct, IEquatable<T>
    {
        /// <summary>
        /// The internal tweenable variable value. When setting the value, subscribers may be notified.
        /// If any async jobs are pending, they will be completed and the state will sync up.
        /// The subscribers will not be notified if this variable is initialized, is configured to check for equality,
        /// and the new value is equivalent.
        /// </summary>
        public new T Value
        {
            get => base.Value;
            set
            {
                if (m_HasJobPending && m_OutputInitialized)
                {
                    // Force complete any lingering jobs
                    CompleteJob();

                    // Sync up the current state of the output store
                    m_JobOutputStore[0] = value;
                }
                base.Value = value;
            }
        }

        bool m_OutputInitialized;
        NativeArray<T> m_JobOutputStore;

        bool m_CurveDirty = true;
        NativeCurve m_NativeCurve;
        bool m_HasJobPending;

        JobHandle m_LastJobHandle;

        /// <summary>
        /// Free up allocated memory.
        /// </summary>
        public void Dispose()
        {
            if (m_OutputInitialized)
            {
                UpdateStateFromCompletedJob();
                m_JobOutputStore.Dispose();
                m_OutputInitialized = false;
            }

            if (m_NativeCurve.isCreated)
            {
                m_NativeCurve.Dispose();
                m_CurveDirty = true;
            }
        }

        /// <summary>
        /// Get the Burst friendly representation of the animation curve reference.
        /// </summary>
        /// <returns>Returns the Burst friendly representation of the animation curve reference.</returns>
        NativeCurve GetNativeCurve()
        {
            RefreshCurve();
            return m_NativeCurve;
        }

        /// <summary>
        /// Refresh internal data to ensure it is ready to package for a job.
        /// </summary>
        void RefreshCurve()
        {
            if (m_CurveDirty || !m_NativeCurve.isCreated)
            {
                m_NativeCurve.Update(animationCurve, 1024);
                m_CurveDirty = false;
            }
        }

        /// <inheritdoc />
        protected override void PreprocessTween()
        {
            base.PreprocessTween();
            
            // We have to update the state from the previous job before getting the new value from which to start the next tween. 
            UpdateStateFromCompletedJob();
        }

        /// <inheritdoc />
        protected override void ExecuteTween(T startValue, T targetValue, float tweenAmount, bool useCurve = false)
        {
            if (tweenAmount > k_NearlyOne)
            {
                Value = targetValue;
                return;
            }

            // If using curve, we want to use it to compute an adjusted state transition target.
            // This is necessary to play animations using the curve.
            var originValue = useCurve ? startValue : targetValue;
            var tweenInterpolationAmount = useCurve ? 1f : tweenAmount;
            var stateTransitionIncrement = useCurve ? (byte)math.ceil(tweenAmount * AffordanceStateData.totalStateTransitionIncrements) : AffordanceStateData.totalStateTransitionIncrements;

            var jobData = new TweenJobData<T>
            {
                initialValue = initialValue,
                stateOriginValue = originValue,
                stateTargetValue = targetValue,
                stateTransitionIncrement = stateTransitionIncrement,
                nativeCurve = GetNativeCurve(),
                tweenStartValue = startValue,
                tweenAmount = tweenInterpolationAmount,
                outputData = GetJobOutputStore(),
            };

            m_LastJobHandle = ScheduleTweenJob(ref jobData);
            m_HasJobPending = true;
        }

        void UpdateStateFromCompletedJob()
        {
            if (!CompleteJob())
                return;

            Value = GetJobOutputStore()[0];
        }

        /// <summary>
        /// Generate the tween job from the given job data and schedule the job for execution on a worker thread.
        /// </summary>
        /// <param name="jobData">Typed job data used in tween job.</param>
        /// <returns>Returns the handle identifying the scheduled job.</returns>
        protected abstract JobHandle ScheduleTweenJob(ref TweenJobData<T> jobData);

        NativeArray<T> GetJobOutputStore()
        {
            if (!m_OutputInitialized)
            {
                m_JobOutputStore = new NativeArray<T>(1, Allocator.Persistent);
                m_OutputInitialized = true;
                
                // Register to auto dispose.
                DisposableManagerSingleton.RegisterDisposable(this);
            }

            return m_JobOutputStore;
        }

        /// <inheritdoc />
        protected override void OnAnimationCurveChanged(AnimationCurve value)
        {
            base.OnAnimationCurveChanged(value);
            m_CurveDirty = true;
        }

        bool CompleteJob()
        {
            if (!m_OutputInitialized || !m_HasJobPending)
                return false;
            
            m_LastJobHandle.Complete();
            m_LastJobHandle = default;
            m_HasJobPending = false;
            return true;
        }
    }
}