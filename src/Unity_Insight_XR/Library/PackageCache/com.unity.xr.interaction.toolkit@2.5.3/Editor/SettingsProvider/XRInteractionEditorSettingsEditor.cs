using UnityEngine;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Editor inspector for <see cref="XRInteractionEditorSettings"/>.
    /// </summary>
    [CustomEditor(typeof(XRInteractionEditorSettings))]
    class XRInteractionEditorSettingsEditor : Editor
    {
        const float k_LabelsWidth = 270f;

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractionEditorSettings.showOldInteractionLayerMaskInInspector"/>.</summary>
        SerializedProperty m_ShowOldInteractionLayerMaskInInspector;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractionEditorSettings.showOldInteractionLayerMaskInInspector"/>.</summary>
            public static readonly GUIContent showOldInteractionLayerMaskInInspector =
                EditorGUIUtility.TrTextContent("Show Old Layer Mask In Inspector",
                    "Enable this to show the \'Deprecated Interaction Layer Mask\' property in the Inspector window.");
        }

        void OnEnable()
        {
            m_ShowOldInteractionLayerMaskInInspector = serializedObject.FindProperty(nameof(m_ShowOldInteractionLayerMaskInInspector));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = k_LabelsWidth;
                EditorGUILayout.PropertyField(m_ShowOldInteractionLayerMaskInInspector, Contents.showOldInteractionLayerMaskInInspector);
                EditorGUIUtility.labelWidth = labelWidth;

                if (check.changed)
                {
                    Repaint();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
