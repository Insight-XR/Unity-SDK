using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Transformation
{
    /// <summary>
    /// Affordance receiver that takes an object transform and applies a relative uniform scale multiplier on the start value.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Transformation/Uniform Transform Scale Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_UniformTransformScaleAffordanceReceiver)] 
    public class UniformTransformScaleAffordanceReceiver : FloatAffordanceReceiver
    {
        [SerializeField]
        [Tooltip("Transform on which to apply scale value.")]
        Transform m_TransformToScale = null;
        
        /// <summary>
        /// Transform on which to apply scale value
        /// </summary>
        public Transform transformToScale
        {
            get => m_TransformToScale;
            set
            {
                m_TransformToScale = value;
                m_HasTransformToScale = m_TransformToScale != null;
            }
        }

        bool m_HasTransformToScale = false;
        Vector3 m_InitialScale = Vector3.one;

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            m_HasTransformToScale = m_TransformToScale != null;
        }

        /// <inheritdoc/>
        protected override float GetCurrentValueForCapture()
        {
            if (m_HasTransformToScale)
            {
                m_InitialScale = m_TransformToScale.localScale;
            }
            return 1f;
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float newValue)
        {
            if (m_HasTransformToScale)
            {
                m_TransformToScale.localScale = m_InitialScale * newValue;
            }
            base.OnAffordanceValueUpdated(newValue);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        void OnValidate()
        {
            if (m_TransformToScale == null)
                m_TransformToScale = transform;
        }
    }
}