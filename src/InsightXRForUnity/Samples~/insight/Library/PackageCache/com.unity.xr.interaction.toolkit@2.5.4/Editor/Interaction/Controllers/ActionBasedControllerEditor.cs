using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// Custom editor for an <see cref="ActionBasedController"/>.
    /// </summary>
    [CustomEditor(typeof(ActionBasedController), true), CanEditMultipleObjects]
    [MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
    public class ActionBasedControllerEditor : XRBaseControllerEditor
    {
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.positionAction"/>.</summary>
        protected SerializedProperty m_PositionAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.rotationAction"/>.</summary>
        protected SerializedProperty m_RotationAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.isTrackedAction"/>.</summary>
        protected SerializedProperty m_IsTrackedAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.trackingStateAction"/>.</summary>
        protected SerializedProperty m_TrackingStateAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.selectAction"/>.</summary>
        protected SerializedProperty m_SelectAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.selectActionValue"/>.</summary>
        protected SerializedProperty m_SelectActionValue;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.activateAction"/>.</summary>
        protected SerializedProperty m_ActivateAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.activateActionValue"/>.</summary>
        protected SerializedProperty m_ActivateActionValue;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.uiPressAction"/>.</summary>
        protected SerializedProperty m_UIPressAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.uiPressActionValue"/>.</summary>
        protected SerializedProperty m_UIPressActionValue;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.uiScrollAction"/>.</summary>
        protected SerializedProperty m_UIScrollAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.hapticDeviceAction"/>.</summary>
        protected SerializedProperty m_HapticDeviceAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.rotateAnchorAction"/>.</summary>
        protected SerializedProperty m_RotateAnchorAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.directionalAnchorRotationAction"/>.</summary>
        protected SerializedProperty m_DirectionalAnchorRotationAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.translateAnchorAction"/>.</summary>
        protected SerializedProperty m_TranslateAnchorAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.scaleToggleAction"/>.</summary>
        protected SerializedProperty m_ScaleToggleAction;
        /// <summary><see cref="SerializedProperty"/> of the <see cref="SerializeField"/> backing <see cref="ActionBasedController.scaleDeltaAction"/>.</summary>
        protected SerializedProperty m_ScaleDeltaAction;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.positionAction"/>.</summary>
            public static GUIContent positionAction = EditorGUIUtility.TrTextContent("Position Action", "The action to use for Position Tracking for this GameObject. (Vector 3 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.rotationAction"/>.</summary>
            public static GUIContent rotationAction = EditorGUIUtility.TrTextContent("Rotation Action", "The action to use for Rotation Tracking for this GameObject. (Quaternion Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.isTrackedAction"/>.</summary>
            public static GUIContent isTrackedAction = EditorGUIUtility.TrTextContent("Is Tracked Action", "The action to read whether this controller is tracked; falls back to the tracked device's is tracked state when not set. (Button Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.trackingStateAction"/>.</summary>
            public static GUIContent trackingStateAction = EditorGUIUtility.TrTextContent("Tracking State Action", "The action to read the values being actively tracked; falls back to the tracked device's tracking state when not set. (Integer Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.selectAction"/>.</summary>
            public static GUIContent selectAction = EditorGUIUtility.TrTextContent("Select Action", "The action to use for selecting an Interactable. (Button Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.selectAction"/>.</summary>
            public static GUIContent selectActionValue = EditorGUIUtility.TrTextContent("Select Action Value", "(Optional) The action to read the float value of Select Action, if different. (Axis or Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.activateAction"/>.</summary>
            public static GUIContent activateAction = EditorGUIUtility.TrTextContent("Activate Action", "The action to use for activating a selected Interactable. (Button Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.activateAction"/>.</summary>
            public static GUIContent activateActionValue = EditorGUIUtility.TrTextContent("Activate Action Value", "(Optional) The action to read the float value of Activate Action, if different. (Axis or Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.uiPressAction"/>.</summary>
            public static GUIContent uiPressAction = EditorGUIUtility.TrTextContent("UI Press Action", "The action to use for Canvas UI interaction. (Button Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.uiPressAction"/>.</summary>
            public static GUIContent uiPressActionValue = EditorGUIUtility.TrTextContent("UI Press Action Value", "(Optional) The action to read the float value of UI Press Action, if different. (Axis or Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.uiScrollAction"/>.</summary>
            public static GUIContent uiScrollAction = EditorGUIUtility.TrTextContent("UI Scroll Action", "The action to read the vector 2 value of UI Scroll Action, typically a joystick or touchpad. (Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.hapticDeviceAction"/>.</summary>
            public static GUIContent hapticDeviceAction = EditorGUIUtility.TrTextContent("Haptic Device Action", "The action to use for identifying the device to send haptic impulses to. Can be any control type that will have an active control driving the action.");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.rotateAnchorAction"/>.</summary>
            public static GUIContent rotateAnchorAction = EditorGUIUtility.TrTextContent("Rotate Anchor Action", "The action to use for rotating the interactor's attach point over time. Will use the x-axis as the rotation input. (Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.directionalAnchorRotationAction"/>.</summary>
            public static GUIContent directionalAnchorRotationAction = EditorGUIUtility.TrTextContent("Directional Anchor Rotation Action", "The action to use for computing a direction angle to rotate the interactor's attach point to match it. (Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.translateAnchorAction"/>.</summary>
            public static GUIContent translateAnchorAction = EditorGUIUtility.TrTextContent("Translate Anchor Action", "The action to use for translating the interactor's attach point closer or further away from the interactor. Will use the y-axis as the translation input. (Vector 2 Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.scaleToggleAction"/>.</summary>
            public static GUIContent scaleToggleAction = EditorGUIUtility.TrTextContent("Scale Toggle Action", "The action to use to enable or disabled reading from the Scale Delta Action. (Button Control)");
            /// <summary><see cref="GUIContent"/> for <see cref="ActionBasedController.scaleDeltaAction"/>.</summary>
            public static GUIContent scaleDeltaAction = EditorGUIUtility.TrTextContent("Scale Delta Action", "The action to use for providing a scale delta value to transformers. Will use the y-axis as the scaling input. (Vector 2 Control)");

            /// <summary>The help box message when Update Tracking Type is not Fixed.</summary>
            public static readonly GUIContent updateModeNotFixed = EditorGUIUtility.TrTextContent("Input System Update Mode is set to Process Events In Fixed Update, but the controller Update Tracking Type is not set to Fixed. This means that input querying of the controller pose will not be in sync with the Input System.");
            /// <summary>The help box message when Update Tracking Type is not Fixed.</summary>
            public static readonly GUIContent updateModeIsFixed = EditorGUIUtility.TrTextContent("Input System Update Mode is set to Process Events In Dynamic Update, but the controller Update Tracking Type is set to Fixed. This means that input querying of the controller pose will not be in sync with the Input System.");
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();

            m_PositionAction = serializedObject.FindProperty("m_PositionAction");
            m_RotationAction = serializedObject.FindProperty("m_RotationAction");
            m_IsTrackedAction = serializedObject.FindProperty("m_IsTrackedAction");
            m_TrackingStateAction = serializedObject.FindProperty("m_TrackingStateAction");
            m_SelectAction = serializedObject.FindProperty("m_SelectAction");
            m_SelectActionValue = serializedObject.FindProperty("m_SelectActionValue");
            m_ActivateAction = serializedObject.FindProperty("m_ActivateAction");
            m_ActivateActionValue = serializedObject.FindProperty("m_ActivateActionValue");
            m_UIPressAction = serializedObject.FindProperty("m_UIPressAction");
            m_UIPressActionValue = serializedObject.FindProperty("m_UIPressActionValue");
            m_UIScrollAction = serializedObject.FindProperty("m_UIScrollAction");
            m_HapticDeviceAction = serializedObject.FindProperty("m_HapticDeviceAction");
            m_RotateAnchorAction = serializedObject.FindProperty("m_RotateAnchorAction");
            m_DirectionalAnchorRotationAction = serializedObject.FindProperty("m_DirectionalAnchorRotationAction");
            m_TranslateAnchorAction = serializedObject.FindProperty("m_TranslateAnchorAction");
            m_ScaleDeltaAction = serializedObject.FindProperty("m_ScaleDeltaAction");
            m_ScaleToggleAction = serializedObject.FindProperty("m_ScaleToggleAction");
        }

        /// <inheritdoc />
        protected override void DrawTrackingConfiguration()
        {
            base.DrawTrackingConfiguration();

            if (m_EnableInputTracking.boolValue)
            {
                switch (InputSystem.settings.updateMode)
                {
                    case InputSettings.UpdateMode.ProcessEventsInFixedUpdate when m_UpdateTrackingType.intValue != (int)XRBaseController.UpdateType.Fixed:
                        EditorGUILayout.HelpBox(Contents.updateModeNotFixed.text, MessageType.Warning);
                        break;
                    case InputSettings.UpdateMode.ProcessEventsInDynamicUpdate when m_UpdateTrackingType.intValue == (int)XRBaseController.UpdateType.Fixed:
                        EditorGUILayout.HelpBox(Contents.updateModeIsFixed.text, MessageType.Warning);
                        break;
                }
            }

            EditorGUILayout.PropertyField(m_PositionAction, Contents.positionAction);
            EditorGUILayout.PropertyField(m_RotationAction, Contents.rotationAction);
            EditorGUILayout.PropertyField(m_IsTrackedAction, Contents.isTrackedAction);
            EditorGUILayout.PropertyField(m_TrackingStateAction, Contents.trackingStateAction);
        }

        /// <inheritdoc />
        protected override void DrawInputConfiguration()
        {
            base.DrawInputConfiguration();
            EditorGUILayout.PropertyField(m_SelectAction, Contents.selectAction);
            EditorGUILayout.PropertyField(m_SelectActionValue, Contents.selectActionValue);
            EditorGUILayout.PropertyField(m_ActivateAction, Contents.activateAction);
            EditorGUILayout.PropertyField(m_ActivateActionValue, Contents.activateActionValue);
            EditorGUILayout.PropertyField(m_UIPressAction, Contents.uiPressAction);
            EditorGUILayout.PropertyField(m_UIPressActionValue, Contents.uiPressActionValue);
            EditorGUILayout.PropertyField(m_UIScrollAction, Contents.uiScrollAction);
        }

        /// <inheritdoc />
        protected override void DrawOtherActions()
        {
            base.DrawOtherActions();
            EditorGUILayout.PropertyField(m_HapticDeviceAction, Contents.hapticDeviceAction);
            EditorGUILayout.PropertyField(m_RotateAnchorAction, Contents.rotateAnchorAction);
            EditorGUILayout.PropertyField(m_DirectionalAnchorRotationAction, Contents.directionalAnchorRotationAction);
            EditorGUILayout.PropertyField(m_TranslateAnchorAction, Contents.translateAnchorAction);
            EditorGUILayout.PropertyField(m_ScaleToggleAction, Contents.scaleToggleAction);
            EditorGUILayout.PropertyField(m_ScaleDeltaAction, Contents.scaleDeltaAction);
        }
    }
}
