using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Abstract base class from which all interactable behaviors derive.
    /// This class hooks into the interaction system (via <see cref="XRInteractionManager"/>) and provides base virtual methods for handling
    /// hover, selection, and focus.
    /// </summary>
    [SelectionBase]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_Interactables)]
    public abstract partial class XRBaseInteractable : MonoBehaviour, IXRActivateInteractable, IXRHoverInteractable, IXRSelectInteractable, IXRFocusInteractable, IXRInteractionStrengthInteractable, IXROverridesGazeAutoSelect
    {
        const float k_InteractionStrengthHover = 0f;
        const float k_InteractionStrengthSelect = 1f;

        /// <summary>
        /// Options for how to process and perform movement of an Interactable.
        /// </summary>
        /// <remarks>
        /// Each method of movement has tradeoffs, and different values may be more appropriate
        /// for each type of Interactable object in a project.
        /// </remarks>
        /// <seealso cref="XRGrabInteractable.movementType"/>
        public enum MovementType
        {
            /// <summary>
            /// Move the Interactable object by setting the velocity and angular velocity of the Rigidbody.
            /// Use this if you don't want the object to be able to move through other Colliders without a Rigidbody
            /// as it follows the Interactor, however with the tradeoff that it can appear to lag behind
            /// and not move as smoothly as <see cref="Instantaneous"/>.
            /// </summary>
            /// <remarks>
            /// Unity sets the velocity values during the FixedUpdate function. This Interactable will move at the
            /// framerate-independent interval of the Physics update, which may be slower than the Update rate.
            /// If the Rigidbody is not set to use interpolation or extrapolation, as the Interactable
            /// follows the Interactor, it may not visually update position each frame and be a slight distance
            /// behind the Interactor or controller due to the difference between the Physics update rate
            /// and the render update rate.
            /// </remarks>
            /// <seealso cref="Rigidbody.velocity"/>
            /// <seealso cref="Rigidbody.angularVelocity"/>
            VelocityTracking,

            /// <summary>
            /// Move the Interactable object by moving the kinematic Rigidbody towards the target position and orientation.
            /// Use this if you want to keep the visual representation synchronized to match its Physics state,
            /// and if you want to allow the object to be able to move through other Colliders without a Rigidbody
            /// as it follows the Interactor.
            /// </summary>
            /// <remarks>
            /// Unity will call the movement methods during the FixedUpdate function. This Interactable will move at the
            /// framerate-independent interval of the Physics update, which may be slower than the Update rate.
            /// If the Rigidbody is not set to use interpolation or extrapolation, as the Interactable
            /// follows the Interactor, it may not visually update position each frame and be a slight distance
            /// behind the Interactor or controller due to the difference between the Physics update rate
            /// and the render update rate. Collisions will be more accurate as compared to <see cref="Instantaneous"/>
            /// since with this method, the Rigidbody will be moved by settings its internal velocity rather than
            /// instantly teleporting to match the Transform pose.
            /// </remarks>
            /// <seealso cref="Rigidbody.MovePosition"/>
            /// <seealso cref="Rigidbody.MoveRotation"/>
            Kinematic,

            /// <summary>
            /// Move the Interactable object by setting the position and rotation of the Transform every frame.
            /// Use this if you want the visual representation to be updated each frame, minimizing latency,
            /// however with the tradeoff that it will be able to move through other Colliders without a Rigidbody
            /// as it follows the Interactor.
            /// </summary>
            /// <remarks>
            /// Unity will set the Transform values each frame, which may be faster than the framerate-independent
            /// interval of the Physics update. The Collider of the Interactable object may be a slight distance
            /// behind the visual as it follows the Interactor due to the difference between the Physics update rate
            /// and the render update rate. Collisions will not be computed as accurately as <see cref="Kinematic"/>
            /// since with this method, the Rigidbody will be forced to instantly teleport poses to match the Transform pose
            /// rather than moving the Rigidbody through setting its internal velocity.
            /// </remarks>
            /// <seealso cref="Transform.position"/>
            /// <seealso cref="Transform.rotation"/>
            Instantaneous,
        }

        /// <summary>
        /// Options for how to calculate an Interactable distance to a location in world space.
        /// </summary>
        /// <seealso cref="distanceCalculationMode"/>
        public enum DistanceCalculationMode
        {
            /// <summary>
            /// Calculates the distance using the Interactable's transform position.
            /// This option has low performance cost, but it may have low distance calculation accuracy for some objects.
            /// </summary>
            TransformPosition,

            /// <summary>
            /// Calculates the distance using the Interactable's colliders list using the shortest distance to each.
            /// This option has moderate performance cost and should have moderate distance calculation accuracy for most objects.
            /// </summary>
            /// <seealso cref="XRInteractableUtility.TryGetClosestCollider"/>
            ColliderPosition,

            /// <summary>
            /// Calculates the distance using the Interactable's colliders list using the shortest distance to the closest point of each
            /// (either on the surface or inside the Collider).
            /// This option has high performance cost but high distance calculation accuracy.
            /// </summary>
            /// <remarks>
            /// The Interactable's colliders can only be of type <see cref="BoxCollider"/>, <see cref="SphereCollider"/>, <see cref="CapsuleCollider"/>, or convex <see cref="MeshCollider"/>.
            /// </remarks>
            /// <seealso cref="Collider.ClosestPoint"/>
            /// <seealso cref="XRInteractableUtility.TryGetClosestPointOnCollider"/>
            ColliderVolume,
        }

        /// <inheritdoc />
        public event Action<InteractableRegisteredEventArgs> registered;

        /// <inheritdoc />
        public event Action<InteractableUnregisteredEventArgs> unregistered;

        /// <summary>
        /// Overriding callback of this object's distance calculation.
        /// Use this to change the calculation performed in <see cref="GetDistance"/> without needing to create a derived class.
        /// <br />
        /// When a callback is assigned to this property, the <see cref="GetDistance"/> execution calls it to perform the
        /// distance calculation instead of using its default calculation (specified by <see cref="distanceCalculationMode"/> in this base class).
        /// Assign <see langword="null"/> to this property to restore the default calculation.
        /// </summary>
        /// <remarks>
        /// The assigned callback will be invoked to calculate and return the distance information of the point on this
        /// Interactable (the first parameter) closest to the given location (the second parameter).
        /// The given location and returned distance information are in world space.
        /// </remarks>
        /// <seealso cref="GetDistance"/>
        /// <seealso cref="DistanceInfo"/>
        public Func<IXRInteractable, Vector3, DistanceInfo> getDistanceOverride { get; set; }

        [SerializeField]
        XRInteractionManager m_InteractionManager;

        /// <summary>
        /// The <see cref="XRInteractionManager"/> that this Interactable will communicate with (will find one if <see langword="null"/>).
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

        [SerializeField]
#pragma warning disable IDE0044 // Add readonly modifier -- readonly fields cannot be serialized by Unity
        List<Collider> m_Colliders = new List<Collider>();
#pragma warning restore IDE0044

        /// <summary>
        /// (Read Only) Colliders to use for interaction with this Interactable (if empty, will use any child Colliders).
        /// </summary>
        public List<Collider> colliders => m_Colliders;

        [SerializeField]
        LayerMask m_InteractionLayerMask = -1;

        [SerializeField]
        InteractionLayerMask m_InteractionLayers = 1;

        /// <summary>
        /// Allows interaction with Interactors whose Interaction Layer Mask overlaps with any Layer in this Interaction Layer Mask.
        /// </summary>
        /// <seealso cref="IXRInteractor.interactionLayers"/>
        /// <seealso cref="IsHoverableBy(IXRHoverInteractor)"/>
        /// <seealso cref="IsSelectableBy(IXRSelectInteractor)"/>
        /// <inheritdoc />
        public InteractionLayerMask interactionLayers
        {
            get => m_InteractionLayers;
            set => m_InteractionLayers = value;
        }

        [SerializeField]
        DistanceCalculationMode m_DistanceCalculationMode = DistanceCalculationMode.ColliderPosition;

        /// <summary>
        /// Specifies how this Interactable calculates its distance to a location, either using its Transform position, Collider
        /// position or Collider volume.
        /// </summary>
        /// <seealso cref="GetDistance"/>
        /// <seealso cref="colliders"/>
        /// <seealso cref="DistanceCalculationMode"/>
        public DistanceCalculationMode distanceCalculationMode
        {
            get => m_DistanceCalculationMode;
            set => m_DistanceCalculationMode = value;
        }

        [SerializeField]
        InteractableSelectMode m_SelectMode = InteractableSelectMode.Single;

        /// <inheritdoc />
        public InteractableSelectMode selectMode
        {
            get => m_SelectMode;
            set => m_SelectMode = value;
        }

        [SerializeField]
        InteractableFocusMode m_FocusMode = InteractableFocusMode.Single;

        /// <inheritdoc />
        public InteractableFocusMode focusMode
        {
            get => m_FocusMode;
            set => m_FocusMode = value;
        }

        [SerializeField]
        GameObject m_CustomReticle;

        /// <summary>
        /// The reticle that appears at the end of the line when valid.
        /// </summary>
        public GameObject customReticle
        {
            get => m_CustomReticle;
            set => m_CustomReticle = value;
        }

        [SerializeField]
        bool m_AllowGazeInteraction;
        /// <summary>
        /// Enables interaction with <see cref="XRGazeInteractor"/>.
        /// </summary>
        public bool allowGazeInteraction
        {
            get => m_AllowGazeInteraction;
            set => m_AllowGazeInteraction = value;
        }

        [SerializeField]
        bool m_AllowGazeSelect;
        /// <summary>
        /// Enables <see cref="XRGazeInteractor"/> to select this <see cref="XRBaseInteractable"/>.
        /// </summary>
        /// <seealso cref="XRRayInteractor.hoverToSelect"/>
        public bool allowGazeSelect
        {
            get => m_AllowGazeSelect;
            set => m_AllowGazeSelect = value;
        }

        [SerializeField]
        bool m_OverrideGazeTimeToSelect;
        /// <inheritdoc />
        public bool overrideGazeTimeToSelect
        {
            get => m_OverrideGazeTimeToSelect;
            set => m_OverrideGazeTimeToSelect = value;
        }

        [SerializeField]
        float m_GazeTimeToSelect = 0.5f;
        /// <inheritdoc />
        public float gazeTimeToSelect
        {
            get => m_GazeTimeToSelect;
            set => m_GazeTimeToSelect = value;
        }

        [SerializeField]
        bool m_OverrideTimeToAutoDeselectGaze;
        /// <inheritdoc />
        public bool overrideTimeToAutoDeselectGaze
        {
            get => m_OverrideTimeToAutoDeselectGaze;
            set => m_OverrideTimeToAutoDeselectGaze = value;
        }

        [SerializeField]
        float m_TimeToAutoDeselectGaze = 3f;
        /// <inheritdoc />
        public float timeToAutoDeselectGaze
        {
            get => m_TimeToAutoDeselectGaze;
            set => m_TimeToAutoDeselectGaze = value;
        }

        [SerializeField]
        bool m_AllowGazeAssistance;
        /// <summary>
        /// Enables gaze assistance with this interactable.
        /// </summary>
        public bool allowGazeAssistance
        {
            get => m_AllowGazeAssistance;
            set => m_AllowGazeAssistance = value;
        }

        [SerializeField]
        HoverEnterEvent m_FirstHoverEntered = new HoverEnterEvent();

        /// <inheritdoc />
        public HoverEnterEvent firstHoverEntered
        {
            get => m_FirstHoverEntered;
            set => m_FirstHoverEntered = value;
        }

        [SerializeField]
        HoverExitEvent m_LastHoverExited = new HoverExitEvent();

        /// <inheritdoc />
        public HoverExitEvent lastHoverExited
        {
            get => m_LastHoverExited;
            set => m_LastHoverExited = value;
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
        SelectEnterEvent m_FirstSelectEntered = new SelectEnterEvent();

        /// <inheritdoc />
        public SelectEnterEvent firstSelectEntered
        {
            get => m_FirstSelectEntered;
            set => m_FirstSelectEntered = value;
        }

        [SerializeField]
        SelectExitEvent m_LastSelectExited = new SelectExitEvent();

        /// <inheritdoc />
        public SelectExitEvent lastSelectExited
        {
            get => m_LastSelectExited;
            set => m_LastSelectExited = value;
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

        [SerializeField]
        FocusEnterEvent m_FirstFocusEntered = new FocusEnterEvent();

        /// <inheritdoc />
        public FocusEnterEvent firstFocusEntered
        {
            get => m_FirstFocusEntered;
            set => m_FirstFocusEntered = value;
        }

        [SerializeField]
        FocusExitEvent m_LastFocusExited = new FocusExitEvent();

        /// <inheritdoc />
        public FocusExitEvent lastFocusExited
        {
            get => m_LastFocusExited;
            set => m_LastFocusExited = value;
        }

        [SerializeField]
        FocusEnterEvent m_FocusEntered = new FocusEnterEvent();

        /// <inheritdoc />
        public FocusEnterEvent focusEntered
        {
            get => m_FocusEntered;
            set => m_FocusEntered = value;
        }

        [SerializeField]
        FocusExitEvent m_FocusExited = new FocusExitEvent();

        /// <inheritdoc />
        public FocusExitEvent focusExited
        {
            get => m_FocusExited;
            set => m_FocusExited = value;
        }

        [SerializeField]
        ActivateEvent m_Activated = new ActivateEvent();

        /// <inheritdoc />
        public ActivateEvent activated
        {
            get => m_Activated;
            set => m_Activated = value;
        }

        [SerializeField]
        DeactivateEvent m_Deactivated = new DeactivateEvent();

        /// <inheritdoc />
        public DeactivateEvent deactivated
        {
            get => m_Deactivated;
            set => m_Deactivated = value;
        }

        readonly HashSetList<IXRHoverInteractor> m_InteractorsHovering = new HashSetList<IXRHoverInteractor>();

        /// <inheritdoc />
        public List<IXRHoverInteractor> interactorsHovering => (List<IXRHoverInteractor>)m_InteractorsHovering.AsList();

        /// <inheritdoc />
        public bool isHovered => m_InteractorsHovering.Count > 0;

        readonly HashSetList<IXRSelectInteractor> m_InteractorsSelecting = new HashSetList<IXRSelectInteractor>();

        /// <inheritdoc />
        public List<IXRSelectInteractor> interactorsSelecting => (List<IXRSelectInteractor>)m_InteractorsSelecting.AsList();

        /// <inheritdoc />
        public IXRSelectInteractor firstInteractorSelecting { get; private set; }

        /// <inheritdoc />
        public bool isSelected => m_InteractorsSelecting.Count > 0;

        readonly HashSetList<IXRInteractionGroup> m_InteractionGroupsFocusing = new HashSetList<IXRInteractionGroup>();

        /// <inheritdoc />
        public List<IXRInteractionGroup> interactionGroupsFocusing => (List<IXRInteractionGroup>)m_InteractionGroupsFocusing.AsList();

        /// <inheritdoc />
        public IXRInteractionGroup firstInteractionGroupFocusing { get; private set; }

        /// <inheritdoc />
        public bool isFocused => m_InteractionGroupsFocusing.Count > 0;

        /// <inheritdoc />
        public bool canFocus => m_FocusMode != InteractableFocusMode.None;

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
        /// Used as additional hover validations for this Interactable.
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
        /// Used as additional select validations for this Interactable.
        /// </summary>
        /// <remarks>
        /// While processing select filters, all changes to this list don't have an immediate effect. Theses changes are
        /// buffered and applied when the processing is finished.
        /// Calling <see cref="IXRFilterList{T}.MoveTo"/> in this list will throw an exception when this list is being processed.
        /// </remarks>
        /// <seealso cref="ProcessSelectFilters"/>
        public IXRFilterList<IXRSelectFilter> selectFilters => m_SelectFilters;

        [SerializeField]
        [RequireInterface(typeof(IXRInteractionStrengthFilter))]
        List<Object> m_StartingInteractionStrengthFilters = new List<Object>();

        /// <summary>
        /// The interaction strength filters that this object uses to automatically populate the <see cref="interactionStrengthFilters"/> List at
        /// startup (optional, may be empty).
        /// All objects in this list should implement the <see cref="IXRInteractionStrengthFilter"/> interface.
        /// </summary>
        /// <remarks>
        /// To access and modify the select filters used after startup, the <see cref="interactionStrengthFilters"/> List should
        /// be used instead.
        /// </remarks>
        /// <seealso cref="interactionStrengthFilters"/>
        public List<Object> startingInteractionStrengthFilters
        {
            get => m_StartingInteractionStrengthFilters;
            set => m_StartingInteractionStrengthFilters = value;
        }

        readonly ExposedRegistrationList<IXRInteractionStrengthFilter> m_InteractionStrengthFilters = new ExposedRegistrationList<IXRInteractionStrengthFilter> { bufferChanges = false };

        /// <summary>
        /// The list of interaction strength filters in this object.
        /// Used to modify the default interaction strength of an Interactor relative to this Interactable.
        /// This is useful for interactables that can be poked to report the depth of the poke interactor as a percentage
        /// while the poke interactor is hovering over this object.
        /// </summary>
        /// <remarks>
        /// While processing interaction strength filters, all changes to this list don't have an immediate effect. Theses changes are
        /// buffered and applied when the processing is finished.
        /// Calling <see cref="IXRFilterList{T}.MoveTo"/> in this list will throw an exception when this list is being processed.
        /// </remarks>
        /// <seealso cref="ProcessInteractionStrengthFilters"/>
        public IXRFilterList<IXRInteractionStrengthFilter> interactionStrengthFilters => m_InteractionStrengthFilters;

        readonly BindableVariable<float> m_LargestInteractionStrength = new BindableVariable<float>();

        /// <inheritdoc />
        public IReadOnlyBindableVariable<float> largestInteractionStrength => m_LargestInteractionStrength;

        readonly Dictionary<IXRSelectInteractor, Pose> m_AttachPoseOnSelect = new Dictionary<IXRSelectInteractor, Pose>();

        readonly Dictionary<IXRSelectInteractor, Pose> m_LocalAttachPoseOnSelect = new Dictionary<IXRSelectInteractor, Pose>();

        readonly Dictionary<IXRInteractor, GameObject> m_ReticleCache = new Dictionary<IXRInteractor, GameObject>();

        /// <summary>
        /// The set of hovered and/or selected interactors that supports returning a variable select input value,
        /// which is used as the pre-filtered interaction strength.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="XRBaseControllerInteractor"/> as the type to get the select input value to use as the pre-filtered
        /// interaction strength. This will be replaced with the interface for the select input value when the dependency
        /// on the <see cref="XRBaseController"/> is removed in a future version of XRI.
        /// </remarks>
        readonly HashSetList<XRBaseControllerInteractor> m_VariableSelectInteractors = new HashSetList<XRBaseControllerInteractor>();

        readonly Dictionary<IXRInteractor, float> m_InteractionStrengths = new Dictionary<IXRInteractor, float>();

        XRInteractionManager m_RegisteredInteractionManager;

        static readonly ProfilerMarker s_ProcessInteractionStrengthMarker = new ProfilerMarker("XRI.ProcessInteractionStrength.Interactables");

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
            // If no colliders were set, populate with children colliders
            if (m_Colliders.Count == 0)
            {
                GetComponentsInChildren(m_Colliders);
                // Skip any that are trigger colliders since these are usually associated with snap volumes.
                // If a user wants to use a trigger collider, they must serialize the reference manually.
                m_Colliders.RemoveAll(col => col.isTrigger);
            }

            // Setup the starting filters
            m_HoverFilters.RegisterReferences(m_StartingHoverFilters, this);
            m_SelectFilters.RegisterReferences(m_StartingSelectFilters, this);
            m_InteractionStrengthFilters.RegisterReferences(m_StartingInteractionStrengthFilters, this);

            // Setup Interaction Manager
            FindCreateInteractionManager();

            // Warn about use of deprecated events
            if (m_OnFirstHoverEntered.GetPersistentEventCount() > 0 ||
                m_OnLastHoverExited.GetPersistentEventCount() > 0 ||
                m_OnHoverEntered.GetPersistentEventCount() > 0 ||
                m_OnHoverExited.GetPersistentEventCount() > 0 ||
                m_OnSelectEntered.GetPersistentEventCount() > 0 ||
                m_OnSelectExited.GetPersistentEventCount() > 0 ||
                m_OnSelectCanceled.GetPersistentEventCount() > 0 ||
                m_OnActivate.GetPersistentEventCount() > 0 ||
                m_OnDeactivate.GetPersistentEventCount() > 0)
            {
                Debug.LogWarning("Some deprecated Interactable Events are being used. These deprecated events will be removed in a future version." +
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
        protected virtual void OnDestroy()
        {
            // Don't need to do anything; method kept for backwards compatibility.
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
                m_InteractionManager.RegisterInteractable(this);
#pragma warning restore 618
                m_RegisteredInteractionManager = m_InteractionManager;
            }
        }

        void UnregisterWithInteractionManager()
        {
            if (m_RegisteredInteractionManager == null)
                return;

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            m_RegisteredInteractionManager.UnregisterInteractable(this);
#pragma warning restore 618
            m_RegisteredInteractionManager = null;
        }

        /// <inheritdoc />
        public virtual Transform GetAttachTransform(IXRInteractor interactor)
        {
            return transform;
        }

        /// <inheritdoc />
        public Pose GetAttachPoseOnSelect(IXRSelectInteractor interactor)
        {
            return m_AttachPoseOnSelect.TryGetValue(interactor, out var pose) ? pose : Pose.identity;
        }

        /// <inheritdoc />
        public Pose GetLocalAttachPoseOnSelect(IXRSelectInteractor interactor)
        {
            return m_LocalAttachPoseOnSelect.TryGetValue(interactor, out var pose) ? pose : Pose.identity;
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method calls the <see cref="GetDistance"/> method to perform the distance calculation.
        /// </remarks>
        public virtual float GetDistanceSqrToInteractor(IXRInteractor interactor)
        {
            var interactorAttachTransform = interactor?.GetAttachTransform(this);
            if (interactorAttachTransform == null)
                return float.MaxValue;

            var interactorPosition = interactorAttachTransform.position;
            var distanceInfo = GetDistance(interactorPosition);
            return distanceInfo.distanceSqr;
        }

        /// <summary>
        /// Gets the distance from this Interactable to the given location.
        /// This method uses the calculation mode configured in <see cref="distanceCalculationMode"/>.
        /// <br />
        /// This method can be overridden (without needing to subclass) by assigning a callback to <see cref="getDistanceOverride"/>.
        /// To restore the previous calculation mode configuration, assign <see langword="null"/> to <see cref="getDistanceOverride"/>.
        /// </summary>
        /// <param name="position">Location in world space to calculate the distance to.</param>
        /// <returns>Returns the distance information (in world space) from this Interactable to the given location.</returns>
        /// <remarks>
        /// This method is used by other methods and systems to calculate this Interactable distance to other objects and
        /// locations (<see cref="GetDistanceSqrToInteractor(IXRInteractor)"/>).
        /// </remarks>
        public virtual DistanceInfo GetDistance(Vector3 position)
        {
            if (getDistanceOverride != null)
                return getDistanceOverride(this, position);

            switch (m_DistanceCalculationMode)
            {
                case DistanceCalculationMode.TransformPosition:
                    var thisObjectPosition = transform.position;
                    var offset = thisObjectPosition - position;
                    var distanceInfo = new DistanceInfo
                    {
                        point = thisObjectPosition,
                        distanceSqr = offset.sqrMagnitude
                    };
                    return distanceInfo;

                case DistanceCalculationMode.ColliderPosition:
                    XRInteractableUtility.TryGetClosestCollider(this, position, out distanceInfo);
                    return distanceInfo;

                case DistanceCalculationMode.ColliderVolume:
                    XRInteractableUtility.TryGetClosestPointOnCollider(this, position, out distanceInfo);
                    return distanceInfo;

                default:
                    Debug.Assert(false, $"Unhandled {nameof(DistanceCalculationMode)}={m_DistanceCalculationMode}.", this);
                    goto case DistanceCalculationMode.TransformPosition;
            }
        }

        /// <inheritdoc />
        public float GetInteractionStrength(IXRInteractor interactor)
        {
            if (m_InteractionStrengths.TryGetValue(interactor, out var interactionStrength))
                return interactionStrength;

            return 0f;
        }

        /// <summary>
        /// Determines if a given Interactor can hover over this Interactable.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid hover state with.</param>
        /// <returns>Returns <see langword="true"/> if hovering is valid this frame. Returns <see langword="false"/> if not.</returns>
        /// <seealso cref="IXRHoverInteractor.CanHover"/>
        public virtual bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            return m_AllowGazeInteraction || !(interactor is XRGazeInteractor);
        }

        /// <summary>
        /// Determines if a given Interactor can select this Interactable.
        /// </summary>
        /// <param name="interactor">Interactor to check for a valid selection with.</param>
        /// <returns>Returns <see langword="true"/> if selection is valid this frame. Returns <see langword="false"/> if not.</returns>
        /// <seealso cref="IXRSelectInteractor.CanSelect"/>
        public virtual bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            return (m_AllowGazeInteraction && m_AllowGazeSelect) || !(interactor is XRGazeInteractor);
        }

        /// <summary>
        /// Determines whether this Interactable is currently being hovered by the Interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check.</param>
        /// <returns>Returns <see langword="true"/> if this Interactable is currently being hovered by the Interactor.
        /// Otherwise, returns <seealso langword="false"/>.</returns>
        /// <remarks>
        /// In other words, returns whether <see cref="interactorsHovering"/> contains <paramref name="interactor"/>.
        /// </remarks>
        /// <seealso cref="interactorsHovering"/>
        public bool IsHovered(IXRHoverInteractor interactor) => m_InteractorsHovering.Contains(interactor);

        /// <summary>
        /// Determines whether this Interactable is currently being selected by the Interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check.</param>
        /// <returns>Returns <see langword="true"/> if this Interactable is currently being selected by the Interactor.
        /// Otherwise, returns <seealso langword="false"/>.</returns>
        /// <remarks>
        /// In other words, returns whether <see cref="interactorsSelecting"/> contains <paramref name="interactor"/>.
        /// </remarks>
        /// <seealso cref="interactorsSelecting"/>
        public bool IsSelected(IXRSelectInteractor interactor) => m_InteractorsSelecting.Contains(interactor);

        /// <summary>
        /// Determines whether this Interactable is currently being hovered by the Interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check.</param>
        /// <returns>Returns <see langword="true"/> if this Interactable is currently being hovered by the Interactor.
        /// Otherwise, returns <seealso langword="false"/>.</returns>
        /// <remarks>
        /// In other words, returns whether <see cref="interactorsHovering"/> contains <paramref name="interactor"/>.
        /// </remarks>
        /// <seealso cref="interactorsHovering"/>
        /// <seealso cref="IsHovered(IXRHoverInteractor)"/>
        protected bool IsHovered(IXRInteractor interactor) => interactor is IXRHoverInteractor hoverInteractor && IsHovered(hoverInteractor);

        /// <summary>
        /// Determines whether this Interactable is currently being selected by the Interactor.
        /// </summary>
        /// <param name="interactor">Interactor to check.</param>
        /// <returns>Returns <see langword="true"/> if this Interactable is currently being selected by the Interactor.
        /// Otherwise, returns <seealso langword="false"/>.</returns>
        /// <remarks>
        /// In other words, returns whether <see cref="interactorsSelecting"/> contains <paramref name="interactor"/>.
        /// </remarks>
        /// <seealso cref="interactorsSelecting"/>
        /// <seealso cref="IsSelected(IXRSelectInteractor)"/>
        protected bool IsSelected(IXRInteractor interactor) => interactor is IXRSelectInteractor selectInteractor && IsSelected(selectInteractor);

        /// <summary>
        /// Looks for the current custom reticle that is attached based on a specific Interactor.
        /// </summary>
        /// <param name="interactor">Interactor that is interacting with this Interactable.</param>
        /// <returns>Returns <see cref="GameObject"/> that represents the attached custom reticle.</returns>
        /// <seealso cref="AttachCustomReticle(IXRInteractor)"/>
        public virtual GameObject GetCustomReticle(IXRInteractor interactor)
        {
            if (m_ReticleCache.TryGetValue(interactor, out var reticle))
            {
                return reticle;
            }
            return null;
        }

        /// <summary>
        /// Attaches the custom reticle to the Interactor.
        /// </summary>
        /// <param name="interactor">Interactor that is interacting with this Interactable.</param>
        /// <seealso cref="RemoveCustomReticle(IXRInteractor)"/>
        public virtual void AttachCustomReticle(IXRInteractor interactor)
        {
            var interactorTransform = interactor?.transform;
            if (interactorTransform == null)
                return;

            // Try and find any attached reticle and swap it
            var reticleProvider = interactorTransform.GetComponent<IXRCustomReticleProvider>();
            if (reticleProvider != null)
            {
                if (m_ReticleCache.TryGetValue(interactor, out var prevReticle))
                {
                    Destroy(prevReticle);
                    m_ReticleCache.Remove(interactor);
                }

                if (m_CustomReticle != null)
                {
                    var reticleInstance = Instantiate(m_CustomReticle);
                    m_ReticleCache.Add(interactor, reticleInstance);
                    reticleProvider.AttachCustomReticle(reticleInstance);
                }
            }
        }

        /// <summary>
        /// Removes the custom reticle from the Interactor.
        /// </summary>
        /// <param name="interactor">Interactor that is no longer interacting with this Interactable.</param>
        /// <seealso cref="AttachCustomReticle(IXRInteractor)"/>
        public virtual void RemoveCustomReticle(IXRInteractor interactor)
        {
            var interactorTransform = interactor?.transform;
            if (interactorTransform == null)
                return;

            // Try and find any attached reticle and swap it
            var reticleProvider = interactorTransform.GetComponent<IXRCustomReticleProvider>();
            if (reticleProvider != null)
            {
                if (m_ReticleCache.TryGetValue(interactor, out var reticleInstance))
                {
                    Destroy(reticleInstance);
                    m_ReticleCache.Remove(interactor);
                    reticleProvider.RemoveCustomReticle();
                }
            }
        }

        /// <summary>
        /// Capture the current Attach Transform pose.
        /// This method is automatically called by Unity to capture the pose during the moment of selection.
        /// </summary>
        /// <param name="interactor">The specific Interactor as context to get the attachment point for.</param>
        /// <remarks>
        /// Unity automatically calls this method during <see cref="OnSelectEntering(SelectEnterEventArgs)"/>
        /// and should not typically need to be called by a user.
        /// </remarks>
        /// <seealso cref="GetAttachPoseOnSelect"/>
        /// <seealso cref="GetLocalAttachPoseOnSelect"/>
        /// <seealso cref="XRBaseInteractor.CaptureAttachPose"/>
        protected void CaptureAttachPose(IXRSelectInteractor interactor)
        {
            var thisAttachTransform = GetAttachTransform(interactor);
            if (thisAttachTransform != null)
            {
                m_AttachPoseOnSelect[interactor] =
                    new Pose(thisAttachTransform.position, thisAttachTransform.rotation);
                m_LocalAttachPoseOnSelect[interactor] =
                    new Pose(thisAttachTransform.localPosition, thisAttachTransform.localRotation);
            }
            else
            {
                m_AttachPoseOnSelect.Remove(interactor);
                m_LocalAttachPoseOnSelect.Remove(interactor);
            }
        }

        /// <inheritdoc />
        public virtual void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
        }

        /// <inheritdoc />
        void IXRInteractionStrengthInteractable.ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase) => ProcessInteractionStrength(updatePhase);

        /// <inheritdoc />
        void IXRInteractable.OnRegistered(InteractableRegisteredEventArgs args) => OnRegistered(args);

        /// <inheritdoc />
        void IXRInteractable.OnUnregistered(InteractableUnregisteredEventArgs args) => OnUnregistered(args);

        /// <inheritdoc />
        void IXRActivateInteractable.OnActivated(ActivateEventArgs args) => OnActivated(args);

        /// <inheritdoc />
        void IXRActivateInteractable.OnDeactivated(DeactivateEventArgs args) => OnDeactivated(args);

        /// <inheritdoc />
        bool IXRHoverInteractable.IsHoverableBy(IXRHoverInteractor interactor)
        {
            if (interactor is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                return IsHoverableBy(baseInteractor) && ProcessHoverFilters(interactor);
#pragma warning restore 618
            return IsHoverableBy(interactor) && ProcessHoverFilters(interactor);
        }

        /// <inheritdoc />
        void IXRHoverInteractable.OnHoverEntering(HoverEnterEventArgs args) => OnHoverEntering(args);

        /// <inheritdoc />
        void IXRHoverInteractable.OnHoverEntered(HoverEnterEventArgs args) => OnHoverEntered(args);

        /// <inheritdoc />
        void IXRHoverInteractable.OnHoverExiting(HoverExitEventArgs args) => OnHoverExiting(args);

        /// <inheritdoc />
        void IXRHoverInteractable.OnHoverExited(HoverExitEventArgs args) => OnHoverExited(args);

        /// <inheritdoc />
        bool IXRSelectInteractable.IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (interactor is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                return IsSelectableBy(baseInteractor) && ProcessSelectFilters(interactor);
#pragma warning restore 618
            return IsSelectableBy(interactor) && ProcessSelectFilters(interactor);
        }

        /// <inheritdoc />
        void IXRSelectInteractable.OnSelectEntering(SelectEnterEventArgs args) => OnSelectEntering(args);

        /// <inheritdoc />
        void IXRSelectInteractable.OnSelectEntered(SelectEnterEventArgs args) => OnSelectEntered(args);

        /// <inheritdoc />
        void IXRSelectInteractable.OnSelectExiting(SelectExitEventArgs args) => OnSelectExiting(args);

        /// <inheritdoc />
        void IXRSelectInteractable.OnSelectExited(SelectExitEventArgs args) => OnSelectExited(args);

        /// <inheritdoc />
        void IXRFocusInteractable.OnFocusEntering(FocusEnterEventArgs args) => OnFocusEntering(args);

        /// <inheritdoc />
        void IXRFocusInteractable.OnFocusEntered(FocusEnterEventArgs args) => OnFocusEntered(args);

        /// <inheritdoc />
        void IXRFocusInteractable.OnFocusExiting(FocusExitEventArgs args) => OnFocusExiting(args);

        /// <inheritdoc />
        void IXRFocusInteractable.OnFocusExited(FocusExitEventArgs args) => OnFocusExited(args);

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when this Interactable is registered with it.
        /// </summary>
        /// <param name="args">Event data containing the Interaction Manager that registered this Interactable.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.RegisterInteractable(IXRInteractable)"/>
        protected virtual void OnRegistered(InteractableRegisteredEventArgs args)
        {
            if (args.manager != m_InteractionManager)
                Debug.LogWarning($"An Interactable was registered with an unexpected {nameof(XRInteractionManager)}." +
                    $" {this} was expecting to communicate with \"{m_InteractionManager}\" but was registered with \"{args.manager}\".", this);

            registered?.Invoke(args);
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when this Interactable is unregistered from it.
        /// </summary>
        /// <param name="args">Event data containing the Interaction Manager that unregistered this Interactable.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="XRInteractionManager.UnregisterInteractable(IXRInteractable)"/>
        protected virtual void OnUnregistered(InteractableUnregisteredEventArgs args)
        {
            if (args.manager != m_RegisteredInteractionManager)
                Debug.LogWarning($"An Interactable was unregistered from an unexpected {nameof(XRInteractionManager)}." +
                    $" {this} was expecting to communicate with \"{m_RegisteredInteractionManager}\" but was unregistered from \"{args.manager}\".", this);

            unregistered?.Invoke(args);
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor first initiates hovering over an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is initiating the hover.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverEntered(HoverEnterEventArgs)"/>
        protected virtual void OnHoverEntering(HoverEnterEventArgs args)
        {
            if (m_CustomReticle != null)
            {
                if (args.interactorObject is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                    AttachCustomReticle(baseInteractor);
#pragma warning restore 618
                else
                    AttachCustomReticle(args.interactorObject);
            }

            var added = m_InteractorsHovering.Add(args.interactorObject);
            Debug.Assert(added, "An Interactable received a Hover Enter event for an Interactor that was already hovering over it.", this);

            if (args.interactorObject is XRBaseControllerInteractor variableSelectInteractor)
                m_VariableSelectInteractors.Add(variableSelectInteractor);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            hoveringInteractors.Add(args.interactor);
            OnHoverEntering(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor first initiates hovering over an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is initiating the hover.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverExited(HoverExitEventArgs)"/>
        protected virtual void OnHoverEntered(HoverEnterEventArgs args)
        {
            if (m_InteractorsHovering.Count == 1)
                m_FirstHoverEntered?.Invoke(args);

            m_HoverEntered?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnHoverEntered(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor ends hovering over an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is ending the hover.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverExited(HoverExitEventArgs)"/>
        protected virtual void OnHoverExiting(HoverExitEventArgs args)
        {
            if (m_CustomReticle != null)
            {
                if (args.interactorObject is XRBaseInteractor baseInteractor)
#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
                    RemoveCustomReticle(baseInteractor);
#pragma warning restore 618
                else
                    RemoveCustomReticle(args.interactorObject);
            }

            var removed = m_InteractorsHovering.Remove(args.interactorObject);
            Debug.Assert(removed, "An Interactable received a Hover Exit event for an Interactor that was not hovering over it.", this);

            if (m_VariableSelectInteractors.Count > 0 &&
                args.interactorObject is XRBaseControllerInteractor variableSelectInteractor &&
                !IsSelected(variableSelectInteractor))
            {
                m_VariableSelectInteractors.Remove(variableSelectInteractor);
            }

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            hoveringInteractors.Remove(args.interactor);
            OnHoverExiting(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor ends hovering over an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is ending the hover.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnHoverEntered(HoverEnterEventArgs)"/>
        protected virtual void OnHoverExited(HoverExitEventArgs args)
        {
            if (m_InteractorsHovering.Count == 0)
                m_LastHoverExited?.Invoke(args);

            m_HoverExited?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnHoverExited(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method right
        /// before the Interactor first initiates selection of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is initiating the selection.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectEntered(SelectEnterEventArgs)"/>
        protected virtual void OnSelectEntering(SelectEnterEventArgs args)
        {
            var added = m_InteractorsSelecting.Add(args.interactorObject);
            Debug.Assert(added, "An Interactable received a Select Enter event for an Interactor that was already selecting it.", this);

            if (args.interactorObject is XRBaseControllerInteractor variableSelectInteractor)
                m_VariableSelectInteractors.Add(variableSelectInteractor);

            if (m_InteractorsSelecting.Count == 1)
                firstInteractorSelecting = args.interactorObject;

            CaptureAttachPose(args.interactorObject);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnSelectEntering(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor first initiates selection of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is initiating the selection.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectExited(SelectExitEventArgs)"/>
        protected virtual void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_InteractorsSelecting.Count == 1)
                m_FirstSelectEntered?.Invoke(args);

            m_SelectEntered?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnSelectEntered(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interactor ends selection of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is ending the selection.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectExited(SelectExitEventArgs)"/>
        protected virtual void OnSelectExiting(SelectExitEventArgs args)
        {
            var removed = m_InteractorsSelecting.Remove(args.interactorObject);
            Debug.Assert(removed, "An Interactable received a Select Exit event for an Interactor that was not selecting it.", this);

            if (m_VariableSelectInteractors.Count > 0 &&
                args.interactorObject is XRBaseControllerInteractor variableSelectInteractor &&
                !IsHovered(variableSelectInteractor))
            {
                m_VariableSelectInteractors.Remove(variableSelectInteractor);
            }

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (args.isCanceled)
                OnSelectCanceling(args.interactor);
            else
                OnSelectExiting(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interactor ends selection of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is ending the selection.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnSelectEntered(SelectEnterEventArgs)"/>
        protected virtual void OnSelectExited(SelectExitEventArgs args)
        {
            if (m_InteractorsSelecting.Count == 0)
                m_LastSelectExited?.Invoke(args);

            m_SelectExited?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            if (args.isCanceled)
                OnSelectCanceled(args.interactor);
            else
                OnSelectExited(args.interactor);
#pragma warning restore 618

            // The dictionaries are pruned so that they don't infinitely grow in size as selections are made.
            if (m_InteractorsSelecting.Count == 0)
            {
                firstInteractorSelecting = null;
                m_AttachPoseOnSelect.Clear();
                m_LocalAttachPoseOnSelect.Clear();
            }
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method right
        /// before the Interaction group first gains focus of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is initiating focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusEntered(FocusEnterEventArgs)"/>
        protected virtual void OnFocusEntering(FocusEnterEventArgs args)
        {
            var added = m_InteractionGroupsFocusing.Add(args.interactionGroup);
            Debug.Assert(added, "An Interactable received a Focus Enter event for an Interaction group that was already focusing it.", this);

            if (m_InteractionGroupsFocusing.Count == 1)
                firstInteractionGroupFocusing = args.interactionGroup;
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interaction group first gains focus of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is initiating the focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusExited(FocusExitEventArgs)"/>
        protected virtual void OnFocusEntered(FocusEnterEventArgs args)
        {
            if (m_InteractionGroupsFocusing.Count == 1)
                m_FirstFocusEntered?.Invoke(args);

            m_FocusEntered?.Invoke(args);
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// right before the Interaction group loses focus of an Interactable
        /// in a first pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is losing focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusExited(FocusExitEventArgs)"/>
        protected virtual void OnFocusExiting(FocusExitEventArgs args)
        {
            var removed = m_InteractionGroupsFocusing.Remove(args.interactionGroup);
            Debug.Assert(removed, "An Interactable received a Focus Exit event for an Interaction group that did not have focus of it.", this);
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method
        /// when the Interaction group loses focus of an Interactable
        /// in a second pass.
        /// </summary>
        /// <param name="args">Event data containing the Interaction group that is losing focus.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnFocusEntered(FocusEnterEventArgs)"/>
        protected virtual void OnFocusExited(FocusExitEventArgs args)
        {
            if (m_InteractionGroupsFocusing.Count == 0)
                m_LastFocusExited?.Invoke(args);

            m_FocusExited?.Invoke(args);

            if (m_InteractionGroupsFocusing.Count == 0)
                firstInteractionGroupFocusing = null;
        }

        /// <summary>
        /// <see cref="XRBaseControllerInteractor"/> calls this method when the
        /// Interactor begins an activation event on this Interactable.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is sending the activate event.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnDeactivated"/>
        protected virtual void OnActivated(ActivateEventArgs args)
        {
            m_Activated?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnActivate(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// <see cref="XRBaseControllerInteractor"/> calls this method when the
        /// Interactor ends an activation event on this Interactable.
        /// </summary>
        /// <param name="args">Event data containing the Interactor that is sending the deactivate event.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnActivated"/>
        protected virtual void OnDeactivated(DeactivateEventArgs args)
        {
            m_Deactivated?.Invoke(args);

#pragma warning disable 618 // Calling deprecated method to help with backwards compatibility with existing user code.
            OnDeactivate(args.interactor);
#pragma warning restore 618
        }

        /// <summary>
        /// The <see cref="XRInteractionManager"/> calls this method to signal to update the interaction strength.
        /// </summary>
        /// <param name="updatePhase">The update phase during which this method is called.</param>
        /// <seealso cref="GetInteractionStrength"/>
        /// <seealso cref="IXRInteractionStrengthInteractable.ProcessInteractionStrength"/>
        protected virtual void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            var maxInteractionStrength = 0f;

            using (s_ProcessInteractionStrengthMarker.Auto())
            {
                m_InteractionStrengths.Clear();

                // Select is checked before Hover to allow process to only be called once per interactor hovering and selecting
                // using the largest initial interaction strength.
                for (int i = 0, count = m_InteractorsSelecting.Count; i < count; ++i)
                {
                    var interactor = m_InteractorsSelecting[i];
                    if (interactor is XRBaseControllerInteractor)
                        continue;

                    var interactionStrength = ProcessInteractionStrengthFilters(interactor, k_InteractionStrengthSelect);
                    m_InteractionStrengths[interactor] = interactionStrength;

                    maxInteractionStrength = Mathf.Max(maxInteractionStrength, interactionStrength);
                }

                for (int i = 0, count = m_InteractorsHovering.Count; i < count; ++i)
                {
                    var interactor = m_InteractorsHovering[i];
                    if (interactor is XRBaseControllerInteractor || IsSelected(interactor))
                        continue;

                    var interactionStrength = ProcessInteractionStrengthFilters(interactor, k_InteractionStrengthHover);
                    m_InteractionStrengths[interactor] = interactionStrength;

                    maxInteractionStrength = Mathf.Max(maxInteractionStrength, interactionStrength);
                }

                for (int i = 0, count = m_VariableSelectInteractors.Count; i < count; ++i)
                {
                    var interactor = m_VariableSelectInteractors[i];

                    // Use the Select input value as the initial interaction strength.
                    // For interactors that use motion controller input, this is typically the analog trigger or grip press amount.
                    // Fall back to the default values for selected and hovered interactors in the case when the interactor
                    // is misconfigured and is missing the component reference.
                    var interactionStrength = interactor.xrController != null
                        ? interactor.xrController.selectInteractionState.value
                        : (IsSelected(interactor) ? k_InteractionStrengthSelect : k_InteractionStrengthHover);
                    interactionStrength = ProcessInteractionStrengthFilters(interactor, interactionStrength);
                    m_InteractionStrengths[interactor] = interactionStrength;

                    maxInteractionStrength = Mathf.Max(maxInteractionStrength, interactionStrength);
                }
            }

            // This is done outside of the ProfilerMarker since it could trigger user callbacks
            m_LargestInteractionStrength.Value = maxInteractionStrength;
        }

        /// <summary>
        /// Returns the processing value of the filters in <see cref="hoverFilters"/> for the given Interactor and this
        /// Interactable.
        /// </summary>
        /// <param name="interactor">The Interactor to be validated by the hover filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="hoverFilters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool ProcessHoverFilters(IXRHoverInteractor interactor)
        {
            return XRFilterUtility.Process(m_HoverFilters, interactor, this);
        }

        /// <summary>
        /// Returns the processing value of the filters in <see cref="selectFilters"/> for the given Interactor and this
        /// Interactable.
        /// </summary>
        /// <param name="interactor">The Interactor to be validated by the select filters.</param>
        /// <returns>
        /// Returns <see langword="true"/> if all processed filters also return <see langword="true"/>, or if
        /// <see cref="selectFilters"/> is empty. Otherwise, returns <see langword="false"/>.
        /// </returns>
        protected bool ProcessSelectFilters(IXRSelectInteractor interactor)
        {
            return XRFilterUtility.Process(m_SelectFilters, interactor, this);
        }

        /// <summary>
        /// Returns the processing value of the interaction strength filters in <see cref="interactionStrengthFilters"/> for the given Interactor and this
        /// Interactable.
        /// </summary>
        /// <param name="interactor">The Interactor to process by the interaction strength filters.</param>
        /// <param name="interactionStrength">The interaction strength before processing.</param>
        /// <returns>Returns the modified interaction strength that is the result of passing the interaction strength through each filter.</returns>
        protected float ProcessInteractionStrengthFilters(IXRInteractor interactor, float interactionStrength)
        {
            return XRFilterUtility.Process(m_InteractionStrengthFilters, interactor, this, interactionStrength);
        }
    }
}