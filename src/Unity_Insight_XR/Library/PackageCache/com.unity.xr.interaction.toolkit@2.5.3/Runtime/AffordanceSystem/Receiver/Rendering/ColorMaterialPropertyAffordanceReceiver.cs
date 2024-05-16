using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Apply affordance to material property block color property.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Color Material Property Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_ColorMaterialPropertyAffordanceReceiver)]
    [RequireComponent(typeof(MaterialPropertyBlockHelper))]
    public class ColorMaterialPropertyAffordanceReceiver : ColorAffordanceReceiver
    {
        [SerializeField]
        [Tooltip("Material Property Block Helper component reference used to set material properties.")]
        MaterialPropertyBlockHelper m_MaterialPropertyBlockHelper;

        /// <summary>
        /// Material Property Block Helper component reference used to set material properties.
        /// </summary>
        public MaterialPropertyBlockHelper materialPropertyBlockHelper
        {
            get => m_MaterialPropertyBlockHelper;
            set => m_MaterialPropertyBlockHelper = value;
        }

        [SerializeField]
        [Tooltip("Shader property name to set the color of. When empty, the component will attempt to use the default for the current render pipeline.")]
        string m_ColorPropertyName;

        /// <summary>
        /// Shader property name to set the color of.
        /// </summary>
        public string colorPropertyName
        {
            get => m_ColorPropertyName;
            set
            {
                m_ColorPropertyName = value;
                UpdateColorPropertyID();
            }
        }

        int m_ColorProperty;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            if (m_MaterialPropertyBlockHelper == null)
                m_MaterialPropertyBlockHelper = GetComponent<MaterialPropertyBlockHelper>();
        }

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();

            if (m_MaterialPropertyBlockHelper == null)
                m_MaterialPropertyBlockHelper = GetComponent<MaterialPropertyBlockHelper>();

            UpdateColorPropertyID();
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(Color newValue)
        {
            m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetColor(m_ColorProperty, newValue);
            base.OnAffordanceValueUpdated(newValue);
        }

        /// <inheritdoc/>
        protected override Color GetCurrentValueForCapture()
        {
            return m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetColor(m_ColorProperty);
        }

        void UpdateColorPropertyID()
        {
            if (!string.IsNullOrEmpty(m_ColorPropertyName))
            {
                m_ColorProperty = Shader.PropertyToID(m_ColorPropertyName);
            }
            else
            {
                m_ColorProperty = GraphicsSettings.currentRenderPipeline != null ? ShaderPropertyLookup.baseColor : ShaderPropertyLookup.color;
            }
        }

        readonly struct ShaderPropertyLookup
        {
            public static readonly int baseColor = Shader.PropertyToID("_BaseColor");
            public static readonly int color = Shader.PropertyToID("_Color"); // Legacy
        }
    }
}