using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

namespace UnityEditor.XR.Interaction.Toolkit.AR
{
    /// <summary>
    /// Custom editor for an <see cref="ARGestureInteractor"/>.
    /// </summary>
    [CustomEditor(typeof(ARGestureInteractor), true), CanEditMultipleObjects]
    public class ARGestureInteractorEditor : XRBaseInteractorEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ARGestureInteractor.xrOrigin"/>.</summary>
        protected SerializedProperty m_XROrigin;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ARGestureInteractor.arSessionOrigin"/>.</summary>
        protected SerializedProperty m_ARSessionOrigin;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ARGestureInteractor.raycastMask"/>.</summary>
        protected SerializedProperty m_RaycastMask;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ARGestureInteractor.raycastTriggerInteraction"/>.</summary>
        protected SerializedProperty m_RaycastTriggerInteraction;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="ARGestureInteractor.xrOrigin"/>.</summary>
            public static readonly GUIContent xrOrigin = EditorGUIUtility.TrTextContent("XR Origin", "The XR Origin that this Interactor will use (such as to get the Camera or to transform from Session space). Will find one if None.");
            /// <summary><see cref="GUIContent"/> for <see cref="ARGestureInteractor.arSessionOrigin"/>.</summary>
            public static readonly GUIContent arSessionOrigin = EditorGUIUtility.TrTextContent("AR Session Origin", "(Deprecated) The AR Session Origin that this Interactor will use (such as to get the Camera or to transform from Session space). Will find one if None.");
            /// <summary><see cref="GUIContent"/> for <see cref="ARGestureInteractor.raycastMask"/>.</summary>
            public static readonly GUIContent raycastMask = EditorGUIUtility.TrTextContent("Raycast Mask", "The layer mask used for limiting ray cast targets.");
            /// <summary><see cref="GUIContent"/> for <see cref="ARGestureInteractor.raycastTriggerInteraction"/>.</summary>
            public static readonly GUIContent raycastTriggerInteraction = EditorGUIUtility.TrTextContent("Raycast Trigger Interaction", "The type of interaction with trigger colliders via ray cast.");
            
            /// <summary>The help box message when AR Session Origin is used.</summary>
            public static readonly GUIContent arSessionOriginDeprecated = EditorGUIUtility.TrTextContent("AR Session Origin has been deprecated. Use the XR Origin component instead.");
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            m_XROrigin = serializedObject.FindProperty("m_XROrigin");
            m_ARSessionOrigin = serializedObject.FindProperty("m_ARSessionOrigin");
            m_RaycastMask = serializedObject.FindProperty("m_RaycastMask");
            m_RaycastTriggerInteraction = serializedObject.FindProperty("m_RaycastTriggerInteraction");
        }

        /// <inheritdoc />
        protected override void DrawProperties()
        {
            base.DrawProperties();
#if AR_FOUNDATION_5_0_OR_NEWER
            EditorGUILayout.PropertyField(m_XROrigin, Contents.xrOrigin);
            using (new EditorGUI.IndentLevelScope())
            {
                if (m_ARSessionOrigin.objectReferenceValue != null)
                    EditorGUILayout.HelpBox(Contents.arSessionOriginDeprecated.text, MessageType.Warning);

                EditorGUILayout.PropertyField(m_ARSessionOrigin, Contents.arSessionOrigin);
            }
#else
            EditorGUILayout.PropertyField(m_ARSessionOrigin, Contents.arSessionOrigin);
#endif

            EditorGUILayout.PropertyField(m_RaycastMask, Contents.raycastMask);
            EditorGUILayout.PropertyField(m_RaycastTriggerInteraction, Contents.raycastTriggerInteraction);
        }
    }
}
