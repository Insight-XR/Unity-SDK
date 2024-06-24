using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRDirectInteractor"/>.
    /// </summary>
    [CustomEditor(typeof(XRDirectInteractor), true), CanEditMultipleObjects]
    public class XRDirectInteractorEditor : XRBaseControllerInteractorEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRDirectInteractor.improveAccuracyWithSphereCollider"/>.</summary>
        protected SerializedProperty m_ImproveAccuracyWithSphereCollider;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRDirectInteractor.physicsLayerMask"/>.</summary>
        protected SerializedProperty m_PhysicsLayerMask;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRDirectInteractor.physicsTriggerInteraction"/>.</summary>
        protected SerializedProperty m_PhysicsTriggerInteraction;

        /// <summary><see cref="GUIContent"/> for <see cref="XRDirectInteractor.improveAccuracyWithSphereCollider"/>.</summary>
        public static readonly GUIContent improveAccuracyWithSphereCollider = EditorGUIUtility.TrTextContent("Improve Accuracy With Sphere Collider", "Generates contacts using optimized sphere cast calls every frame instead of relying on contact events on Fixed Update. Disable to force the use of trigger events.");
        /// <summary><see cref="GUIContent"/> for <see cref="XRDirectInteractor.physicsLayerMask"/>.</summary>
        public static readonly GUIContent physicsLayerMask = EditorGUIUtility.TrTextContent("Physics Layer Mask", "Physics layer mask used for limiting direct interactor overlaps when using the Improve Accuracy With Sphere Collider option.");
        /// <summary><see cref="GUIContent"/> for <see cref="XRDirectInteractor.physicsTriggerInteraction"/>.</summary>
        public static readonly GUIContent physicsTriggerInteraction = EditorGUIUtility.TrTextContent("Physics Trigger Interaction", "Determines whether the direct interactor sphere overlap will hit triggers when using the Improve Accuracy With Sphere Collider option.");

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            m_ImproveAccuracyWithSphereCollider = serializedObject.FindProperty("m_ImproveAccuracyWithSphereCollider");
            m_PhysicsLayerMask = serializedObject.FindProperty("m_PhysicsLayerMask");
            m_PhysicsTriggerInteraction = serializedObject.FindProperty("m_PhysicsTriggerInteraction");
        }

        /// <inheritdoc />
        protected override void DrawProperties()
        {
            // Not calling base method to completely override drawn properties

            DrawInteractionManagement();
            DrawInteractionConfiguration();
            DrawSphereColliderConfiguration();

            EditorGUILayout.Space();

            DrawSelectionConfiguration();
        }

        /// <summary>
        /// Draw the property fields related to interaction configuration.
        /// </summary>
        protected virtual void DrawInteractionConfiguration()
        {
            EditorGUILayout.PropertyField(m_AttachTransform, BaseContents.attachTransform);
            EditorGUILayout.PropertyField(m_DisableVisualsWhenBlockedInGroup, BaseContents.disableVisualsWhenBlockedInGroup);
        }

        /// <summary>
        /// Draw property fields related to Sphere Collider accuracy and casting configuration.
        /// </summary>
        protected virtual void DrawSphereColliderConfiguration()
        {
            EditorGUILayout.PropertyField(m_ImproveAccuracyWithSphereCollider, improveAccuracyWithSphereCollider);
            if (m_ImproveAccuracyWithSphereCollider.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_PhysicsLayerMask, physicsLayerMask);
                    EditorGUILayout.PropertyField(m_PhysicsTriggerInteraction, physicsTriggerInteraction);
                }
            }
        }

        /// <summary>
        /// Draw the property fields related to selection configuration.
        /// </summary>
        protected virtual void DrawSelectionConfiguration()
        {
            DrawSelectActionTrigger();
            EditorGUILayout.PropertyField(m_KeepSelectedTargetValid, BaseContents.keepSelectedTargetValid);
            EditorGUILayout.PropertyField(m_HideControllerOnSelect, BaseControllerContents.hideControllerOnSelect);
            EditorGUILayout.PropertyField(m_AllowHoveredActivate, BaseControllerContents.allowHoveredActivate);
            EditorGUILayout.PropertyField(m_TargetPriorityMode, BaseControllerContents.targetPriorityMode);
            EditorGUILayout.PropertyField(m_StartingSelectedInteractable, BaseContents.startingSelectedInteractable);
        }
    }
}
