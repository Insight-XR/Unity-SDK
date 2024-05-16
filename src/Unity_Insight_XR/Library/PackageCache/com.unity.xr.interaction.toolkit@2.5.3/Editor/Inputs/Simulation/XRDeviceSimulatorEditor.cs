using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace UnityEditor.XR.Interaction.Toolkit.Inputs.Simulation
{
    /// <summary>
    /// Custom editor for <see cref="XRDeviceSimulator"/>.
    /// </summary>
    [CustomEditor(typeof(XRDeviceSimulator), true), CanEditMultipleObjects]
    class XRDeviceSimulatorEditor : BaseInteractionEditor
    {
        const string k_GlobalActionsExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".GlobalActionsExpanded";
        const string k_ControllerActionsExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".GlobalActionsExpanded";
        const string k_HandActionsExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".HandActionsExpanded";
        const string k_SimulatorSettingsExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".SimulatorSettingsExpanded";
        const string k_SensitivityExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".SensitivityExpanded";
        const string k_AnalogConfigurationExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".AnalogConfigurationExpanded";
        const string k_TrackingStateExpandedKey = "XRI." + nameof(XRDeviceSimulatorEditor) + ".TrackingStateExpanded";

        // Global Actions
        bool m_GlobalActionsExpanded;
        SerializedProperty m_DeviceSimulatorActionAsset;
        SerializedProperty m_KeyboardXTranslateAction;
        SerializedProperty m_KeyboardYTranslateAction;
        SerializedProperty m_KeyboardZTranslateAction;
        SerializedProperty m_ManipulateLeftAction;
        SerializedProperty m_ToggleManipulateLeftAction;
        SerializedProperty m_ManipulateRightAction;
        SerializedProperty m_ToggleManipulateRightAction;
        SerializedProperty m_ToggleManipulateBodyAction;
        SerializedProperty m_ManipulateHeadAction;
        SerializedProperty m_HandControllerModeAction;
        SerializedProperty m_CycleDevicesAction;
        SerializedProperty m_StopManipulationAction;
        SerializedProperty m_MouseDeltaAction;
        SerializedProperty m_MouseScrollAction;
        SerializedProperty m_RotateModeOverrideAction;
        SerializedProperty m_ToggleMouseTransformationModeAction;
        SerializedProperty m_NegateModeAction;
        SerializedProperty m_XConstraintAction;
        SerializedProperty m_YConstraintAction;
        SerializedProperty m_ZConstraintAction;
        SerializedProperty m_ResetAction;
        SerializedProperty m_ToggleCursorLockAction;
        SerializedProperty m_TogglePrimary2DAxisTargetAction;
        SerializedProperty m_ToggleSecondary2DAxisTargetAction;
        SerializedProperty m_ToggleDevicePositionTargetAction;

        // Controller Actions
        bool m_ControllerActionsExpanded;
        SerializedProperty m_ControllerActionAsset;
        SerializedProperty m_Axis2DAction;
        SerializedProperty m_RestingHandAxis2DAction;
        SerializedProperty m_GripAction;
        SerializedProperty m_TriggerAction;
        SerializedProperty m_PrimaryButtonAction;
        SerializedProperty m_SecondaryButtonAction;
        SerializedProperty m_MenuAction;
        SerializedProperty m_Primary2DAxisClickAction;
        SerializedProperty m_Secondary2DAxisClickAction;
        SerializedProperty m_Primary2DAxisTouchAction;
        SerializedProperty m_Secondary2DAxisTouchAction;
        SerializedProperty m_PrimaryTouchAction;
        SerializedProperty m_SecondaryTouchAction;
        
        // Hand Actions
        bool m_HandActionsExpanded;
        SerializedProperty m_HandActionAsset;
        SerializedProperty m_RestingHandExpressionCapture;
        SerializedProperty m_SimulatedHandExpressions;

        // Simulator Settings
        bool m_SimulatorSettingsExpanded;
        SerializedProperty m_CameraTransform;
        SerializedProperty m_KeyboardTranslateSpace;
        SerializedProperty m_MouseTranslateSpace;
        SerializedProperty m_DesiredCursorLockMode;
        SerializedProperty m_RemoveOtherHMDDevices;
        SerializedProperty m_HandTrackingCapability;
        SerializedProperty m_DeviceSimulatorUI;
        
        // Sensitivity
        bool m_SensitivityExpanded;
        SerializedProperty m_KeyboardXTranslateSpeed;
        SerializedProperty m_KeyboardYTranslateSpeed;
        SerializedProperty m_KeyboardZTranslateSpeed;
        SerializedProperty m_KeyboardBodyTranslateMultiplier;
        SerializedProperty m_MouseXTranslateSensitivity;
        SerializedProperty m_MouseYTranslateSensitivity;
        SerializedProperty m_MouseScrollTranslateSensitivity;
        SerializedProperty m_MouseXRotateSensitivity;
        SerializedProperty m_MouseYRotateSensitivity;
        SerializedProperty m_MouseScrollRotateSensitivity;
        SerializedProperty m_MouseYRotateInvert;

        // Analog Configuration
        bool m_AnalogConfigurationExpanded;
        SerializedProperty m_GripAmount;
        SerializedProperty m_TriggerAmount;
        
        // Tracking State
        bool m_TrackingStateExpanded;
        SerializedProperty m_HMDIsTracked;
        SerializedProperty m_HMDTrackingState;
        SerializedProperty m_LeftControllerIsTracked;
        SerializedProperty m_LeftControllerTrackingState;
        SerializedProperty m_RightControllerIsTracked;
        SerializedProperty m_RightControllerTrackingState;
        SerializedProperty m_LeftHandIsTracked;
        SerializedProperty m_RightHandIsTracked;

        /// <summary>
        /// Contents of GUI elements used by this editor.
        /// </summary>
        protected static class Contents
        {
            public static readonly GUIStyle foldoutTitleStyle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold,
            };
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        /// <seealso cref="MonoBehaviour"/>
        protected virtual void OnEnable()
        {
            m_DeviceSimulatorActionAsset = serializedObject.FindProperty("m_DeviceSimulatorActionAsset");
            m_KeyboardXTranslateAction = serializedObject.FindProperty("m_KeyboardXTranslateAction");
            m_KeyboardYTranslateAction = serializedObject.FindProperty("m_KeyboardYTranslateAction");
            m_KeyboardZTranslateAction = serializedObject.FindProperty("m_KeyboardZTranslateAction");
            m_ManipulateLeftAction = serializedObject.FindProperty("m_ManipulateLeftAction");
            m_ManipulateRightAction = serializedObject.FindProperty("m_ManipulateRightAction");
            m_ManipulateHeadAction = serializedObject.FindProperty("m_ManipulateHeadAction");
            m_ToggleManipulateLeftAction = serializedObject.FindProperty("m_ToggleManipulateLeftAction");
            m_ToggleManipulateRightAction = serializedObject.FindProperty("m_ToggleManipulateRightAction");
            m_ToggleManipulateBodyAction = serializedObject.FindProperty("m_ToggleManipulateBodyAction");
            m_HandControllerModeAction = serializedObject.FindProperty("m_HandControllerModeAction");
            m_CycleDevicesAction = serializedObject.FindProperty("m_CycleDevicesAction");
            m_StopManipulationAction = serializedObject.FindProperty("m_StopManipulationAction");
            m_MouseDeltaAction = serializedObject.FindProperty("m_MouseDeltaAction");
            m_MouseScrollAction = serializedObject.FindProperty("m_MouseScrollAction");
            m_RotateModeOverrideAction = serializedObject.FindProperty("m_RotateModeOverrideAction");
            m_ToggleMouseTransformationModeAction = serializedObject.FindProperty("m_ToggleMouseTransformationModeAction");
            m_NegateModeAction = serializedObject.FindProperty("m_NegateModeAction");
            m_XConstraintAction = serializedObject.FindProperty("m_XConstraintAction");
            m_YConstraintAction = serializedObject.FindProperty("m_YConstraintAction");
            m_ZConstraintAction = serializedObject.FindProperty("m_ZConstraintAction");
            m_ResetAction = serializedObject.FindProperty("m_ResetAction");
            m_ToggleCursorLockAction = serializedObject.FindProperty("m_ToggleCursorLockAction");
            m_TogglePrimary2DAxisTargetAction = serializedObject.FindProperty("m_TogglePrimary2DAxisTargetAction");
            m_ToggleSecondary2DAxisTargetAction = serializedObject.FindProperty("m_ToggleSecondary2DAxisTargetAction");
            m_ToggleDevicePositionTargetAction = serializedObject.FindProperty("m_ToggleDevicePositionTargetAction");

            m_ControllerActionAsset = serializedObject.FindProperty("m_ControllerActionAsset");
            m_Axis2DAction = serializedObject.FindProperty("m_Axis2DAction");
            m_RestingHandAxis2DAction = serializedObject.FindProperty("m_RestingHandAxis2DAction");
            m_GripAction = serializedObject.FindProperty("m_GripAction");
            m_TriggerAction = serializedObject.FindProperty("m_TriggerAction");
            m_PrimaryButtonAction = serializedObject.FindProperty("m_PrimaryButtonAction");
            m_SecondaryButtonAction = serializedObject.FindProperty("m_SecondaryButtonAction");
            m_MenuAction = serializedObject.FindProperty("m_MenuAction");
            m_Primary2DAxisClickAction = serializedObject.FindProperty("m_Primary2DAxisClickAction");
            m_Secondary2DAxisClickAction = serializedObject.FindProperty("m_Secondary2DAxisClickAction");
            m_Primary2DAxisTouchAction = serializedObject.FindProperty("m_Primary2DAxisTouchAction");
            m_Secondary2DAxisTouchAction = serializedObject.FindProperty("m_Secondary2DAxisTouchAction");
            m_PrimaryTouchAction = serializedObject.FindProperty("m_PrimaryTouchAction");
            m_SecondaryTouchAction = serializedObject.FindProperty("m_SecondaryTouchAction");
            
            m_HandActionAsset = serializedObject.FindProperty("m_HandActionAsset");
            m_RestingHandExpressionCapture = serializedObject.FindProperty("m_RestingHandExpressionCapture");
            m_SimulatedHandExpressions = serializedObject.FindProperty("m_SimulatedHandExpressions");
            
            m_CameraTransform = serializedObject.FindProperty("m_CameraTransform");
            m_KeyboardTranslateSpace = serializedObject.FindProperty("m_KeyboardTranslateSpace");
            m_MouseTranslateSpace = serializedObject.FindProperty("m_MouseTranslateSpace");
            m_DesiredCursorLockMode = serializedObject.FindProperty("m_DesiredCursorLockMode");
            m_RemoveOtherHMDDevices = serializedObject.FindProperty("m_RemoveOtherHMDDevices");
            m_HandTrackingCapability = serializedObject.FindProperty("m_HandTrackingCapability");
            m_DeviceSimulatorUI = serializedObject.FindProperty("m_DeviceSimulatorUI");
            
            m_KeyboardXTranslateSpeed = serializedObject.FindProperty("m_KeyboardXTranslateSpeed");
            m_KeyboardYTranslateSpeed = serializedObject.FindProperty("m_KeyboardYTranslateSpeed");
            m_KeyboardZTranslateSpeed = serializedObject.FindProperty("m_KeyboardZTranslateSpeed");
            m_KeyboardBodyTranslateMultiplier = serializedObject.FindProperty("m_KeyboardBodyTranslateMultiplier");
            m_MouseXTranslateSensitivity = serializedObject.FindProperty("m_MouseXTranslateSensitivity");
            m_MouseYTranslateSensitivity = serializedObject.FindProperty("m_MouseYTranslateSensitivity");
            m_MouseScrollTranslateSensitivity = serializedObject.FindProperty("m_MouseScrollTranslateSensitivity");
            m_MouseXRotateSensitivity = serializedObject.FindProperty("m_MouseXRotateSensitivity");
            m_MouseYRotateSensitivity = serializedObject.FindProperty("m_MouseYRotateSensitivity");
            m_MouseScrollRotateSensitivity = serializedObject.FindProperty("m_MouseScrollRotateSensitivity");
            m_MouseYRotateInvert = serializedObject.FindProperty("m_MouseYRotateInvert");
            
            m_GripAmount = serializedObject.FindProperty("m_GripAmount");
            m_TriggerAmount = serializedObject.FindProperty("m_TriggerAmount");
            
            m_HMDIsTracked = serializedObject.FindProperty("m_HMDIsTracked");
            m_HMDTrackingState = serializedObject.FindProperty("m_HMDTrackingState");
            m_LeftControllerIsTracked = serializedObject.FindProperty("m_LeftControllerIsTracked");
            m_LeftControllerTrackingState = serializedObject.FindProperty("m_LeftControllerTrackingState");
            m_RightControllerIsTracked = serializedObject.FindProperty("m_RightControllerIsTracked");
            m_RightControllerTrackingState = serializedObject.FindProperty("m_RightControllerTrackingState");
            m_LeftHandIsTracked = serializedObject.FindProperty("m_LeftHandIsTracked");
            m_RightHandIsTracked = serializedObject.FindProperty("m_RightHandIsTracked");

            m_GlobalActionsExpanded = SessionState.GetBool(k_GlobalActionsExpandedKey, false);
            m_ControllerActionsExpanded = SessionState.GetBool(k_ControllerActionsExpandedKey, false);
            m_HandActionsExpanded = SessionState.GetBool(k_HandActionsExpandedKey, false);
            m_SimulatorSettingsExpanded = SessionState.GetBool(k_SimulatorSettingsExpandedKey, false);
            m_SensitivityExpanded = SessionState.GetBool(k_SensitivityExpandedKey, false);
            m_AnalogConfigurationExpanded = SessionState.GetBool(k_AnalogConfigurationExpandedKey, false);
            m_TrackingStateExpanded = SessionState.GetBool(k_TrackingStateExpandedKey, false);
        }

        /// <inheritdoc />
        protected override void DrawInspector()
        {
            DrawGlobalActions();
            EditorGUILayout.Space();
            DrawControllerActions();
            EditorGUILayout.Space();
            DrawHandActions();
            EditorGUILayout.Space();
            DrawGeneralSimulatorSettings();
            EditorGUILayout.Space();
            DrawSensitivitySettings();
            EditorGUILayout.Space();
            DrawAnalogConfigurationSettings();
            EditorGUILayout.Space();
            DrawTrackingStateSettings();
            EditorGUILayout.Space();
            DrawDerivedProperties();
        }

        /// <summary>
        /// Draw the property fields related to global actions.
        /// </summary>
        protected virtual void DrawGlobalActions()
        {
            EditorGUILayout.LabelField("Global Actions", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_DeviceSimulatorActionAsset);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_GlobalActionsExpanded = EditorGUILayout.Foldout(m_GlobalActionsExpanded, "Device Simulator Actions", true);
                    if (check.changed)
                        SessionState.SetBool(k_GlobalActionsExpandedKey, m_GlobalActionsExpanded);
                }

                if (!m_GlobalActionsExpanded)
                    return;

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_KeyboardXTranslateAction);
                    EditorGUILayout.PropertyField(m_KeyboardYTranslateAction);
                    EditorGUILayout.PropertyField(m_KeyboardZTranslateAction);
                    EditorGUILayout.PropertyField(m_ToggleManipulateLeftAction);
                    EditorGUILayout.PropertyField(m_ToggleManipulateRightAction);
                    EditorGUILayout.PropertyField(m_ToggleManipulateBodyAction);
                    EditorGUILayout.PropertyField(m_ManipulateLeftAction);
                    EditorGUILayout.PropertyField(m_ManipulateRightAction);
                    EditorGUILayout.PropertyField(m_ManipulateHeadAction);
                    EditorGUILayout.PropertyField(m_HandControllerModeAction);
                    EditorGUILayout.PropertyField(m_CycleDevicesAction);
                    EditorGUILayout.PropertyField(m_StopManipulationAction);
                    EditorGUILayout.PropertyField(m_MouseDeltaAction);
                    EditorGUILayout.PropertyField(m_MouseScrollAction);
                    EditorGUILayout.PropertyField(m_RotateModeOverrideAction);
                    EditorGUILayout.PropertyField(m_ToggleMouseTransformationModeAction);
                    EditorGUILayout.PropertyField(m_NegateModeAction);
                    EditorGUILayout.PropertyField(m_XConstraintAction);
                    EditorGUILayout.PropertyField(m_YConstraintAction);
                    EditorGUILayout.PropertyField(m_ZConstraintAction);
                    EditorGUILayout.PropertyField(m_ResetAction);
                    EditorGUILayout.PropertyField(m_ToggleCursorLockAction);
                    EditorGUILayout.PropertyField(m_TogglePrimary2DAxisTargetAction);
                    EditorGUILayout.PropertyField(m_ToggleSecondary2DAxisTargetAction);
                    EditorGUILayout.PropertyField(m_ToggleDevicePositionTargetAction);
                }
            }
        }


        /// <summary>
        /// Draw the property fields related to controller actions.
        /// </summary>
        protected virtual void DrawControllerActions()
        {
            EditorGUILayout.LabelField("Controller Actions", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ControllerActionAsset);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_ControllerActionsExpanded = EditorGUILayout.Foldout(m_ControllerActionsExpanded, "Controller Actions", true);
                    if (check.changed)
                        SessionState.SetBool(k_ControllerActionsExpandedKey, m_ControllerActionsExpanded);
                }

                if (!m_ControllerActionsExpanded)
                    return;

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_Axis2DAction);
                    EditorGUILayout.PropertyField(m_RestingHandAxis2DAction);
                    EditorGUILayout.PropertyField(m_GripAction);
                    EditorGUILayout.PropertyField(m_TriggerAction);
                    EditorGUILayout.PropertyField(m_PrimaryButtonAction);
                    EditorGUILayout.PropertyField(m_SecondaryButtonAction);
                    EditorGUILayout.PropertyField(m_MenuAction);
                    EditorGUILayout.PropertyField(m_Primary2DAxisClickAction);
                    EditorGUILayout.PropertyField(m_Secondary2DAxisClickAction);
                    EditorGUILayout.PropertyField(m_Primary2DAxisTouchAction);
                    EditorGUILayout.PropertyField(m_Secondary2DAxisTouchAction);
                    EditorGUILayout.PropertyField(m_PrimaryTouchAction);
                    EditorGUILayout.PropertyField(m_SecondaryTouchAction);
                }
            }
        }
        
        /// <summary>
        /// Draw the property fields related to hand actions.
        /// </summary>
        protected virtual void DrawHandActions()
        {
            EditorGUILayout.LabelField("Hand Actions", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_HandActionAsset);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    m_HandActionsExpanded = EditorGUILayout.Foldout(m_HandActionsExpanded, "Hand Actions", true);
                    if (check.changed)
                        SessionState.SetBool(k_HandActionsExpandedKey, m_HandActionsExpanded);
                }

                if (!m_HandActionsExpanded)
                    return;

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(m_RestingHandExpressionCapture);
                    EditorGUILayout.PropertyField(m_SimulatedHandExpressions);
                }
            }
        }


        /// <summary>
        /// Draw the property fields related to general simulator settings.
        /// </summary>
        protected virtual void DrawGeneralSimulatorSettings()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SimulatorSettingsExpanded = EditorGUILayout.Foldout(m_SimulatorSettingsExpanded, "Simulator Settings", true, Contents.foldoutTitleStyle);
                if (check.changed)
                    SessionState.SetBool(k_SimulatorSettingsExpandedKey, m_SimulatorSettingsExpanded);
            }

            if (!m_SimulatorSettingsExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_CameraTransform);
                EditorGUILayout.PropertyField(m_KeyboardTranslateSpace);
                EditorGUILayout.PropertyField(m_MouseTranslateSpace);
                EditorGUILayout.PropertyField(m_DesiredCursorLockMode);
                EditorGUILayout.PropertyField(m_RemoveOtherHMDDevices);
                EditorGUILayout.PropertyField(m_HandTrackingCapability);
                EditorGUILayout.PropertyField(m_DeviceSimulatorUI);
            }
        }


        /// <summary>
        /// Draw the property fields related to sensitivity settings.
        /// </summary>
        protected virtual void DrawSensitivitySettings()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_SensitivityExpanded = EditorGUILayout.Foldout(m_SensitivityExpanded, "Sensitivity", true, Contents.foldoutTitleStyle);
                if (check.changed)
                    SessionState.SetBool(k_SensitivityExpandedKey, m_SensitivityExpanded);
            }

            if (!m_SensitivityExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_KeyboardXTranslateSpeed);
                EditorGUILayout.PropertyField(m_KeyboardYTranslateSpeed);
                EditorGUILayout.PropertyField(m_KeyboardZTranslateSpeed);
                EditorGUILayout.PropertyField(m_KeyboardBodyTranslateMultiplier);
                EditorGUILayout.PropertyField(m_MouseXTranslateSensitivity);
                EditorGUILayout.PropertyField(m_MouseYTranslateSensitivity);
                EditorGUILayout.PropertyField(m_MouseScrollTranslateSensitivity);
                EditorGUILayout.PropertyField(m_MouseXRotateSensitivity);
                EditorGUILayout.PropertyField(m_MouseYRotateSensitivity);
                EditorGUILayout.PropertyField(m_MouseScrollRotateSensitivity);
                EditorGUILayout.PropertyField(m_MouseYRotateInvert);
            }
        }

        /// <summary>
        /// Draw the property fields related to analog configuration.
        /// </summary>
        protected virtual void DrawAnalogConfigurationSettings()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_AnalogConfigurationExpanded = EditorGUILayout.Foldout(m_AnalogConfigurationExpanded, "Analog Configuration", true, Contents.foldoutTitleStyle);
                if (check.changed)
                    SessionState.SetBool(k_AnalogConfigurationExpandedKey, m_AnalogConfigurationExpanded);
            }

            if (!m_AnalogConfigurationExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_GripAmount);
                EditorGUILayout.PropertyField(m_TriggerAmount);
            }
        }

        /// <summary>
        /// Draw the property fields related to tracking state.
        /// </summary>
        protected virtual void DrawTrackingStateSettings()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_TrackingStateExpanded = EditorGUILayout.Foldout(m_TrackingStateExpanded, "Tracking State", true, Contents.foldoutTitleStyle);
                if (check.changed)
                    SessionState.SetBool(k_TrackingStateExpandedKey, m_TrackingStateExpanded);
            }

            if (!m_TrackingStateExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_HMDIsTracked);
                EditorGUILayout.PropertyField(m_HMDTrackingState);
                EditorGUILayout.PropertyField(m_LeftControllerIsTracked);
                EditorGUILayout.PropertyField(m_LeftControllerTrackingState);
                EditorGUILayout.PropertyField(m_RightControllerIsTracked);
                EditorGUILayout.PropertyField(m_RightControllerTrackingState);
                EditorGUILayout.PropertyField(m_LeftHandIsTracked);
                EditorGUILayout.PropertyField(m_RightHandIsTracked);
            }
        }
    }
}
