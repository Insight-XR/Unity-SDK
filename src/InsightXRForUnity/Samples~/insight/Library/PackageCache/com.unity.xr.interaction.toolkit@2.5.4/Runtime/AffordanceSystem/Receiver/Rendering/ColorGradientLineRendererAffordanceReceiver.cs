using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Affordance receiver used to animate Line Renderer start or end color.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Color Gradient Line Renderer Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_ColorGradientLineRendererAffordanceReceiver)]
    public class ColorGradientLineRendererAffordanceReceiver : ColorAffordanceReceiver
    {
        /// <summary>
        /// Controls which Line Renderer property is updated when applying color animated with affordance receiver.
        /// </summary>
        /// <seealso cref="lineColorProperty"/>
        public enum LineColorProperty
        {
            /// <summary>
            /// Animates Line Renderer start color.
            /// </summary>
            /// <seealso cref="LineRenderer.startColor"/>
            StartColor,
            
            /// <summary>
            /// Animates Line Renderer end color.
            /// </summary>
            /// <seealso cref="LineRenderer.endColor"/>
            EndColor,
        }
        
        [SerializeField]
        [Tooltip("Line Renderer on which to animate colors.")]
        LineRenderer m_LineRenderer;

        /// <summary>
        /// Line Renderer on which to animate colors.
        /// </summary>
        public LineRenderer lineRenderer
        {
            get => m_LineRenderer;
            set => m_LineRenderer = value;
        }

        [SerializeField]
        [Tooltip("Mode determining how color is applied to the associated Line Renderer.")]
        LineColorProperty m_LineColorProperty = LineColorProperty.StartColor;

        /// <summary>
        /// Mode determining how color is applied to the associated Line Renderer.
        /// </summary>
        /// <seealso cref="LineColorProperty"/>
        public LineColorProperty lineColorProperty
        {
            get => m_LineColorProperty;
            set
            {
                m_LineColorProperty = value;
                CaptureInitialValue();
            }
        }
        
        [SerializeField]
        [Tooltip("Prevent XR Interactor Line Visual from controlling line rendering color if present.")]
        bool m_DisableXRInteractorLineVisualColorControlIfPresent = true;

        /// <summary>
        /// Prevent <see cref="XRInteractorLineVisual"/> from controlling line rendering color if present.
        /// </summary>
        /// <seealso cref="XRInteractorLineVisual.setLineColorGradient"/>
        public bool disableXRInteractorLineVisualColorControlIfPresent
        {
            get => m_DisableXRInteractorLineVisualColorControlIfPresent;
            set => m_DisableXRInteractorLineVisualColorControlIfPresent = value;
        }

        Color m_InitialStartColor;
        Color m_InitialEndColor;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            if (m_LineRenderer == null)
            {
                m_LineRenderer = GetComponentInParent<LineRenderer>();
                if (m_LineRenderer == null)
                {
                    XRLoggingUtils.LogError("Missing Line Renderer on " + this, this);
                    enabled = false;
                }
            }
        }

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();

            // If the XRInteractorLineVisual is present, we need to disable it from handling the line visuals.
            if (m_DisableXRInteractorLineVisualColorControlIfPresent)
            {
                var lineVisual = GetComponentInParent<XRInteractorLineVisual>();
                if (lineVisual != null)
                {
                    lineVisual.setLineColorGradient = false;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnAffordanceValueUpdated(Color newValue)
        {
            base.OnAffordanceValueUpdated(newValue);

            switch (m_LineColorProperty)
            {
                case LineColorProperty.StartColor:
                    m_LineRenderer.startColor = newValue;
                    break;
                case LineColorProperty.EndColor:
                    m_LineRenderer.endColor = newValue;
                    break;
            }
        }

        /// <inheritdoc />
        protected override void CaptureInitialValue()
        {
            if (initialValueCaptured)
                return;
            
            m_InitialStartColor = m_LineRenderer.startColor;
            m_InitialEndColor = m_LineRenderer.endColor;
            base.CaptureInitialValue();
        }

        /// <inheritdoc />
        protected override Color GetCurrentValueForCapture()
        {
            switch (m_LineColorProperty)
            {
                case LineColorProperty.StartColor:
                    return m_InitialStartColor;
                case LineColorProperty.EndColor:
                    return m_InitialEndColor;
                default:
                    return base.GetCurrentValueForCapture();
            }
        }
    }
}