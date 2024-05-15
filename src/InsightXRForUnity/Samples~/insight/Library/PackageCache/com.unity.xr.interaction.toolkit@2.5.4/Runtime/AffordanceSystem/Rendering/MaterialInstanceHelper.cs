namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering
{
    /// <summary>
    /// Creates material instance for a material associated with a given renderer material index and provide accessor to it.
    /// </summary>
    [AddComponentMenu("Affordance System/Rendering/Material Instance Helper", 12)]
    [HelpURL(XRHelpURLConstants.k_MaterialInstanceHelper)]
    public class MaterialInstanceHelper : MaterialHelperBase
    {
        Material m_MaterialInstance;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            if (m_MaterialInstance != null)
            {
                Destroy(m_MaterialInstance);
                m_MaterialInstance = null;
            }
        }

        /// <summary>
        /// Try to get initialized material instance as configured on the component.
        /// </summary>
        /// <param name="materialInstance">Material instance. Will be <see langword="null"/> if invalid.</param>
        /// <returns>Returns <see langword="true"/> if material instance is initialized. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryGetMaterialInstance(out Material materialInstance)
        {
            if (!isInitialized)
            {
                materialInstance = null;
                return false;
            }

            materialInstance = m_MaterialInstance;
            return true;
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            if (m_MaterialInstance == null)
            {
                var sharedMaterials = rendererTarget.sharedMaterials;
                m_MaterialInstance = new Material(sharedMaterials[materialIndex]);
                sharedMaterials[materialIndex] = m_MaterialInstance;
                rendererTarget.sharedMaterials = sharedMaterials;
                base.Initialize();
            }
        }
    }
}
