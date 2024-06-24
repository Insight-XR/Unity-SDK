using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRInteractableSnapVolume"/>.
    /// </summary>
    [CustomEditor(typeof(XRInteractableSnapVolume), true), CanEditMultipleObjects]
    public class XRInteractableSnapVolumeEditor : BaseInteractionEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractableSnapVolume.interactionManager"/>.</summary>
        protected SerializedProperty m_InteractionManager;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractableSnapVolume.interactableObject"/>.</summary>
        protected SerializedProperty m_InteractableObject;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractableSnapVolume.snapCollider"/>.</summary>
        protected SerializedProperty m_SnapCollider;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractableSnapVolume.disableSnapColliderWhenSelected"/>.</summary>
        protected SerializedProperty m_DisableSnapColliderWhenSelected;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractableSnapVolume.snapToCollider"/>.</summary>
        protected SerializedProperty m_SnapToCollider;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractableSnapVolume.interactionManager"/>.</summary>
            public static readonly GUIContent interactionManager = EditorGUIUtility.TrTextContent("Interaction Manager", "The XR Interaction Manager that this snap volume will communicate with (will find one if None).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractableSnapVolume.interactableObject"/>.</summary>
            public static readonly GUIContent interactableObject = EditorGUIUtility.TrTextContent("Interactable Object", "The interactable associated with this XR Interactable Snap Volume. If not set, Unity will find it up the hierarchy.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractableSnapVolume.snapCollider"/>.</summary>
            public static readonly GUIContent snapCollider = EditorGUIUtility.TrTextContent("Snap Collider", "The trigger collider to associate with the interactable when it is hit/collided. Rays will snap from this to the Snap To Collider.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractableSnapVolume.disableSnapColliderWhenSelected"/>.</summary>
            public static readonly GUIContent disableSnapColliderWhenSelected = EditorGUIUtility.TrTextContent("Disable Snap Collider When Selected", "Automatically disable or enable the Snap Collider when the interactable is selected or deselected.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractableSnapVolume.snapToCollider"/>.</summary>
            public static readonly GUIContent snapToCollider = EditorGUIUtility.TrTextContent("Snap To Collider", "(Optional) The collider that will be used to find the closest point to snap to. If not set, the associated XR Interactable transform position will be used.");

            /// <summary>The help box message when <see cref="XRInteractableSnapVolume.snapCollider"/> is <see langword="null"/>.</summary>
            public static readonly GUIContent missingSnapCollider = EditorGUIUtility.TrTextContent("Missing required Snap Collider assignment, such as a Sphere Collider.");
            /// <summary>The help box message when <see cref="XRInteractableSnapVolume.snapCollider"/> is not a valid collider type.</summary>
            public static readonly GUIContent unsupportedColliderType = EditorGUIUtility.TrTextContent("Snap Collider must be a Box Collider, Sphere Collider, Capsule Collider, or convex Mesh Collider.");
            /// <summary>The help box message when <see cref="XRInteractableSnapVolume.snapCollider"/> has <see cref="Collider.isTrigger"/> <see langword="false"/>.</summary>
            public static readonly GUIContent colliderNotTrigger = EditorGUIUtility.TrTextContent("Snap Collider must have Is Trigger enabled. Unity will set it at runtime if not fixed.");
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_InteractionManager = serializedObject.FindProperty("m_InteractionManager");
            m_InteractableObject = serializedObject.FindProperty("m_InteractableObject");
            m_SnapCollider = serializedObject.FindProperty("m_SnapCollider");
            m_DisableSnapColliderWhenSelected = serializedObject.FindProperty("m_DisableSnapColliderWhenSelected");
            m_SnapToCollider = serializedObject.FindProperty("m_SnapToCollider");
        }

        /// <inheritdoc />
        /// <seealso cref="DrawBeforeProperties"/>
        /// <seealso cref="DrawProperties"/>
        /// <seealso cref="BaseInteractionEditor.DrawDerivedProperties"/>
        protected override void DrawInspector()
        {
            DrawBeforeProperties();
            DrawProperties();
            DrawDerivedProperties();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the section of the custom inspector before <see cref="DrawProperties"/>.
        /// By default, this draws the read-only Script property.
        /// </summary>
        protected virtual void DrawBeforeProperties()
        {
            DrawScript();
        }

        /// <summary>
        /// This method is automatically called by <see cref="DrawInspector"/> to
        /// draw the property fields. Override this method to customize the
        /// properties shown in the Inspector. This is typically the method overridden
        /// when a derived behavior adds additional serialized properties
        /// that should be displayed in the Inspector.
        /// </summary>
        protected virtual void DrawProperties()
        {
            var snapVolume = (XRInteractableSnapVolume)target;

            // There's no update loop to detect when the reference changed without this Editor calling the property itself,
            // so disable the control when the snap volume is already registered
            var isRegistered = Application.isPlaying && snapVolume.isActiveAndEnabled;

            using (new EditorGUI.DisabledScope(isRegistered))
            {
                EditorGUILayout.PropertyField(m_InteractionManager, Contents.interactionManager);
                EditorGUILayout.PropertyField(m_InteractableObject, Contents.interactableObject);
                EditorGUILayout.PropertyField(m_SnapCollider, Contents.snapCollider);
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_DisableSnapColliderWhenSelected, Contents.disableSnapColliderWhenSelected);
                }
            }

            if (m_SnapCollider.objectReferenceValue == null)
                EditorGUILayout.HelpBox(Contents.missingSnapCollider.text, MessageType.Warning, false);
            else if(!XRInteractableSnapVolume.SupportsTriggerCollider(m_SnapCollider.objectReferenceValue as Collider))
                EditorGUILayout.HelpBox(Contents.unsupportedColliderType.text, MessageType.Error, false);
            else if(!((Collider)m_SnapCollider.objectReferenceValue).isTrigger)
                EditorGUILayout.HelpBox(Contents.colliderNotTrigger.text, MessageType.Warning, false);

            EditorGUILayout.PropertyField(m_SnapToCollider, Contents.snapToCollider);
        }
    }
}
