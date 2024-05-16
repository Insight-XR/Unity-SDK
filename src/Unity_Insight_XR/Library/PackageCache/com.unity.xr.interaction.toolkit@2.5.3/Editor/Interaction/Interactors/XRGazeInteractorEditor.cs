using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRGazeInteractor"/>.
    /// </summary>
    [CustomEditor(typeof(XRGazeInteractor), true), CanEditMultipleObjects]
    public class XRGazeInteractorEditor : XRRayInteractorEditor
    {
        const string k_GazeConfigurationExpandedKey = "XRI." + nameof(XRGazeInteractorEditor) + ".GazeConfigurationExpanded";

        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.gazeAssistanceSnapVolume"/>.</summary>
        protected SerializedProperty m_GazeAssistanceSnapVolume;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.gazeAssistanceCalculation"/>.</summary>
        protected SerializedProperty m_GazeAssistanceCalculation;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.gazeAssistanceColliderFixedSize"/>.</summary>
        protected SerializedProperty m_GazeAssistanceColliderFixedSize;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.gazeAssistanceColliderScale"/>.</summary>
        protected SerializedProperty m_GazeAssistanceColliderScale;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.gazeAssistanceDistanceScaling"/>.</summary>
        protected SerializedProperty m_GazeAssistanceDistanceScaling;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.clampGazeAssistanceDistanceScaling"/>.</summary>
        protected SerializedProperty m_ClampGazeAssistanceDistanceScaling;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRGazeInteractor.gazeAssistanceDistanceScalingClampValue"/>.</summary>
        protected SerializedProperty m_GazeAssistanceDistanceScalingClampValue;

        bool m_GazeConfigurationExpanded;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class GazeContents
        {
            /// <summary>The gaze configuration foldout.</summary>
            public static readonly GUIContent gazeConfiguration = EditorGUIUtility.TrTextContent("Gaze Configuration", "Settings for gaze interactions.");

            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.gazeAssistanceSnapVolume"/>.</summary>
            public static readonly GUIContent gazeAssistanceSnapVolume = EditorGUIUtility.TrTextContent("Gaze Assistance Snap Volume", "The snap volume to place where this interactor hits a valid target for gaze assistance. If not set, Unity will create one by default.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.gazeAssistanceCalculation"/>.</summary>
            public static readonly GUIContent gazeAssistanceCalculation = EditorGUIUtility.TrTextContent("Gaze Assistance Calculation", "Defines the way the gaze assistance calculates and sizes the assistance area.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.gazeAssistanceColliderFixedSize"/>.</summary>
            public static readonly GUIContent gazeAssistanceColliderFixedSize = EditorGUIUtility.TrTextContent("Gaze Assistance Fixed Size", "The radius of the assistance collider when Gaze Assistance Calculation is Fixed Size.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.gazeAssistanceColliderScale"/>.</summary>
            public static readonly GUIContent gazeAssistanceColliderScale = EditorGUIUtility.TrTextContent("Gaze Assistance Collider Scale", "The radius of the assistance collider when Gaze Assistance Calculation is Fixed Size or Collider Size.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.gazeAssistanceDistanceScaling"/>.</summary>
            public static readonly GUIContent gazeAssistanceDistanceScaling = EditorGUIUtility.TrTextContent("Gaze Assistance Distance Scaling", "If true, the assistance collider will scale based on distance from the interactable.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.clampGazeAssistanceDistanceScaling"/>.</summary>
            public static readonly GUIContent clampGazeAssistanceDistanceScaling = EditorGUIUtility.TrTextContent("Clamp Distance Scaling", "If true, the assistance collider scale will be clamped to the distance scaling clamp value.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRGazeInteractor.gazeAssistanceDistanceScalingClampValue"/>.</summary>
            public static readonly GUIContent gazeAssistanceDistanceScalingClampValue = EditorGUIUtility.TrTextContent("Distance Scaling Clamp Value", "The value the assistance collider scale will be clamped to.");
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            m_GazeAssistanceSnapVolume = serializedObject.FindProperty("m_GazeAssistanceSnapVolume");
            m_GazeAssistanceCalculation = serializedObject.FindProperty("m_GazeAssistanceCalculation");
            m_GazeAssistanceColliderFixedSize = serializedObject.FindProperty("m_GazeAssistanceColliderFixedSize");
            m_GazeAssistanceColliderScale = serializedObject.FindProperty("m_GazeAssistanceColliderScale");
            m_GazeAssistanceDistanceScaling = serializedObject.FindProperty("m_GazeAssistanceDistanceScaling");
            m_ClampGazeAssistanceDistanceScaling = serializedObject.FindProperty("m_ClampGazeAssistanceDistanceScaling");
            m_GazeAssistanceDistanceScalingClampValue = serializedObject.FindProperty("m_GazeAssistanceDistanceScalingClampValue");

            m_GazeConfigurationExpanded = SessionState.GetBool(k_GazeConfigurationExpandedKey, false);
        }

        /// <inheritdoc />
        protected override void DrawProperties()
        {
            base.DrawProperties();
            
            EditorGUILayout.Space();
            
            DrawGazeConfiguration();
        }

        /// <summary>
        /// Draw the property fields related to gaze configuration.
        /// </summary>
        protected virtual void DrawGazeConfiguration()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_GazeConfigurationExpanded = EditorGUILayout.Foldout(m_GazeConfigurationExpanded, GazeContents.gazeConfiguration, true);
                if (check.changed)
                    SessionState.SetBool(k_GazeConfigurationExpandedKey, m_GazeConfigurationExpanded);
            }

            if (!m_GazeConfigurationExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_GazeAssistanceSnapVolume, GazeContents.gazeAssistanceSnapVolume);
                EditorGUILayout.PropertyField(m_GazeAssistanceCalculation, GazeContents.gazeAssistanceCalculation);
                if (m_GazeAssistanceCalculation.intValue == (int)XRGazeInteractor.GazeAssistanceCalculation.FixedSize)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(m_GazeAssistanceColliderFixedSize, GazeContents.gazeAssistanceColliderFixedSize);
                    }
                }

                EditorGUILayout.PropertyField(m_GazeAssistanceColliderScale, GazeContents.gazeAssistanceColliderScale);
                EditorGUILayout.PropertyField(m_GazeAssistanceDistanceScaling, GazeContents.gazeAssistanceDistanceScaling);
                if (m_GazeAssistanceDistanceScaling.boolValue)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.PropertyField(m_ClampGazeAssistanceDistanceScaling, GazeContents.clampGazeAssistanceDistanceScaling);
                        if (m_ClampGazeAssistanceDistanceScaling.boolValue)
                        {
                            using (new EditorGUI.IndentLevelScope())
                            {
                                EditorGUILayout.PropertyField(m_GazeAssistanceDistanceScalingClampValue, GazeContents.gazeAssistanceDistanceScalingClampValue);
                            }
                        }
                    }
                }
            }
        }
    }
}