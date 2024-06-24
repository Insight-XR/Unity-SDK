using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for a <see cref="ClimbInteractable"/>.
    /// </summary>
    [CustomEditor(typeof(ClimbInteractable)), CanEditMultipleObjects]
    public class ClimbInteractableEditor : XRBaseInteractableEditor
    {
        const string k_ClimbConfigurationExpandedKey = "XRI." + nameof(ClimbInteractableEditor) + ".ClimbConfigurationExpanded";

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ClimbInteractable.climbProvider"/>.</summary>
        protected SerializedProperty m_ClimbProvider;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ClimbInteractable.climbTransform"/>.</summary>
        protected SerializedProperty m_ClimbTransform;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ClimbInteractable.filterInteractionByDistance"/>.</summary>
        protected SerializedProperty m_FilterInteractionByDistance;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ClimbInteractable.maxInteractionDistance"/>.</summary>
        protected SerializedProperty m_MaxInteractionDistance;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ClimbInteractable.climbSettingsOverride"/>.</summary>
        protected SerializedProperty m_ClimbSettingsOverride;

        bool m_ClimbConfigurationExpanded;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class ClimbContents
        {
            /// <summary>The climb configuration foldout.</summary>
            public static readonly GUIContent climbConfiguration = EditorGUIUtility.TrTextContent("Climb Configuration", "Settings for climb interactions.");
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            m_ClimbProvider = serializedObject.FindProperty("m_ClimbProvider");
            m_ClimbTransform = serializedObject.FindProperty("m_ClimbTransform");
            m_FilterInteractionByDistance = serializedObject.FindProperty("m_FilterInteractionByDistance");
            m_MaxInteractionDistance = serializedObject.FindProperty("m_MaxInteractionDistance");
            m_ClimbSettingsOverride = serializedObject.FindProperty("m_ClimbSettingsOverride");

            m_ClimbConfigurationExpanded = SessionState.GetBool(k_ClimbConfigurationExpandedKey, true);
        }

        /// <inheritdoc />
        protected override void DrawProperties()
        {
            base.DrawProperties();

            EditorGUILayout.Space();

            DrawClimbConfiguration();
        }

        /// <summary>
        /// Draw the Climb Configuration foldout.
        /// </summary>
        /// <seealso cref="DrawClimbConfigurationNested"/>
        protected virtual void DrawClimbConfiguration()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_ClimbConfigurationExpanded = EditorGUILayout.Foldout(m_ClimbConfigurationExpanded, ClimbContents.climbConfiguration, true);
                if (check.changed)
                    SessionState.SetBool(k_ClimbConfigurationExpandedKey, m_ClimbConfigurationExpanded);
            }

            if (!m_ClimbConfigurationExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                DrawClimbConfigurationNested();
            }
        }

        /// <summary>
        /// Draw the nested contents of the Climb Configuration foldout.
        /// </summary>
        /// <seealso cref="DrawClimbConfiguration"/>
        protected virtual void DrawClimbConfigurationNested()
        {
            EditorGUILayout.PropertyField(m_ClimbProvider);
            EditorGUILayout.PropertyField(m_ClimbTransform);
            EditorGUILayout.PropertyField(m_FilterInteractionByDistance);
            if (m_FilterInteractionByDistance.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_MaxInteractionDistance);
                }
            }

            EditorGUILayout.PropertyField(m_ClimbSettingsOverride);
        }
    }
}