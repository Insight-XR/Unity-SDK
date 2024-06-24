// ENABLE_VR is not defined on Game Core but the assembly is available with limited features when the XR module is enabled.
// These are the guards that Input System uses in GenericXRDevice.cs to define the XRController and XRHMD classes.
#if ENABLE_VR || UNITY_GAMECORE
#define XR_INPUT_DEVICES_AVAILABLE
#endif

using System.Collections.Generic;
using UnityEngine.InputSystem;

#if XR_INPUT_DEVICES_AVAILABLE
using UnityEngine.InputSystem.XR;
#endif

#if XR_HANDS_1_1_OR_NEWER
using UnityEngine.XR.Hands;
#endif

#if OPENXR_1_6_OR_NEWER
using UnityEngine.XR.OpenXR.Features.Interactions;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Inputs
{
    /// <summary>
    /// Tracking status of the device in a unified format.
    /// </summary>
    /// <seealso cref="XRInputTrackingAggregator"/>
    public struct TrackingStatus
    {
        /// <summary>
        /// Whether the device is available.
        /// </summary>
        /// <seealso cref="UnityEngine.InputSystem.InputDevice.added"/>
        /// <seealso cref="InputDevice.isValid"/>
        /// <seealso cref="XRHandSubsystem"/>
        public bool isConnected { get; set; }

        /// <summary>
        /// Whether the device is tracked.
        /// </summary>
        /// <seealso cref="TrackedDevice.isTracked"/>
        /// <seealso cref="EyeGazeInteraction.EyeGazeDevice.pose"/>
        /// <seealso cref="PoseControl.isTracked"/>
        /// <seealso cref="CommonUsages.isTracked"/>
        /// <seealso cref="XRHand.isTracked"/>
        public bool isTracked { get; set; }

        /// <summary>
        /// Whether the device tracking values are valid.
        /// </summary>
        /// <seealso cref="TrackedDevice.trackingState"/>
        /// <seealso cref="EyeGazeInteraction.EyeGazeDevice.pose"/>
        /// <seealso cref="PoseControl.trackingState"/>
        /// <seealso cref="CommonUsages.trackingState"/>
        /// <seealso cref="XRHand.isTracked"/>
        public InputTrackingState trackingState { get; set; }
    }

    /// <summary>
    /// Provides methods for obtaining the tracking status of XR devices registered with Unity
    /// without needing to know the input system it is sourced from.
    /// </summary>
    /// <remarks>
    /// XR devices may be added to Unity through different mechanisms, such as native XR devices registered
    /// with the XR module, real or simulated devices registered with the Input System package, or the
    /// Hand Tracking Subsystem of OpenXR.
    /// </remarks>
    public static class XRInputTrackingAggregator
    {
        /// <summary>
        /// Provides shortcut properties for describing XR module input device characteristics for common XR devices.
        /// </summary>
        public static class Characteristics
        {
            /// <summary>
            /// HMD characteristics.
            /// <see cref="InputDeviceCharacteristics.HeadMounted"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/>
            /// </summary>
            public static InputDeviceCharacteristics hmd => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;

            /// <summary>
            /// Eye gaze characteristics.
            /// <see cref="InputDeviceCharacteristics.HeadMounted"/> <c>|</c> <see cref="InputDeviceCharacteristics.EyeTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/>
            /// </summary>
            public static InputDeviceCharacteristics eyeGaze => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice;

            /// <summary>
            /// Left controller characteristics.
            /// <see cref="InputDeviceCharacteristics.HeldInHand"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Controller"/> <c>|</c> <see cref="InputDeviceCharacteristics.Left"/>
            /// </summary>
            public static InputDeviceCharacteristics leftController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

            /// <summary>
            /// Right controller characteristics.
            /// <see cref="InputDeviceCharacteristics.HeldInHand"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Controller"/> <c>|</c> <see cref="InputDeviceCharacteristics.Right"/>
            /// </summary>
            public static InputDeviceCharacteristics rightController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

            /// <summary>
            /// Left tracked hand characteristics.
            /// <see cref="InputDeviceCharacteristics.HandTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Left"/>
            /// </summary>
            public static InputDeviceCharacteristics leftTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left;

            /// <summary>
            /// Right tracked hand characteristics.
            /// <see cref="InputDeviceCharacteristics.HandTracking"/> <c>|</c> <see cref="InputDeviceCharacteristics.TrackedDevice"/> <c>|</c> <see cref="InputDeviceCharacteristics.Right"/>
            /// </summary>
            public static InputDeviceCharacteristics rightTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right;
        }

        /// <summary>
        /// Temporary list used when getting the XR module devices.
        /// </summary>
        static List<InputDevice> s_XRInputDevices;

#if XR_HANDS_1_1_OR_NEWER
        /// <summary>
        /// Temporary list used when getting the hand subsystems.
        /// </summary>
        static List<XRHandSubsystem> s_HandSubsystems;
#endif

        /// <summary>
        /// Gets the tracking status of the HMD device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetHMDStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<XRHMD>();
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif

            // Try XR module devices
            if (TryGetDeviceWithExactCharacteristics(Characteristics.hmd, out var xrInputDevice))
                return GetTrackingStatus(xrInputDevice);

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the eye gaze device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetEyeGazeStatus()
        {
            if (!Application.isPlaying)
                return default;

#if OPENXR_1_6_OR_NEWER
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<EyeGazeInteraction.EyeGazeDevice>();
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif

            // Try XR module devices
            if (TryGetDeviceWithExactCharacteristics(Characteristics.eyeGaze, out var xrInputDevice))
                return GetTrackingStatus(xrInputDevice);

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the left motion controller device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetLeftControllerStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<InputSystem.XR.XRController>(InputSystem.CommonUsages.LeftHand);
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif

            // Try XR module devices
            if (TryGetDeviceWithExactCharacteristics(Characteristics.leftController, out var xrInputDevice))
                return GetTrackingStatus(xrInputDevice);

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the right motion controller device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetRightControllerStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<InputSystem.XR.XRController>(InputSystem.CommonUsages.RightHand);
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif

            // Try XR module devices
            if (TryGetDeviceWithExactCharacteristics(Characteristics.rightController, out var xrInputDevice))
                return GetTrackingStatus(xrInputDevice);

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the left tracked hand device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetLeftTrackedHandStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_HANDS_1_1_OR_NEWER
            // Try XR Hand Subsystem devices
            if (TryGetHandSubsystem(out var handSubsystem))
                return GetTrackingStatus(handSubsystem.leftHand);

#if XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<XRHandDevice>(InputSystem.CommonUsages.LeftHand);
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif
#endif

            // Try XR module devices
            if (TryGetDeviceWithExactCharacteristics(Characteristics.leftTrackedHand, out var xrInputDevice))
                return GetTrackingStatus(xrInputDevice);

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the right tracked hand device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetRightTrackedHandStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_HANDS_1_1_OR_NEWER
            // Try XR Hand Subsystem devices
            if (TryGetHandSubsystem(out var handSubsystem))
                return GetTrackingStatus(handSubsystem.rightHand);

#if XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<XRHandDevice>(InputSystem.CommonUsages.RightHand);
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif
#endif

            // Try XR module devices
            if (TryGetDeviceWithExactCharacteristics(Characteristics.rightTrackedHand, out var xrInputDevice))
                return GetTrackingStatus(xrInputDevice);

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the left Meta Hand Tracking Aim device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetLeftMetaAimHandStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_HANDS_1_1_OR_NEWER && XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<MetaAimHand>(InputSystem.CommonUsages.LeftHand);
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif

            return default;
        }

        /// <summary>
        /// Gets the tracking status of the right Meta Hand Tracking Aim device for this frame.
        /// </summary>
        /// <returns>Returns a snapshot of the tracking status for this frame.</returns>
        public static TrackingStatus GetRightMetaAimHandStatus()
        {
            if (!Application.isPlaying)
                return default;

#if XR_HANDS_1_1_OR_NEWER && XR_INPUT_DEVICES_AVAILABLE
            // Try Input System devices
            var currentDevice = InputSystem.InputSystem.GetDevice<MetaAimHand>(InputSystem.CommonUsages.RightHand);
            if (currentDevice != null)
                return GetTrackingStatus(currentDevice);
#endif

            return default;
        }

#if XR_HANDS_1_1_OR_NEWER
        /// <summary>
        /// Gets the first hand subsystem. If there are multiple, returns the first running subsystem.
        /// </summary>
        /// <param name="handSubsystem">When this method returns, contains the hand subsystem if found.</param>
        /// <returns>Returns <see langword="true"/> if a hand subsystem was found. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="SubsystemManager.GetSubsystems{T}"/>
        internal static bool TryGetHandSubsystem(out XRHandSubsystem handSubsystem)
        {
            s_HandSubsystems ??= new List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(s_HandSubsystems);
            if (s_HandSubsystems.Count == 0)
            {
                handSubsystem = default;
                return false;
            }

            if (s_HandSubsystems.Count > 1)
            {
                for (var i = 0; i < s_HandSubsystems.Count; ++i)
                {
                    handSubsystem = s_HandSubsystems[i];
                    if (handSubsystem.running)
                        return true;
                }
            }

            handSubsystem = s_HandSubsystems[0];
            return true;
        }
#endif

        /// <summary>
        /// Gets the first active XR input device that matches the specified <see cref="InputDeviceCharacteristics"/> exactly.
        /// </summary>
        /// <param name="desiredCharacteristics">A bitwise combination of the exact characteristics you are looking for.</param>
        /// <param name="inputDevice">When this method returns, contains the input device if a match was found.</param>
        /// <returns>Returns <see langword="true"/> if a match was found. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// This function finds any input devices available to the XR Subsystem that match the specified <see cref="InputDeviceCharacteristics"/>
        /// bitmask exactly. The function does not include devices that only provide some of the desired characteristics or capabilities.
        /// </remarks>
        /// <seealso cref="InputDevices.GetDevicesWithCharacteristics"/>
        internal static bool TryGetDeviceWithExactCharacteristics(InputDeviceCharacteristics desiredCharacteristics, out InputDevice inputDevice)
        {
            s_XRInputDevices ??= new List<InputDevice>();
            // The InputDevices.GetDevicesWithCharacteristics method does a bitwise comparison, not an equal check,
            // so it may return devices that have additional characteristic flags (HMD characteristics is a subset
            // of Eye Gaze characteristics, so this additional filtering ensures the correct device is returned if both are added).
            // Instead, get all devices and use equals to make sure the characteristics matches exactly.
            InputDevices.GetDevices(s_XRInputDevices);
            for (var index = 0; index < s_XRInputDevices.Count; ++index)
            {
                inputDevice = s_XRInputDevices[index];
                if (inputDevice.characteristics == desiredCharacteristics)
                    return true;
            }

            inputDevice = default;
            return false;
        }

        static TrackingStatus GetTrackingStatus(TrackedDevice device)
        {
            if (device == null)
                return default;

            return new TrackingStatus
            {
                isConnected = device.added,
                isTracked = device.isTracked.isPressed,
                trackingState = (InputTrackingState)device.trackingState.value,
            };
        }

#if OPENXR_1_6_OR_NEWER
        static TrackingStatus GetTrackingStatus(EyeGazeInteraction.EyeGazeDevice device)
        {
            if (device == null)
                return default;

            return new TrackingStatus
            {
                isConnected = device.added,
                isTracked = device.pose.isTracked.isPressed,
                trackingState = (InputTrackingState)device.pose.trackingState.value,
            };
        }
#endif

        static TrackingStatus GetTrackingStatus(InputDevice device)
        {
            return new TrackingStatus
            {
                isConnected = device.isValid,
                isTracked = device.TryGetFeatureValue(CommonUsages.isTracked, out var isTracked) && isTracked,
                trackingState = device.TryGetFeatureValue(CommonUsages.trackingState, out var trackingState) ? trackingState : InputTrackingState.None,
            };
        }

#if XR_HANDS_1_1_OR_NEWER
        static TrackingStatus GetTrackingStatus(XRHand hand)
        {
            return new TrackingStatus
            {
                isConnected = true,
                isTracked = hand.isTracked,
                trackingState = hand.isTracked ? InputTrackingState.Position | InputTrackingState.Rotation : InputTrackingState.None,
            };
        }
#endif
    }
}
