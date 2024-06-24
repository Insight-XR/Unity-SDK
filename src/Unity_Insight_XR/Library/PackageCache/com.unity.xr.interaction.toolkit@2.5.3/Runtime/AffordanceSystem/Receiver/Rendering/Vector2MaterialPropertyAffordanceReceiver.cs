using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Apply affordance to material property block Vector2 property.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Vector2 Material Property Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_Vector2MaterialPropertyAffordanceReceiver)]
    [RequireComponent(typeof(MaterialPropertyBlockHelper))]
    public class Vector2MaterialPropertyAffordanceReceiver : Vector2AffordanceReceiver
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
        string m_Vector2PropertyName;

        /// <summary>
        /// Shader property name to set the vector value of.
        /// </summary>
        public string vector2PropertyName
        {
            get => m_Vector2PropertyName;
            set
            {
                m_Vector2PropertyName = value;
                m_Vector2Property = Shader.PropertyToID(m_Vector2PropertyName);
            }
        }

        int m_Vector2Property;

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

            m_Vector2Property = Shader.PropertyToID(m_Vector2PropertyName);
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float2 newValue)
        {
            m_MaterialPropertyBlockHelper.GetMaterialPropertyBlock()?.SetVector(m_Vector2Property, (Vector2)newValue);
            base.OnAffordanceValueUpdated(newValue);
        }

        /// <inheritdoc/>
        protected override float2 GetCurrentValueForCapture()
        {
            return (Vector2)m_MaterialPropertyBlockHelper.GetSharedMaterialForTarget().GetVector(m_Vector2Property);
        }
    }
}