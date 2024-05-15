using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    /// <summary>
    /// This class is a convenience class to handle interactor link between a <see cref="IUIInteractor"/>
    /// and an <see cref="XRUIInputModule"/>.
    /// </summary>
    class RegisteredUIInteractorCache
    {
        XRUIInputModule m_InputModule;
        XRUIInputModule m_RegisteredInputModule;
        readonly IUIInteractor m_UiInteractor;
        readonly XRBaseInteractor m_BaseInteractor;
        
        /// <summary>
        /// Initializes and returns an instance of <see cref="RegisteredUIInteractorCache"/>.
        /// </summary>
        /// <param name="uiInteractor">This is the interactor that will be registered with the UI Input Module.</param>
        public RegisteredUIInteractorCache(IUIInteractor uiInteractor)
        {
            // This constructor only requires the IUIInteractor reference
            // as only one XRUIInputModule may be present at one time.
            m_UiInteractor = uiInteractor;
            m_BaseInteractor = uiInteractor as XRBaseInteractor;
        }
        
        /// <summary>
        /// Register with or unregister from the Input Module (if necessary).
        /// </summary>
        /// <remarks>
        /// If this behavior is not active and enabled, this function does nothing.
        /// </remarks>
        public void RegisterOrUnregisterXRUIInputModule(bool enabled)
        {
            if (!Application.isPlaying || (m_BaseInteractor != null && !m_BaseInteractor.isActiveAndEnabled))
                return;
            
            if (enabled)
                RegisterWithXRUIInputModule();
            else
                UnregisterFromXRUIInputModule();
        }

        /// <summary>
        /// Register with the <see cref="XRUIInputModule"/> (if necessary).
        /// </summary>
        /// <seealso cref="UnregisterFromXRUIInputModule"/>
        public void RegisterWithXRUIInputModule()
        {
            if (m_InputModule == null)
                FindOrCreateXRUIInputModule();

            if (m_RegisteredInputModule == m_InputModule)
                return;

            UnregisterFromXRUIInputModule();

            m_InputModule.RegisterInteractor(m_UiInteractor);
            m_RegisteredInputModule = m_InputModule;
        }

        /// <summary>
        /// Unregister from the <see cref="XRUIInputModule"/> (if necessary).
        /// </summary>
        /// <seealso cref="RegisterWithXRUIInputModule"/>
        public void UnregisterFromXRUIInputModule()
        {
            if (m_RegisteredInputModule != null)
                m_RegisteredInputModule.UnregisterInteractor(m_UiInteractor);

            m_RegisteredInputModule = null;
        }
        
        void FindOrCreateXRUIInputModule()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                if (ComponentLocatorUtility<EventSystem>.TryFindComponent(out eventSystem))
                {
                    // Remove the Standalone Input Module if already implemented, since it will block the XRUIInputModule
                    if (eventSystem.TryGetComponent<StandaloneInputModule>(out var standaloneInputModule))
                        Object.Destroy(standaloneInputModule);
                }
                else
                {
                    eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
                }
            }

            if (!eventSystem.TryGetComponent(out m_InputModule))
                m_InputModule = eventSystem.gameObject.AddComponent<XRUIInputModule>();
        }

        /// <summary>
        /// Attempts to retrieve the current UI Model.
        /// </summary>
        /// <param name="model">The returned model that reflects the UI state of this Interactor.</param>
        /// <returns>Returns <see langword="true"/> if the model was able to retrieved. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryGetUIModel(out TrackedDeviceModel model)
        {
            if (m_InputModule != null)
            {
                return m_InputModule.GetTrackedDeviceModel(m_UiInteractor, out model);
            }

            model = TrackedDeviceModel.invalid;
            return false;
        }

        /// <summary>
        /// Use this to determine if the ray is currently hovering over a UI GameObject.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if hovering over a UI element. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="UIInputModule.IsPointerOverGameObject(int)"/>
        /// <seealso cref="EventSystem.IsPointerOverGameObject(int)"/>
        public bool IsOverUIGameObject()
        {
            return (m_InputModule != null && TryGetUIModel(out var uiModel) && m_InputModule.IsPointerOverGameObject(uiModel.pointerId));
        }
    }
}