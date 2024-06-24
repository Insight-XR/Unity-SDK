using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.UI
{
    /// <summary>
    /// Affordance receiver applying a color theme to a UGUI Image component 
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/UI/Image Color Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_ImageColorAffordanceReceiver)]
    public class ImageColorAffordanceReceiver : ColorAffordanceReceiver
    {
        [Tooltip("Image to apply the color to.")]
        [SerializeField]
        Image m_Image;

        /// <summary>
        /// Image to apply the color to.
        /// </summary>
        public Image image
        {
            get => m_Image;
            set => m_Image = value;
        }

        [Tooltip("If set, alpha changes will be applied to the CanvasGroup rather than the Image.")]
        [SerializeField]
        CanvasGroup m_CanvasGroup;

        /// <summary>
        /// If set, alpha changes will be applied to the <see cref="CanvasGroup"/> rather than the <see cref="Image"/>.
        /// </summary>
        public CanvasGroup canvasGroup
        {
            get => m_CanvasGroup;
            set => m_CanvasGroup = value;
        }

        [Tooltip("Ignore alpha changes in color theme.")]
        [SerializeField]
        bool m_IgnoreAlpha;

        /// <summary>
        /// Ignore alpha changes in color theme.
        /// </summary>
        public bool ignoreAlpha
        {
            get => m_IgnoreAlpha;
            set => m_IgnoreAlpha = value;
        }

        bool m_HasImage = false;
        bool m_HasCanvasGroup = false;
        
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            m_HasImage = m_Image != null;
            m_HasCanvasGroup = m_CanvasGroup != null;
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(Color newValue)
        {
            if (m_HasImage)
            {
                if (m_HasCanvasGroup)
                {
                    m_Image.color = new Color(newValue.r, newValue.g, newValue.b, 1f);
                    if (!m_IgnoreAlpha)
                        m_CanvasGroup.alpha = newValue.a;
                }
                else
                {
                    m_Image.color = m_IgnoreAlpha ? new Color(newValue.r, newValue.g, newValue.b, 1f) : newValue;
                }
            }

            base.OnAffordanceValueUpdated(newValue);
        }

        /// <inheritdoc/>
        protected override Color GetCurrentValueForCapture()
        {
            return m_HasImage ? m_Image.color : base.GetCurrentValueForCapture();
        }
    }
}