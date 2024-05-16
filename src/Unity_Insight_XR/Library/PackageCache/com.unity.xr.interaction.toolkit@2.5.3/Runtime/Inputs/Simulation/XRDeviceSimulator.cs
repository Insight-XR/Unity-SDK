using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands;
using UnityEngine.XR.Hands.ProviderImplementation;
#endif

#if !(ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER))
// Disable warnings about unused fields. This component is not functional when the simulated devices cannot be created,
// but the class signature and SerializeField fields are kept to avoid losing data.
#pragma warning disable 414 // The field 'field' is assigned but its value is never used
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation
{
    /// <summary>
    /// A component which handles mouse and keyboard input from the user and uses it to
    /// drive simulated XR controllers and an XR head mounted display (HMD).
    /// </summary>
    /// <remarks>
    /// This class does not directly manipulate the camera or controllers which are part of
    /// the XR Origin, but rather drives them indirectly through simulated input devices.
    /// <br /><br />
    /// Use the Package Manager window to install the <i>XR Device Simulator</i> sample into
    /// your project to get sample mouse and keyboard bindings for Input System actions that
    /// this component expects. The sample also includes a prefab of a <see cref="GameObject"/>
    /// with this component attached that has references to those sample actions already set.
    /// To make use of this simulator, add the prefab to your scene (the prefab makes use
    /// of <see cref="InputActionManager"/> to ensure the Input System actions are enabled).
    /// <br /><br />
    /// Note that the XR Origin must read the position and rotation of the HMD and controllers
    /// by using Input System actions (such as by using <see cref="ActionBasedController"/>
    /// and <see cref="TrackedPoseDriver"/>) for this simulator to work as expected.
    /// Attempting to use XR input subsystem device methods (such as by using <see cref="XRController"/>
    /// and <see cref="SpatialTracking.TrackedPoseDriver"/>) will not work as expected
    /// since this simulator depends on the Input System to drive the simulated devices.
    /// </remarks>
    /// <seealso cref="XRSimulatedController"/>
    /// <seealso cref="XRSimulatedHMD"/>
    /// <seealso cref="SimulatedInputLayoutLoader"/>
    [AddComponentMenu("XR/Debug/XR Device Simulator", 11)]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_DeviceSimulator)]
    [HelpURL(XRHelpURLConstants.k_XRDeviceSimulator)]
    public class XRDeviceSimulator : MonoBehaviour
    {
        /// <summary>
        /// The maximum angle the XR Camera can have around the X axis.
        /// </summary>
        const float k_CameraMaxXAngle = 80f;

        static readonly Vector3 s_LeftDeviceDefaultInitialPosition = new Vector3(-0.1f, -0.05f, 0.3f);
        static readonly Vector3 s_RightDeviceDefaultInitialPosition = new Vector3(0.1f, -0.05f, 0.3f);

        /// <summary>
        /// The coordinate space in which to operate.
        /// </summary>
        /// <seealso cref="keyboardTranslateSpace"/>
        /// <seealso cref="mouseTranslateSpace"/>
        public enum Space
        {
            /// <summary>
            /// Applies translations of a controller or HMD relative to its own coordinate space, considering its own rotations.
            /// Will translate a controller relative to itself, independent of the camera.
            /// </summary>
            Local,

            /// <summary>
            /// Applies translations of a controller or HMD relative to its parent. If the object does not have a parent, meaning
            /// it is a root object, the parent coordinate space is the same as the world coordinate space. This is the same
            /// as <see cref="Local"/> but without considering its own rotations.
            /// </summary>
            Parent,

            /// <summary>
            /// Applies translations of a controller or HMD relative to the screen.
            /// Will translate a controller relative to the camera, independent of the controller's orientation.
            /// </summary>
            Screen,
        }

        /// <summary>
        /// The transformation mode in which to operate.
        /// </summary>
        /// <seealso cref="mouseTransformationMode"/>
        public enum TransformationMode
        {
            /// <summary>
            /// Applies translations from input.
            /// </summary>
            Translate,

            /// <summary>
            /// Applies rotations from input.
            /// </summary>
            Rotate,
        }

        /// <summary>
        /// The target device or devices to update from input.
        /// </summary>
        /// <remarks>
        /// <see cref="FlagsAttribute"/> to support updating multiple controls from one input
        /// (e.g. to drive a controller and the head from the same input).
        /// </remarks>
        [Flags]
        internal enum TargetedDevices
        {
            /// <summary>
            /// No target device to update.
            /// </summary>
            None = 0,
            
            /// <summary>
            /// No target device, behaving as an FPS controller.
            /// </summary>
            FPS = 1 << 0,

            /// <summary>
            /// Update left controller or hand position and rotation.
            /// </summary>
            LeftDevice = 1 << 1,

            /// <summary>
            /// Update right controller or hand position and rotation.
            /// </summary>
            RightDevice = 1 << 2,

            /// <summary>
            /// Update HMD position and rotation.
            /// </summary>
            HMD = 1 << 3,
        }

        /// <summary>
        /// The device mode of the left and right device.
        /// </summary>
        /// <seealso cref="deviceMode"/>
        public enum DeviceMode
        {
            /// <summary>
            /// Motion controller mode.
            /// </summary>
            Controller,

            /// <summary>
            /// Tracked hand mode.
            /// </summary>
            Hand,
        }

        /// <summary>
        /// The target device control(s) to update from input.
        /// </summary>
        /// <remarks>
        /// <see cref="FlagsAttribute"/> to support updating multiple controls from input
        /// (e.g. to drive the primary and secondary 2D axis on a controller from the same input).
        /// </remarks>
        /// <seealso cref="axis2DTargets"/>
        [Flags]
        public enum Axis2DTargets
        {
            /// <summary>
            /// Do not update device state from input.
            /// </summary>
            None = 0,

            /// <summary>
            /// Update device position from input.
            /// </summary>
            Position = 1 << 0,

            /// <summary>
            /// Update the primary touchpad or joystick on a controller device from input.
            /// </summary>
            Primary2DAxis = 1 << 1,

            /// <summary>
            /// Update the secondary touchpad or joystick on a controller device from input.
            /// </summary>
            Secondary2DAxis = 1 << 2,
        }

        /// <summary>
        /// A hand expression that can be simulated by performing an input action.
        /// </summary>
        [Serializable]
        public class SimulatedHandExpression : ISerializationCallbackReceiver
        {
            [SerializeField]
            [Tooltip("The unique name for the hand expression.")]
            [Delayed]
            string m_Name;

            /// <summary>
            /// The name of the hand expression to simulate when the input action is performed.
            /// </summary>
            public string name => m_ExpressionName.ToString();

            [SerializeField]
            [Tooltip("The input action to trigger the hand expression.")]
            InputActionReference m_ToggleAction;

            /// <summary>
            /// The input action reference to trigger this simulated hand expression.
            /// </summary>
            public InputActionReference toggleAction => m_ToggleAction;

            [SerializeField]
            [Tooltip("The captured hand expression to simulate when the input action is performed.")]
            HandExpressionCapture m_Capture;

            /// <summary>
            /// The captured expression to simulate when the input action is performed.
            /// </summary>
            internal HandExpressionCapture capture
            {
                get => m_Capture;
                set => m_Capture = value;
            }

            HandExpressionName m_ExpressionName;

            /// <summary>
            /// The name of the hand expression to simulate when the input action is performed.
            /// Use this for a faster name identifier than comparing by <see cref="string"/> name.
            /// </summary>
            internal HandExpressionName expressionName
            {
                get => m_ExpressionName;
                set => m_ExpressionName = value;
            }

            /// <summary>
            /// Sprite icon for the simulated hand expression.
            /// </summary>
            public Sprite icon => m_Capture.icon;

            Action<SimulatedHandExpression, InputAction.CallbackContext> m_Performed;

            /// <summary>
            /// Event that is called when the input action for the simulated hand expression is performed.
            /// </summary>
            /// <remarks>
            /// Wraps the performed action of the <see cref="toggleAction"/> in order to add a reference
            /// to this class in the callback method signature.
            /// </remarks>
            public event Action<SimulatedHandExpression, InputAction.CallbackContext> performed
            {
                add
                {
                    m_Performed += value;
                    if (!m_Subscribed)
                    {
                        m_Subscribed = true;
                        m_ToggleAction.action.performed += OnActionPerformed;
                    }
                }
                remove
                {
                    m_Performed -= value;
                    if (m_Performed == null)
                    {
                        m_Subscribed = false;
                        m_ToggleAction.action.performed -= OnActionPerformed;
                    }
                }
            }

            bool m_Subscribed;

            /// <inheritdoc/>
            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
                m_Name = m_ExpressionName.ToString();
            }

            /// <inheritdoc/>
            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                m_ExpressionName = new HandExpressionName(m_Name);
            }

            void OnActionPerformed(InputAction.CallbackContext context)
            {
                m_Performed?.Invoke(this, context);
            }
        }
        
        [SerializeField]
        [Tooltip("Input Action asset containing controls for the simulator itself. Unity will automatically enable and disable it with this component.")]
        InputActionAsset m_DeviceSimulatorActionAsset;
        /// <summary>
        /// Input Action asset containing controls for the simulator itself. Unity will automatically enable and disable it with this component.
        /// </summary>
        public InputActionAsset deviceSimulatorActionAsset
        {
            get => m_DeviceSimulatorActionAsset;
            set => m_DeviceSimulatorActionAsset = value;
        }

        [SerializeField]
        [Tooltip("Input Action asset containing controls for the simulated controllers. Unity will automatically enable and disable it as needed.")]
        InputActionAsset m_ControllerActionAsset;
        /// <summary>
        /// Input Action asset containing controls for the simulated controllers. Unity will automatically enable and disable it as needed.
        /// </summary>
        public InputActionAsset controllerActionAsset
        {
            get => m_ControllerActionAsset;
            set => m_ControllerActionAsset = value;
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the x-axis (left/right) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardXTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the x-axis (left/right) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardXTranslateAction
        {
            get => m_KeyboardXTranslateAction;
            set
            {
                UnsubscribeKeyboardXTranslateAction();
                m_KeyboardXTranslateAction = value;
                SubscribeKeyboardXTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the y-axis (up/down) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardYTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the y-axis (up/down) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardYTranslateAction
        {
            get => m_KeyboardYTranslateAction;
            set
            {
                UnsubscribeKeyboardYTranslateAction();
                m_KeyboardYTranslateAction = value;
                SubscribeKeyboardYTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the z-axis (forward/back) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardZTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the z-axis (forward/back) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardZTranslateAction
        {
            get => m_KeyboardZTranslateAction;
            set
            {
                UnsubscribeKeyboardZTranslateAction();
                m_KeyboardZTranslateAction = value;
                SubscribeKeyboardZTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to enable manipulation of the left-hand controller while held. Must be a Button Control.")]
        InputActionReference m_ManipulateLeftAction;
        /// <summary>
        /// The Input System Action used to enable manipulation of the left-hand controller while held.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <remarks>
        /// Note that if controls on the left-hand controller are actuated when this action is released,
        /// those controls will continue to remain actuated. This is to allow for multi-hand interactions
        /// without needing to have dedicated bindings for manipulating each controller separately and concurrently.
        /// </remarks>
        /// <seealso cref="manipulateRightAction"/>
        /// <seealso cref="toggleManipulateLeftAction"/>
        public InputActionReference manipulateLeftAction
        {
            get => m_ManipulateLeftAction;
            set
            {
                UnsubscribeManipulateLeftAction();
                m_ManipulateLeftAction = value;
                SubscribeManipulateLeftAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to enable manipulation of the right-hand controller while held. Must be a Button Control.")]
        InputActionReference m_ManipulateRightAction;
        /// <summary>
        /// The Input System Action used to enable manipulation of the right-hand controller while held.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <remarks>
        /// Note that if controls on the right-hand controller are actuated when this action is released,
        /// those controls will continue to remain actuated. This is to allow for multi-hand interactions
        /// without needing to have dedicated bindings for manipulating each controller separately and concurrently.
        /// </remarks>
        /// <seealso cref="manipulateLeftAction"/>
        public InputActionReference manipulateRightAction
        {
            get => m_ManipulateRightAction;
            set
            {
                UnsubscribeManipulateRightAction();
                m_ManipulateRightAction = value;
                SubscribeManipulateRightAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of the left-hand controller when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleManipulateLeftAction;
        /// <summary>
        /// The Input System Action used to toggle enable manipulation of the left-hand controller when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="manipulateLeftAction"/>
        /// <seealso cref="toggleManipulateRightAction"/>
        public InputActionReference toggleManipulateLeftAction
        {
            get => m_ToggleManipulateLeftAction;
            set
            {
                UnsubscribeToggleManipulateLeftAction();
                m_ToggleManipulateLeftAction = value;
                SubscribeToggleManipulateLeftAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of the right-hand controller when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleManipulateRightAction;
        /// <summary>
        /// The Input System Action used to toggle enable manipulation of the right-hand controller when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="manipulateRightAction"/>
        /// <seealso cref="toggleManipulateLeftAction"/>
        public InputActionReference toggleManipulateRightAction
        {
            get => m_ToggleManipulateRightAction;
            set
            {
                UnsubscribeToggleManipulateRightAction();
                m_ToggleManipulateRightAction = value;
                SubscribeToggleManipulateRightAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable looking around with the HMD and controllers. Must be a Button Control.")]
        InputActionReference m_ToggleManipulateBodyAction;
        /// <summary>
        /// The Input System Action used to toggle enable looking around with the HMD and controllers.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference toggleManipulateBodyAction
        {
            get => m_ToggleManipulateBodyAction;
            set
            {
                UnsubscribeToggleManipulateBodyAction();
                m_ToggleManipulateBodyAction = value;
                SubscribeToggleManipulateBodyAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to enable manipulation of the HMD while held. Must be a Button Control.")]
        InputActionReference m_ManipulateHeadAction;
        /// <summary>
        /// The Input System Action used to enable manipulation of the HMD while held.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference manipulateHeadAction
        {
            get => m_ManipulateHeadAction;
            set
            {
                UnsubscribeManipulateHeadAction();
                m_ManipulateHeadAction = value;
                SubscribeManipulateHeadAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to change between hand and controller mode. Must be a Button Control.")]
        InputActionReference m_HandControllerModeAction;
        /// <summary>
        /// The Input System Action used to change between hand and controller mode.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference handControllerModeAction
        {
            get => m_HandControllerModeAction;
            set
            {
                UnsubscribeHandControllerModeAction();
                m_HandControllerModeAction = value;
                SubscribeHandControllerModeAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to cycle between the different available devices. Must be a Button Control.")]
        InputActionReference m_CycleDevicesAction;
        /// <summary>
        /// The Input System Action used to cycle between the different available devices.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="manipulateHeadAction"/>
        /// <seealso cref="manipulateLeftAction"/>
        /// <seealso cref="manipulateRightAction"/>
        public InputActionReference cycleDevicesAction
        {
            get => m_CycleDevicesAction;
            set
            {
                UnsubscribeCycleDevicesAction();
                m_CycleDevicesAction = value;
                SubscribeCycleDevicesAction();
            }
        }
        
        [SerializeField]
        [Tooltip("The Input System Action used to stop all manipulation. Must be a Button Control.")]
        InputActionReference m_StopManipulationAction;
        /// <summary>
        /// The Input System Action used to stop all manipulation.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference stopManipulationAction
        {
            get => m_StopManipulationAction;
            set
            {
                UnsubscribeStopManipulationAction();
                m_StopManipulationAction = value;
                SubscribeStopManipulationAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes. Must be a Value Vector2 Control.")]
        InputActionReference m_MouseDeltaAction;
        /// <summary>
        /// The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to the screen-space motion delta of the mouse in pixels.
        /// </remarks>
        /// <seealso cref="mouseScrollAction"/>
        public InputActionReference mouseDeltaAction
        {
            get => m_MouseDeltaAction;
            set
            {
                UnsubscribeMouseDeltaAction();
                m_MouseDeltaAction = value;
                SubscribeMouseDeltaAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the z-axis. Must be a Value Vector2 Control.")]
        InputActionReference m_MouseScrollAction;
        /// <summary>
        /// The Input System Action used to translate or rotate by a scaled amount along or about the z-axis.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to the horizontal and vertical scroll wheels, though only the vertical is used.
        /// </remarks>
        /// <seealso cref="mouseDeltaAction"/>
        public InputActionReference mouseScrollAction
        {
            get => m_MouseScrollAction;
            set
            {
                UnsubscribeMouseScrollAction();
                m_MouseScrollAction = value;
                SubscribeMouseScrollAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to cause the manipulated device(s) to rotate when moving the mouse when held. Must be a Button Control.")]
        InputActionReference m_RotateModeOverrideAction;
        /// <summary>
        /// The Input System Action used to cause the manipulated device(s) to rotate when moving the mouse when held.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <remarks>
        /// Forces rotation mode when held, no matter what the current mouse transformation mode is.
        /// </remarks>
        /// <seealso cref="negateModeAction"/>
        public InputActionReference rotateModeOverrideAction
        {
            get => m_RotateModeOverrideAction;
            set
            {
                UnsubscribeRotateModeOverrideAction();
                m_RotateModeOverrideAction = value;
                SubscribeRotateModeOverrideAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle between translating or rotating the manipulated device(s) when moving the mouse when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleMouseTransformationModeAction;
        /// <summary>
        /// The Input System Action used to toggle between translating or rotating the manipulated device(s)
        /// when moving the mouse when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference toggleMouseTransformationModeAction
        {
            get => m_ToggleMouseTransformationModeAction;
            set
            {
                UnsubscribeToggleMouseTransformationModeAction();
                m_ToggleMouseTransformationModeAction = value;
                SubscribeToggleMouseTransformationModeAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to cause the manipulated device(s) to rotate when moving the mouse while held when it would normally translate, and vice-versa. Must be a Button Control.")]
        InputActionReference m_NegateModeAction;
        /// <summary>
        /// The Input System Action used to cause the manipulated device(s) to rotate when moving the mouse
        /// while held when it would normally translate, and vice-versa.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <remarks>
        /// Can be used to temporarily change the mouse transformation mode to the other mode while held
        /// for making quick adjustments.
        /// </remarks>
        /// <seealso cref="toggleMouseTransformationModeAction"/>
        public InputActionReference negateModeAction
        {
            get => m_NegateModeAction;
            set
            {
                UnsubscribeNegateModeAction();
                m_NegateModeAction = value;
                SubscribeNegateModeAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to constrain the translation or rotation to the x-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
        InputActionReference m_XConstraintAction;
        /// <summary>
        /// The Input System Action used to constrain the translation or rotation to the x-axis when moving the mouse or resetting.
        /// May be combined with another axis constraint to constrain to a plane.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="yConstraintAction"/>
        /// <seealso cref="zConstraintAction"/>
        public InputActionReference xConstraintAction
        {
            get => m_XConstraintAction;
            set
            {
                UnsubscribeXConstraintAction();
                m_XConstraintAction = value;
                SubscribeXConstraintAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to constrain the translation or rotation to the y-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
        InputActionReference m_YConstraintAction;
        /// <summary>
        /// The Input System Action used to constrain the translation or rotation to the y-axis when moving the mouse or resetting.
        /// May be combined with another axis constraint to constrain to a plane.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="xConstraintAction"/>
        /// <seealso cref="zConstraintAction"/>
        public InputActionReference yConstraintAction
        {
            get => m_YConstraintAction;
            set
            {
                UnsubscribeYConstraintAction();
                m_YConstraintAction = value;
                SubscribeYConstraintAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to constrain the translation or rotation to the z-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
        InputActionReference m_ZConstraintAction;
        /// <summary>
        /// The Input System Action used to constrain the translation or rotation to the z-axis when moving the mouse or resetting.
        /// May be combined with another axis constraint to constrain to a plane.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="xConstraintAction"/>
        /// <seealso cref="yConstraintAction"/>
        public InputActionReference zConstraintAction
        {
            get => m_ZConstraintAction;
            set
            {
                UnsubscribeZConstraintAction();
                m_ZConstraintAction = value;
                SubscribeZConstraintAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to cause the manipulated device(s) to reset position or rotation (depending on the effective manipulation mode). Must be a Button Control.")]
        InputActionReference m_ResetAction;
        /// <summary>
        /// The Input System Action used to cause the manipulated device(s) to reset position or rotation
        /// (depending on the effective manipulation mode).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <remarks>
        /// Resets position to <see cref="Vector3.zero"/> and rotation to <see cref="Quaternion.identity"/>.
        /// May be combined with axis constraints (<see cref="xConstraintAction"/>, <see cref="yConstraintAction"/>, and <see cref="zConstraintAction"/>).
        /// </remarks>
        public InputActionReference resetAction
        {
            get => m_ResetAction;
            set
            {
                UnsubscribeResetAction();
                m_ResetAction = value;
                SubscribeResetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle the cursor lock mode for the game window when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleCursorLockAction;
        /// <summary>
        /// The Input System Action used to toggle the cursor lock mode for the game window when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="Cursor.lockState"/>
        /// <seealso cref="desiredCursorLockMode"/>
        public InputActionReference toggleCursorLockAction
        {
            get => m_ToggleCursorLockAction;
            set
            {
                UnsubscribeToggleCursorLockAction();
                m_ToggleCursorLockAction = value;
                SubscribeToggleCursorLockAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable translation from keyboard inputs when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleDevicePositionTargetAction;
        /// <summary>
        /// The Input System Action used to toggle enable translation from keyboard inputs when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        public InputActionReference toggleDevicePositionTargetAction
        {
            get => m_ToggleDevicePositionTargetAction;
            set
            {
                UnsubscribeToggleDevicePositionTargetAction();
                m_ToggleDevicePositionTargetAction = value;
                SubscribeToggleDevicePositionTargetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of the Primary2DAxis of the controllers when pressed. Must be a Button Control.")]
        InputActionReference m_TogglePrimary2DAxisTargetAction;
        /// <summary>
        /// The Input System action used to toggle enable manipulation of the <see cref="Axis2DTargets.Primary2DAxis"/> of the controllers when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="toggleSecondary2DAxisTargetAction"/>
        /// <seealso cref="toggleDevicePositionTargetAction"/>
        /// <seealso cref="axis2DAction"/>
        public InputActionReference togglePrimary2DAxisTargetAction
        {
            get => m_TogglePrimary2DAxisTargetAction;
            set
            {
                UnsubscribeTogglePrimary2DAxisTargetAction();
                m_TogglePrimary2DAxisTargetAction = value;
                SubscribeTogglePrimary2DAxisTargetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of the Secondary2DAxis of the controllers when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleSecondary2DAxisTargetAction;
        /// <summary>
        /// The Input System action used to toggle enable manipulation of the <see cref="Axis2DTargets.Secondary2DAxis"/> of the controllers when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="togglePrimary2DAxisTargetAction"/>
        /// <seealso cref="toggleDevicePositionTargetAction"/>
        /// <seealso cref="axis2DAction"/>
        public InputActionReference toggleSecondary2DAxisTargetAction
        {
            get => m_ToggleSecondary2DAxisTargetAction;
            set
            {
                UnsubscribeToggleSecondary2DAxisTargetAction();
                m_ToggleSecondary2DAxisTargetAction = value;
                SubscribeToggleSecondary2DAxisTargetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the value of one or more 2D Axis controls on the manipulated controller device(s). Must be a Value Vector2 Control.")]
        InputActionReference m_Axis2DAction;
        /// <summary>
        /// The Input System Action used to control the value of one or more 2D Axis controls on the manipulated controller device(s).
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="togglePrimary2DAxisTargetAction"/> and <see cref="toggleSecondary2DAxisTargetAction"/> toggle enables
        /// the ability to manipulate 2D Axis controls on the simulated controllers, and this <see cref="axis2DAction"/>
        /// actually controls the value of them while those controller devices are being manipulated.
        /// <br />
        /// Typically bound to WASD on a keyboard, and controls the primary and/or secondary 2D Axis controls on them.
        /// </remarks>
        public InputActionReference axis2DAction
        {
            get => m_Axis2DAction;
            set
            {
                UnsubscribeAxis2DAction();
                m_Axis2DAction = value;
                SubscribeAxis2DAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control one or more 2D Axis controls on the opposite hand of the exclusively manipulated controller device. Must be a Value Vector2 Control.")]
        InputActionReference m_RestingHandAxis2DAction;
        /// <summary>
        /// The Input System Action used to control one or more 2D Axis controls on the opposite hand
        /// of the exclusively manipulated controller device.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to Q and E on a keyboard for the horizontal component, and controls the opposite hand's
        /// 2D Axis controls when manipulating one (and only one) controller. Can be used to quickly and simultaneously
        /// control the 2D Axis on the other hand's controller. In a typical setup of continuous movement bound on the left-hand
        /// controller stick, and turning bound on the right-hand controller stick, while exclusively manipulating the left-hand
        /// controller to move, this action can be used to trigger turning.
        /// </remarks>
        /// <seealso cref="axis2DAction"/>
        public InputActionReference restingHandAxis2DAction
        {
            get => m_RestingHandAxis2DAction;
            set
            {
                UnsubscribeRestingHandAxis2DAction();
                m_RestingHandAxis2DAction = value;
                SubscribeRestingHandAxis2DAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Grip control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_GripAction;
        /// <summary>
        /// The Input System Action used to control the Grip control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference gripAction
        {
            get => m_GripAction;
            set
            {
                UnsubscribeGripAction();
                m_GripAction = value;
                SubscribeGripAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Trigger control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_TriggerAction;
        /// <summary>
        /// The Input System Action used to control the Trigger control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference triggerAction
        {
            get => m_TriggerAction;
            set
            {
                UnsubscribeTriggerAction();
                m_TriggerAction = value;
                SubscribeTriggerAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the PrimaryButton control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_PrimaryButtonAction;
        /// <summary>
        /// The Input System Action used to control the PrimaryButton control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primaryButtonAction
        {
            get => m_PrimaryButtonAction;
            set
            {
                UnsubscribePrimaryButtonAction();
                m_PrimaryButtonAction = value;
                SubscribePrimaryButtonAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the SecondaryButton control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_SecondaryButtonAction;
        /// <summary>
        /// The Input System Action used to control the SecondaryButton control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondaryButtonAction
        {
            get => m_SecondaryButtonAction;
            set
            {
                UnsubscribeSecondaryButtonAction();
                m_SecondaryButtonAction = value;
                SubscribeSecondaryButtonAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Menu control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_MenuAction;
        /// <summary>
        /// The Input System Action used to control the Menu control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference menuAction
        {
            get => m_MenuAction;
            set
            {
                UnsubscribeMenuAction();
                m_MenuAction = value;
                SubscribeMenuAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Primary2DAxisClick control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Primary2DAxisClickAction;
        /// <summary>
        /// The Input System Action used to control the Primary2DAxisClick control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primary2DAxisClickAction
        {
            get => m_Primary2DAxisClickAction;
            set
            {
                UnsubscribePrimary2DAxisClickAction();
                m_Primary2DAxisClickAction = value;
                SubscribePrimary2DAxisClickAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Secondary2DAxisClick control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Secondary2DAxisClickAction;
        /// <summary>
        /// The Input System Action used to control the Secondary2DAxisClick control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondary2DAxisClickAction
        {
            get => m_Secondary2DAxisClickAction;
            set
            {
                UnsubscribeSecondary2DAxisClickAction();
                m_Secondary2DAxisClickAction = value;
                SubscribeSecondary2DAxisClickAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Primary2DAxisTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Primary2DAxisTouchAction;
        /// <summary>
        /// The Input System Action used to control the Primary2DAxisTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primary2DAxisTouchAction
        {
            get => m_Primary2DAxisTouchAction;
            set
            {
                UnsubscribePrimary2DAxisTouchAction();
                m_Primary2DAxisTouchAction = value;
                SubscribePrimary2DAxisTouchAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Secondary2DAxisTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Secondary2DAxisTouchAction;
        /// <summary>
        /// The Input System Action used to control the Secondary2DAxisTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondary2DAxisTouchAction
        {
            get => m_Secondary2DAxisTouchAction;
            set
            {
                UnsubscribeSecondary2DAxisTouchAction();
                m_Secondary2DAxisTouchAction = value;
                SubscribeSecondary2DAxisTouchAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the PrimaryTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_PrimaryTouchAction;
        /// <summary>
        /// The Input System Action used to control the PrimaryTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primaryTouchAction
        {
            get => m_PrimaryTouchAction;
            set
            {
                UnsubscribePrimaryTouchAction();
                m_PrimaryTouchAction = value;
                SubscribePrimaryTouchAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the SecondaryTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_SecondaryTouchAction;
        /// <summary>
        /// The Input System Action used to control the SecondaryTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondaryTouchAction
        {
            get => m_SecondaryTouchAction;
            set
            {
                UnsubscribeSecondaryTouchAction();
                m_SecondaryTouchAction = value;
                SubscribeSecondaryTouchAction();
            }
        }
        
        [SerializeField]
        [Tooltip("Input Action asset containing controls for the simulated hands. Unity will automatically enable and disable it as needed.")]
        InputActionAsset m_HandActionAsset;
        /// <summary>
        /// Input Action asset containing controls for the simulated hands. Unity will automatically enable and disable it as needed.
        /// </summary>
        public InputActionAsset handActionAsset
        {
            get => m_HandActionAsset;
            set => m_HandActionAsset = value;
        }

        [SerializeField]
        [Tooltip("The resting hand expression to use when no other hand expression is active.")]
        HandExpressionCapture m_RestingHandExpressionCapture;

        [SerializeField]
        [Tooltip("The list of hand expressions to simulate.")]
        List<SimulatedHandExpression> m_SimulatedHandExpressions = new List<SimulatedHandExpression>();

        /// <summary>
        /// The list of simulated hand expressions for the device simulator.
        /// </summary>
        public List<SimulatedHandExpression> simulatedHandExpressions => m_SimulatedHandExpressions;

        [SerializeField]
        [Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset.")]
        Transform m_CameraTransform;
        /// <summary>
        /// The <see cref="Transform"/> that contains the <see cref="Camera"/>. This is usually the "Head" of XR Origins.
        /// Automatically set to <see cref="Camera.main"/> if unset.
        /// </summary>
        public Transform cameraTransform
        {
            get => m_CameraTransform;
            set => m_CameraTransform = value;
        }

        [SerializeField]
        [Tooltip("The coordinate space in which keyboard translation should operate.")]
        Space m_KeyboardTranslateSpace = Space.Local;
        /// <summary>
        /// The coordinate space in which keyboard translation should operate.
        /// </summary>
        /// <seealso cref="Space"/>
        /// <seealso cref="mouseTranslateSpace"/>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        public Space keyboardTranslateSpace
        {
            get => m_KeyboardTranslateSpace;
            set => m_KeyboardTranslateSpace = value;
        }

        [SerializeField]
        [Tooltip("The coordinate space in which mouse translation should operate.")]
        Space m_MouseTranslateSpace = Space.Screen;
        /// <summary>
        /// The coordinate space in which mouse translation should operate.
        /// </summary>
        /// <seealso cref="Space"/>
        /// <seealso cref="keyboardTranslateSpace"/>
        public Space mouseTranslateSpace
        {
            get => m_MouseTranslateSpace;
            set => m_MouseTranslateSpace = value;
        }

        [SerializeField]
        [Tooltip("Speed of translation in the x-axis (left/right) when triggered by keyboard input.")]
        float m_KeyboardXTranslateSpeed = 0.2f;
        /// <summary>
        /// Speed of translation in the x-axis (left/right) when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateSpeed"/>
        /// <seealso cref="keyboardZTranslateSpeed"/>
        public float keyboardXTranslateSpeed
        {
            get => m_KeyboardXTranslateSpeed;
            set => m_KeyboardXTranslateSpeed = value;
        }

        [SerializeField]
        [Tooltip("Speed of translation in the y-axis (up/down) when triggered by keyboard input.")]
        float m_KeyboardYTranslateSpeed = 0.2f;
        /// <summary>
        /// Speed of translation in the y-axis (up/down) when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardXTranslateSpeed"/>
        /// <seealso cref="keyboardZTranslateSpeed"/>
        public float keyboardYTranslateSpeed
        {
            get => m_KeyboardYTranslateSpeed;
            set => m_KeyboardYTranslateSpeed = value;
        }

        [SerializeField]
        [Tooltip("Speed of translation in the z-axis (forward/back) when triggered by keyboard input.")]
        float m_KeyboardZTranslateSpeed = 0.2f;
        /// <summary>
        /// Speed of translation in the z-axis (forward/back) when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardZTranslateAction"/>
        /// <seealso cref="keyboardXTranslateSpeed"/>
        /// <seealso cref="keyboardYTranslateSpeed"/>
        public float keyboardZTranslateSpeed
        {
            get => m_KeyboardZTranslateSpeed;
            set => m_KeyboardZTranslateSpeed = value;
        }

        [SerializeField]
        [Tooltip("Speed multiplier applied for body translation when triggered by keyboard input.")]
        float m_KeyboardBodyTranslateMultiplier = 5f;
        /// <summary>
        /// Speed multiplier applied for body translation when triggered by keyboard input.
        /// </summary>
        /// <seealso cref="keyboardXTranslateSpeed"/>
        /// <seealso cref="keyboardYTranslateSpeed"/>
        /// <seealso cref="keyboardZTranslateSpeed"/>
        public float keyboardBodyTranslateMultiplier
        {
            get => m_KeyboardBodyTranslateMultiplier;
            set => m_KeyboardBodyTranslateMultiplier = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of translation in the x-axis (left/right) when triggered by mouse input.")]
        float m_MouseXTranslateSensitivity = 0.0004f;
        /// <summary>
        /// Sensitivity of translation in the x-axis (left/right) when triggered by mouse input.
        /// </summary>
        /// <seealso cref="mouseDeltaAction"/>
        /// <seealso cref="mouseYTranslateSensitivity"/>
        /// <seealso cref="mouseScrollTranslateSensitivity"/>
        public float mouseXTranslateSensitivity
        {
            get => m_MouseXTranslateSensitivity;
            set => m_MouseXTranslateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of translation in the y-axis (up/down) when triggered by mouse input.")]
        float m_MouseYTranslateSensitivity = 0.0004f;
        /// <summary>
        /// Sensitivity of translation in the y-axis (up/down) when triggered by mouse input.
        /// </summary>
        /// <seealso cref="mouseDeltaAction"/>
        /// <seealso cref="mouseXTranslateSensitivity"/>
        /// <seealso cref="mouseScrollTranslateSensitivity"/>
        public float mouseYTranslateSensitivity
        {
            get => m_MouseYTranslateSensitivity;
            set => m_MouseYTranslateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of translation in the z-axis (forward/back) when triggered by mouse scroll input.")]
        float m_MouseScrollTranslateSensitivity = 0.0002f;
        /// <summary>
        /// Sensitivity of translation in the z-axis (forward/back) when triggered by mouse scroll input.
        /// </summary>
        /// <seealso cref="mouseScrollAction"/>
        /// <seealso cref="mouseXTranslateSensitivity"/>
        /// <seealso cref="mouseYTranslateSensitivity"/>
        public float mouseScrollTranslateSensitivity
        {
            get => m_MouseScrollTranslateSensitivity;
            set => m_MouseScrollTranslateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.")]
        float m_MouseXRotateSensitivity = 0.2f;
        /// <summary>
        /// Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.
        /// </summary>
        /// <seealso cref="mouseDeltaAction"/>
        /// <seealso cref="mouseYRotateSensitivity"/>
        /// <seealso cref="mouseScrollRotateSensitivity"/>
        public float mouseXRotateSensitivity
        {
            get => m_MouseXRotateSensitivity;
            set => m_MouseXRotateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.")]
        float m_MouseYRotateSensitivity = 0.2f;
        /// <summary>
        /// Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.
        /// </summary>
        /// <seealso cref="mouseDeltaAction"/>
        /// <seealso cref="mouseXRotateSensitivity"/>
        /// <seealso cref="mouseScrollRotateSensitivity"/>
        public float mouseYRotateSensitivity
        {
            get => m_MouseYRotateSensitivity;
            set => m_MouseYRotateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("Sensitivity of rotation along the z-axis (roll) when triggered by mouse scroll input.")]
        float m_MouseScrollRotateSensitivity = 0.05f;
        /// <summary>
        /// Sensitivity of rotation along the z-axis (roll) when triggered by mouse scroll input.
        /// </summary>
        /// <seealso cref="mouseScrollAction"/>
        /// <seealso cref="mouseXRotateSensitivity"/>
        /// <seealso cref="mouseYRotateSensitivity"/>
        public float mouseScrollRotateSensitivity
        {
            get => m_MouseScrollRotateSensitivity;
            set => m_MouseScrollRotateSensitivity = value;
        }

        [SerializeField]
        [Tooltip("A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input." +
            "\nA false value (default) means typical FPS style where moving the mouse up/down pitches up/down." +
            "\nA true value means flight control style where moving the mouse up/down pitches down/up.")]
        bool m_MouseYRotateInvert;
        /// <summary>
        /// A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input.
        /// A <see langword="false"/> value (default) means typical FPS style where moving the mouse up/down pitches up/down.
        /// A <see langword="true"/> value means flight control style where moving the mouse up/down pitches down/up.
        /// </summary>
        public bool mouseYRotateInvert
        {
            get => m_MouseYRotateInvert;
            set => m_MouseYRotateInvert = value;
        }

        [SerializeField]
        [Tooltip("The desired cursor lock mode to toggle to from None (either Locked or Confined).")]
        CursorLockMode m_DesiredCursorLockMode = CursorLockMode.Locked;
        /// <summary>
        /// The desired cursor lock mode to toggle to from <see cref="CursorLockMode.None"/>
        /// (either <see cref="CursorLockMode.Locked"/> or <see cref="CursorLockMode.Confined"/>).
        /// </summary>
        /// <seealso cref="toggleCursorLockAction"/>
        public CursorLockMode desiredCursorLockMode
        {
            get => m_DesiredCursorLockMode;
            set => m_DesiredCursorLockMode = value;
        }

        [SerializeField]
        [Tooltip("Whether or not to remove other XR HMD devices in this session so that they don't conflict with the XR Device Simulator.")]
        bool m_RemoveOtherHMDDevices = true;
        /// <summary>
        /// This boolean value indicates whether or not we remove other <see cref="XRHMD"/> devices in this session so that they don't conflict with the <see cref="XRDeviceSimulator"/>.
        /// A <see langword="true"/> value (default) means we remove all other <see cref="XRHMD"/> devices except the <see cref="XRSimulatedHMD"/> generated by the <see cref="XRDeviceSimulator"/>.
        /// A <see langword="false"/> value means we do not remove any other <see cref="XRHMD"/> devices.
        /// </summary>
        public bool removeOtherHMDDevices
        {
            get => m_RemoveOtherHMDDevices;
            set => m_RemoveOtherHMDDevices = value;
        }

        [SerializeField]
        [Tooltip("Whether to create a simulated Hand Tracking Subsystem and provider on startup. Requires the XR Hands package.")]
        bool m_HandTrackingCapability = true;
        /// <summary>
        /// Whether to create a simulated Hand Tracking Subsystem and provider on startup. Requires the XR Hands package.
        /// </summary>
        public bool handTrackingCapability
        {
            get => m_HandTrackingCapability;
            set => m_HandTrackingCapability = value;
        }

        [SerializeField]
        [Tooltip("The optional Device Simulator UI prefab to use along with the XR Device Simulator.")]
        GameObject m_DeviceSimulatorUI;
        /// <summary>
        /// The optional Device Simulator UI prefab to use along with the XR Device Simulator.
        /// </summary>
        public GameObject deviceSimulatorUI
        {
            get => m_DeviceSimulatorUI;
            set => m_DeviceSimulatorUI = value;
        }

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of the simulated grip on the controller when the Grip control is pressed.")]
        float m_GripAmount = 1f;
        /// <summary>
        /// The amount of the simulated grip on the controller when the Grip control is pressed.
        /// </summary>
        /// <seealso cref="gripAction"/>
        public float gripAmount
        {
            get => m_GripAmount;
            set => m_GripAmount = value;
        }

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of the simulated trigger pull on the controller when the Trigger control is pressed.")]
        float m_TriggerAmount = 1f;
        /// <summary>
        /// The amount of the simulated trigger pull on the controller when the Trigger control is pressed.
        /// </summary>
        /// <seealso cref="triggerAction"/>
        public float triggerAmount
        {
            get => m_TriggerAmount;
            set => m_TriggerAmount = value;
        }

        [SerializeField]
        [Tooltip("Whether the HMD should report the pose as fully tracked or unavailable/inferred.")]
        bool m_HMDIsTracked = true;
        /// <summary>
        /// Whether the HMD should report the pose as fully tracked or unavailable/inferred.
        /// </summary>
        public bool hmdIsTracked
        {
            get => m_HMDIsTracked;
            set => m_HMDIsTracked = value;
        }

        [SerializeField]
        [Tooltip("Which tracking values the HMD should report as being valid or meaningful to use, which could mean either tracked or inferred.")]
        InputTrackingState m_HMDTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
        /// <summary>
        /// Which tracking values the HMD should report as being valid or meaningful to use, which could mean either tracked or inferred.
        /// </summary>
        public InputTrackingState hmdTrackingState
        {
            get => m_HMDTrackingState;
            set => m_HMDTrackingState = value;
        }

        [SerializeField]
        [Tooltip("Whether the left-hand controller should report the pose as fully tracked or unavailable/inferred.")]
        bool m_LeftControllerIsTracked = true;
        /// <summary>
        /// Whether the left-hand controller should report the pose as fully tracked or unavailable/inferred.
        /// </summary>
        public bool leftControllerIsTracked
        {
            get => m_LeftControllerIsTracked;
            set => m_LeftControllerIsTracked = value;
        }

        [SerializeField]
        [Tooltip("Which tracking values the left-hand controller should report as being valid or meaningful to use, which could mean either tracked or inferred.")]
        InputTrackingState m_LeftControllerTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
        /// <summary>
        /// Which tracking values the left-hand controller should report as being valid or meaningful to use, which could mean either tracked or inferred.
        /// </summary>
        public InputTrackingState leftControllerTrackingState
        {
            get => m_LeftControllerTrackingState;
            set => m_LeftControllerTrackingState = value;
        }

        [SerializeField]
        [Tooltip("Whether the right-hand controller should report the pose as fully tracked or unavailable/inferred.")]
        bool m_RightControllerIsTracked = true;
        /// <summary>
        /// Whether the right-hand controller should report the pose as fully tracked or unavailable/inferred.
        /// </summary>
        public bool rightControllerIsTracked
        {
            get => m_RightControllerIsTracked;
            set => m_RightControllerIsTracked = value;
        }

        [SerializeField]
        [Tooltip("Which tracking values the right-hand controller should report as being valid or meaningful to use, which could mean either tracked or inferred.")]
        InputTrackingState m_RightControllerTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
        /// <summary>
        /// Which tracking values the right-hand controller should report as being valid or meaningful to use, which could mean either tracked or inferred.
        /// </summary>
        public InputTrackingState rightControllerTrackingState
        {
            get => m_RightControllerTrackingState;
            set => m_RightControllerTrackingState = value;
        }
        
        [SerializeField]
        [Tooltip("Whether the left hand should report the pose as fully tracked or unavailable/inferred.")]
        bool m_LeftHandIsTracked = true;
        /// <summary>
        /// Whether the left hand should report the pose as fully tracked or unavailable/inferred.
        /// </summary>
        public bool leftHandIsTracked
        {
            get => m_LeftHandIsTracked;
            set => m_LeftHandIsTracked = value;
        }

        [SerializeField]
        [Tooltip("Whether the right hand should report the pose as fully tracked or unavailable/inferred.")]
        bool m_RightHandIsTracked = true;
        /// <summary>
        /// Whether the right hand should report the pose as fully tracked or unavailable/inferred.
        /// </summary>
        public bool rightHandIsTracked
        {
            get => m_RightHandIsTracked;
            set => m_RightHandIsTracked = value;
        }

        /// <summary>
        /// The transformation mode in which the mouse should operate.
        /// </summary>
        /// <seealso cref="TransformationMode"/>
        public TransformationMode mouseTransformationMode { get; set; } = TransformationMode.Rotate;

        /// <summary>
        /// Is the user currently using negate mode.
        /// </summary>
        /// <seealso cref="mouseTransformationMode"/>
        public bool negateMode { get; private set; }

        /// <summary>
        /// One or more 2D Axis controls that keyboard input should apply to (or none).
        /// </summary>
        /// <remarks>
        /// Used to control a combination of the position (<see cref="Axis2DTargets.Position"/>),
        /// primary 2D axis (<see cref="Axis2DTargets.Primary2DAxis"/>), or
        /// secondary 2D axis (<see cref="Axis2DTargets.Secondary2DAxis"/>) of manipulated device(s).
        /// </remarks>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        /// <seealso cref="axis2DAction"/>
        /// <seealso cref="restingHandAxis2DAction"/>
        public Axis2DTargets axis2DTargets { get; set; } = Axis2DTargets.Primary2DAxis;

        /// <summary>
        /// Whether the simulator is manipulating the Left device (controller or hand).
        /// </summary>
        public bool manipulatingLeftDevice => m_TargetedDeviceInput.HasDevice(TargetedDevices.LeftDevice);

        /// <summary>
        /// Whether the simulator is manipulating the Right device (controller or hand).
        /// </summary>
        public bool manipulatingRightDevice => m_TargetedDeviceInput.HasDevice(TargetedDevices.RightDevice);

        /// <summary>
        /// Whether the simulator is manipulating the Left Controller.
        /// </summary>
        public bool manipulatingLeftController => m_DeviceMode == DeviceMode.Controller && manipulatingLeftDevice;

        /// <summary>
        /// Whether the simulator is manipulating the Right Controller.
        /// </summary>
        public bool manipulatingRightController => m_DeviceMode == DeviceMode.Controller && manipulatingRightDevice;
        
        /// <summary>
        /// Whether the simulator is manipulating the Left Hand.
        /// </summary>
        public bool manipulatingLeftHand => m_DeviceMode == DeviceMode.Hand && manipulatingLeftDevice;
        
        /// <summary>
        /// Whether the simulator is manipulating the Right Hand.
        /// </summary>
        public bool manipulatingRightHand => m_DeviceMode == DeviceMode.Hand && manipulatingRightDevice;

        /// <summary>
        /// Whether the simulator is manipulating the HMD, Left Controller, and Right Controller as if the whole player was turning their torso,
        /// similar to a typical FPS style.
        /// </summary>
        public bool manipulatingFPS => m_TargetedDeviceInput == TargetedDevices.FPS;

        /// <summary>
        /// The runtime instance of the XR Device Simulator.
        /// </summary>
        public static XRDeviceSimulator instance { get; private set; }

        TargetedDevices m_TargetedDeviceInput = TargetedDevices.FPS;

        TargetedDevices targetedDeviceInput
        {
            get => m_TargetedDeviceInput;
            set => m_TargetedDeviceInput = value;
        }

        DeviceMode m_DeviceMode = DeviceMode.Controller;

        /// <summary>
        /// Whether the simulator is in controller mode or tracked hand mode.
        /// </summary>
        /// <seealso cref="DeviceMode"/>
        public DeviceMode deviceMode => m_DeviceMode;

        bool m_DeviceModeDirty;
        bool m_StartedDeviceModeChange;

        (Transform transform, Camera camera) m_CachedCamera;

        /// <summary>
        /// Current value of the x-axis when using keyboard translate.
        /// </summary>
        float m_KeyboardXTranslateInput;

        /// <summary>
        /// Current value of the y-axis when using keyboard translate.
        /// </summary>
        float m_KeyboardYTranslateInput;

        /// <summary>
        /// Current value of the z-axis when using keyboard translate.
        /// </summary>
        float m_KeyboardZTranslateInput;

        Vector2 m_MouseDeltaInput;
        Vector2 m_MouseScrollInput;

        bool m_RotateModeOverrideInput;

        bool m_XConstraintInput;
        bool m_YConstraintInput;
        bool m_ZConstraintInput;

        bool m_ResetInput;

        Vector2 m_Axis2DInput;
        Vector2 m_RestingHandAxis2DInput;

        bool m_GripInput;
        bool m_TriggerInput;
        bool m_PrimaryButtonInput;
        bool m_SecondaryButtonInput;
        bool m_MenuInput;
        bool m_Primary2DAxisClickInput;
        bool m_Secondary2DAxisClickInput;
        bool m_Primary2DAxisTouchInput;
        bool m_Secondary2DAxisTouchInput;
        bool m_PrimaryTouchInput;
        bool m_SecondaryTouchInput;

        bool m_ManipulatedRestingHandAxis2D;

        Vector3 m_LeftControllerEuler;
        Vector3 m_RightControllerEuler;
        Vector3 m_CenterEyeEuler;

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
        XRSimulatedHMDState m_HMDState;
        XRSimulatedControllerState m_LeftControllerState;
        XRSimulatedControllerState m_RightControllerState;

        XRSimulatedHMD m_HMDDevice;
        XRSimulatedController m_LeftControllerDevice;
        XRSimulatedController m_RightControllerDevice;

        bool m_OnInputDeviceChangeSubscribed;
#endif

#if XR_HANDS_1_1_OR_NEWER
        XRHandProviderUtility.SubsystemUpdater m_SubsystemUpdater;
        XRDeviceSimulatorHandsSubsystem m_SimHandSubsystem;
#endif

        XRSimulatedHandState m_LeftHandState;
        XRSimulatedHandState m_RightHandState;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning($"Another instance of XR Device Simulator already exists ({instance}), destroying {gameObject}.", this);
                Destroy(gameObject);
                return;
            }

            if (m_DeviceSimulatorActionAsset == null)
            {
                if (m_ManipulateLeftAction != null)
                    m_DeviceSimulatorActionAsset = m_ManipulateLeftAction.asset;

                if (m_DeviceSimulatorActionAsset == null && m_ManipulateRightAction != null)
                    m_DeviceSimulatorActionAsset = m_ManipulateRightAction.asset;

                if (m_DeviceSimulatorActionAsset == null)
                    Debug.LogError("No Device Simulator Action Asset has been defined, please assign one for the XR Device Simulator to work.", this);
                else
                    Debug.LogWarning($"No Device Simulator Action Asset has been defined for the XR Device Simulator, using a default one: {m_DeviceSimulatorActionAsset.name}", m_DeviceSimulatorActionAsset);
            }

            if (m_ControllerActionAsset == null)
            {
                if (gripAction != null)
                    m_ControllerActionAsset = gripAction.asset;

                if (m_ControllerActionAsset == null)
                    Debug.LogError("No Controller Action Asset has been defined, please assign one for the XR Device Simulator to work.", this);
                else
                    Debug.LogWarning($"No Controller Action Asset has been defined for the XR Device Simulator, using a default one: {m_ControllerActionAsset.name}", m_ControllerActionAsset);
            }
            
            if (m_HandActionAsset == null && m_SimulatedHandExpressions.Count > 0)
            {
                if (m_SimulatedHandExpressions[0].toggleAction != null)
                    m_HandActionAsset = m_SimulatedHandExpressions[0].toggleAction.asset;

                if (m_HandActionAsset == null)
                    Debug.LogError("No Hand Action Asset has been defined, please assign one for the XR Device Simulator to work.", this);
                else
                    Debug.LogWarning($"No Hand Action Asset has been defined for the XR Device Simulator, using a default one: {m_HandActionAsset.name}", m_HandActionAsset);
            }

            InitializeHandSubsystem();

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            m_HMDState.Reset();
            m_LeftControllerState.Reset();
            m_RightControllerState.Reset();
            m_LeftHandState.Reset();
            m_RightHandState.Reset();

            // Adding offset to the controller/hand when starting simulation to move them away from the Camera position
            m_LeftControllerState.devicePosition = s_LeftDeviceDefaultInitialPosition;
            m_RightControllerState.devicePosition = s_RightDeviceDefaultInitialPosition;
            m_LeftHandState.position = s_LeftDeviceDefaultInitialPosition;
            m_RightHandState.position = s_RightDeviceDefaultInitialPosition;

            if (m_DeviceSimulatorUI != null)
                Instantiate(m_DeviceSimulatorUI, transform);
#else
            Debug.LogWarning("XR Device Simulator is not functional on platforms where ENABLE_VR is not defined.", this);
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)

#if XR_HANDS_1_1_OR_NEWER
            m_SimHandSubsystem?.Start();
            m_SubsystemUpdater?.Start();
#endif

            if (m_RemoveOtherHMDDevices)
            {
                // Operate on a copy of the devices array since we are removing from it
                foreach (var device in InputSystem.InputSystem.devices.ToArray())
                {
                    if (device is XRHMD && !(device is XRSimulatedHMD))
                    {
                        InputSystem.InputSystem.RemoveDevice(device);
                    }
                }

                InputSystem.InputSystem.onDeviceChange += OnInputDeviceChange;
                m_OnInputDeviceChangeSubscribed = true;
            }
#endif

            FindCameraTransform();

            AddDevices();

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            SubscribeKeyboardXTranslateAction();
            SubscribeKeyboardYTranslateAction();
            SubscribeKeyboardZTranslateAction();
            SubscribeManipulateLeftAction();
            SubscribeToggleManipulateLeftAction();
            SubscribeManipulateRightAction();
            SubscribeToggleManipulateRightAction();
            SubscribeToggleManipulateBodyAction();
            SubscribeManipulateHeadAction();
            SubscribeStopManipulationAction();
            SubscribeHandControllerModeAction();
            SubscribeCycleDevicesAction();
            SubscribeMouseDeltaAction();
            SubscribeMouseScrollAction();
            SubscribeRotateModeOverrideAction();
            SubscribeToggleMouseTransformationModeAction();
            SubscribeNegateModeAction();
            SubscribeXConstraintAction();
            SubscribeYConstraintAction();
            SubscribeZConstraintAction();
            SubscribeResetAction();
            SubscribeToggleCursorLockAction();
            SubscribeToggleDevicePositionTargetAction();
            SubscribeTogglePrimary2DAxisTargetAction();
            SubscribeToggleSecondary2DAxisTargetAction();
            SubscribeAxis2DAction();
            SubscribeRestingHandAxis2DAction();
            SubscribeGripAction();
            SubscribeTriggerAction();
            SubscribePrimaryButtonAction();
            SubscribeSecondaryButtonAction();
            SubscribeMenuAction();
            SubscribePrimary2DAxisClickAction();
            SubscribeSecondary2DAxisClickAction();
            SubscribePrimary2DAxisTouchAction();
            SubscribeSecondary2DAxisTouchAction();
            SubscribePrimaryTouchAction();
            SubscribeSecondaryTouchAction();

#if XR_HANDS_1_1_OR_NEWER
            SubscribeHandExpressionActions();

            if (m_HandActionAsset != null)
                m_HandActionAsset.Enable();
#endif
            if (m_ControllerActionAsset != null)
                m_ControllerActionAsset.Enable();

            if (m_DeviceSimulatorActionAsset != null)
                m_DeviceSimulatorActionAsset.Enable();
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_OnInputDeviceChangeSubscribed)
            {
                InputSystem.InputSystem.onDeviceChange -= OnInputDeviceChange;
                m_OnInputDeviceChangeSubscribed = false;
            }
#endif

            RemoveDevices();

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            UnsubscribeKeyboardXTranslateAction();
            UnsubscribeKeyboardYTranslateAction();
            UnsubscribeKeyboardZTranslateAction();
            UnsubscribeManipulateLeftAction();
            UnsubscribeToggleManipulateLeftAction();
            UnsubscribeManipulateRightAction();
            UnsubscribeToggleManipulateRightAction();
            UnsubscribeToggleManipulateBodyAction();
            UnsubscribeManipulateHeadAction();
            UnsubscribeStopManipulationAction();
            UnsubscribeHandControllerModeAction();
            UnsubscribeCycleDevicesAction();
            UnsubscribeMouseDeltaAction();
            UnsubscribeMouseScrollAction();
            UnsubscribeRotateModeOverrideAction();
            UnsubscribeToggleMouseTransformationModeAction();
            UnsubscribeNegateModeAction();
            UnsubscribeXConstraintAction();
            UnsubscribeYConstraintAction();
            UnsubscribeZConstraintAction();
            UnsubscribeResetAction();
            UnsubscribeToggleCursorLockAction();
            UnsubscribeToggleDevicePositionTargetAction();
            UnsubscribeTogglePrimary2DAxisTargetAction();
            UnsubscribeToggleSecondary2DAxisTargetAction();
            UnsubscribeAxis2DAction();
            UnsubscribeRestingHandAxis2DAction();
            UnsubscribeGripAction();
            UnsubscribeTriggerAction();
            UnsubscribePrimaryButtonAction();
            UnsubscribeSecondaryButtonAction();
            UnsubscribeMenuAction();
            UnsubscribePrimary2DAxisClickAction();
            UnsubscribeSecondary2DAxisClickAction();
            UnsubscribePrimary2DAxisTouchAction();
            UnsubscribeSecondary2DAxisTouchAction();
            UnsubscribePrimaryTouchAction();
            UnsubscribeSecondaryTouchAction();

#if XR_HANDS_1_1_OR_NEWER
            UnsubscribeHandExpressionActions();

            m_SubsystemUpdater?.Stop();
            m_SimHandSubsystem?.Stop();

            if (m_HandActionAsset != null)
                m_HandActionAsset.Disable();
#endif

            if (m_ControllerActionAsset != null)
                m_ControllerActionAsset.Disable();

            if (m_DeviceSimulatorActionAsset != null)
                m_DeviceSimulatorActionAsset.Disable();
#endif
        }

        void OnDestroy()
        {
#if XR_HANDS_1_1_OR_NEWER
            m_SimHandSubsystem?.Destroy();
            m_SubsystemUpdater?.Destroy();
            m_SimHandSubsystem = null;
            m_SubsystemUpdater = null;
#endif
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Update()
        {
            ProcessPoseInput();
            ProcessControlInput();

#if XR_HANDS_1_1_OR_NEWER
            if (m_DeviceModeDirty)
            {
                switch (m_DeviceMode)
                {
                    // Changing from hands to controllers over multiple frames.
                    // Step 1: Simulate hand tracking lost
                    // Step 2: Add controller input devices.
                    case DeviceMode.Controller when !m_StartedDeviceModeChange:
                        // Step 1
                        m_SimHandSubsystem?.SetUpdateHandsAllowed(false);
                        m_StartedDeviceModeChange = true;
                        break;
                    case DeviceMode.Controller:
                        // Step 2
                        AddControllerDevices();
                        m_DeviceModeDirty = false;
                        m_StartedDeviceModeChange = false;
                        break;
                    // Changing from controllers to hands over multiple frames.
                    // Step 1: Remove controller devices.
                    // Step 2: Simulate hand tracking reacquired.
                    case DeviceMode.Hand when !m_StartedDeviceModeChange:
                        // Step 1
                        RemoveControllerDevices();
                        m_StartedDeviceModeChange = true;
                        break;
                    case DeviceMode.Hand:
                    {
                        // Step 2
                        m_SimHandSubsystem?.SetUpdateHandsAllowed(true);
                        m_DeviceModeDirty = false;
                        m_StartedDeviceModeChange = false;
                        break;
                    }
                }
            }
#endif

            ApplyHandState();

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_HMDDevice != null && m_HMDDevice.added)
            {
                InputState.Change(m_HMDDevice, m_HMDState);
            }

            if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
            {
                InputState.Change(m_LeftControllerDevice, m_LeftControllerState);
            }

            if (m_RightControllerDevice != null && m_RightControllerDevice.added)
            {
                InputState.Change(m_RightControllerDevice, m_RightControllerState);
            }
#endif
        }

        void InitializeHandSubsystem()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (!m_HandTrackingCapability)
                return;

            if (m_RestingHandExpressionCapture == null)
                return;

            if (m_RemoveOtherHMDDevices)
            {
                var currentHandSubsystems = new List<XRHandSubsystem>();
                SubsystemManager.GetSubsystems(currentHandSubsystems);
                foreach (var handSubsystem in currentHandSubsystems)
                {
                    if (handSubsystem.running)
                        handSubsystem.Stop();
                }
            }

            var descriptors = new List<XRHandSubsystemDescriptor>();
            SubsystemManager.GetSubsystemDescriptors(descriptors);
            for (var i = 0; i < descriptors.Count; ++i)
            {
                var descriptor = descriptors[i];
                if (descriptor.id == XRDeviceSimulatorHandsProvider.id)
                {
                    m_SimHandSubsystem = descriptor.Create() as XRDeviceSimulatorHandsSubsystem;
                    break;
                }
            }

            if (m_SimHandSubsystem == null)
            {
                Debug.LogError("Couldn't find Device Simulator hands subsystem.", this);
                return;
            }

            // Pass the hand expression captures to the simulated hand subsystem
            m_SimHandSubsystem.SetCapturedExpression(HandExpressionName.Default, m_RestingHandExpressionCapture);
            for (var index = 0; index < m_SimulatedHandExpressions.Count; ++index)
            {
                var simulatedExpression = m_SimulatedHandExpressions[index];

                if (simulatedExpression.capture != null)
                    m_SimHandSubsystem.SetCapturedExpression(simulatedExpression.expressionName, simulatedExpression.capture);
                else
                    Debug.LogError($"Missing Capture reference for Simulated Hand Expression: {simulatedExpression.expressionName}", this);
            }

            m_SimHandSubsystem.SetUpdateHandsAllowed(false);

            m_SubsystemUpdater = new XRHandProviderUtility.SubsystemUpdater(m_SimHandSubsystem);
#endif
        }

        /// <summary>
        /// Finds and sets <see cref="cameraTransform"/> if necessary.
        /// </summary>
        /// <returns>Returns <see langword="true"/> if the camera reference is valid. Otherwise, returns <see langword="false"/>.</returns>
        bool FindCameraTransform()
        {
            // Sync the cache tuple if necessary
            if (m_CachedCamera.transform != m_CameraTransform)
                m_CachedCamera = (m_CameraTransform, m_CameraTransform != null ? m_CameraTransform.GetComponent<Camera>() : null);

            // Camera.main returns the first active and enabled main camera, so if the cached one
            // is no longer enabled, find the new main camera. This is to support, for example,
            // toggling between different XROrigin rigs each with their own main camera.
            if (m_CachedCamera.transform == null ||
                (m_CachedCamera.camera != null && !m_CachedCamera.camera.isActiveAndEnabled))
            {
                var mainCamera = Camera.main;
                if (mainCamera == null)
                    return false;

                m_CameraTransform = mainCamera.transform;
                m_CachedCamera = (m_CameraTransform, m_CameraTransform.GetComponent<Camera>());
            }

            return true;
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated device(s)
        /// related to position and rotation.
        /// </summary>
        protected virtual void ProcessPoseInput()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            // Set tracked states
            m_LeftControllerState.isTracked = m_LeftControllerIsTracked;
            m_RightControllerState.isTracked = m_RightControllerIsTracked;
            m_HMDState.isTracked = m_HMDIsTracked;
            m_LeftControllerState.trackingState = (int)m_LeftControllerTrackingState;
            m_RightControllerState.trackingState = (int)m_RightControllerTrackingState;
            m_HMDState.trackingState = (int)m_HMDTrackingState;

            if (m_TargetedDeviceInput == TargetedDevices.None)
                return;

            if (!FindCameraTransform())
                return;

            var cameraParent = m_CameraTransform.parent;
            var cameraParentRotation = cameraParent != null ? cameraParent.rotation : Quaternion.identity;
            var inverseCameraParentRotation = Quaternion.Inverse(cameraParentRotation);

            // If we are not manipulating any input, manipulate the devices as an FPS controller.
            // It allows the player to translate along the ground and rotate while keeping the controllers in front,
            // essentially rotating the HMD and both controllers around a common pivot rather than local to each.
            // Time delay as a workaround to avoid large mouse deltas on the first frame.
            if (m_TargetedDeviceInput == TargetedDevices.FPS && Time.time > 1f)
            {
                // Keyboard translation
                var scaledKeyboardTranslateInput = new Vector3(
                    m_KeyboardXTranslateInput * m_KeyboardXTranslateSpeed * m_KeyboardBodyTranslateMultiplier * Time.deltaTime,
                    m_KeyboardYTranslateInput * m_KeyboardYTranslateSpeed * m_KeyboardBodyTranslateMultiplier * Time.deltaTime,
                    m_KeyboardZTranslateInput * m_KeyboardZTranslateSpeed * m_KeyboardBodyTranslateMultiplier * Time.deltaTime);

                var forward = m_CameraTransform.forward;
                var cameraParentUp = cameraParentRotation * Vector3.up;
                if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(forward, cameraParentUp)), 1f))
                {
                    forward = -cameraTransform.up;
                }

                var forwardProjected = Vector3.ProjectOnPlane(forward, cameraParentUp);
                var forwardRotation = Quaternion.LookRotation(forwardProjected, cameraParentUp);
                var translationInWorldSpace = forwardRotation * scaledKeyboardTranslateInput;
                var translationInDeviceSpace = inverseCameraParentRotation * translationInWorldSpace;

                // Modify both controllers and hands in FPS mode no matter the device mode of the simulator
                // because we want to keep the devices in front. If we only updated one set, switching the mode
                // to the other would have the other devices no longer in front in the same relative position,
                // which is probably not what the user wants.
                m_LeftControllerState.devicePosition += translationInDeviceSpace;
                m_RightControllerState.devicePosition += translationInDeviceSpace;
                m_LeftHandState.position += translationInDeviceSpace;
                m_RightHandState.position += translationInDeviceSpace;

                m_HMDState.centerEyePosition += translationInDeviceSpace;
                m_HMDState.devicePosition = m_HMDState.centerEyePosition;

                // Mouse rotation
                var scaledMouseDeltaInput =
                    new Vector3(m_MouseDeltaInput.x * m_MouseXRotateSensitivity,
                        m_MouseDeltaInput.y * m_MouseYRotateSensitivity * (m_MouseYRotateInvert ? 1f : -1f),
                        m_MouseScrollInput.y * m_MouseScrollRotateSensitivity);

                Vector3 anglesDelta;
                if (m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) // X
                    anglesDelta = new Vector3(-scaledMouseDeltaInput.x + scaledMouseDeltaInput.y, 0f, 0f);
                else if (!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) // Y
                    anglesDelta = new Vector3(0f, scaledMouseDeltaInput.x + -scaledMouseDeltaInput.y, 0f);
                else
                    anglesDelta = new Vector3(scaledMouseDeltaInput.y, scaledMouseDeltaInput.x, 0f);

                m_CenterEyeEuler += anglesDelta;
                // Avoid awkward pitch angles
                m_CenterEyeEuler.x = Mathf.Clamp(m_CenterEyeEuler.x, -k_CameraMaxXAngle, k_CameraMaxXAngle);
                m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
                m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;

                var controllerRotationDelta = Quaternion.AngleAxis(anglesDelta.y, Quaternion.Euler(0f, m_CenterEyeEuler.y, 0f) * Vector3.up);
                var pivotPoint = m_HMDState.centerEyePosition;

                // Controllers
                m_LeftControllerState.devicePosition = controllerRotationDelta * (m_LeftControllerState.devicePosition - pivotPoint) + pivotPoint;
                m_LeftControllerState.deviceRotation = controllerRotationDelta * m_LeftControllerState.deviceRotation;
                m_RightControllerState.devicePosition = controllerRotationDelta * (m_RightControllerState.devicePosition - pivotPoint) + pivotPoint;
                m_RightControllerState.deviceRotation = controllerRotationDelta * m_RightControllerState.deviceRotation;

                // Replace euler angle representation with the updated value to make sure
                // the rotation of the controller doesn't jump when manipulating them not in FPS mode.
                m_LeftControllerEuler = m_LeftControllerState.deviceRotation.eulerAngles;
                m_RightControllerEuler = m_RightControllerState.deviceRotation.eulerAngles;

                // Hands
                m_LeftHandState.position = controllerRotationDelta * (m_LeftHandState.position - pivotPoint) + pivotPoint;
                m_LeftHandState.rotation = controllerRotationDelta * m_LeftHandState.rotation;
                m_RightHandState.position = controllerRotationDelta * (m_RightHandState.position - pivotPoint) + pivotPoint;
                m_RightHandState.rotation = controllerRotationDelta * m_RightHandState.rotation;

                m_LeftHandState.euler = m_LeftHandState.rotation.eulerAngles;
                m_RightHandState.euler = m_RightHandState.rotation.eulerAngles;

                // Reset
                if (m_ResetInput)
                {
                    // Controllers
                    // We reset both position and rotation in this FPS mode, so axis constraint is ignored
                    m_LeftControllerState.devicePosition = s_LeftDeviceDefaultInitialPosition;
                    m_RightControllerState.devicePosition = s_RightDeviceDefaultInitialPosition;

                    m_LeftControllerEuler = Vector3.zero;
                    m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);

                    m_RightControllerEuler = Vector3.zero;
                    m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);

                    // Hands
                    m_LeftHandState.position = s_LeftDeviceDefaultInitialPosition;
                    m_RightHandState.position = s_RightDeviceDefaultInitialPosition;

                    m_LeftHandState.euler = Vector3.zero;
                    m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);

                    m_RightHandState.euler = Vector3.zero;
                    m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);

                    // HMD
                    m_HMDState.centerEyePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
                    m_HMDState.devicePosition = m_HMDState.centerEyePosition;

                    m_CenterEyeEuler = Vector3.zero;
                    m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
                    m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
                }
            }

            if ((axis2DTargets & Axis2DTargets.Position) != 0)
            {
                // Determine frame of reference
                GetAxes(m_KeyboardTranslateSpace, m_CameraTransform, out var right, out var up, out var forward);

                // Keyboard translation
                var deltaPosition =
                    right * (m_KeyboardXTranslateInput * m_KeyboardXTranslateSpeed * Time.deltaTime) +
                    up * (m_KeyboardYTranslateInput * m_KeyboardYTranslateSpeed * Time.deltaTime) +
                    forward * (m_KeyboardZTranslateInput * m_KeyboardZTranslateSpeed * Time.deltaTime);

                if (manipulatingLeftController)
                {
                    var deltaRotation = GetDeltaRotation(m_KeyboardTranslateSpace, m_LeftControllerState, inverseCameraParentRotation);
                    m_LeftControllerState.devicePosition += deltaRotation * deltaPosition;
                }

                if (manipulatingRightController)
                {
                    var deltaRotation = GetDeltaRotation(m_KeyboardTranslateSpace, m_RightControllerState, inverseCameraParentRotation);
                    m_RightControllerState.devicePosition += deltaRotation * deltaPosition;
                }

                if (manipulatingLeftHand)
                {
                    var deltaRotation = GetDeltaRotation(m_KeyboardTranslateSpace, m_LeftHandState, inverseCameraParentRotation);
                    m_LeftHandState.position += deltaRotation * deltaPosition;
                }

                if (manipulatingRightHand)
                {
                    var deltaRotation = GetDeltaRotation(m_KeyboardTranslateSpace, m_RightHandState, inverseCameraParentRotation);
                    m_RightHandState.position += deltaRotation * deltaPosition;
                }

                if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
                {
                    var deltaRotation = GetDeltaRotation(m_KeyboardTranslateSpace, m_HMDState, inverseCameraParentRotation);
                    m_HMDState.centerEyePosition += deltaRotation * deltaPosition;
                    m_HMDState.devicePosition = m_HMDState.centerEyePosition;
                }
            }

            if ((mouseTransformationMode == TransformationMode.Translate && !m_RotateModeOverrideInput && !negateMode) ||
                (mouseTransformationMode == TransformationMode.Rotate || m_RotateModeOverrideInput) && negateMode)
            {
                // Determine frame of reference
                GetAxes(m_MouseTranslateSpace, m_CameraTransform, out var right, out var up, out var forward);

                // Mouse translation
                var scaledMouseDeltaInput =
                    new Vector3(m_MouseDeltaInput.x * m_MouseXTranslateSensitivity,
                        m_MouseDeltaInput.y * m_MouseYTranslateSensitivity,
                        m_MouseScrollInput.y * m_MouseScrollTranslateSensitivity);

                Vector3 deltaPosition;
                if (m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // XZ
                    deltaPosition = right * scaledMouseDeltaInput.x + forward * scaledMouseDeltaInput.y;
                else if (!m_XConstraintInput && m_YConstraintInput && m_ZConstraintInput) // YZ
                    deltaPosition = up * scaledMouseDeltaInput.y + forward * scaledMouseDeltaInput.x;
                else if (m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) // X
                    deltaPosition = right * (scaledMouseDeltaInput.x + scaledMouseDeltaInput.y);
                else if (!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) // Y
                    deltaPosition = up * (scaledMouseDeltaInput.x + scaledMouseDeltaInput.y);
                else if (!m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // Z
                    deltaPosition = forward * (scaledMouseDeltaInput.x + scaledMouseDeltaInput.y);
                else
                    deltaPosition = right * scaledMouseDeltaInput.x + up * scaledMouseDeltaInput.y;

                // Scroll contribution
                deltaPosition += forward * scaledMouseDeltaInput.z;

                if (manipulatingLeftController)
                {
                    var deltaRotation = GetDeltaRotation(m_MouseTranslateSpace, m_LeftControllerState, inverseCameraParentRotation);
                    m_LeftControllerState.devicePosition += deltaRotation * deltaPosition;
                }

                if (manipulatingRightController)
                {
                    var deltaRotation = GetDeltaRotation(m_MouseTranslateSpace, m_RightControllerState, inverseCameraParentRotation);
                    m_RightControllerState.devicePosition += deltaRotation * deltaPosition;
                }

                if (manipulatingLeftHand)
                {
                    var deltaRotation = GetDeltaRotation(m_MouseTranslateSpace, m_LeftHandState, inverseCameraParentRotation);
                    m_LeftHandState.position += deltaRotation * deltaPosition;
                }

                if (manipulatingRightHand)
                {
                    var deltaRotation = GetDeltaRotation(mouseTranslateSpace, m_RightHandState, inverseCameraParentRotation);
                    m_RightHandState.position += deltaRotation * deltaPosition;
                }

                if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
                {
                    var deltaRotation = GetDeltaRotation(m_MouseTranslateSpace, m_HMDState, inverseCameraParentRotation);
                    m_HMDState.centerEyePosition += deltaRotation * deltaPosition;
                    m_HMDState.devicePosition = m_HMDState.centerEyePosition;
                }

                // Reset
                if (m_ResetInput)
                {
                    var resetScale = GetResetScale();

                    if (manipulatingLeftController)
                    {
                        var devicePosition = Vector3.Scale(m_LeftControllerState.devicePosition, resetScale);
                        // The active control for the InputAction will be null while the Action is in waiting at (0, 0, 0)
                        // so use a small value to reset the position to near origin.
                        if (devicePosition.magnitude <= 0f)
                            devicePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);

                        m_LeftControllerState.devicePosition = devicePosition;
                    }

                    if (manipulatingRightController)
                    {
                        var devicePosition = Vector3.Scale(m_RightControllerState.devicePosition, resetScale);
                        // The active control for the InputAction will be null while the Action is in waiting at (0, 0, 0)
                        // so use a small value to reset the position to near origin.
                        if (devicePosition.magnitude <= 0f)
                            devicePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);

                        m_RightControllerState.devicePosition = devicePosition;
                    }

                    if (manipulatingLeftHand)
                    {
                        var devicePosition = Vector3.Scale(m_LeftHandState.position, resetScale);
                        // The active control for the InputAction will be null while the Action is in waiting at (0, 0, 0)
                        // so use a small value to reset the position to near origin.
                        if (devicePosition.magnitude <= 0f)
                            devicePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);

                        m_LeftHandState.position = devicePosition;
                    }

                    if (manipulatingRightHand)
                    {
                        var devicePosition = Vector3.Scale(m_RightHandState.position, resetScale);
                        // The active control for the InputAction will be null while the Action is in waiting at (0, 0, 0)
                        // so use a small value to reset the position to near origin.
                        if (devicePosition.magnitude <= 0f)
                            devicePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);

                        m_RightHandState.position = devicePosition;
                    }

                    if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
                    {
                        // TODO: Tracked Pose Driver (New Input System) has a bug where it only subscribes to
                        // performed and not canceled, so the Transform will not be updated until the magnitude
                        // is considered actuated to trigger a performed event. As a workaround, set to
                        // a small value (enough to be considered actuated) instead of Vector3.zero.
                        var centerEyePosition = Vector3.Scale(m_HMDState.centerEyePosition, resetScale);
                        if (centerEyePosition.magnitude <= 0f)
                            centerEyePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);

                        m_HMDState.centerEyePosition = centerEyePosition;
                        m_HMDState.devicePosition = m_HMDState.centerEyePosition;
                    }
                }
            }
            else
            {
                // Mouse rotation
                var scaledMouseDeltaInput =
                    new Vector3(m_MouseDeltaInput.x * m_MouseXRotateSensitivity,
                        m_MouseDeltaInput.y * m_MouseYRotateSensitivity * (m_MouseYRotateInvert ? 1f : -1f),
                        m_MouseScrollInput.y * m_MouseScrollRotateSensitivity);

                Vector3 anglesDelta;
                if (m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // XZ
                    anglesDelta = new Vector3(scaledMouseDeltaInput.y, 0f, -scaledMouseDeltaInput.x);
                else if (!m_XConstraintInput && m_YConstraintInput && m_ZConstraintInput) // YZ
                    anglesDelta = new Vector3(0f, scaledMouseDeltaInput.x, -scaledMouseDeltaInput.y);
                else if (m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) // X
                    anglesDelta = new Vector3(-scaledMouseDeltaInput.x + scaledMouseDeltaInput.y, 0f, 0f);
                else if (!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) // Y
                    anglesDelta = new Vector3(0f, scaledMouseDeltaInput.x + -scaledMouseDeltaInput.y, 0f);
                else if (!m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // Z
                    anglesDelta = new Vector3(0f, 0f, -scaledMouseDeltaInput.x + -scaledMouseDeltaInput.y);
                else
                    anglesDelta = new Vector3(scaledMouseDeltaInput.y, scaledMouseDeltaInput.x, 0f);

                // Scroll contribution
                anglesDelta += new Vector3(0f, 0f, scaledMouseDeltaInput.z);

                if (manipulatingLeftController)
                {
                    m_LeftControllerEuler += anglesDelta;
                    m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
                }

                if (manipulatingRightController)
                {
                    m_RightControllerEuler += anglesDelta;
                    m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
                }

                if (manipulatingLeftHand)
                {
                    m_LeftHandState.euler += anglesDelta;
                    m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);
                }

                if (manipulatingRightHand)
                {
                    m_RightHandState.euler += anglesDelta;
                    m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);
                }

                if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
                {
                    m_CenterEyeEuler += anglesDelta;
                    m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
                    m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
                }

                // Reset
                if (m_ResetInput)
                {
                    var resetScale = GetResetScale();

                    if (manipulatingLeftController)
                    {
                        m_LeftControllerEuler = Vector3.Scale(m_LeftControllerEuler, resetScale);
                        m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
                    }

                    if (manipulatingRightController)
                    {
                        m_RightControllerEuler = Vector3.Scale(m_RightControllerEuler, resetScale);
                        m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
                    }

                    if (manipulatingLeftHand)
                    {
                        m_LeftHandState.euler = Vector3.Scale(m_LeftHandState.euler, resetScale);
                        m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);
                    }

                    if (manipulatingRightHand)
                    {
                        m_RightHandState.euler = Vector3.Scale(m_RightHandState.euler, resetScale);
                        m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);
                    }

                    if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
                    {
                        m_CenterEyeEuler = Vector3.Scale(m_CenterEyeEuler, resetScale);
                        m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
                        m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
                    }
                }
            }
#endif
            }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to input controls.
        /// </summary>
        protected virtual void ProcessControlInput()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_DeviceMode != DeviceMode.Controller)
                return;

            ProcessAxis2DControlInput();

            if (manipulatingLeftController)
                ProcessButtonControlInput(ref m_LeftControllerState);
            else
                ProcessAnalogButtonControlInput(ref m_LeftControllerState);

            if (manipulatingRightController)
                ProcessButtonControlInput(ref m_RightControllerState);
            else
                ProcessAnalogButtonControlInput(ref m_RightControllerState);
#endif
        }

        void ApplyHandState()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (m_DeviceMode != DeviceMode.Hand)
                return;

            if (m_SimHandSubsystem == null)
                return;

            m_SimHandSubsystem.SetIsTracked(Handedness.Left, m_LeftHandIsTracked);
            m_SimHandSubsystem.SetIsTracked(Handedness.Right, m_RightHandIsTracked);

            m_SimHandSubsystem.SetHandExpression(Handedness.Left, m_LeftHandState.expressionName);
            m_SimHandSubsystem.SetRootHandPose(Handedness.Left, new Pose(m_LeftHandState.position, m_LeftHandState.rotation));

            m_SimHandSubsystem.SetHandExpression(Handedness.Right, m_RightHandState.expressionName);
            m_SimHandSubsystem.SetRootHandPose(Handedness.Right, new Pose(m_RightHandState.position, m_RightHandState.rotation));
#endif
        }

        void ToggleHandExpression(SimulatedHandExpression simulatedExpression)
        {
#if XR_HANDS_1_1_OR_NEWER
            if (m_SimHandSubsystem == null)
                return;

            // When toggling off, change back to the default resting hand. Otherwise, change to the expression pressed.
            if (manipulatingLeftHand)
            {
                m_LeftHandState.expressionName = m_LeftHandState.expressionName == simulatedExpression.expressionName
                    ? HandExpressionName.Default
                    : simulatedExpression.expressionName;
                m_SimHandSubsystem.SetHandExpression(Handedness.Left, m_LeftHandState.expressionName);
            }

            if (manipulatingRightHand)
            {
                m_RightHandState.expressionName = m_RightHandState.expressionName == simulatedExpression.expressionName
                    ? HandExpressionName.Default
                    : simulatedExpression.expressionName;
                m_SimHandSubsystem.SetHandExpression(Handedness.Right, m_RightHandState.expressionName);
            }
#endif
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to 2D Axis input controls.
        /// </summary>
        protected virtual void ProcessAxis2DControlInput()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            // Early return if not manipulating either Left or Right Controller
            if ((m_TargetedDeviceInput & (TargetedDevices.LeftDevice | TargetedDevices.RightDevice)) == 0)
                return;

            if ((axis2DTargets & Axis2DTargets.Primary2DAxis) != 0)
            {
                if (manipulatingLeftController)
                    m_LeftControllerState.primary2DAxis = m_Axis2DInput;

                if (manipulatingRightController)
                    m_RightControllerState.primary2DAxis = m_Axis2DInput;

                if (manipulatingLeftController ^ manipulatingRightController)
                {
                    if (m_RestingHandAxis2DInput != Vector2.zero || m_ManipulatedRestingHandAxis2D)
                    {
                        if (manipulatingLeftController)
                            m_RightControllerState.primary2DAxis = m_RestingHandAxis2DInput;

                        if (manipulatingRightController)
                            m_LeftControllerState.primary2DAxis = m_RestingHandAxis2DInput;

                        m_ManipulatedRestingHandAxis2D = m_RestingHandAxis2DInput != Vector2.zero;
                    }
                    else
                    {
                        m_ManipulatedRestingHandAxis2D = false;
                    }
                }
            }

            if ((axis2DTargets & Axis2DTargets.Secondary2DAxis) != 0)
            {
                if (manipulatingLeftController)
                    m_LeftControllerState.secondary2DAxis = m_Axis2DInput;

                if (manipulatingRightController)
                    m_RightControllerState.secondary2DAxis = m_Axis2DInput;

                if (manipulatingLeftController ^ manipulatingRightController)
                {
                    if (m_RestingHandAxis2DInput != Vector2.zero || m_ManipulatedRestingHandAxis2D)
                    {
                        if (manipulatingLeftController)
                            m_RightControllerState.secondary2DAxis = m_RestingHandAxis2DInput;

                        if (manipulatingRightController)
                            m_LeftControllerState.secondary2DAxis = m_RestingHandAxis2DInput;

                        m_ManipulatedRestingHandAxis2D = m_RestingHandAxis2DInput != Vector2.zero;
                    }
                    else
                    {
                        m_ManipulatedRestingHandAxis2D = false;
                    }
                }
            }
#endif
        }

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER) || PACKAGE_DOCS_GENERATION
        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to button input controls.
        /// </summary>
        /// <param name="controllerState">The controller state that will be processed.</param>
        protected virtual void ProcessButtonControlInput(ref XRSimulatedControllerState controllerState)
        {
            controllerState.grip = m_GripInput ? m_GripAmount : 0f;
            controllerState.WithButton(ControllerButton.GripButton, m_GripInput);
            controllerState.trigger = m_TriggerInput ? m_TriggerAmount : 0f;
            controllerState.WithButton(ControllerButton.TriggerButton, m_TriggerInput);
            controllerState.WithButton(ControllerButton.PrimaryButton, m_PrimaryButtonInput);
            controllerState.WithButton(ControllerButton.SecondaryButton, m_SecondaryButtonInput);
            controllerState.WithButton(ControllerButton.MenuButton, m_MenuInput);
            controllerState.WithButton(ControllerButton.Primary2DAxisClick, m_Primary2DAxisClickInput);
            controllerState.WithButton(ControllerButton.Secondary2DAxisClick, m_Secondary2DAxisClickInput);
            controllerState.WithButton(ControllerButton.Primary2DAxisTouch, m_Primary2DAxisTouchInput);
            controllerState.WithButton(ControllerButton.Secondary2DAxisTouch, m_Secondary2DAxisTouchInput);
            controllerState.WithButton(ControllerButton.PrimaryTouch, m_PrimaryTouchInput);
            controllerState.WithButton(ControllerButton.SecondaryTouch, m_SecondaryTouchInput);
        }

        /// <summary>
        /// Update the state of manipulated controller device related to analog values only.
        /// This is used to adjust the grip and trigger values when the user adjusts the slider
        /// when not manipulating the device.
        /// </summary>
        /// <param name="controllerState">The controller state that will be processed.</param>
        protected virtual void ProcessAnalogButtonControlInput(ref XRSimulatedControllerState controllerState)
        {
            if (controllerState.HasButton(ControllerButton.GripButton))
                controllerState.grip = m_GripAmount;

            if (controllerState.HasButton(ControllerButton.TriggerButton))
                controllerState.trigger = m_TriggerAmount;
        }
#endif

        /// <summary>
        /// Add simulated XR devices to the Input System.
        /// </summary>
        /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputSystem.html#UnityEngine_InputSystem_InputSystem_AddDevice__1_System_String_"/>
        protected virtual void AddDevices()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            // Simulated HMD
            if (m_HMDDevice == null)
            {
                var descHMD = new InputDeviceDescription
                {
                    product = nameof(XRSimulatedHMD),
                    capabilities = new XRDeviceDescriptor
                    {
                        characteristics = XRInputTrackingAggregator.Characteristics.hmd,
                    }.ToJson(),
                };

                m_HMDDevice = InputSystem.InputSystem.AddDevice(descHMD) as XRSimulatedHMD;
                if (m_HMDDevice == null)
                    Debug.LogError($"Failed to create {nameof(XRSimulatedHMD)}.", this);
            }
            else
            {
                InputSystem.InputSystem.AddDevice(m_HMDDevice);
            }

            if (m_DeviceMode == DeviceMode.Controller)
                AddControllerDevices();
#endif
        }

        void AddControllerDevices()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_LeftControllerDevice == null)
            {
                var descLeftHand = new InputDeviceDescription
                {
                    product = nameof(XRSimulatedController),
                    capabilities = new XRDeviceDescriptor
                    {
                        deviceName = $"{nameof(XRSimulatedController)} - {InputSystem.CommonUsages.LeftHand}",
                        characteristics = XRInputTrackingAggregator.Characteristics.leftController,
                    }.ToJson(),
                };

                m_LeftControllerDevice = InputSystem.InputSystem.AddDevice(descLeftHand) as XRSimulatedController;
                if (m_LeftControllerDevice != null)
                    InputSystem.InputSystem.SetDeviceUsage(m_LeftControllerDevice, InputSystem.CommonUsages.LeftHand);
                else
                    Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {InputSystem.CommonUsages.LeftHand}.", this);
            }
            else
            {
                InputSystem.InputSystem.AddDevice(m_LeftControllerDevice);
            }

            if (m_RightControllerDevice == null)
            {
                var descRightHand = new InputDeviceDescription
                {
                    product = nameof(XRSimulatedController),
                    capabilities = new XRDeviceDescriptor
                    {
                        deviceName = $"{nameof(XRSimulatedController)} - {InputSystem.CommonUsages.RightHand}",
                        characteristics = XRInputTrackingAggregator.Characteristics.rightController,
                    }.ToJson(),
                };

                m_RightControllerDevice = InputSystem.InputSystem.AddDevice(descRightHand) as XRSimulatedController;
                if (m_RightControllerDevice != null)
                    InputSystem.InputSystem.SetDeviceUsage(m_RightControllerDevice, InputSystem.CommonUsages.RightHand);
                else
                    Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {InputSystem.CommonUsages.RightHand}.", this);
            }
            else
            {
                InputSystem.InputSystem.AddDevice(m_RightControllerDevice);
            }
#endif
        }

        /// <summary>
        /// Remove simulated XR devices from the Input System.
        /// </summary>
        /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/api/UnityEngine.InputSystem.InputSystem.html#UnityEngine_InputSystem_InputSystem_RemoveDevice_UnityEngine_InputSystem_InputDevice_"/>
        protected virtual void RemoveDevices()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_HMDDevice != null && m_HMDDevice.added)
                InputSystem.InputSystem.RemoveDevice(m_HMDDevice);

            RemoveControllerDevices();
#endif
        }

        void RemoveControllerDevices()
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
            {
                InputSystem.InputSystem.RemoveDevice(m_LeftControllerDevice);
            }

            if (m_RightControllerDevice != null && m_RightControllerDevice.added)
            {
                InputSystem.InputSystem.RemoveDevice(m_RightControllerDevice);
            }
#endif
        }

        void OnInputDeviceChange(InputSystem.InputDevice device, InputDeviceChange change)
        {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
            if (!m_RemoveOtherHMDDevices)
                return;

            switch (change)
            {
                case InputDeviceChange.Added:
                    if (device is XRHMD && !(device is XRSimulatedHMD))
                        InputSystem.InputSystem.RemoveDevice(device);
                    break;
            }
#endif
        }

        /// <summary>
        /// Gets a <see cref="Vector3"/> that can be multiplied component-wise with another <see cref="Vector3"/>
        /// to reset components of the <see cref="Vector3"/>, based on axis constraint inputs.
        /// </summary>
        /// <returns></returns>
        /// <seealso cref="resetAction"/>
        /// <seealso cref="xConstraintAction"/>
        /// <seealso cref="yConstraintAction"/>
        /// <seealso cref="zConstraintAction"/>
        protected Vector3 GetResetScale()
        {
            return m_XConstraintInput || m_YConstraintInput || m_ZConstraintInput
                ? new Vector3(m_XConstraintInput ? 0f : 1f, m_YConstraintInput ? 0f : 1f, m_ZConstraintInput ? 0f : 1f)
                : Vector3.zero;
        }

#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
        static void GetAxes(Space translateSpace, Transform cameraTransform, out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            if (cameraTransform == null)
                throw new ArgumentNullException(nameof(cameraTransform));

            switch (translateSpace)
            {
                case Space.Local:
                    // Makes the assumption that the Camera and the Controllers are siblings
                    // (meaning they share a parent GameObject).
                    var cameraParent = cameraTransform.parent;
                    if (cameraParent != null)
                    {
                        right = cameraParent.TransformDirection(Vector3.right);
                        up = cameraParent.TransformDirection(Vector3.up);
                        forward = cameraParent.TransformDirection(Vector3.forward);
                    }
                    else
                    {
                        right = Vector3.right;
                        up = Vector3.up;
                        forward = Vector3.forward;
                    }

                    break;
                case Space.Parent:
                    right = Vector3.right;
                    up = Vector3.up;
                    forward = Vector3.forward;
                    break;
                case Space.Screen:
                    right = cameraTransform.TransformDirection(Vector3.right);
                    up = cameraTransform.TransformDirection(Vector3.up);
                    forward = cameraTransform.TransformDirection(Vector3.forward);
                    break;
                default:
                    right = Vector3.right;
                    up = Vector3.up;
                    forward = Vector3.forward;
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return;
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedControllerState state, in Quaternion inverseCameraParentRotation)
            => GetDeltaRotation(translateSpace, state.deviceRotation, inverseCameraParentRotation);

        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedHandState state, in Quaternion inverseCameraParentRotation)
            => GetDeltaRotation(translateSpace, state.rotation, inverseCameraParentRotation);

        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedHMDState state, in Quaternion inverseCameraParentRotation)
            => GetDeltaRotation(translateSpace, state.centerEyeRotation, inverseCameraParentRotation);

        static Quaternion GetDeltaRotation(Space translateSpace, Quaternion rotation, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return rotation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }
#endif

        static void Subscribe(InputActionReference reference, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null)
        {
            var action = GetInputAction(reference);
            if (action != null)
            {
                if (performed != null)
                    action.performed += performed;
                if (canceled != null)
                    action.canceled += canceled;
            }
        }

        static void Unsubscribe(InputActionReference reference, Action<InputAction.CallbackContext> performed = null, Action<InputAction.CallbackContext> canceled = null)
        {
            var action = GetInputAction(reference);
            if (action != null)
            {
                if (performed != null)
                    action.performed -= performed;
                if (canceled != null)
                    action.canceled -= canceled;
            }
        }

        /// <summary>
        /// Returns the negated <see cref="TransformationMode"/> of the given <paramref name="mode"/>.
        /// </summary>
        /// <param name="mode">The <see cref="TransformationMode"/> to get the negated mode of.</param>
        /// <returns>Returns <see cref="TransformationMode.Translate"/> if given <see cref="TransformationMode.Rotate"/>, and vice versa.</returns>
        public static TransformationMode Negate(TransformationMode mode)
        {
            switch (mode)
            {
                case TransformationMode.Rotate:
                    return TransformationMode.Translate;
                case TransformationMode.Translate:
                    return TransformationMode.Rotate;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(mode)}={mode}.");
                    return TransformationMode.Rotate;
            }
        }

        CursorLockMode Negate(CursorLockMode mode)
        {
            switch (mode)
            {
                case CursorLockMode.None:
                    return m_DesiredCursorLockMode;
                case CursorLockMode.Locked:
                case CursorLockMode.Confined:
                    return CursorLockMode.None;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(mode)}={mode}.");
                    return CursorLockMode.None;
            }
        }

        static DeviceMode Negate(DeviceMode mode)
        {
            switch (mode)
            {
                case DeviceMode.Controller:
                    return DeviceMode.Hand;
                case DeviceMode.Hand:
                    return DeviceMode.Controller;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(mode)}={mode}.");
                    return DeviceMode.Controller;
            }
        }

        void SubscribeKeyboardXTranslateAction() => Subscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);
        void UnsubscribeKeyboardXTranslateAction() => Unsubscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);

        void SubscribeKeyboardYTranslateAction() => Subscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);
        void UnsubscribeKeyboardYTranslateAction() => Unsubscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);

        void SubscribeKeyboardZTranslateAction() => Subscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);
        void UnsubscribeKeyboardZTranslateAction() => Unsubscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);

        void SubscribeManipulateLeftAction() => Subscribe(m_ManipulateLeftAction, OnManipulateLeftPerformed, OnManipulateLeftCanceled);
        void UnsubscribeManipulateLeftAction() => Unsubscribe(m_ManipulateLeftAction, OnManipulateLeftPerformed, OnManipulateLeftCanceled);

        void SubscribeManipulateRightAction() => Subscribe(m_ManipulateRightAction, OnManipulateRightPerformed, OnManipulateRightCanceled);
        void UnsubscribeManipulateRightAction() => Unsubscribe(m_ManipulateRightAction, OnManipulateRightPerformed, OnManipulateRightCanceled);

        void SubscribeToggleManipulateLeftAction() => Subscribe(m_ToggleManipulateLeftAction, OnToggleManipulateLeftPerformed);
        void UnsubscribeToggleManipulateLeftAction() => Unsubscribe(m_ToggleManipulateLeftAction, OnToggleManipulateLeftPerformed);

        void SubscribeToggleManipulateRightAction() => Subscribe(m_ToggleManipulateRightAction, OnToggleManipulateRightPerformed);
        void UnsubscribeToggleManipulateRightAction() => Unsubscribe(m_ToggleManipulateRightAction, OnToggleManipulateRightPerformed);

        void SubscribeToggleManipulateBodyAction() => Subscribe(m_ToggleManipulateBodyAction, OnToggleManipulateBodyPerformed);
        void UnsubscribeToggleManipulateBodyAction() => Unsubscribe(m_ToggleManipulateBodyAction, OnToggleManipulateBodyPerformed);

        void SubscribeManipulateHeadAction() => Subscribe(m_ManipulateHeadAction, OnManipulateHeadPerformed, OnManipulateHeadCanceled);
        void UnsubscribeManipulateHeadAction() => Unsubscribe(m_ManipulateHeadAction, OnManipulateHeadPerformed, OnManipulateHeadCanceled);

        void SubscribeHandControllerModeAction() => Subscribe(m_HandControllerModeAction, OnHandControllerModePerformed);
        void UnsubscribeHandControllerModeAction() => Unsubscribe(m_HandControllerModeAction, OnHandControllerModePerformed);

        void SubscribeCycleDevicesAction() => Subscribe(m_CycleDevicesAction, OnCycleDevicesPerformed);
        void UnsubscribeCycleDevicesAction() => Unsubscribe(m_CycleDevicesAction, OnCycleDevicesPerformed);

        void SubscribeStopManipulationAction() => Subscribe(m_StopManipulationAction, OnStopManipulationPerformed);
        void UnsubscribeStopManipulationAction() => Unsubscribe(m_StopManipulationAction, OnStopManipulationPerformed);

        void SubscribeMouseDeltaAction() => Subscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);
        void UnsubscribeMouseDeltaAction() => Unsubscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);

        void SubscribeMouseScrollAction() => Subscribe(m_MouseScrollAction, OnMouseScrollPerformed, OnMouseScrollCanceled);
        void UnsubscribeMouseScrollAction() => Unsubscribe(m_MouseScrollAction, OnMouseScrollPerformed, OnMouseScrollCanceled);

        void SubscribeRotateModeOverrideAction() => Subscribe(m_RotateModeOverrideAction, OnRotateModeOverridePerformed, OnRotateModeOverrideCanceled);
        void UnsubscribeRotateModeOverrideAction() => Unsubscribe(m_RotateModeOverrideAction, OnRotateModeOverridePerformed, OnRotateModeOverrideCanceled);

        void SubscribeToggleMouseTransformationModeAction() => Subscribe(m_ToggleMouseTransformationModeAction, OnToggleMouseTransformationModePerformed);
        void UnsubscribeToggleMouseTransformationModeAction() => Unsubscribe(m_ToggleMouseTransformationModeAction, OnToggleMouseTransformationModePerformed);

        void SubscribeNegateModeAction() => Subscribe(m_NegateModeAction, OnNegateModePerformed, OnNegateModeCanceled);
        void UnsubscribeNegateModeAction() => Unsubscribe(m_NegateModeAction, OnNegateModePerformed, OnNegateModeCanceled);

        void SubscribeXConstraintAction() => Subscribe(m_XConstraintAction, OnXConstraintPerformed, OnXConstraintCanceled);
        void UnsubscribeXConstraintAction() => Unsubscribe(m_XConstraintAction, OnXConstraintPerformed, OnXConstraintCanceled);

        void SubscribeYConstraintAction() => Subscribe(m_YConstraintAction, OnYConstraintPerformed, OnYConstraintCanceled);
        void UnsubscribeYConstraintAction() => Unsubscribe(m_YConstraintAction, OnYConstraintPerformed, OnYConstraintCanceled);

        void SubscribeZConstraintAction() => Subscribe(m_ZConstraintAction, OnZConstraintPerformed, OnZConstraintCanceled);
        void UnsubscribeZConstraintAction() => Unsubscribe(m_ZConstraintAction, OnZConstraintPerformed, OnZConstraintCanceled);

        void SubscribeResetAction() => Subscribe(m_ResetAction, OnResetPerformed, OnResetCanceled);
        void UnsubscribeResetAction() => Unsubscribe(m_ResetAction, OnResetPerformed, OnResetCanceled);

        void SubscribeToggleCursorLockAction() => Subscribe(m_ToggleCursorLockAction, OnToggleCursorLockPerformed);
        void UnsubscribeToggleCursorLockAction() => Unsubscribe(m_ToggleCursorLockAction, OnToggleCursorLockPerformed);

        void SubscribeToggleDevicePositionTargetAction() => Subscribe(m_ToggleDevicePositionTargetAction, OnToggleDevicePositionTargetPerformed);
        void UnsubscribeToggleDevicePositionTargetAction() => Unsubscribe(m_ToggleDevicePositionTargetAction, OnToggleDevicePositionTargetPerformed);

        void SubscribeTogglePrimary2DAxisTargetAction() => Subscribe(m_TogglePrimary2DAxisTargetAction, OnTogglePrimary2DAxisTargetPerformed);
        void UnsubscribeTogglePrimary2DAxisTargetAction() => Unsubscribe(m_TogglePrimary2DAxisTargetAction, OnTogglePrimary2DAxisTargetPerformed);

        void SubscribeToggleSecondary2DAxisTargetAction() => Subscribe(m_ToggleSecondary2DAxisTargetAction, OnToggleSecondary2DAxisTargetPerformed);
        void UnsubscribeToggleSecondary2DAxisTargetAction() => Unsubscribe(m_ToggleSecondary2DAxisTargetAction, OnToggleSecondary2DAxisTargetPerformed);

        void SubscribeAxis2DAction() => Subscribe(m_Axis2DAction, OnAxis2DPerformed, OnAxis2DCanceled);
        void UnsubscribeAxis2DAction() => Unsubscribe(m_Axis2DAction, OnAxis2DPerformed, OnAxis2DCanceled);

        void SubscribeRestingHandAxis2DAction() => Subscribe(m_RestingHandAxis2DAction, OnRestingHandAxis2DPerformed, OnRestingHandAxis2DCanceled);
        void UnsubscribeRestingHandAxis2DAction() => Unsubscribe(m_RestingHandAxis2DAction, OnRestingHandAxis2DPerformed, OnRestingHandAxis2DCanceled);

        void SubscribeGripAction() => Subscribe(m_GripAction, OnGripPerformed, OnGripCanceled);
        void UnsubscribeGripAction() => Unsubscribe(m_GripAction, OnGripPerformed, OnGripCanceled);

        void SubscribeTriggerAction() => Subscribe(m_TriggerAction, OnTriggerPerformed, OnTriggerCanceled);
        void UnsubscribeTriggerAction() => Unsubscribe(m_TriggerAction, OnTriggerPerformed, OnTriggerCanceled);

        void SubscribePrimaryButtonAction() => Subscribe(m_PrimaryButtonAction, OnPrimaryButtonPerformed, OnPrimaryButtonCanceled);
        void UnsubscribePrimaryButtonAction() => Unsubscribe(m_PrimaryButtonAction, OnPrimaryButtonPerformed, OnPrimaryButtonCanceled);

        void SubscribeSecondaryButtonAction() => Subscribe(m_SecondaryButtonAction, OnSecondaryButtonPerformed, OnSecondaryButtonCanceled);
        void UnsubscribeSecondaryButtonAction() => Unsubscribe(m_SecondaryButtonAction, OnSecondaryButtonPerformed, OnSecondaryButtonCanceled);

        void SubscribeMenuAction() => Subscribe(m_MenuAction, OnMenuPerformed, OnMenuCanceled);
        void UnsubscribeMenuAction() => Unsubscribe(m_MenuAction, OnMenuPerformed, OnMenuCanceled);

        void SubscribePrimary2DAxisClickAction() => Subscribe(m_Primary2DAxisClickAction, OnPrimary2DAxisClickPerformed, OnPrimary2DAxisClickCanceled);
        void UnsubscribePrimary2DAxisClickAction() => Unsubscribe(m_Primary2DAxisClickAction, OnPrimary2DAxisClickPerformed, OnPrimary2DAxisClickCanceled);

        void SubscribeSecondary2DAxisClickAction() => Subscribe(m_Secondary2DAxisClickAction, OnSecondary2DAxisClickPerformed, OnSecondary2DAxisClickCanceled);
        void UnsubscribeSecondary2DAxisClickAction() => Unsubscribe(m_Secondary2DAxisClickAction, OnSecondary2DAxisClickPerformed, OnSecondary2DAxisClickCanceled);

        void SubscribePrimary2DAxisTouchAction() => Subscribe(m_Primary2DAxisTouchAction, OnPrimary2DAxisTouchPerformed, OnPrimary2DAxisTouchCanceled);
        void UnsubscribePrimary2DAxisTouchAction() => Unsubscribe(m_Primary2DAxisTouchAction, OnPrimary2DAxisTouchPerformed, OnPrimary2DAxisTouchCanceled);

        void SubscribeSecondary2DAxisTouchAction() => Subscribe(m_Secondary2DAxisTouchAction, OnSecondary2DAxisTouchPerformed, OnSecondary2DAxisTouchCanceled);
        void UnsubscribeSecondary2DAxisTouchAction() => Unsubscribe(m_Secondary2DAxisTouchAction, OnSecondary2DAxisTouchPerformed, OnSecondary2DAxisTouchCanceled);

        void SubscribePrimaryTouchAction() => Subscribe(m_PrimaryTouchAction, OnPrimaryTouchPerformed, OnPrimaryTouchCanceled);
        void UnsubscribePrimaryTouchAction() => Unsubscribe(m_PrimaryTouchAction, OnPrimaryTouchPerformed, OnPrimaryTouchCanceled);

        void SubscribeSecondaryTouchAction() => Subscribe(m_SecondaryTouchAction, OnSecondaryTouchPerformed, OnSecondaryTouchCanceled);
        void UnsubscribeSecondaryTouchAction() => Unsubscribe(m_SecondaryTouchAction, OnSecondaryTouchPerformed, OnSecondaryTouchCanceled);

#if XR_HANDS_1_1_OR_NEWER
        void SubscribeHandExpressionActions()
        {
            foreach (var simulatedExpression in m_SimulatedHandExpressions)
            {
                simulatedExpression.performed += OnHandExpressionPerformed;
            }
        }
        
        void UnsubscribeHandExpressionActions()
        {
            foreach (var simulatedExpression in m_SimulatedHandExpressions)
            {
                simulatedExpression.performed -= OnHandExpressionPerformed;
            }
        }

        void OnHandExpressionPerformed(SimulatedHandExpression simulatedExpression, InputAction.CallbackContext context)
        {
            ToggleHandExpression(simulatedExpression);
        }
#endif

        void OnKeyboardXTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardXTranslateInput = context.ReadValue<float>();
        void OnKeyboardXTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardXTranslateInput = 0f;

        void OnKeyboardYTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardYTranslateInput = context.ReadValue<float>();
        void OnKeyboardYTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardYTranslateInput = 0f;

        void OnKeyboardZTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardZTranslateInput = context.ReadValue<float>();
        void OnKeyboardZTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardZTranslateInput = 0f;

        void OnManipulateLeftPerformed(InputAction.CallbackContext context) => targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.LeftDevice);
        void OnManipulateLeftCanceled(InputAction.CallbackContext context) => targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.LeftDevice);

        void OnManipulateRightPerformed(InputAction.CallbackContext context) => targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.RightDevice);
        void OnManipulateRightCanceled(InputAction.CallbackContext context) => targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.RightDevice);

        void OnToggleManipulateLeftPerformed(InputAction.CallbackContext context)
        {
            targetedDeviceInput = !targetedDeviceInput.HasDevice(TargetedDevices.LeftDevice)
                ? targetedDeviceInput.WithDevice(TargetedDevices.LeftDevice).WithoutDevice(TargetedDevices.RightDevice)
                : TargetedDevices.FPS;
        }

        void OnToggleManipulateRightPerformed(InputAction.CallbackContext context)
        {
            targetedDeviceInput = !targetedDeviceInput.HasDevice(TargetedDevices.RightDevice)
                ? targetedDeviceInput.WithDevice(TargetedDevices.RightDevice).WithoutDevice(TargetedDevices.LeftDevice)
                : TargetedDevices.FPS;
        }

        void OnToggleManipulateBodyPerformed(InputAction.CallbackContext context) => targetedDeviceInput = TargetedDevices.FPS;

        void OnManipulateHeadPerformed(InputAction.CallbackContext context) => targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.HMD);
        void OnManipulateHeadCanceled(InputAction.CallbackContext context) => targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.HMD);

        void OnHandControllerModePerformed(InputAction.CallbackContext context)
        {
#if XR_HANDS_1_1_OR_NEWER
            // Fully changing between controller and hand mode takes multiple frames.
            // Don't allow changing the mode again before it has finished.
            if (m_DeviceModeDirty)
                return;

            m_DeviceMode = Negate(m_DeviceMode);
            m_DeviceModeDirty = true;
#endif
        }

        void OnCycleDevicesPerformed(InputAction.CallbackContext context)
        {
            // Cycle logic is FPS > LeftDevice > RightDevice
            if (targetedDeviceInput == TargetedDevices.None)
                targetedDeviceInput = TargetedDevices.FPS;
            else if (targetedDeviceInput ==TargetedDevices.FPS)
                targetedDeviceInput = TargetedDevices.LeftDevice;
            else if (targetedDeviceInput.HasDevice(TargetedDevices.LeftDevice))
                targetedDeviceInput = TargetedDevices.RightDevice;
            else if (targetedDeviceInput.HasDevice(TargetedDevices.RightDevice))
                targetedDeviceInput = TargetedDevices.FPS;
        }

        void OnStopManipulationPerformed(InputAction.CallbackContext context) => targetedDeviceInput = TargetedDevices.None;

        void OnMouseDeltaPerformed(InputAction.CallbackContext context) => m_MouseDeltaInput = context.ReadValue<Vector2>();
        void OnMouseDeltaCanceled(InputAction.CallbackContext context) => m_MouseDeltaInput = Vector2.zero;

        void OnMouseScrollPerformed(InputAction.CallbackContext context) => m_MouseScrollInput = context.ReadValue<Vector2>();
        void OnMouseScrollCanceled(InputAction.CallbackContext context) => m_MouseScrollInput = Vector2.zero;

        void OnRotateModeOverridePerformed(InputAction.CallbackContext context) => m_RotateModeOverrideInput = true;
        void OnRotateModeOverrideCanceled(InputAction.CallbackContext context) => m_RotateModeOverrideInput = false;

        void OnToggleMouseTransformationModePerformed(InputAction.CallbackContext context) => mouseTransformationMode = Negate(mouseTransformationMode);

        void OnNegateModePerformed(InputAction.CallbackContext context) => negateMode = true;
        void OnNegateModeCanceled(InputAction.CallbackContext context) => negateMode = false;

        void OnXConstraintPerformed(InputAction.CallbackContext context) => m_XConstraintInput = true;
        void OnXConstraintCanceled(InputAction.CallbackContext context) => m_XConstraintInput = false;

        void OnYConstraintPerformed(InputAction.CallbackContext context) => m_YConstraintInput = true;
        void OnYConstraintCanceled(InputAction.CallbackContext context) => m_YConstraintInput = false;

        void OnZConstraintPerformed(InputAction.CallbackContext context) => m_ZConstraintInput = true;
        void OnZConstraintCanceled(InputAction.CallbackContext context) => m_ZConstraintInput = false;

        void OnResetPerformed(InputAction.CallbackContext context) => m_ResetInput = true;
        void OnResetCanceled(InputAction.CallbackContext context) => m_ResetInput = false;

        void OnToggleCursorLockPerformed(InputAction.CallbackContext context) => Cursor.lockState = Negate(Cursor.lockState);

        void OnToggleDevicePositionTargetPerformed(InputAction.CallbackContext context) => axis2DTargets = (axis2DTargets & Axis2DTargets.Position) != 0 ? Axis2DTargets.None : Axis2DTargets.Position;

        void OnTogglePrimary2DAxisTargetPerformed(InputAction.CallbackContext context) => axis2DTargets = (axis2DTargets & Axis2DTargets.Primary2DAxis) != 0 ? Axis2DTargets.None :  Axis2DTargets.Primary2DAxis;

        void OnToggleSecondary2DAxisTargetPerformed(InputAction.CallbackContext context) => axis2DTargets = (axis2DTargets & Axis2DTargets.Secondary2DAxis) != 0 ? Axis2DTargets.None :  Axis2DTargets.Secondary2DAxis;

        void OnAxis2DPerformed(InputAction.CallbackContext context) => m_Axis2DInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        void OnAxis2DCanceled(InputAction.CallbackContext context) => m_Axis2DInput = Vector2.zero;

        void OnRestingHandAxis2DPerformed(InputAction.CallbackContext context) => m_RestingHandAxis2DInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        void OnRestingHandAxis2DCanceled(InputAction.CallbackContext context) => m_RestingHandAxis2DInput = Vector2.zero;

        void OnGripPerformed(InputAction.CallbackContext context) => m_GripInput = true;
        void OnGripCanceled(InputAction.CallbackContext context) => m_GripInput = false;

        void OnTriggerPerformed(InputAction.CallbackContext context) => m_TriggerInput = true;
        void OnTriggerCanceled(InputAction.CallbackContext context) => m_TriggerInput = false;

        void OnPrimaryButtonPerformed(InputAction.CallbackContext context) => m_PrimaryButtonInput = true;
        void OnPrimaryButtonCanceled(InputAction.CallbackContext context) => m_PrimaryButtonInput = false;

        void OnSecondaryButtonPerformed(InputAction.CallbackContext context) => m_SecondaryButtonInput = true;
        void OnSecondaryButtonCanceled(InputAction.CallbackContext context) => m_SecondaryButtonInput = false;

        void OnMenuPerformed(InputAction.CallbackContext context) => m_MenuInput = true;
        void OnMenuCanceled(InputAction.CallbackContext context) => m_MenuInput = false;

        void OnPrimary2DAxisClickPerformed(InputAction.CallbackContext context) => m_Primary2DAxisClickInput = true;
        void OnPrimary2DAxisClickCanceled(InputAction.CallbackContext context) => m_Primary2DAxisClickInput = false;

        void OnSecondary2DAxisClickPerformed(InputAction.CallbackContext context) => m_Secondary2DAxisClickInput = true;
        void OnSecondary2DAxisClickCanceled(InputAction.CallbackContext context) => m_Secondary2DAxisClickInput = false;

        void OnPrimary2DAxisTouchPerformed(InputAction.CallbackContext context) => m_Primary2DAxisTouchInput = true;
        void OnPrimary2DAxisTouchCanceled(InputAction.CallbackContext context) => m_Primary2DAxisTouchInput = false;

        void OnSecondary2DAxisTouchPerformed(InputAction.CallbackContext context) => m_Secondary2DAxisTouchInput = true;
        void OnSecondary2DAxisTouchCanceled(InputAction.CallbackContext context) => m_Secondary2DAxisTouchInput = false;

        void OnPrimaryTouchPerformed(InputAction.CallbackContext context) => m_PrimaryTouchInput = true;
        void OnPrimaryTouchCanceled(InputAction.CallbackContext context) => m_PrimaryTouchInput = false;

        void OnSecondaryTouchPerformed(InputAction.CallbackContext context) => m_SecondaryTouchInput = true;
        void OnSecondaryTouchCanceled(InputAction.CallbackContext context) => m_SecondaryTouchInput = false;

        static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }

        internal static unsafe bool TryExecuteCommand(InputDeviceCommand* commandPtr, out long result)
        {
            // This is a utility method called by XRSimulatedHMD and XRSimulatedController
            // since both devices share the same command handling.
            // This replicates the logic in XRToISXDevice::IOCTL (XRInputToISX.cpp).
            var type = commandPtr->type;
            if (type == RequestSyncCommand.Type)
            {
                // The state is maintained by XRDeviceSimulator, so no need for any change
                // when focus is regained. Returning success instructs Input System to not
                // reset the device.
                result = InputDeviceCommand.GenericSuccess;
                return true;
            }

            if (type == QueryCanRunInBackground.Type)
            {
                ((QueryCanRunInBackground*)commandPtr)->canRunInBackground = true;
                result = InputDeviceCommand.GenericSuccess;
                return true;
            }

            result = default;
            return false;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="XRDeviceSimulator.TargetedDevices"/>.
    /// </summary>
    static class TargetedDevicesExtensions
    {
        /// <summary>
        /// Returns the flags enum with the given flag set.
        /// </summary>
        /// <param name="devices">The flags enum instance.</param>
        /// <param name="device">The flag to also set in the returned instance.</param>
        /// <returns>Returns the flags enum with the given flag set.</returns>
        public static XRDeviceSimulator.TargetedDevices WithDevice(this XRDeviceSimulator.TargetedDevices devices, XRDeviceSimulator.TargetedDevices device)
        {
            return devices | device;
        }

        /// <summary>
        /// Returns the flags enum with the given flag not set.
        /// </summary>
        /// <param name="devices">The flags enum instance.</param>
        /// <param name="device">The flag to clear in the returned instance.</param>
        /// <returns>Returns the flags enum with the given flag not set.</returns>
        public static XRDeviceSimulator.TargetedDevices WithoutDevice(this XRDeviceSimulator.TargetedDevices devices, XRDeviceSimulator.TargetedDevices device)
        {
            return devices & ~device;
        }

        /// <summary>
        /// Determines whether one or more bit fields are set in the flags
        /// Non-boxing version of <c>HasFlag</c> for <see cref="XRDeviceSimulator.TargetedDevices"/>.
        /// </summary>
        /// <param name="devices">The flags enum instance.</param>
        /// <param name="device">The flag to check if set.</param>
        /// <returns>Returns <see langword="true"/> if the bit field or bit fields are set, otherwise returns <see langword="false"/>.</returns>
        public static bool HasDevice(this XRDeviceSimulator.TargetedDevices devices, XRDeviceSimulator.TargetedDevices device)
        {
            return (devices & device) == device;
        }
    }
}
