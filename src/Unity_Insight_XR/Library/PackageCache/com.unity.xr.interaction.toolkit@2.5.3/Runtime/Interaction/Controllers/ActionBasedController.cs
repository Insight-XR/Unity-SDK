using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
using UnityEngine.InputSystem.XR;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interprets feature values on a tracked input controller device using actions from the Input System
    /// into XR Interaction states, such as Select. Additionally, it applies the current Pose value
    /// of a tracked device to the transform of the GameObject.
    /// </summary>
    /// <remarks>
    /// This behavior requires that the Input System is enabled in the <b>Active Input Handling</b>
    /// setting in <b>Edit &gt; Project Settings &gt; Player</b> for input values to be read.
    /// Each input action must also be enabled to read the current value of the action. Referenced
    /// input actions in an Input Action Asset are not enabled by default.
    /// </remarks>
    /// <seealso cref="XRBaseController"/>
    [AddComponentMenu("XR/XR Controller (Action-based)", 11)]
    [HelpURL(XRHelpURLConstants.k_ActionBasedController)]
    public partial class ActionBasedController : XRBaseController
    {
        [SerializeField]
        InputActionProperty m_PositionAction = new InputActionProperty(new InputAction("Position", expectedControlType: "Vector3"));
        /// <summary>
        /// The Input System action to use for Position Tracking for this GameObject. Must be a <see cref="Vector3Control"/> Control.
        /// </summary>
        public InputActionProperty positionAction
        {
            get => m_PositionAction;
            set => SetInputActionProperty(ref m_PositionAction, value);
        }

        [SerializeField]
        InputActionProperty m_RotationAction = new InputActionProperty(new InputAction("Rotation", expectedControlType: "Quaternion"));
        /// <summary>
        /// The Input System action to use for Rotation Tracking for this GameObject. Must be a <see cref="QuaternionControl"/> Control.
        /// </summary>
        public InputActionProperty rotationAction
        {
            get => m_RotationAction;
            set => SetInputActionProperty(ref m_RotationAction, value);
        }

        [SerializeField]
        InputActionProperty m_IsTrackedAction = new InputActionProperty(new InputAction("Is Tracked", type: InputActionType.Button) { wantsInitialStateCheck = true });
        /// <summary>
        /// The Input System action to read the Is Tracked state when updating this GameObject position and rotation;
        /// falls back to the tracked device's is tracked state that drives the position or rotation action when not set.
        /// Must be an action with a button-like interaction where phase equals performed when is tracked.
        /// Typically a <see cref="ButtonControl"/> Control.
        /// </summary>
        public InputActionProperty isTrackedAction
        {
            get => m_IsTrackedAction;
            set => SetInputActionProperty(ref m_IsTrackedAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_TrackingStateAction = new InputActionProperty(new InputAction("Tracking State", expectedControlType: "Integer"));
        /// <summary>
        /// The Input System action to read the Tracking State when updating this GameObject position and rotation;
        /// falls back to the tracked device's tracking state that drives the position or rotation action when not set.
        /// Must be an <see cref="IntegerControl"/> Control.
        /// </summary>
        /// <seealso cref="InputTrackingState"/>
        public InputActionProperty trackingStateAction
        {
            get => m_TrackingStateAction;
            set => SetInputActionProperty(ref m_TrackingStateAction, value);
        }

        [SerializeField]
        InputActionProperty m_SelectAction = new InputActionProperty(new InputAction("Select", type: InputActionType.Button));
        /// <summary>
        /// The Input System action to use for selecting an Interactable.
        /// Must be an action with a button-like interaction where phase equals performed when pressed.
        /// Typically a <see cref="ButtonControl"/> Control or a Value type action with a Press or Sector interaction.
        /// </summary>
        /// <seealso cref="selectActionValue"/>
        public InputActionProperty selectAction
        {
            get => m_SelectAction;
            set => SetInputActionProperty(ref m_SelectAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_SelectActionValue = new InputActionProperty(new InputAction("Select Value", expectedControlType: "Axis"));
        /// <summary>
        /// The Input System action to read values for selecting an Interactable.
        /// Must be an <see cref="AxisControl"/> Control or <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <remarks>
        /// Optional, Unity uses <see cref="selectAction"/> when not set.
        /// </remarks>
        /// <seealso cref="selectAction"/>
        public InputActionProperty selectActionValue
        {
            get => m_SelectActionValue;
            set => SetInputActionProperty(ref m_SelectActionValue, value);
        }

        [SerializeField]
        InputActionProperty m_ActivateAction = new InputActionProperty(new InputAction("Activate", type: InputActionType.Button));
        /// <summary>
        /// The Input System action to use for activating a selected Interactable.
        /// Must be an action with a button-like interaction where phase equals performed when pressed.
        /// Typically a <see cref="ButtonControl"/> Control or a Value type action with a Press or Sector interaction.
        /// </summary>
        /// <seealso cref="activateActionValue"/>
        public InputActionProperty activateAction
        {
            get => m_ActivateAction;
            set => SetInputActionProperty(ref m_ActivateAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_ActivateActionValue = new InputActionProperty(new InputAction("Activate Value", expectedControlType: "Axis"));
        /// <summary>
        /// The Input System action to read values for activating a selected Interactable.
        /// Must be an <see cref="AxisControl"/> Control or <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <remarks>
        /// Optional, Unity uses <see cref="activateAction"/> when not set.
        /// </remarks>
        /// <seealso cref="activateAction"/>
        public InputActionProperty activateActionValue
        {
            get => m_ActivateActionValue;
            set => SetInputActionProperty(ref m_ActivateActionValue, value);
        }

        [SerializeField]
        InputActionProperty m_UIPressAction = new InputActionProperty(new InputAction("UI Press", type: InputActionType.Button));
        /// <summary>
        /// The Input System action to use for Canvas UI interaction.
        /// Must be an action with a button-like interaction where phase equals performed when pressed.
        /// Typically a <see cref="ButtonControl"/> Control or a Value type action with a Press interaction.
        /// </summary>
        /// <seealso cref="uiPressActionValue"/>
        public InputActionProperty uiPressAction
        {
            get => m_UIPressAction;
            set => SetInputActionProperty(ref m_UIPressAction, value);
        }
        
        [SerializeField]
        InputActionProperty m_UIPressActionValue = new InputActionProperty(new InputAction("UI Press Value", expectedControlType: "Axis"));
        /// <summary>
        /// The Input System action to read values for Canvas UI interaction.
        /// Must be an <see cref="AxisControl"/> Control or <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <remarks>
        /// Optional, Unity uses <see cref="uiPressAction"/> when not set.
        /// </remarks>
        /// <seealso cref="uiPressAction"/>
        public InputActionProperty uiPressActionValue
        {
            get => m_UIPressActionValue;
            set => SetInputActionProperty(ref m_UIPressActionValue, value);
        }

        [SerializeField]
        InputActionProperty m_UIScrollAction = new InputActionProperty(new InputAction("UI Scroll", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to read values for Canvas UI scrolling.
        /// Must be a <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <seealso cref="uiPressAction"/>
        public InputActionProperty uiScrollAction
        {
            get => m_UIScrollAction;
            set => SetInputActionProperty(ref m_UIScrollAction, value);
        }

        [SerializeField]
        InputActionProperty m_HapticDeviceAction = new InputActionProperty(new InputAction("Haptic Device", type: InputActionType.PassThrough));
        /// <summary>
        /// The Input System action to use for identifying the device to send haptic impulses to.
        /// Can be any control type that will have an active control driving the action.
        /// </summary>
        public InputActionProperty hapticDeviceAction
        {
            get => m_HapticDeviceAction;
            set => SetInputActionProperty(ref m_HapticDeviceAction, value);
        }

        [SerializeField]
        InputActionProperty m_RotateAnchorAction = new InputActionProperty(new InputAction("Rotate Anchor", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for rotating the interactor's attach point over time.
        /// Must be a <see cref="Vector2Control"/> Control. Uses the x-axis as the rotation input.
        /// </summary>
        public InputActionProperty rotateAnchorAction
        {
            get => m_RotateAnchorAction;
            set => SetInputActionProperty(ref m_RotateAnchorAction, value);
        }

        [SerializeField]
        InputActionProperty m_DirectionalAnchorRotationAction = new InputActionProperty(new InputAction("Directional Anchor Rotation", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for computing a direction angle to rotate the interactor's attach point to match it.
        /// Must be a <see cref="Vector2Control"/> Control. The direction angle should be computed as the arctangent function of x/y.
        /// </summary>
        public InputActionProperty directionalAnchorRotationAction
        {
            get => m_DirectionalAnchorRotationAction;
            set => SetInputActionProperty(ref m_DirectionalAnchorRotationAction, value);
        }

        [SerializeField]
        InputActionProperty m_TranslateAnchorAction = new InputActionProperty(new InputAction("Translate Anchor", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for translating the interactor's attach point closer or further away from the interactor.
        /// Must be a <see cref="Vector2Control"/> Control. Uses the y-axis as the translation input.
        /// </summary>
        public InputActionProperty translateAnchorAction
        {
            get => m_TranslateAnchorAction;
            set => SetInputActionProperty(ref m_TranslateAnchorAction, value);
        }

        [SerializeField]
        InputActionProperty m_ScaleToggleAction = new InputActionProperty(new InputAction("Scale Toggle", type: InputActionType.Button));
        /// <summary>
        /// The Input System action to use to enable or disable reading from the Scale Delta Action.
        /// Must be a <see cref="ButtonControl"/> Control. The pressed state of the button will toggle the scale state.
        /// </summary>
        /// <seealso cref="scaleDeltaAction"/>
        public InputActionProperty scaleToggleAction
        {
            get => m_ScaleToggleAction;
            set => SetInputActionProperty(ref m_ScaleToggleAction, value);
        }

        [SerializeField]
        InputActionProperty m_ScaleDeltaAction = new InputActionProperty(new InputAction("Scale Delta", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for providing a scale delta value to transformers.
        /// Must be a <see cref="Vector2Control"/> Control. Uses the y-axis as the scale input.
        /// </summary>
        /// <seealso cref="IXRScaleValueProvider"/>
        public InputActionProperty scaleDeltaAction
        {
            get => m_ScaleDeltaAction;
            set => SetInputActionProperty(ref m_ScaleDeltaAction, value);
        }

        bool m_HasCheckedDisabledTrackingInputReferenceActions;
        bool m_HasCheckedDisabledInputReferenceActions;

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            EnableAllDirectActions();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            DisableAllDirectActions();
        }

        /// <inheritdoc />
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);
            if (controllerState == null)
                return;

            var posAction = m_PositionAction.action;
            var rotAction = m_RotationAction.action;
            var isTrackedInputAction = m_IsTrackedAction.action;
            var trackingStateInputAction = m_TrackingStateAction.action;

            // Warn the user if using referenced actions and they are disabled
            if (!m_HasCheckedDisabledTrackingInputReferenceActions && (posAction != null || rotAction != null))
            {
                if (IsDisabledReferenceAction(m_PositionAction) || IsDisabledReferenceAction(m_RotationAction))
                {
                    Debug.LogWarning("'Enable Input Tracking' is enabled, but Position and/or Rotation Action is disabled." +
                        " The pose of the controller will not be updated correctly until the Input Actions are enabled." +
                        " Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action." +
                        " The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.",
                        this);
                }

                m_HasCheckedDisabledTrackingInputReferenceActions = true;
            }

            // Update isTracked and inputTrackingState
            controllerState.isTracked = false;
            controllerState.inputTrackingState = InputTrackingState.None;

            // Actions without bindings are considered empty and will fallback
            if (isTrackedInputAction != null && isTrackedInputAction.bindings.Count > 0)
            {
                controllerState.isTracked = IsPressed(isTrackedInputAction);
            }
            else
            {
                // Fallback: Tracking State > {Position or Rotation when same device, combine otherwise}
                if (trackingStateInputAction?.activeControl?.device is TrackedDevice trackingStateTrackedDevice)
                {
                    controllerState.isTracked = trackingStateTrackedDevice.isTracked.isPressed;
                }
                else
                {
                    var positionTrackedDevice = posAction?.activeControl?.device as TrackedDevice;
                    var rotationTrackedDevice = rotAction?.activeControl?.device as TrackedDevice;

                    var positionIsTracked = positionTrackedDevice?.isTracked.isPressed ?? false;

                    // If the tracking devices are different, their Is Tracked values will be ANDed together
                    if (positionTrackedDevice != rotationTrackedDevice)
                    {
                        var rotationIsTracked = rotationTrackedDevice?.isTracked.isPressed ?? false;
                        controllerState.isTracked = positionIsTracked && rotationIsTracked;
                    }
                    else
                    {
                        controllerState.isTracked = positionIsTracked;
                    }
                }
            }

            // Actions without bindings are considered empty and will fallback
            if (trackingStateInputAction != null && trackingStateInputAction.bindings.Count > 0)
            {
                controllerState.inputTrackingState = (InputTrackingState)trackingStateInputAction.ReadValue<int>();
            }
            else
            {
                // Fallback: Is Tracked > {Position or Rotation when same device, combine otherwise}
                if (isTrackedInputAction?.activeControl?.device is TrackedDevice isTrackedDevice)
                {
                    controllerState.inputTrackingState = (InputTrackingState)isTrackedDevice.trackingState.ReadValue();
                }
                else
                {
                    var positionTrackedDevice = posAction?.activeControl?.device as TrackedDevice;
                    var rotationTrackedDevice = rotAction?.activeControl?.device as TrackedDevice;

                    var positionTrackingState = positionTrackedDevice != null
                        ? (InputTrackingState)positionTrackedDevice.trackingState.ReadValue()
                        : InputTrackingState.None;

                    // If the tracking devices are different only the InputTrackingState.Position and InputTrackingState.Rotation flags will be considered
                    if (positionTrackedDevice != rotationTrackedDevice)
                    {
                        var rotationTrackingState = rotationTrackedDevice != null
                            ? (InputTrackingState)rotationTrackedDevice.trackingState.ReadValue()
                            : InputTrackingState.None;

                        controllerState.inputTrackingState =
                            (positionTrackingState & InputTrackingState.Position) |
                            (rotationTrackingState & InputTrackingState.Rotation);
                    }
                    else
                    {
                        controllerState.inputTrackingState = positionTrackingState;
                    }
                }
            }

            // Update position when valid
            if (posAction != null && (controllerState.inputTrackingState & InputTrackingState.Position) != 0)
            {
                controllerState.position = posAction.ReadValue<Vector3>();
            }

            // Update rotation when valid
            if (rotAction != null && (controllerState.inputTrackingState & InputTrackingState.Rotation) != 0)
            {
                controllerState.rotation = rotAction.ReadValue<Quaternion>();
            }
        }

        /// <inheritdoc />
        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);
            if (controllerState == null)
                return;

            // Warn the user if using referenced actions and they are disabled
            if (!m_HasCheckedDisabledInputReferenceActions &&
                (m_SelectAction.action != null || m_ActivateAction.action != null || m_UIPressAction.action != null))
            {
                if (IsDisabledReferenceAction(m_SelectAction) || IsDisabledReferenceAction(m_ActivateAction) || IsDisabledReferenceAction(m_UIPressAction))
                {
                    Debug.LogWarning("'Enable Input Actions' is enabled, but Select, Activate, and/or UI Press Action is disabled." +
                        " The controller input will not be handled correctly until the Input Actions are enabled." +
                        " Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action." +
                        " The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.",
                        this);
                }

                m_HasCheckedDisabledInputReferenceActions = true;
            }

            controllerState.ResetFrameDependentStates();

            var selectValueAction = m_SelectActionValue.action;
            if (selectValueAction == null || selectValueAction.bindings.Count <= 0)
                selectValueAction = m_SelectAction.action;
            controllerState.selectInteractionState.SetFrameState(IsPressed(m_SelectAction.action), ReadValue(selectValueAction));

            var activateValueAction = m_ActivateActionValue.action;
            if (activateValueAction == null || activateValueAction.bindings.Count <= 0)
                activateValueAction = m_ActivateAction.action;
            controllerState.activateInteractionState.SetFrameState(IsPressed(m_ActivateAction.action), ReadValue(activateValueAction));

            var uiPressValueAction = m_UIPressActionValue.action;
            if (uiPressValueAction == null || uiPressValueAction.bindings.Count <= 0)
                uiPressValueAction = m_UIPressAction.action;
            controllerState.uiPressInteractionState.SetFrameState(IsPressed(m_UIPressAction.action), ReadValue(uiPressValueAction));

            var uiScrollAction = m_UIScrollAction.action;
            if (uiScrollAction != null)
                controllerState.uiScrollValue = uiScrollAction.ReadValue<Vector2>();
        }

        /// <summary>
        /// Evaluates whether the given input action is considered performed.
        /// Unity automatically calls this method during <see cref="UpdateInput"/> to determine
        /// if the interaction state is active this frame.
        /// </summary>
        /// <param name="action">The input action to check.</param>
        /// <returns>Returns <see langword="true"/> when the input action is considered performed. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// More accurately, this evaluates whether the action with a button-like interaction is performed.
        /// Depending on the interaction of the input action, the control driving the value of the input action
        /// may technically be pressed and though the interaction may be in progress, it may not yet be performed,
        /// such as for a Hold interaction. In that example, this method returns <see langword="false"/>.
        /// </remarks>
        /// <seealso cref="InteractionState.active"/>
        protected virtual bool IsPressed(InputAction action)
        {
            if (action == null)
                return false;

#if INPUT_SYSTEM_1_1_OR_NEWER || INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-preview.2 or newer, including pre-release
                return action.phase == InputActionPhase.Performed;
#else
            if (action.activeControl is ButtonControl buttonControl)
                return buttonControl.isPressed;

            if (action.activeControl is AxisControl)
                return action.ReadValue<float>() >= m_ButtonPressPoint;

            return action.triggered || action.phase == InputActionPhase.Performed;
#endif
        }

        /// <summary>
        /// Reads and returns the given action value.
        /// Unity automatically calls this method during <see cref="UpdateInput"/> to determine
        /// the amount or strength of the interaction state this frame.
        /// </summary>
        /// <param name="action">The action to read the value from.</param>
        /// <returns>Returns the action value. If the action is <see langword="null"/> returns the default <see langword="float"/> value (<c>0f</c>).</returns>
        /// <seealso cref="InteractionState.value"/>
        protected virtual float ReadValue(InputAction action)
        {
            if (action == null)
                return default;

            if (action.activeControl is AxisControl)
                return action.ReadValue<float>();

            if (action.activeControl is Vector2Control)
                return action.ReadValue<Vector2>().magnitude;

            return IsPressed(action) ? 1f : 0f;
        }

        /// <inheritdoc />
        public override bool SendHapticImpulse(float amplitude, float duration)
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_HapticDeviceAction.action?.activeControl?.device is XRControllerWithRumble rumbleController)
            {
                rumbleController.SendImpulse(amplitude, duration);
                return true;
            }
#endif

            return false;
        }

        void EnableAllDirectActions()
        {
            m_PositionAction.EnableDirectAction();
            m_RotationAction.EnableDirectAction();
            m_IsTrackedAction.EnableDirectAction();
            m_TrackingStateAction.EnableDirectAction();
            m_SelectAction.EnableDirectAction();
            m_SelectActionValue.EnableDirectAction();
            m_ActivateAction.EnableDirectAction();
            m_ActivateActionValue.EnableDirectAction();
            m_UIPressAction.EnableDirectAction();
            m_UIPressActionValue.EnableDirectAction();
            m_UIScrollAction.EnableDirectAction();
            m_HapticDeviceAction.EnableDirectAction();
            m_RotateAnchorAction.EnableDirectAction();
            m_DirectionalAnchorRotationAction.EnableDirectAction();
            m_TranslateAnchorAction.EnableDirectAction();
            m_ScaleToggleAction.EnableDirectAction();
            m_ScaleDeltaAction.EnableDirectAction();
        }

        void DisableAllDirectActions()
        {
            m_PositionAction.DisableDirectAction();
            m_RotationAction.DisableDirectAction();
            m_IsTrackedAction.DisableDirectAction();
            m_TrackingStateAction.DisableDirectAction();
            m_SelectAction.DisableDirectAction();
            m_SelectActionValue.DisableDirectAction();
            m_ActivateAction.DisableDirectAction();
            m_ActivateActionValue.DisableDirectAction();
            m_UIPressAction.DisableDirectAction();
            m_UIPressActionValue.DisableDirectAction();
            m_UIScrollAction.DisableDirectAction();
            m_HapticDeviceAction.DisableDirectAction();
            m_RotateAnchorAction.DisableDirectAction();
            m_DirectionalAnchorRotationAction.DisableDirectAction();
            m_TranslateAnchorAction.DisableDirectAction();
            m_ScaleToggleAction.DisableDirectAction();
            m_ScaleDeltaAction.DisableDirectAction();
        }

        void SetInputActionProperty(ref InputActionProperty property, InputActionProperty value)
        {
            if (Application.isPlaying)
                property.DisableDirectAction();

            property = value;

            if (Application.isPlaying && isActiveAndEnabled)
                property.EnableDirectAction();
        }

        static bool IsDisabledReferenceAction(InputActionProperty property) =>
            property.reference != null && property.reference.action != null && !property.reference.action.enabled;
    }
}
