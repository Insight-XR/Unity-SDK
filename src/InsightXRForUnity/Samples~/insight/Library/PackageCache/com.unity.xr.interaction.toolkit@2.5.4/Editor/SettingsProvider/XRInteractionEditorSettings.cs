using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Editor;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Settings class for XR Interaction Toolkit editor values.
    /// </summary>
    [ScriptableSettingsPath(ProjectPath.k_XRInteractionSettingsFolder)]
    class XRInteractionEditorSettings : EditorScriptableSettings<XRInteractionEditorSettings>
    {
        [SerializeField]
        bool m_InteractionLayerUpdaterShown = true;

        /// <summary>
        /// Gets whether the updater dialog option was shown for users.
        /// </summary>
        /// <returns>Returns whether the dialog option to update Interaction Layers was shown.</returns>
        internal bool interactionLayerUpdaterShown
        {
            get => m_InteractionLayerUpdaterShown;
            set => m_InteractionLayerUpdaterShown = value;
        }
        
        [SerializeField]
        bool m_ShowOldInteractionLayerMaskInInspector;
        
        /// <summary>
        /// Gets whether the deprecated physics Layer property <c>m_InteractionLayerMask</c> should be shown in the Inspector window.
        /// </summary>
        /// <returns>Returns whether the old <c>m_InteractionLayerMask</c> property should be shown in the Inspector window.</returns>
        internal bool showOldInteractionLayerMaskInInspector
        {
            get => m_ShowOldInteractionLayerMaskInInspector;
            set => m_ShowOldInteractionLayerMaskInInspector = value;
        }
    }
}
