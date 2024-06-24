using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering
{
    /// <summary>
    /// Base class for renderer bridge components that abstract the work of setting up material instances or property blocks. 
    /// </summary>
    public abstract class MaterialHelperBase : MonoBehaviour
    {
        [SerializeField]
        Renderer m_Renderer;

        /// <summary>
        /// The renderer to set material parameter overrides on.
        /// </summary>
        /// <remarks>
        /// Changing this value after being initialized is not supported.
        /// </remarks>
        public Renderer rendererTarget
        {
            get => m_Renderer;
            set => m_Renderer = value;
        }

        [SerializeField]
        int m_MaterialIndex;

        /// <summary>
        /// The index of the material you want to set the parameters of.
        /// </summary>
        /// <remarks>
        /// Changing this value after being initialized is not supported.
        /// </remarks>
        public int materialIndex
        {
            get => m_MaterialIndex;
            set => m_MaterialIndex = value;
        }

        /// <summary>
        /// Whether <see cref="Initialize"/> has been called. The component is automatically initialized during <c>OnEnable</c>.
        /// </summary>
        /// <seealso cref="Initialize"/>
        protected bool isInitialized { get; private set; }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            // If we've already initialized with a valid material instance, then do nothing.
            if (isInitialized)
                return;

            if (m_Renderer == null)
                m_Renderer = GetComponentInParent<Renderer>();

            if (m_Renderer == null)
            {
                XRLoggingUtils.LogError($"No renderer found on {this}. Disabling this material helper component.", this);
                enabled = false;
                return;
            }

            if (m_Renderer.sharedMaterials.Length == 0)
            {
                XRLoggingUtils.LogError($"Renderer found on {this} does not have any shared materials. Disabling this material helper component.", this);
                enabled = false;
                return;
            }

            if (m_MaterialIndex > m_Renderer.sharedMaterials.Length)
            {
                XRLoggingUtils.LogWarning($"Insufficient number of materials set on associated render for {this}." +
                    " Setting target material index to 0.", this);
                m_MaterialIndex = 0;
                return;
            }

            Initialize();
        }

        /// <summary>
        /// Initialize the property block or material instance.
        /// </summary>
        protected virtual void Initialize()
        {
            isInitialized = true;
        }

        /// <summary>
        /// Returns the <see cref="Material"/> for the <see cref="rendererTarget"/> located in array location <see cref="materialIndex"/>
        /// </summary>
        /// <returns>A <see cref="Material"/> from the current <see cref="rendererTarget"/></returns>
        public Material GetSharedMaterialForTarget()
        {
            return m_Renderer.sharedMaterials[materialIndex];
        }
    }
}
