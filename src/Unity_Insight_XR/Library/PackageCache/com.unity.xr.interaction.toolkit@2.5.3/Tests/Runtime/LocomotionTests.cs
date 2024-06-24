using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class LocomotionTests
    {
        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndMatchTargetUp()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            LocomotionSystem locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.TargetUp;

            // set teleportation anchor plane in the forward direction of controller
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-45, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);
            Vector3 cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(teleAnchor.transform.up).Using(Vector3ComparerWithEqualsOperator.Instance));
            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(xrOrigin.Camera.transform.forward, teleAnchor.transform.up);
            Assert.That(projectedCameraForward.normalized, Is.EqualTo(teleAnchor.transform.forward).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndMatchWorldSpace()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            LocomotionSystem locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.WorldSpaceUp;

            // set teleportation anchor plane in the forward direction of controller
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-45, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);
            Vector3 cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance), "XR Origin position");
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(Vector3.up).Using(Vector3ComparerWithEqualsOperator.Instance), "XR Origin up vector");
            Assert.That(xrOrigin.Camera.transform.forward, Is.EqualTo(Vector3.forward).Using(Vector3ComparerWithEqualsOperator.Instance), "Projected forward");
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndMatchTargetUpAndForward()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            LocomotionSystem locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.TargetUpAndForward;

            // set teleportation anchor plane in the forward direction of controller
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-45, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);
            Vector3 cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(teleAnchor.transform.up).Using(Vector3ComparerWithEqualsOperator.Instance));
            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(xrOrigin.Camera.transform.forward, teleAnchor.transform.up);
            Assert.That(projectedCameraForward.normalized, Is.EqualTo(teleAnchor.transform.forward).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndFilterByHitNormal()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            var locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            var teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchor with plane as child so the hit normal can be misaligned with the anchor's up
            var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "plane";
            var planeTrans = plane.transform;
            var teleAnchorTrans = new GameObject("teleportation anchor").transform;
            planeTrans.SetParent(teleAnchorTrans);
            planeTrans.Rotate(-45, 0, 0, Space.World);
            teleAnchorTrans.position = interactor.transform.forward + Vector3.down;
            var teleAnchor = teleAnchorTrans.gameObject.AddComponent<TeleportationAnchor>();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.TargetUpAndForward;
            teleAnchor.filterSelectionByHitNormal = true;
            teleAnchor.upNormalToleranceDegrees = 30f;

            var cameraTrans = xrOrigin.Camera.transform;
            var originalCameraPosition = cameraTrans.position;
            var originalCameraForward = cameraTrans.forward;
            var originTrans = xrOrigin.transform;
            var originalOriginUp = originTrans.up;

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action. first teleportation should fail
            yield return new WaitForSeconds(1f);
            Assert.That(cameraTrans.position, Is.EqualTo(originalCameraPosition).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(originTrans.up, Is.EqualTo(originalOriginUp).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(cameraTrans.forward, Is.EqualTo(originalCameraForward).Using(Vector3ComparerWithEqualsOperator.Instance));

            // now increase the normal tolerance and try teleporting again
            controllerRecorder.isPlaying = false;
            controllerRecorder.ResetPlayback();
            teleAnchor.upNormalToleranceDegrees = 50f;
            controllerRecorder.isPlaying = true;
            yield return new WaitForSeconds(1f);
            var cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(teleAnchor.transform.up).Using(Vector3ComparerWithEqualsOperator.Instance));
            var projectedCameraForward = Vector3.ProjectOnPlane(xrOrigin.Camera.transform.forward, teleAnchor.transform.up);
            Assert.That(projectedCameraForward.normalized, Is.EqualTo(teleAnchor.transform.forward).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithProjectile()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            LocomotionSystem locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.ProjectileCurve;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.TargetUp;

            // set teleportation anchor plane
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-90, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);

            Vector3 cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(teleAnchor.transform.up).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithBezierCurve()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            LocomotionSystem locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            TeleportationProvider teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.BezierCurve;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.TargetUp;

            // set teleportation anchor plane
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-90, 0, 0, Space.World);

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.2f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);

            Vector3 cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(teleAnchor.transform.up).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndMatchWorldUpAndDirectionalInput()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            var locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            var teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // fake directional input by manually rotating the attach transform
            var attachTransform = interactor.attachTransform;
            attachTransform.Rotate(Vector3.up, 30f);

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.WorldSpaceUp;
            teleAnchor.matchDirectionalInput = true;

            // set teleportation anchor plane in the forward direction of controller
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-45, 0, 0, Space.World);

            // calculate expected forward direction AFTER rotating attach transform but BEFORE the controller performs the fake teleportation
            var expectedForward = Vector3.ProjectOnPlane(attachTransform.forward, Vector3.up).normalized;

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);
            var cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(Vector3.up).Using(Vector3ComparerWithEqualsOperator.Instance), "XR Origin up vector");
            Assert.That(xrOrigin.Camera.transform.forward, Is.EqualTo(expectedForward).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator TeleportToAnchorWithStraightLineAndMatchTargetUpAndDirectionalInput()
        {
            var manager = TestUtilities.CreateInteractionManager();
            var xrOrigin = TestUtilities.CreateXROrigin();

            // config teleportation on XR Origin
            var locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            var teleProvider = xrOrigin.gameObject.AddComponent<TeleportationProvider>();
            teleProvider.system = locoSys;

            // interactor
            var interactor = TestUtilities.CreateRayInteractor();

            interactor.transform.SetParent(xrOrigin.CameraFloorOffsetObject.transform);
            interactor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;
            interactor.lineType = XRRayInteractor.LineType.StraightLine;

            // controller
            var controller = interactor.GetComponent<XRController>();

            // fake directional input by manually rotating the attach transform
            var attachTransform = interactor.attachTransform;
            attachTransform.Rotate(Vector3.up, 30f);

            // create teleportation anchors
            var teleAnchor = TestUtilities.CreateTeleportAnchorPlane();
            teleAnchor.interactionManager = manager;
            teleAnchor.teleportationProvider = teleProvider;
            teleAnchor.matchOrientation = MatchOrientation.TargetUp;
            teleAnchor.matchDirectionalInput = true;

            // set teleportation anchor plane in the forward direction of controller
            teleAnchor.transform.position = interactor.transform.forward + Vector3.down;
            teleAnchor.transform.Rotate(-45, 0, 0, Space.World);

            // calculate expected forward direction AFTER rotating attach transform but BEFORE the controller performs the fake teleportation
            var expectedForward = Vector3.ProjectOnPlane(attachTransform.forward, teleAnchor.transform.up).normalized;

            var controllerRecorder = TestUtilities.CreateControllerRecorder(controller, (recording) =>
            {
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.0f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(0.1f, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    true, false, false));
                recording.AddRecordingFrameNonAlloc(new XRControllerState(float.MaxValue, Vector3.zero, Quaternion.identity, InputTrackingState.All, true,
                    false, false, false));
            });
            controllerRecorder.isPlaying = true;

            // wait for 1s to make sure the recorder simulates the action
            yield return new WaitForSeconds(1f);
            var cameraPosAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            Assert.That(xrOrigin.Camera.transform.position, Is.EqualTo(teleAnchor.transform.position + cameraPosAdjustment).Using(Vector3ComparerWithEqualsOperator.Instance));
            Assert.That(xrOrigin.Origin.transform.up, Is.EqualTo(teleAnchor.transform.up).Using(Vector3ComparerWithEqualsOperator.Instance), "XR Origin up vector");
            var projectedCameraForward = Vector3.ProjectOnPlane(xrOrigin.Camera.transform.forward, teleAnchor.transform.up);
            Assert.That(projectedCameraForward.normalized, Is.EqualTo(expectedForward).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator SnapTurn()
        {
            var xrOrigin = TestUtilities.CreateXROrigin();

            // Config snap turn on XR Origin
            var locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            locoSys.xrOrigin = xrOrigin;
            var snapProvider = xrOrigin.gameObject.AddComponent<DeviceBasedSnapTurnProvider>();
            snapProvider.system = locoSys;
            float turnAmount = snapProvider.turnAmount;

            snapProvider.FakeStartTurn(false);

            yield return new WaitForSeconds(0.1f);

            Assert.That(xrOrigin.transform.rotation.eulerAngles, Is.EqualTo(new Vector3(0f, turnAmount, 0f)).Using(Vector3ComparerWithEqualsOperator.Instance));
        }

        [UnityTest]
        public IEnumerator SnapTurnAround()
        {
            var xrOrigin = TestUtilities.CreateXROrigin();

            // Config snap turn on XR Origin
            var locoSys = xrOrigin.gameObject.AddComponent<LocomotionSystem>();
            locoSys.xrOrigin = xrOrigin;
            var snapProvider = xrOrigin.gameObject.AddComponent<DeviceBasedSnapTurnProvider>();
            snapProvider.system = locoSys;

            snapProvider.FakeStartTurnAround();

            yield return new WaitForSeconds(0.1f);

            Assert.That(xrOrigin.transform.rotation.eulerAngles, Is.EqualTo(new Vector3(0f, 180f, 0f)).Using(Vector3ComparerWithEqualsOperator.Instance));
        }
    }
}
