using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRInteractorReticleVisual"/>.
    /// </summary>
    [CustomEditor(typeof(XRInteractorReticleVisual), true), CanEditMultipleObjects]
    public class XRInteractorReticleVisualEditor : BaseInteractionEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.maxRaycastDistance"/>.</summary>
        protected SerializedProperty m_MaxRaycastDistance;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.reticlePrefab"/>.</summary>
        protected SerializedProperty m_ReticlePrefab;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.prefabScalingFactor"/>.</summary>
        protected SerializedProperty m_PrefabScalingFactor;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.undoDistanceScaling"/>.</summary>
        protected SerializedProperty m_UndoDistanceScaling;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.alignPrefabWithSurfaceNormal"/>.</summary>
        protected SerializedProperty m_AlignPrefabWithSurfaceNormal;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.endpointSmoothingTime"/>.</summary>
        protected SerializedProperty m_EndpointSmoothingTime;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.drawWhileSelecting"/>.</summary>
        protected SerializedProperty m_DrawWhileSelecting;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorReticleVisual.raycastMask"/>.</summary>
        protected SerializedProperty m_RaycastMask;

        readonly List<Collider> m_ReticleColliders = new List<Collider>();
        bool m_ReticleCheckInitialized;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.maxRaycastDistance"/>.</summary>
            public static readonly GUIContent maxRaycastDistance = EditorGUIUtility.TrTextContent("Max Raycast Distance", "The max distance to ray cast from this Interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.reticlePrefab"/>.</summary>
            public static readonly GUIContent reticlePrefab = EditorGUIUtility.TrTextContent("Reticle Prefab", "Prefab which Unity draws over ray cast destination.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.prefabScalingFactor"/>.</summary>
            public static readonly GUIContent prefabScalingFactor = EditorGUIUtility.TrTextContent("Prefab Scaling Factor", "Amount to scale Prefab (before applying distance scaling).");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.undoDistanceScaling"/>.</summary>
            public static readonly GUIContent undoDistanceScaling = EditorGUIUtility.TrTextContent("Undo Distance Scaling", "Enable to have Unity undo the apparent scale of the Prefab by distance.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.alignPrefabWithSurfaceNormal"/>.</summary>
            public static readonly GUIContent alignPrefabWithSurfaceNormal = EditorGUIUtility.TrTextContent("Align Prefab With Surface Normal", "Enable to have Unity align the Prefab to the ray cast surface normal.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.endpointSmoothingTime"/>.</summary>
            public static readonly GUIContent endpointSmoothingTime = EditorGUIUtility.TrTextContent("Endpoint Smoothing Time", "Smoothing time for endpoint.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.drawWhileSelecting"/>.</summary>
            public static readonly GUIContent drawWhileSelecting = EditorGUIUtility.TrTextContent("Draw While Selecting", "Enable to have Unity draw the Reticle Prefab while selecting an Interactable.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorReticleVisual.raycastMask"/>.</summary>
            public static readonly GUIContent raycastMask = EditorGUIUtility.TrTextContent("Raycast Mask", "Layer mask for ray cast.");

            /// <summary>The help box message when the Reticle has a Collider that will disrupt the ray cast.</summary>
            public static readonly GUIContent reticleColliderWarning = EditorGUIUtility.TrTextContent("Reticle Prefab has a Collider which may disrupt the ray cast. Remove or disable the Collider component on the Reticle or adjust the Raycast Mask/Collider Layer.");
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_MaxRaycastDistance = serializedObject.FindProperty("m_MaxRaycastDistance");
            m_ReticlePrefab = serializedObject.FindProperty("m_ReticlePrefab");
            m_PrefabScalingFactor = serializedObject.FindProperty("m_PrefabScalingFactor");
            m_UndoDistanceScaling = serializedObject.FindProperty("m_UndoDistanceScaling");
            m_AlignPrefabWithSurfaceNormal = serializedObject.FindProperty("m_AlignPrefabWithSurfaceNormal");
            m_EndpointSmoothingTime = serializedObject.FindProperty("m_EndpointSmoothingTime");
            m_DrawWhileSelecting = serializedObject.FindProperty("m_DrawWhileSelecting");
            m_RaycastMask = serializedObject.FindProperty("m_RaycastMask");

            m_ReticleCheckInitialized = false;
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
            EditorGUILayout.PropertyField(m_MaxRaycastDistance, Contents.maxRaycastDistance);
            DrawReticle();
            EditorGUILayout.PropertyField(m_PrefabScalingFactor, Contents.prefabScalingFactor);
            EditorGUILayout.PropertyField(m_UndoDistanceScaling, Contents.undoDistanceScaling);
            EditorGUILayout.PropertyField(m_AlignPrefabWithSurfaceNormal, Contents.alignPrefabWithSurfaceNormal);
            EditorGUILayout.PropertyField(m_EndpointSmoothingTime, Contents.endpointSmoothingTime);
            EditorGUILayout.PropertyField(m_DrawWhileSelecting, Contents.drawWhileSelecting);
            EditorGUILayout.PropertyField(m_RaycastMask, Contents.raycastMask);
        }

        /// <summary>
        /// Draw property fields related to the reticle.
        /// </summary>
        protected virtual void DrawReticle()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(m_ReticlePrefab, Contents.reticlePrefab);

                // Show a warning if the reticle GameObject has a Collider, which would cause
                // a feedback loop issue with the raycast hitting the reticle.
                if (!serializedObject.isEditingMultipleObjects && m_ReticlePrefab.objectReferenceValue != null)
                {
                    // Get the list of Colliders on the reticle, only doing so when the reticle property changed
                    // or if this is the first time here in order to reduce the cost of evaluating this warning.
                    if (check.changed || !m_ReticleCheckInitialized)
                    {
                        var reticle = (GameObject)m_ReticlePrefab.objectReferenceValue;
                        reticle.GetComponentsInChildren(m_ReticleColliders);

                        m_ReticleCheckInitialized = true;
                    }

                    if (m_ReticleColliders.Count > 0)
                    {
                        // Allow the Collider as long as the Raycast Mask is set to ignore it
                        var raycastMask = m_RaycastMask.intValue;
                        foreach (var collider in m_ReticleColliders)
                        {
                            if (collider != null && collider.enabled && (raycastMask & (1 << collider.gameObject.layer)) != 0)
                            {
                                EditorGUILayout.HelpBox(Contents.reticleColliderWarning.text, MessageType.Warning, true);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
