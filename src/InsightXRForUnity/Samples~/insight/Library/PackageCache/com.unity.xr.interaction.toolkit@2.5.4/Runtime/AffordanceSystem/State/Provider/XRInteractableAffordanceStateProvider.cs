using System;
using System.Collections;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State
{
    /// <summary>
    /// State Machine component that derives an interaction affordance state from an associated <see cref="IXRInteractable"/>.
    /// </summary>
    [AddComponentMenu("Affordance System/XR Interactable Affordance State Provider", 11)]
    [HelpURL(XRHelpURLConstants.k_XRInteractableAffordanceStateProvider)]
    [DisallowMultipleComponent]
    public class XRInteractableAffordanceStateProvider : BaseAffordanceStateProvider
    {
        /// <summary>
        /// Animation mode options used for select state callbacks.
        /// </summary>
        /// <seealso cref="selectClickAnimationMode"/>
        public enum SelectClickAnimationMode
        {
            /// <summary>
            /// No click animation override for select events.
            /// </summary>
            None,

            /// <summary>
            /// Use click animation on select entered event.
            /// </summary>
            SelectEntered,

            /// <summary>
            /// Use click animation on select exited event.
            /// </summary>
            SelectExited,
        }

        /// <summary>
        /// Animation mode options used for activate state callbacks.
        /// </summary>
        /// <seealso cref="activateClickAnimationMode"/>
        public enum ActivateClickAnimationMode
        {
            /// <summary>
            /// No click animation override for activate events.
            /// </summary>
            None,

            /// <summary>
            /// Use click animation on activate event.
            /// </summary>
            Activated,

            /// <summary>
            /// Use click animation on deactivate event.
            /// </summary>
            Deactivated,
        }

        [SerializeField]
        [RequireInterface(typeof(IXRInteractable))]
        [Tooltip("The interactable component that drives the affordance states. If null, Unity will try and find an interactable component attached.")]
        Object m_InteractableSource;

        /// <summary>
        /// The interactable component that drives the affordance states.
        /// If <see langword="null"/>, Unity will try and find an interactable component attached.
        /// </summary>
        public Object interactableSource
        {
            get => m_InteractableSource;
            set
            {
                m_InteractableSource = value;
                if (Application.isPlaying && isActiveAndEnabled)
                    SetBoundInteractionReceiver(value as IXRInteractable);
            }
        }

        [Header("Event Constraints")]
        [SerializeField]
        [Tooltip("When hover events are registered and this is true, the state will fallback to idle or disabled.")]
        bool m_IgnoreHoverEvents;

        /// <summary>
        /// When hover events are registered and this is true, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreHoverEvents
        {
            get => m_IgnoreHoverEvents;
            set => m_IgnoreHoverEvents = value;
        }

        [SerializeField]
        [Tooltip("When this is true, the state will fallback to hover if the later is not ignored. When this is false, this provider will check " +
                 "if the Interactable Source has priority for selection when hovered, and update its state accordingly.")]
        bool m_IgnoreHoverPriorityEvents = true;

        /// <summary>
        /// When hover events are registered and this is true, the state will fallback to hover. When this is <see langword="false"/>, this
        /// provider will check if the Interactable Source has priority for selection when hovered, and update its state accordingly.
        /// </summary>
        /// <remarks>When updating this value to <see langword="false"/> during runtime, previously hover events are ignored.</remarks>
        public bool ignoreHoverPriorityEvents
        {
            get => m_IgnoreHoverPriorityEvents;
            set
            {
                if (Application.isPlaying && isActiveAndEnabled && !m_IgnoreHoverPriorityEvents && value)
                {
                    StopHoveredPriorityRoutine();
                    RefreshState();
                }

                m_IgnoreHoverPriorityEvents = value;
            }
        }

        [SerializeField]
        [Tooltip("When focus events are registered and this is true, the state will fallback to idle or disabled.")]
        bool m_IgnoreFocusEvents = true;

        /// <summary>
        /// When focus events are registered and this is true, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreFocusEvents
        {
            get => m_IgnoreFocusEvents;
            set => m_IgnoreFocusEvents = value;
        }

        [SerializeField]
        [Tooltip("When select events are registered and this is true, the state will fallback to idle or disabled. " +
                 "Note this will not affect click animations which can be disabled separately.")]
        bool m_IgnoreSelectEvents;

        /// <summary>
        /// When select events are registered and this is true, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreSelectEvents
        {
            get => m_IgnoreSelectEvents;
            set => m_IgnoreSelectEvents = value;
        }

        [SerializeField]
        [Tooltip("When activate events are registered and this is true, the state will fallback to idle or disabled." +
                 "Note this will not affect click animations which can be disabled separately.")]
        bool m_IgnoreActivateEvents;

        /// <summary>
        /// When activate events are registered and this is true, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreActivateEvents
        {
            get => m_IgnoreActivateEvents;
            set => m_IgnoreActivateEvents = value;
        }

        [Header("Click Animation Config")]
        [SerializeField]
        [Tooltip("Condition to trigger click animation for Selected interaction events.")]
        SelectClickAnimationMode m_SelectClickAnimationMode = SelectClickAnimationMode.SelectEntered;

        /// <summary>
        /// Condition to trigger click animation for Selected interaction events.
        /// </summary>
        /// <seealso cref="SelectClickAnimationMode"/>
        public SelectClickAnimationMode selectClickAnimationMode
        {
            get => m_SelectClickAnimationMode;
            set => m_SelectClickAnimationMode = value;
        }

        [SerializeField]
        [Tooltip("Condition to trigger click animation for activated interaction events.")]
        ActivateClickAnimationMode m_ActivateClickAnimationMode = ActivateClickAnimationMode.None;

        /// <summary>
        /// Condition to trigger click animation for activated interaction events.
        /// </summary>
        /// <seealso cref="ActivateClickAnimationMode"/>
        public ActivateClickAnimationMode activateClickAnimationMode
        {
            get => m_ActivateClickAnimationMode;
            set => m_ActivateClickAnimationMode = value;
        }

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Duration of click animations for selected and activated events.")]
        float m_ClickAnimationDuration = 0.25f;

        /// <summary>
        /// Duration of click animations for selected and activated events.
        /// </summary>
        public float clickAnimationDuration
        {
            get => m_ClickAnimationDuration;
            set => m_ClickAnimationDuration = value;
        }

        [SerializeField]
        [Tooltip("Animation curve reference for click animation events. Select the More menu (\u22ee) to choose between a direct reference and a reusable scriptable object animation curve datum.")]
        AnimationCurveDatumProperty m_ClickAnimationCurve = new AnimationCurveDatumProperty(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

        /// <summary>
        /// Animation curve reference for click animation events.
        /// </summary>
        public AnimationCurveDatumProperty clickAnimationCurve
        {
            get => m_ClickAnimationCurve;
            set => m_ClickAnimationCurve = value;
        }

        /// <summary>
        /// Is attached interactable in a hovered state.
        /// </summary>
        protected virtual bool isHovered => m_HasHoverInteractable && m_HoverInteractable.isHovered;

        /// <summary>
        /// Is attached interactable in a selected state.
        /// </summary>
        protected virtual bool isSelected => m_HasSelectInteractable && m_SelectInteractable.isSelected;

        /// <summary>
        /// Is attached interactable in a focused state.
        /// </summary>
        protected virtual bool isFocused => m_FocusInteractable != null && m_FocusInteractable.isFocused;

        /// <summary>
        /// Is attached interactable in an activated state.
        /// </summary>
        protected virtual bool isActivated => m_IsActivated;

        /// <summary>
        /// Is attached interactable in a registered state.
        /// </summary>
        protected virtual bool isRegistered => m_IsRegistered;

        IXRInteractable m_Interactable;
        IXRHoverInteractable m_HoverInteractable;
        IXRSelectInteractable m_SelectInteractable;
        IXRFocusInteractable m_FocusInteractable;
        IXRActivateInteractable m_ActivateInteractable;
        IXRInteractionStrengthInteractable m_InteractionStrengthInteractable;

        Coroutine m_SelectedClickAnimation;
        Coroutine m_ActivatedClickAnimation;
        Coroutine m_HoveredPriorityRoutine;

        bool m_IsBoundToInteractionEvents;

        bool m_IsActivated;
        bool m_IsRegistered;
        bool m_IsHoveredPriority;

        bool m_HasHoverInteractable;
        bool m_HasSelectInteractable;
        bool m_HasInteractionStrengthInteractable;

        int m_HoveringPriorityInteractorCount;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            var receiver = m_InteractableSource != null && m_InteractableSource is IXRInteractable interactable
                ? interactable
                : GetComponentInParent<IXRInteractable>();
            if (!SetBoundInteractionReceiver(receiver))
            {
                XRLoggingUtils.LogWarning($"Could not find required interactable component on {gameObject}" +
                    " for which to provide affordance states.", this);
                enabled = false;
            }
        }

        /// <inheritdoc />
        protected override void OnValidate()
        {
            base.OnValidate();

            if (Application.isPlaying && isActiveAndEnabled && m_IgnoreHoverPriorityEvents)
            {
                StopHoveredPriorityRoutine();
                RefreshState();
            }
        }

        /// <summary>
        /// Bind affordance provider state to <see cref="IXRInteractable"/> state and events.
        /// </summary>
        /// <param name="receiver">Receiver to bind events from.</param>
        /// <returns>Whether binding was successful.</returns>
        public bool SetBoundInteractionReceiver(IXRInteractable receiver)
        {
            ClearBindings();

            var isInteractableValid = receiver is Object unityObject && unityObject != null;
            if (isInteractableValid)
            {
                m_Interactable = receiver;

                if (m_Interactable is IXRHoverInteractable hoverInteractable)
                    m_HoverInteractable = hoverInteractable;

                if (m_Interactable is IXRSelectInteractable selectInteractable)
                    m_SelectInteractable = selectInteractable;

                if (m_Interactable is IXRFocusInteractable focusInteractable)
                    m_FocusInteractable = focusInteractable;

                if (m_Interactable is IXRActivateInteractable activateInteractable)
                    m_ActivateInteractable = activateInteractable;

                if (m_Interactable is IXRInteractionStrengthInteractable interactionStrengthInteractable)
                    m_InteractionStrengthInteractable = interactionStrengthInteractable;
            }
            else
            {
                m_Interactable = null;
                m_HoverInteractable = null;
                m_SelectInteractable = null;
                m_FocusInteractable = null;
                m_ActivateInteractable = null;
                m_InteractionStrengthInteractable = null;
            }

            m_HasHoverInteractable = m_HoverInteractable != null;
            m_HasSelectInteractable = m_SelectInteractable != null;
            m_HasInteractionStrengthInteractable = m_InteractionStrengthInteractable != null;

            BindToProviders();
            return isInteractableValid;
        }

        /// <summary>
        /// Callback triggered when the interactable is registered with the <see cref="XRInteractionManager"/>.
        /// Sets the internal isRegistered flag to true and refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="InteractableRegisteredEventArgs"/> callback args.</param>
        protected virtual void OnRegistered(InteractableRegisteredEventArgs args)
        {
            m_IsRegistered = true;
            RefreshState();
        }

        /// <summary>
        /// Callback triggered when the interactable is unregistered with the <see cref="XRInteractionManager"/>.
        /// Sets the internal isRegistered flag to false and refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="InteractableUnregisteredEventArgs"/> callback args.</param>
        protected virtual void OnUnregistered(InteractableUnregisteredEventArgs args)
        {
            m_IsRegistered = false;
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRHoverInteractable"/> when the first interactor begins hovering over this interactable.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="HoverEnterEventArgs"/> callback args.</param>
        /// <seealso cref="IXRHoverInteractable.firstHoverEntered"/>
        protected virtual void OnFirstHoverEntered(HoverEnterEventArgs args)
        {
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRHoverInteractable"/> when the last interactor exits hovering over this interactable.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="HoverExitEventArgs"/> callback args.</param>
        /// <seealso cref="IXRHoverInteractable.lastHoverExited"/>
        protected virtual void OnLastHoverExited(HoverExitEventArgs args)
        {
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRHoverInteractable"/> when an interactor begins hovering over this interactable.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="HoverEnterEventArgs"/> callback args.</param>
        /// <seealso cref="IXRHoverInteractable.hoverEntered"/>
        protected virtual void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (m_IgnoreHoverPriorityEvents)
                return;

            if (args.interactorObject is IXRTargetPriorityInteractor priorityInteractor)
            {
                m_HoveringPriorityInteractorCount++;
                if (priorityInteractor.targetPriorityMode != TargetPriorityMode.None)
                    m_HoveredPriorityRoutine = m_HoveredPriorityRoutine ?? StartCoroutine(HoveredPriorityRoutine());
            }
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRHoverInteractable"/> when an interactor exits hovering over this interactable.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="HoverExitEventArgs"/> callback args.</param>
        /// <seealso cref="IXRHoverInteractable.hoverExited"/>
        protected virtual void OnHoverExited(HoverExitEventArgs args)
        {
            if (m_IgnoreHoverPriorityEvents)
                return;

            if (args.interactorObject is IXRTargetPriorityInteractor)
            {
                m_HoveringPriorityInteractorCount--;
                if (m_HoveringPriorityInteractorCount > 0)
                    return;

                StopHoveredPriorityRoutine();
                RefreshState();
            }
        }

        void StopHoveredPriorityRoutine()
        {
            m_HoveringPriorityInteractorCount = 0;
            m_IsHoveredPriority = false;
            if (m_HoveredPriorityRoutine != null)
            {
                StopCoroutine(m_HoveredPriorityRoutine);
                m_HoveredPriorityRoutine = null;
            }
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRSelectInteractor"/> when the first interactor begins selecting over this interactable.
        /// Refreshes the affordance state and triggers the <see cref="SelectedClickBehavior"/> animation coroutine if the select animation mode is set to SelectEntered.
        /// </summary>
        /// <param name="args"><see cref="SelectEnterEventArgs"/> callback args.</param>
        /// <seealso cref="IXRSelectInteractable.firstSelectEntered"/>
        protected virtual void OnFirstSelectEntered(SelectEnterEventArgs args)
        {
            if (m_IgnoreSelectEvents || m_SelectClickAnimationMode != SelectClickAnimationMode.SelectEntered || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                RefreshState();
                return;
            }

            SelectedClickBehavior();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRSelectInteractor"/> when the last interactor exits selecting over this interactable.
        /// Refreshes the affordance state and triggers the <see cref="SelectedClickBehavior"/> animation coroutine if the select animation mode is set to SelectExited.
        /// </summary>
        /// <param name="args"><see cref="SelectExitEventArgs"/> callback args.</param>
        /// <seealso cref="IXRSelectInteractable.lastSelectExited"/>
        protected virtual void OnLastSelectExited(SelectExitEventArgs args)
        {
            if (m_IgnoreSelectEvents || m_SelectClickAnimationMode != SelectClickAnimationMode.SelectExited || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                // If Select animation is playing and we are exiting, we need to wait for the animation to finish before refreshing state
                if(m_SelectedClickAnimation != null)
                    return;
                
                RefreshState();
                return;
            }

            SelectedClickBehavior();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRFocusInteractable"/> when the first interactor gains focus of this interactable.
        /// Refreshes the affordance state
        /// </summary>
        /// <param name="args"><see cref="FocusEnterEventArgs"/> callback args.</param>
        /// <seealso cref="IXRFocusInteractable.firstFocusEntered"/>
        protected virtual void OnFirstFocusEntered(FocusEnterEventArgs args)
        {
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRFocusInteractable"/> when the last interactor loses focus of this interactable.
        /// Refreshes the affordance state
        /// </summary>
        /// <param name="args"><see cref="FocusExitEventArgs"/> callback args.</param>
        /// <seealso cref="IXRFocusInteractable.lastFocusExited"/>
        protected virtual void OnLastFocusExited(FocusExitEventArgs args)
        {
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRActivateInteractable"/> when the interactor triggers an activated event on the interactable.
        /// Refreshes the affordance state and triggers the <see cref="ActivatedClickBehavior"/> animation coroutine if the activated animation mode is set to Activated.
        /// </summary>
        /// <param name="args"><see cref="ActivateEventArgs"/> callback args.</param>
        /// <seealso cref="IXRActivateInteractable.activated"/>
        protected virtual void OnActivatedEvent(ActivateEventArgs args)
        {
            m_IsActivated = true;
            if (m_IgnoreActivateEvents || (m_ActivateClickAnimationMode != ActivateClickAnimationMode.Activated) || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                RefreshState();
                return;
            }

            ActivatedClickBehavior();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRActivateInteractable"/> when the interactor triggers an deactivated event on the interactable.
        /// Refreshes the affordance state and triggers the <see cref="ActivatedClickBehavior"/> animation coroutine if the activated animation mode is set to deactivated.
        /// </summary>
        /// <param name="args"><see cref="DeactivateEventArgs"/> callback args.</param>
        /// <seealso cref="IXRActivateInteractable.deactivated"/>
        protected virtual void OnDeactivatedEvent(DeactivateEventArgs args)
        {
            m_IsActivated = false;
            if (m_IgnoreActivateEvents || (m_ActivateClickAnimationMode != ActivateClickAnimationMode.Deactivated) || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                // If activate animation is playing and we are exiting, we need to wait for the animation to finish before refreshing state
                if (m_ActivatedClickAnimation != null)
                    return;
                
                RefreshState();
                return;
            }

            ActivatedClickBehavior();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRInteractionStrengthInteractable"/> when the largest interaction strength of the interactable changes.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="value">The new largest interaction strength value of all interactors hovering or selecting the interactable.</param>
        protected virtual void OnLargestInteractionStrengthChanged(float value)
        {
            // If currently executing animation, do not update interaction strength state.
            if(m_SelectedClickAnimation != null || m_ActivatedClickAnimation != null)
                return;

            RefreshState();
        }

        /// <summary>
        /// Handles starting the selected click animation coroutine. Stops any previously started coroutine.
        /// </summary>
        protected virtual void SelectedClickBehavior()
        {
            StopAllClickAnimations();
            m_SelectedClickAnimation = StartCoroutine(ClickAnimation(AffordanceStateShortcuts.selected, m_ClickAnimationDuration, () => m_SelectedClickAnimation = null));
        }

        /// <summary>
        /// Handles starting the activated click animation coroutine. Stops any previously started coroutine.
        /// </summary>
        protected virtual void ActivatedClickBehavior()
        {
            StopAllClickAnimations();
            m_ActivatedClickAnimation = StartCoroutine(ClickAnimation(AffordanceStateShortcuts.activated, m_ClickAnimationDuration, () => m_ActivatedClickAnimation = null));
        }

        void StopActivatedCoroutine()
        {
            if (m_ActivatedClickAnimation == null)
                return;
            StopCoroutine(m_ActivatedClickAnimation);
            m_ActivatedClickAnimation = null;
        }

        void StopSelectedCoroutine()
        {
            if (m_SelectedClickAnimation == null)
                return;
            StopCoroutine(m_SelectedClickAnimation);
            m_SelectedClickAnimation = null;
        }
        
        void StopAllClickAnimations()
        {
            StopActivatedCoroutine();
            StopSelectedCoroutine();
        }

        /// <summary>
        /// Click animation coroutine that plays over a set period of time, transitioning between the lower and upper bounds of a given affordance state.
        /// </summary>
        /// <param name="targetStateIndex">Target animation state with bounds which to transition between.</param>
        /// <param name="duration">Duration of the animation.</param>
        /// <param name="onComplete">OnComplete callback action.</param>
        /// <returns>Enumerator used to play as a coroutine.</returns>
        protected virtual IEnumerator ClickAnimation(byte targetStateIndex, float duration, Action onComplete = null)
        {
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                var animValue = Mathf.Clamp01(elapsedTime / duration);
                var curveAdjustedAnimValue = m_ClickAnimationCurve.Value.Evaluate(animValue);
                var newAnimationState = new AffordanceStateData(targetStateIndex, curveAdjustedAnimValue);
                UpdateAffordanceState(newAnimationState);

                yield return null;
                elapsedTime += Time.deltaTime;
            }

            yield return null;

            RefreshState();
            onComplete?.Invoke();
        }

        /// <summary>
        /// Evaluates the state of the current interactable to generate a corresponding <see cref="AffordanceStateData"/>.
        /// </summary>
        /// <returns>Newly generated affordance state corresponding to the interactable state.</returns>
        protected virtual AffordanceStateData GenerateNewAffordanceState()
        {
            if (!m_IsBoundToInteractionEvents)
            {
                return currentAffordanceStateData.Value;
            }

            if (isActivated && !m_IgnoreActivateEvents)
            {
                return AffordanceStateShortcuts.activatedState;
            }

            if (!isActivated && isSelected && !m_IgnoreSelectEvents)
            {
                var transitionAmount = m_HasInteractionStrengthInteractable ? m_InteractionStrengthInteractable.largestInteractionStrength.Value : 1f;
                return new AffordanceStateData(AffordanceStateShortcuts.selected, transitionAmount);
            }

            if (!isActivated && !isSelected && isHovered && !m_IgnoreHoverEvents)
            {
                var stateIndex = m_IsHoveredPriority ? AffordanceStateShortcuts.hoveredPriority : AffordanceStateShortcuts.hovered;
                var transitionAmount = m_HasInteractionStrengthInteractable ? m_InteractionStrengthInteractable.largestInteractionStrength.Value : 0f;
                return new AffordanceStateData(stateIndex, transitionAmount);
            }

            if (!isActivated && !isSelected && !isHovered && isFocused && !m_IgnoreFocusEvents)
            {
                return AffordanceStateShortcuts.focusedState;
            }

            return isRegistered ? AffordanceStateShortcuts.idleState : AffordanceStateShortcuts.disabledState;
        }

        IEnumerator HoveredPriorityRoutine()
        {
            do
            {
                if (m_HoverInteractable is XRBaseInteractable baseInteractable &&
                    baseInteractable.interactionManager != null &&
                    baseInteractable.interactionManager.IsHighestPriorityTarget(baseInteractable) != m_IsHoveredPriority)
                {
                    m_IsHoveredPriority = !m_IsHoveredPriority;
                    RefreshState();
                }

                yield return null;
            } while (m_HoveringPriorityInteractorCount > 0);

            m_HoveredPriorityRoutine = null;
        }

        /// <inheritdoc/>
        protected override void BindToProviders()
        {
            base.BindToProviders();

            m_IsBoundToInteractionEvents = m_Interactable is Object unityObject && unityObject != null;
            if (m_IsBoundToInteractionEvents)
            {
                m_Interactable.registered += OnRegistered;
                m_Interactable.unregistered += OnUnregistered;

                if (m_HoverInteractable != null)
                {
                    m_HoverInteractable.firstHoverEntered.AddListener(OnFirstHoverEntered);
                    m_HoverInteractable.lastHoverExited.AddListener(OnLastHoverExited);
                    m_HoverInteractable.hoverEntered.AddListener(OnHoverEntered);
                    m_HoverInteractable.hoverExited.AddListener(OnHoverExited);
                }

                if (m_SelectInteractable != null)
                {
                    m_SelectInteractable.firstSelectEntered.AddListener(OnFirstSelectEntered);
                    m_SelectInteractable.lastSelectExited.AddListener(OnLastSelectExited);
                }

                if (m_FocusInteractable != null)
                {
                    m_FocusInteractable.firstFocusEntered.AddListener(OnFirstFocusEntered);
                    m_FocusInteractable.lastFocusExited.AddListener(OnLastFocusExited);
                }

                if (m_ActivateInteractable != null)
                {
                    m_ActivateInteractable.activated.AddListener(OnActivatedEvent);
                    m_ActivateInteractable.deactivated.AddListener(OnDeactivatedEvent);
                }

                if (m_InteractionStrengthInteractable != null)
                {
                    AddBinding(m_InteractionStrengthInteractable.largestInteractionStrength.Subscribe(OnLargestInteractionStrengthChanged));
                }

                m_IsActivated = false;

                // Initialize field for whether the interactable is registered to distinguish between Idle and Disabled affordance states.
                // The registration status is not yet part of the base interactable interface, so we use a reasonable assumption here.
                if (m_Interactable is XRBaseInteractable baseInteractable)
                {
                    m_IsRegistered = baseInteractable.interactionManager != null && baseInteractable.interactionManager.IsRegistered(m_Interactable);
                }
                else if (m_Interactable is Behaviour behavior)
                {
                    m_IsRegistered = behavior.isActiveAndEnabled;
                }
                else
                {
                    m_IsRegistered = true;
                }
            }

            RefreshState();
        }

        /// <summary>
        /// Re-evaluates the current affordance state and triggers events for receivers if it changed.
        /// </summary>
        public void RefreshState()
        {
            var newState = GenerateNewAffordanceState();

            // If leaving the selected state, we have to terminate select animation coroutines.
            if (newState.stateIndex != AffordanceStateShortcuts.selected)
                StopSelectedCoroutine();

            // If leaving the activated state, we have to terminate activated animation coroutines.
            if (newState.stateIndex != AffordanceStateShortcuts.activated)
                StopActivatedCoroutine();

            UpdateAffordanceState(newState);
        }

        /// <inheritdoc/>
        protected override void ClearBindings()
        {
            base.ClearBindings();

            if (m_IsBoundToInteractionEvents)
            {
                m_Interactable.registered -= OnRegistered;
                m_Interactable.unregistered -= OnUnregistered;

                if (m_HoverInteractable != null)
                {
                    m_HoverInteractable.firstHoverEntered.RemoveListener(OnFirstHoverEntered);
                    m_HoverInteractable.lastHoverExited.RemoveListener(OnLastHoverExited);
                    m_HoverInteractable.hoverEntered.RemoveListener(OnHoverEntered);
                    m_HoverInteractable.hoverExited.RemoveListener(OnHoverExited);
                }

                if (m_SelectInteractable != null)
                {
                    m_SelectInteractable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
                    m_SelectInteractable.lastSelectExited.RemoveListener(OnLastSelectExited);
                }

                if (m_FocusInteractable != null)
                {
                    m_FocusInteractable.firstFocusEntered.RemoveListener(OnFirstFocusEntered);
                    m_FocusInteractable.lastFocusExited.RemoveListener(OnLastFocusExited);
                }

                if (m_ActivateInteractable != null)
                {
                    m_ActivateInteractable.activated.RemoveListener(OnActivatedEvent);
                    m_ActivateInteractable.deactivated.RemoveListener(OnDeactivatedEvent);
                }

                // No need to unsubscribe from interaction strength here since it was added to the binding group
                // and would have been cleared upon calling the base method.
            }

            m_IsBoundToInteractionEvents = false;
        }
    }
}
