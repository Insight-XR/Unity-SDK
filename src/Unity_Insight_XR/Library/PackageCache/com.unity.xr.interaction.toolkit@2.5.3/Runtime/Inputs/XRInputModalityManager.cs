// ENABLE_VR is not defined on Game Core but the assembly is available with limited features when the XR module is enabled.
// These are the guards that Input System uses in GenericXRDevice.cs to define the XRController and XRHMD classes.
#if ENABLE_VR || UNITY_GAMECORE
#define XR_INPUT_DEVICES_AVAILABLE
#endif

using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands;
#endif
#if XR_MANAGEMENT_4_0_OR_NEWER
using UnityEngine.XR.Management;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Inputs
{
    /// <summary>
    /// Manages swapping between hands and controllers at runtime based on whether hands and controllers are tracked.
    /// </summary>
    /// <remarks>
    /// This component uses the following logic for determining which modality is active:
    /// If a hand begin tracking, this component will switch to the hand group of interactors.
    /// If the player wakes the motion controllers by grabbing them, this component will switch to the motion controller group of interactors
    /// once they become tracked. While waiting to activate the controller GameObject while not tracked, both groups will be deactivated.
    /// <br />
    /// This component is useful even when a project does not use hand tracking. By assigning the motion controller set of GameObjects,
    /// this component will keep them deactivated until the controllers become tracked to avoid showing the controllers at the default
    /// origin position.
    /// </remarks>
    [AddComponentMenu("XR/XR Input Modality Manager", 11)]
    [HelpURL(XRHelpURLConstants.k_XRInputModalityManager)]
    public class XRInputModalityManager : MonoBehaviour
    {
        /// <summary>
        /// The mode of an individual hand.
        /// </summary>
        public enum InputMode
        {
            /// <summary>
            /// Neither mode. This is also the mode when waiting for the motion controller to be tracked.
            /// Toggle off both sets of GameObjects.
            /// </summary>
            None,

            /// <summary>
            /// The user is using hand tracking for their hand input.
            /// Swap to the Hand Tracking GameObject for the hand.
            /// </summary>
            TrackedHand,

            /// <summary>
            /// The user is using a motion controller for their hand input.
            /// Swap to the Motion Controllers GameObject for the hand.
            /// </summary>
            MotionController,
        }

#if XR_HANDS_1_1_OR_NEWER
        [Header("Hand Tracking")]
#else
        [HideInInspector]
#endif
        [SerializeField]
        [Tooltip("GameObject representing the left hand group of interactors. Will toggle on when using hand tracking and off when using motion controllers.")]
        GameObject m_LeftHand;

        /// <summary>
        /// GameObject representing the left hand group of interactors. Will toggle on when using hand tracking and off when using motion controllers.
        /// </summary>
        public GameObject leftHand
        {
            get => m_LeftHand;
            set => m_LeftHand = value;
        }

#if !XR_HANDS_1_1_OR_NEWER
        [HideInInspector]
#endif
        [SerializeField]
        [Tooltip("GameObject representing the right hand group of interactors. Will toggle on when using hand tracking and off when using motion controllers.")]
        GameObject m_RightHand;

        /// <summary>
        /// GameObject representing the right hand group of interactors. Will toggle on when using hand tracking and off when using motion controllers.
        /// </summary>
        public GameObject rightHand
        {
            get => m_RightHand;
            set => m_RightHand = value;
        }

        [Header("Motion Controllers")]
        [SerializeField]
        [Tooltip("GameObject representing the left motion controller group of interactors. Will toggle on when using motion controllers and off when using hand tracking.")]
        GameObject m_LeftController;

        /// <summary>
        /// GameObject representing the left motion controller group of interactors. Will toggle on when using motion controllers and off when using hand tracking.
        /// </summary>
        public GameObject leftController
        {
            get => m_LeftController;
            set => m_LeftController = value;
        }

        [SerializeField]
        [Tooltip("GameObject representing the left motion controller group of interactors. Will toggle on when using motion controllers and off when using hand tracking.")]
        GameObject m_RightController;

        /// <summary>
        /// GameObject representing the left motion controller group of interactors. Will toggle on when using motion controllers and off when using hand tracking.
        /// </summary>
        public GameObject rightController
        {
            get => m_RightController;
            set => m_RightController = value;
        }

#if XR_HANDS_1_1_OR_NEWER
        [Header("Events")]
#else
        [HideInInspector]
#endif
        [SerializeField]
        UnityEvent m_TrackedHandModeStarted;

        /// <summary>
        /// Calls the methods in its invocation list when hand tracking mode is started.
        /// </summary>
        /// <remarks>
        /// This event does not fire again for the other hand if the first already started this mode.
        /// </remarks>
        public UnityEvent trackedHandModeStarted
        {
            get => m_TrackedHandModeStarted;
            set => m_TrackedHandModeStarted = value;
        }

#if !XR_HANDS_1_1_OR_NEWER
        [HideInInspector]
#endif
        [SerializeField]
        UnityEvent m_TrackedHandModeEnded;

        /// <summary>
        /// Calls the methods in its invocation list when both hands have stopped hand tracking mode.
        /// </summary>
        public UnityEvent trackedHandModeEnded
        {
            get => m_TrackedHandModeEnded;
            set => m_TrackedHandModeEnded = value;
        }

#if !XR_HANDS_1_1_OR_NEWER
        [Header("Events")]
#endif
        [SerializeField]
        UnityEvent m_MotionControllerModeStarted;

        /// <summary>
        /// Calls the methods in its invocation list when motion controller mode is started.
        /// </summary>
        /// <remarks>
        /// This event does not fire again for the other hand if the first already started this mode.
        /// </remarks>
        public UnityEvent motionControllerModeStarted
        {
            get => m_MotionControllerModeStarted;
            set => m_MotionControllerModeStarted = value;
        }

        [SerializeField]
        UnityEvent m_MotionControllerModeEnded;

        /// <summary>
        /// Calls the methods in its invocation list when both hands have stopped motion controller mode.
        /// </summary>
        public UnityEvent motionControllerModeEnded
        {
            get => m_MotionControllerModeEnded;
            set => m_MotionControllerModeEnded = value;
        }

#if XR_HANDS_1_1_OR_NEWER
        XRHandSubsystem m_HandSubsystem;
        bool m_LoggedMissingHandSubsystem;
#endif

        /// <summary>
        /// Monitor used for waiting until a controller device from the Input System becomes tracked.
        /// </summary>
        /// <remarks>
        /// Used to avoid enabling the controller visuals and interactors if the controller isn't yet tracked
        /// to avoid seeing it at origin, since both controller devices are added upon the first
        /// being picked up by the player.
        /// </remarks>
        readonly TrackedDeviceMonitor m_TrackedDeviceMonitor = new TrackedDeviceMonitor();

        /// <summary>
        /// Monitor used for waiting until a controller device from the XR module becomes tracked.
        /// </summary>
        readonly InputDeviceMonitor m_InputDeviceMonitor = new InputDeviceMonitor();

        /// <summary>
        /// Static bindable variable used to track the current input mode.
        /// </summary>
        public static IReadOnlyBindableVariable<InputMode> currentInputMode => s_CurrentInputMode;
        
        static BindableEnum<InputMode> s_CurrentInputMode = new BindableEnum<InputMode>(InputMode.None);

        InputMode m_LeftInputMode;
        InputMode m_RightInputMode;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (m_HandSubsystem == null || !m_HandSubsystem.running)
            {
                // We don't log here if the hand subsystem is missing because the subsystem may not yet be added
                // if manually done by other behaviors during the first frame's Awake/OnEnable/Start.
                XRInputTrackingAggregator.TryGetHandSubsystem(out m_HandSubsystem);
            }
#else
            if (m_LeftHand != null || m_RightHand != null)
                Debug.LogWarning("Script requires XR Hands (com.unity.xr.hands) package to switch to hand tracking groups. Install using Window > Package Manager or click Fix on the related issue in Edit > Project Settings > XR Plug-in Management > Project Validation.", this);
#endif

            SubscribeHandSubsystem();
            InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
            InputDevices.deviceConnected += OnDeviceConnected;
            InputDevices.deviceDisconnected += OnDeviceDisconnected;
            InputDevices.deviceConfigChanged += OnDeviceConfigChanged;
            m_TrackedDeviceMonitor.trackingAcquired += OnControllerTrackingAcquired;
            m_InputDeviceMonitor.trackingAcquired += OnControllerTrackingAcquired;

            UpdateLeftMode();
            UpdateRightMode();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            UnsubscribeHandSubsystem();
            InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
            InputDevices.deviceConnected -= OnDeviceConnected;
            InputDevices.deviceDisconnected -= OnDeviceDisconnected;
            InputDevices.deviceConfigChanged -= OnDeviceConfigChanged;
            if (m_TrackedDeviceMonitor != null)
            {
                m_TrackedDeviceMonitor.trackingAcquired -= OnControllerTrackingAcquired;
                m_TrackedDeviceMonitor.ClearAllDevices();
            }

            if (m_InputDeviceMonitor != null)
            {
                m_InputDeviceMonitor.trackingAcquired -= OnControllerTrackingAcquired;
                m_InputDeviceMonitor.ClearAllDevices();
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
#if XR_HANDS_1_1_OR_NEWER
            // Retry finding the running hand subsystem if necessary.
            // Only bother to try if hand tracking GameObjects are used.
            if ((m_HandSubsystem == null || !m_HandSubsystem.running) && (m_LeftHand != null || m_RightHand != null))
            {
                if (XRInputTrackingAggregator.TryGetHandSubsystem(out var runningHandSubsystem))
                {
                    if (runningHandSubsystem != m_HandSubsystem)
                    {
                        UnsubscribeHandSubsystem();
                        m_HandSubsystem = runningHandSubsystem;
                        SubscribeHandSubsystem();

                        UpdateLeftMode();
                        UpdateRightMode();
                    }
                }
                // Don't warn if there was some hand subsystem obtained at one time.
                // Without this check, the warning would be logged when exiting play mode.
                else if (m_HandSubsystem == null)
                {
                    LogMissingHandSubsystem();
                }
            }
#endif
        }

        void SubscribeHandSubsystem()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (m_HandSubsystem != null)
                m_HandSubsystem.trackingAcquired += OnHandTrackingAcquired;
#endif
        }

        void UnsubscribeHandSubsystem()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (m_HandSubsystem != null)
                m_HandSubsystem.trackingAcquired -= OnHandTrackingAcquired;
#endif
        }

        void LogMissingHandSubsystem()
        {
#if XR_HANDS_1_1_OR_NEWER
            if (m_LoggedMissingHandSubsystem)
                return;

#if XR_MANAGEMENT_4_0_OR_NEWER
            // If the hand subsystem couldn't be found and Initialize XR on Startup is enabled, warn about enabling Hand Tracking Subsystem.
            // If a user turns off that project setting, don't warn to console since the subsystem wouldn't have been created yet.
            // This warning should allow most users to fix a misconfiguration when they have either of the hand tracking GameObjects set.
            if (m_LeftHand != null || m_RightHand != null)
            {
                var instance = XRGeneralSettings.Instance;
                if (instance != null && instance.InitManagerOnStart)
                {
                    Debug.LogWarning("Hand Tracking Subsystem not found or not running, can't subscribe to hand tracking status." +
                        " Enable that feature in the OpenXR project settings and ensure OpenXR is enabled as the plug-in provider." +
                        " This component will reattempt getting a reference to the subsystem each frame.", this);
                }
            }
#endif

            m_LoggedMissingHandSubsystem = true;
#endif
        }

        void SetLeftMode(InputMode inputMode)
        {
            SafeSetActive(m_LeftHand, inputMode == InputMode.TrackedHand);
            SafeSetActive(m_LeftController, inputMode == InputMode.MotionController);
            var oldMode = m_LeftInputMode;
            m_LeftInputMode = inputMode;

            OnModeChanged(oldMode, inputMode, m_RightInputMode);
        }

        void SetRightMode(InputMode inputMode)
        {
            SafeSetActive(m_RightHand, inputMode == InputMode.TrackedHand);
            SafeSetActive(m_RightController, inputMode == InputMode.MotionController);
            var oldMode = m_RightInputMode;
            m_RightInputMode = inputMode;

            OnModeChanged(oldMode, inputMode, m_LeftInputMode);
        }

        void OnModeChanged(InputMode oldInputMode, InputMode newInputMode, InputMode otherHandInputMode)
        {
            if (oldInputMode == newInputMode)
                return;

            // Invoke the events for the overall input modality.
            // "Started" when the first device changes to it, "Ended" when the last remaining device changes away from it.
            // Invoke the "Ended" event before the "Started" event for intuitive ordering.
            if (otherHandInputMode != InputMode.TrackedHand && oldInputMode == InputMode.TrackedHand)
            {
                m_TrackedHandModeEnded?.Invoke();
            }
            else if (otherHandInputMode != InputMode.MotionController && oldInputMode == InputMode.MotionController)
            {
                m_MotionControllerModeEnded?.Invoke();
            }

            if (otherHandInputMode != InputMode.TrackedHand && newInputMode == InputMode.TrackedHand)
            {
                m_TrackedHandModeStarted?.Invoke();
            }
            else if (otherHandInputMode != InputMode.MotionController && newInputMode == InputMode.MotionController)
            {
                m_MotionControllerModeStarted?.Invoke();
            }

            s_CurrentInputMode.Value = newInputMode;
        }

        static void SafeSetActive(GameObject gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
                gameObject.SetActive(active);
        }

        bool GetLeftHandIsTracked()
        {
#if XR_HANDS_1_1_OR_NEWER
            return m_HandSubsystem != null && m_HandSubsystem.leftHand.isTracked;
#else
            return false;
#endif
        }

        bool GetRightHandIsTracked()
        {
#if XR_HANDS_1_1_OR_NEWER
            return m_HandSubsystem != null && m_HandSubsystem.rightHand.isTracked;
#else
            return false;
#endif
        }

        void UpdateLeftMode()
        {
            if (GetLeftHandIsTracked())
            {
                SetLeftMode(InputMode.TrackedHand);
                return;
            }

#if XR_INPUT_DEVICES_AVAILABLE
            var controllerDevice = InputSystem.InputSystem.GetDevice<InputSystem.XR.XRController>(InputSystem.CommonUsages.LeftHand);
            if (controllerDevice != null)
            {
                UpdateMode(controllerDevice, SetLeftMode);
                return;
            }
#endif

            if (XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.leftController, out var xrInputDevice))
            {
                UpdateMode(xrInputDevice, SetLeftMode);
                return;
            }

            SetLeftMode(InputMode.None);
        }

        void UpdateRightMode()
        {
            if (GetRightHandIsTracked())
            {
                SetRightMode(InputMode.TrackedHand);
                return;
            }

#if XR_INPUT_DEVICES_AVAILABLE
            var controllerDevice = InputSystem.InputSystem.GetDevice<InputSystem.XR.XRController>(InputSystem.CommonUsages.RightHand);
            if (controllerDevice != null)
            {
                UpdateMode(controllerDevice, SetRightMode);
                return;
            }
#endif

            if (XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.rightController, out var xrInputDevice))
            {
                UpdateMode(xrInputDevice, SetRightMode);
                return;
            }

            SetRightMode(InputMode.None);
        }

#if XR_INPUT_DEVICES_AVAILABLE
        void UpdateMode(InputSystem.XR.XRController controllerDevice, Action<InputMode> setModeMethod)
        {
            if (controllerDevice == null)
            {
                setModeMethod(InputMode.None);
                return;
            }

            if (controllerDevice.isTracked.isPressed)
            {
                setModeMethod(InputMode.MotionController);
            }
            else
            {
                // Start monitoring for when the controller is tracked, see OnControllerTrackingAcquired
                setModeMethod(InputMode.None);
                m_TrackedDeviceMonitor.AddDevice(controllerDevice);
            }
        }
#endif

        void UpdateMode(InputDevice controllerDevice, Action<InputMode> setModeMethod)
        {
            if (!controllerDevice.isValid)
            {
                setModeMethod(InputMode.None);
                return;
            }

            if (controllerDevice.TryGetFeatureValue(CommonUsages.isTracked, out var isTracked) && isTracked)
            {
                setModeMethod(InputMode.MotionController);
            }
            else
            {
                // Start monitoring for when the controller is tracked, see OnControllerTrackingAcquired
                setModeMethod(InputMode.None);
                m_InputDeviceMonitor.AddDevice(controllerDevice);
            }
        }

        void OnDeviceChange(InputSystem.InputDevice device, InputDeviceChange change)
        {
#if XR_INPUT_DEVICES_AVAILABLE
            if (!(device is InputSystem.XR.XRController controllerDevice))
                return;

            if (change == InputDeviceChange.Added ||
                change == InputDeviceChange.Reconnected ||
                change == InputDeviceChange.Enabled ||
                change == InputDeviceChange.UsageChanged)
            {
                if (!device.added)
                    return;

                // Swap to controller
                var usages = device.usages;
                if (usages.Contains(InputSystem.CommonUsages.LeftHand))
                {
                    UpdateMode(controllerDevice, SetLeftMode);
                }
                else if (usages.Contains(InputSystem.CommonUsages.RightHand))
                {
                    UpdateMode(controllerDevice, SetRightMode);
                }
            }
            else if (change == InputDeviceChange.Removed ||
                     change == InputDeviceChange.Disconnected ||
                     change == InputDeviceChange.Disabled)
            {
                m_TrackedDeviceMonitor.RemoveDevice(controllerDevice);

                // Swap to hand tracking if tracked or turn off the controller
                var usages = device.usages;
                if (usages.Contains(InputSystem.CommonUsages.LeftHand))
                {
                    var mode = GetLeftHandIsTracked() ? InputMode.TrackedHand : InputMode.None;
                    SetLeftMode(mode);
                }
                else if (usages.Contains(InputSystem.CommonUsages.RightHand))
                {
                    var mode = GetRightHandIsTracked() ? InputMode.TrackedHand : InputMode.None;
                    SetRightMode(mode);
                }
            }
#endif
        }

        void OnDeviceConnected(InputDevice device)
        {
            // Swap to controller
            var characteristics = device.characteristics;
            if (characteristics == XRInputTrackingAggregator.Characteristics.leftController)
            {
                UpdateMode(device, SetLeftMode);
            }
            else if (characteristics == XRInputTrackingAggregator.Characteristics.rightController)
            {
                UpdateMode(device, SetRightMode);
            }
        }

        void OnDeviceDisconnected(InputDevice device)
        {
            m_InputDeviceMonitor.RemoveDevice(device);

            // Swap to hand tracking if tracked or turn off the controller
            var characteristics = device.characteristics;
            if (characteristics == XRInputTrackingAggregator.Characteristics.leftController)
            {
                var mode = GetLeftHandIsTracked() ? InputMode.TrackedHand : InputMode.None;
                SetLeftMode(mode);
            }
            else if (characteristics == XRInputTrackingAggregator.Characteristics.rightController)
            {
                var mode = GetRightHandIsTracked() ? InputMode.TrackedHand : InputMode.None;
                SetRightMode(mode);
            }
        }

        void OnDeviceConfigChanged(InputDevice device)
        {
            // Do the same as if the device was added
            OnDeviceConnected(device);
        }

        void OnControllerTrackingAcquired(TrackedDevice device)
        {
#if XR_INPUT_DEVICES_AVAILABLE
            if (!(device is InputSystem.XR.XRController))
                return;

            var usages = device.usages;
            if (m_LeftInputMode == InputMode.None && usages.Contains(InputSystem.CommonUsages.LeftHand))
            {
                SetLeftMode(InputMode.MotionController);
            }
            else if (m_RightInputMode == InputMode.None && usages.Contains(InputSystem.CommonUsages.RightHand))
            {
                SetRightMode(InputMode.MotionController);
            }
#endif
        }

        void OnControllerTrackingAcquired(InputDevice device)
        {
            var characteristics = device.characteristics;
            if (m_LeftInputMode == InputMode.None && characteristics == XRInputTrackingAggregator.Characteristics.leftController)
            {
                SetLeftMode(InputMode.MotionController);
            }
            else if (m_RightInputMode == InputMode.None && characteristics == XRInputTrackingAggregator.Characteristics.rightController)
            {
                SetRightMode(InputMode.MotionController);
            }
        }

#if XR_HANDS_1_1_OR_NEWER
        void OnHandTrackingAcquired(XRHand hand)
        {
            switch (hand.handedness)
            {
                case Handedness.Left:
                    SetLeftMode(InputMode.TrackedHand);
                    break;

                case Handedness.Right:
                    SetRightMode(InputMode.TrackedHand);
                    break;
            }
        }
#endif

        /// <summary>
        /// Helper class to monitor tracked devices from Input System and invoke an event
        /// when the device is tracked. Used in the behavior to keep a GameObject deactivated
        /// until the device becomes tracked, at which point the callback method can activate it.
        /// </summary>
        /// <seealso cref="InputDeviceMonitor"/>
        class TrackedDeviceMonitor
        {
            /// <summary>
            /// Event that is invoked one time when the device is tracked.
            /// </summary>
            /// <seealso cref="AddDevice"/>
            /// <seealso cref="TrackedDevice.isTracked"/>
            public event Action<TrackedDevice> trackingAcquired;

            readonly List<int> m_MonitoredDevices = new List<int>();

            bool m_Subscribed;

            /// <summary>
            /// Add a tracked device to monitor and invoke <see cref="trackingAcquired"/>
            /// one time when the device is tracked. The device is automatically removed
            /// from being monitored when tracking is acquired.
            /// </summary>
            /// <param name="device">The tracked device to start monitoring.</param>
            /// <remarks>
            /// Waits until the next Input System update to read if the device is tracked.
            /// </remarks>
            public void AddDevice(TrackedDevice device)
            {
                // Start subscribing if necessary
                if (!m_MonitoredDevices.Contains(device.deviceId))
                {
                    m_MonitoredDevices.Add(device.deviceId);
                    Subscribe();
                }
            }

            /// <summary>
            /// Stop monitoring the device for its tracked status.
            /// </summary>
            /// <param name="device">The tracked device to stop monitoring.</param>
            public void RemoveDevice(TrackedDevice device)
            {
                // Stop subscribing if there are no devices left to monitor
                if (m_MonitoredDevices.Remove(device.deviceId) && m_MonitoredDevices.Count == 0)
                    Unsubscribe();
            }

            /// <summary>
            /// Stop monitoring all devices for their tracked status.
            /// </summary>
            public void ClearAllDevices()
            {
                if (m_MonitoredDevices.Count > 0)
                {
                    m_MonitoredDevices.Clear();
                    Unsubscribe();
                }
            }

            void Subscribe()
            {
                if (!m_Subscribed && m_MonitoredDevices.Count > 0)
                {
                    InputSystem.InputSystem.onAfterUpdate += OnAfterInputUpdate;
                    m_Subscribed = true;
                }
            }

            void Unsubscribe()
            {
                if (m_Subscribed)
                {
                    InputSystem.InputSystem.onAfterUpdate -= OnAfterInputUpdate;
                    m_Subscribed = false;
                }
            }

            void OnAfterInputUpdate()
            {
                for (var index = 0; index < m_MonitoredDevices.Count; ++index)
                {
                    if (!(InputSystem.InputSystem.GetDeviceById(m_MonitoredDevices[index]) is TrackedDevice device))
                        continue;

                    if (!device.isTracked.isPressed)
                        continue;

                    // Stop monitoring and invoke event
                    m_MonitoredDevices.RemoveAt(index);
                    --index;

                    trackingAcquired?.Invoke(device);
                }

                // Once all monitored devices have been tracked, unsubscribe from the global event
                if (m_MonitoredDevices.Count == 0)
                    Unsubscribe();
            }
        }

        /// <summary>
        /// Helper class to monitor input devices from the XR module and invoke an event
        /// when the device is tracked. Used in the behavior to keep a GameObject deactivated
        /// until the device becomes tracked, at which point the callback method can activate it.
        /// </summary>
        /// <seealso cref="TrackedDeviceMonitor"/>
        class InputDeviceMonitor
        {
            /// <summary>
            /// Event that is invoked one time when the device is tracked.
            /// </summary>
            /// <seealso cref="AddDevice"/>
            /// <seealso cref="CommonUsages.isTracked"/>
            /// <seealso cref="InputTracking.trackingAcquired"/>
            public event Action<InputDevice> trackingAcquired;

            readonly List<InputDevice> m_MonitoredDevices = new List<InputDevice>();

            bool m_Subscribed;

            /// <summary>
            /// Add an input device to monitor and invoke <see cref="trackingAcquired"/>
            /// one time when the device is tracked. The device is automatically removed
            /// from being monitored when tracking is acquired.
            /// </summary>
            /// <param name="device">The input device to start monitoring.</param>
            /// <remarks>
            /// Waits until the next global tracking acquired event to read if the device is tracked.
            /// </remarks>
            public void AddDevice(InputDevice device)
            {
                // Start subscribing if necessary
                if (!m_MonitoredDevices.Contains(device))
                {
                    m_MonitoredDevices.Add(device);
                    Subscribe();
                }
            }

            /// <summary>
            /// Stop monitoring the device for its tracked status.
            /// </summary>
            /// <param name="device">The input device to stop monitoring</param>
            public void RemoveDevice(InputDevice device)
            {
                // Stop subscribing if there are no devices left to monitor
                if (m_MonitoredDevices.Remove(device) && m_MonitoredDevices.Count == 0)
                    Unsubscribe();
            }

            /// <summary>
            /// Stop monitoring all devices for their tracked status.
            /// </summary>
            public void ClearAllDevices()
            {
                if (m_MonitoredDevices.Count > 0)
                {
                    m_MonitoredDevices.Clear();
                    Unsubscribe();
                }
            }

            void Subscribe()
            {
                if (!m_Subscribed && m_MonitoredDevices.Count > 0)
                {
                    InputTracking.trackingAcquired += OnTrackingAcquired;
                    m_Subscribed = true;
                }
            }

            void Unsubscribe()
            {
                if (m_Subscribed)
                {
                    InputTracking.trackingAcquired -= OnTrackingAcquired;
                    m_Subscribed = false;
                }
            }

            void OnTrackingAcquired(XRNodeState nodeState)
            {
                // The XRNodeState argument is ignored since there can be overlap of different input devices
                // at that XRNode, so instead each monitored device is read for its IsTracked state.
                // If the InputDevice constructor is ever made public instead of internal, we could instead just
                // get the InputDevice from the XRNodeState.uniqueID since that corresponds to the InputDevice.deviceId.
                // For the typically small number of devices monitored, an extra read call is not too expensive.

                for (var index = 0; index < m_MonitoredDevices.Count; ++index)
                {
                    var device = m_MonitoredDevices[index];
                    if (!(device.TryGetFeatureValue(CommonUsages.isTracked, out var isTracked) && isTracked))
                        continue;

                    // Stop monitoring and invoke event
                    m_MonitoredDevices.RemoveAt(index);
                    --index;

                    trackingAcquired?.Invoke(device);
                }

                // Once all monitored devices have been tracked, unsubscribe from the global event
                if (m_MonitoredDevices.Count == 0)
                    Unsubscribe();
            }
        }
    }
}
