using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering
{
    /// <summary>
    /// Affordance receiver for a Skinned Mesh Renderer with blend shapes.
    /// </summary>
    [AddComponentMenu("Affordance System/Receiver/Rendering/Blend Shape Affordance Receiver", 12)]
    [HelpURL(XRHelpURLConstants.k_BlendShapeAffordanceReceiver)]
    public class BlendShapeAffordanceReceiver : FloatAffordanceReceiver
    {
        [SerializeField]
        [Tooltip("Skinned Mesh Renderer to apply blend shapes animations to.")]
        SkinnedMeshRenderer m_SkinnedMeshRenderer;

        /// <summary>
        /// Skinned Mesh Renderer to apply blend shapes animations to.
        /// </summary>
        public SkinnedMeshRenderer skinnedMeshRenderer
        {
            get => m_SkinnedMeshRenderer;
            set => m_SkinnedMeshRenderer = value;
        }

        [SerializeField]
        [Tooltip("BlendShape index to animate.")]
        int m_BlendShapeIndex;

        /// <summary>
        /// BlendShape index to animate.
        /// </summary>
        public int blendShapeIndex
        {
            get => m_BlendShapeIndex;
            set => m_BlendShapeIndex = value;
        }

        /// <inheritdoc/>
        protected override void OnEnable()
        {
            if (m_SkinnedMeshRenderer == null)
            {
                XRLoggingUtils.LogError("Missing Skinned Mesh Renderer on " + this, this);
                enabled = false;
                return;
            }

            base.OnEnable();
        }

        /// <inheritdoc/>
        protected override void OnAffordanceValueUpdated(float newValue)
        {
            m_SkinnedMeshRenderer.SetBlendShapeWeight(m_BlendShapeIndex, newValue);
            base.OnAffordanceValueUpdated(newValue);
        }

        /// <inheritdoc/>
        protected override float GetCurrentValueForCapture()
        {
            return m_SkinnedMeshRenderer.GetBlendShapeWeight(m_BlendShapeIndex);
        }
    }
}