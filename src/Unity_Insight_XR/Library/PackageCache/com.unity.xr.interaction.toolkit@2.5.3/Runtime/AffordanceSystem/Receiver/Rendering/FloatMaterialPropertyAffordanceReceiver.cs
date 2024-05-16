using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Apply affordance to material property block float property.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Float Material Property Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_FloatMaterialPropertyAffordanceReceiver)]
    [RequireComponent(typeof(MaterialPropertyBlockHelper))]
    public class FloatMaterialPropertyAffordanceReceiver : FloatAffordanceReceiver
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
        [Tooltip("Shader property name to set the float value of.")]
        string m_FloatPropertyName;

        /// <summary>
        /// Shader property name to set the float value of.
        /// </summary>
        public string floatPropertyName
        {
            get => m_FloatPropertyName;
            set
            {
                m_FloatPropertyName = value;
                m_FloatProperty = Shader.PropertyToID(m_FloatPropertyName);
            }
        }

        int m_FloatProperty;

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

            m_FloatProperty = Shader.PropertyToID(m_FloatPropertyName);
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float newValue)
        {
            m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetFloat(m_FloatProperty, newValue);
            base.OnAffordanceValueUpdated(newValue);
        }
        
        /// <inheritdoc/>
        protected override float GetCurrentValueForCapture()
        {
            return m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetFloat(m_FloatProperty);
        }
    }
}