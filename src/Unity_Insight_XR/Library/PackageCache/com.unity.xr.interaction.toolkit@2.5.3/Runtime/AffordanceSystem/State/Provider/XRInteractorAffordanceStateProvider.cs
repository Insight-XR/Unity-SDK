using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State
{
    /// <summary>
    /// State Machine component that derives an interaction affordance state from an associated <see cref="IXRInteractor"/>.
    /// </summary>
    [AddComponentMenu("Affordance System/XR Interactor Affordance State Provider", 11)]
    [HelpURL(XRHelpURLConstants.k_XRInteractorAffordanceStateProvider)]
    [DisallowMultipleComponent]
    public class XRInteractorAffordanceStateProvider : BaseAffordanceStateProvider
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
        [RequireInterface(typeof(IXRInteractor))]
        [Tooltip("The interactor component that drives the affordance states. If null, Unity will try and find an interactor component attached.")]
        Object m_InteractorSource;

        /// <summary>
        /// The interactor component that drives the affordance states.
        /// If <see langword="null"/>, Unity will try and find an interactor component attached.
        /// </summary>
        public Object interactorSource
        {
            get => m_InteractorSource;
            set
            {
                m_InteractorSource = value;
                if (Application.isPlaying && isActiveAndEnabled)
                    SetBoundInteractionReceiver(value as IXRInteractor);
            }
        }

        [Header("Event Constraints")]
        [SerializeField]
        [Tooltip("When hover events are registered and this is true, the state will fallback to idle or disabled.")]
        bool m_IgnoreHoverEvents;

        /// <summary>
        /// When hover events are registered and this is <see langword="true"/>, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreHoverEvents
        {
            get => m_IgnoreHoverEvents;
            set => m_IgnoreHoverEvents = value;
        }

        [SerializeField]
        [Tooltip("When select events are registered and this is true, the state will fallback to idle or disabled. " +
                 "\nNote: Click animations must be disabled separately.")]
        bool m_IgnoreSelectEvents;

        /// <summary>
        /// When select events are registered and this is <see langword="true"/>, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreSelectEvents
        {
            get => m_IgnoreSelectEvents;
            set => m_IgnoreSelectEvents = value;
        }

        [SerializeField]
        [Tooltip("When activate events are registered and this is true, the state will fallback to idle or disabled." +
                 "\nNote: Click animations must be disabled separately.")]
        bool m_IgnoreActivateEvents = true;

        /// <summary>
        /// When activate events are registered and this is <see langword="true"/>, the state will fallback to idle or disabled.
        /// </summary>
        public bool ignoreActivateEvents
        {
            get => m_IgnoreActivateEvents;
            set => m_IgnoreActivateEvents = value;
        }

        [SerializeField]
        [Tooltip("With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas.")]
        bool m_IgnoreUGUIHover;

        /// <summary>
        /// With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas.
        /// </summary>
        public bool ignoreUGUIHover
        {
            get => m_IgnoreUGUIHover;
            set => m_IgnoreUGUIHover = value;
        }
        
        [SerializeField]
        [Tooltip("With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas and triggering the select input.")]
        bool m_IgnoreUGUISelect;

        /// <summary>
        /// With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas and triggering the select input.
        /// </summary>
        public bool ignoreUGUISelect
        {
            get => m_IgnoreUGUISelect;
            set => m_IgnoreUGUISelect = value;
        }

        [SerializeField]
        [Tooltip("This option will prevent Hover, Select, and Activate events from being triggered when they come from the XR Interaction Manager. UGUI hover and select events will still come through.")]
        bool m_IgnoreXRInteractionEvents;

        /// <summary>
        /// This option will prevent Hover, Select, and Activate events from being triggered when they come from the XR Interaction Manager. UGUI hover and select events will still come through.
        /// </summary>
        public bool ignoreXRInteractionEvents
        {
            get => m_IgnoreXRInteractionEvents;
            set => m_IgnoreXRInteractionEvents = value;
        }

        /// <summary>
        /// Is attached interactor in a hovered state.
        /// </summary>
        protected virtual bool hasXRHover => (!m_IgnoreXRInteractionEvents && m_HasHoverInteractor && m_HoverInteractor.hasHover);

        /// <summary>
        /// Is the interactor overlapping a UI Canvas and hitting a UI raycast target.
        /// </summary>
        protected virtual bool hasUIHover => !m_IgnoreUGUIHover && m_UIHovering;
        
        /// <summary>
        /// Is attached interactor in a selected state.
        /// </summary>
        protected virtual bool hasXRSelection => !m_IgnoreXRInteractionEvents && m_HasSelectInteractor && m_SelectInteractor.hasSelection;
        
        /// <summary>
        /// Whether the interactor is hovering UI and the interactor select action is pressed.
        /// </summary>
        protected virtual bool hasUISelection => !m_IgnoreUGUISelect && m_UISelecting;
        
        /// <summary>
        /// Is attached interactable in an activated state.
        /// </summary>
        protected virtual bool isActivated => !m_IgnoreXRInteractionEvents && m_IsActivated;

        /// <summary>
        /// Is attached interactor in a registered state.
        /// </summary>
        protected virtual bool isRegistered => m_IsRegistered;

        /// <summary>
        /// Check if interactor is blocked by interaction within its group.
        /// </summary>
        protected virtual bool isBlockedByGroup => m_IsIXRInteractor && !m_Interactor.IsBlockedByInteractionWithinGroup();

        [Header("Click Animation Config")]
        [SerializeField]
        [Tooltip("Condition to trigger click animation for Selected interaction events.")]
        SelectClickAnimationMode m_SelectClickAnimationMode = SelectClickAnimationMode.SelectEntered;

        /// <summary>
        /// Condition to trigger click animation for Selected interaction events.
        /// </summary>
        public SelectClickAnimationMode selectClickAnimationMode
        {
            get => m_SelectClickAnimationMode;
            set => m_SelectClickAnimationMode = value;
        }

        [SerializeField]
        [Tooltip("Condition to trigger click animation for activated interaction events.")]
        ActivateClickAnimationMode m_ActivateClickAnimationMode = ActivateClickAnimationMode.Activated;

        /// <summary>
        /// Condition to trigger click animation for activated interaction events.
        /// </summary>
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
        [Tooltip("Animation curve reference for click animation events. Select the More menu (\u22ee) to choose between a direct reference and a reusable asset.")]
        AnimationCurveDatumProperty m_ClickAnimationCurve = new AnimationCurveDatumProperty(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

        /// <summary>
        /// Animation curve reference for click animation events.
        /// </summary>
        public AnimationCurveDatumProperty clickAnimationCurve
        {
            get => m_ClickAnimationCurve;
            set => m_ClickAnimationCurve = value;
        }

        IXRInteractor m_Interactor;
        IXRHoverInteractor m_HoverInteractor;
        IXRSelectInteractor m_SelectInteractor;
        IXRInteractionStrengthInteractor m_InteractionStrengthInteractor;
        XRRayInteractor m_RayInteractor;

        bool m_IsBoundToInteractionEvents;

        bool m_HasRayInteractor;
        bool m_HasHoverInteractor;
        bool m_HasSelectInteractor;
        bool m_HasInteractionStrengthInteractor;
        // ReSharper disable once InconsistentNaming
        bool m_IsIXRInteractor;

        Coroutine m_SelectedClickAnimation;
        Coroutine m_ActivatedClickAnimation;

        bool m_IsActivated;
        bool m_IsRegistered;

        readonly HashSet<IXRActivateInteractable> m_BoundActivateInteractable = new HashSet<IXRActivateInteractable>();

        bool m_UIHovering;
        bool m_UISelecting;

        Coroutine m_UGUIUpdateCoroutine;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            var receiver = m_InteractorSource != null && m_InteractorSource is IXRInteractor interactor
                ? interactor
                : GetComponentInParent<IXRInteractor>();
            if (!SetBoundInteractionReceiver(receiver))
            {
                XRLoggingUtils.LogWarning($"Could not find required interactor component on {gameObject}" +
                    " for which to provide affordance states.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Bind affordance provider state to <see cref="IXRInteractor"/> state and events.
        /// </summary>
        /// <param name="interactor">Receiver to bind events from.</param>
        /// <returns>Whether binding was successful.</returns>
        public bool SetBoundInteractionReceiver(IXRInteractor interactor)
        {
            ClearBindings();

            var isInteractorValid = interactor is Object unityObject && unityObject != null;
            if (isInteractorValid)
            {
                m_Interactor = interactor;

                if (m_Interactor is IXRHoverInteractor hoverInteractor)
                    m_HoverInteractor = hoverInteractor;

                if (m_Interactor is IXRSelectInteractor selectInteractor)
                    m_SelectInteractor = selectInteractor;

                if (m_Interactor is IXRInteractionStrengthInteractor interactionStrengthInteractor)
                    m_InteractionStrengthInteractor = interactionStrengthInteractor;

                if (m_Interactor is XRRayInteractor rayInteractor)
                    m_RayInteractor = rayInteractor;
            }
            else
            {
                m_Interactor = null;
                m_HoverInteractor = null;
                m_SelectInteractor = null;
                m_InteractionStrengthInteractor = null;
                m_RayInteractor = null;
            }

            m_HasRayInteractor = m_RayInteractor != null;
            m_HasHoverInteractor = m_HoverInteractor != null;
            m_HasSelectInteractor = m_SelectInteractor != null;
            m_HasInteractionStrengthInteractor = m_InteractionStrengthInteractor != null;
            m_IsIXRInteractor = m_Interactor != null;

            BindToProviders();
            return isInteractorValid;
        }

        /// <inheritdoc/>
        protected override void BindToProviders()
        {
            base.BindToProviders();

            m_IsBoundToInteractionEvents = m_Interactor is Object unityObject && unityObject != null;
            if (m_IsBoundToInteractionEvents)
            {
                m_Interactor.registered += OnRegistered;
                m_Interactor.unregistered += OnUnregistered;

                if (m_HasHoverInteractor)
                {
                    m_HoverInteractor.hoverEntered.AddListener(OnHoverEntered);
                    m_HoverInteractor.hoverExited.AddListener(OnHoverExited);
                }

                if (m_HasSelectInteractor)
                {
                    m_SelectInteractor.selectEntered.AddListener(OnSelectEntered);
                    m_SelectInteractor.selectExited.AddListener(OnSelectExited);
                }

                if (m_HasInteractionStrengthInteractor)
                {
                    AddBinding(m_InteractionStrengthInteractor.largestInteractionStrength.Subscribe(OnLargestInteractionStrengthChanged));
                }

                m_IsActivated = false;

                // Initialize field for whether the interactor is registered to distinguish between Idle and Disabled affordance states.
                // The registration status is not yet part of the base interactor interface, so we use a reasonable assumption here.
                if (m_Interactor is XRBaseInteractor baseInteractor)
                {
                    m_IsRegistered = baseInteractor.interactionManager != null && baseInteractor.interactionManager.IsRegistered(m_Interactor);
                }
                else if (m_Interactor is Behaviour behavior)
                {
                    m_IsRegistered = behavior.isActiveAndEnabled;
                }
                else
                {
                    m_IsRegistered = true;
                }

                if (m_UGUIUpdateCoroutine != null)
                {
                    StopCoroutine(m_UGUIUpdateCoroutine);
                }
                m_UGUIUpdateCoroutine = StartCoroutine(UIUpdateCheckCoroutine());
            }

            RefreshState();
        }

        /// <summary>
        /// Re-evaluates the current affordance state and triggers events for receivers if it changed.
        /// </summary>
        public void RefreshState()
        {
            UpdateAffordanceState(GenerateNewAffordanceState());
        }

        /// <inheritdoc/>
        protected override void ClearBindings()
        {
            base.ClearBindings();

            if (m_IsBoundToInteractionEvents)
            {
                m_Interactor.registered -= OnRegistered;
                m_Interactor.unregistered -= OnUnregistered;

                if (m_HasHoverInteractor)
                {
                    m_HoverInteractor.hoverEntered.RemoveListener(OnHoverEntered);
                    m_HoverInteractor.hoverExited.RemoveListener(OnHoverExited);
                }

                if (m_HasSelectInteractor)
                {
                    m_SelectInteractor.selectEntered.RemoveListener(OnSelectEntered);
                    m_SelectInteractor.selectExited.RemoveListener(OnSelectExited);
                }

                // No need to unsubscribe from interaction strength here since it was added to the binding group
                // and would have been cleared upon calling the base method.
            }

            foreach (var activateInteractable in m_BoundActivateInteractable)
            {
                if (activateInteractable == null)
                    continue;
                activateInteractable.activated.RemoveListener(OnActivated);
                activateInteractable.deactivated.RemoveListener(OnDeactivated);
            }
            m_BoundActivateInteractable.Clear();

            m_IsBoundToInteractionEvents = false;

            if (m_UGUIUpdateCoroutine != null)
            {
                StopCoroutine(m_UGUIUpdateCoroutine);
                m_UGUIUpdateCoroutine = null;
            }
        }

        /// <summary>
        /// Evaluates the state of the current interactor to generate a corresponding <see cref="AffordanceStateData"/>.
        /// </summary>
        /// <returns>Newly generated affordance state corresponding to the interactor state.</returns>
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

            if ((hasXRSelection || hasUISelection) && !m_IgnoreSelectEvents)
            {
                var transitionAmount = m_HasInteractionStrengthInteractor ? m_InteractionStrengthInteractor.largestInteractionStrength.Value : 1f;
                return new AffordanceStateData(AffordanceStateShortcuts.selected, transitionAmount);
            }

            if ((hasXRHover || hasUIHover) && !m_IgnoreHoverEvents)
            {
                var transitionAmount = m_HasInteractionStrengthInteractor ? m_InteractionStrengthInteractor.largestInteractionStrength.Value : 0f;
                return new AffordanceStateData(AffordanceStateShortcuts.hovered, transitionAmount);
            }

            return isRegistered && !isBlockedByGroup ? AffordanceStateShortcuts.idleState : AffordanceStateShortcuts.disabledState;
        }

        /// <summary>
        /// Callback triggered when the interactor is registered with the <see cref="XRInteractionManager"/>.
        /// Sets the internal <see cref="isRegistered"/> flag to true and refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="InteractorRegisteredEventArgs"/> callback args.</param>
        protected virtual void OnRegistered(InteractorRegisteredEventArgs args)
        {
            m_IsRegistered = true;
            RefreshState();
        }

        /// <summary>
        /// Callback triggered when the interactor is unregistered with the <see cref="XRInteractionManager"/>.
        /// Sets the internal <see cref="isRegistered"/> flag to false and refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="InteractorUnregisteredEventArgs"/> callback args.</param>
        protected virtual void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            m_IsRegistered = false;
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRHoverInteractor"/> when this interactor begins hovering.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="HoverEnterEventArgs"/> callback args.</param>
        /// <seealso cref="IXRHoverInteractor.hoverEntered"/>
        protected virtual void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (!m_IgnoreActivateEvents)
            {
                if (args.interactableObject is IXRActivateInteractable activateInteractable && !m_BoundActivateInteractable.Contains(activateInteractable))
                {
                    m_BoundActivateInteractable.Add(activateInteractable);

                    // Remove any stale listeners
                    activateInteractable.activated.RemoveListener(OnActivated);
                    activateInteractable.deactivated.RemoveListener(OnDeactivated);

                    activateInteractable.activated.AddListener(OnActivated);
                    activateInteractable.deactivated.AddListener(OnDeactivated);
                }
            }
            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRHoverInteractor"/> when this interactor is no longer hovering.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="HoverExitEventArgs"/> callback args.</param>
        /// <seealso cref="IXRHoverInteractor.hoverExited"/>
        protected virtual void OnHoverExited(HoverExitEventArgs args)
        {
            if (args.interactableObject is IXRActivateInteractable activateInteractable && m_BoundActivateInteractable.Contains(activateInteractable))
            {
                m_BoundActivateInteractable.Remove(activateInteractable);
                activateInteractable.activated.RemoveListener(OnActivated);
                activateInteractable.deactivated.RemoveListener(OnDeactivated);
            }

            RefreshState();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRSelectInteractor"/> when the first interactor begins selecting.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="SelectEnterEventArgs"/> callback args.</param>
        /// <seealso cref="IXRSelectInteractor.selectEntered"/>
        protected virtual void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (!m_IgnoreActivateEvents)
            {
                if (args.interactableObject is IXRActivateInteractable activateInteractable && !m_BoundActivateInteractable.Contains(activateInteractable))
                {
                    m_BoundActivateInteractable.Add(activateInteractable);

                    // Remove any stale listeners
                    activateInteractable.activated.RemoveListener(OnActivated);
                    activateInteractable.deactivated.RemoveListener(OnDeactivated);

                    activateInteractable.activated.AddListener(OnActivated);
                    activateInteractable.deactivated.AddListener(OnDeactivated);
                }
            }

            if (m_IgnoreSelectEvents || m_IgnoreXRInteractionEvents ||
                m_SelectClickAnimationMode != SelectClickAnimationMode.SelectEntered || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                RefreshState();
                return;
            }

            SelectedClickBehavior();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRSelectInteractor"/> when this interactor is no longer selecting.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="args"><see cref="SelectExitEventArgs"/> callback args.</param>
        /// <seealso cref="IXRSelectInteractor.selectExited"/>
        protected virtual void OnSelectExited(SelectExitEventArgs args)
        {
            // If for some reason hover exits first, we need to be able to remove listeners
            if (!hasXRHover && args.interactableObject is IXRActivateInteractable activateInteractable && m_BoundActivateInteractable.Contains(activateInteractable))
            {
                m_BoundActivateInteractable.Remove(activateInteractable);
                activateInteractable.activated.RemoveListener(OnActivated);
                activateInteractable.deactivated.RemoveListener(OnDeactivated);
            }

            if (m_IgnoreSelectEvents || m_IgnoreXRInteractionEvents ||
                m_SelectClickAnimationMode != SelectClickAnimationMode.SelectExited || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                RefreshState();
                return;
            }

            SelectedClickBehavior();
        }

        /// <summary>
        /// Callback triggered by <see cref="IXRInteractionStrengthInteractor"/> when the largest interaction strength of this interactor changes.
        /// Refreshes the affordance state.
        /// </summary>
        /// <param name="value">The new largest interaction strength value of this interactor.</param>
        protected virtual void OnLargestInteractionStrengthChanged(float value)
        {
            // If currently executing animation, do not update interaction strength state.
            if(m_SelectedClickAnimation != null || m_ActivatedClickAnimation != null)
                return;

            RefreshState();
        }

        void OnActivated(ActivateEventArgs args)
        {
            m_IsActivated = true;

            if (m_IgnoreActivateEvents || m_IgnoreXRInteractionEvents ||
                (m_ActivateClickAnimationMode != ActivateClickAnimationMode.Activated) || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                RefreshState();
                return;
            }

            ActivatedClickBehavior();
        }

        void OnDeactivated(DeactivateEventArgs args)
        {
            m_IsActivated = false;

            if (m_IgnoreActivateEvents || m_IgnoreXRInteractionEvents ||
                (m_ActivateClickAnimationMode != ActivateClickAnimationMode.Deactivated) || m_ClickAnimationDuration < Mathf.Epsilon)
            {
                RefreshState();
                return;
            }

            ActivatedClickBehavior();
        }

        /// <summary>
        /// Handles starting the selected click animation coroutine. Stops any previously started coroutine.
        /// </summary>
        protected virtual void SelectedClickBehavior()
        {
            if (m_SelectedClickAnimation != null)
                StopCoroutine(m_SelectedClickAnimation);

            m_SelectedClickAnimation = StartCoroutine(ClickAnimation(AffordanceStateShortcuts.selected, m_ClickAnimationDuration, () => m_SelectedClickAnimation = null));
        }

        /// <summary>
        /// Handles starting the activated click animation coroutine. Stops any previously started coroutine.
        /// </summary>
        protected virtual void ActivatedClickBehavior()
        {
            if (m_ActivatedClickAnimation != null)
                StopCoroutine(m_ActivatedClickAnimation);

            m_ActivatedClickAnimation = StartCoroutine(ClickAnimation(AffordanceStateShortcuts.activated, m_ClickAnimationDuration, () => m_ActivatedClickAnimation = null));
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

        IEnumerator UIUpdateCheckCoroutine()
        {
            while (true)
            {
                yield return null;

                // Check if UGUI is active on ray interactor
                if (m_HasRayInteractor)
                {
                    var newUIHovering = false;
                    var newUISelecting = false;
                    if (!(m_IgnoreHoverEvents && m_IgnoreSelectEvents) && m_RayInteractor.TryGetCurrentUIRaycastResult(out _, out var raycastEndpointIndex) && raycastEndpointIndex != 0)
                    {
                        if (!m_IgnoreSelectEvents && !m_IgnoreUGUISelect && m_RayInteractor.TryGetUIModel(out var uiModel) && uiModel.select)
                        {
                            newUISelecting = true;
                        }

                        if (!m_IgnoreHoverEvents && !m_IgnoreUGUIHover)
                        {
                            newUIHovering = true;
                        }
                    }

                    var stateChanged = newUIHovering != m_UIHovering || newUISelecting != m_UISelecting;

                    m_UIHovering = newUIHovering;
                    m_UISelecting = newUISelecting;

                    if (stateChanged)
                    {
                        RefreshState();
                    }
                }
            }
            // ReSharper disable once IteratorNeverReturns -- stopped when behavior is disabled.
        }
    }
}