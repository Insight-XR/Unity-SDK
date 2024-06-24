using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.ARFoundation;
#endif

namespace UnityEditor.XR.Interaction.Toolkit
{
    /// <summary>
    /// A helper class for <see cref="CreateUtils"/>, isolating the creation logic for Main Cameras into
    /// a separate file.
    /// </summary>
    static class XRMainCameraFactory
    {
        internal static Camera CreateXRMainCamera(CreateUtils.HardwareTarget target, CreateUtils.InputType inputType)
        {
            switch (target)
            {
                case CreateUtils.HardwareTarget.VR:
                    return CreateVRMainCamera(inputType);
                case CreateUtils.HardwareTarget.AR:
                    return CreateARMainCamera(inputType);
                default:
                    throw new InvalidEnumArgumentException($"Invalid {nameof(CreateUtils.HardwareTarget)}: {target}");
            }
        }

        static Camera CreateVRMainCamera(CreateUtils.InputType inputType)
        {
            var camera = CreateMainCamera();
            camera.nearClipPlane = 0.01f;

            SetupInput(camera, inputType);
            return camera;
        }

        static Camera CreateARMainCamera(CreateUtils.InputType inputType)
        {
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                Debug.LogWarningFormat(
                    mainCam.gameObject,
                    "XR Origin Main Camera requires the \"MainCamera\" tag, but the current scene contains another enabled Camera tagged \"MainCamera\". For AR to function properly, remove the \"MainCamera\" tag from \'{0}\' or disable it.",
                    mainCam.name);
            }

            var camera = CreateMainCamera();
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;

#if AR_FOUNDATION_PRESENT
            var cameraGo = camera.gameObject;
            cameraGo.AddComponent<ARCameraManager>();
            cameraGo.AddComponent<ARCameraBackground>();
#endif

            SetupInput(camera, inputType);
            return camera;
        }

        static Camera CreateMainCamera()
        {
            var cameraGo = ObjectFactory.CreateGameObject(
                "Main Camera",
                typeof(Camera),
                typeof(AudioListener));

            var camera = cameraGo.GetComponent<Camera>();
            camera.tag = "MainCamera";

            return camera;
        }

        static void SetupInput(Camera camera, CreateUtils.InputType inputType)
        {
            switch (inputType)
            {
                case CreateUtils.InputType.ActionBased:
                    SetupActionBasedInput(camera);
                    break;
                case CreateUtils.InputType.DeviceBased:
                    SetupDeviceBasedInput(camera);
                    break;
                default:
                    throw new InvalidEnumArgumentException($"Invalid {nameof(CreateUtils.InputType)}: {inputType}");
            }
        }

        static void SetupActionBasedInput(Camera camera)
        {
            var trackedPoseDriver = camera.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();

            var positionAction = new InputAction("Position", binding: "<XRHMD>/centerEyePosition", expectedControlType: "Vector3");
            positionAction.AddBinding("<HandheldARInputDevice>/devicePosition");
            var rotationAction = new InputAction("Rotation", binding: "<XRHMD>/centerEyeRotation", expectedControlType: "Quaternion");
            rotationAction.AddBinding("<HandheldARInputDevice>/deviceRotation");
#if INPUT_SYSTEM_1_1_OR_NEWER && !INPUT_SYSTEM_1_1_PREVIEW // 1.1.0-pre.6 or newer, excluding older preview
            trackedPoseDriver.positionInput = new InputActionProperty(positionAction);
            trackedPoseDriver.rotationInput = new InputActionProperty(rotationAction);
#if INPUT_SYSTEM_1_5_OR_NEWER
            var trackingStateAction = new InputAction("Tracking State", binding: "<XRHMD>/trackingState", expectedControlType: "Integer");
            trackedPoseDriver.trackingStateInput = new InputActionProperty(trackingStateAction);
            trackedPoseDriver.ignoreTrackingState = false;
#endif
#else
            trackedPoseDriver.positionAction = positionAction;
            trackedPoseDriver.rotationAction = rotationAction;
#endif
        }

        static void SetupDeviceBasedInput(Camera camera)
        {
            var trackedPoseDriver = camera.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();

            trackedPoseDriver.SetPoseSource(
                UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice,
                UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center);
        }
    }
}
