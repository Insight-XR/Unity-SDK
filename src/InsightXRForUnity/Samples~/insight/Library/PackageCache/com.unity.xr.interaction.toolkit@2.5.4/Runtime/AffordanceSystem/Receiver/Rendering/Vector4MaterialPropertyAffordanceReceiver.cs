using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Apply affordance to material property block Vector4 property.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Vector4 Material Property Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_Vector4MaterialPropertyAffordanceReceiver)]
    [RequireComponent(typeof(MaterialPropertyBlockHelper))]
    public class Vector4MaterialPropertyAffordanceReceiver : Vector4AffordanceReceiver
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
        [Tooltip("Shader property name to set the vector value of.")]
        string m_Vector4PropertyName;

        /// <summary>
        /// Shader property name to set the vector value of.
        /// </summary>
        public string vector4PropertyName
        {
            get => m_Vector4PropertyName;
            set
            {
                m_Vector4PropertyName = value;
                m_Vector4Property = Shader.PropertyToID(m_Vector4PropertyName);
            }
        }

        int m_Vector4Property;

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

            m_Vector4Property = Shader.PropertyToID(m_Vector4PropertyName);
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float4 newValue)
        {
            m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetVector(m_Vector4Property, newValue);
            base.OnAffordanceValueUpdated(newValue);
        }

        /// <inheritdoc/>
        protected override float4 GetCurrentValueForCapture()
        {
            return m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetVector(m_Vector4Property);
        }
    }
}