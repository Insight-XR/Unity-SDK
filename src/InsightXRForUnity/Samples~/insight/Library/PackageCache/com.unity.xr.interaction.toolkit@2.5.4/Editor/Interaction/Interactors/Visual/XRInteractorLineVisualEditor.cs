using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="XRInteractorLineVisual"/>.
    /// </summary>
    [CustomEditor(typeof(XRInteractorLineVisual), true), CanEditMultipleObjects]
    public class XRInteractorLineVisualEditor : BaseInteractionEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineWidth"/>.</summary>
        protected SerializedProperty m_LineWidth;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.widthCurve"/>.</summary>
        protected SerializedProperty m_WidthCurve;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.setLineColorGradient"/>.</summary>
        protected SerializedProperty m_SetLineColorGradient;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.validColorGradient"/>.</summary>
        protected SerializedProperty m_ValidColorGradient;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.invalidColorGradient"/>.</summary>
        protected SerializedProperty m_InvalidColorGradient;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.blockedColorGradient"/>.</summary>
        protected SerializedProperty m_BlockedColorGradient;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.smoothMovement"/>.</summary>
        protected SerializedProperty m_SmoothMovement;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.followTightness"/>.</summary>
        protected SerializedProperty m_FollowTightness;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.snapThresholdDistance"/>.</summary>
        protected SerializedProperty m_SnapThresholdDistance;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.reticle"/>.</summary>
        protected SerializedProperty m_Reticle;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.blockedReticle"/>.</summary>
        protected SerializedProperty m_BlockedReticle;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.overrideInteractorLineLength"/>.</summary>
        protected SerializedProperty m_OverrideInteractorLineLength;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineLength"/>.</summary>
        protected SerializedProperty m_LineLength;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.stopLineAtFirstRaycastHit"/>.</summary>
        protected SerializedProperty m_StopLineAtFirstRaycastHit;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.stopLineAtSelection"/>.</summary>
        protected SerializedProperty m_StopLineAtSelection;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.treatSelectionAsValidState"/>.</summary>
        protected SerializedProperty m_TreatSelectionAsValidState;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.snapEndpointIfAvailable"/>.</summary>
        protected SerializedProperty m_SnapEndpointIfAvailable;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineBendRatio"/>.</summary>
        protected SerializedProperty m_LineBendRatio;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.overrideInteractorLineOrigin"/>.</summary>
        protected SerializedProperty m_OverrideInteractorLineOrigin;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineOriginTransform"/>.</summary>
        protected SerializedProperty m_LineOriginTransform;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineOriginOffset"/>.</summary>
        protected SerializedProperty m_LineOriginOffset;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.autoAdjustLineLength"/>.</summary>
        protected SerializedProperty m_AutoAdjustLineLength;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.minLineLength"/>.</summary>
        protected SerializedProperty m_MinLineLength;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.useDistanceToHitAsMaxLineLength"/>.</summary>
        protected SerializedProperty m_UseDistanceToHitAsMaxLineLength;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineRetractionDelay"/>.</summary>
        protected SerializedProperty m_LineRetractionDelay;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="XRInteractorLineVisual.lineLengthChangeSpeed"/>.</summary>
        protected SerializedProperty m_LineLengthChangeSpeed;

        readonly List<Collider> m_ReticleColliders = new List<Collider>();
        readonly List<Collider> m_BlockedReticleColliders = new List<Collider>();
        XRRayInteractor m_RayInteractor;
        bool m_ReticleCheckInitialized;

        static readonly LayerMask s_EverythingMask = (-1);

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineWidth"/>.</summary>
            public static readonly GUIContent lineWidth = EditorGUIUtility.TrTextContent("Line Width", "Controls the width of the line.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.widthCurve"/>.</summary>
            public static readonly GUIContent widthCurve = EditorGUIUtility.TrTextContent("Width Curve", "Controls the relative width of the line from start to end.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.setLineColorGradient"/>.</summary>
            public static readonly GUIContent setLineColorGradient = EditorGUIUtility.TrTextContent("Set Line Color Gradient", "Whether to control the color of the Line Renderer. Disable to manually control it externally from this component.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.validColorGradient"/>.</summary>
            public static readonly GUIContent validColorGradient = EditorGUIUtility.TrTextContent("Valid Color Gradient", "Controls the color of the line as a gradient from start to end to indicate a valid state.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.invalidColorGradient"/>.</summary>
            public static readonly GUIContent invalidColorGradient = EditorGUIUtility.TrTextContent("Invalid Color Gradient", "Controls the color of the line as a gradient from start to end to indicate an invalid state.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.blockedColorGradient"/>.</summary>
            public static readonly GUIContent blockedColorGradient = EditorGUIUtility.TrTextContent("Blocked Color Gradient", "Controls the color of the line as a gradient from start to end to indicate a state where the interactor has a valid target but selection is blocked.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.smoothMovement"/>.</summary>
            public static readonly GUIContent smoothMovement = EditorGUIUtility.TrTextContent("Smooth Movement", "Controls whether the rendered segments will be delayed from and smoothly follow the target segments.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.followTightness"/>.</summary>
            public static readonly GUIContent followTightness = EditorGUIUtility.TrTextContent("Follow Tightness", "Controls the speed that the rendered segments will follow the target segments when Smooth Movement is enabled.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.snapThresholdDistance"/>.</summary>
            public static readonly GUIContent snapThresholdDistance = EditorGUIUtility.TrTextContent("Snap Threshold Distance", "Controls the threshold distance between line points at two consecutive frames to snap rendered segments to target segments when Smooth Movement is enabled.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.reticle"/>.</summary>
            public static readonly GUIContent reticle = EditorGUIUtility.TrTextContent("Reticle", "Stores the reticle that will appear at the end of the line when it is valid.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.blockedReticle"/>.</summary>
            public static readonly GUIContent blockedReticle = EditorGUIUtility.TrTextContent("Blocked Reticle", "Stores the reticle that will appear at the end of the line when the interactor has a valid target but selection is blocked.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.overrideInteractorLineLength"/>.</summary>
            public static readonly GUIContent overrideInteractorLineLength = EditorGUIUtility.TrTextContent("Override Line Length", "Controls which source is used to determine the length of the line. Set to true to use the Line Length set by this behavior. Set to false have the length of the line determined by the interactor.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineLength"/>.</summary>
            public static readonly GUIContent lineLength = EditorGUIUtility.TrTextContent("Line Length", "Controls the length of the line when overriding.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.stopLineAtFirstRaycastHit"/>.</summary>
            public static readonly GUIContent stopLineAtFirstRaycastHit = EditorGUIUtility.TrTextContent("Stop Line At First Raycast Hit", "Controls whether the line will be cut short by the first invalid ray cast hit. The line will always stop at valid targets, even if this is false.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.stopLineAtSelection"/>.</summary>
            public static readonly GUIContent stopLineAtSelection = EditorGUIUtility.TrTextContent("Stop Line At Selection", "Controls whether the line will stop at the attach point of the closest interactable selected by the interactor, if there is one.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.treatSelectionAsValidState"/>.</summary>
            public static readonly GUIContent treatSelectionAsValidState = EditorGUIUtility.TrTextContent("Treat Selection As Valid State", "Forces the use of valid state visuals while the interactor is selecting an interactable, whether or not the interactor has any valid targets.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.snapEndpointIfAvailable"/>.</summary>
            public static readonly GUIContent snapEndpointIfAvailable = EditorGUIUtility.TrTextContent("Snap Endpoint If Available", "Controls whether the visualized line will snap endpoint if the ray hits a XRInteractableSnapVolume.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineBendRatio"/>.</summary>
            public static readonly GUIContent lineBendRatio = EditorGUIUtility.TrTextContent("Line Bend Ratio", "When line is bent because target end point is out of line with the ray or snap volume is in use, this ratio determines what the bend point is. A value of 1 means the line will not bend.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.overrideInteractorLineOrigin"/>.</summary>
            public static readonly GUIContent overrideInteractorLineOrigin = EditorGUIUtility.TrTextContent("Override Line Origin", "Controls whether to use a different Transform as the starting position and direction of the line.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineOriginTransform"/>.</summary>
            public static readonly GUIContent lineOriginTransform = EditorGUIUtility.TrTextContent("Line Origin Transform", "The starting position and direction of the line when overriding.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineOriginOffset"/>.</summary>
            public static readonly GUIContent lineOriginOffset = EditorGUIUtility.TrTextContent("Line Origin Offset", "Offset from line origin along the line direction before line rendering begins. Only works if the line provider is using straight lines.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.autoAdjustLineLength"/>.</summary>
            public static readonly GUIContent autoAdjustLineLength = EditorGUIUtility.TrTextContent("Auto Adjust Line Length", "Determines whether the length of the line will retract over time when no valid hits or selection occur.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.minLineLength"/>.</summary>
            public static readonly GUIContent minLineLength = EditorGUIUtility.TrTextContent("Minimum Line Length", "Controls the minimum length of the line when overriding. When no valid hits occur, the ray visual shrinks down to this size.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.useDistanceToHitAsMaxLineLength"/>.</summary>
            public static readonly GUIContent useDistanceToHitAsMaxLineLength = EditorGUIUtility.TrTextContent("Use Distance To Hit As Max Line Length", "Determines whether the max line length will be the the distance to the hit point or the fixed line length.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineRetractionDelay"/>.</summary>
            public static readonly GUIContent lineRetractionDelay = EditorGUIUtility.TrTextContent("Line Retraction Delay", "Time in seconds elapsed after last valid hit or selection for line to begin retracting to the minimum override length.");
            /// <summary><see cref="GUIContent"/> for <see cref="XRInteractorLineVisual.lineLengthChangeSpeed"/>.</summary>
            public static readonly GUIContent lineLengthChangeSpeed = EditorGUIUtility.TrTextContent("Line Length Change Speed", "Scalar used to control the speed of changes in length of the line when overriding it's length.");

            /// <summary>The help box message when the Reticle has a Collider that will disrupt the XR Ray Interactor ray cast.</summary>
            public static readonly GUIContent reticleColliderWarning = EditorGUIUtility.TrTextContent("Reticle has a Collider which may disrupt the XR Ray Interactor ray cast. Remove or disable the Collider component on the Reticle or adjust the Raycast Mask/Collider Layer.");
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            m_LineWidth = serializedObject.FindProperty("m_LineWidth");
            m_WidthCurve = serializedObject.FindProperty("m_WidthCurve");
            m_SetLineColorGradient = serializedObject.FindProperty("m_SetLineColorGradient");
            m_ValidColorGradient = serializedObject.FindProperty("m_ValidColorGradient");
            m_InvalidColorGradient = serializedObject.FindProperty("m_InvalidColorGradient");
            m_BlockedColorGradient = serializedObject.FindProperty("m_BlockedColorGradient");
            m_SmoothMovement = serializedObject.FindProperty("m_SmoothMovement");
            m_FollowTightness = serializedObject.FindProperty("m_FollowTightness");
            m_SnapThresholdDistance = serializedObject.FindProperty("m_SnapThresholdDistance");
            m_Reticle = serializedObject.FindProperty("m_Reticle");
            m_BlockedReticle = serializedObject.FindProperty("m_BlockedReticle");
            m_OverrideInteractorLineLength = serializedObject.FindProperty("m_OverrideInteractorLineLength");
            m_LineLength = serializedObject.FindProperty("m_LineLength");
            m_StopLineAtFirstRaycastHit = serializedObject.FindProperty("m_StopLineAtFirstRaycastHit");
            m_StopLineAtSelection = serializedObject.FindProperty("m_StopLineAtSelection");
            m_TreatSelectionAsValidState = serializedObject.FindProperty("m_TreatSelectionAsValidState");
            m_SnapEndpointIfAvailable = serializedObject.FindProperty("m_SnapEndpointIfAvailable");
            m_LineBendRatio = serializedObject.FindProperty("m_LineBendRatio");
            m_OverrideInteractorLineOrigin = serializedObject.FindProperty("m_OverrideInteractorLineOrigin");
            m_LineOriginTransform = serializedObject.FindProperty("m_LineOriginTransform");
            m_LineOriginOffset = serializedObject.FindProperty("m_LineOriginOffset");
            m_AutoAdjustLineLength = serializedObject.FindProperty("m_AutoAdjustLineLength");
            m_MinLineLength = serializedObject.FindProperty("m_MinLineLength");
            m_UseDistanceToHitAsMaxLineLength = serializedObject.FindProperty("m_UseDistanceToHitAsMaxLineLength");
            m_LineRetractionDelay = serializedObject.FindProperty("m_LineRetractionDelay");
            m_LineLengthChangeSpeed = serializedObject.FindProperty("m_LineLengthChangeSpeed");

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
            DrawWidthConfiguration();
            DrawLineOriginConfiguration();
            EditorGUILayout.Space();
            DrawColorConfiguration();
            EditorGUILayout.Space();
            DrawLengthConfiguration();
            EditorGUILayout.Space();
            DrawSmoothMovement();
            DrawSnappingSettings();
            DrawReticle();
        }

        /// <summary>
        /// Draw property fields related to the line width.
        /// </summary>
        protected virtual void DrawWidthConfiguration()
        {
            EditorGUILayout.PropertyField(m_LineWidth, Contents.lineWidth);
            EditorGUILayout.PropertyField(m_WidthCurve, Contents.widthCurve);
        }

        /// <summary>
        /// Draw property fields related to line origin.
        /// </summary>
        protected virtual void DrawLineOriginConfiguration()
        {
            EditorGUILayout.PropertyField(m_OverrideInteractorLineOrigin, Contents.overrideInteractorLineOrigin);
            if (m_OverrideInteractorLineOrigin.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_LineOriginTransform, Contents.lineOriginTransform);
                }
            }

            EditorGUILayout.PropertyField(m_LineOriginOffset, Contents.lineOriginOffset);
        }

        /// <summary>
        /// Draw property fields related to color gradients.
        /// </summary>
        protected virtual void DrawColorConfiguration()
        {
            EditorGUILayout.PropertyField(m_SetLineColorGradient, Contents.setLineColorGradient);
            EditorGUILayout.PropertyField(m_ValidColorGradient, Contents.validColorGradient);
            EditorGUILayout.PropertyField(m_InvalidColorGradient, Contents.invalidColorGradient);
            EditorGUILayout.PropertyField(m_BlockedColorGradient, Contents.blockedColorGradient);
            EditorGUILayout.PropertyField(m_TreatSelectionAsValidState, Contents.treatSelectionAsValidState);
        }

        /// <summary>
        /// Draw property fields related to the line length.
        /// </summary>
        protected virtual void DrawLengthConfiguration()
        {
            EditorGUILayout.PropertyField(m_OverrideInteractorLineLength, Contents.overrideInteractorLineLength);
            if (m_OverrideInteractorLineLength.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_LineLength, Contents.lineLength);
                    
                    EditorGUILayout.PropertyField(m_AutoAdjustLineLength, Contents.autoAdjustLineLength);
                    if (m_AutoAdjustLineLength.boolValue)
                    {
                        using (new EditorGUI.IndentLevelScope())
                        {
                            EditorGUILayout.PropertyField(m_MinLineLength, Contents.minLineLength);
                            EditorGUILayout.PropertyField(m_UseDistanceToHitAsMaxLineLength, Contents.useDistanceToHitAsMaxLineLength);
                            EditorGUILayout.PropertyField(m_LineRetractionDelay, Contents.lineRetractionDelay);
                            EditorGUILayout.PropertyField(m_LineLengthChangeSpeed, Contents.lineLengthChangeSpeed);
                        }
                    }
                }
            }

            EditorGUILayout.PropertyField(m_StopLineAtFirstRaycastHit, Contents.stopLineAtFirstRaycastHit);
            EditorGUILayout.PropertyField(m_StopLineAtSelection, Contents.stopLineAtSelection);
        }

        /// <summary>
        /// Draw property fields related to smooth movement.
        /// </summary>
        protected virtual void DrawSmoothMovement()
        {
            EditorGUILayout.PropertyField(m_SmoothMovement, Contents.smoothMovement);

            if (m_SmoothMovement.boolValue)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_FollowTightness, Contents.followTightness);
                    EditorGUILayout.PropertyField(m_SnapThresholdDistance, Contents.snapThresholdDistance);
                }
            }
        }
        
        /// <summary>
        /// Draw property fields related to snapping.
        /// </summary>
        protected virtual void DrawSnappingSettings()
        {
            EditorGUILayout.PropertyField(m_SnapEndpointIfAvailable, Contents.snapEndpointIfAvailable);
            EditorGUILayout.PropertyField(m_LineBendRatio, Contents.lineBendRatio);
        }

        /// <summary>
        /// Draw property fields related to the reticle.
        /// </summary>
        protected virtual void DrawReticle()
        {
            EditorGUILayout.Space();
            
            // Get the list of Colliders on  each reticle if this is the first time here in order to reduce the cost of evaluating the collider check warnings.
            if (!serializedObject.isEditingMultipleObjects && !m_ReticleCheckInitialized)
            {
                GatherObjectColliders(m_Reticle, m_ReticleColliders);
                GatherObjectColliders(m_BlockedReticle, m_BlockedReticleColliders);
                m_RayInteractor = ((XRInteractorLineVisual)serializedObject.targetObject).GetComponent<XRRayInteractor>();
                m_ReticleCheckInitialized = true;
            }

            DrawReticleProperty(m_Reticle, Contents.reticle, m_ReticleColliders);
            DrawReticleProperty(m_BlockedReticle, Contents.blockedReticle, m_BlockedReticleColliders);
        }

        static void GatherObjectColliders(SerializedProperty gameObjectProperty, List<Collider> colliders)
        {
            if (gameObjectProperty.objectReferenceValue == null)
            {
                colliders.Clear();
                return;
            }

            var gameObject = (GameObject)gameObjectProperty.objectReferenceValue;
            gameObject.GetComponentsInChildren(colliders);
        }

        void DrawReticleProperty(SerializedProperty property, GUIContent label, List<Collider> reticleColliders)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(property, label);

                // Show a warning if the reticle GameObject has a Collider, which would cause
                // a feedback loop issue with the raycast hitting the reticle.
                if (!serializedObject.isEditingMultipleObjects)
                {
                    // Update the list of Colliders on the reticle if the reticle property changed.
                    if (check.changed)
                    {
                        GatherObjectColliders(property, reticleColliders);
                        m_RayInteractor = ((XRInteractorLineVisual)serializedObject.targetObject).GetComponent<XRRayInteractor>();
                    }

                    if (reticleColliders.Count > 0)
                    {
                        // If there is an XR Ray Interactor, allow the Collider as long as the Raycast Mask is set to ignore it
                        var raycastMask = m_RayInteractor != null ? m_RayInteractor.raycastMask : s_EverythingMask;
                        foreach (var collider in reticleColliders)
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
