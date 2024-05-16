using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Apply affordance to material property block Vector3 property.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Vector3 Material Property Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_Vector3MaterialPropertyAffordanceReceiver)]
    [RequireComponent(typeof(MaterialPropertyBlockHelper))]
    public class Vector3MaterialPropertyAffordanceReceiver : Vector3AffordanceReceiver
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
        string m_Vector3PropertyName;

        /// <summary>
        /// Shader property name to set the vector value of.
        /// </summary>
        public string vector3PropertyName
        {
            get => m_Vector3PropertyName;
            set
            {
                m_Vector3PropertyName = value;
                m_Vector3Property = Shader.PropertyToID(m_Vector3PropertyName);
            }
        }

        int m_Vector3Property;

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

            m_Vector3Property = Shader.PropertyToID(m_Vector3PropertyName);
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float3 newValue)
        {
            m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetVector(m_Vector3Property, (Vector3)newValue);
            base.OnAffordanceValueUpdated(newValue);
        }
        
        /// <inheritdoc/>
        protected override float3 GetCurrentValueForCapture()
        {
            return (Vector3)m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetVector(m_Vector3Property);
        }
    }
}