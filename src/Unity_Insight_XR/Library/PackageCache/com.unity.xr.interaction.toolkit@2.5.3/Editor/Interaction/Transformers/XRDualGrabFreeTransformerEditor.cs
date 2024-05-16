using Unity.XR.CoreUtils;
using UnityEditor.XR.Interaction.Toolkit.Utilities.Internal;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace UnityEditor.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Custom editor for an <see cref="XRDualGrabFreeTransformer"/>.
    /// </summary>
    [CustomEditor(typeof(XRDualGrabFreeTransformer), true), CanEditMultipleObjects]
    class XRDualGrabFreeTransformerEditor : BaseInteractionEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRDualGrabFreeTransformer.multiSelectPosition"/>.</summary>
        protected SerializedProperty m_MultiSelectPosition;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRDualGrabFreeTransformer.multiSelectRotation"/>.</summary>
        protected SerializedProperty m_MultiSelectRotation;

        static bool s_DrawInteractorAttachHandles = true;
        static bool s_DrawInteractableAttachHandles;
        static bool s_DrawBlendedHandle = true;

        // Smaller than the size of the regular PositionHandle to distinguish it even better
        // ReSharper disable InconsistentNaming -- Treat as const, private so no need to worry about stale references
        static readonly Vector3 k_BlendedAxisSize = new Vector3(0.5f, 0.5f, 0.5f);
        static readonly Vector3 k_AttachAxisSize = new Vector3(0.4f, 0.4f, 0.4f);

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRDualGrabFreeTransformer.multiSelectPosition"/>.</summary>
            public static readonly GUIContent multiSelectPosition = EditorGUIUtility.TrTextContent("Multi Select Position", "Where the object will snap to when multiple interactors are selecting it. Average is the center point between both interactors.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRDualGrabFreeTransformer.multiSelectRotation"/>.</summary>
            public static readonly GUIContent multiSelectRotation = EditorGUIUtility.TrTextContent("Multi Select Rotation", "Which hand will be used to create a positional offset from the object to rotate around. Average does not offset the object at all.");
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_MultiSelectPosition = serializedObject.FindProperty("m_MultiSelectPosition");
            m_MultiSelectRotation = serializedObject.FindProperty("m_MultiSelectRotation");
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
            EditorGUILayout.PropertyField(m_MultiSelectPosition, Contents.multiSelectPosition);
            EditorGUILayout.PropertyField(m_MultiSelectRotation, Contents.multiSelectRotation);
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        static void DrawGizmos(XRDualGrabFreeTransformer transformer, GizmoType gizmoType)
        {
            // The Pose will be default all zeros when never grabbed.
            // Important to use Quaternion.Equals instead of operator== since the former does not work when it's
            // an invalid default Quaternion, whereas the Equals method checks that each float component is equal.
            if (!Application.isPlaying || !transformer.isActiveAndEnabled || transformer.lastInteractorAttachPose.rotation.Equals(default))
                return;

            var grabInteractable = transformer.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null || grabInteractable.interactorsSelecting.Count <= 1)
                return;

            if (grabInteractable.interactorsSelecting.Count == 2)
            {
                var firstPosition = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).position;
                var secondPosition = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable).position;
                Handles.DrawDottedLine(firstPosition, secondPosition, 3f);
            }

            var index = 0;
            foreach (var interactor in grabInteractable.interactorsSelecting)
            {
                Pose pose;

                // Interactor's Attach Transform
                if (s_DrawInteractorAttachHandles)
                {
                    pose = interactor.GetAttachTransform(grabInteractable).GetWorldPose();
                    PositionHandleUtility.DrawLineOnlyPositionHandle(pose.position, pose.rotation, k_AttachAxisSize);
                    Handles.Label(pose.position, $"I{index}");
                }

                // Interactable's Attach Transform
                if (s_DrawInteractableAttachHandles)
                {
                    pose = grabInteractable.GetAttachTransform(interactor).GetWorldPose();
                    PositionHandleUtility.DrawLineOnlyPositionHandle(pose.position, pose.rotation, k_AttachAxisSize);
                    Handles.Label(pose.position, $"G{index}");
                }

                ++index;
            }

            if (s_DrawBlendedHandle)
            {
                PositionHandleUtility.DrawLineOnlyPositionHandle(transformer.lastInteractorAttachPose.position, transformer.lastInteractorAttachPose.rotation, k_BlendedAxisSize);
                Handles.Label(transformer.lastInteractorAttachPose.position, "B");
            }
        }

        void OnSceneGUI()
        {
            if (!Application.isPlaying)
                return;

            var transformer = (XRDualGrabFreeTransformer)target;
            if (!transformer.isActiveAndEnabled || transformer.lastInteractorAttachPose.rotation.Equals(default))
                return;

            var grabInteractable = transformer.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null || grabInteractable.interactorsSelecting.Count <= 1)
                return;

            Handles.BeginGUI();

            GUILayout.Label("XR Dual Grab Free Transformer");
            s_DrawInteractorAttachHandles = GUILayout.Toggle(s_DrawInteractorAttachHandles, "Draw Interactor Handles");
            s_DrawInteractableAttachHandles = GUILayout.Toggle(s_DrawInteractableAttachHandles, "Draw Interactable Handles");
            s_DrawBlendedHandle = GUILayout.Toggle(s_DrawBlendedHandle, "Draw Blended Interactor Handle");

            Handles.EndGUI();
        }
    }
}
