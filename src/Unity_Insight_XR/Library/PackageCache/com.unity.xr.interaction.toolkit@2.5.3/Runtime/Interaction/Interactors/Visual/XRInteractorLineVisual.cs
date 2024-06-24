using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Curves;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Get line points and hit point info for rendering.
    /// </summary>
    /// <seealso cref="XRInteractorLineVisual"/>
    /// <seealso cref="XRRayInteractor"/>
    public interface ILineRenderable
    {
        /// <summary>
        /// Gets the polygonal chain represented by a list of endpoints which form line segments to approximate the curve.
        /// Positions are in world space coordinates.
        /// </summary>
        /// <param name="linePoints">When this method returns, contains the sample points if successful.</param>
        /// <param name="numPoints">When this method returns, contains the number of sample points if successful.</param>
        /// <returns>Returns <see langword="true"/> if the sample points form a valid line, such as by having at least two points.
        /// Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// Getting line points with <see cref="Vector3"/> array is much less performant than using a native array.
        /// Use <see cref="IAdvancedLineRenderable.GetLinePoints(ref NativeArray{Vector3},out int,Ray?)"/> instead if available.
        /// </remarks>
        bool GetLinePoints(ref Vector3[] linePoints, out int numPoints);

        /// <summary>
        /// Gets the current ray cast hit information, if a hit occurs. It returns the world position and the normal vector
        /// of the hit point, and its position in linePoints.
        /// </summary>
        /// <param name="position">When this method returns, contains the world position of the ray impact point if a hit occurred.</param>
        /// <param name="normal">When this method returns, contains the world normal of the surface the ray hit if a hit occurred.</param>
        /// <param name="positionInLine">When this method returns, contains the index of the sample endpoint within the list of points returned by <see cref="GetLinePoints"/>
        /// where a hit occurred. Otherwise, a value of <c>0</c> if no hit occurred.</param>
        /// <param name="isValidTarget">When this method returns, contains whether both a hit occurred and it is a valid target for interaction.</param>
        /// <returns>Returns <see langword="true"/> if a hit occurs, implying the ray cast hit information is valid. Otherwise, returns <see langword="false"/>.</returns>
        bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget);
    }

    /// <summary>
    /// An advanced interface for providing line data for rendering with additional functionality.
    /// </summary>
    /// <seealso cref="XRInteractorLineVisual"/>
    /// <seealso cref="XRRayInteractor"/>
    public interface IAdvancedLineRenderable : ILineRenderable
    {
        /// <summary>
        /// Gets the polygonal chain represented by a list of endpoints which form line segments to approximate the curve.
        /// Positions are in world space coordinates.
        /// </summary>
        /// <param name="linePoints">When this method returns, contains the sample points if successful.</param>
        /// <param name="numPoints">When this method returns, contains the number of sample points if successful.</param>
        /// <param name="rayOriginOverride">Optional ray origin override used when re-computing the line.</param>
        /// <returns>Returns <see langword="true"/> if the sample points form a valid line, such as by having at least two points.
        /// Otherwise, returns <see langword="false"/>.</returns>
        bool GetLinePoints(ref NativeArray<Vector3> linePoints, out int numPoints, Ray? rayOriginOverride = null);

        /// <summary>
        /// Gets the line origin and direction.
        /// Origin and Direction are in world space coordinates.
        /// </summary>
        /// <param name="origin">Point in space where the line originates from.</param>
        /// <param name="direction">Direction vector used to draw line.</param>
        void GetLineOriginAndDirection(out Vector3 origin, out Vector3 direction);
    }

    /// <summary>
    /// Interactor helper object aligns a <see cref="LineRenderer"/> with the Interactor.
    /// </summary>
    [AddComponentMenu("XR/Visual/XR Interactor Line Visual", 11)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer))]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_LineVisual)]
    [HelpURL(XRHelpURLConstants.k_XRInteractorLineVisual)]
#if BURST_PRESENT
    [BurstCompile]
#endif
    public class XRInteractorLineVisual : MonoBehaviour, IXRCustomReticleProvider
    {
        const float k_MinLineWidth = 0.0001f;
        const float k_MaxLineWidth = 0.05f;
        const float k_MinLineBendRatio = 0.01f;
        const float k_MaxLineBendRatio = 1f;

        [SerializeField, Range(k_MinLineWidth, k_MaxLineWidth)]
        float m_LineWidth = 0.005f;
        /// <summary>
        /// Controls the width of the line.
        /// </summary>
        public float lineWidth
        {
            get => m_LineWidth;
            set
            {
                m_LineWidth = value;
                m_PerformSetup = true;

                // Force update user scale since it calls an update to the line width
                m_UserScaleVar.BroadcastValue();
            }
        }

        [SerializeField]
        bool m_OverrideInteractorLineLength = true;
        /// <summary>
        /// A boolean value that controls which source Unity uses to determine the length of the line.
        /// Set to <see langword="true"/> to use the Line Length set by this behavior.
        /// Set to <see langword="false"/> to have the length of the line determined by the Interactor.
        /// </summary>
        /// <seealso cref="lineLength"/>
        public bool overrideInteractorLineLength
        {
            get => m_OverrideInteractorLineLength;
            set => m_OverrideInteractorLineLength = value;
        }

        [SerializeField]
        float m_LineLength = 10f;
        /// <summary>
        /// Controls the length of the line when overriding.
        /// </summary>
        /// <seealso cref="overrideInteractorLineLength"/>
        /// <seealso cref="minLineLength"/>
        public float lineLength
        {
            get => m_LineLength;
            set => m_LineLength = value;
        }

        [SerializeField]
        bool m_AutoAdjustLineLength;

        /// <summary>
        /// Determines whether the length of the line will retract over time when no valid hits or selection occur.
        /// </summary>
        /// <seealso cref="minLineLength"/>
        /// <seealso cref="lineRetractionDelay"/>
        public bool autoAdjustLineLength
        {
            get => m_AutoAdjustLineLength;
            set => m_AutoAdjustLineLength = value;
        }

        [SerializeField]
        float m_MinLineLength = 0.5f;

        /// <summary>
        /// Controls the minimum length of the line when overriding.
        /// When no valid hits occur, the ray visual shrinks down to this size.
        /// </summary>
        /// <seealso cref="overrideInteractorLineLength"/>
        /// <seealso cref="autoAdjustLineLength"/>
        /// <seealso cref="lineLength"/>
        public float minLineLength
        {
            get => m_MinLineLength;
            set => m_MinLineLength = value;
        }

        [SerializeField]
        bool m_UseDistanceToHitAsMaxLineLength = true;

        /// <summary>
        /// Determines whether the max line length will be the the distance to the hit point or the fixed line length.
        /// </summary>
        /// <seealso cref="lineLength"/>
        public bool useDistanceToHitAsMaxLineLength
        {
            get => m_UseDistanceToHitAsMaxLineLength;
            set => m_UseDistanceToHitAsMaxLineLength = value;
        }

        [SerializeField]
        float m_LineRetractionDelay = 0.5f;

        /// <summary>
        /// Time in seconds elapsed after last valid hit or selection for line to begin retracting to the minimum override length.
        /// </summary>
        /// <seealso cref="lineRetractionDelay"/>
        /// <seealso cref="minLineLength"/>
        public float lineRetractionDelay
        {
            get => m_LineRetractionDelay;
            set => m_LineRetractionDelay = value;
        }

        [SerializeField]
        float m_LineLengthChangeSpeed = 12f;

        /// <summary>
        /// Scalar used to control the speed of changes in length of the line when overriding it's length.
        /// </summary>
        /// <seealso cref="minLineLength"/>
        /// <seealso cref="lineRetractionDelay"/>
        public float lineLengthChangeSpeed
        {
            get => m_LineLengthChangeSpeed;
            set => m_LineLengthChangeSpeed = value;
        }

        [SerializeField]
        AnimationCurve m_WidthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        /// <summary>
        /// Controls the relative width of the line from start to end.
        /// </summary>
        public AnimationCurve widthCurve
        {
            get => m_WidthCurve;
            set
            {
                m_WidthCurve = value;
                m_PerformSetup = true;
            }
        }

        [SerializeField]
        bool m_SetLineColorGradient = true;
        /// <summary>
        /// Determines whether or not this component will control the color of the Line Renderer.
        /// Disable to manually control the color externally from this component.
        /// </summary>
        /// <remarks>
        /// Useful to disable when using the affordance system for line color control instead of through this behavior.
        /// </remarks>
        public bool setLineColorGradient
        {
            get => m_SetLineColorGradient;
            set => m_SetLineColorGradient = value;
        }

        [SerializeField]
        Gradient m_ValidColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };
        /// <summary>
        /// Controls the color of the line as a gradient from start to end to indicate a valid state.
        /// </summary>
        public Gradient validColorGradient
        {
            get => m_ValidColorGradient;
            set => m_ValidColorGradient = value;
        }

        [SerializeField]
        Gradient m_InvalidColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.red, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };
        /// <summary>
        /// Controls the color of the line as a gradient from start to end to indicate an invalid state.
        /// </summary>
        public Gradient invalidColorGradient
        {
            get => m_InvalidColorGradient;
            set => m_InvalidColorGradient = value;
        }

        [SerializeField]
        Gradient m_BlockedColorGradient = new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.yellow, 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) },
        };
        /// <summary>
        /// Controls the color of the line as a gradient from start to end to indicate a state where the interactor has
        /// a valid target but selection is blocked.
        /// </summary>
        public Gradient blockedColorGradient
        {
            get => m_BlockedColorGradient;
            set => m_BlockedColorGradient = value;
        }

        [SerializeField]
        bool m_TreatSelectionAsValidState;
        /// <summary>
        /// Forces the use of valid state visuals while the interactor is selecting an interactable, whether or not the Interactor has any valid targets.
        /// </summary>
        /// <seealso cref="validColorGradient"/>
        public bool treatSelectionAsValidState
        {
            get => m_TreatSelectionAsValidState;
            set => m_TreatSelectionAsValidState = value;
        }

        [SerializeField]
        bool m_SmoothMovement;
        /// <summary>
        /// Controls whether the rendered segments will be delayed from and smoothly follow the target segments.
        /// </summary>
        /// <seealso cref="followTightness"/>
        /// <seealso cref="snapThresholdDistance"/>
        public bool smoothMovement
        {
            get => m_SmoothMovement;
            set => m_SmoothMovement = value;
        }

        [SerializeField]
        float m_FollowTightness = 10f;
        /// <summary>
        /// Controls the speed that the rendered segments follow the target segments when Smooth Movement is enabled.
        /// </summary>
        /// <seealso cref="smoothMovement"/>
        /// <seealso cref="snapThresholdDistance"/>
        public float followTightness
        {
            get => m_FollowTightness;
            set => m_FollowTightness = value;
        }

        [SerializeField]
        float m_SnapThresholdDistance = 10f;

        /// <summary>
        /// Controls the threshold distance between line points at two consecutive frames to snap rendered segments to target segments when Smooth Movement is enabled.
        /// </summary>
        /// <seealso cref="smoothMovement"/>
        /// <seealso cref="followTightness"/>
        public float snapThresholdDistance
        {
            get => m_SnapThresholdDistance;
            set
            {
                m_SnapThresholdDistance = value;
                m_SquareSnapThresholdDistance = m_SnapThresholdDistance * m_SnapThresholdDistance;
            }
        }

        [SerializeField]
        GameObject m_Reticle;
        /// <summary>
        /// Stores the reticle that appears at the end of the line when it is valid.
        /// </summary>
        /// <remarks>
        /// Unity will instantiate it while playing when it is a Prefab asset.
        /// </remarks>
        public GameObject reticle
        {
            get => m_Reticle;
            set
            {
                m_Reticle = value;
                if (Application.isPlaying)
                    SetupReticle();
            }
        }

        [SerializeField]
        GameObject m_BlockedReticle;
        /// <summary>
        /// Stores the reticle that appears at the end of the line when the interactor has a valid target but selection is blocked.
        /// </summary>
        /// <remarks>
        /// Unity will instantiate it while playing when it is a Prefab asset.
        /// </remarks>
        public GameObject blockedReticle
        {
            get => m_BlockedReticle;
            set
            {
                m_BlockedReticle = value;
                if (Application.isPlaying)
                    SetupBlockedReticle();
            }
        }

        [SerializeField]
        bool m_StopLineAtFirstRaycastHit = true;
        /// <summary>
        /// Controls whether this behavior always cuts the line short at the first ray cast hit, even when invalid.
        /// </summary>
        /// <remarks>
        /// The line will always stop short at valid targets, even if this property is set to false.
        /// If you wish this line to pass through valid targets, they must be placed on a different layer.
        /// <see langword="true"/> means to do the same even when pointing at an invalid target.
        /// <see langword="false"/> means the line will continue to the configured line length.
        /// </remarks>
        public bool stopLineAtFirstRaycastHit
        {
            get => m_StopLineAtFirstRaycastHit;
            set => m_StopLineAtFirstRaycastHit = value;
        }

        [SerializeField]
        bool m_StopLineAtSelection;
        /// <summary>
        /// Controls whether the line will stop at the attach point of the closest interactable selected by the interactor, if there is one.
        /// </summary>
        public bool stopLineAtSelection
        {
            get => m_StopLineAtSelection;
            set => m_StopLineAtSelection = value;
        }

        [SerializeField]
        bool m_SnapEndpointIfAvailable = true;
        /// <summary>
        /// Controls whether the visualized line will snap endpoint if the ray hits a XRInteractableSnapVolume.
        /// </summary>
        /// <remarks>
        /// Currently snapping only works with an <see cref="XRRayInteractor"/>.
        /// </remarks>
        public bool snapEndpointIfAvailable
        {
            get => m_SnapEndpointIfAvailable;
            set => m_SnapEndpointIfAvailable = value;
        }

        [SerializeField]
        [Range(k_MinLineBendRatio, k_MaxLineBendRatio)]
        float m_LineBendRatio = 0.5f;

        /// <summary>
        /// This ratio determines where the bend point is on a bent line. Line bending occurs due to hitting a snap volume or because the target end point is out of line with the ray. A value of 1 means the line will not bend.
        /// </summary>
        public float lineBendRatio
        {
            get => m_LineBendRatio;
            set => m_LineBendRatio = Mathf.Clamp(value, k_MinLineBendRatio, k_MaxLineBendRatio);
        }

        [SerializeField]
        bool m_OverrideInteractorLineOrigin = true;

        /// <summary>
        /// A boolean value that controls whether to use a different <see cref="Transform"/> as the starting position and direction of the line.
        /// Set to <see langword="true"/> to use the line origin specified by <see cref="lineOriginTransform"/>.
        /// Set to <see langword="false"/> to use the the line origin specified by the interactor.
        /// </summary>
        /// <seealso cref="lineOriginTransform"/>
        /// <seealso cref="IAdvancedLineRenderable.GetLinePoints(ref NativeArray{Vector3},out int,Ray?)"/>
        public bool overrideInteractorLineOrigin
        {
            get => m_OverrideInteractorLineOrigin;
            set => m_OverrideInteractorLineOrigin = value;
        }

        [SerializeField]
        Transform m_LineOriginTransform;

        /// <summary>
        /// The starting position and direction of the line when overriding.
        /// </summary>
        /// <seealso cref="overrideInteractorLineOrigin"/>
        public Transform lineOriginTransform
        {
            get => m_LineOriginTransform;
            set => m_LineOriginTransform = value;
        }

        [SerializeField]
        float m_LineOriginOffset;

        /// <summary>
        /// Offset from line origin along the line direction before line rendering begins. Only works if the line provider is using straight lines.
        /// This value applies even when not overriding the line origin with a different <see cref="Transform"/>.
        /// </summary>
        public float lineOriginOffset
        {
            get => m_LineOriginOffset;
            set => m_LineOriginOffset = value;
        }

        float m_SquareSnapThresholdDistance;

        Vector3 m_ReticlePos;
        Vector3 m_ReticleNormal;
        int m_EndPositionInLine;

        bool m_SnapCurve = true;
        bool m_PerformSetup;
        GameObject m_ReticleToUse;

        LineRenderer m_LineRenderer;

        // Interface to get target point
        ILineRenderable m_LineRenderable;
        IAdvancedLineRenderable m_AdvancedLineRenderable;
        bool m_HasAdvancedLineRenderable;

        IXRSelectInteractor m_LineRenderableAsSelectInteractor;
        IXRHoverInteractor m_LineRenderableAsHoverInteractor;
        XRBaseInteractor m_LineRenderableAsBaseInteractor;
        XRRayInteractor m_LineRenderableAsRayInteractor;

        // Reusable list of target points
        NativeArray<Vector3> m_TargetPoints;
        int m_NumTargetPoints = -1;

        // Reusable lists of target points for the old interface
        Vector3[] m_TargetPointsFallback = Array.Empty<Vector3>();

        // Reusable list of rendered points
        NativeArray<Vector3> m_RenderPoints;
        int m_NumRenderPoints = -1;

        // Reusable list of rendered points to smooth movement
        NativeArray<Vector3> m_PreviousRenderPoints;
        int m_NumPreviousRenderPoints = -1;

        readonly Vector3[] m_ClearArray = { Vector3.zero, Vector3.zero };

        GameObject m_CustomReticle;
        bool m_CustomReticleAttached;

        // Snapping
        XRInteractableSnapVolume m_XRInteractableSnapVolume;
        const int k_NumberOfSegmentsForBendableLine = 20;

        bool m_PreviousShouldBendLine;
        Vector3 m_PreviousLineDirection;

        // Most recent hit information
        Vector3 m_CurrentHitPoint;
        bool m_HasHitInfo;
        bool m_ValidHit;
        float m_LastValidHitTime;
        float m_LastValidLineLength;

        // Previously hit collider
        Collider m_PreviousCollider;
        XROrigin m_XROrigin;

        bool m_HasRayInteractor;
        bool m_HasBaseInteractor;
        bool m_HasHoverInteractor;
        bool m_HasSelectInteractor;

        readonly BindableVariable<float> m_UserScaleVar = new BindableVariable<float>();
        readonly FloatTweenableVariable m_LineLengthOverrideTweenableVariable = new FloatTweenableVariable();
        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Reset()
        {
            // Don't need to do anything; method kept for backwards compatibility.
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            if (Application.isPlaying)
                UpdateSettings();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_LineRenderable = GetComponent<ILineRenderable>();
            m_AdvancedLineRenderable = m_LineRenderable as IAdvancedLineRenderable;
            m_HasAdvancedLineRenderable = m_AdvancedLineRenderable != null;

            if (m_LineRenderable != null)
            {
                if (m_LineRenderable is XRBaseInteractor baseInteractor)
                {
                    m_LineRenderableAsBaseInteractor = baseInteractor;
                    m_HasBaseInteractor = true;
                }

                if (m_LineRenderable is IXRSelectInteractor selectInteractor)
                {
                    m_LineRenderableAsSelectInteractor = selectInteractor;
                    m_HasSelectInteractor = true;
                }

                if (m_LineRenderable is IXRHoverInteractor hoverInteractor)
                {
                    m_LineRenderableAsHoverInteractor = hoverInteractor;
                    m_HasHoverInteractor = true;
                }

                if (m_LineRenderable is XRRayInteractor rayInteractor)
                {
                    m_LineRenderableAsRayInteractor = rayInteractor;
                    m_HasRayInteractor = true;
                }
            }

            FindXROrigin();
            SetupReticle();
            SetupBlockedReticle();
            ClearLineRenderer();
            UpdateSettings();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (m_LineRenderer == null)
            {
                XRLoggingUtils.LogError($"Missing Line Renderer component on {this}. Disabling line visual.", this);
                enabled = false;
                return;
            }

            if (m_LineRenderable == null)
            {
                XRLoggingUtils.LogError($"Missing {nameof(ILineRenderable)} / Ray Interactor component on {this}. Disabling line visual.", this);
                enabled = false;

                m_LineRenderer.enabled = false;
                return;
            }

            m_SnapCurve = true;
            if (m_ReticleToUse != null)
            {
                m_ReticleToUse.SetActive(false);
                m_ReticleToUse = null;
            }

            m_BindingsGroup.AddBinding(m_UserScaleVar.Subscribe(userScale => m_LineRenderer.widthMultiplier = userScale * Mathf.Clamp(m_LineWidth, k_MinLineWidth, k_MaxLineWidth)));

            Application.onBeforeRender += OnBeforeRenderLineVisual;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_BindingsGroup.Clear();

            if (m_LineRenderer != null)
                m_LineRenderer.enabled = false;

            if (m_ReticleToUse != null)
            {
                m_ReticleToUse.SetActive(false);
                m_ReticleToUse = null;
            }

            Application.onBeforeRender -= OnBeforeRenderLineVisual;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            if (m_TargetPoints.IsCreated)
                m_TargetPoints.Dispose();
            if (m_RenderPoints.IsCreated)
                m_RenderPoints.Dispose();
            if (m_PreviousRenderPoints.IsCreated)
                m_PreviousRenderPoints.Dispose();

            m_LineLengthOverrideTweenableVariable.Dispose();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void LateUpdate()
        {
            if (m_PerformSetup)
            {
                UpdateSettings();
                m_PerformSetup = false;
            }

            if (m_LineRenderer.useWorldSpace && m_XROrigin != null)
            {
                // Update line width with user scale
                var xrOrigin = m_XROrigin.Origin;
                var userScale = xrOrigin != null ? xrOrigin.transform.localScale.x : 1f;
                m_UserScaleVar.Value = userScale;
            }
        }

        [BeforeRenderOrder(XRInteractionUpdateOrder.k_BeforeRenderLineVisual)]
        void OnBeforeRenderLineVisual()
        {
            UpdateLineVisual();
        }

        internal void UpdateLineVisual()
        {
            if (m_LineRenderableAsBaseInteractor != null &&
                m_LineRenderableAsBaseInteractor.disableVisualsWhenBlockedInGroup &&
                m_LineRenderableAsBaseInteractor.IsBlockedByInteractionWithinGroup())
            {
                m_LineRenderer.enabled = false;
                return;
            }

            m_NumRenderPoints = 0;

            // Get all the line sample points from the ILineRenderable interface
            if (!GetLinePoints(ref m_TargetPoints, out m_NumTargetPoints) || m_NumTargetPoints == 0)
            {
                m_LineRenderer.enabled = false;
                return;
            }

            var hasSelection = m_HasSelectInteractor && m_LineRenderableAsSelectInteractor.hasSelection;

            // Using a straight line type because it's likely the straight line won't gracefully follow an object not in it's path.
            var hasStraightRayCast = m_HasRayInteractor && m_LineRenderableAsRayInteractor.lineType == XRRayInteractor.LineType.StraightLine;

            // Query the line provider for origin data and apply overrides if needed.
            GetLineOriginAndDirection(ref m_TargetPoints, m_NumTargetPoints, hasStraightRayCast, out var lineOrigin, out var lineDirection);

            // Query the raycaster to determine line hit information and determine if hit was valid. Also check for snap volumes.
            m_ValidHit = ExtractHitInformation(ref m_TargetPoints, m_NumTargetPoints, out var targetEndPoint, out var hitSnapVolume);

            var curveRayTowardAttachPoint = hasSelection && hasStraightRayCast;
            
            // If overriding ray origin, the line end point will be decoupled from the raycast hit point, so we bend towards it.
            bool bendForOverride = m_OverrideInteractorLineOrigin && m_ValidHit;
            var curveRayTowardHitPoint = bendForOverride && hasStraightRayCast;
            
            var shouldBendLine = (hitSnapVolume || curveRayTowardAttachPoint || curveRayTowardHitPoint) && m_LineBendRatio < 1f;

            if (shouldBendLine)
            {
                m_NumTargetPoints = k_NumberOfSegmentsForBendableLine;
                m_EndPositionInLine = m_NumTargetPoints - 1;

                if (curveRayTowardAttachPoint)
                {
                    // This function assumes there is an active selection. Calling it without selection will lead to errors.
                    FindClosestInteractableAttachPoint(lineOrigin, out targetEndPoint);
                }
            }

            // Make sure we have the correct sized arrays for everything.
            EnsureSize(ref m_TargetPoints, m_NumTargetPoints);
            if (!EnsureSize(ref m_RenderPoints, m_NumTargetPoints))
            {
                m_NumRenderPoints = 0;
            }
            if (!EnsureSize(ref m_PreviousRenderPoints, m_NumTargetPoints))
            {
                m_NumPreviousRenderPoints = 0;
            }

            if (shouldBendLine)
            {
                // Since curves regenerate the whole line from key points, we only need to lerp the origin and forward to achieve ideal smoothing results.
                if (m_SmoothMovement)
                {
                    if (m_PreviousShouldBendLine && m_NumPreviousRenderPoints > 0)
                    {
                        var lineDelta = m_FollowTightness * Time.deltaTime;
                        lineDirection = Vector3.Lerp(m_PreviousLineDirection, lineDirection, lineDelta);
                        lineOrigin = Vector3.Lerp( m_PreviousRenderPoints[0], lineOrigin, lineDelta);
                    }
                    m_PreviousLineDirection = lineDirection;
                }

                CalculateLineCurveRenderPoints(m_NumTargetPoints, m_LineBendRatio, lineOrigin, lineDirection, targetEndPoint, ref m_TargetPoints);
            }
            m_PreviousShouldBendLine = shouldBendLine;

            // Unchanged
            // If there is a big movement (snap turn, teleportation), snap the curve
            if (m_NumPreviousRenderPoints != m_NumTargetPoints)
            {
                m_SnapCurve = true;
            }
            // Compare the two endpoints of the curve, as that will have the largest delta.
            else if (m_SmoothMovement &&
                     m_NumPreviousRenderPoints > 0 &&
                     m_NumPreviousRenderPoints <= m_PreviousRenderPoints.Length &&
                     m_NumTargetPoints > 0 &&
                     m_NumTargetPoints <= m_TargetPoints.Length)
            {
                var prevPointIndex = m_NumPreviousRenderPoints - 1;
                var currPointIndex = m_NumTargetPoints - 1;
                m_SnapCurve = Vector3.SqrMagnitude(m_PreviousRenderPoints[prevPointIndex] - m_TargetPoints[currPointIndex]) > m_SquareSnapThresholdDistance;
            }

            AdjustLineAndReticle(hasSelection, shouldBendLine, lineOrigin, targetEndPoint);

            // We don't smooth points for the bent line as we smooth it when computing the curve
            var shouldSmoothPoints = !shouldBendLine && m_SmoothMovement && (m_NumPreviousRenderPoints == m_NumTargetPoints) && !m_SnapCurve;

            if (m_OverrideInteractorLineLength || shouldSmoothPoints)
            {
                var float3TargetPoints = m_TargetPoints.Reinterpret<float3>();
                var float3PrevRenderPoints = m_PreviousRenderPoints.Reinterpret<float3>();
                var float3RenderPoints = m_RenderPoints.Reinterpret<float3>();

                var newLineLength = m_OverrideInteractorLineLength && m_AutoAdjustLineLength
                    ? UpdateTargetLineLength(lineOrigin, targetEndPoint, m_MinLineLength, m_LineLength, m_LineRetractionDelay, m_LineLengthChangeSpeed, m_ValidHit || hasSelection, m_UseDistanceToHitAsMaxLineLength)
                    : m_LineLength;

                m_NumRenderPoints = ComputeNewRenderPoints(m_NumRenderPoints, m_NumTargetPoints, newLineLength,
                    shouldSmoothPoints, m_OverrideInteractorLineLength, m_FollowTightness * Time.deltaTime,
                    ref float3TargetPoints, ref float3PrevRenderPoints, ref float3RenderPoints);
            }
            else
            {
                // Copy from m_TargetPoints into m_RenderPoints
                NativeArray<Vector3>.Copy(m_TargetPoints, 0, m_RenderPoints, 0, m_NumTargetPoints);
                m_NumRenderPoints = m_NumTargetPoints;
            }

            // When a straight line has only two points and color gradients have more than two keys,
            // interpolate points between the two points to enable better color gradient effects.
            if (m_ValidHit || m_TreatSelectionAsValidState && hasSelection)
            {
                // Use regular valid state visuals unless we are hovering and selection is blocked.
                // We use regular valid state visuals if not hovering because the blocked state does not apply
                // (e.g. we could have a valid target that is UI and therefore not hoverable or selectable as an interactable).
                var useBlockedVisuals = false;
                if (!hasSelection && m_HasBaseInteractor && m_LineRenderableAsBaseInteractor.hasHover)
                {
                    var interactionManager = m_LineRenderableAsBaseInteractor.interactionManager;
                    var canSelectSomething = false;
                    foreach (var interactable in m_LineRenderableAsBaseInteractor.interactablesHovered)
                    {
                        if (interactable is IXRSelectInteractable selectInteractable && interactionManager.IsSelectPossible(m_LineRenderableAsBaseInteractor, selectInteractable))
                        {
                            canSelectSomething = true;
                            break;
                        }
                    }

                    useBlockedVisuals = !canSelectSomething;
                }

                SetColorGradient(useBlockedVisuals ? m_BlockedColorGradient : m_ValidColorGradient);
                AssignReticle(useBlockedVisuals);
            }
            else
            {
                ClearReticle();
                SetColorGradient(m_InvalidColorGradient);
            }

            if (m_NumRenderPoints >= 2)
            {
                m_LineRenderer.enabled = true;
                m_LineRenderer.positionCount = m_NumRenderPoints;
                m_LineRenderer.SetPositions(m_RenderPoints);
            }
            else
            {
                m_LineRenderer.enabled = false;
                return;
            }

            // Update previous points
            // Copy from m_RenderPoints into m_PreviousRenderPoints
            NativeArray<Vector3>.Copy(m_RenderPoints, 0, m_PreviousRenderPoints, 0, m_NumRenderPoints);
            m_NumPreviousRenderPoints = m_NumRenderPoints;
            m_SnapCurve = false;
        }

        bool GetLinePoints(ref NativeArray<Vector3> linePoints, out int numPoints)
        {
            if (m_HasAdvancedLineRenderable)
            {
                Ray? rayOriginOverride = null;
                if (m_OverrideInteractorLineOrigin && m_LineOriginTransform != null)
                {
                    var lineOrigin = m_LineOriginTransform.position;
                    var lineDirection = m_LineOriginTransform.forward;
                    rayOriginOverride = new Ray(lineOrigin, lineDirection);
                }

                return m_AdvancedLineRenderable.GetLinePoints(ref linePoints, out numPoints, rayOriginOverride);
            }

            var hasLinePoint = m_LineRenderable.GetLinePoints(ref m_TargetPointsFallback, out numPoints);
            EnsureSize(ref linePoints, numPoints);
            NativeArray<Vector3>.Copy(m_TargetPointsFallback, linePoints, numPoints);
            return hasLinePoint;
        }

        void AdjustLineAndReticle(bool hasSelection, bool bendLine, in Vector3 lineOrigin, in Vector3 targetEndPoint)
        {
            // If the line hits, insert reticle position into the list for smoothing.
            // Remove the last point in the list to keep the number of points consistent.
            if (m_HasHitInfo)
            {
                m_ReticlePos = targetEndPoint;

                // End the line at the current hit point.
                if ((m_ValidHit || m_StopLineAtFirstRaycastHit) && m_EndPositionInLine > 0 && m_EndPositionInLine < m_NumTargetPoints)
                {
                    // The hit position might not lie within the line segment, for example if a sphere cast is used, so use a point projected onto the
                    // segment so that the endpoint is continuous with the rest of the curve.
                    var lastSegmentStartPoint = m_TargetPoints[m_EndPositionInLine - 1];
                    var lastSegmentEndPoint = m_TargetPoints[m_EndPositionInLine];
                    var lastSegment = lastSegmentEndPoint - lastSegmentStartPoint;
                    var projectedHitSegment = Vector3.Project(m_ReticlePos - lastSegmentStartPoint, lastSegment);

                    // Don't bend the line backwards
                    if (Vector3.Dot(projectedHitSegment, lastSegment) < 0)
                        projectedHitSegment = Vector3.zero;

                    m_ReticlePos = lastSegmentStartPoint + projectedHitSegment;
                    m_TargetPoints[m_EndPositionInLine] = m_ReticlePos;
                    m_NumTargetPoints = m_EndPositionInLine + 1;
                }
            }

            // Stop line if there is a selection
            if (m_StopLineAtSelection && hasSelection && !bendLine)
            {
                // Use the selected interactable closest to the start of the line.
                var sqrMagnitude = Vector3.SqrMagnitude(targetEndPoint - lineOrigin);

                // Only stop at selection if it is closer than the current end point.
                var currentEndSqDistance = Vector3.SqrMagnitude(m_TargetPoints[m_EndPositionInLine] - lineOrigin);
                if (sqrMagnitude < currentEndSqDistance || m_EndPositionInLine == 0)
                {
                    // Find out where the selection point belongs in the line points. Use the closest target point.
                    var endPositionForSelection = 1;
                    var sqDistanceFromEndPoint = Vector3.SqrMagnitude(m_TargetPoints[endPositionForSelection] - targetEndPoint);
                    for (var i = 2; i < m_NumTargetPoints; i++)
                    {
                        var sqDistance = Vector3.SqrMagnitude(m_TargetPoints[i] - targetEndPoint);
                        if (sqDistance < sqDistanceFromEndPoint)
                        {
                            endPositionForSelection = i;
                            sqDistanceFromEndPoint = sqDistance;
                        }
                        else
                        {
                            break;
                        }
                    }

                    m_EndPositionInLine = endPositionForSelection;
                    m_NumTargetPoints = m_EndPositionInLine + 1;
                    m_ReticlePos = targetEndPoint;
                    if (!m_HasHitInfo)
                        m_ReticleNormal = Vector3.Normalize(m_TargetPoints[m_EndPositionInLine - 1] - m_ReticlePos);
                    m_TargetPoints[m_EndPositionInLine] = m_ReticlePos;
                }
            }
        }

        void FindClosestInteractableAttachPoint(in Vector3 lineOrigin, out Vector3 closestPoint)
        {
            // Use the selected interactable closest to the start of the line.
            var interactablesSelected = m_LineRenderableAsSelectInteractor.interactablesSelected;
            closestPoint = interactablesSelected[0].GetAttachTransform(m_LineRenderableAsSelectInteractor).position;

            if (interactablesSelected.Count > 1)
            {
                var closestSqDistance = Vector3.SqrMagnitude(closestPoint - lineOrigin);
                for (var i = 1; i < interactablesSelected.Count; ++i)
                {
                    var endPoint = interactablesSelected[i].GetAttachTransform(m_LineRenderableAsSelectInteractor).position;
                    var sqDistance = Vector3.SqrMagnitude(endPoint - lineOrigin);
                    if (sqDistance < closestSqDistance)
                    {
                        closestPoint = endPoint;
                        closestSqDistance = sqDistance;
                    }
                }
            }
        }

        static bool EnsureSize(ref NativeArray<Vector3> array, int targetSize)
        {
            if (array.IsCreated && array.Length >= targetSize)
                return true;

            if (array.IsCreated)
                array.Dispose();

            array = new NativeArray<Vector3>(targetSize, Allocator.Persistent);
            return false;
        }

        void GetLineOriginAndDirection(ref NativeArray<Vector3> targetPoints, int numTargetPoints, bool isLineStraight, out Vector3 lineOrigin, out Vector3 lineDirection)
        {
            if (m_OverrideInteractorLineOrigin && m_LineOriginTransform != null)
            {
                lineOrigin = m_LineOriginTransform.position;
                lineDirection = m_LineOriginTransform.forward;
            }
            else
            {
                if (m_HasAdvancedLineRenderable)
                {
                    // Get accurate line origin and direction.
                    m_AdvancedLineRenderable.GetLineOriginAndDirection(out lineOrigin, out lineDirection);
                }
                else
                {
                    lineOrigin = targetPoints[0];
                    var lineEnd = targetPoints[numTargetPoints - 1];
                    lineDirection = (lineEnd - lineOrigin).normalized;
                }
            }

            // If we have a straight line and offset is greater than 0, but smaller than our override length, we apply the offset.
            if (isLineStraight &&
                m_LineOriginOffset > 0f && (!m_OverrideInteractorLineLength || m_LineOriginOffset < m_LineLength))
            {
                lineOrigin += lineDirection * m_LineOriginOffset;
            }
            
            // Write the modified line origin back into the array
            targetPoints[0] = lineOrigin;
        }

        bool ExtractHitInformation(ref NativeArray<Vector3> targetPoints, int numTargetPoints, out Vector3 targetEndPoint, out bool hitSnapVolume)
        {
            Collider hitCollider = null;
            hitSnapVolume = false;
            // NativeArray<T> does not implement the indexer operator as a readonly get (C# 8 feature),
            // so this method param is ref instead of in to avoid a defensive copy being created.
            targetEndPoint = targetPoints[numTargetPoints - 1];

            m_HasHitInfo = m_LineRenderable.TryGetHitInfo(out m_CurrentHitPoint, out m_ReticleNormal, out m_EndPositionInLine, out var validHit);
            if (m_HasHitInfo)
            {
                targetEndPoint = m_CurrentHitPoint;

                if (validHit && m_SnapEndpointIfAvailable && m_HasRayInteractor)
                {
                    // When hovering a new collider, check if it has a specified snapping volume, if it does then get the closest point on it
                    if (m_LineRenderableAsRayInteractor.TryGetCurrent3DRaycastHit(out var raycastHit, out _))
                        hitCollider = raycastHit.collider;

                    if (hitCollider != m_PreviousCollider && hitCollider != null)
                        m_LineRenderableAsBaseInteractor.interactionManager.TryGetInteractableForCollider(hitCollider, out _, out m_XRInteractableSnapVolume);

                    if (m_XRInteractableSnapVolume != null)
                    {
                        // If we have a selection, get the closest point to the attach transform position on the snap to collider 
                        targetEndPoint = m_LineRenderableAsRayInteractor.hasSelection
                            ? m_XRInteractableSnapVolume.GetClosestPointOfAttachTransform(m_LineRenderableAsRayInteractor)
                            : m_XRInteractableSnapVolume.GetClosestPoint(targetEndPoint);
                        
                        m_EndPositionInLine = k_NumberOfSegmentsForBendableLine - 1; // Override hit index because we're going to use a custom line where the hit point is the end
                        hitSnapVolume = true;
                    }
                }
            }

            if (hitCollider == null)
                m_XRInteractableSnapVolume = null;

            m_PreviousCollider = hitCollider;

            return validHit;
        }

        /// <summary>
        /// Calculates the target render points based on the targeted snapped endpoint and the actual position of the raycast line.
        /// </summary>
#if UNITY_2022_2_OR_NEWER && BURST_PRESENT
        [BurstCompile]
#endif
        static void CalculateLineCurveRenderPoints(int numTargetPoints, float curveRatio, in Vector3 lineOrigin, in Vector3 lineDirection, in Vector3 endPoint, ref NativeArray<Vector3> targetPoints)
        {
            var float3TargetPoints = targetPoints.Reinterpret<float3>();
            CurveUtility.GenerateCubicBezierCurve(numTargetPoints, curveRatio, lineOrigin, lineDirection, endPoint, ref float3TargetPoints);
        }

#if UNITY_2022_2_OR_NEWER && BURST_PRESENT
        [BurstCompile]
#endif
        static int ComputeNewRenderPoints(int numRenderPoints, int numTargetPoints, float targetLineLength, bool shouldSmoothPoints, bool shouldOverwritePoints, float pointSmoothIncrement,
            ref NativeArray<float3> targetPoints, ref NativeArray<float3> previousRenderPoints, ref NativeArray<float3> renderPoints)
        {
            var length = 0f;
            var maxRenderPoints = renderPoints.Length;
            var finalNumRenderPoints = numRenderPoints;
            for (var i = 0; i < numTargetPoints && finalNumRenderPoints < maxRenderPoints; ++i)
            {
                var targetPoint = targetPoints[i];
                var newPoint = !shouldSmoothPoints ? targetPoint : math.lerp(previousRenderPoints[i], targetPoint, pointSmoothIncrement);

                if (shouldOverwritePoints && finalNumRenderPoints > 0 && maxRenderPoints > 0)
                {
                    var lastRenderPoint = renderPoints[finalNumRenderPoints - 1];
                    if (EvaluateLineEndPoint(targetLineLength, shouldSmoothPoints, targetPoint, lastRenderPoint, ref newPoint, ref length))
                    {
                        renderPoints[finalNumRenderPoints] = newPoint;
                        finalNumRenderPoints++;
                        break;
                    }
                }

                renderPoints[finalNumRenderPoints] = newPoint;
                finalNumRenderPoints++;
            }

            return finalNumRenderPoints;
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static bool EvaluateLineEndPoint(float targetLineLength, bool shouldSmoothPoint, in float3 unsmoothedTargetPoint, in float3 lastRenderPoint, ref float3 newRenderPoint, ref float lineLength)
        {
            var segmentVector = newRenderPoint - lastRenderPoint;
            var segmentLength = math.length(segmentVector);

            if (shouldSmoothPoint)
            {
                var lengthToUnsmoothedSegment = math.distance(lastRenderPoint, unsmoothedTargetPoint);

                // If we hit something, we need to shorten the ray immediately and not wait for the smoothed end point to catch up.
                if (lengthToUnsmoothedSegment < segmentLength)
                {
                    newRenderPoint = lastRenderPoint + math.normalize(segmentVector) * lengthToUnsmoothedSegment;
                    segmentLength = lengthToUnsmoothedSegment;
                }
            }

            lineLength += segmentLength;
            if (lineLength <= targetLineLength)
                return false;

            var delta = lineLength - targetLineLength;

            // Re-project final point to match the desired length
            var tVal = 1 - (delta / segmentLength);
            newRenderPoint = math.lerp(lastRenderPoint, newRenderPoint, tVal);
            return true;
        }

        float UpdateTargetLineLength(in Vector3 lineOrigin, in Vector3 hitPoint, float minimumLineLength, float maximumLineLength, float lineRetractionDelaySeconds, float lineRetractionScalar, bool hasHit, bool deriveMaxLineLength)
        {
            var currentTime = Time.unscaledTime;

            if (hasHit)
            {
                m_LastValidHitTime = Time.unscaledTime;
                m_LastValidLineLength = deriveMaxLineLength ? Mathf.Min(Vector3.Distance(lineOrigin, hitPoint), maximumLineLength) : maximumLineLength;
            }

            var timeSinceLastValidHit = currentTime - m_LastValidHitTime;

            if (timeSinceLastValidHit > lineRetractionDelaySeconds)
            {
                m_LineLengthOverrideTweenableVariable.target = minimumLineLength;

                var timeScalar = (timeSinceLastValidHit - lineRetractionDelaySeconds) * lineRetractionScalar;

                // Accelerate line shrinking over time
                m_LineLengthOverrideTweenableVariable.HandleTween(Time.unscaledDeltaTime * timeScalar);
            }
            else
            {
                m_LineLengthOverrideTweenableVariable.target = Mathf.Max(m_LastValidLineLength, minimumLineLength);
                m_LineLengthOverrideTweenableVariable.HandleTween(Time.unscaledDeltaTime * lineRetractionScalar);
            }

            return m_LineLengthOverrideTweenableVariable.Value;
        }

        void AssignReticle(bool useBlockedVisuals)
        {
            // Set reticle position and show reticle
            var previouslyUsedReticle = m_ReticleToUse;
            var validStateReticle = useBlockedVisuals ? m_BlockedReticle : m_Reticle;
            m_ReticleToUse = m_CustomReticleAttached ? m_CustomReticle : validStateReticle;
            if (previouslyUsedReticle != null && previouslyUsedReticle != m_ReticleToUse)
                previouslyUsedReticle.SetActive(false);

            if (m_ReticleToUse != null)
            {
                m_ReticleToUse.transform.position = m_ReticlePos;
                if (m_HasHoverInteractor && m_LineRenderableAsHoverInteractor.GetOldestInteractableHovered() is IXRReticleDirectionProvider reticleDirectionProvider)
                {
                    reticleDirectionProvider.GetReticleDirection(m_LineRenderableAsHoverInteractor, m_ReticleNormal, out var reticleUp, out var reticleForward);
                    Quaternion lookRotation;
                    if (reticleForward.HasValue)
                        BurstMathUtility.LookRotationWithForwardProjectedOnPlane(reticleForward.Value, reticleUp, out lookRotation);
                    else
                        BurstMathUtility.LookRotationWithForwardProjectedOnPlane(m_ReticleToUse.transform.forward, reticleUp, out lookRotation);

                    m_ReticleToUse.transform.rotation = lookRotation;
                }
                else
                {
                    m_ReticleToUse.transform.forward = -m_ReticleNormal;
                }

                m_ReticleToUse.SetActive(true);
            }
        }

        void ClearReticle()
        {
            if (m_ReticleToUse != null)
            {
                m_ReticleToUse.SetActive(false);
                m_ReticleToUse = null;
            }
        }

        void SetColorGradient(Gradient colorGradient)
        {
            if (!m_SetLineColorGradient)
                return;
            m_LineRenderer.colorGradient = colorGradient;
        }

        void UpdateSettings()
        {
            m_SquareSnapThresholdDistance = m_SnapThresholdDistance * m_SnapThresholdDistance;

            if (TryFindLineRenderer())
            {
                m_LineRenderer.widthMultiplier =  Mathf.Clamp(m_LineWidth, k_MinLineWidth, k_MaxLineWidth);
                m_LineRenderer.widthCurve = m_WidthCurve;
                m_SnapCurve = true;
            }

            m_LineLengthOverrideTweenableVariable.target = lineLength;
            m_LineLengthOverrideTweenableVariable.HandleTween(1f);
        }

        bool TryFindLineRenderer()
        {
            m_LineRenderer = GetComponent<LineRenderer>();
            if (m_LineRenderer == null)
            {
                Debug.LogWarning("No Line Renderer found for Interactor Line Visual.", this);
                enabled = false;
                return false;
            }
            return true;
        }

        void ClearLineRenderer()
        {
            if (TryFindLineRenderer())
            {
                m_LineRenderer.SetPositions(m_ClearArray);
                m_LineRenderer.positionCount = 0;
            }
        }

        void FindXROrigin()
        {
            if (m_XROrigin == null)
                ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin);
        }

        void SetupReticle()
        {
            if (m_Reticle == null)
                return;

            // Instantiate if the reticle is a Prefab asset rather than a scene GameObject
            if (!m_Reticle.scene.IsValid())
                m_Reticle = Instantiate(m_Reticle);

            m_Reticle.SetActive(false);
        }

        void SetupBlockedReticle()
        {
            if (m_BlockedReticle == null)
                return;

            // Instantiate if the reticle is a Prefab asset rather than a scene GameObject
            if (!m_BlockedReticle.scene.IsValid())
                m_BlockedReticle = Instantiate(m_BlockedReticle);

            m_BlockedReticle.SetActive(false);
        }

        /// <inheritdoc />
        public bool AttachCustomReticle(GameObject reticleInstance)
        {
            m_CustomReticle = reticleInstance;
            m_CustomReticleAttached = true;
            return true;
        }

        /// <inheritdoc />
        public bool RemoveCustomReticle()
        {
            m_CustomReticle = null;
            m_CustomReticleAttached = false;
            return true;
        }
    }
}