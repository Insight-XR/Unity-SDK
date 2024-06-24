using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    /// <summary>
    /// Custom implementation of <see cref="GraphicRaycaster"/> for XR Interaction Toolkit.
    /// This behavior is used to ray cast against a <see cref="Canvas"/>. The Raycaster looks
    /// at all Graphics on the canvas and determines if any of them have been hit by a ray
    /// from a tracked device.
    /// </summary>
    [AddComponentMenu("Event/Tracked Device Graphic Raycaster", 11)]
    [HelpURL(XRHelpURLConstants.k_TrackedDeviceGraphicRaycaster)]
    public class TrackedDeviceGraphicRaycaster : BaseRaycaster, IPokeStateDataProvider, IMultiPokeStateDataProvider
    {
        const int k_MaxRaycastHits = 10;

        readonly struct RaycastHitData
        {
            public RaycastHitData(Graphic graphic, Vector3 worldHitPosition, Vector2 screenPosition, float distance, int displayIndex)
            {
                this.graphic = graphic;
                this.worldHitPosition = worldHitPosition;
                this.screenPosition = screenPosition;
                this.distance = distance;
                this.displayIndex = displayIndex;
            }

            public Graphic graphic { get; }
            public Vector3 worldHitPosition { get; }
            public Vector2 screenPosition { get; }
            public float distance { get; }
            public int displayIndex { get; }
        }

        /// <summary>
        /// Compares ray cast hits by graphic depth, to sort in descending order.
        /// </summary>
        sealed class RaycastHitComparer : IComparer<RaycastHitData>
        {
            public int Compare(RaycastHitData a, RaycastHitData b)
                => b.graphic.depth.CompareTo(a.graphic.depth);
        }

        [SerializeField]
        [Tooltip("Whether Graphics facing away from the ray caster are checked for ray casts. Enable this to ignore backfacing Graphics.")]
        bool m_IgnoreReversedGraphics;

        /// <summary>
        /// Whether Graphics facing away from the ray caster are checked for ray casts.
        /// Enable this to ignore backfacing Graphics.
        /// </summary>
        public bool ignoreReversedGraphics
        {
            get => m_IgnoreReversedGraphics;
            set => m_IgnoreReversedGraphics = value;
        }

        [SerializeField]
        [Tooltip("Whether or not 2D occlusion is checked when performing ray casts. Enable to make Graphics be blocked by 2D objects that exist in front of it.")]
        bool m_CheckFor2DOcclusion;

        /// <summary>
        /// Whether or not 2D occlusion is checked when performing ray casts.
        /// Enable to make Graphics be blocked by 2D objects that exist in front of it.
        /// </summary>
        /// <remarks>
        /// This property has no effect when the project does not include the Physics 2D module.
        /// </remarks>
        public bool checkFor2DOcclusion
        {
            get => m_CheckFor2DOcclusion;
            set => m_CheckFor2DOcclusion = value;
        }

        [SerializeField]
        [Tooltip("Whether or not 3D occlusion is checked when performing ray casts. Enable to make Graphics be blocked by 3D objects that exist in front of it.")]
        bool m_CheckFor3DOcclusion;

        /// <summary>
        /// Whether or not 3D occlusion is checked when performing ray casts.
        /// Enable to make Graphics be blocked by 3D objects that exist in front of it.
        /// </summary>
        public bool checkFor3DOcclusion
        {
            get => m_CheckFor3DOcclusion;
            set => m_CheckFor3DOcclusion = value;
        }

        [SerializeField]
        [Tooltip("The layers of objects that are checked to determine if they block Graphic ray casts when checking for 2D or 3D occlusion.")]
        LayerMask m_BlockingMask = -1;

        /// <summary>
        /// The layers of objects that are checked to determine if they block Graphic ray casts
        /// when checking for 2D or 3D occlusion.
        /// </summary>
        public LayerMask blockingMask
        {
            get => m_BlockingMask;
            set => m_BlockingMask = value;
        }

        [SerializeField]
        [Tooltip("Specifies whether the ray cast should hit Triggers when checking for 3D occlusion.")]
        QueryTriggerInteraction m_RaycastTriggerInteraction = QueryTriggerInteraction.Ignore;

        /// <summary>
        /// Specifies whether the ray cast should hit Triggers when checking for 3D occlusion.
        /// </summary>
        public QueryTriggerInteraction raycastTriggerInteraction
        {
            get => m_RaycastTriggerInteraction;
            set => m_RaycastTriggerInteraction = value;
        }

        /// <summary>
        /// See [BaseRaycaster.eventCamera](xref:UnityEngine.EventSystems.BaseRaycaster.eventCamera).
        /// </summary>
        public override Camera eventCamera => canvas != null && canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

        /// <summary>
        /// Performs a ray cast against objects within this Raycaster's domain.
        /// </summary>
        /// <param name="eventData">Data containing where and how to ray cast.</param>
        /// <param name="resultAppendList">The resultant hits from the ray cast.</param>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (eventData is TrackedDeviceEventData trackedEventData)
            {
                PerformRaycasts(trackedEventData, resultAppendList);
            }
        }

        Canvas m_Canvas;

        Canvas canvas
        {
            get
            {
                if (m_Canvas != null)
                    return m_Canvas;

                TryGetComponent(out m_Canvas);
                return m_Canvas;
            }
        }

        bool m_HasWarnedEventCameraNull;

        readonly RaycastHit[] m_OcclusionHits3D = new RaycastHit[k_MaxRaycastHits];
#if PHYSICS2D_MODULE_PRESENT
        // Create for a single hit only. In 2D physics it'll always be the closest hit.
        readonly RaycastHit2D[] m_OcclusionHits2D = new RaycastHit2D[1];
#endif
        static readonly RaycastHitComparer s_RaycastHitComparer = new RaycastHitComparer();

        static readonly Vector3[] s_Corners = new Vector3[4];

        // Use this list on each ray cast to avoid continually allocating.
        readonly List<RaycastHitData> m_RaycastResultsCache = new List<RaycastHitData>();

        [NonSerialized]
        static readonly List<RaycastHitData> s_SortedGraphics = new List<RaycastHitData>();

        // Poke-specific variables and methods
        XRPokeLogic m_PokeLogic;

        [NonSerialized]
        static readonly Dictionary<IUIInteractor, TrackedDeviceGraphicRaycaster> s_InteractorRaycasters = new Dictionary<IUIInteractor, TrackedDeviceGraphicRaycaster>();

        [NonSerialized]
        static readonly Dictionary<TrackedDeviceGraphicRaycaster, HashSet<IUIInteractor>> s_PokeHoverRaycasters = new Dictionary<TrackedDeviceGraphicRaycaster, HashSet<IUIInteractor>>();

        /// <summary>
        /// Checks if poke interactor is interacting with any raycaster in the scene. 
        /// </summary>
        /// <param name="interactor">Poke ui interactor to check.</param>
        /// <returns>True if any poke interactor is hovering or selecting a graphic in the scene.</returns>
        internal static bool IsPokeInteractingWithUI(IUIInteractor interactor)
        {
            foreach (var pokeUIInteractorSet in s_PokeHoverRaycasters.Values)
            {
                if (pokeUIInteractorSet.Contains(interactor))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes interactor from poke data and calls OnHoverExited on the <see cref="XRPokeLogic"/>.
        /// </summary>
        /// <param name="interactor">Interactor to end the poke interaction.</param>
        void EndPokeInteraction(IUIInteractor interactor)
        {
            if (interactor == null)
                return;
            
            m_PokeLogic.OnHoverExited(interactor);
            s_InteractorRaycasters.Remove(interactor);
            s_PokeHoverRaycasters[this].Remove(interactor);
        }

        /// <summary>
        /// Attempts to get the <see cref="PokeStateData"/> for the provided <see cref="IUIInteractor"/>.
        /// </summary>
        /// <param name="interactor">The <see cref="IUIInteractor"/> to check.</param>
        /// <param name="data">The <see cref="PokeStateData"/> associated with the <see cref="IUIInteractor"/> if it is found.</param>
        /// <returns>Returns <see langword="true"/> if the <see cref="IUIInteractor"/> is found and its associated <see cref="PokeStateData"/> is retrieved successfully, otherwise returns <see langword="false"/>.</returns>
        internal static bool TryGetPokeStateDataForInteractor(IUIInteractor interactor, out PokeStateData data)
        {
            foreach (var kvp in s_PokeHoverRaycasters)
            {
                var pokeUIInteractorSet = kvp.Value;
                if (pokeUIInteractorSet.Contains(interactor))
                {
                    var raycaster = kvp.Key;
                    data = raycaster.pokeStateData.Value;
                    return true;
                }
            }

            data = default;
            return false;
        }

        /// <inheritdoc />
        public IReadOnlyBindableVariable<PokeStateData> pokeStateData => m_PokeLogic?.pokeStateData;

        Dictionary<Transform, BindableVariable<PokeStateData>> pokeStateDataDictionary { get; } = new Dictionary<Transform, BindableVariable<PokeStateData>>();

        BindingsGroup m_BindingsGroup = new BindingsGroup();
        
        /// <summary>
        /// Gets the <see cref="PokeStateData"/> as a <see cref="IReadOnlyBindableVariable{TValue}"/> for the target transform.
        /// </summary>
        /// <param name="target">The target to get the <see cref="PokeStateData"/> for.</param>
        /// <returns>Returns a <see cref="IReadOnlyBindableVariable{TValue}"/> for the <see cref="PokeStateData"/> for the target.</returns>
        public IReadOnlyBindableVariable<PokeStateData> GetPokeStateDataForTarget(Transform target)
        {
            if (!pokeStateDataDictionary.ContainsKey(target))
                pokeStateDataDictionary[target] = new BindableVariable<PokeStateData>();
            return pokeStateDataDictionary[target];
        }

        /// <summary>
        /// This method is used to determine if the <see cref="IUIInteractor"/> has a currently active selection using poke.
        /// </summary>
        /// <param name="interactor">The <see cref="IUIInteractor"/> to check against, typically a <see cref="XRPokeInteractor"/>.</param>
        /// <returns>Returns <see langword="true"/> if the <see cref="IUIInteractor"/> meets requirements for poke with any <see cref="TrackedDeviceGraphicRaycaster"/>.</returns>
        internal static bool HasPokeSelect(IUIInteractor interactor)
        {
            return s_InteractorRaycasters.TryGetValue(interactor, out var raycaster) && raycaster != null;
        }

        PhysicsScene m_LocalPhysicsScene;
#if PHYSICS2D_MODULE_PRESENT
        PhysicsScene2D m_LocalPhysicsScene2D;
#endif

        static RaycastHit FindClosestHit(RaycastHit[] hits, int count)
        {
            var index = 0;
            var distance = float.MaxValue;
            for (var i = 0; i < count; i++)
            {
                if (hits[i].distance < distance)
                {
                    distance = hits[i].distance;
                    index = i;
                }
            }

            return hits[index];
        }

        /// <summary>
        /// See <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Awake.html">MonoBehaviour.Awake</a>.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_LocalPhysicsScene = gameObject.scene.GetPhysicsScene();
#if PHYSICS2D_MODULE_PRESENT
            m_LocalPhysicsScene2D = gameObject.scene.GetPhysicsScene2D();
#endif
            s_PokeHoverRaycasters.Add(this, new HashSet<IUIInteractor>());
            SetupPoke();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            // Clean up any existing data of interactors hovering or selecting this disabled TrackedDeviceGraphicRaycaster 
            using (HashSetPool<IUIInteractor>.Get(out var interactorHashSet))
            {
                foreach (var kvp in s_InteractorRaycasters)
                {
                    if (kvp.Value == this)
                        interactorHashSet.Add(kvp.Key);
                }
                foreach (var interactor in s_PokeHoverRaycasters[this])
                {
                    interactorHashSet.Add(interactor);
                }
                // End poke interaction on each interactor, which calls OnHoverExited
                foreach (var interactor in interactorHashSet)
                {
                    EndPokeInteraction(interactor);
                }
            }
        }

        /// <summary>
        /// See <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDestroy.html">MonoBehaviour.OnDestroy</a>.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            s_PokeHoverRaycasters.Remove(this);
            pokeStateDataDictionary.Clear();
            m_BindingsGroup.Clear();
        }

        void SetupPoke()
        {
            m_BindingsGroup.Clear();
            if (m_PokeLogic == null)
                m_PokeLogic = new XRPokeLogic();

            var pokeData = new PokeThresholdData
            {
                pokeDirection = PokeAxis.Z,
                interactionDepthOffset = 0f,
                enablePokeAngleThreshold = true,
                pokeAngleThreshold = 89.9f,
            };

            m_PokeLogic.Initialize(transform, pokeData, null);
            m_PokeLogic.SetPokeDepth(0.1f);
            m_BindingsGroup.AddBinding(m_PokeLogic.pokeStateData.SubscribeAndUpdate(data =>
            {
                if (data.target != null)
                {
                    if (!pokeStateDataDictionary.ContainsKey(data.target))
                        pokeStateDataDictionary[data.target] = new BindableVariable<PokeStateData>();
                    pokeStateDataDictionary[data.target].Value = data;
                }
                else
                {
                    // If we get a null target, we should reset targetted listeners.
                    foreach (var value in pokeStateDataDictionary.Values)
                    {
                        value.Value = data;
                    }
                }
            }));
        }

        void PerformRaycasts(TrackedDeviceEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (canvas == null)
                return;

            // Property can call Camera.main, so cache the reference
            var currentEventCamera = eventCamera;
            if (currentEventCamera == null)
            {
                if (!m_HasWarnedEventCameraNull)
                {
                    Debug.LogWarning("Event Camera must be set on World Space Canvas to perform ray casts with tracked device." +
                        " UI events will not function correctly until it is set.",
                        this);
                    m_HasWarnedEventCameraNull = true;
                }

                return;
            }

            var layerMask = eventData.layerMask;
            var interactor = eventData.interactor;

            if (interactor != null && interactor.TryGetUIModel(out var uiModel) && uiModel.interactionType == UIInteractionType.Poke)
            {
                // First we see if the interactor is selecting on anything else, then check if they are part of the same root canvas, otherwise skip processing.
                // If they share the same root canvas, we do a sorting check to see if we cancel out of the poke or bubble the highest sorted raycaster to the top of the list.
                // Notes for future readers: In the case of a dropdown, a Blocker will be created with order 29999 and Dropdown with 30000, prioritizing dropdown selection.
                if (s_InteractorRaycasters.TryGetValue(interactor, out var graphicRaycaster) && graphicRaycaster != null && graphicRaycaster != this &&
                    (graphicRaycaster.canvas.rootCanvas != canvas.rootCanvas || (graphicRaycaster.canvas.rootCanvas == canvas.rootCanvas && graphicRaycaster.canvas.sortingOrder >= canvas.sortingOrder)))
                {
                    return;
                }

                // Check if poke is blocked for this frame. Unlike rays, updates for poke ui interaction are isolated from the poke interactor.
                if (PerformSpherecast(uiModel.position, uiModel.pokeDepth, layerMask, currentEventCamera, resultAppendList) && resultAppendList.Count > 0)
                {
                    eventData.rayHitIndex = 1;
                    var firstHit = resultAppendList[0];
                    var hitTransform = firstHit.gameObject.transform;

                    m_PokeLogic.SetPokeDepth(uiModel.pokeDepth);
                    
                    // Check if not already hovering interactor
                    if (!s_PokeHoverRaycasters[this].Contains(interactor))
                    {
                        s_PokeHoverRaycasters[this].Add(interactor);
                        m_PokeLogic.OnHoverEntered(interactor, new Pose(uiModel.position, uiModel.orientation), hitTransform);
                    }
                    
                    if (m_PokeLogic.MeetsRequirementsForSelectAction(interactor, hitTransform.position, uiModel.position, 0f, hitTransform))
                    {
                        s_InteractorRaycasters[interactor] = this;
                    }
                    else
                    {
                        s_InteractorRaycasters.Remove(interactor);
                    }
                }
                else
                {
                    EndPokeInteraction(interactor);
                }
            }
            else
            {
                var rayPoints = eventData.rayPoints;
                for (var i = 1; i < rayPoints.Count; i++)
                {
                    var from = rayPoints[i - 1];
                    var to = rayPoints[i];
                    if (PerformRaycast(from, to, layerMask, currentEventCamera, resultAppendList))
                    {
                        eventData.rayHitIndex = i;
                        break;
                    }
                }
            }
        }

        bool PerformSpherecast(Vector3 origin, float radius, LayerMask layerMask, Camera currentEventCamera, List<RaycastResult> resultAppendList)
        {
            m_RaycastResultsCache.Clear();
            SortedSpherecastGraphics(canvas, origin, radius, layerMask, currentEventCamera, m_RaycastResultsCache);

            if (m_RaycastResultsCache.Count <= 0)
                return false;

            var firstResult = m_RaycastResultsCache[0];
            var ray = new Ray(origin, firstResult.worldHitPosition - origin);

            // Results from spherecast aim every which direction! We only want to test the nearest first direction.
            m_RaycastResultsCache.Clear();
            m_RaycastResultsCache.Add(firstResult);

            return ProcessSortedHitsResults(ray, float.PositiveInfinity, false, m_RaycastResultsCache, resultAppendList);
        }

        bool PerformRaycast(Vector3 from, Vector3 to, LayerMask layerMask, Camera currentEventCamera, List<RaycastResult> resultAppendList)
        {
            var hitSomething = false;

            var rayDistance = Vector3.Distance(to, from);
            var ray = new Ray(from, to - from);

            var hitDistance = rayDistance;
            if (m_CheckFor3DOcclusion)
            {
                var hitCount = m_LocalPhysicsScene.Raycast(ray.origin, ray.direction, m_OcclusionHits3D, hitDistance, m_BlockingMask, m_RaycastTriggerInteraction);

                if (hitCount > 0)
                {
                    var hit = FindClosestHit(m_OcclusionHits3D, hitCount);
                    hitDistance = hit.distance;
                    hitSomething = true;
                }
            }

            if (m_CheckFor2DOcclusion)
            {
#if PHYSICS2D_MODULE_PRESENT
                if (m_LocalPhysicsScene2D.GetRayIntersection(ray, hitDistance, m_OcclusionHits2D, m_BlockingMask) > 0)
                {
                    // Unlike 3D physics, all 2D physics spatial queries are sorted by distance or in this case,
                    // sorted by Z depth along the ray so there's no need to find the closest hit, it'll always be the first result.
                    hitDistance = m_OcclusionHits2D[0].distance;
                    hitSomething = true;
                }
#endif
            }

            m_RaycastResultsCache.Clear();
            SortedRaycastGraphics(canvas, ray, hitDistance, layerMask, currentEventCamera, m_RaycastResultsCache);

            return ProcessSortedHitsResults(ray, hitDistance, hitSomething, m_RaycastResultsCache, resultAppendList);
        }

        bool ProcessSortedHitsResults(Ray ray, float hitDistance, bool hitSomething, List<RaycastHitData> raycastHitDatums, List<RaycastResult> resultAppendList)
        {
            // Now that we have a list of sorted hits, process any extra settings and filters.
            foreach (var hitData in raycastHitDatums)
            {
                var validHit = true;

                var go = hitData.graphic.gameObject;
                if (m_IgnoreReversedGraphics)
                {
                    var forward = ray.direction;
                    var goDirection = go.transform.rotation * Vector3.forward;
                    validHit = Vector3.Dot(forward, goDirection) > 0;
                }

                validHit &= hitData.distance < hitDistance;

                if (validHit)
                {
                    var trans = go.transform;
                    var transForward = trans.forward;
                    var castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        distance = hitData.distance,
                        index = resultAppendList.Count,
                        depth = hitData.graphic.depth,
                        sortingLayer = canvas.sortingLayerID,
                        sortingOrder = canvas.sortingOrder,
                        worldPosition = hitData.worldHitPosition,
                        worldNormal = -transForward,
                        screenPosition = hitData.screenPosition,
                        displayIndex = hitData.displayIndex,
                    };
                    resultAppendList.Add(castResult);

                    hitSomething = true;
                }
            }

            return hitSomething;
        }

        static void SortedSpherecastGraphics(Canvas canvas, Vector3 origin, float radius, LayerMask layerMask, Camera eventCamera, List<RaycastHitData> results)
        {
            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            s_SortedGraphics.Clear();
            for (int i = 0; i < graphics.Count; ++i)
            {
                var graphic = graphics[i];

                if (!ShouldTestGraphic(graphic, layerMask))
                    continue;

#if UNITY_2020_1_OR_NEWER
                var raycastPadding = graphic.raycastPadding;
#else
                var raycastPadding = Vector4.zero;
#endif

                if (SphereIntersectsRectTransform(graphic.rectTransform, raycastPadding, origin, out var worldPos, out var distance))
                {
                    if (distance <= radius)
                    {
                        Vector2 screenPos = eventCamera.WorldToScreenPoint(worldPos);
                        // mask/image intersection - See Unity docs on eventAlphaThreshold for when this does anything
                        if (graphic.Raycast(screenPos, eventCamera))
                        {
                            s_SortedGraphics.Add(new RaycastHitData(graphic, worldPos, screenPos, distance, eventCamera.targetDisplay));
                        }
                    }
                }
            }

            SortingHelpers.Sort(s_SortedGraphics, s_RaycastHitComparer);
            results.AddRange(s_SortedGraphics);
        }

        static void SortedRaycastGraphics(Canvas canvas, Ray ray, float maxDistance, LayerMask layerMask, Camera eventCamera, List<RaycastHitData> results)
        {
            var graphics = GraphicRegistry.GetGraphicsForCanvas(canvas);

            s_SortedGraphics.Clear();
            for (int i = 0; i < graphics.Count; ++i)
            {
                var graphic = graphics[i];

                if (!ShouldTestGraphic(graphic, layerMask))
                    continue;

#if UNITY_2020_1_OR_NEWER
                var raycastPadding = graphic.raycastPadding;
#else
                var raycastPadding = Vector4.zero;
#endif

                if (RayIntersectsRectTransform(graphic.rectTransform, raycastPadding, ray, out var worldPos, out var distance))
                {
                    if (distance <= maxDistance)
                    {
                        Vector2 screenPos = eventCamera.WorldToScreenPoint(worldPos);
                        // mask/image intersection - See Unity docs on eventAlphaThreshold for when this does anything
                        if (graphic.Raycast(screenPos, eventCamera))
                        {
                            s_SortedGraphics.Add(new RaycastHitData(graphic, worldPos, screenPos, distance, eventCamera.targetDisplay));
                        }
                    }
                }
            }

            SortingHelpers.Sort(s_SortedGraphics, s_RaycastHitComparer);
            results.AddRange(s_SortedGraphics);
        }

        static bool ShouldTestGraphic(Graphic graphic, LayerMask layerMask)
        {
            // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
            if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                return false;

            if (((1 << graphic.gameObject.layer) & layerMask) == 0)
                return  false;

            return true;
        }

        static bool SphereIntersectsRectTransform(RectTransform transform, Vector4 raycastPadding, Vector3 from, out Vector3 worldPosition, out float distance)
        {
            var plane = GetRectTransformPlane(transform, raycastPadding, s_Corners);
            var closestPoint = plane.ClosestPointOnPlane(from);
            var ray = new Ray(from, closestPoint - from);
            return RayIntersectsRectTransform(ray, plane, out worldPosition, out distance);
        }

        static bool RayIntersectsRectTransform(RectTransform transform, Vector4 raycastPadding, Ray ray, out Vector3 worldPosition, out float distance)
        {
            var plane = GetRectTransformPlane(transform, raycastPadding, s_Corners);
            return RayIntersectsRectTransform(ray, plane, out worldPosition, out distance);
        }

        static bool RayIntersectsRectTransform(Ray ray, Plane plane, out Vector3 worldPosition, out float distance)
        {
            if (plane.Raycast(ray, out var enter))
            {
                var intersection = ray.GetPoint(enter);

                var bottomEdge = s_Corners[3] - s_Corners[0];
                var leftEdge = s_Corners[1] - s_Corners[0];
                var bottomDot = Vector3.Dot(intersection - s_Corners[0], bottomEdge);
                var leftDot = Vector3.Dot(intersection - s_Corners[0], leftEdge);

                // If the intersection is right of the left edge and above the bottom edge.
                if (leftDot >= 0f && bottomDot >= 0f)
                {
                    var topEdge = s_Corners[1] - s_Corners[2];
                    var rightEdge = s_Corners[3] - s_Corners[2];
                    var topDot = Vector3.Dot(intersection - s_Corners[2], topEdge);
                    var rightDot = Vector3.Dot(intersection - s_Corners[2], rightEdge);

                    // If the intersection is left of the right edge, and below the top edge
                    if (topDot >= 0f && rightDot >= 0f)
                    {
                        worldPosition = intersection;
                        distance = enter;
                        return true;
                    }
                }
            }

            worldPosition = Vector3.zero;
            distance = 0f;
            return false;
        }

        static Plane GetRectTransformPlane(RectTransform transform, Vector4 raycastPadding, Vector3[] fourCornersArray)
        {
            GetRectTransformWorldCorners(transform, raycastPadding, fourCornersArray);
            return new Plane(fourCornersArray[0], fourCornersArray[1], fourCornersArray[2]);
        }

        // This method is similar to RecTransform.GetWorldCorners, but with support for the raycastPadding offset.
        static void GetRectTransformWorldCorners(RectTransform transform, Vector4 offset, Vector3[] fourCornersArray)
        {
            if (fourCornersArray == null || fourCornersArray.Length < 4)
            {
                Debug.LogError("Calling GetRectTransformWorldCorners with an array that is null or has less than 4 elements.");
                return;
            }

            // GraphicRaycaster.Raycast uses RectTransformUtility.RectangleContainsScreenPoint instead,
            // which redirects to PointInRectangle defined in RectTransformUtil.cpp. However, that method
            // uses the Camera to convert from the given screen point to a ray, but this class uses
            // the ray from the Ray Interactor that feeds the event data.
            // Offset calculation for raycastPadding from PointInRectangle method, which replaces RectTransform.GetLocalCorners.
            var rect = transform.rect;
            var x0 = rect.x + offset.x;
            var y0 = rect.y + offset.y;
            var x1 = rect.xMax - offset.z;
            var y1 = rect.yMax - offset.w;
            fourCornersArray[0] = new Vector3(x0, y0, 0f);
            fourCornersArray[1] = new Vector3(x0, y1, 0f);
            fourCornersArray[2] = new Vector3(x1, y1, 0f);
            fourCornersArray[3] = new Vector3(x1, y0, 0f);

            // Transform the local corners to world space, which is from RectTransform.GetWorldCorners.
            var localToWorldMatrix = transform.localToWorldMatrix;
            for (var index = 0; index < 4; ++index)
                fourCornersArray[index] = localToWorldMatrix.MultiplyPoint(fourCornersArray[index]);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        protected void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!enabled)
                return;

            if (m_PokeLogic != null)
                m_PokeLogic?.DrawGizmos();
#endif
        }
    }
}
