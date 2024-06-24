using System;
using System.Collections.Generic;
#if UNITY_EDITOR && !UNITY_2021_3_OR_NEWER
using System.Text;
#endif
using Unity.Profiling;
#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using UnityEditor.Search;
#elif UNITY_EDITOR && !UNITY_2021_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;
#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.Interaction.Toolkit.AR;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The Interaction Manager acts as an intermediary between Interactors and Interactables.
    /// It is possible to have multiple Interaction Managers, each with their own valid set of Interactors and Interactables.
    /// Upon being enabled, both Interactors and Interactables register themselves with a valid Interaction Manager
    /// (if a specific one has not already been assigned in the inspector). The loaded scenes must have at least one Interaction Manager
    /// for Interactors and Interactables to be able to communicate.
    /// </summary>
    /// <remarks>
    /// Many of the methods on the Interactors and Interactables are designed to be called by this Interaction Manager
    /// rather than being called directly in order to maintain consistency between both targets of an interaction event.
    /// </remarks>
    /// <seealso cref="IXRInteractor"/>
    /// <seealso cref="IXRInteractable"/>
    [AddComponentMenu("XR/XR Interaction Manager", 11)]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_InteractionManager)]
    [HelpURL(XRHelpURLConstants.k_XRInteractionManager)]
    public partial class XRInteractionManager : MonoBehaviour
    {
        /// <summary>
        /// Calls the methods in its invocation list when an <see cref="IXRInteractionGroup"/> is registered.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractionGroupRegisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="RegisterInteractionGroup(IXRInteractionGroup)"/>
        /// <seealso cref="IXRInteractionGroup.registered"/>
        public event Action<InteractionGroupRegisteredEventArgs> interactionGroupRegistered;

        /// <summary>
        /// Calls the methods in its invocation list when an <see cref="IXRInteractionGroup"/> is unregistered.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractionGroupUnregisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="UnregisterInteractionGroup(IXRInteractionGroup)"/>
        /// <seealso cref="IXRInteractionGroup.unregistered"/>
        public event Action<InteractionGroupUnregisteredEventArgs> interactionGroupUnregistered;

        /// <summary>
        /// Calls the methods in its invocation list when an <see cref="IXRInteractor"/> is registered.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractorRegisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="RegisterInteractor(IXRInteractor)"/>
        /// <seealso cref="IXRInteractor.registered"/>
        public event Action<InteractorRegisteredEventArgs> interactorRegistered;

        /// <summary>
        /// Calls the methods in its invocation list when an <see cref="IXRInteractor"/> is unregistered.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractorUnregisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="UnregisterInteractor(IXRInteractor)"/>
        /// <seealso cref="IXRInteractor.unregistered"/>
        public event Action<InteractorUnregisteredEventArgs> interactorUnregistered;

        /// <summary>
        /// Calls the methods in its invocation list when an <see cref="IXRInteractable"/> is registered.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractableRegisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="RegisterInteractable(IXRInteractable)"/>
        /// <seealso cref="IXRInteractable.registered"/>
        public event Action<InteractableRegisteredEventArgs> interactableRegistered;

        /// <summary>
        /// Calls the methods in its invocation list when an <see cref="IXRInteractable"/> is unregistered.
        /// </summary>
        /// <remarks>
        /// The <see cref="InteractableUnregisteredEventArgs"/> passed to each listener is only valid while the event is invoked,
        /// do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="UnregisterInteractable(IXRInteractable)"/>
        /// <seealso cref="IXRInteractable.unregistered"/>
        public event Action<InteractableUnregisteredEventArgs> interactableUnregistered;


        /// <summary>
        /// Calls this method in its invocation list when an <see cref="IXRInteractionGroup"/> gains focus.
        /// </summary>
        public event Action<FocusEnterEventArgs> focusGained;

        /// <summary>
        /// Calls this method in its invocation list when an <see cref="IXRInteractionGroup"/> loses focus.
        /// </summary>
        public event Action<FocusExitEventArgs> focusLost;

        [SerializeField]
        [RequireInterface(typeof(IXRHoverFilter))]
        List<Object> m_StartingHoverFilters = new List<Object>();

        /// <summary>
        /// The hover filters that this object uses to automatically populate the <see cref="hoverFilters"/> List at
        /// startup (optional, may be empty).
        /// All objects in this list should implement the <see cref="IXRHoverFilter"/> interface.
        /// </summary>
        /// <remarks>
        /// To access and modify the hover filters used after startup, the <see cref="hoverFilters"/> List should
        /// be used instead.
        /// </remarks>
        /// <seealso cref="hoverFilters"/>
        public List<Object> startingHoverFilters
        {
            get => m_StartingHoverFilters;
            set => m_StartingHoverFilters = value;
        }

        readonly ExposedRegistrationList<IXRHoverFilter> m_HoverFilters = new ExposedRegistrationList<IXRHoverFilter> { bufferChanges = false };

        /// <summary>
        /// The list of global hover filters in this object.
        /// Used as additional hover validations for this manager.
        /// </summary>
        /// <remarks>
        /// While processing hover filters, all changes to this list don't have an immediate effect. These changes are
        /// buffered and applied when the processing is finished.
        /// Calling <see cref="IXRFilterList{T}.MoveTo"/> in this list will throw an exception when this list is being processed.
        /// </remarks>
        /// <seealso cref="ProcessHoverFilters"/>
        public IXRFilterList<IXRHoverFilter> hoverFilters => m_HoverFilters;

        [SerializeField]
        [RequireInterface(typeof(IXRSelectFilter))]
        List<Object> m_StartingSelectFilters = new List<Object>();

        /// <summary>
        /// The select filters that this object uses to automatically populate the <see cref="selectFilters"/> List at
        /// startup (optional, may be empty).
        /// All objects in this list should implement the <see cref="IXRSelectFilter"/> interface.
        /// </summary>
        /// <remarks>
        /// To access and modify the select filters used after startup, the <see cref="selectFilters"/> List should
        /// be used instead.
        /// </remarks>
        /// <seealso cref="selectFilters"/>
        public List<Object> startingSelectFilters
        {
            get => m_StartingSelectFilters;
            set => m_StartingSelectFilters = value;
        }

        readonly ExposedRegistrationList<IXRSelectFilter> m_SelectFilters = new ExposedRegistrationList<IXRSelectFilter> { bufferChanges = false };

        /// <summary>
        /// The list of global select filters in this object.
        /// Used as additional select validations for this manager.
        /// </summary>
        /// <remarks>
        /// While processing select filters, all changes to this list don't have an immediate effect. Theses changes are
        /// buffered and applied when the processing is finished.
        /// Calling <see cref="IXRFilterList{T}.MoveTo"/> in this list will throw an exception when this list is being processed.
        /// </remarks>
        /// <seealso cref="ProcessSelectFilters"/>
        public IXRFilterList<IXRSelectFilter> selectFilters => m_SelectFilters;

        /// <summary>
        /// (Read Only) The last <see cref="IXRFocusInteractable"/> that was focused by
        /// any <see cref="IXRInteractor"/>.
        /// </summary>
        public IXRFocusInteractable lastFocused { get; protected set; }

        /// <summary>
        /// (Read Only) List of enabled Interaction Manager instances.
        /// </summary>
        /// <remarks>
        /// Intended to be used by XR Interaction Debugger.
        /// </remarks>
        internal static List<XRInteractionManager> activeInteractionManagers { get; } = new List<XRInteractionManager>();

        /// <summary>
        /// Map of all registered objects to test for colliding.
        /// </summary>
        readonly Dictionary<Collider, IXRInteractable> m_ColliderToInteractableMap = new Dictionary<Collider, IXRInteractable>();

        /// <summary>
        /// Map of colliders and their associated <see cref="XRInteractableSnapVolume"/>.
        /// </summary>
        readonly Dictionary<Collider, XRInteractableSnapVolume> m_ColliderToSnapVolumes = new Dictionary<Collider, XRInteractableSnapVolume>();

        /// <summary>
        /// List of registered Interactors.
        /// </summary>
        readonly RegistrationList<IXRInteractor> m_Interactors = new RegistrationList<IXRInteractor>();

        /// <summary>
        /// List of registered Interaction Groups.
        /// </summary>
        readonly RegistrationList<IXRInteractionGroup> m_InteractionGroups = new RegistrationList<IXRInteractionGroup>();

        /// <summary>
        /// List of registered Interactables.
        /// </summary>
        readonly RegistrationList<IXRInteractable> m_Interactables = new RegistrationList<IXRInteractable>();

        /// <summary>
        /// Reusable list of Interactables for retrieving the current hovered Interactables of an Interactor.
        /// </summary>
        readonly List<IXRHoverInteractable> m_CurrentHovered = new List<IXRHoverInteractable>();

        /// <summary>
        /// Reusable list of Interactables for retrieving the current selected Interactables of an Interactor.
        /// </summary>
        readonly List<IXRSelectInteractable> m_CurrentSelected = new List<IXRSelectInteractable>();

        /// <summary>
        /// Map of Interactables that have the highest priority for selection in a frame.
        /// </summary>
        readonly Dictionary<IXRSelectInteractable, List<IXRTargetPriorityInteractor>> m_HighestPriorityTargetMap = new Dictionary<IXRSelectInteractable, List<IXRTargetPriorityInteractor>>();

        /// <summary>
        /// Pool of Target Priority Interactor lists. Used by m_HighestPriorityTargetMap.
        /// </summary>
        static readonly LinkedPool<List<IXRTargetPriorityInteractor>> s_TargetPriorityInteractorListPool = new LinkedPool<List<IXRTargetPriorityInteractor>>(() => new List<IXRTargetPriorityInteractor>(), actionOnRelease: list => list.Clear(), collectionCheck: false);

        /// <summary>
        /// Reusable list of valid targets for an Interactor.
        /// </summary>
        readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();

        /// <summary>
        /// Reusable set of valid targets for an Interactor.
        /// </summary>
        readonly HashSet<IXRInteractable> m_UnorderedValidTargets = new HashSet<IXRInteractable>();

        /// <summary>
        /// Set of all Interactors that are in an Interaction Group.
        /// </summary>
        readonly HashSet<IXRInteractor> m_InteractorsInGroup = new HashSet<IXRInteractor>();

        /// <summary>
        /// Set of all Interaction Groups that are in an Interaction Group.
        /// </summary>
        readonly HashSet<IXRInteractionGroup> m_GroupsInGroup = new HashSet<IXRInteractionGroup>();

        readonly List<XRBaseInteractable> m_DeprecatedValidTargets = new List<XRBaseInteractable>();
        readonly List<IXRInteractionGroup> m_ScratchInteractionGroups = new List<IXRInteractionGroup>();
        readonly List<IXRInteractor> m_ScratchInteractors = new List<IXRInteractor>();
        readonly List<IXRInteractable> m_ScratchInteractables = new List<IXRInteractable>();

        // Reusable event args
        readonly LinkedPool<FocusEnterEventArgs> m_FocusEnterEventArgs = new LinkedPool<FocusEnterEventArgs>(() => new FocusEnterEventArgs(), collectionCheck: false);
        readonly LinkedPool<FocusExitEventArgs> m_FocusExitEventArgs = new LinkedPool<FocusExitEventArgs>(() => new FocusExitEventArgs(), collectionCheck: false);
        readonly LinkedPool<SelectEnterEventArgs> m_SelectEnterEventArgs = new LinkedPool<SelectEnterEventArgs>(() => new SelectEnterEventArgs(), collectionCheck: false);
        readonly LinkedPool<SelectExitEventArgs> m_SelectExitEventArgs = new LinkedPool<SelectExitEventArgs>(() => new SelectExitEventArgs(), collectionCheck: false);
        readonly LinkedPool<HoverEnterEventArgs> m_HoverEnterEventArgs = new LinkedPool<HoverEnterEventArgs>(() => new HoverEnterEventArgs(), collectionCheck: false);
        readonly LinkedPool<HoverExitEventArgs> m_HoverExitEventArgs = new LinkedPool<HoverExitEventArgs>(() => new HoverExitEventArgs(), collectionCheck: false);
        readonly LinkedPool<InteractionGroupRegisteredEventArgs> m_InteractionGroupRegisteredEventArgs = new LinkedPool<InteractionGroupRegisteredEventArgs>(() => new InteractionGroupRegisteredEventArgs(), collectionCheck: false);
        readonly LinkedPool<InteractionGroupUnregisteredEventArgs> m_InteractionGroupUnregisteredEventArgs = new LinkedPool<InteractionGroupUnregisteredEventArgs>(() => new InteractionGroupUnregisteredEventArgs(), collectionCheck: false);
        readonly LinkedPool<InteractorRegisteredEventArgs> m_InteractorRegisteredEventArgs = new LinkedPool<InteractorRegisteredEventArgs>(() => new InteractorRegisteredEventArgs(), collectionCheck: false);
        readonly LinkedPool<InteractorUnregisteredEventArgs> m_InteractorUnregisteredEventArgs = new LinkedPool<InteractorUnregisteredEventArgs>(() => new InteractorUnregisteredEventArgs(), collectionCheck: false);
        readonly LinkedPool<InteractableRegisteredEventArgs> m_InteractableRegisteredEventArgs = new LinkedPool<InteractableRegisteredEventArgs>(() => new InteractableRegisteredEventArgs(), collectionCheck: false);
        readonly LinkedPool<InteractableUnregisteredEventArgs> m_InteractableUnregisteredEventArgs = new LinkedPool<InteractableUnregisteredEventArgs>(() => new InteractableUnregisteredEventArgs(), collectionCheck: false);

        static readonly ProfilerMarker s_PreprocessInteractorsMarker = new ProfilerMarker("XRI.PreprocessInteractors");
        static readonly ProfilerMarker s_ProcessInteractionStrengthMarker = new ProfilerMarker("XRI.ProcessInteractionStrength");
        static readonly ProfilerMarker s_ProcessInteractorsMarker = new ProfilerMarker("XRI.ProcessInteractors");
        static readonly ProfilerMarker s_ProcessInteractablesMarker = new ProfilerMarker("XRI.ProcessInteractables");
        static readonly ProfilerMarker s_UpdateGroupMemberInteractionsMarker = new ProfilerMarker("XRI.UpdateGroupMemberInteractions");
        internal static readonly ProfilerMarker s_GetValidTargetsMarker = new ProfilerMarker("XRI.GetValidTargets");
        static readonly ProfilerMarker s_FilterRegisteredValidTargetsMarker = new ProfilerMarker("XRI.FilterRegisteredValidTargets");
        internal static readonly ProfilerMarker s_EvaluateInvalidFocusMarker = new ProfilerMarker("XRI.EvaluateInvalidFocus");
        internal static readonly ProfilerMarker s_EvaluateInvalidSelectionsMarker = new ProfilerMarker("XRI.EvaluateInvalidSelections");
        internal static readonly ProfilerMarker s_EvaluateInvalidHoversMarker = new ProfilerMarker("XRI.EvaluateInvalidHovers");
        internal static readonly ProfilerMarker s_EvaluateValidSelectionsMarker = new ProfilerMarker("XRI.EvaluateValidSelections");
        internal static readonly ProfilerMarker s_EvaluateValidHoversMarker = new ProfilerMarker("XRI.EvaluateValidHovers");
        static readonly ProfilerMarker s_FocusEnterMarker = new ProfilerMarker("XRI.FocusEnter");
        static readonly ProfilerMarker s_FocusExitMarker = new ProfilerMarker("XRI.FocusExit");
        static readonly ProfilerMarker s_SelectEnterMarker = new ProfilerMarker("XRI.SelectEnter");
        static readonly ProfilerMarker s_SelectExitMarker = new ProfilerMarker("XRI.SelectExit");
        static readonly ProfilerMarker s_HoverEnterMarker = new ProfilerMarker("XRI.HoverEnter");
        static readonly ProfilerMarker s_HoverExitMarker = new ProfilerMarker("XRI.HoverExit");

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            // Setup the starting filters
            m_HoverFilters.RegisterReferences(m_StartingHoverFilters, this);
            m_SelectFilters.RegisterReferences(m_StartingSelectFilters, this);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (activeInteractionManagers.Count > 0)
            {
                var message = "There are multiple active and enabled XR Interaction Manager components in the loaded scenes." +
                    " This is supported, but may not be intended since interactors and interactables are not able to interact with those registered to a different manager." +
                    " You can use the <b>Window</b> > <b>Analysis</b> > <b>XR Interaction Debugger</b> window to verify the interactors and interactables registered with each.";
#if UNITY_EDITOR
                if (ComponentLocatorUtility<XRInteractionManager>.componentCache != null)
                {
                    message += " The default manager that interactors and interactables automatically register with when None is: " +
                        GetHierarchyPath(ComponentLocatorUtility<XRInteractionManager>.componentCache.gameObject);
                }
#endif

                Debug.LogWarning(message, this);
            }

            activeInteractionManagers.Add(this);
            Application.onBeforeRender += OnBeforeRender;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
            activeInteractionManagers.Remove(this);
            ClearPriorityForSelectionMap();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable -- ProfilerMarker.Begin with context object does not have Pure attribute
        protected virtual void Update()
        {
            ClearPriorityForSelectionMap();
            FlushRegistration();

            using (s_PreprocessInteractorsMarker.Auto())
                PreprocessInteractors(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

            foreach (var interactionGroup in m_InteractionGroups.registeredSnapshot)
            {
                if (!m_InteractionGroups.IsStillRegistered(interactionGroup) || m_GroupsInGroup.Contains(interactionGroup))
                    continue;

                using (s_EvaluateInvalidFocusMarker.Auto())
                    ClearInteractionGroupFocusInternal(interactionGroup);

                using (s_UpdateGroupMemberInteractionsMarker.Auto())
                    interactionGroup.UpdateGroupMemberInteractions();
            }

            foreach (var interactor in m_Interactors.registeredSnapshot)
            {
                if (!m_Interactors.IsStillRegistered(interactor) || m_InteractorsInGroup.Contains(interactor))
                    continue;

                using (s_GetValidTargetsMarker.Auto())
                    GetValidTargets(interactor, m_ValidTargets);

                // Cast to the abstract base classes to assist with backwards compatibility with existing user code.
                GetOfType(m_ValidTargets, m_DeprecatedValidTargets);

                var selectInteractor = interactor as IXRSelectInteractor;
                var hoverInteractor = interactor as IXRHoverInteractor;

                if (selectInteractor != null)
                {
                    using (s_EvaluateInvalidSelectionsMarker.Auto())
                        ClearInteractorSelectionInternal(selectInteractor, m_ValidTargets);
                }

                if (hoverInteractor != null)
                {
                    using (s_EvaluateInvalidHoversMarker.Auto())
                        ClearInteractorHoverInternal(hoverInteractor, m_ValidTargets, m_DeprecatedValidTargets);
                }

                if (selectInteractor != null)
                {
                    using (s_EvaluateValidSelectionsMarker.Auto())
                        InteractorSelectValidTargetsInternal(selectInteractor, m_ValidTargets, m_DeprecatedValidTargets);
                }

                if (hoverInteractor != null)
                {
                    using (s_EvaluateValidHoversMarker.Auto())
                        InteractorHoverValidTargetsInternal(hoverInteractor, m_ValidTargets, m_DeprecatedValidTargets);
                }
            }

            using (s_ProcessInteractionStrengthMarker.Auto())
                ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase.Dynamic);

            using (s_ProcessInteractorsMarker.Auto())
                ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
            using (s_ProcessInteractablesMarker.Auto())
                ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void LateUpdate()
        {
            FlushRegistration();

            using (s_ProcessInteractorsMarker.Auto())
                ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Late);
            using (s_ProcessInteractablesMarker.Auto())
                ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Late);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            FlushRegistration();

            using (s_ProcessInteractorsMarker.Auto())
                ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Fixed);
            using (s_ProcessInteractablesMarker.Auto())
                ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Fixed);
        }

        /// <summary>
        /// Delegate method used to register for "Just Before Render" input updates for VR devices.
        /// </summary>
        /// <seealso cref="Application"/>
        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderOrder)]
        protected virtual void OnBeforeRender()
        {
            FlushRegistration();

            using (s_ProcessInteractorsMarker.Auto())
                ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
            using (s_ProcessInteractablesMarker.Auto())
                ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
        }
        // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable

        /// <summary>
        /// Automatically called each frame to preprocess all interactors registered with this manager.
        /// </summary>
        /// <param name="updatePhase">The update phase.</param>
        /// <remarks>
        /// Please see the <see cref="XRInteractionUpdateOrder.UpdatePhase"/> documentation for more details on update order.
        /// </remarks>
        /// <seealso cref="IXRInteractor.PreprocessInteractor"/>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        protected virtual void PreprocessInteractors(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            foreach (var interactionGroup in m_InteractionGroups.registeredSnapshot)
            {
                if (!m_InteractionGroups.IsStillRegistered(interactionGroup) || m_GroupsInGroup.Contains(interactionGroup))
                    continue;

                interactionGroup.PreprocessGroupMembers(updatePhase);
            }

            foreach (var interactor in m_Interactors.registeredSnapshot)
            {
                if (!m_Interactors.IsStillRegistered(interactor) || m_InteractorsInGroup.Contains(interactor))
                    continue;

                interactor.PreprocessInteractor(updatePhase);
            }
        }

        /// <summary>
        /// Automatically called each frame to process all interactors registered with this manager.
        /// </summary>
        /// <param name="updatePhase">The update phase.</param>
        /// <remarks>
        /// Please see the <see cref="XRInteractionUpdateOrder.UpdatePhase"/> documentation for more details on update order.
        /// </remarks>
        /// <seealso cref="IXRInteractor.PreprocessInteractor"/>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        protected virtual void ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            foreach (var interactionGroup in m_InteractionGroups.registeredSnapshot)
            {
                if (!m_InteractionGroups.IsStillRegistered(interactionGroup) || m_GroupsInGroup.Contains(interactionGroup))
                    continue;

                interactionGroup.ProcessGroupMembers(updatePhase);
            }

            foreach (var interactor in m_Interactors.registeredSnapshot)
            {
                if (!m_Interactors.IsStillRegistered(interactor) || m_InteractorsInGroup.Contains(interactor))
                    continue;

                interactor.ProcessInteractor(updatePhase);
            }
        }

        /// <summary>
        /// Automatically called each frame to process all interactables registered with this manager.
        /// </summary>
        /// <param name="updatePhase">The update phase.</param>
        /// <remarks>
        /// Please see the <see cref="XRInteractionUpdateOrder.UpdatePhase"/> documentation for more details on update order.
        /// </remarks>
        /// <seealso cref="IXRInteractable.ProcessInteractable"/>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        protected virtual void ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            foreach (var interactable in m_Interactables.registeredSnapshot)
            {
                if (!m_Interactables.IsStillRegistered(interactable))
                    continue;

                interactable.ProcessInteractable(updatePhase);
            }
        }

        /// <summary>
        /// Automatically called each frame to process interaction strength of interactables and interactors registered with this manager.
        /// </summary>
        /// <param name="updatePhase">The update phase.</param>
        /// <seealso cref="IXRInteractionStrengthInteractable.ProcessInteractionStrength"/>
        /// <seealso cref="IXRInteractionStrengthInteractor.ProcessInteractionStrength"/>
        /// <seealso cref="XRInteractionUpdateOrder.UpdatePhase"/>
        protected virtual void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            // Unlike other processing, with interaction strength, interactables are processed before interactors
            // since interactables with the ability to be poked dictate the interaction strength. After the
            // interaction strength of interactables are computed for this frame, they are gathered into
            // the interactor for use in affordances or within the process step.
            foreach (var interactable in m_Interactables.registeredSnapshot)
            {
                if (!m_Interactables.IsStillRegistered(interactable))
                    continue;

                if (interactable is IXRInteractionStrengthInteractable interactionStrengthInteractable)
                    interactionStrengthInteractable.ProcessInteractionStrength(updatePhase);
            }

            foreach (var interactor in m_Interactors.registeredSnapshot)
            {
                if (!m_Interactors.IsStillRegistered(interactor))
                    continue;

                if (interactor is IXRInteractionStrengthInteractor interactionStrengthInteractor)
                    interactionStrengthInteractor.ProcessInteractionStrength(updatePhase);
            }
        }

        /// <summary>
        /// Whether the given Interactor can hover the given Interactable.
        /// You can extend this method to add global hover validations by code.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns whether the given Interactor can hover the given Interactable.</returns>
        /// <remarks>
        /// You can also extend the global hover validations without needing to create a derived class by adding hover
        /// filters to this object (see <see cref="startingHoverFilters"/> and <see cref="hoverFilters"/>).
        /// </remarks>
        /// <seealso cref="IsHoverPossible"/>
        public virtual bool CanHover(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return interactor.isHoverActive && IsHoverPossible(interactor, interactable);
        }

        /// <summary>
        /// Whether the given Interactor would be able to hover the given Interactable if the Interactor were in a state where it could hover.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns whether the given Interactor would be able to hover the given Interactable if the Interactor were in a state where it could hover.</returns>
        /// <seealso cref="CanHover"/>
        public bool IsHoverPossible(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return HasInteractionLayerOverlap(interactor, interactable) && ProcessHoverFilters(interactor, interactable) &&
                interactor.CanHover(interactable) && interactable.IsHoverableBy(interactor);
        }

        /// <summary>
        /// Whether the given Interactor can select the given Interactable.
        /// You can extend this method to add global select validations by code.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns whether the given Interactor can select the given Interactable.</returns>
        /// <remarks>
        /// You can also extend the global select validations without needing to create a derived class by adding select
        /// filters to this object (see <see cref="startingSelectFilters"/> and <see cref="selectFilters"/>).
        /// </remarks>
        /// <seealso cref="IsSelectPossible"/>
        public virtual bool CanSelect(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return interactor.isSelectActive && IsSelectPossible(interactor, interactable);
        }

        /// <summary>
        /// Whether the given Interactor would be able to select the given Interactable if the Interactor were in a state where it could select.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns whether the given Interactor would be able to select the given Interactable if the Interactor were in a state where it could select.</returns>
        /// <seealso cref="CanSelect"/>
        public bool IsSelectPossible(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return HasInteractionLayerOverlap(interactor, interactable) && ProcessSelectFilters(interactor, interactable) &&
                interactor.CanSelect(interactable) && interactable.IsSelectableBy(interactor);
        }

        /// <summary>
        /// Whether the given Interactor can gain focus of the given Interactable.
        /// You can extend this method to add global focus validations by code.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns whether the given Interactor can gain focus of the given Interactable.</returns>
        /// <seealso cref="IsFocusPossible"/>
        public virtual bool CanFocus(IXRInteractor interactor, IXRFocusInteractable interactable)
        {
            return IsFocusPossible(interactor, interactable);
        }

        /// <summary>
        /// Whether the given Interactor would be able gain focus of the given Interactable if the Interactor were in a state where it could focus.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns whether the given Interactor would be able to gain focus of the given Interactable if the Interactor were in a state where it could focus.</returns>
        /// <seealso cref="CanSelect"/>
        public bool IsFocusPossible(IXRInteractor interactor, IXRFocusInteractable interactable)
        {
            return interactable.canFocus && HasInteractionLayerOverlap(interactor, interactable);
        }

        /// <summary>
        /// Registers a new Interaction Group to be processed.
        /// </summary>
        /// <param name="interactionGroup">The Interaction Group to be registered.</param>
        public virtual void RegisterInteractionGroup(IXRInteractionGroup interactionGroup)
        {
            IXRInteractionGroup containingGroup = null;
            if (interactionGroup is IXRGroupMember groupMember)
                containingGroup = groupMember.containingGroup;

            if (containingGroup != null && !IsRegistered(containingGroup))
            {
                Debug.LogError($"Cannot register {interactionGroup} with Interaction Manager before its containing " +
                               "Interaction Group is registered.", this);
                return;
            }

            if (m_InteractionGroups.Register(interactionGroup))
            {
                if (containingGroup != null)
                    m_GroupsInGroup.Add(interactionGroup);

                using (m_InteractionGroupRegisteredEventArgs.Get(out var args))
                {
                    args.manager = this;
                    args.interactionGroupObject = interactionGroup;
                    args.containingGroupObject = containingGroup;
                    OnRegistered(args);
                }
            }
        }

        /// <summary>
        /// Automatically called when an Interaction Group is registered with this Interaction Manager.
        /// Notifies the Interaction Group, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">Event data containing the registered Interaction Group.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="RegisterInteractionGroup(IXRInteractionGroup)"/>
        protected virtual void OnRegistered(InteractionGroupRegisteredEventArgs args)
        {
            Debug.Assert(args.manager == this, this);

            args.interactionGroupObject.OnRegistered(args);
            interactionGroupRegistered?.Invoke(args);
        }

        /// <summary>
        /// Unregister an Interaction Group so it is no longer processed.
        /// </summary>
        /// <param name="interactionGroup">The Interaction Group to be unregistered.</param>
        public virtual void UnregisterInteractionGroup(IXRInteractionGroup interactionGroup)
        {
            if (!IsRegistered(interactionGroup))
                return;

            interactionGroup.OnBeforeUnregistered();

            // Make sure no registered interactors or groups still reference this group
            if (m_InteractionGroups.flushedCount > 0)
            {
                m_InteractionGroups.GetRegisteredItems(m_ScratchInteractionGroups);
                foreach (var group in m_ScratchInteractionGroups)
                {
                    if (group is IXRGroupMember groupMember && groupMember.containingGroup == interactionGroup)
                    {
                        Debug.LogError($"Cannot unregister {interactionGroup} with Interaction Manager before its " +
                            "Group Members have been re-registered as not part of the Group.", this);
                        return;
                    }
                }
            }

            if (m_Interactors.flushedCount > 0)
            {
                m_Interactors.GetRegisteredItems(m_ScratchInteractors);
                foreach (var interactor in m_ScratchInteractors)
                {
                    if (interactor is IXRGroupMember groupMember && groupMember.containingGroup == interactionGroup)
                    {
                        Debug.LogError($"Cannot unregister {interactionGroup} with Interaction Manager before its " +
                            "Group Members have been re-registered as not part of the Group.", this);
                        return;
                    }
                }
            }

            if (m_InteractionGroups.Unregister(interactionGroup))
            {
                m_GroupsInGroup.Remove(interactionGroup);
                using (m_InteractionGroupUnregisteredEventArgs.Get(out var args))
                {
                    args.manager = this;
                    args.interactionGroupObject = interactionGroup;
                    OnUnregistered(args);
                }
            }
        }

        /// <summary>
        /// Automatically called when an Interaction Group is unregistered from this Interaction Manager.
        /// Notifies the Interaction Group, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">Event data containing the unregistered Interaction Group.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="UnregisterInteractionGroup(IXRInteractionGroup)"/>
        protected virtual void OnUnregistered(InteractionGroupUnregisteredEventArgs args)
        {
            Debug.Assert(args.manager == this, this);

            args.interactionGroupObject.OnUnregistered(args);
            interactionGroupUnregistered?.Invoke(args);
        }

        /// <summary>
        /// Gets all currently registered Interaction groups
        /// </summary>
        /// <param name="interactionGroups">The list that will filled with all of the registered interaction groups</param>
        public void GetInteractionGroups(List<IXRInteractionGroup> interactionGroups)
        {
            m_InteractionGroups.GetRegisteredItems(interactionGroups);
        }

        /// <summary>
        /// Gets the registered Interaction Group with the given name.
        /// </summary>
        /// <param name="groupName">The name of the interaction group to retrieve.</param>
        /// <returns>Returns the interaction group with matching name, or null if none were found.</returns>
        /// <seealso cref="IXRInteractionGroup.groupName"/>
        public IXRInteractionGroup GetInteractionGroup(string groupName)
        {
            foreach (var interactionGroup in m_InteractionGroups.registeredSnapshot)
            {
                if (interactionGroup.groupName == groupName)
                    return interactionGroup;
            }

            return null;
        }

        /// <summary>
        /// Registers a new Interactor to be processed.
        /// </summary>
        /// <param name="interactor">The Interactor to be registered.</param>
        public virtual void RegisterInteractor(IXRInteractor interactor)
        {
            IXRInteractionGroup containingGroup = null;
            if (interactor is IXRGroupMember groupMember)
                containingGroup = groupMember.containingGroup;

            if (containingGroup != null && !IsRegistered(containingGroup))
            {
                Debug.LogError($"Cannot register {interactor} with Interaction Manager before its containing " +
                               "Interaction Group is registered.", this);
                return;
            }

            if (m_Interactors.Register(interactor))
            {
                if (containingGroup != null)
                    m_InteractorsInGroup.Add(interactor);

                using (m_InteractorRegisteredEventArgs.Get(out var args))
                {
                    args.manager = this;
                    args.interactorObject = interactor;
                    args.containingGroupObject = containingGroup;
                    OnRegistered(args);
                }
            }
        }

        /// <summary>
        /// Automatically called when an Interactor is registered with this Interaction Manager.
        /// Notifies the Interactor, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">Event data containing the registered Interactor.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="RegisterInteractor(IXRInteractor)"/>
        protected virtual void OnRegistered(InteractorRegisteredEventArgs args)
        {
            Debug.Assert(args.manager == this, this);

            args.interactorObject.OnRegistered(args);
            interactorRegistered?.Invoke(args);
        }

        /// <summary>
        /// Unregister an Interactor so it is no longer processed.
        /// </summary>
        /// <param name="interactor">The Interactor to be unregistered.</param>
        public virtual void UnregisterInteractor(IXRInteractor interactor)
        {
            if (!IsRegistered(interactor))
                return;

            var interactorTransform = interactor.transform;

            // We suppress canceling focus for inactive interactors vs. destroyed interactors as that is used as a method of mediation
            if (interactorTransform == null || interactorTransform.gameObject.activeSelf)
                CancelInteractorFocusInternal(interactor);

            if (interactor is IXRSelectInteractor selectInteractor)
                CancelInteractorSelectionInternal(selectInteractor);

            if (interactor is IXRHoverInteractor hoverInteractor)
                CancelInteractorHoverInternal(hoverInteractor);

            if (m_Interactors.Unregister(interactor))
            {
                m_InteractorsInGroup.Remove(interactor);
                using (m_InteractorUnregisteredEventArgs.Get(out var args))
                {
                    args.manager = this;
                    args.interactorObject = interactor;
                    OnUnregistered(args);
                }
            }
        }

        /// <summary>
        /// Automatically called when an Interactor is unregistered from this Interaction Manager.
        /// Notifies the Interactor, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">Event data containing the unregistered Interactor.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="UnregisterInteractor(IXRInteractor)"/>
        protected virtual void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            Debug.Assert(args.manager == this, this);

            args.interactorObject.OnUnregistered(args);
            interactorUnregistered?.Invoke(args);
        }

        /// <summary>
        /// Registers a new Interactable to be processed.
        /// </summary>
        /// <param name="interactable">The Interactable to be registered.</param>
        public virtual void RegisterInteractable(IXRInteractable interactable)
        {
            if (m_Interactables.Register(interactable))
            {
                foreach (var interactableCollider in interactable.colliders)
                {
                    if (interactableCollider == null)
                        continue;

                    // Add the association for a fast lookup which maps from Collider to Interactable.
                    // Warn if the same Collider is already used by another registered Interactable
                    // since the lookup will only return the earliest registered rather than a list of all.
                    // The warning is suppressed in the case of gesture interactables since it's common
                    // to compose multiple on the same GameObject.
                    if (!m_ColliderToInteractableMap.TryGetValue(interactableCollider, out var associatedInteractable))
                    {
                        m_ColliderToInteractableMap.Add(interactableCollider, interactable);
                    }
#if AR_FOUNDATION_PRESENT
                    else if (!(interactable is ARBaseGestureInteractable && associatedInteractable is ARBaseGestureInteractable))
#else
                    else
#endif
                    {
                        Debug.LogWarning("A collider used by an Interactable object is already registered with another Interactable object." +
                            $" The {interactableCollider} will remain associated with {associatedInteractable}, which was registered before {interactable}." +
                            $" The value returned by {nameof(XRInteractionManager)}.{nameof(TryGetInteractableForCollider)} will be the first association.",
                            interactable as Object);
                    }
                }

                using (m_InteractableRegisteredEventArgs.Get(out var args))
                {
                    args.manager = this;
                    args.interactableObject = interactable;
                    OnRegistered(args);
                }
            }
        }

        /// <summary>
        /// Automatically called when an Interactable is registered with this Interaction Manager.
        /// Notifies the Interactable, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">Event data containing the registered Interactable.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="RegisterInteractable(IXRInteractable)"/>
        protected virtual void OnRegistered(InteractableRegisteredEventArgs args)
        {
            Debug.Assert(args.manager == this, this);

            args.interactableObject.OnRegistered(args);
            interactableRegistered?.Invoke(args);
        }

        /// <summary>
        /// Unregister an Interactable so it is no longer processed.
        /// </summary>
        /// <param name="interactable">The Interactable to be unregistered.</param>
        public virtual void UnregisterInteractable(IXRInteractable interactable)
        {
            if (!IsRegistered(interactable))
                return;

            if (interactable is IXRFocusInteractable focusable)
                CancelInteractableFocusInternal(focusable);

            if (interactable is IXRSelectInteractable selectable)
                CancelInteractableSelectionInternal(selectable);

            if (interactable is IXRHoverInteractable hoverable)
                CancelInteractableHoverInternal(hoverable);

            if (m_Interactables.Unregister(interactable))
            {
                // This makes the assumption that the list of Colliders has not been changed after
                // the Interactable is registered. If any were removed afterward, those would remain
                // in the dictionary.
                foreach (var interactableCollider in interactable.colliders)
                {
                    if (interactableCollider == null)
                        continue;

                    if (m_ColliderToInteractableMap.TryGetValue(interactableCollider, out var associatedInteractable) && associatedInteractable == interactable)
                        m_ColliderToInteractableMap.Remove(interactableCollider);
                }

                using (m_InteractableUnregisteredEventArgs.Get(out var args))
                {
                    args.manager = this;
                    args.interactableObject = interactable;
                    OnUnregistered(args);
                }
            }
        }

        /// <summary>
        /// Automatically called when an Interactable is unregistered from this Interaction Manager.
        /// Notifies the Interactable, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="args">Event data containing the unregistered Interactable.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="UnregisterInteractable(IXRInteractable)"/>
        protected virtual void OnUnregistered(InteractableUnregisteredEventArgs args)
        {
            Debug.Assert(args.manager == this, this);

            args.interactableObject.OnUnregistered(args);
            interactableUnregistered?.Invoke(args);
        }

        /// <summary>
        /// Registers a new snap volume to associate the snap collider and interactable.
        /// </summary>
        /// <param name="snapVolume">The snap volume to be registered.</param>
        /// <seealso cref="UnregisterSnapVolume"/>
        public void RegisterSnapVolume(XRInteractableSnapVolume snapVolume)
        {
            if (snapVolume == null)
                return;

            var snapCollider = snapVolume.snapCollider;
            if (snapCollider == null)
                return;

            if (!m_ColliderToSnapVolumes.TryGetValue(snapCollider, out var associatedSnapVolume))
            {
                m_ColliderToSnapVolumes.Add(snapCollider, snapVolume);
            }
            else
            {
                Debug.LogWarning("A collider used by a snap volume component is already registered with another snap volume component." +
                    $" The {snapCollider} will remain associated with {associatedSnapVolume}, which was registered before {snapVolume}." +
                    $" The value returned by {nameof(XRInteractionManager)}.{nameof(TryGetInteractableForCollider)} will be the first association.",
                    snapVolume);
            }
        }

        /// <summary>
        /// Unregister the snap volume so it is no longer associated with the snap collider or interactable.
        /// </summary>
        /// <param name="snapVolume">The snap volume to be unregistered.</param>
        /// <seealso cref="RegisterSnapVolume"/>
        public void UnregisterSnapVolume(XRInteractableSnapVolume snapVolume)
        {
            if (snapVolume == null)
                return;

            // This makes the assumption that the snap collider has not been changed after
            // the snap volume is registered.
            var snapCollider = snapVolume.snapCollider;
            if (snapCollider == null)
                return;

            if (m_ColliderToSnapVolumes.TryGetValue(snapCollider, out var associatedSnapVolume) && associatedSnapVolume == snapVolume)
                m_ColliderToSnapVolumes.Remove(snapCollider);
        }

        /// <summary>
        /// Returns all registered Interaction Groups into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive registered Interaction Groups.</param>
        /// <remarks>
        /// This method populates the list with the registered Interaction Groups at the time the
        /// method is called. It is not a live view, meaning Interaction Groups
        /// registered or unregistered afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        public void GetRegisteredInteractionGroups(List<IXRInteractionGroup> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            m_InteractionGroups.GetRegisteredItems(results);
        }

        /// <summary>
        /// Returns all registered Interactors into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive registered Interactors.</param>
        /// <remarks>
        /// This method populates the list with the registered Interactors at the time the
        /// method is called. It is not a live view, meaning Interactors
        /// registered or unregistered afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        /// <seealso cref="GetRegisteredInteractables(List{IXRInteractable})"/>
        public void GetRegisteredInteractors(List<IXRInteractor> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            m_Interactors.GetRegisteredItems(results);
        }

        /// <summary>
        /// Returns all registered Interactables into List <paramref name="results"/>.
        /// </summary>
        /// <param name="results">List to receive registered Interactables.</param>
        /// <remarks>
        /// This method populates the list with the registered Interactables at the time the
        /// method is called. It is not a live view, meaning Interactables
        /// registered or unregistered afterward will not be reflected in the
        /// results of this method.
        /// Clears <paramref name="results"/> before adding to it.
        /// </remarks>
        /// <seealso cref="GetRegisteredInteractors(List{IXRInteractor})"/>
        public void GetRegisteredInteractables(List<IXRInteractable> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            m_Interactables.GetRegisteredItems(results);
        }

        /// <summary>
        /// Checks whether the <paramref name="interactionGroup"/> is registered with this Interaction Manager.
        /// </summary>
        /// <param name="interactionGroup">The Interaction Group to check.</param>
        /// <returns>Returns <see langword="true"/> if registered. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="RegisterInteractionGroup(IXRInteractionGroup)"/>
        public bool IsRegistered(IXRInteractionGroup interactionGroup)
        {
            return m_InteractionGroups.IsRegistered(interactionGroup);
        }

        /// <summary>
        /// Checks whether the <paramref name="interactor"/> is registered with this Interaction Manager.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <returns>Returns <see langword="true"/> if registered. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="RegisterInteractor(IXRInteractor)"/>
        public bool IsRegistered(IXRInteractor interactor)
        {
            return m_Interactors.IsRegistered(interactor);
        }

        /// <summary>
        /// Checks whether the <paramref name="interactable"/> is registered with this Interaction Manager.
        /// </summary>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns <see langword="true"/> if registered. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="RegisterInteractable(IXRInteractable)"/>
        public bool IsRegistered(IXRInteractable interactable)
        {
            return m_Interactables.IsRegistered(interactable);
        }

        /// <summary>
        /// Gets the Interactable a specific <see cref="Collider"/> is attached to.
        /// </summary>
        /// <param name="interactableCollider">The collider of the Interactable to retrieve.</param>
        /// <param name="interactable">The returned Interactable associated with the collider.</param>
        /// <returns>Returns <see langword="true"/> if an Interactable was associated with the collider. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryGetInteractableForCollider(Collider interactableCollider, out IXRInteractable interactable)
        {
            interactable = null;
            if (interactableCollider == null)
                return false;

            // Try direct association, and then fallback to snap volume association
            var hasDirectAssociation = m_ColliderToInteractableMap.TryGetValue(interactableCollider, out interactable);
            if (!hasDirectAssociation)
            {
                if (m_ColliderToSnapVolumes.TryGetValue(interactableCollider, out var snapVolume) && snapVolume != null)
                    interactable = snapVolume.interactable;
            }

            return interactable != null && (!(interactable is Object unityObject) || unityObject != null);
        }

        /// <summary>
        /// Gets the Interactable a specific <see cref="Collider"/> is attached to.
        /// </summary>
        /// <param name="interactableCollider">The collider of the Interactable to retrieve.</param>
        /// <param name="interactable">The returned Interactable associated with the collider.</param>
        /// <param name="snapVolume">The returned snap volume associated with the collider.</param>
        /// <returns>Returns <see langword="true"/> if an Interactable was associated with the collider. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryGetInteractableForCollider(Collider interactableCollider, out IXRInteractable interactable, out XRInteractableSnapVolume snapVolume)
        {
            interactable = null;
            snapVolume = null;
            if (interactableCollider == null)
                return false;

            // Populate both out params
            var hasDirectAssociation = m_ColliderToInteractableMap.TryGetValue(interactableCollider, out interactable);
            if (m_ColliderToSnapVolumes.TryGetValue(interactableCollider, out snapVolume) && snapVolume != null)
            {
                if (hasDirectAssociation)
                {
                    // Detect mismatch, ignore the snap volume
                    if (snapVolume.interactable != interactable)
                        snapVolume = null;
                }
                else
                {
                    interactable = snapVolume.interactable;
                }
            }

            return interactable != null && (!(interactable is Object unityObject) || unityObject != null);
        }

        /// <summary>
        /// Gets whether the given Interactable is the highest priority candidate for selection in this frame, useful for
        /// custom feedback.
        /// Only <see cref="IXRTargetPriorityInteractor"/>s that are configured to monitor Targets will be considered.
        /// </summary>
        /// <param name="target">The Interactable to check if it's the highest priority candidate for selection.</param>
        /// <param name="interactors">(Optional) Returns the list of Interactors where the given Interactable has the highest priority for selection.</param>
        /// <returns>Returns <see langword="true"/> if the given Interactable is the highest priority candidate for selection. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// Clears <paramref name="interactors"/> before adding to it.
        /// </remarks>
        public bool IsHighestPriorityTarget(IXRSelectInteractable target, List<IXRTargetPriorityInteractor> interactors = null)
        {
            if (!m_HighestPriorityTargetMap.TryGetValue(target, out var targetPriorityInteractors))
                return false;

            if (interactors == null)
                return true;

            interactors.Clear();
            interactors.AddRange(targetPriorityInteractors);
            return true;
        }

        /// <summary>
        /// Retrieves the list of Interactables that the given Interactor could possibly interact with this frame.
        /// This list is sorted by priority (with highest priority first), and will only contain Interactables
        /// that are registered with this Interaction Manager.
        /// </summary>
        /// <param name="interactor">The Interactor to get valid targets for.</param>
        /// <param name="targets">The results list to populate with Interactables that are valid for selection, hover, or focus.</param>
        /// <remarks>
        /// Unity expects the <paramref name="interactor"/>'s implementation of <see cref="IXRInteractor.GetValidTargets"/> to clear <paramref name="targets"/> before adding to it.
        /// </remarks>
        /// <seealso cref="IXRInteractor.GetValidTargets"/>
        public void GetValidTargets(IXRInteractor interactor, List<IXRInteractable> targets)
        {
            targets.Clear();
            interactor.GetValidTargets(targets);

            // To attempt to be backwards compatible with user scripts that have not been upgraded to use the interfaces,
            // call the old method to let existing code modify the list.
            if (interactor is XRBaseInteractor baseInteractor)
            {
                m_DeprecatedValidTargets.Clear();
                GetOfType(targets, m_DeprecatedValidTargets);
                if (targets.Count == m_DeprecatedValidTargets.Count)
                {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                    baseInteractor.GetValidTargets(m_DeprecatedValidTargets);
#pragma warning restore 618

                    GetOfType(m_DeprecatedValidTargets, targets);
                }
            }

            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable -- ProfilerMarker.Begin with context object does not have Pure attribute
            using (s_FilterRegisteredValidTargetsMarker.Auto())
                RemoveAllUnregistered(this, targets);
        }

        /// <summary>
        /// Removes all the Interactables from the given list that are not being handled by the manager.
        /// </summary>
        /// <param name="manager">The Interaction Manager to check registration against.</param>
        /// <param name="interactables">List of elements that will be filtered to exclude those not registered.</param>
        /// <returns>Returns the number of elements removed from the list.</returns>
        /// <remarks>
        /// Does not modify the manager at all, just the list.
        /// </remarks>
        internal static int RemoveAllUnregistered(XRInteractionManager manager, List<IXRInteractable> interactables)
        {
            var numRemoved = 0;
            for (var i = interactables.Count - 1; i >= 0; --i)
            {
                if (!manager.m_Interactables.IsRegistered(interactables[i]))
                {
                    interactables.RemoveAt(i);
                    ++numRemoved;
                }
            }

            return numRemoved;
        }

        /// <summary>
        /// Automatically called each frame during Update to clear the focus of the interaction group if necessary due to current conditions.
        /// </summary>
        /// <param name="interactionGroup">The interaction group to potentially exit its focus state.</param>
        protected virtual void ClearInteractionGroupFocus(IXRInteractionGroup interactionGroup)
        {
            // We want to unfocus whenever we select 'nothing'
            // If nothing is focused, then we are not in that scenario.
            // Otherwise, we check for selection activation with lack of selected object.
            var focusInteractor = interactionGroup.focusInteractor;
            var focusInteractable = interactionGroup.focusInteractable;
            if (focusInteractor == null || focusInteractable == null)
                return;

            var cleared = false;

            var selectInteractor = focusInteractor as IXRSelectInteractor;
            var selectInteractable = focusInteractable as IXRSelectInteractable;
            
            if (selectInteractor != null)
                cleared = (selectInteractor.isSelectActive && !selectInteractor.IsSelecting(selectInteractable));

            if (cleared || !CanFocus(focusInteractor, focusInteractable))
            {
                FocusExitInternal(interactionGroup, interactionGroup.focusInteractable);
            }
        }

        internal void ClearInteractionGroupFocusInternal(IXRInteractionGroup interactionGroup)
        {
            ClearInteractionGroupFocus(interactionGroup);
        }

        void CancelInteractorFocusInternal(IXRInteractor interactor)
        {
            var asGroupMember = interactor as IXRGroupMember;
            var group = asGroupMember?.containingGroup;

            if (group != null && group.focusInteractable != null)
            {
                FocusCancelInternal(group, group.focusInteractable);
            }
        }

        /// <summary>
        /// Automatically called when an Interactable is unregistered to cancel the focus of the Interactable if necessary.
        /// </summary>
        /// <param name="interactable">The Interactable to potentially exit its focus state due to cancellation.</param>
        public virtual void CancelInteractableFocus(IXRFocusInteractable interactable)
        {
            for (var i = interactable.interactionGroupsFocusing.Count - 1; i >= 0; --i)
            {
                FocusCancelInternal(interactable.interactionGroupsFocusing[i], interactable);
            }
        }

        void CancelInteractableFocusInternal(IXRFocusInteractable interactable)
        {

            CancelInteractableFocus(interactable);
        }

        /// <summary>
        /// Automatically called each frame during Update to clear the selection of the Interactor if necessary due to current conditions.
        /// </summary>
        /// <param name="interactor">The Interactor to potentially exit its selection state.</param>
        /// <param name="validTargets">The list of interactables that this Interactor could possibly interact with this frame.</param>
        /// <seealso cref="ClearInteractorHover(IXRHoverInteractor, List{IXRInteractable})"/>
        protected virtual void ClearInteractorSelection(IXRSelectInteractor interactor, List<IXRInteractable> validTargets)
        {
            if (interactor.interactablesSelected.Count == 0)
                return;

            m_CurrentSelected.Clear();
            m_CurrentSelected.AddRange(interactor.interactablesSelected);

            // Performance optimization of the Contains checks by putting the valid targets into a HashSet.
            // Some Interactors like ARGestureInteractor can have hundreds of valid Interactables
            // since they will add most ARBaseGestureInteractable instances.
            m_UnorderedValidTargets.Clear();
            if (validTargets.Count > 0)
            {
                foreach (var target in validTargets)
                {
                    m_UnorderedValidTargets.Add(target);
                }
            }

            for (var i = m_CurrentSelected.Count - 1; i >= 0; --i)
            {
                var interactable = m_CurrentSelected[i];
                // Selection, unlike hover, can control whether the interactable has to continue being a valid target
                // to automatically cause it to be deselected.
                if (!CanSelect(interactor, interactable) || (!interactor.keepSelectedTargetValid && !m_UnorderedValidTargets.Contains(interactable)))
                    SelectExitInternal(interactor, interactable);
            }
        }

        internal void ClearInteractorSelectionInternal(IXRSelectInteractor interactor, List<IXRInteractable> validTargets)
        {
            ClearInteractorSelection(interactor, validTargets);
            if (interactor is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                ClearInteractorSelection(baseInteractor);
#pragma warning restore 618
        }

        /// <summary>
        /// Automatically called when an Interactor is unregistered to cancel the selection of the Interactor if necessary.
        /// </summary>
        /// <param name="interactor">The Interactor to potentially exit its selection state due to cancellation.</param>
        public virtual void CancelInteractorSelection(IXRSelectInteractor interactor)
        {
            for (var i = interactor.interactablesSelected.Count - 1; i >= 0; --i)
            {
                SelectCancelInternal(interactor, interactor.interactablesSelected[i]);
            }
        }

        void CancelInteractorSelectionInternal(IXRSelectInteractor interactor)
        {
            if (interactor is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                CancelInteractorSelection(baseInteractor);
#pragma warning restore 618
            else
                CancelInteractorSelection(interactor);
        }

        /// <summary>
        /// Automatically called when an Interactable is unregistered to cancel the selection of the Interactable if necessary.
        /// </summary>
        /// <param name="interactable">The Interactable to potentially exit its selection state due to cancellation.</param>
        public virtual void CancelInteractableSelection(IXRSelectInteractable interactable)
        {
            for (var i = interactable.interactorsSelecting.Count - 1; i >= 0; --i)
            {
                SelectCancelInternal(interactable.interactorsSelecting[i], interactable);
            }
        }

        void CancelInteractableSelectionInternal(IXRSelectInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactable is XRBaseInteractable baseInteractable)
                CancelInteractableSelection(baseInteractable);
#pragma warning restore 618
            else
                CancelInteractableSelection(interactable);
        }

        /// <summary>
        /// Automatically called each frame during Update to clear the hover state of the Interactor if necessary due to current conditions.
        /// </summary>
        /// <param name="interactor">The Interactor to potentially exit its hover state.</param>
        /// <param name="validTargets">The list of interactables that this Interactor could possibly interact with this frame.</param>
        /// <seealso cref="ClearInteractorSelection(IXRSelectInteractor, List{IXRInteractable})"/>
        protected virtual void ClearInteractorHover(IXRHoverInteractor interactor, List<IXRInteractable> validTargets)
        {
            if (interactor.interactablesHovered.Count == 0)
                return;

            m_CurrentHovered.Clear();
            m_CurrentHovered.AddRange(interactor.interactablesHovered);

            // Performance optimization of the Contains checks by putting the valid targets into a HashSet.
            // Some Interactors like ARGestureInteractor can have hundreds of valid Interactables
            // since they will add most ARBaseGestureInteractable instances.
            m_UnorderedValidTargets.Clear();
            if (validTargets.Count > 0)
            {
                foreach (var target in validTargets)
                {
                    m_UnorderedValidTargets.Add(target);
                }
            }

            for (var i = m_CurrentHovered.Count - 1; i >= 0; --i)
            {
                var interactable = m_CurrentHovered[i];
                if (!CanHover(interactor, interactable) || !m_UnorderedValidTargets.Contains(interactable))
                    HoverExitInternal(interactor, interactable);
            }
        }

        internal void ClearInteractorHoverInternal(IXRHoverInteractor interactor, List<IXRInteractable> validTargets, List<XRBaseInteractable> deprecatedValidTargets)
        {
            ClearInteractorHover(interactor, validTargets);
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor)
                ClearInteractorHover(baseInteractor, deprecatedValidTargets);
#pragma warning restore 618
        }

        /// <summary>
        /// Automatically called when an Interactor is unregistered to cancel the hover state of the Interactor if necessary.
        /// </summary>
        /// <param name="interactor">The Interactor to potentially exit its hover state due to cancellation.</param>
        public virtual void CancelInteractorHover(IXRHoverInteractor interactor)
        {
            for (var i = interactor.interactablesHovered.Count - 1; i >= 0; --i)
            {
                HoverCancelInternal(interactor, interactor.interactablesHovered[i]);
            }
        }

        void CancelInteractorHoverInternal(IXRHoverInteractor interactor)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor)
                CancelInteractorHover(baseInteractor);
#pragma warning restore 618
            else
                CancelInteractorHover(interactor);
        }

        /// <summary>
        /// Automatically called when an Interactable is unregistered to cancel the hover state of the Interactable if necessary.
        /// </summary>
        /// <param name="interactable">The Interactable to potentially exit its hover state due to cancellation.</param>
        public virtual void CancelInteractableHover(IXRHoverInteractable interactable)
        {
            for (var i = interactable.interactorsHovering.Count - 1; i >= 0; --i)
            {
                HoverCancelInternal(interactable.interactorsHovering[i], interactable);
            }
        }

        void CancelInteractableHoverInternal(IXRHoverInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactable is XRBaseInteractable baseInteractable)
                CancelInteractableHover(baseInteractable);
#pragma warning restore 618
            else
                CancelInteractableHover(interactable);
        }

        /// <summary>
        /// Initiates focus of an Interactable by an Interactor. This method may first result in other interaction events
        /// such as causing the Interactable to first lose focus.
        /// </summary>
        /// <param name="interactor">The Interactor that is gaining focus. Must be a member of an Interaction group.</param>
        /// <param name="interactable">The Interactable being focused.</param>
        /// <remarks>
        /// This attempt may be ignored depending on the focus policy of the Interactor and/or the Interactable. This attempt will also be ignored if the Interactor is not a member of an Interaction group.
        /// </remarks>
        public virtual void FocusEnter(IXRInteractor interactor, IXRFocusInteractable interactable)
        {
            var asGroupMember = interactor as IXRGroupMember;
            var group = asGroupMember?.containingGroup;

            if (group == null || !CanFocus(interactor, interactable))
                return;

            if (interactable.isFocused && !ResolveExistingFocus(group, interactable))
                return;

            using (m_FocusEnterEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                args.interactionGroup = group;
                FocusEnterInternal(group, interactable, args);
            }
        }

        /// <summary>
        /// Initiates losing focus of an Interactable by an Interactor.
        /// </summary>
        /// <param name="group">The Interaction group that is losing focus.</param>
        /// <param name="interactable">The Interactable that is no longer focused.</param>
        public virtual void FocusExit(IXRInteractionGroup group, IXRFocusInteractable interactable)
        {
            var interactor = group.focusInteractor;

            using (m_FocusExitEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                args.interactionGroup = group;
                args.isCanceled = false;
                FocusExitInternal(group, interactable, args);
            }
        }

        internal void FocusExitInternal(IXRInteractionGroup group, IXRFocusInteractable interactable)
        {
            FocusExit(group, interactable);
        }

        /// <summary>
        /// Initiates losing focus of an Interactable by an Interaction group due to cancellation,
        /// such as from either being unregistered due to being disabled or destroyed.
        /// </summary>
        /// <param name="group">The Interaction group that is losing focus of the interactable.</param>
        /// <param name="interactable">The Interactable that is no longer focused.</param>
        public virtual void FocusCancel(IXRInteractionGroup group, IXRFocusInteractable interactable)
        {
            using (m_FocusExitEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = group.focusInteractor;
                args.interactableObject = interactable;
                args.interactionGroup = group;
                args.isCanceled = true;
                FocusExitInternal(group, interactable, args);
            }
        }

        void FocusCancelInternal(IXRInteractionGroup group, IXRFocusInteractable interactable)
        {
            FocusCancel(group, interactable);
        }

        /// <summary>
        /// Initiates selection of an Interactable by an Interactor. This method may first result in other interaction events
        /// such as causing the Interactable to first exit being selected.
        /// </summary>
        /// <param name="interactor">The Interactor that is selecting.</param>
        /// <param name="interactable">The Interactable being selected.</param>
        /// <remarks>
        /// This attempt may be ignored depending on the selection policy of the Interactor and/or the Interactable.
        /// </remarks>
        public virtual void SelectEnter(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            if (interactable.isSelected && !ResolveExistingSelect(interactor, interactable))
                return;

            using (m_SelectEnterEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                SelectEnterInternal(interactor, interactable, args);
            }

            if (interactable is IXRFocusInteractable focusInteractable)
            {
                FocusEnter(interactor, focusInteractable);                    
            }
        }

        void SelectEnterInternal(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                SelectEnter(baseInteractor, baseInteractable);
#pragma warning restore 618
            else
                SelectEnter(interactor, interactable);
        }

        /// <summary>
        /// Initiates ending selection of an Interactable by an Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor that is no longer selecting.</param>
        /// <param name="interactable">The Interactable that is no longer being selected.</param>
        public virtual void SelectExit(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            using (m_SelectExitEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                args.isCanceled = false;
                SelectExitInternal(interactor, interactable, args);
            }
        }

        internal void SelectExitInternal(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                SelectExit(baseInteractor, baseInteractable);
#pragma warning restore 618
            else
                SelectExit(interactor, interactable);
        }

        /// <summary>
        /// Initiates ending selection of an Interactable by an Interactor due to cancellation,
        /// such as from either being unregistered due to being disabled or destroyed.
        /// </summary>
        /// <param name="interactor">The Interactor that is no longer selecting.</param>
        /// <param name="interactable">The Interactable that is no longer being selected.</param>
        public virtual void SelectCancel(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            using (m_SelectExitEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                args.isCanceled = true;
                SelectExitInternal(interactor, interactable, args);
            }
        }

        void SelectCancelInternal(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                SelectCancel(baseInteractor, baseInteractable);
#pragma warning restore 618
            else
                SelectCancel(interactor, interactable);
        }

        /// <summary>
        /// Initiates hovering of an Interactable by an Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor that is hovering.</param>
        /// <param name="interactable">The Interactable being hovered over.</param>
        public virtual void HoverEnter(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            using (m_HoverEnterEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                HoverEnterInternal(interactor, interactable, args);
            }
        }

        void HoverEnterInternal(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                HoverEnter(baseInteractor, baseInteractable);
#pragma warning restore 618
            else
                HoverEnter(interactor, interactable);
        }

        /// <summary>
        /// Initiates ending hovering of an Interactable by an Interactor.
        /// </summary>
        /// <param name="interactor">The Interactor that is no longer hovering.</param>
        /// <param name="interactable">The Interactable that is no longer being hovered over.</param>
        public virtual void HoverExit(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            using (m_HoverExitEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                args.isCanceled = false;
                HoverExitInternal(interactor, interactable, args);
            }
        }

        internal void HoverExitInternal(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                HoverExit(baseInteractor, baseInteractable);
#pragma warning restore 618
            else
                HoverExit(interactor, interactable);
        }

        /// <summary>
        /// Initiates ending hovering of an Interactable by an Interactor due to cancellation,
        /// such as from either being unregistered due to being disabled or destroyed.
        /// </summary>
        /// <param name="interactor">The Interactor that is no longer hovering.</param>
        /// <param name="interactable">The Interactable that is no longer being hovered over.</param>
        public virtual void HoverCancel(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            using (m_HoverExitEventArgs.Get(out var args))
            {
                args.manager = this;
                args.interactorObject = interactor;
                args.interactableObject = interactable;
                args.isCanceled = true;
                HoverExitInternal(interactor, interactable, args);
            }
        }

        void HoverCancelInternal(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                HoverCancel(baseInteractor, baseInteractable);
#pragma warning restore 618
            else
                HoverCancel(interactor, interactable);
        }

        /// <summary>
        /// Initiates focus of an Interactable by an interaction group, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="group">The interaction group that is gaining focus.</param>
        /// <param name="interactable">The Interactable being focused.</param>
        /// <param name="args">Event data containing the interaction group and Interactable involved in the event.</param>
        /// <remarks>
        /// The interaction group and interactable are notified immediately without waiting for a previous call to finish
        /// in the case when this method is called again in a nested way. This means that if this method is
        /// called during the handling of the first event, the second will start and finish before the first
        /// event finishes calling all methods in the sequence to notify of the first event.
        /// </remarks>
        // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable -- ProfilerMarker.Begin with context object does not have Pure attribute
        protected virtual void FocusEnter(IXRInteractionGroup group, IXRFocusInteractable interactable, FocusEnterEventArgs args)
        {
            Debug.Assert(args.interactableObject == interactable, this);
            Debug.Assert(args.interactionGroup == group, this);
            Debug.Assert(args.manager == this || args.manager == null, this);
            args.manager = this;

            using (s_FocusEnterMarker.Auto())
            {
                group.OnFocusEntering(args);
                interactable.OnFocusEntering(args);
                interactable.OnFocusEntered(args);
            }

            lastFocused = interactable;
            focusGained?.Invoke(args);
        }

        void FocusEnterInternal(IXRInteractionGroup group, IXRFocusInteractable interactable, FocusEnterEventArgs args)
        {
            FocusEnter(group, interactable, args);
        }

        /// <summary>
        /// Initiates losing focus of an Interactable by an Interaction Group, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="group">The Interaction Group that is no longer selecting.</param>
        /// <param name="interactable">The Interactable that is no longer being selected.</param>
        /// <param name="args">Event data containing the Interactor and Interactable involved in the event.</param>
        /// <remarks>
        /// The interactable is notified immediately without waiting for a previous call to finish
        /// in the case when this method is called again in a nested way. This means that if this method is
        /// called during the handling of the first event, the second will start and finish before the first
        /// event finishes calling all methods in the sequence to notify of the first event.
        /// </remarks>
        protected virtual void FocusExit(IXRInteractionGroup group, IXRFocusInteractable interactable, FocusExitEventArgs args)
        {
            Debug.Assert(args.interactorObject == group.focusInteractor, this);
            Debug.Assert(args.interactableObject == interactable, this);
            Debug.Assert(args.manager == this || args.manager == null, this);
            args.manager = this;

            using (s_FocusExitMarker.Auto())
            {
                group.OnFocusExiting(args);
                interactable.OnFocusExiting(args);
                interactable.OnFocusExited(args);
            }

            if (interactable == lastFocused)
                lastFocused = null;

            focusLost?.Invoke(args);
        }

        void FocusExitInternal(IXRInteractionGroup group, IXRFocusInteractable interactable, FocusExitEventArgs args)
        {
            FocusExit(group, interactable, args);
        }

        /// <summary>
        /// Initiates selection of an Interactable by an Interactor, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="interactor">The Interactor that is selecting.</param>
        /// <param name="interactable">The Interactable being selected.</param>
        /// <param name="args">Event data containing the Interactor and Interactable involved in the event.</param>
        /// <remarks>
        /// The interactor and interactable are notified immediately without waiting for a previous call to finish
        /// in the case when this method is called again in a nested way. This means that if this method is
        /// called during the handling of the first event, the second will start and finish before the first
        /// event finishes calling all methods in the sequence to notify of the first event.
        /// </remarks>
        // ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable -- ProfilerMarker.Begin with context object does not have Pure attribute
        protected virtual void SelectEnter(IXRSelectInteractor interactor, IXRSelectInteractable interactable, SelectEnterEventArgs args)
        {
            Debug.Assert(args.interactorObject == interactor, this);
            Debug.Assert(args.interactableObject == interactable, this);
            Debug.Assert(args.manager == this || args.manager == null, this);
            args.manager = this;

            using (s_SelectEnterMarker.Auto())
            {
                interactor.OnSelectEntering(args);
                interactable.OnSelectEntering(args);
                interactor.OnSelectEntered(args);
                interactable.OnSelectEntered(args);
            }
        }

        void SelectEnterInternal(IXRSelectInteractor interactor, IXRSelectInteractable interactable, SelectEnterEventArgs args)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                SelectEnter(baseInteractor, baseInteractable, args);
#pragma warning restore 618
            else
                SelectEnter(interactor, interactable, args);
        }

        /// <summary>
        /// Initiates ending selection of an Interactable by an Interactor, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="interactor">The Interactor that is no longer selecting.</param>
        /// <param name="interactable">The Interactable that is no longer being selected.</param>
        /// <param name="args">Event data containing the Interactor and Interactable involved in the event.</param>
        /// <remarks>
        /// The interactor and interactable are notified immediately without waiting for a previous call to finish
        /// in the case when this method is called again in a nested way. This means that if this method is
        /// called during the handling of the first event, the second will start and finish before the first
        /// event finishes calling all methods in the sequence to notify of the first event.
        /// </remarks>
        protected virtual void SelectExit(IXRSelectInteractor interactor, IXRSelectInteractable interactable, SelectExitEventArgs args)
        {
            Debug.Assert(args.interactorObject == interactor, this);
            Debug.Assert(args.interactableObject == interactable, this);
            Debug.Assert(args.manager == this || args.manager == null, this);
            args.manager = this;

            using (s_SelectExitMarker.Auto())
            {
                interactor.OnSelectExiting(args);
                interactable.OnSelectExiting(args);
                interactor.OnSelectExited(args);
                interactable.OnSelectExited(args);
            }
        }

        void SelectExitInternal(IXRSelectInteractor interactor, IXRSelectInteractable interactable, SelectExitEventArgs args)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                SelectExit(baseInteractor, baseInteractable, args);
#pragma warning restore 618
            else
                SelectExit(interactor, interactable, args);
        }

        /// <summary>
        /// Initiates hovering of an Interactable by an Interactor, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="interactor">The Interactor that is hovering.</param>
        /// <param name="interactable">The Interactable being hovered over.</param>
        /// <param name="args">Event data containing the Interactor and Interactable involved in the event.</param>
        /// <remarks>
        /// The interactor and interactable are notified immediately without waiting for a previous call to finish
        /// in the case when this method is called again in a nested way. This means that if this method is
        /// called during the handling of the first event, the second will start and finish before the first
        /// event finishes calling all methods in the sequence to notify of the first event.
        /// </remarks>
        protected virtual void HoverEnter(IXRHoverInteractor interactor, IXRHoverInteractable interactable, HoverEnterEventArgs args)
        {
            Debug.Assert(args.interactorObject == interactor, this);
            Debug.Assert(args.interactableObject == interactable, this);
            Debug.Assert(args.manager == this || args.manager == null, this);
            args.manager = this;

            using (s_HoverEnterMarker.Auto())
            {
                interactor.OnHoverEntering(args);
                interactable.OnHoverEntering(args);
                interactor.OnHoverEntered(args);
                interactable.OnHoverEntered(args);
            }
        }

        void HoverEnterInternal(IXRHoverInteractor interactor, IXRHoverInteractable interactable, HoverEnterEventArgs args)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                HoverEnter(baseInteractor, baseInteractable, args);
#pragma warning restore 618
            else
                HoverEnter(interactor, interactable, args);
        }

        /// <summary>
        /// Initiates ending hovering of an Interactable by an Interactor, passing the given <paramref name="args"/>.
        /// </summary>
        /// <param name="interactor">The Interactor that is no longer hovering.</param>
        /// <param name="interactable">The Interactable that is no longer being hovered over.</param>
        /// <param name="args">Event data containing the Interactor and Interactable involved in the event.</param>
        /// <remarks>
        /// The interactor and interactable are notified immediately without waiting for a previous call to finish
        /// in the case when this method is called again in a nested way. This means that if this method is
        /// called during the handling of the first event, the second will start and finish before the first
        /// event finishes calling all methods in the sequence to notify of the first event.
        /// </remarks>
        protected virtual void HoverExit(IXRHoverInteractor interactor, IXRHoverInteractable interactable, HoverExitEventArgs args)
        {
            Debug.Assert(args.interactorObject == interactor, this);
            Debug.Assert(args.interactableObject == interactable, this);
            Debug.Assert(args.manager == this || args.manager == null, this);
            args.manager = this;

            using (s_HoverExitMarker.Auto())
            {
                interactor.OnHoverExiting(args);
                interactable.OnHoverExiting(args);
                interactor.OnHoverExited(args);
                interactable.OnHoverExited(args);
            }
        }
        // ReSharper restore PossiblyImpureMethodCallOnReadonlyVariable

        void HoverExitInternal(IXRHoverInteractor interactor, IXRHoverInteractable interactable, HoverExitEventArgs args)
        {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && interactable is XRBaseInteractable baseInteractable)
                HoverExit(baseInteractor, baseInteractable, args);
#pragma warning restore 618
            else
                HoverExit(interactor, interactable, args);
        }

        /// <summary>
        /// Automatically called each frame during Update to enter the selection state of the Interactor if necessary due to current conditions.
        /// </summary>
        /// <param name="interactor">The Interactor to potentially enter its selection state.</param>
        /// <param name="validTargets">The list of interactables that this Interactor could possibly interact with this frame.</param>
        /// <remarks>
        /// If the Interactor implements <see cref="IXRTargetPriorityInteractor"/> and is configured to monitor Targets, this method will update its
        /// Targets For Selection property.
        /// </remarks>
        /// <seealso cref="InteractorHoverValidTargets(IXRHoverInteractor, List{IXRInteractable})"/>
        protected virtual void InteractorSelectValidTargets(IXRSelectInteractor interactor, List<IXRInteractable> validTargets)
        {
            if (validTargets.Count == 0)
                return;

            var targetPriorityInteractor = interactor as IXRTargetPriorityInteractor;
            var targetPriorityMode = TargetPriorityMode.None;
            if (targetPriorityInteractor != null)
                targetPriorityMode = targetPriorityInteractor.targetPriorityMode;

            var foundHighestPriorityTarget = false;
            foreach (var target in validTargets)
            {
                if (!(target is IXRSelectInteractable interactable))
                    continue;

                if (targetPriorityMode == TargetPriorityMode.None || targetPriorityMode == TargetPriorityMode.HighestPriorityOnly && foundHighestPriorityTarget)
                {
                    if (CanSelect(interactor, interactable))
                        SelectEnterInternal(interactor, interactable);
                }
                else if (IsSelectPossible(interactor, interactable))
                {
                    if (!foundHighestPriorityTarget)
                    {
                        foundHighestPriorityTarget = true;

                        if (!m_HighestPriorityTargetMap.TryGetValue(interactable, out var interactorList))
                        {
                            interactorList = s_TargetPriorityInteractorListPool.Get();
                            m_HighestPriorityTargetMap[interactable] = interactorList;
                        }
                        interactorList.Add(targetPriorityInteractor);
                    }

                    // ReSharper disable once PossibleNullReferenceException -- Guaranteed to not be null in this branch since not TargetPriorityMode.None
                    targetPriorityInteractor.targetsForSelection?.Add(interactable);

                    if (interactor.isSelectActive)
                        SelectEnterInternal(interactor, interactable);
                }
            }
        }

        internal void InteractorSelectValidTargetsInternal(IXRSelectInteractor interactor, List<IXRInteractable> validTargets, List<XRBaseInteractable> deprecatedValidTargets)
        {
            InteractorSelectValidTargets(interactor, validTargets);
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor)
                InteractorSelectValidTargets(baseInteractor, deprecatedValidTargets);
#pragma warning restore 618
        }

        /// <summary>
        /// Automatically called each frame during Update to enter the hover state of the Interactor if necessary due to current conditions.
        /// </summary>
        /// <param name="interactor">The Interactor to potentially enter its hover state.</param>
        /// <param name="validTargets">The list of interactables that this Interactor could possibly interact with this frame.</param>
        /// <seealso cref="InteractorSelectValidTargets(IXRSelectInteractor, List{IXRInteractable})"/>
        protected virtual void InteractorHoverValidTargets(IXRHoverInteractor interactor, List<IXRInteractable> validTargets)
        {
            if (validTargets.Count == 0)
                return;

            foreach (var target in validTargets)
            {
                if (target is IXRHoverInteractable interactable)
                {
                    if (CanHover(interactor, interactable) && !interactor.IsHovering(interactable))
                    {
                        HoverEnterInternal(interactor, interactable);
                    }
                }
            }
        }

        internal void InteractorHoverValidTargetsInternal(IXRHoverInteractor interactor, List<IXRInteractable> validTargets, List<XRBaseInteractable> deprecatedValidTargets)
        {
            InteractorHoverValidTargets(interactor, validTargets);
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor)
                InteractorHoverValidTargets(baseInteractor, deprecatedValidTargets);
#pragma warning restore 618
        }

        /// <summary>
        /// Automatically called when gaining focus of an Interactable by an interaction group is initiated
        /// and the Interactable is already focused.
        /// </summary>
        /// <param name="interactionGroup">The interaction group that is gaining focus.</param>
        /// <param name="interactable">The Interactable being focused.</param>
        /// <returns>Returns <see langword="true"/> if the existing focus was successfully resolved and focus should continue.
        /// Otherwise, returns <see langword="false"/> if the focus should be ignored.</returns>
        /// <seealso cref="FocusEnter(IXRInteractor, IXRFocusInteractable)"/>
        protected virtual bool ResolveExistingFocus(IXRInteractionGroup interactionGroup, IXRFocusInteractable interactable)
        {
            Debug.Assert(interactable.isFocused, this);

            if (interactionGroup.focusInteractable == interactable)
                return false;

            switch (interactable.focusMode)
            {
                case InteractableFocusMode.Single:
                    ExitInteractableFocus(interactable);
                    break;
                case InteractableFocusMode.Multiple:
                    break;
                default:
                    Debug.Assert(false, $"Unhandled {nameof(InteractableFocusMode)}={interactable.focusMode}", this);
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Automatically called when selection of an Interactable by an Interactor is initiated
        /// and the Interactable is already selected.
        /// </summary>
        /// <param name="interactor">The Interactor that is selecting.</param>
        /// <param name="interactable">The Interactable being selected.</param>
        /// <returns>Returns <see langword="true"/> if the existing selection was successfully resolved and selection should continue.
        /// Otherwise, returns <see langword="false"/> if the select should be ignored.</returns>
        /// <seealso cref="SelectEnter(IXRSelectInteractor, IXRSelectInteractable)"/>
        protected virtual bool ResolveExistingSelect(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            Debug.Assert(interactable.isSelected, this);

            if (interactor.IsSelecting(interactable))
                return false;

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (interactor is XRBaseInteractor baseInteractor && baseInteractor.requireSelectExclusive)
                return false;
#pragma warning restore 618

            switch (interactable.selectMode)
            {
                case InteractableSelectMode.Single:
                    ExitInteractableSelection(interactable);
                    break;
                case InteractableSelectMode.Multiple:
                    break;
                default:
                    Debug.Assert(false, $"Unhandled {nameof(InteractableSelectMode)}={interactable.selectMode}", this);
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the Interactor and Interactable share at least one interaction layer
        /// between their Interaction Layer Masks.
        /// </summary>
        /// <param name="interactor">The Interactor to check.</param>
        /// <param name="interactable">The Interactable to check.</param>
        /// <returns>Returns <see langword="true"/> if the Interactor and Interactable share at least one interaction layer. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="IXRInteractor.interactionLayers"/>
        /// <seealso cref="IXRInteractable.interactionLayers"/>
        protected static bool HasInteractionLayerOverlap(IXRInteractor interactor, IXRInteractable interactable)
        {
            return (interactor.interactionLayers & interactable.interactionLayers) != 0;
        }

        /// <summary>
        /// Returns the processing value of the filters in <see cref="hoverFilters"/> for the given Interactor and
        /// Interactable.
        /// </summary>
        /// <param name="interactor">The Interactor to be validated by the hover filters.</param>
        /// <param name="interactable">The Interactable to be validated by the hover filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="hoverFilters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool ProcessHoverFilters(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            return XRFilterUtility.Process(m_HoverFilters, interactor, interactable);
        }

        /// <summary>
        /// Returns the processing value of the filters in <see cref="selectFilters"/> for the given Interactor and
        /// Interactable.
        /// </summary>
        /// <param name="interactor">The Interactor to be validated by the select filters.</param>
        /// <param name="interactable">The Interactable to be validated by the select filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="selectFilters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool ProcessSelectFilters(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            return XRFilterUtility.Process(m_SelectFilters, interactor, interactable);
        }

        void ExitInteractableSelection(IXRSelectInteractable interactable)
        {
            for (var i = interactable.interactorsSelecting.Count - 1; i >= 0; --i)
            {
                SelectExitInternal(interactable.interactorsSelecting[i], interactable);
            }
        }

        void ExitInteractableFocus(IXRFocusInteractable interactable)
        {
            for (var i = interactable.interactionGroupsFocusing.Count - 1; i >= 0; --i)
            {
                FocusExitInternal(interactable.interactionGroupsFocusing[i], interactable);
            }
        }

        void ClearPriorityForSelectionMap()
        {
            if (m_HighestPriorityTargetMap.Count == 0)
                return;

            foreach (var interactorList in m_HighestPriorityTargetMap.Values)
            {
                foreach (var interactor in interactorList)
                    interactor?.targetsForSelection?.Clear();

                s_TargetPriorityInteractorListPool.Release(interactorList);
            }

            m_HighestPriorityTargetMap.Clear();
        }

        void FlushRegistration()
        {
            m_InteractionGroups.Flush();
            m_Interactors.Flush();
            m_Interactables.Flush();
        }

        internal static void GetOfType<TSource, TDestination>(List<TSource> source, List<TDestination> destination)
        {
            destination.Clear();
            if (source.Count == 0)
                return;

            foreach (var item in source)
            {
                if (item is TDestination destinationItem)
                {
                    destination.Add(destinationItem);
                }
            }
        }

#if UNITY_EDITOR
        static string GetHierarchyPath(GameObject gameObject, bool includeScene = true)
        {
#if UNITY_2021_3_OR_NEWER
            return SearchUtils.GetHierarchyPath(gameObject, includeScene);
#else
            var sb = new StringBuilder(200);
            if (includeScene)
            {
                var sceneName = gameObject.scene.name;
                if (sceneName == string.Empty)
                {
                    var prefabStage = PrefabStageUtility.GetPrefabStage(gameObject);
                    if (prefabStage != null)
                        sceneName = "Prefab Stage";
                    else
                        sceneName = "Unsaved Scene";
                }

                sb.Append("<b>" + sceneName + "</b>");
            }

            sb.Append(GetTransformPath(gameObject.transform));

            var path = sb.ToString();
            return path;

            static string GetTransformPath(Transform tform)
            {
                if (tform.parent == null)
                    return "/" + tform.name;
                return GetTransformPath(tform.parent) + "/" + tform.name;
            }
#endif
        }
#endif
    }
}