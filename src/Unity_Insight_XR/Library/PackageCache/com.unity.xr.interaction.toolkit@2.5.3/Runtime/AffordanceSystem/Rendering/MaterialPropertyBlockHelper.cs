namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering
{
    /// <summary>
    /// Bridge between components needing to write to a renderer's material property block.
    /// Allows multiple components to handle their own material properties and write them to a central space and update it only as needed.
    /// </summary>
    [AddComponentMenu("Affordance System/Rendering/Material Property Block Helper", 12)]
    [HelpURL(XRHelpURLConstants.k_MaterialPropertyBlockHelper)]
    public class MaterialPropertyBlockHelper : MaterialHelperBase
    {
        MaterialPropertyBlock m_PropertyBlock;
        bool m_IsDirty;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            m_PropertyBlock = null;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void LateUpdate()
        {
            if (!m_IsDirty || !isInitialized)
                return;

            rendererTarget.SetPropertyBlock(m_PropertyBlock, materialIndex);
            m_IsDirty = false;
        }

        /// <summary>
        /// Get material property block associated the the material index set in the inspector.
        /// </summary>
        /// <param name="markPropertyBlockAsDirty">If true, marks property block as dirty to be updated at the end of the frame.</param>
        /// <returns>Material property block for associated material index.</returns>
        public MaterialPropertyBlock GetMaterialPropertyBlock(bool markPropertyBlockAsDirty = true)
        {
            if (markPropertyBlockAsDirty)
                m_IsDirty = true;
            return m_PropertyBlock;
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();
            m_PropertyBlock = new MaterialPropertyBlock();
            rendererTarget.GetPropertyBlock(m_PropertyBlock, materialIndex);
        }
    }
}
