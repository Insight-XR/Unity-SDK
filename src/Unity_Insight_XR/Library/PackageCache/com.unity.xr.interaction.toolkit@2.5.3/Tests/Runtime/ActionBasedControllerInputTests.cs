// ENABLE_VR is not defined on Game Core but the assembly is available with limited features when the XR module is enabled.
// These are the guards that Input System uses in GenericXRDevice.cs to define the XRController and XRHMD classes.
#if ENABLE_VR || UNITY_GAMECORE
#define XR_INPUT_DEVICES_AVAILABLE
#endif

using System.Collections;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class ActionBasedControllerInputTests : InputTestFixture
    {
        public override void TearDown()
        {
            base.TearDown();
            TestUtilities.DestroyAllSceneObjects();
        }

#if XR_INPUT_DEVICES_AVAILABLE
        [UnityTest]
        public IEnumerator TrackingStatusUsesFallbackTrackedDevice()
        {
            // Create a single TrackedDevice, and keep the input action empty for Is Tracked and Tracking State
            // to verify that those values re-use the values from the Position/Rotation device.
            var trackedDevice = InputSystem.InputSystem.AddDevice<InputSystem.XR.XRController>();

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = asset.AddActionMap("Main");
            var positionAction = actionMap.AddAction("Position", InputActionType.Value, "<XRController>/devicePosition");
            var rotationAction = actionMap.AddAction("Rotation", InputActionType.Value, "<XRController>/deviceRotation");

            var controllerGameObject = new GameObject("Action Based Controller");
            var actionBasedController = controllerGameObject.AddComponent<ActionBasedController>();
            actionBasedController.positionAction = new InputActionProperty(positionAction);
            actionBasedController.rotationAction = new InputActionProperty(rotationAction);

            // Empty when Use Reference is disabled
            actionBasedController.isTrackedAction = new InputActionProperty(new InputAction());
            actionBasedController.trackingStateAction = new InputActionProperty(new InputAction());

            var position = new Vector3(1f, 2f, 3f);
            var rotation = Quaternion.Euler(0f, 45f, 0f);

            Set(trackedDevice.devicePosition, position);
            Set(trackedDevice.deviceRotation, rotation);
            Set(trackedDevice.isTracked, 1f);
            Set(trackedDevice.trackingState, (int)InputTrackingState.All);

            yield return null;

            var currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.True);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.All));

            // Empty when Use Reference is enabled
            actionBasedController.isTrackedAction = new InputActionProperty(null);
            actionBasedController.trackingStateAction = new InputActionProperty(null);

            yield return null;

            currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.True);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.All));
        }
#endif

#if XR_INPUT_DEVICES_AVAILABLE
        [UnityTest]
        public IEnumerator IsTrackedUsesTrackingStateFallback()
        {
            // Create two different TrackedDevice, and keep the input action empty for Is Tracked
            // to verify that it prioritizes the Tracking State device.
            var poseDevice = InputSystem.InputSystem.AddDevice<InputSystem.XR.XRController>();
            var trackingStatusDevice = InputSystem.InputSystem.AddDevice<TrackedDevice>();

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = asset.AddActionMap("Main");
            var positionAction = actionMap.AddAction("Position", InputActionType.Value, "<XRController>/devicePosition");
            var rotationAction = actionMap.AddAction("Rotation", InputActionType.Value, "<XRController>/deviceRotation");
            var trackingStateAction = actionMap.AddAction("Tracking State", InputActionType.Value, "<TrackedDevice>/trackingState");

            var controllerGameObject = new GameObject("Action Based Controller");
            var actionBasedController = controllerGameObject.AddComponent<ActionBasedController>();
            actionBasedController.positionAction = new InputActionProperty(positionAction);
            actionBasedController.rotationAction = new InputActionProperty(rotationAction);
            actionBasedController.trackingStateAction = new InputActionProperty(trackingStateAction);

            // Empty when Use Reference is disabled
            actionBasedController.isTrackedAction = new InputActionProperty(new InputAction());

            var position = new Vector3(1f, 2f, 3f);
            var rotation = Quaternion.Euler(0f, 45f, 0f);

            Set(poseDevice.devicePosition, position);
            Set(poseDevice.deviceRotation, rotation);
            Set(poseDevice.isTracked, 0f);
            Set(poseDevice.trackingState, (int)InputTrackingState.None);
            Set(trackingStatusDevice.isTracked, 1f);
            Set(trackingStatusDevice.trackingState, (int)InputTrackingState.All);

            yield return null;

            var currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.True);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.All));

            // Empty when Use Reference is enabled
            actionBasedController.isTrackedAction = new InputActionProperty(null);

            yield return null;

            currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.True);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.All));
        }
#endif

#if XR_INPUT_DEVICES_AVAILABLE
        [UnityTest]
        public IEnumerator TrackingStateUsesIsTrackedFallback()
        {
            // Create two different TrackedDevice, and keep the input action empty for Tracking State
            // to verify that it prioritizes the Is Tracked device.
            var poseDevice = InputSystem.InputSystem.AddDevice<InputSystem.XR.XRController>();
            var trackingStatusDevice = InputSystem.InputSystem.AddDevice<TrackedDevice>();

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = asset.AddActionMap("Main");
            var positionAction = actionMap.AddAction("Position", InputActionType.Value, "<XRController>/devicePosition");
            var rotationAction = actionMap.AddAction("Rotation", InputActionType.Value, "<XRController>/deviceRotation");
            var isTrackedAction = actionMap.AddAction("Is Tracked", InputActionType.Button, "<TrackedDevice>/isTracked");
            isTrackedAction.wantsInitialStateCheck = true;

            var controllerGameObject = new GameObject("Action Based Controller");
            var actionBasedController = controllerGameObject.AddComponent<ActionBasedController>();
            actionBasedController.positionAction = new InputActionProperty(positionAction);
            actionBasedController.rotationAction = new InputActionProperty(rotationAction);
            actionBasedController.isTrackedAction = new InputActionProperty(isTrackedAction);

            // Empty when Use Reference is disabled
            actionBasedController.trackingStateAction = new InputActionProperty(new InputAction());

            var position = new Vector3(1f, 2f, 3f);
            var rotation = Quaternion.Euler(0f, 45f, 0f);

            Set(poseDevice.devicePosition, position);
            Set(poseDevice.deviceRotation, rotation);
            Set(poseDevice.isTracked, 0f);
            Set(poseDevice.trackingState, (int)InputTrackingState.None);
            Set(trackingStatusDevice.isTracked, 1f);
            Set(trackingStatusDevice.trackingState, (int)InputTrackingState.All);

            yield return null;

            var currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.True);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.All));

            // Empty when Use Reference is enabled
            actionBasedController.trackingStateAction = new InputActionProperty(null);

            yield return null;

            currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.True);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.All));
        }
#endif

        [UnityTest]
        public IEnumerator TrackingStatusCombinesPositionRotationTrackedDevice()
        {
            // Create two TrackedDevice, one for Position and one for Rotation, and keep the input action empty for Is Tracked and Tracking State
            // to verify that those values re-use and combine the values from the Position and Rotation devices.
            var positionDevice = InputSystem.InputSystem.AddDevice<TrackedDevice>();
            var rotationDevice = InputSystem.InputSystem.AddDevice<TrackedDevice>();

            InputSystem.InputSystem.SetDeviceUsage(positionDevice, InputSystem.CommonUsages.LeftHand);
            InputSystem.InputSystem.SetDeviceUsage(rotationDevice, InputSystem.CommonUsages.RightHand);

            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            var actionMap = asset.AddActionMap("Main");
            var positionAction = actionMap.AddAction("Position", InputActionType.Value, "<TrackedDevice>{LeftHand}/devicePosition");
            var rotationAction = actionMap.AddAction("Rotation", InputActionType.Value, "<TrackedDevice>{RightHand}/deviceRotation");

            var controllerGameObject = new GameObject("Action Based Controller");
            var actionBasedController = controllerGameObject.AddComponent<ActionBasedController>();
            actionBasedController.positionAction = new InputActionProperty(positionAction);
            actionBasedController.rotationAction = new InputActionProperty(rotationAction);

            // Empty when Use Reference is disabled
            actionBasedController.isTrackedAction = new InputActionProperty(new InputAction());
            actionBasedController.trackingStateAction = new InputActionProperty(new InputAction());

            var firstPose = new Pose(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 45f, 0f));
            var secondPose = new Pose(new Vector3(4f, 5f, 6f), Quaternion.Euler(30f, 60f, 90f));

            Set(positionDevice.devicePosition, firstPose.position);
            Set(positionDevice.deviceRotation, firstPose.rotation);
            Set(positionDevice.isTracked, 0f);
            Set(positionDevice.trackingState, (int)InputTrackingState.All);
            Set(rotationDevice.devicePosition, secondPose.position);
            Set(rotationDevice.deviceRotation, secondPose.rotation);
            Set(rotationDevice.isTracked, 1f);
            Set(rotationDevice.trackingState, (int)InputTrackingState.All);

            yield return null;

            var currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(firstPose.position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(secondPose.rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.False);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));

            // Empty when Use Reference is enabled
            actionBasedController.isTrackedAction = new InputActionProperty(null);
            actionBasedController.trackingStateAction = new InputActionProperty(null);

            yield return null;

            currentControllerState = actionBasedController.currentControllerState;

            Assert.That(currentControllerState.position, Is.EqualTo(firstPose.position).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(currentControllerState.rotation, Is.EqualTo(secondPose.rotation).Using(QuaternionEqualityComparer.Instance));
            Assert.That(currentControllerState.isTracked, Is.False);
            Assert.That(currentControllerState.inputTrackingState, Is.EqualTo(InputTrackingState.Position | InputTrackingState.Rotation));
        }
    }
}
