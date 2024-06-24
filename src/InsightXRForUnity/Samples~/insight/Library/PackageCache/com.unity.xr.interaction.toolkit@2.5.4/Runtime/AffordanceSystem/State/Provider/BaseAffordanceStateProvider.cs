using System.Collections.Generic;
using Unity.Jobs;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State
{
    /// <summary>
    /// Base state machine for scheduling tween jobs on registered receivers.
    /// Starts when affordance state updates and new tweens need computing. Stops when transitions are complete.
    /// </summary>
    public abstract class BaseAffordanceStateProvider : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 5f)]
        [Tooltip("Duration of transition in seconds. 0 means no smoothing.")]
        float m_TransitionDuration = 0.125f;

        /// <summary>
        /// Duration of transition in seconds. <c>0</c> means no smoothing.
        /// </summary>
        public float transitionDuration
        {
            get => m_TransitionDuration;
            set
            {
                m_TransitionDuration = value;
                RefreshTransitionDuration();
            }
        }

        /// <summary>
        /// Returns true if last transition is complete.
        /// </summary>
        public bool isCurrentlyTransitioning => !m_CompletingTweens || m_ScheduledJobs.Count > 0;

        readonly BindableVariable<AffordanceStateData> m_AffordanceStateData = new BindableVariable<AffordanceStateData>();

        /// <summary>
        /// Bindable variable holding the affordance state data which is propagated to affordance receivers when changed.
        /// </summary>
        /// <seealso cref="UpdateAffordanceState"/>
        public IReadOnlyBindableVariable<AffordanceStateData> currentAffordanceStateData => m_AffordanceStateData;

        readonly HashSetList<IAsyncAffordanceStateReceiver> m_AsyncAffordanceReceivers = new HashSetList<IAsyncAffordanceStateReceiver>();
        readonly HashSetList<ISynchronousAffordanceStateReceiver> m_SynchronousAffordanceReceivers = new HashSetList<ISynchronousAffordanceStateReceiver>();
        readonly List<JobHandle> m_ScheduledJobs = new List<JobHandle>();

        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        float m_TimeSinceLastStateUpdate;
        bool m_IsFirstFrame = true;
        bool m_CompletingTweens;
        float m_InterpolationSpeed = 8f;
        float m_MaxTransitionDuration = 5f;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnValidate()
        {
            RefreshTransitionDuration();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            RefreshTransitionDuration();
            BindToProviders();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            ClearBindings();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Update()
        {
            if (m_IsFirstFrame)
            {
                OnAffordanceStateUpdated(m_AffordanceStateData.Value);
                DoTween(1f);
                m_IsFirstFrame = false;
                return;
            }

            DoTween((m_InterpolationSpeed > 0f) ? Time.deltaTime * m_InterpolationSpeed : 1f);
        }

        /// <summary>
        /// Inform all registered receivers of the current affordance state data and subscribe to changes of the affordance state.
        /// </summary>
        /// <remarks>
        /// This method is automatically called by Unity when this component is enabled.
        /// </remarks>
        /// <seealso cref="IAffordanceStateReceiver.OnAffordanceStateUpdated"/>
        protected virtual void BindToProviders()
        {
            ClearBindings();
            m_IsFirstFrame = true;
            AddBinding(m_AffordanceStateData.SubscribeAndUpdate(OnAffordanceStateUpdated));
        }

        /// <summary>
        /// Triggers unbind action on all bindings and destroys all stored binding actions, as well as clears the
        /// group of all registered bindings.
        /// </summary>
        /// <remarks>
        /// This method is automatically called by Unity when this component is disabled.
        /// </remarks>
        protected virtual void ClearBindings()
        {
            m_BindingsGroup.Clear();
        }

        /// <summary>
        /// Register binding to the binding group.
        /// </summary>
        /// <param name="binding">Binding to register.</param>
        /// <seealso cref="ClearBindings"/>
        protected void AddBinding(IEventBinding binding)
        {
            m_BindingsGroup.AddBinding(binding);
        }

        /// <summary>
        /// Externally control the affordance state used as a target for affordance receivers.
        /// Useful especially in the process of networking affordance states.
        /// </summary>
        /// <param name="newAffordanceStateData">New affordance state target.</param>
        public void UpdateAffordanceState(AffordanceStateData newAffordanceStateData)
        {
            m_AffordanceStateData.Value = newAffordanceStateData;
        }

        void OnAffordanceStateUpdated(AffordanceStateData newAffordanceStateData)
        {
            for (var i = 0; i < m_AsyncAffordanceReceivers.Count; ++i)
            {
                m_AsyncAffordanceReceivers[i].OnAffordanceStateUpdated(m_AffordanceStateData.Value, newAffordanceStateData);
            }

            for (var i = 0; i < m_SynchronousAffordanceReceivers.Count; ++i)
            {
                m_SynchronousAffordanceReceivers[i].OnAffordanceStateUpdated(m_AffordanceStateData.Value, newAffordanceStateData);
            }

            m_TimeSinceLastStateUpdate = 0f;
            m_CompletingTweens = false;
        }

        /// <summary>
        /// Entry point for receivers to register themselves to have their tween jobs scheduled.
        /// </summary>
        /// <param name="receiver">Receiver to register.</param>
        /// <returns>Returns <see langword="true"/> if receiver was newly registered as a result of this method.
        /// Otherwise, returns <see langword="false"/> if already registered.</returns>
        public bool RegisterAffordanceReceiver(IAffordanceStateReceiver receiver)
        {
            if (receiver is IAsyncAffordanceStateReceiver asyncReceiver)
                return RegisterAffordanceReceiver(asyncReceiver);

            if (receiver is ISynchronousAffordanceStateReceiver synchronousReceiver)
                return RegisterAffordanceReceiver(synchronousReceiver);

            if (receiver != null)
                Debug.LogError($"Unhandled type of {nameof(IAffordanceStateReceiver)}: {receiver.GetType().Name}", this);

            return false;
        }

        bool RegisterAffordanceReceiver(IAsyncAffordanceStateReceiver receiver)
        {
            return m_AsyncAffordanceReceivers.Add(receiver);
        }

        bool RegisterAffordanceReceiver(ISynchronousAffordanceStateReceiver receiver)
        {
            return m_SynchronousAffordanceReceivers.Add(receiver);
        }

        /// <summary>
        /// Entry point for receivers to unregister themselves from having their tween jobs scheduled.
        /// Calling this will also force complete any outstanding jobs if it is an asynchronous receiver.
        /// </summary>
        /// <param name="receiver">Receiver to unregister.</param>
        /// <returns>Returns <see langword="true"/> if receiver was newly unregistered as a result of this method.
        /// Otherwise, returns <see langword="false"/> if already unregistered.</returns>
        public bool UnregisterAffordanceReceiver(IAffordanceStateReceiver receiver)
        {
            if (receiver is IAsyncAffordanceStateReceiver asyncReceiver)
                return UnregisterAffordanceReceiver(asyncReceiver);

            if (receiver is ISynchronousAffordanceStateReceiver synchronousReceiver)
                return UnregisterAffordanceReceiver(synchronousReceiver);

            if (receiver != null)
                Debug.LogError($"Unhandled type of {nameof(IAffordanceStateReceiver)}: {receiver.GetType().Name}", this);

            return false;
        }

        bool UnregisterAffordanceReceiver(IAsyncAffordanceStateReceiver receiver)
        {
            // Force complete jobs
            CompleteJobs();
            return m_AsyncAffordanceReceivers.Remove(receiver);
        }

        bool UnregisterAffordanceReceiver(ISynchronousAffordanceStateReceiver receiver)
        {
            return m_SynchronousAffordanceReceivers.Remove(receiver);
        }

        bool CompleteJobs()
        {
            for (var i = 0; i < m_ScheduledJobs.Count; ++i)
            {
                m_ScheduledJobs[i].Complete();
            }

            var completedAnyJobs = m_ScheduledJobs.Count > 0;
            m_ScheduledJobs.Clear();
            return completedAnyJobs;
        }

        void DoTween(float tweenTarget)
        {
            // Complete previous jobs
            var completedAnyJob = CompleteJobs();
            if (completedAnyJob)
            {
                // Update state
                for (var i = 0; i < m_AsyncAffordanceReceivers.Count; ++i)
                {
                    m_AsyncAffordanceReceivers[i].UpdateStateFromCompletedJob();
                }
            }

            var adjustedTarget = tweenTarget;
            // If we arrive at the max transition duration or a target of 1f, then we force the tween to end
            var completingTween = m_TimeSinceLastStateUpdate > m_MaxTransitionDuration || adjustedTarget > 0.99f;
            if (completingTween)
            {
                // If we've fully transitioned to the new state, avoid scheduling new jobs
                if (m_CompletingTweens)
                    return;

                // Snap to end state
                adjustedTarget = 1f;
                m_CompletingTweens = true;
            }

            // Schedule new tween jobs
            // Tweening is 1 frame behind to execute all the tween computation asynchronously
            for (var i = 0; i < m_AsyncAffordanceReceivers.Count; ++i)
            {
                m_ScheduledJobs.Add(m_AsyncAffordanceReceivers[i].HandleTween(adjustedTarget));
            }

            // Handle the tweens for synchronous affordance receivers
            for (var i = 0; i < m_SynchronousAffordanceReceivers.Count; ++i)
            {
                m_SynchronousAffordanceReceivers[i].HandleTween(adjustedTarget);
            }

            // Increment timer
            m_TimeSinceLastStateUpdate += Time.deltaTime;
        }

        void RefreshTransitionDuration()
        {
            m_InterpolationSpeed = m_TransitionDuration > 0f ? 1f / m_TransitionDuration : 0f;

            // Set a max duration of 4x the transition speed.
            // This is to give enough time for a smooth falloff of easing towards the target.
            // After this point, we snap to the target and stop scheduling tweens.
            m_MaxTransitionDuration = m_TransitionDuration * 4f;
        }
    }
}