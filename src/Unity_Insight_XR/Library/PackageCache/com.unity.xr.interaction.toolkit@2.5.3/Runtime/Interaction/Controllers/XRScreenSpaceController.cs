using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI;

#if AR_FOUNDATION_PRESENT || PACKAGE_DOCS_GENERATION
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
#endif

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interprets screen presses and gestures by using actions from the  Input System and converting them
    /// into XR Interaction states, such as Select. It applies the current press position on the screen to
    /// move the transform of the GameObject.
    /// </summary>
    /// <remarks>
    /// This behavior requires that the Input System is enabled in the <b>Active Input Handling</b>
    /// setting in <b>Edit &gt; Project Settings &gt; Player</b> for input values to be read.
    /// Each input action must also be enabled to read the current value of the action. Referenced
    /// input actions in an Input Action Asset are not enabled by default.
    /// </remarks>
    /// <seealso cref="XRBaseController"/>
    /// <seealso cref="TouchscreenGestureInputController"/>
    [AddComponentMenu("XR/XR Screen Space Controller", 11)]
    [HelpURL(XRHelpURLConstants.k_XRScreenSpaceController)]
    public partial class XRScreenSpaceController : XRBaseController
    {
        [Header("Touchscreen Gesture Actions")]
        [SerializeField]
        [Tooltip("When enabled, a Touchscreen Gesture Input Controller will be added to the Input System device list to detect touch gestures.")]
        bool m_EnableTouchscreenGestureInputController = true;
        /// <summary>
        /// When enabled, a <see cref="TouchscreenGestureInputController"/> will be added to the Input System device list to detect touch gestures.
        /// This input controller drives the gesture values for the input actions for the screen space controller.
        /// </summary>
        public bool enableTouchscreenGestureInputController
        {
            get => m_EnableTouchscreenGestureInputController;
            set => m_EnableTouchscreenGestureInputController = value;
        }

        [SerializeField]
        [Tooltip("The action to use for the screen tap position. (Vector 2 Control).")]
        InputActionProperty m_TapStartPositionAction = new InputActionProperty(new InputAction("Tap Start Position", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for reading screen Tap Position for this GameObject. Must be a <see cref="Vector2Control"/> Control.
        /// </summary>
        public InputActionProperty tapStartPositionAction
        {
            get => m_TapStartPositionAction;
            set => SetInputActionProperty(ref m_TapStartPositionAction, value);
        }

        [SerializeField]
        [Tooltip("The action to use for the current screen drag position. (Vector 2 Control).")]
        InputActionProperty m_DragCurrentPositionAction = new InputActionProperty(new InputAction("Drag Current Position", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for reading the screen Drag Position for this GameObject. Must be a <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <seealso cref="dragDeltaAction"/>
        public InputActionProperty dragCurrentPositionAction
        {
            get => m_DragCurrentPositionAction;
            set => SetInputActionProperty(ref m_DragCurrentPositionAction, value);
        }

        [SerializeField]
        [Tooltip("The action to use for the delta of the screen drag. (Vector 2 Control).")]
        InputActionProperty m_DragDeltaAction = new InputActionProperty(new InputAction("Drag Delta", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action used to read the delta Drag values for this GameObject. Must be a <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <seealso cref="dragCurrentPositionAction"/>
        public InputActionProperty dragDeltaAction
        {
            get => m_DragDeltaAction;
            set => SetInputActionProperty(ref m_DragDeltaAction, value);
        }

        [SerializeField, FormerlySerializedAs("m_PinchStartPosition")]
        [Tooltip("The action to use for the screen pinch gesture start position. (Vector 2 Control).")]
        InputActionProperty m_PinchStartPositionAction = new InputActionProperty(new InputAction("Pinch Start Position", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for reading the Pinch Start Position for this GameObject. Must be a <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <seealso cref="pinchGapDeltaAction"/>
        public InputActionProperty pinchStartPositionAction
        {
            get => m_PinchStartPositionAction;
            set => SetInputActionProperty(ref m_PinchStartPositionAction, value);
        }

        [SerializeField]
        [Tooltip("The action to use for the gap of the screen pinch gesture. (Axis Control).")]
        InputActionProperty m_PinchGapAction = new InputActionProperty(new InputAction(expectedControlType: "Axis"));
        /// <summary>
        /// The Input System action used to read the Pinch values for this GameObject. Must be an <see cref="AxisControl"/> Control.
        /// </summary>
        /// <seealso cref="pinchGapDeltaAction"/>
        public InputActionProperty pinchGapAction
        {
            get => m_PinchGapAction;
            set => SetInputActionProperty(ref m_PinchGapAction, value);
        }

        [SerializeField]
        [Tooltip("The action to use for the delta of the screen pinch gesture. (Axis Control).")]
        InputActionProperty m_PinchGapDeltaAction = new InputActionProperty(new InputAction("Pinch Gap Delta", expectedControlType: "Axis"));
        /// <summary>
        /// The Input System action used to read the delta Pinch values for this GameObject. Must be an <see cref="AxisControl"/> Control.
        /// </summary>
        /// <seealso cref="pinchStartPositionAction"/>
        public InputActionProperty pinchGapDeltaAction
        {
            get => m_PinchGapDeltaAction;
            set => SetInputActionProperty(ref m_PinchGapDeltaAction, value);
        }

        [SerializeField, FormerlySerializedAs("m_TwistStartPosition")]
        [Tooltip("The action to use for the screen twist gesture start position. (Vector 2 Control).")]
        InputActionProperty m_TwistStartPositionAction = new InputActionProperty(new InputAction("Twist Start Position", expectedControlType: "Vector2"));
        /// <summary>
        /// The Input System action to use for reading the Twist Start Position for this GameObject. Must be a <see cref="Vector2Control"/> Control.
        /// </summary>
        /// <seealso cref="twistDeltaRotationAction"/>
        public InputActionProperty twistStartPositionAction
        {
            get => m_TwistStartPositionAction;
            set => SetInputActionProperty(ref m_TwistStartPositionAction, value);
        }

        [SerializeField, FormerlySerializedAs("m_TwistRotationDeltaAction")]
        [Tooltip("The action to use for the delta of the screen twist gesture. (Axis Control).")]
        InputActionProperty m_TwistDeltaRotationAction = new InputActionProperty(new InputAction("Twist Delta Rotation", expectedControlType: "Axis"));
        /// <summary>
        /// The Input System action used to read the delta Twist values for this GameObject. Must be an <see cref="AxisControl"/> Control.
        /// </summary>
        /// <seealso cref="twistStartPositionAction"/>
        public InputActionProperty twistDeltaRotationAction
        {
            get => m_TwistDeltaRotationAction;
            set => SetInputActionProperty(ref m_TwistDeltaRotationAction, value);
        }

        [SerializeField, FormerlySerializedAs("m_ScreenTouchCount")]
        [Tooltip("The number of concurrent touches on the screen. (Integer Control).")]
        InputActionProperty m_ScreenTouchCountAction = new InputActionProperty(new InputAction("Screen Touch Count", expectedControlType: "Integer"));
        /// <summary>
        /// The number of concurrent touches on the screen. Must be an <see cref="IntegerControl"/> Control.
        /// </summary>
        public InputActionProperty screenTouchCountAction
        {
            get => m_ScreenTouchCountAction;
            set => SetInputActionProperty(ref m_ScreenTouchCountAction, value);
        }

        [SerializeField]
        [Tooltip("The camera associated with the screen, and through which screen presses/touches will be interpreted.")]
        Camera m_ControllerCamera;
        /// <summary>
        /// The camera associated with the screen, and through which screen presses/touches will be interpreted.
        /// </summary>
        public Camera controllerCamera
        {
            get => m_ControllerCamera;
            set => m_ControllerCamera = value;
        }
        
        [SerializeField]
        [Tooltip("Tells the XR Screen Space Controller to ignore interactions when hitting a screen space canvas.")]
        bool m_BlockInteractionsWithScreenSpaceUI = true;
        /// <summary>
        /// Tells the XR Screen Space Controller to ignore interactions when hitting a screen space canvas.
        /// </summary>
        /// <seealso cref="Canvas.renderMode"/>
        public bool blockInteractionsWithScreenSpaceUI
        {
            get => m_BlockInteractionsWithScreenSpaceUI;
            set => m_BlockInteractionsWithScreenSpaceUI = value;
        }

        /// <summary>
        /// This value is the change in scale based on input from the <see cref="pinchGapDeltaAction"/> pinch gap delta action
        /// with <see cref="Screen.dpi"/> applied as a factor of the value read in. The delta refers to the change from the previous frame.
        /// </summary>
        /// <remarks>
        /// This value may come back as zero if the input action is not assigned or cannot be read.
        /// </remarks>
        public float scaleDelta { get; private set; }

#if AR_FOUNDATION_PRESENT
        TouchscreenGestureInputController m_GestureInputController;
#endif
        bool m_HasCheckedDisabledTrackingInputReferenceActions;
        bool m_HasCheckedDisabledInputReferenceActions;
        UIInputModule m_UIInputModule;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Start()
        {
            if (m_ControllerCamera == null)
            {
                m_ControllerCamera = Camera.main;
                if (m_ControllerCamera == null)
                {
                    Debug.LogWarning($"Could not find associated {nameof(Camera)} in scene." +
                        $"This {nameof(XRScreenSpaceController)} will be disabled.", this);
                    enabled = false;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            EnableAllDirectActions();
            InitializeTouchscreenGestureController();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            DisableAllDirectActions();
            DestroyTouchscreenGestureController();
            m_UIInputModule = null;
        }

        /// <inheritdoc />
        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            base.UpdateTrackingInput(controllerState);
            if (controllerState == null || IsPointerOverScreenSpaceCanvas())
                return;

            // Warn the user if using referenced actions and they are disabled
            if (!m_HasCheckedDisabledTrackingInputReferenceActions &&
                (m_DragCurrentPositionAction.action != null || m_TapStartPositionAction.action != null || m_TwistStartPositionAction.action != null))
            {
                if (IsDisabledReferenceAction(m_DragCurrentPositionAction) ||
                    IsDisabledReferenceAction(m_TapStartPositionAction) ||
                    IsDisabledReferenceAction(m_TwistStartPositionAction))
                {
                    Debug.LogWarning("'Enable Input Tracking' is enabled, but the Tap, Drag, Pinch, and/or Twist Action is disabled." +
                        " The pose of the controller will not be updated correctly until the Input Actions are enabled." +
                        " Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action." +
                        " The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.",
                        this);
                }

                m_HasCheckedDisabledTrackingInputReferenceActions = true;
            }

            var currentTouchCount = m_ScreenTouchCountAction.action?.ReadValue<int>() ?? 0;
            controllerState.isTracked = currentTouchCount > 0;

            if (TryGetCurrentPositionAction(currentTouchCount, out var posAction))
            {
                var screenPos =  posAction.ReadValue<Vector2>();
                var screenToWorldPoint = m_ControllerCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, m_ControllerCamera.nearClipPlane));
                var directionVector = (screenToWorldPoint - m_ControllerCamera.transform.position).normalized;
                controllerState.position = transform.parent != null ? transform.parent.InverseTransformPoint(screenToWorldPoint) : screenToWorldPoint;
                controllerState.rotation = Quaternion.LookRotation(directionVector);
                controllerState.inputTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;
            }
            else
            {
                controllerState.inputTrackingState = InputTrackingState.None;
            }
        }

        /// <inheritdoc />
        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);
            if (controllerState == null || IsPointerOverScreenSpaceCanvas())
                return;

            // Warn the user if using referenced actions and they are disabled
            if (!m_HasCheckedDisabledInputReferenceActions &&
                (m_TwistDeltaRotationAction.action != null || m_DragCurrentPositionAction.action != null || m_TapStartPositionAction.action != null))
            {
                if (IsDisabledReferenceAction(m_TwistDeltaRotationAction) ||
                    IsDisabledReferenceAction(m_DragCurrentPositionAction) ||
                    IsDisabledReferenceAction(m_TapStartPositionAction))
                {
                    Debug.LogWarning("'Enable Input Actions' is enabled, but the Tap, Drag, Pinch, and/or Twist Action is disabled." +
                        " The controller input will not be handled correctly until the Input Actions are enabled." +
                        " Input Actions in an Input Action Asset must be explicitly enabled to read the current value of the action." +
                        " The Input Action Manager behavior can be added to a GameObject in a Scene and used to enable all Input Actions in a referenced Input Action Asset.",
                        this);
                }

                m_HasCheckedDisabledInputReferenceActions = true;
            }

            controllerState.ResetFrameDependentStates();

            if (TryGetCurrentTwoInputSelectAction(out var twoInputSelectAction))
            {
                controllerState.selectInteractionState.SetFrameState(twoInputSelectAction.phase.IsInProgress(), twoInputSelectAction.ReadValue<float>());
            }
            else if (TryGetCurrentOneInputSelectAction(out var oneInputSelectAction))
            {
                controllerState.selectInteractionState.SetFrameState(oneInputSelectAction.phase == InputActionPhase.Started, oneInputSelectAction.ReadValue<Vector2>().magnitude);
            }
            else
            {
                controllerState.selectInteractionState.SetFrameState(false, 0f);
            }

            scaleDelta = m_PinchGapDeltaAction.action != null ? m_PinchGapDeltaAction.action.ReadValue<float>() / Screen.dpi : 0f;
        }

        bool TryGetCurrentPositionAction(int touchCount, out InputAction action)
        {
            if (touchCount <= 1)
            {
                if (m_DragCurrentPositionAction.action != null &&
                    m_DragCurrentPositionAction.action.phase == InputActionPhase.Started)
                {
                    action = m_DragCurrentPositionAction.action;
                    return true;
                }

                if (m_TapStartPositionAction.action != null &&
                    m_TapStartPositionAction.action.phase == InputActionPhase.Started)
                {
                    action = m_TapStartPositionAction.action;
                    return true;
                }
            }

            action = null;
            return false;
        }

        bool TryGetCurrentOneInputSelectAction(out InputAction action)
        {
            if (m_DragCurrentPositionAction.action != null &&
                m_DragCurrentPositionAction.action.phase == InputActionPhase.Started)
            {
                action = m_DragCurrentPositionAction.action;
                return true;
            }

            if (m_TapStartPositionAction.action != null &&
                m_TapStartPositionAction.action.phase == InputActionPhase.Started)
            {
                action = m_TapStartPositionAction.action;
                return true;
            }

            action = null;
            return false;
        }

        bool TryGetCurrentTwoInputSelectAction(out InputAction action)
        {
            if (m_PinchGapAction.action != null &&
                m_PinchGapAction.action.phase.IsInProgress())
            {
                action = m_PinchGapAction.action;
                return true;
            }

            if (m_PinchGapDeltaAction.action != null &&
                m_PinchGapDeltaAction.action.phase.IsInProgress())
            {
                action = m_PinchGapDeltaAction.action;
                return true;
            }

            if (m_TwistDeltaRotationAction.action != null &&
                m_TwistDeltaRotationAction.action.phase.IsInProgress())
            {
                action = m_TwistDeltaRotationAction.action;
                return true;
            }

            action = null;
            return false;
        }

        bool FindUIInputModule()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.currentInputModule != null)
            {
                m_UIInputModule = eventSystem.currentInputModule as UIInputModule;
            }
            return m_UIInputModule != null;
        }

        bool IsPointerOverScreenSpaceCanvas()
        {
            if (m_BlockInteractionsWithScreenSpaceUI)
            {
                if (m_UIInputModule != null || FindUIInputModule())
                {
                    var uiObject = m_UIInputModule.GetCurrentGameObject(-1);
                    if (uiObject == null)
                        return false;

                    var canvas = uiObject.GetComponentInParent<Canvas>();
                    var renderMode = canvas.renderMode;
                    return renderMode == RenderMode.ScreenSpaceOverlay || renderMode == RenderMode.ScreenSpaceCamera;
                }
            }

            return false;
        }

        void InitializeTouchscreenGestureController()
        {
#if AR_FOUNDATION_PRESENT
            if (!m_EnableTouchscreenGestureInputController)
                return;

            if (m_GestureInputController == null)
            {
                m_GestureInputController = InputSystem.InputSystem.AddDevice<TouchscreenGestureInputController>();
                if (m_GestureInputController == null)
                {
                    Debug.LogError($"Failed to create {nameof(TouchscreenGestureInputController)}.", this);
                }
            }
#endif
        }

        void DestroyTouchscreenGestureController()
        {
#if AR_FOUNDATION_PRESENT
            if (m_GestureInputController != null && m_GestureInputController.added)
            {
                InputSystem.InputSystem.RemoveDevice(m_GestureInputController);
            }
#endif
        }

        void EnableAllDirectActions()
        {
            m_TapStartPositionAction.EnableDirectAction();
            m_DragCurrentPositionAction.EnableDirectAction();
            m_DragDeltaAction.EnableDirectAction();
            m_PinchStartPositionAction.EnableDirectAction();
            m_PinchGapAction.EnableDirectAction();
            m_PinchGapDeltaAction.EnableDirectAction();
            m_TwistStartPositionAction.EnableDirectAction();
            m_TwistDeltaRotationAction.EnableDirectAction();
            m_ScreenTouchCountAction.EnableDirectAction();
        }

        void DisableAllDirectActions()
        {
            m_TapStartPositionAction.DisableDirectAction();
            m_DragCurrentPositionAction.DisableDirectAction();
            m_DragDeltaAction.DisableDirectAction();
            m_PinchStartPositionAction.DisableDirectAction();
            m_PinchGapAction.DisableDirectAction();
            m_PinchGapDeltaAction.DisableDirectAction();
            m_TwistStartPositionAction.DisableDirectAction();
            m_TwistDeltaRotationAction.DisableDirectAction();
            m_ScreenTouchCountAction.DisableDirectAction();
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