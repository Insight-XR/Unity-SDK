using NUnit.Framework;

#if ENABLE_VR || ENABLE_AR
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    [TestFixture]
    class XRControllerTests
    {
        // ReSharper disable once ClassNeverInstantiated.Local -- MonoBehaviour class
        class XRControllerWrapper : XRController
        {
            public void FakeUpdate()
            {
                var controllerState = new XRControllerState();
                UpdateTrackingInput(controllerState);
                ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase.Dynamic, controllerState);
            }

        }

#if ENABLE_VR || ENABLE_AR
        class TestPoseProvider : BasePoseProvider
        {
            public static readonly Vector3 testPosition = new Vector3(1f, 2f, 3f);
            public static readonly Quaternion testRotation = new Quaternion(0.09853293f, 0.09853293f, 0.09853293f, 0.9853293f);

            public override PoseDataFlags GetPoseFromProvider(out Pose output)
            {
                var tmp = new Pose
                {
                    position = testPosition,
                    rotation = testRotation,
                };
                output = tmp;
                return PoseDataFlags.Position | PoseDataFlags.Rotation;
            }
        }
#endif
        static XRDirectInteractor CreateDirectInteractorWithWrappedXRController()
        {
            var interactorGO = new GameObject();
            CreateGOSphereCollider(interactorGO);
            var controllerWrapper = interactorGO.AddComponent<XRControllerWrapper>();
            var interactor = interactorGO.AddComponent<XRDirectInteractor>();
#if ENABLE_VR || ENABLE_AR
            var tpp = interactorGO.AddComponent<TestPoseProvider>();
            controllerWrapper.poseProvider = tpp;
#endif
            return interactor;
        }

#if ENABLE_VR || ENABLE_AR
        [Test]
        public void XRControllerPoseProviderTest()
        {
            TestUtilities.CreateInteractionManager();
            var directInteractor = CreateDirectInteractorWithWrappedXRController();
            var controllerWrapper = directInteractor.GetComponent<XRControllerWrapper>();
            Assert.That(controllerWrapper, Is.Not.Null);

            var tpp = directInteractor.GetComponent<TestPoseProvider>();
            Assert.That(controllerWrapper.poseProvider, Is.EqualTo(tpp));

            controllerWrapper.FakeUpdate();

            Assert.That(controllerWrapper.gameObject.transform.position, Is.EqualTo(TestPoseProvider.testPosition));
            Assert.That(controllerWrapper.gameObject.transform.rotation.Equals(TestPoseProvider.testRotation));
        }
#endif

        static void CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            var collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
        }

        [TearDown]
        public void TearDown()
        {
            TestUtilities.DestroyAllSceneObjects();
        }
    }
}
