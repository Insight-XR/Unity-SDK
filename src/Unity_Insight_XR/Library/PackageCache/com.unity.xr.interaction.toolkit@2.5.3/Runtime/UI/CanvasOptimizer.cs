using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    /// <summary>
    /// Keeps track of canvases in a scene and optimizes them by removing unnecessary components in nested canvases and canvases out of view.
    /// </summary>
    [AddComponentMenu("Event/Canvas Optimizer", 11)]
    [HelpURL(XRHelpURLConstants.k_CanvasOptimizer)]
    public class CanvasOptimizer : MonoBehaviour
    {
        class CanvasState
        {
            const float k_CanvasCheckInterval = 0.5f;

            class CanvasSettings
            {
                public bool present { get; set; }

                AdditionalCanvasShaderChannels m_AdditionalShaderChannels;
                float m_NormalizedSortingGridSize;
                bool m_OverridePixelPerfect;
                bool m_OverrideSorting;
                float m_PlaneDistance;
                float m_ReferencePixelsPerUnit;
                RenderMode m_RenderMode;
                float m_ScaleFactor;
                int m_SortingLayerID;
                string m_SortingLayerName;
                int m_SortingOrder;
                int m_TargetDisplay;

                public void CopyFrom(Canvas source)
                {
                    m_AdditionalShaderChannels = source.additionalShaderChannels;
                    m_NormalizedSortingGridSize = source.normalizedSortingGridSize;
                    m_OverridePixelPerfect = source.overridePixelPerfect;
                    m_OverrideSorting = source.overrideSorting;
                    m_PlaneDistance = source.planeDistance;
                    m_ReferencePixelsPerUnit = source.referencePixelsPerUnit;
                    m_RenderMode = source.renderMode;
                    m_ScaleFactor = source.scaleFactor;
                    m_SortingLayerID = source.sortingLayerID;
                    m_SortingLayerName = source.sortingLayerName;
                    m_SortingOrder = source.sortingOrder;
                    m_TargetDisplay = source.targetDisplay;
                }

                public void CopyTo(Canvas dest)
                {
                    dest.additionalShaderChannels = m_AdditionalShaderChannels;
                    dest.normalizedSortingGridSize = m_NormalizedSortingGridSize;
                    dest.overridePixelPerfect = m_OverridePixelPerfect;
                    dest.overrideSorting = m_OverrideSorting;
                    dest.planeDistance = m_PlaneDistance;
                    dest.referencePixelsPerUnit = m_ReferencePixelsPerUnit;
                    dest.renderMode = m_RenderMode;
                    dest.scaleFactor = m_ScaleFactor;
                    dest.sortingLayerID = m_SortingLayerID;
                    dest.sortingLayerName = m_SortingLayerName;
                    dest.sortingOrder = m_SortingOrder;
                    dest.targetDisplay = m_TargetDisplay;
                }
            }

            class CanvasScalerSettings
            {
                public bool present { get; set; }

                float m_DefaultSpriteDPI;
                float m_DynamicPixelsPerUnit;
                float m_FallbackScreenDPI;
                float m_MatchWidthOrHeight;
                CanvasScaler.Unit m_PhysicalUnit;
                float m_ReferencePixelsPerUnit;
                Vector2 m_ReferenceResolution;
                float m_ScaleFactor;
                CanvasScaler.ScreenMatchMode m_ScreenMatchMode;
                CanvasScaler.ScaleMode m_UiScaleMode;

                public void CopyFrom(CanvasScaler source)
                {
                    m_DefaultSpriteDPI = source.defaultSpriteDPI;
                    m_DynamicPixelsPerUnit = source.dynamicPixelsPerUnit;
                    m_FallbackScreenDPI = source.fallbackScreenDPI;
                    m_MatchWidthOrHeight = source.matchWidthOrHeight;
                    m_PhysicalUnit = source.physicalUnit;
                    m_ReferencePixelsPerUnit = source.referencePixelsPerUnit;
                    m_ReferenceResolution = source.referenceResolution;
                    m_ScaleFactor = source.scaleFactor;
                    m_ScreenMatchMode = source.screenMatchMode;
                    m_UiScaleMode = source.uiScaleMode;
                }

                public void CopyTo(CanvasScaler dest)
                {
                    dest.defaultSpriteDPI = m_DefaultSpriteDPI;
                    dest.dynamicPixelsPerUnit = m_DynamicPixelsPerUnit;
                    dest.fallbackScreenDPI = m_FallbackScreenDPI;
                    dest.matchWidthOrHeight = m_MatchWidthOrHeight;
                    dest.physicalUnit = m_PhysicalUnit;
                    dest.referencePixelsPerUnit = m_ReferencePixelsPerUnit;
                    dest.referenceResolution = m_ReferenceResolution;
                    dest.scaleFactor = m_ScaleFactor;
                    dest.screenMatchMode = m_ScreenMatchMode;
                    dest.uiScaleMode = m_UiScaleMode;
                }
            }

            class GraphicRaycasterSettings
            {
                public bool present { get; set; }

                LayerMask m_BlockingMask;
                GraphicRaycaster.BlockingObjects m_BlockingObjects;
                bool m_IgnoreReversedGraphics;

                public void CopyFrom(GraphicRaycaster source)
                {
                    m_BlockingMask = source.blockingMask;
                    m_BlockingObjects = source.blockingObjects;
                    m_IgnoreReversedGraphics = source.ignoreReversedGraphics;
                }

                public void CopyTo(GraphicRaycaster dest)
                {
                    dest.blockingMask = m_BlockingMask;
                    dest.blockingObjects = m_BlockingObjects;
                    dest.ignoreReversedGraphics = m_IgnoreReversedGraphics;
                }
            }

            CanvasTracker m_Tracker;

            readonly CanvasSettings m_CanvasSettings = new CanvasSettings();
            readonly CanvasScalerSettings m_CanvasScalerSettings = new CanvasScalerSettings();
            readonly GraphicRaycasterSettings m_GraphicRaycasterSettings = new GraphicRaycasterSettings();
            bool m_WasNested;
            bool m_Nested;
            bool m_RaysDisabled;
            Canvas m_Canvas;
            GraphicRaycaster m_Raycaster;
            TrackedDeviceGraphicRaycaster m_TrackedDeviceGraphicRaycaster;

            float m_CheckTimer;

            internal void Initialize(CanvasTracker tracker)
            {
                m_Tracker = tracker;
                var go = m_Tracker.gameObject;
                go.TryGetComponent(out m_Canvas);
                go.TryGetComponent(out m_Raycaster);
                CheckForNestedChanges(true);
            }

            internal void CheckForNestedChanges(bool force = false)
            {
                if (!m_Tracker.transformDirty && !force)
                    return;

                m_Tracker.transformDirty = false;

                var transform = m_Tracker.transform;

                // Check for nesting
                var parent = transform.parent;
                var parentCanvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
                m_Nested = (parentCanvas != null);

                // If nested has occurred, remove unnecessary components
                if (m_Nested && (!m_WasNested || force))
                {
                    if (transform.TryGetComponent<CanvasScaler>(out var canvasScaler))
                    {
                        m_CanvasScalerSettings.present = true;
                        m_CanvasScalerSettings.CopyFrom(canvasScaler);
                        Destroy(canvasScaler);
                    }
                    else
                        m_CanvasScalerSettings.present = false;

                    if (transform.TryGetComponent<GraphicRaycaster>(out var graphicRaycaster))
                    {
                        m_GraphicRaycasterSettings.present = true;
                        m_GraphicRaycasterSettings.CopyFrom(graphicRaycaster);
                        Destroy(graphicRaycaster);
                    }
                    else
                        m_GraphicRaycasterSettings.present = false;

                    if (transform.TryGetComponent<Canvas>(out var canvas))
                    {
                        m_CanvasSettings.present = true;
                        m_CanvasSettings.CopyFrom(canvas);
                        Destroy(canvas);
                    }
                    else
                        m_CanvasSettings.present = false;

                    if (transform.TryGetComponent(out m_TrackedDeviceGraphicRaycaster))
                    {
                        // ReSharper disable once PossibleNullReferenceException -- already verified above with m_Nested
                        if (!parentCanvas.TryGetComponent<TrackedDeviceGraphicRaycaster>(out _))
                            Debug.LogWarning($"Tracked device raycaster not present on parent canvas: {parent.name}. Tracked device input will likely not work on: {transform.name}", transform);

                        m_TrackedDeviceGraphicRaycaster.enabled = false;
                    }
                }

                // If nesting has not occurred, restore the components
                if (!m_Nested && (m_WasNested || force))
                {
                    if (m_CanvasSettings.present)
                    {
                        var go = transform.gameObject;
                        m_Canvas = go.AddComponent<Canvas>();

                        m_CanvasSettings.CopyTo(m_Canvas);

                        if (m_CanvasScalerSettings.present)
                        {
                            var canvasScaler = go.AddComponent<CanvasScaler>();
                            m_CanvasScalerSettings.CopyTo(canvasScaler);
                        }

                        if (m_GraphicRaycasterSettings.present)
                        {
                            m_Raycaster = go.AddComponent<GraphicRaycaster>();
                            m_GraphicRaycasterSettings.CopyTo(m_Raycaster);
                        }

                        if (m_TrackedDeviceGraphicRaycaster != null)
                            m_TrackedDeviceGraphicRaycaster.enabled = true;
                    }
                }

                m_WasNested = m_Nested;
            }

            internal void CheckForOutOfView(Transform gazeSource, float fovAngle, float facingAngle, float maxDistance)
            {
                if (m_Nested)
                    return;

                if (m_Canvas.renderMode != RenderMode.WorldSpace)
                    return;

                m_CheckTimer += Time.deltaTime;

                if (m_CheckTimer < k_CanvasCheckInterval)
                    return;

                m_CheckTimer = 0f;

                var transform = m_Canvas.transform;
                var gazePos = gazeSource.position;
                var gazeDir = gazeSource.forward;
                var targetPos = transform.position;
                var targetDir = transform.forward;

                // Check if canvas is facing away from camera
                // Check if canvas is off camera
                // If any of these are true, disable the ray casters
                var disableRayCasters = BurstGazeUtility.IsOutsideGaze(gazePos, gazeDir, targetPos, fovAngle) ||
                                        !BurstGazeUtility.IsAlignedToGazeForward(gazeDir, targetDir, facingAngle) &&
                                        BurstGazeUtility.IsOutsideDistanceRange(gazePos, targetPos, maxDistance);

                // See if state changed
                if (m_RaysDisabled != disableRayCasters)
                {
                    m_RaysDisabled = disableRayCasters;

                    // Disable tracked device caster
                    if (m_Raycaster != null)
                        m_Raycaster.enabled = !m_RaysDisabled;

                    if (m_TrackedDeviceGraphicRaycaster != null)
                        m_TrackedDeviceGraphicRaycaster.enabled = !m_RaysDisabled;
                }
            }
        }

        [SerializeField]
        [Tooltip("How wide of an field-of-view to use when determining if a canvas is in view.")]
        float m_RayPositionIgnoreAngle = 45f;

        /// <summary>
        /// How wide of an field-of-view to use when determining if a canvas is in view.
        /// </summary>
        public float rayPositionIgnoreAngle
        {
            get => m_RayPositionIgnoreAngle;
            set => m_RayPositionIgnoreAngle = value;
        }

        [SerializeField]
        [Tooltip("How much the camera and canvas rotate away from one another and still be considered facing.")]
        float m_RayFacingIgnoreAngle = 75f;

        /// <summary>
        /// How much the camera and canvas rotate away from one another and still be considered facing.
        /// </summary>
        public float rayFacingIgnoreAngle
        {
            get => m_RayFacingIgnoreAngle;
            set => m_RayFacingIgnoreAngle = value;
        }

        [SerializeField]
        [Tooltip("How far away a canvas can be from this camera and still receive input.")]
        float m_RayPositionIgnoreDistance = 25f;

        /// <summary>
        /// How far away a canvas can be from this camera and still receive input.
        /// </summary>
        public float rayPositionIgnoreDistance
        {
            get => m_RayPositionIgnoreDistance;
            set => m_RayPositionIgnoreDistance = value;
        }

        Camera m_CullingCamera;
        Transform m_CullingCameraTransform;
        readonly Dictionary<CanvasTracker, CanvasState> m_CanvasTrackers = new Dictionary<CanvasTracker, CanvasState>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            if (ComponentLocatorUtility<CanvasOptimizer>.FindComponent() != this)
            {
                Debug.LogWarning($"Duplicate Canvas Optimizer {gameObject.name} found. Only one Canvas Optimizer is allowed in the scene at a time.", this);
                Destroy(this);
                enabled = false;
                return;
            }

            FindCullingCamera();

            // Canvases cannot auto-register, so collect all canvases in the scene at start
#if UNITY_2023_1_OR_NEWER
            var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var canvases = FindObjectsOfType<Canvas>(true);
#endif
            for (var index = 0; index < canvases.Length; ++index)
            {
                var canvas = canvases[index];
                RegisterCanvas(canvas);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            CheckForNestedCanvasChanges();
            CheckForOutOfViewCanvases();
        }

        /// <summary>
        /// Allows the canvas optimizer to process this canvas. Will be called automatically for all canvases in the scene.
        /// </summary>
        /// <param name="canvas">The canvas to optimize.</param>
        /// <remarks>This only needs to be called manually for canvases instantiated at runtime.</remarks>
        public void RegisterCanvas(Canvas canvas)
        {
            var canvasTracker = InitializeCanvasTracking(canvas);
            if (m_CanvasTrackers.ContainsKey(canvasTracker))
                return;

            var canvasState = new CanvasState();
            canvasState.Initialize(canvasTracker);
            m_CanvasTrackers.Add(canvasTracker, canvasState);
        }

        /// <summary>
        /// Tells the canvas optimizer to stop processing this canvas. Will be called automatically for all canvases in the scene.
        /// </summary>
        /// <param name="canvas">The canvas to stop optimizing.</param>
        /// <remarks>This only needs to be called manually for canvases destroyed during runtime.</remarks>
        public void UnregisterCanvas(Canvas canvas)
        {
            // Remove matching canvas tracker
            if (canvas.TryGetComponent(out CanvasTracker toRemove))
            {
                m_CanvasTrackers.Remove(toRemove);
            }
        }

        static CanvasTracker InitializeCanvasTracking(Canvas target)
        {
            // Put parent tracker on target
            if (!target.gameObject.TryGetComponent(out CanvasTracker tracker))
            {
                tracker = target.gameObject.AddComponent<CanvasTracker>();
                tracker.hideFlags = HideFlags.HideAndDontSave;
            }

            return tracker;
        }

        void CheckForNestedCanvasChanges()
        {
            foreach (var canvasData in m_CanvasTrackers.Values)
            {
                canvasData.CheckForNestedChanges();
            }
        }

        void CheckForOutOfViewCanvases()
        {
            // Find the new main camera if necessary
            if (m_CullingCamera == null || !m_CullingCamera.enabled)
            {
                FindCullingCamera();

                if (m_CullingCameraTransform == null)
                    return;
            }

            foreach (var canvasData in m_CanvasTrackers.Values)
            {
                canvasData.CheckForOutOfView(m_CullingCameraTransform, m_RayPositionIgnoreAngle, m_RayFacingIgnoreAngle, m_RayPositionIgnoreDistance);
            }
        }

        void FindCullingCamera()
        {
            m_CullingCamera = Camera.main;
            m_CullingCameraTransform = m_CullingCamera != null ? m_CullingCamera.transform : null;
        }
    }
}
