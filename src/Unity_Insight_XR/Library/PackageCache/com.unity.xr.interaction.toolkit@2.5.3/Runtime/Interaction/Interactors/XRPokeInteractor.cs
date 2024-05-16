using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor used for interacting with interactables through poking.
    /// </summary>
    /// <seealso cref="XRPokeFilter"/>
    [AddComponentMenu("XR/XR Poke Interactor", 11)]
    [HelpURL(XRHelpURLConstants.k_XRPokeInteractor)]
    public class XRPokeInteractor : XRBaseInteractor, IUIHoverInteractor, IPokeStateDataProvider
    {
        /// <summary>
        /// Reusable list of interactables (used to process the valid targets when this interactor has a filter).
        /// </summary>
        static readonly List<IXRInteractable> s_Results = new List<IXRInteractable>();

        [SerializeField]
        [Tooltip("The depth threshold within which an interaction can begin to be evaluated as a poke.")]
        float m_PokeDepth = 0.1f;

        /// <summary>
        /// The depth threshold within which an interaction can begin to be evaluated as a poke.
        /// </summary>
        public float pokeDepth
        {
            get => m_PokeDepth;
            set => m_PokeDepth = value;
        }

        [SerializeField]
        [Tooltip("The width threshold within which an interaction can begin to be evaluated as a poke.")]
        float m_PokeWidth = 0.0075f;

        /// <summary>
        /// The width threshold within which an interaction can begin to be evaluated as a poke.
        /// </summary>
        public float pokeWidth
        {
            get => m_PokeWidth;
            set => m_PokeWidth = value;
        }

        [SerializeField]
        [Tooltip("The width threshold within which an interaction can be evaluated as a poke select.")]
        float m_PokeSelectWidth = 0.015f;

        /// <summary>
        /// The width threshold within which an interaction can be evaluated as a poke select.
        /// </summary>
        public float pokeSelectWidth
        {
            get => m_PokeSelectWidth;
            set => m_PokeSelectWidth = value;
        }

        [SerializeField]
        [Tooltip("The radius threshold within which an interaction can be evaluated as a poke hover.")]
        float m_PokeHoverRadius = 0.015f;

        /// <summary>
        /// The radius threshold within which an interaction can be evaluated as a poke hover.
        /// </summary>
        public float pokeHoverRadius
        {
            get => m_PokeHoverRadius;
            set => m_PokeHoverRadius = value;
        }

        [SerializeField]
        [Tooltip("Distance along the poke interactable interaction axis that allows for a poke to be triggered sooner/with less precision.")]
        float m_PokeInteractionOffset = 0.005f;

        /// <summary>
        /// Distance along the poke interactable interaction axis that allows for a poke to be triggered sooner/with less precision.
        /// </summary>
        public float pokeInteractionOffset
        {
            get => m_PokeInteractionOffset;
            set => m_PokeInteractionOffset = value;
        }

        [SerializeField]
        [Tooltip("Physics layer mask used for limiting poke sphere overlap.")]
        LayerMask m_PhysicsLayerMask = Physics.AllLayers;

        /// <summary>
        /// Physics layer mask used for limiting poke sphere overlap.
        /// </summary>
        public LayerMask physicsLayerMask
        {
            get => m_PhysicsLayerMask;
            set => m_PhysicsLayerMask = value;
        }

        [SerializeField]
        [Tooltip("Determines whether the poke sphere overlap will hit triggers.")]
        QueryTriggerInteraction m_PhysicsTriggerInteraction = QueryTriggerInteraction.Ignore;

        /// <summary>
        /// Determines whether the poke sphere overlap will hit triggers.
        /// </summary>
        public QueryTriggerInteraction physicsTriggerInteraction
        {
            get => m_PhysicsTriggerInteraction;
            set => m_PhysicsTriggerInteraction = value;
        }

        [SerializeField]
        [Tooltip("Denotes whether or not valid targets will only include objects with a poke filter.")]
        bool m_RequirePokeFilter = true;

        /// <summary>
        /// Denotes whether or not valid targets will only include objects with a poke filter.
        /// </summary>
        public bool requirePokeFilter
        {
            get => m_RequirePokeFilter;
            set => m_RequirePokeFilter = value;
        }

        [SerializeField]
        [Tooltip("When enabled, this allows the poke interactor to hover and select UI elements.")]
        bool m_EnableUIInteraction = true;

        /// <summary>
        /// Gets or sets whether this Interactor is able to affect UI.
        /// </summary>
        public bool enableUIInteraction
        {
            get => m_EnableUIInteraction;
            set
            {
                if (m_EnableUIInteraction != value)
                {
                    m_EnableUIInteraction = value;
                    m_RegisteredUIInteractorCache?.RegisterOrUnregisterXRUIInputModule(m_EnableUIInteraction);
                }
            }
        }

        [SerializeField]
        [Tooltip("Denotes whether or not debug visuals are enabled for this poke interactor.")]
        bool m_DebugVisualizationsEnabled;

        /// <summary>
        /// Denotes whether or not debug visuals are enabled for this poke interactor.
        /// Debug visuals include two spheres to display whether or not hover and select colliders have collided.
        /// </summary>
        public bool debugVisualizationsEnabled
        {
            get => m_DebugVisualizationsEnabled;
            set => m_DebugVisualizationsEnabled = value;
        }

        [SerializeField]
        UIHoverEnterEvent m_UIHoverEntered = new UIHoverEnterEvent();

        /// <inheritdoc />
        public UIHoverEnterEvent uiHoverEntered
        {
            get => m_UIHoverEntered;
            set => m_UIHoverEntered = value;
        }

        [SerializeField]
        UIHoverExitEvent m_UIHoverExited = new UIHoverExitEvent();

        /// <inheritdoc />
        public UIHoverExitEvent uiHoverExited
        {
            get => m_UIHoverExited;
            set => m_UIHoverExited = value;
        }

        BindableVariable<PokeStateData> m_PokeStateData = new BindableVariable<PokeStateData>();

        /// <inheritdoc />
        public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeStateData;

        GameObject m_HoverDebugSphere;
        MeshRenderer m_HoverDebugRenderer;

        Vector3 m_LastPokeInteractionPoint;

        bool m_FirstFrame = true;
        IXRSelectInteractable m_CurrentPokeTarget;
        IXRPokeFilter m_CurrentPokeFilter;

        readonly RaycastHit[] m_SphereCastHits = new RaycastHit[25];
        readonly Collider[] m_OverlapSphereHits = new Collider[25];
        readonly List<PokeCollision> m_PokeTargets = new List<PokeCollision>();
        readonly List<IXRSelectFilter> m_InteractableSelectFilters = new List<IXRSelectFilter>();
        
        readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();
        static readonly Dictionary<IXRInteractable, IXRPokeFilter> s_ValidTargetsScratchMap = new Dictionary<IXRInteractable, IXRPokeFilter>();

        RegisteredUIInteractorCache m_RegisteredUIInteractorCache;
        PhysicsScene m_LocalPhysicsScene;

        // Used to avoid GC Alloc each frame in UpdateUIModel
        Func<Vector3> m_PositionGetter;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            useAttachPointVelocity = true;
            m_LocalPhysicsScene = gameObject.scene.GetPhysicsScene();
            m_RegisteredUIInteractorCache = new RegisteredUIInteractorCache(this);
            m_PositionGetter = GetPokePosition;
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            SetDebugObjectVisibility(true);
            m_FirstFrame = true;

            if (m_EnableUIInteraction)
                m_RegisteredUIInteractorCache.RegisterWithXRUIInputModule();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            SetDebugObjectVisibility(false);

            if (m_EnableUIInteraction)
                m_RegisteredUIInteractorCache.UnregisterFromXRUIInputModule();
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        /// <inheritdoc />
        public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.PreprocessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                TrackedDeviceGraphicRaycaster.ValidatePokeInteractionData(this);
                isInteractingWithUI = TrackedDeviceGraphicRaycaster.IsPokeInteractingWithUI(this);
                RegisterValidTargets(out m_CurrentPokeTarget, out m_CurrentPokeFilter);
                ProcessPokeStateData();
            }
        }

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                UpdateDebugVisuals();
        }

        bool RegisterValidTargets(out IXRSelectInteractable currentTarget, out IXRPokeFilter pokeFilter)
        {
            int sphereOverlapCount = EvaluateSphereOverlap();
            bool hasOverlap = sphereOverlapCount > 0;
            
            m_ValidTargets.Clear();
            s_ValidTargetsScratchMap.Clear();

            if (hasOverlap)
            {
                
                int pokeTargetsCount = m_PokeTargets.Count;
                for (var i = 0; i < pokeTargetsCount; ++i)
                {
                    var interactable = m_PokeTargets[i].interactable;
                    if (interactable is not (IXRSelectInteractable and IXRHoverInteractable hoverable) || !hoverable.IsHoverableBy(this))
                        continue;
                    m_ValidTargets.Add(m_PokeTargets[i].interactable);
                    s_ValidTargetsScratchMap.Add(m_PokeTargets[i].interactable, m_PokeTargets[i].filter);
                }
                
                // Sort before target filter
                if (m_ValidTargets.Count > 1)
                {
                    SortingHelpers.SortByDistanceToInteractor(this, m_ValidTargets, s_Results);
                    m_ValidTargets.Clear();
                    m_ValidTargets.AddRange(s_Results);
                }
                
                var filter = targetFilter;
                if (filter != null && filter.canProcess)
                {
                    filter.Process(this, m_ValidTargets, s_Results);

                    // Copy results elements to targets
                    m_ValidTargets.Clear();
                    m_ValidTargets.AddRange(s_Results);
                }
                
                if (m_ValidTargets.Count == 0)
                    hasOverlap = false;
            }
            currentTarget = hasOverlap ? (IXRSelectInteractable)m_ValidTargets[0] : null;
            pokeFilter = hasOverlap ? s_ValidTargetsScratchMap[currentTarget] : null;
            return hasOverlap;
        }

        void ProcessPokeStateData()
        {
            PokeStateData newPokeStateData = default;
            if (isInteractingWithUI)
            {
                TrackedDeviceGraphicRaycaster.TryGetPokeStateDataForInteractor(this, out newPokeStateData);
            }
            else if (m_CurrentPokeFilter != null && m_CurrentPokeFilter is IPokeStateDataProvider pokeStateDataProvider)
            {
                newPokeStateData = pokeStateDataProvider.pokeStateData.Value;
            }
            m_PokeStateData.Value = newPokeStateData;
        }

        /// <inheritdoc />
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();

            if (!isActiveAndEnabled)
                return;

            if(m_ValidTargets.Count > 0)
                targets.Add(m_ValidTargets[0]);
        }

        int EvaluateSphereOverlap()
        {
            m_PokeTargets.Clear();

            // Hover Check
            Vector3 pokeInteractionPoint = GetAttachTransform(null).position;
            Vector3 overlapStart = m_LastPokeInteractionPoint;
            Vector3 interFrameEnd = pokeInteractionPoint;

            BurstPhysicsUtils.GetSphereOverlapParameters(overlapStart, interFrameEnd, out var normalizedOverlapVector, out var overlapSqrMagnitude, out var overlapDistance);

            // If no movement is recorded.
            // Check if spherecast size is sufficient for proper cast, or if first frame since last frame poke position will be invalid.
            if (m_FirstFrame || overlapSqrMagnitude < 0.001f)
            {
                var numberOfOverlaps = m_LocalPhysicsScene.OverlapSphere(interFrameEnd, m_PokeHoverRadius, m_OverlapSphereHits,
                    m_PhysicsLayerMask, m_PhysicsTriggerInteraction);

                for (var i = 0; i < numberOfOverlaps; ++i)
                {
                    if (FindPokeTarget(m_OverlapSphereHits[i], out var newPokeCollision))
                    {
                        m_PokeTargets.Add(newPokeCollision);
                    }
                }
            }
            else
            {
                var numberOfOverlaps = m_LocalPhysicsScene.SphereCast(
                    overlapStart,
                    m_PokeHoverRadius,
                    normalizedOverlapVector,
                    m_SphereCastHits,
                    overlapDistance,
                    m_PhysicsLayerMask,
                    m_PhysicsTriggerInteraction);

                for (var i = 0; i < numberOfOverlaps; ++i)
                {
                    if (FindPokeTarget(m_SphereCastHits[i].collider, out var newPokeCollision))
                    {
                        m_PokeTargets.Add(newPokeCollision);
                    }
                }
            }

            m_LastPokeInteractionPoint = pokeInteractionPoint;
            m_FirstFrame = false;

            return m_PokeTargets.Count;
        }

        bool FindPokeTarget(Collider hitCollider, out PokeCollision newPokeCollision)
        {
            newPokeCollision = default;
            if (interactionManager.TryGetInteractableForCollider(hitCollider, out var interactable))
            {
                if (m_RequirePokeFilter)
                {
                    if (interactable is XRBaseInteractable baseInteractable)
                    {
                        baseInteractable.selectFilters.GetAll(m_InteractableSelectFilters);
                        foreach (var filter in m_InteractableSelectFilters)
                        {
                            if (filter is XRPokeFilter pokeFilter && filter.canProcess)
                            {
                                newPokeCollision = new PokeCollision(interactable, pokeFilter);
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    newPokeCollision = new PokeCollision(interactable, null);
                    return true;
                }
            }

            return false;
        }

        void SetDebugObjectVisibility(bool isVisible)
        {
            if (m_DebugVisualizationsEnabled)
            {
                if (m_HoverDebugSphere == null)
                {
                    m_HoverDebugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_HoverDebugSphere.name = "[Debug] Poke - HoverVisual: " + this;
                    m_HoverDebugSphere.transform.SetParent(GetAttachTransform(null), false);
                    m_HoverDebugSphere.transform.localScale = new Vector3(m_PokeHoverRadius, m_PokeHoverRadius, m_PokeHoverRadius);

                    if (m_HoverDebugSphere.TryGetComponent<Collider>(out var debugCollider))
                        Destroy(debugCollider);

                    m_HoverDebugRenderer = GetOrAddComponent<MeshRenderer>(m_HoverDebugSphere);
                }
            }

            var visibility = m_DebugVisualizationsEnabled && isVisible;

            if (m_HoverDebugSphere != null && m_HoverDebugSphere.activeSelf != visibility)
                m_HoverDebugSphere.SetActive(visibility);
        }

        void UpdateDebugVisuals()
        {
            SetDebugObjectVisibility(m_CurrentPokeTarget != null);

            if (!m_DebugVisualizationsEnabled)
                return;
            m_HoverDebugRenderer.material.color = m_PokeTargets.Count > 0 ? new Color(0f, 0.8f, 0f, 0.1f) : new Color(0.8f, 0f, 0f, 0.1f);
        }

        static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            return go.TryGetComponent<T>(out var component) ? component : go.AddComponent<T>();
        }

        readonly struct PokeCollision
        {
            public readonly IXRInteractable interactable;
            public readonly IXRPokeFilter filter;

            public PokeCollision(IXRInteractable interactable, IXRPokeFilter filter)
            {
                this.interactable = interactable;
                this.filter = filter;
            }
        }

        /// <inheritdoc />
        public virtual void UpdateUIModel(ref TrackedDeviceModel model)
        {
            if (!isActiveAndEnabled || this.IsBlockedByInteractionWithinGroup())
            {
                model.Reset(false);
                return;
            }

            var pokeInteractionTransform = GetAttachTransform(null);
            var position = pokeInteractionTransform.position;
            var orientation = pokeInteractionTransform.rotation;
            Vector3 startPoint = position;
            Vector3 penetrationDirection = orientation * Vector3.forward;
            Vector3 endPoint = startPoint + (penetrationDirection * m_PokeDepth);

            model.position = position;
            model.orientation = orientation;
            model.positionGetter = m_PositionGetter;
            model.select = TrackedDeviceGraphicRaycaster.HasPokeSelect(this);
            model.raycastLayerMask = m_PhysicsLayerMask;
            model.pokeDepth = m_PokeDepth;
            model.interactionType = UIInteractionType.Poke;

            var raycastPoints = model.raycastPoints;
            raycastPoints.Clear();
            raycastPoints.Add(startPoint);
            raycastPoints.Add(endPoint);
        }

        Vector3 GetPokePosition()
        {
            return GetAttachTransform(null).position;
        }

        /// <inheritdoc />
        public bool TryGetUIModel(out TrackedDeviceModel model)
        {
            return m_RegisteredUIInteractorCache.TryGetUIModel(out model);
        }

        /// <inheritdoc />
        void IUIHoverInteractor.OnUIHoverEntered(UIHoverEventArgs args) => OnUIHoverEntered(args);

        /// <inheritdoc />
        void IUIHoverInteractor.OnUIHoverExited(UIHoverEventArgs args) => OnUIHoverExited(args);

        /// <summary>
        /// The <see cref="XRUIInputModule"/> calls this method when the Interactor begins hovering over a UI element.
        /// </summary>
        /// <param name="args">Event data containing the UI element that is being hovered over.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnUIHoverExited(UIHoverEventArgs)"/>
        protected virtual void OnUIHoverEntered(UIHoverEventArgs args)
        {
            m_UIHoverEntered?.Invoke(args);
        }

        /// <summary>
        /// The <see cref="XRUIInputModule"/> calls this method when the Interactor ends hovering over a UI element.
        /// </summary>
        /// <param name="args">Event data containing the UI element that is no longer hovered over.</param>
        /// <remarks>
        /// <paramref name="args"/> is only valid during this method call, do not hold a reference to it.
        /// </remarks>
        /// <seealso cref="OnUIHoverEntered(UIHoverEventArgs)"/>
        protected virtual void OnUIHoverExited(UIHoverEventArgs args)
        {
            m_UIHoverExited?.Invoke(args);
        }
    }
}
