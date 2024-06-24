using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Abstract base class from which all interactor behaviours derive.
    /// This class hooks into the interaction system (via <see cref="XRInteractionManager"/>) and provides base virtual methods for handling
    /// hover and selection
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Interactors)]
    public abstract partial class XRBaseInteractor : MonoBehaviour, IXRHoverInteractor, IXRSelectInteractor,
        IXRTargetPriorityInteractor, IXRGroupMember, IXRInteractionStrengthInteractor
    {
        const float k_InteractionStrengthHover = 0f;
        const float k_InteractionStrengthSelect = 1f;

        /// <inheritdoc />
        public event Action<InteractorRegisteredEventArgs> registered;

        /// <inheritdoc />
        public event Action<InteractorUnregisteredEventArgs> unregistered;

        [SerializeField]
        XRInteractionManager m_InteractionManager;

        /// <summary>
        /// The <see cref="XRInteractionManager"/> that this Interactor will communicate with (will find one if <see langword="null"/>).
        /// </summary>
        public XRInteractionManager interactionManager
        {
            get => m_InteractionManager;
            set
            {
                m_InteractionManager = value;
                if (Application.isPlaying && isActiveAndEnabled)
                    RegisterWithInteractionManager();
            }
        }

        /// <inheritdoc />
        public IXRInteractionGroup containingGroup { get; private set; }

        [SerializeField]
        LayerMask m_InteractionLayerMask = -1;

        [SerializeField]
        InteractionLayerMask m_InteractionLayers = -1;

        /// <summary>
        /// Allows interaction with Interactables whose Interaction Layer Mask overlaps with any Layer in this Interaction Layer Mask.
        /// </summary>
        /// <seealso cref="IXRInteractable.interactionLayers"/>
        /// <seealso cref="CanHover(IXRHoverInteractable)"/>
        /// <seealso cref="CanSelect(IXRSelectInteractable)"/>
        /// <inheritdoc />
        public InteractionLayerMask interactionLayers
        {
            get => m_InteractionLayers;
            set => m_InteractionLayers = value;
        }

        [SerializeField]
        Transform m_AttachTransform;

        /// <summary>
        /// The <see cref="Transform"/> that is used as the attach point for Interactables.
        /// </summary>
        /// <remarks>
        /// Automatically instantiated and set in <see cref="Awake"/> if <see langword="null"/>.
        /// Setting this will not automatically destroy the previous object.
        /// </remarks>
        public Transform attachTransform
        {
            get => m_AttachTransform;
            set => m_AttachTransform = value;
        }

        [SerializeField]
        bool m_KeepSelectedTargetValid = true;

        /// <inheritdoc />
        public bool keepSelectedTargetValid
        {
            get => m_KeepSelectedTargetValid;
            set => m_KeepSelectedTargetValid = value;
        }

        [SerializeField]
        bool m_DisableVisualsWhenBlockedInGroup = true;

        /// <summary>
        /// Whether to disable Interactor visuals (such as <see cref="XRInteractorLineVisual"/>) when this Interactor
        /// is part of an <see cref="IXRInteractionGroup"/> and is incapable of interacting due to active interaction
        /// by another Interactor in the Group.
        /// </summary>
        public bool disableVisualsWhenBlockedInGroup
        {
            get => m_DisableVisualsWhenBlockedInGroup;
            set => m_DisableVisualsWhenBlockedInGroup = value;
        }

        [SerializeField]
        XRBaseInteractable m_StartingSelectedInteractable;

        /// <summary>
        /// The Interactable that this Interactor automatically selects at startup (optional, may be <see langword="null"/>).
        /// </summary>
        public XRBaseInteractable startingSelectedInteractable
        {
            get => m_StartingSelectedInteractable;
            set => m_StartingSelectedInteractable = value;
        }

        [SerializeField]
        XRBaseTargetFilter m_StartingTargetFilter;

        /// <summary>
        /// The Target Filter that this Interactor automatically links at startup (optional, may be <see langword="null"/>).
        /// </summary>
        /// <remarks>
        /// To modify the Target Filter after startup, the <see cref="targetFilter"/> property should be used instead.
        /// </remarks>
        /// <seealso cref="targetFilter"/>
        public XRBaseTargetFilter startingTargetFilter
        {
            get => m_StartingTargetFilter;
            set => m_StartingTargetFilter = value;
        }

        [SerializeField]
        HoverEnterEvent m_HoverEntered = new HoverEnterEvent();

        /// <inheritdoc />
        public HoverEnterEvent hoverEntered
        {
            get => m_HoverEntered;
            set => m_HoverEntered = value;
        }

        [SerializeField]
        HoverExitEvent m_HoverExited = new HoverExitEvent();

        /// <inheritdoc />
        public HoverExitEvent hoverExited
        {
            get => m_HoverExited;
            set => m_HoverExited = value;
        }

        [SerializeField]
        SelectEnterEvent m_SelectEntered = new SelectEnterEvent();

        /// <inheritdoc />
        public SelectEnterEvent selectEntered
        {
            get => m_SelectEntered;
            set => m_SelectEntered = value;
        }

        [SerializeField]
        SelectExitEvent m_SelectExited = new SelectExitEvent();

        /// <inheritdoc />
        public SelectExitEvent selectExited
        {
            get => m_SelectExited;
            set => m_SelectExited = value;
        }

        IXRTargetFilter m_TargetFilter;

        /// <summary>
        /// The Target Filter that this Interactor is linked to.
        /// </summary>
        /// <seealso cref="startingTargetFilter"/>
        public IXRTargetFilter targetFilter
        {
            get
            {
                if (m_TargetFilter is Object unityObj && unityObj == null)
                    return null;

                return m_TargetFilter;
            }
            set
            {
                if (Application.isPlaying)
                {
                    targetFilter?.Unlink(this);
                    m_TargetFilter = value;
                    targetFilter?.Link(this);
                }
                else
                {
                    m_TargetFilter = value;
                }
            }
        }

        bool m_AllowHover = true;

        /// <summary>
        /// Defines whether this interactor allows hover events.
        /// </summary>
        /// <remarks>
        /// A hover exit event will still occur if this value is disabled while hovering.
        /// </remarks>
        public bool allowHover
        {
            get => m_AllowHover;
            set => m_AllowHover = value;
        }

        bool m_AllowSelect = true;

        /// <summary>
        /// Defines whether this interactor allows select events.
        /// </summary>
        /// <remarks>
        /// A select exit event will still occur if this value is disabled while selecting.
        /// </remarks>
        public bool allowSelect
        {
            get => m_AllowSelect;
            set => m_AllowSelect = value;
        }

        bool m_IsPerformingManualInteraction;

        /// <summary>
        /// Defines whether this interactor is performing a manual interaction or not.
        /// </summary>
        /// <seealso cref="StartManualInteraction(IXRSelectInteractable)"/>
        /// <seealso cref="EndManualInteraction"/>
        public bool isPerformingManualInteraction => m_IsPerformingManualInteraction;

        readonly HashSetList<IXRHoverInteractable> m_InteractablesHovered = new HashSetList<IXRHoverInteractable>();

        /// <inheritdoc />
        public List<IXRHoverInteractable> interactablesHovered => (List<IXRHoverInteractable>)m_InteractablesHovered.AsList();

        /// <inheritdoc />
        public bool hasHover => m_InteractablesHovered.Count > 0;

        readonly HashSetList<IXRSelectInteractable> m_InteractablesSelected = new HashSetList<IXRSelectInteractable>();

        /// <inheritdoc />
        public List<IXRSelectInteractable> interactablesSelected => (List<IXRSelectInteractable>)m_InteractablesSelected.AsList();

        /// <inheritdoc />
        public IXRSelectInteractable firstInteractableSelected { get; private set; }

        /// <inheritdoc />
        public bool hasSelection => m_InteractablesSelected.Count > 0;

        /// <summary>
        /// Determines if interactor is interacting with UGUI canvas.
        /// </summary>
        internal bool isInteractingWithUI { get; set; }

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
        /// The list of hover filters in this object.
        /// Used as additional hover validations for this Interactor.
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
        /// The list of select filters in this object.
        /// Used as additional select validations for this Interactor.
        /// </summary>
        /// <remarks>
        /// While processing select filters, all changes to this list don't have an immediate effect. Theses changes are
        /// buffered and applied when the processing is finished.
        /// Calling <see cref="IXRFilterList{T}.MoveTo"/> in this list will throw an exception when this list is being processed.
        /// </remarks>
        /// <seealso cref="ProcessSelectFilters"/>
        public IXRFilterList<IXRSelectFilter> selectFilters => m_SelectFilters;

        readonly BindableVariable<float> m_LargestInteractionStrength = new BindableVariable<float>();

        /// <inheritdoc />
        public IReadOnlyBindableVariable<float> largestInteractionStrength => m_LargestInteractionStrength;

        readonly Dictionary<IXRSelectInteractable, Pose> m_AttachPoseOnSelect = new Dictionary<IXRSelectInteractable, Pose>();

        readonly Dictionary<IXRSelectInteractable, Pose> m_LocalAttachPoseOnSelect = new Dictionary<IXRSelectInteractable, Pose>();

        readonly HashSetList<IXRInteractionStrengthInteractable> m_InteractionStrengthInteractables = new HashSetList<IXRInteractionStrengthInteractable>();

        readonly Dictionary<IXRInteractable, float> m_InteractionStrengths = new Dictionary<IXRInteractable, float>();

        IXRSelectInteractable m_ManualInteractionInteractable;

        XRInteractionManager m_RegisteredInteractionManager;

        static readonly ProfilerMarker s_ProcessInteractionStrengthMarker = new ProfilerMarker("XRI.ProcessInteractionStrength.Interactors");

        /// <summary>
        /// When set to <see langword="true"/>, attach point velocity and angular velocity will be updated
        /// during the <see cref="PreprocessInteractor"/> calls. Set to <see langword="false"/> to avoid unnecessary performance cost
        /// when the velocity values are not needed.
        /// </summary>
        internal bool useAttachPointVelocity { get; set; }

        /// <summary>
        /// Last computed default attach point velocity, based on multi-frame sampling of the pose in world space.
        /// Only calculated if <see cref="useAttachPointVelocity"/> is enabled.
        /// </summary>
        /// <seealso cref="GetAttachPointAngularVelocity"/>
        internal Vector3 GetAttachPointVelocity()
        {
            if (TryGetXROrigin(out var origin))
            {
                return origin.TransformDirection(m_AttachPointVelocity);
            }
            return m_AttachPointVelocity;
        }

        Vector3 m_AttachPointVelocity;

        /// <summary>
        /// Last computed default attach point angular velocity, based on multi-frame sampling of the pose in world space.
        /// Only calculated if <see cref="useAttachPointVelocity"/> is enabled.
        /// </summary>
        /// <seealso cref="GetAttachPointVelocity"/>
        internal Vector3 GetAttachPointAngularVelocity()
        {
            if (TryGetXROrigin(out var origin))
            {
                return origin.TransformDirection(m_AttachPointAngularVelocity);
            }
            return m_AttachPointAngularVelocity;
        }
        
        Vector3 m_AttachPointAngularVelocity;

        Transform m_XROriginTransform;
        bool m_HasXROrigin;
        bool m_FailedToFindXROrigin;
        
        /// <summary>
        /// Attempts to locate and return the XR Origin reference frame for the interactor.
        /// </summary>
        /// <seealso cref="XROrigin"/>
        internal bool TryGetXROrigin(out Transform origin)
        {
            if (m_HasXROrigin)
            {
                origin = m_XROriginTransform;
                return true;
            }

            if (!m_FailedToFindXROrigin)
            {
                var xrOrigin = GetComponentInParent<XROrigin>();
                if (xrOrigin != null)
                {
                    var originGo = xrOrigin.Origin;
                    if (originGo != null)
                    {
                        m_XROriginTransform = originGo.transform;
                        m_HasXROrigin = true;
                        origin = m_XROriginTransform;
                        return true;
                    }
                }
                m_FailedToFindXROrigin = true;
            }
            origin = null;
            return false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        protected virtual void Reset()
        {
#if UNITY_EDITOR
            // Don't need to do anything; method kept for backwards compatibility.
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            // Create empty attach transform if none specified
            CreateAttachTransform();

            // Setup the starting filters
            if (m_StartingTargetFilter != null)
                targetFilter = m_StartingTargetFilter;
            m_HoverFilters.RegisterReferences(m_StartingHoverFilters, this);
            m_SelectFilters.RegisterReferences(m_StartingSelectFilters, this);

            // Setup Interaction Manager
            FindCreateInteractionManager();

            // Warn about use of deprecated events
            if (m_OnHoverEntered.GetPersistentEventCount() > 0 ||
                m_OnHoverExited.GetPersistentEventCount() > 0 ||
                m_OnSelectEntered.GetPersistentEventCount() > 0 ||
                m_OnSelectExited.GetPersistentEventCount() > 0)
            {
                Debug.LogWarning("Some deprecated Interactor Events are being used. These deprecated events will be removed in a future version." +
                    " Please convert these to use the newer events, and update script method signatures for Dynamic listeners.", this);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            FindCreateInteractionManager();
            RegisterWithInteractionManager();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            UnregisterWithInteractionManager();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Start()
        {
            if (m_InteractionManager != null && m_StartingSelectedInteractable != null)
                m_InteractionManager.SelectEnter(this, (IXRSelectInteractable)m_StartingSelectedInteractable);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDestroy()
        {
            // Unlink this Interactor from the Target Filter
            targetFilter?.Unlink(this);

            if (containingGroup != null && (!(containingGroup is Object unityObject) || unityObject != null))
                containingGroup.RemoveGroupMember(this);
        }

        /// <inheritdoc />
        public virtual Transform GetAttachTransform(IXRInteractable interactable)
        {
            return m_AttachTransform != null ? m_AttachTransform : transform;
        }

        /// <inheritdoc />
        public Pose GetAttachPoseOnSelect(IXRSelectInteractable interactable)
        {
            return m_AttachPoseOnSelect.TryGetValue(interactable, out var pose) ? pose : Pose.identity;
        }

        /// <inheritdoc />
        public Pose GetLocalAttachPoseOnSelect(IXRSelectInteractable interactable)
        {
            return m_LocalAttachPoseOnSelect.TryGetValue(interactable, out var pose) ? pose : Pose.identity;
        }

        /// <inheritdoc />
        public virtual void GetValidTargets(List<IXRInteractable> targets)
        {
        }

        void FindCreateInteractionManager()
        {
            if (m_InteractionManager != null)
                return;

            m_InteractionManager = ComponentLocatorUtility<XRInteractionManager>.FindOrCreateComponent();
        }

        void RegisterWithInteractionManager()
        {
            if (m_RegisteredInteractionManager == m_InteractionManager)
                return;

            UnregisterWithInteractionManager();

            if (m_InteractionManager != null)
            {
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                m_InteractionManager.RegisterInteractor(this);
#pragma warning restore 618
                m_RegisteredInteractionManager = m_InteractionManager;
            }
        }

        void UnregisterWithInteractionManager()
        {
            if (m_RegisteredInteractionManager == null)
                return;

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            m_RegisteredInteractionManager.UnregisterInteractor(this);
#pragma warning restore 618
            m_RegisteredInteractionManager = null;
        }

        /// <inheritdoc />
        public virtual bool isHoverActive => m_AllowHover;

        /// <inheritdoc />
        public virtual bool isSelectActive => m_AllowSelect;

        /// <inheritdoc />
        public virtual TargetPriorityMode targetPriorityMode { get; set; }

        /// <inheritdoc />
        public virtual List<IXRSelectInteractable> targetsForSelection { get; set; }

        /// <summary>
        /// Determines if the Interactable is valid for hover this frame.
        /// </summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns>Returns <see langword="true"/> if the Interactable can be hovered over this frame.</returns>
        /// <seealso cref="IXRHoverInteractable.IsHoverableBy"/>
        public virtual bool CanHover(IXRHoverInteractable interactable) => true;

        /// <summary>
        /// Determines if the Interactable is valid for selection this frame.
        /// </summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns>Returns <see langword="true"/> if the Interactable can be selected this frame.</returns>
        /// <seealso cref="IXRSelectInteractable.IsSelectableBy"/>
        public virtual bool CanSelect(IXRSelectInteractable interactable) => true;

        /// <inheritdoc />
        public bool IsHovering(IXRHoverInteractable interactable) => m_InteractablesHovered.Contains(interactable);

        /// <inheritdoc />
        public bool IsSelecting(IXRSelectInteractable interactable) => m_InteractablesSelected.Contains(interactable);

        /// <summary>
        /// Determines whether this Interactor is currently hovering the Interactable.
        /// </summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns>Returns <see langword="true"/> if this Interactor is currently hovering the Interactable.
        /// Otherwise, returns <seealso langword="false"/>.</returns>
        /// <remarks>
        /// In other words, returns whether <see cref="interactablesHovered"/> contains <paramref name="interactable"/>.
        /// </remarks>
        /// <seealso cref="interactablesHovered"/>
        /// <seealso cref="IXRHoverInteractor.IsHovering"/>
        protected bool IsHovering(IXRInteractable interactable) => interactable is IXRHoverInteractable hoverable && IsHovering(hoverable);

        /// <summary>
        /// Determines whether this Interactor is currently selecting the Interactable.
        /// </summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns>Returns <see langword="true"/> if this Interactor is currently selecting the Interactable.
        /// Otherwise, returns <seealso langword="false"/>.</returns>
        /// <remarks>
        /// In other words, returns whether <see cref="interactablesSelected"/> contains <paramref name="interactable"/>.
        /// </remarks>
        /// <seealso cref="interactablesSelected"/>
        /// <seealso cref="IXRSelectInteractor.IsSelecting"/>
        protected bool IsSelecting(IXRInteractable interactable) => interactable is IXRSelectInteractable selectable && IsSelecting(selectable);

        /// <summary>
        /// (Read Only) Overriding movement type of the selected Interactable's movement.
        /// By default, this does not override the movement type.
        /// </summary>
        /// <remarks>
        /// You can use this to change the effective movement type of an Interactable for different
        /// Interactors. An example would be having an Interactable use <see cref="XRBaseInteractable.MovementType.VelocityTracking"/>
        /// so it does not move through geometry with a Collider when interacting with it using a Ray or Direct Interactor,
        /// but have a Socket Interactor override the movement type to be <see cref="XRBaseInteractable.MovementType.Instantaneous"/>
        /// for reduced movement latency.
        /// </remarks>
        /// <seealso cref="XRGrabInteractable.movementType"/>
        public virtual XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride => null;

        /// <summary>
        /// Capture the current Attach Transform pose.
        /// This method is automatically called by Unity to capture the pose during the moment of selection.
        /// </summary>
        /// <param name="interactable">The specific Interactable as context to get the attachment point for.</param>
        /// <remarks>
        /// Unity automatically calls this method during <see cref="OnSelectEntering(SelectEnterEventArgs)"/>
        /// and should not typically need to be called by a user.
        /// </remarks>
        /// <seealso cref="GetAttachPoseOnSelect"/>
        /// <seealso cref="GetLocalAttachPoseOnSelect"/>
        /// <seealso cref="XRBaseInteractable.CaptureAttachPose"/>
        protected void CaptureAttachPose(IXRSelectInteractable interactable)
        {
            var thisAttachTransform = GetAttachTransform(interactable);
            if (thisAttachTransform != null)
            {
                m_AttachPoseOnSelect[interactable] =
                    new Pose(thisAttachTransform.position, thisAttachTransform.rotation);
                m_LocalAttachPoseOnSelect[interactable] =
                    new Pose(thisAttachTransform.localPosition, thisAttachTransform.localRotation);
            }
            else
            {
                m_AttachPoseOnSelect.Remove(interactable);
                m_LocalAttachPoseOnSelect.Remove(interactable);
            }
        }

        /// <summary>
        /// Create a new child GameObject to use as the attach transform if one is not set.
        /// </summary>
        /// <seealso cref="attachTransform"/>
        protected void CreateAttachTransform()
        {
            if (m_AttachTransform == null)
            {
                m_AttachTransform = new GameObject($"[{gameObject.name}] Attach").transform;
                m_AttachTransform.SetParent(transform, false);
                m_AttachTransform.localPosition = Vector3.zero;
                m_AttachTransform.localRotation = Quaternion.identity;
            }
        }

        /// <inheritdoc />
        public virtual void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            if (useAttachPointVelocity)
                UpdateVelocityAndAngularVelocity();
        }

        /// <inheritdoc />
        public virtual void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
        }

        /// <inheritdoc />
        public float GetInteractionStrength(IXRInteractable interactable)
        {
            if (m_InteractionStrengths.TryGetValue(interactable, out var interactionStrength))
                return interactionStrength;

            return 0f;
        }

        /// <inheritdoc />
        void IXRInteractionStrengthInteractor.ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase) => ProcessInteractionStrength(updatePhase);

        /// <inheritdoc />
        void IXRInteractor.OnRegistered(InteractorRegisteredEventArgs args) => OnRegistered(args);

        /// <inheritdoc />
        void IXRInteractor.OnUnregistered(InteractorUnregisteredEventArgs args) => OnUnregistered(args);

        /// <inheritdoc />
        bool IXRHoverInteractor.CanHover(IXRHoverInteractable interactable)
        {
            if (interactable is XRBaseInteractable baseInteractable)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                return CanHover(baseInteractable) && ProcessHoverFilters(interactable);
#pragma warning restore 618
            return CanHover(interactable) && ProcessHoverFilters(interactable);
        }

        /// <inheritdoc />
        void IXRHoverInteractor.OnHoverEntering(HoverEnterEventArgs args) => OnHoverEntering(args);

        /// <inheritdoc />
        void IXRHoverInteractor.OnHoverEntered(HoverEnterEventArgs args) => OnHoverEntered(args);

        /// <inheritdoc />
        void IXRHoverInteractor.OnHoverExiting(HoverExitEventArgs args) => OnHoverExiting(args);

        /// <inheritdoc />
        void IXRHoverInteractor.OnHoverExited(HoverExitEventArgs args) => OnHoverExited(args);

        /// <inheritdoc />
        bool IXRSelectInteractor.CanSelect(IXRSelectInteractable interactable)
        {
            if (interactable is XRBaseInteractable baseInteractable)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                return CanSelect(baseInteractable) && ProcessSelectFilters(interactable);
#pragma warning restore 618
            return CanSelect(interactable) && ProcessSelectFilters(interactable);
        }

        /// <inheritdoc />
        void IXRSelectInteractor.OnSelectEntering(SelectEnterEventArgs args) => OnSelectEntering(args);

        /// <inheritdoc />
        void IXRSelectInteractor.OnSelectEntered(SelectEnterEventArgs args) => OnSelectEntered(args);

        /// <inheritdoc />
        void IXRSelectInteractor.OnSelectExiting(SelectExitEventArgs args) => OnSelectExiting(args);

        /// <inheritdoc />
        void IXRSelectInteractor.OnSelectExited(SelectExitEventArgs args) => OnSelectExited(args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when this Interactor is registered with it.
        /// </summary>
        /// <param name="args">Event data containing the Interaction Manager that registered this Interactor.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.RegisterInteractor(IXRInteractor)"/>
        protected virtual void OnRegistered(InteractorRegisteredEventArgs args)
        {
            if (args.manager != m_InteractionManager)
                Debug.LogWarning($"An Interactor was registered with an unexpected {nameof(XRInteractionManager)}." +
                    $" {this} was expecting to communicate with \"{m_InteractionManager}\" but was registered with \"{args.manager}\".", this);

            registered?.Invoke(args);
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when this Interactor is unregistered from it.
        /// </summary>
        /// <param name="args">Event data containing the Interaction Manager that unregistered this Interactor.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.UnregisterInteractor(IXRInteractor)"/>
        protected virtual void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            if (args.manager != m_RegisteredInteractionManager)
                Debug.LogWarning($"An Interactor was unregistered from an unexpected {nameof(XRInteractionManager)}." +
                    $" {this} was expecting to communicate with \"{m_RegisteredInteractionManager}\" but was unregistered from \"{args.manager}\".", this);

            unregistered?.Invoke(args);
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor first initiates hovering over an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is being hovered over.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverEntered(HoverEnterEventArgs)"/>
        protected virtual void OnHoverEntering(HoverEnterEventArgs args)
        {
            var added = m_InteractablesHovered.Add(args.interactableObject);
            Debug.Assert(added, "An Interactor received a Hover Enter event for an Interactable that it was already hovering over.", this);

            if (args.interactableObject is IXRInteractionStrengthInteractable interactionStrengthInteractable)
                m_InteractionStrengthInteractables.Add(interactionStrengthInteractable);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            hoverTargets.Add(args.interactable);
            OnHoverEntering(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor first initiates hovering over an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is being hovered over.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverExited(HoverExitEventArgs)"/>
        protected virtual void OnHoverEntered(HoverEnterEventArgs args)
        {
            m_HoverEntered?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnHoverEntered(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor ends hovering over an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is no longer hovered over.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverExited(HoverExitEventArgs)"/>
        protected virtual void OnHoverExiting(HoverExitEventArgs args)
        {
            var removed = m_InteractablesHovered.Remove(args.interactableObject);
            Debug.Assert(removed, "An Interactor received a Hover Exit event for an Interactable that it was not hovering over.", this);

            if (m_InteractionStrengthInteractables.Count > 0 &&
                args.interactableObject is IXRInteractionStrengthInteractable interactionStrengthInteractable &&
                !IsSelecting(interactionStrengthInteractable))
            {
                m_InteractionStrengthInteractables.Remove(interactionStrengthInteractable);
            }

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            hoverTargets.Remove(args.interactable);
            OnHoverExiting(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor ends hovering over an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is no longer hovered over.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverEntered(HoverEnterEventArgs)"/>
        protected virtual void OnHoverExited(HoverExitEventArgs args)
        {
            m_HoverExited?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnHoverExited(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor first initiates selection of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is being selected.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectEntered(SelectEnterEventArgs)"/>
        protected virtual void OnSelectEntering(SelectEnterEventArgs args)
        {
            var added = m_InteractablesSelected.Add(args.interactableObject);
            Debug.Assert(added, "An Interactor received a Select Enter event for an Interactable that it was already selecting.", this);

            if (args.interactableObject is IXRInteractionStrengthInteractable interactionStrengthInteractable)
                m_InteractionStrengthInteractables.Add(interactionStrengthInteractable);

            if (m_InteractablesSelected.Count == 1)
                firstInteractableSelected = args.interactableObject;

            CaptureAttachPose(args.interactableObject);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnSelectEntering(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor first initiates selection of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is being selected.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectExited(SelectExitEventArgs)"/>
        protected virtual void OnSelectEntered(SelectEnterEventArgs args)
        {
            m_SelectEntered?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnSelectEntered(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor ends selection of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is no longer selected.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectExited(SelectExitEventArgs)"/>
        protected virtual void OnSelectExiting(SelectExitEventArgs args)
        {
            var removed = m_InteractablesSelected.Remove(args.interactableObject);
            Debug.Assert(removed, "An Interactor received a Select Exit event for an Interactable that it was not selecting.", this);

            if (m_InteractionStrengthInteractables.Count > 0 &&
                args.interactableObject is IXRInteractionStrengthInteractable interactionStrengthInteractable &&
                !IsHovering(interactionStrengthInteractable))
            {
                m_InteractionStrengthInteractables.Remove(interactionStrengthInteractable);
            }

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnSelectExiting(args.interactable);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor ends selection of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactable that is no longer selected.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectEntered(SelectEnterEventArgs)"/>
        protected virtual void OnSelectExited(SelectExitEventArgs args)
        {
            m_SelectExited?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnSelectExited(args.interactable);
#pragma warning restore 618

            // The dictionaries are pruned so that they don't infinitely grow in size as selections are made.
            if (m_InteractablesSelected.Count == 0)
            {
                firstInteractableSelected = null;
                m_AttachPoseOnSelect.Clear();
                m_LocalAttachPoseOnSelect.Clear();
            }
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method to signal to update the interaction strength.
        /// </summary>
        /// <param name="updatePhase">The update phase during which this method is called.</param>
        /// <seealso cref="GetInteractionStrength"/>
        /// <seealso cref="IXRInteractionStrengthInteractor.ProcessInteractionStrength"/>
        protected virtual void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            var maxInteractionStrength = 0f;

            using (s_ProcessInteractionStrengthMarker.Auto())
            {
                m_InteractionStrengths.Clear();

                // Select is checked before Hover to allow process to only be called once per interactor hovering and selecting
                // using the largest initial interaction strength.
                for (int i = 0, count = m_InteractablesSelected.Count; i < count; ++i)
                {
                    var interactable = m_InteractablesSelected[i];
                    if (interactable is IXRInteractionStrengthInteractable)
                        continue;

                    m_InteractionStrengths[interactable] = k_InteractionStrengthSelect;

                    maxInteractionStrength = k_InteractionStrengthSelect;
                }

                for (int i = 0, count = m_InteractablesHovered.Count; i < count; ++i)
                {
                    var interactable = m_InteractablesHovered[i];
                    if (interactable is IXRInteractionStrengthInteractable || IsSelecting(interactable))
                        continue;

                    m_InteractionStrengths[interactable] = k_InteractionStrengthHover;
                }

                for (int i = 0, count = m_InteractionStrengthInteractables.Count; i < count; ++i)
                {
                    var interactable = m_InteractionStrengthInteractables[i];
                    var interactionStrength = interactable.GetInteractionStrength(this);
                    m_InteractionStrengths[interactable] = interactionStrength;

                    maxInteractionStrength = Mathf.Max(maxInteractionStrength, interactionStrength);
                }
            }

            // This is done outside of the ProfilerMarker since it could trigger user callbacks
            m_LargestInteractionStrength.Value = maxInteractionStrength;
        }

        /// <summary>
        /// Manually initiate selection of an Interactable.
        /// </summary>
        /// <param name="interactable">Interactable that is being selected.</param>
        /// <seealso cref="EndManualInteraction"/>
        public virtual void StartManualInteraction(IXRSelectInteractable interactable)
        {
            if (interactionManager == null)
            {
                Debug.LogWarning("Cannot start manual interaction without an Interaction Manager set.", this);
                return;
            }

            interactionManager.SelectEnter(this, interactable);
            m_IsPerformingManualInteraction = true;
            m_ManualInteractionInteractable = interactable;
        }

        /// <summary>
        /// Ends the manually initiated selection of an Interactable.
        /// </summary>
        /// <seealso cref="StartManualInteraction(IXRSelectInteractable)"/>
        public virtual void EndManualInteraction()
        {
            if (interactionManager == null)
            {
                Debug.LogWarning("Cannot end manual interaction without an Interaction Manager set.", this);
                return;
            }

            if (!m_IsPerformingManualInteraction)
            {
                Debug.LogWarning("Tried to end manual interaction but was not performing manual interaction. Ignoring request.", this);
                return;
            }

            interactionManager.SelectExit(this, m_ManualInteractionInteractable);
            m_IsPerformingManualInteraction = false;
            m_ManualInteractionInteractable = null;
        }

        /// <summary>
        /// Returns the processing value of the filters in <see cref="hoverFilters"/> for this Interactor and the
        /// given Interactable.
        /// </summary>
        /// <param name="interactable">The Interactable to be validated by the hover filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="hoverFilters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool ProcessHoverFilters(IXRHoverInteractable interactable)
        {
            return XRFilterUtility.Process(m_HoverFilters, this, interactable);
        }

        /// <summary>
        /// Returns the processing value of the filters in <see cref="selectFilters"/> for this Interactor and the
        /// given Interactable.
        /// </summary>
        /// <param name="interactable">The Interactor to be validated by the select filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="selectFilters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool ProcessSelectFilters(IXRSelectInteractable interactable)
        {
            return XRFilterUtility.Process(m_SelectFilters, this, interactable);
        }

        /// <inheritdoc />
        void IXRGroupMember.OnRegisteringAsGroupMember(IXRInteractionGroup group)
        {
            if (containingGroup != null)
            {
                Debug.LogError($"{name} is already part of a Group. Remove the member from the Group first.", this);
                return;
            }

            if (!group.ContainsGroupMember(this))
            {
                Debug.LogError($"{nameof(IXRGroupMember.OnRegisteringAsGroupMember)} was called but the Group does not contain {name}. " +
                               "Add the member to the Group rather than calling this method directly.", this);
                return;
            }

            containingGroup = group;
        }

        /// <inheritdoc />
        void IXRGroupMember.OnRegisteringAsNonGroupMember()
        {
            containingGroup = null;
        }
        
        // Velocity logic taken from MRTK:
        // https://github.com/microsoft/MixedRealityToolkit-Unity/blob/6e061451d7caed1fcb7c324baf92be293efda4cf/Assets/MRTK/Core/Providers/Hands/BaseHand.cs#L42
        // Velocity internal states
        float m_DeltaTimeStart;
        const int k_VelocityUpdateInterval = 6;
        int m_FrameOn;

        readonly Vector3[] m_VelocityPositionsCache = new Vector3[k_VelocityUpdateInterval];
        readonly Vector3[] m_VelocityNormalsCache = new Vector3[k_VelocityUpdateInterval];
        Vector3 m_VelocityPositionsSum;
        Vector3 m_VelocityNormalsSum;

        /// <summary>
        /// Compute and updates the velocity and angular velocity properties using the attach transform pose as a reference.
        /// </summary>
        void UpdateVelocityAndAngularVelocity()
        {
            // $TODO: Update to take/use IXRInteractable instead of 'null'
            var currentAttachTransform = GetAttachTransform(null);
            bool hasXROrigin = TryGetXROrigin(out var xrOrigin);
            
            if (m_FrameOn < k_VelocityUpdateInterval)
            {
                m_VelocityPositionsCache[m_FrameOn] = hasXROrigin ? xrOrigin.InverseTransformPoint(currentAttachTransform.position) : currentAttachTransform.position;
                m_VelocityPositionsSum += m_VelocityPositionsCache[m_FrameOn];
                m_VelocityNormalsCache[m_FrameOn] = hasXROrigin ? xrOrigin.InverseTransformVector(currentAttachTransform.up) : currentAttachTransform.up;
                m_VelocityNormalsSum += m_VelocityNormalsCache[m_FrameOn];
            }
            else
            {
                var frameIndex = m_FrameOn % k_VelocityUpdateInterval;

                var deltaTime = Time.unscaledTime - m_DeltaTimeStart;

                var newPosition = hasXROrigin ? xrOrigin.InverseTransformPoint(currentAttachTransform.position) : currentAttachTransform.position;
                var newNormal = hasXROrigin ? xrOrigin.InverseTransformVector(currentAttachTransform.up) : currentAttachTransform.up;

                var newPositionsSum = m_VelocityPositionsSum - m_VelocityPositionsCache[frameIndex] + newPosition;
                var newNormalsSum = m_VelocityNormalsSum - m_VelocityNormalsCache[frameIndex] + newNormal;
                m_AttachPointVelocity = (newPositionsSum - m_VelocityPositionsSum) / deltaTime / k_VelocityUpdateInterval;

                var fromDirection = m_VelocityNormalsSum / k_VelocityUpdateInterval;
                var toDirection = newNormalsSum / k_VelocityUpdateInterval;
                
                var rotation = Quaternion.FromToRotation(fromDirection, toDirection);
                var rotationRate = rotation.eulerAngles * Mathf.Deg2Rad;
                m_AttachPointAngularVelocity = rotationRate / deltaTime;

                m_VelocityPositionsCache[frameIndex] = newPosition;
                m_VelocityNormalsCache[frameIndex] = newNormal;
                m_VelocityPositionsSum = newPositionsSum;
                m_VelocityNormalsSum = newNormalsSum;
            }

            m_DeltaTimeStart = Time.unscaledTime;
            m_FrameOn++;
        }
    }
}
