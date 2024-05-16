using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.XR.CoreUtils;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace UnityEngine.XR.Interaction.Toolkit.Tests
{
    internal static class TestUtilities
    {
        internal static void DestroyAllSceneObjects()
        {
            for (var index = 0; index < SceneManager.sceneCount; ++index)
            {
                var scene = SceneManager.GetSceneAt(index);
                foreach (var go in scene.GetRootGameObjects())
                {
                    if (go.name.Contains("tests runner"))
                        continue;
                    Object.DestroyImmediate(go);
                }
            }
        }

        internal static BoxCollider CreateGOBoxCollider(GameObject go, bool isTrigger = true)
        {
            BoxCollider collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(2.0f, 2.0f, 2.0f);
            collider.isTrigger = isTrigger;
            return collider;
        }

        internal static SphereCollider CreateGOSphereCollider(GameObject go, bool isTrigger = true)
        {
            SphereCollider collider = go.AddComponent<SphereCollider>();
            collider.radius = 1.0f;
            collider.isTrigger = isTrigger;
            return collider;
        }

        internal static XRInteractionManager CreateInteractionManager()
        {
            GameObject managerGO = new GameObject("Interaction Manager");
            XRInteractionManager manager = managerGO.AddComponent<XRInteractionManager>();
            return manager;
        }

        internal static XRDirectInteractor CreateDirectInteractor()
        {
            GameObject interactorGO = new GameObject("Direct Interactor");
            CreateGOSphereCollider(interactorGO);
            XRController controller = interactorGO.AddComponent<XRController>();
            XRDirectInteractor interactor = interactorGO.AddComponent<XRDirectInteractor>();
            interactor.xrController = controller;
            controller.enableInputTracking = false;
            controller.enableInputActions = false;
            return interactor;
        }

        internal static XRPokeInteractor CreatePokeInteractor()
        {
            GameObject interactorGO = new GameObject("Poke Interactor");
            XRPokeInteractor interactor = interactorGO.AddComponent<XRPokeInteractor>();
            return interactor;
        }

        internal static XROrigin CreateXROrigin()
        {
            var xrOriginGO = new GameObject("XR Origin");
            xrOriginGO.SetActive(false);
            var xrOrigin = xrOriginGO.AddComponent<XROrigin>();
            xrOrigin.Origin = xrOriginGO;

            // Add camera offset
            var cameraOffsetGO = new GameObject("CameraOffset");
            cameraOffsetGO.transform.SetParent(xrOrigin.transform,false);
            xrOrigin.CameraFloorOffsetObject = cameraOffsetGO;

            xrOrigin.transform.position = Vector3.zero;
            xrOrigin.transform.rotation = Quaternion.identity;

            // Add camera
            var cameraGO = new GameObject("Camera");
            var camera = cameraGO.AddComponent<Camera>();

            cameraGO.transform.SetParent(cameraOffsetGO.transform, false);
            xrOrigin.Camera = cameraGO.GetComponent<Camera>();
            xrOriginGO.SetActive(true);

#if ENABLE_VR
            XRDevice.DisableAutoXRCameraTracking(camera, true);
#endif

            return xrOrigin;
        }

        internal static TeleportationAnchor CreateTeleportAnchorPlane()
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "plane";
            TeleportationAnchor teleAnchor = plane.AddComponent<TeleportationAnchor>();
            return teleAnchor;
        }

        internal static XRRayInteractor CreateRayInteractor()
        {
            GameObject interactorGO = new GameObject("Ray Interactor");
            XRController controller = interactorGO.AddComponent<XRController>();
            XRRayInteractor interactor = interactorGO.AddComponent<XRRayInteractor>();
            XRInteractorLineVisual ilv = interactorGO.AddComponent<XRInteractorLineVisual>();
            interactor.xrController = controller;
            controller.enableInputTracking = false;
            interactor.enableUIInteraction = false;
            controller.enableInputActions = false;
            return interactor;
        }

        internal static XRGazeInteractor CreateGazeInteractor()
        {
            GameObject interactorGO = new GameObject("Gaze Interactor");
            XRController controller = interactorGO.AddComponent<XRController>();
            XRGazeInteractor interactor = interactorGO.AddComponent<XRGazeInteractor>();
            interactor.xrController = controller;
            controller.enableInputTracking = false;
            interactor.enableUIInteraction = false;
            controller.enableInputActions = false;
            return interactor;
        }

        internal static XRSocketInteractor CreateSocketInteractor()
        {
            GameObject interactorGO = new GameObject("Socket Interactor");
            CreateGOSphereCollider(interactorGO);
            XRSocketInteractor interactor = interactorGO.AddComponent<XRSocketInteractor>();
            return interactor;
        }

        internal static MockInteractor CreateMockInteractor()
        {
            var interactorGO = new GameObject("Mock Interactor");
            interactorGO.transform.localPosition = Vector3.zero;
            interactorGO.transform.localRotation = Quaternion.identity;
            var interactor = interactorGO.AddComponent<MockInteractor>();
            return interactor;
        }

        internal static MockClassInteractable CreateMockClassInteractable()
        {
            var interactableGO = new GameObject("Mock Interactable");
            interactableGO.transform.localPosition = Vector3.zero;
            interactableGO.transform.localRotation = Quaternion.identity;
            var interactable = new MockClassInteractable(interactableGO.transform);
            return interactable;
        }

        internal static XRGrabInteractable CreateGrabInteractable()
        {
            GameObject interactableGO = new GameObject("Grab Interactable");
            CreateGOSphereCollider(interactableGO, false);
            XRGrabInteractable interactable = interactableGO.AddComponent<XRGrabInteractable>();
            interactable.throwOnDetach = false;
            var rigidBody = interactableGO.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            return interactable;
        }

        internal static XRSimpleInteractable CreateSimpleInteractable()
        {
            GameObject interactableGO = new GameObject("Simple Interactable");
            CreateGOSphereCollider(interactableGO, false);
            XRSimpleInteractable interactable = interactableGO.AddComponent<XRSimpleInteractable>();
            Rigidbody rigidBody = interactableGO.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            return interactable;
        }

        internal static XRSimpleInteractable CreateTriggerInteractable()
        {
            GameObject interactableGO = new GameObject("Trigger Interactable");
            var collider = CreateGOSphereCollider(interactableGO, false);
            XRSimpleInteractable interactable = interactableGO.AddComponent<XRSimpleInteractable>();
            Rigidbody rigidBody = interactableGO.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            collider.isTrigger = true;      // We set the trigger here, rather than using the function argument so that the collider gets added to the interactable's list of colliders
            return interactable;
        }

        internal static XRInteractableSnapVolume CreateSnapVolume()
        {
            GameObject snapVolumeGO = new GameObject("Snap Volume");
            CreateGOBoxCollider(snapVolumeGO, true);
            var boxCollider = snapVolumeGO.GetComponent<BoxCollider>();
            XRInteractableSnapVolume snapVolume = snapVolumeGO.AddComponent<XRInteractableSnapVolume>();
            snapVolume.snapCollider = boxCollider;
            return snapVolume;
        }

        internal static XRSimpleInteractable CreateMultiSelectableSimpleInteractable()
        {
            var interactable = CreateSimpleInteractable();
            interactable.selectMode = InteractableSelectMode.Multiple;
            return interactable;
        }

        internal static XRSimpleInteractable CreateSimpleInteractableWithColliders()
        {
            GameObject interactableGO = new GameObject("Simple Interactable with Colliders");
            CreateGOSphereCollider(interactableGO, false);
            CreateGOBoxCollider(interactableGO, false);
            XRSimpleInteractable interactable = interactableGO.AddComponent<XRSimpleInteractable>();
            Rigidbody rigidBody = interactableGO.AddComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.isKinematic = true;
            return interactable;
        }

        internal static XRControllerRecorder CreateControllerRecorder(XRController controller, Action<XRControllerRecording> addRecordingFrames)
        {
            var controllerRecorder = controller.gameObject.AddComponent<XRControllerRecorder>();
            controllerRecorder.xrController = controller;
            controllerRecorder.recording = ScriptableObject.CreateInstance<XRControllerRecording>();
            addRecordingFrames(controllerRecorder.recording);
            controllerRecorder.recording.SetFrameDependentData();
            return controllerRecorder;
        }

        internal static XRTargetFilter CreateTargetFilter()
        {
            GameObject filterGO = new GameObject("Target Filter");
            return filterGO.AddComponent<XRTargetFilter>();
        }

        internal static XRInteractionGroup CreateInteractionGroup()
        {
            var groupGO = new GameObject("Interaction Group");
            groupGO.transform.localPosition = Vector3.zero;
            groupGO.transform.localRotation = Quaternion.identity;
            var group = groupGO.AddComponent<XRInteractionGroup>();
            return group;
        }

        internal static XRInteractionGroup CreateGroupWithMockInteractors(out MockInteractor memberInteractor1,
            out MockInteractor memberInteractor2, out MockInteractor memberInteractor3)
        {
            var group = CreateInteractionGroup();
            memberInteractor1 = CreateMockInteractor();
            memberInteractor2 = CreateMockInteractor();
            memberInteractor3 = CreateMockInteractor();
            group.AddGroupMember(memberInteractor1);
            group.AddGroupMember(memberInteractor2);
            group.AddGroupMember(memberInteractor3);
            return group;
        }

        internal static XRInteractionGroup CreateGroupWithHoverOnlyMockInteractors(out MockInteractor memberInteractor1,
            out MockInteractor memberInteractor2, out MockInteractor memberInteractor3)
        {
            var group = CreateInteractionGroup();
            memberInteractor1 = CreateMockInteractor();
            memberInteractor2 = CreateMockInteractor();
            memberInteractor3 = CreateMockInteractor();
            memberInteractor1.allowSelect = false;
            memberInteractor2.allowSelect = false;
            memberInteractor3.allowSelect = false;
            group.AddGroupMember(memberInteractor1);
            group.AddGroupMember(memberInteractor2);
            group.AddGroupMember(memberInteractor3);
            return group;
        }

        internal static XRInteractionGroup CreateGroupWithEmptyGroups(out XRInteractionGroup memberGroup1,
            out XRInteractionGroup memberGroup2, out XRInteractionGroup memberGroup3)
        {
            var group = CreateInteractionGroup();
            memberGroup1 = CreateInteractionGroup();
            memberGroup2 = CreateInteractionGroup();
            memberGroup3 = CreateInteractionGroup();
            group.AddGroupMember(memberGroup1);
            group.AddGroupMember(memberGroup2);
            group.AddGroupMember(memberGroup3);
            return group;
        }
    }

    class MockInteractor : XRBaseInteractor
    {
        public event Action<XRInteractionUpdateOrder.UpdatePhase> preprocessed;
        public event Action<XRInteractionUpdateOrder.UpdatePhase> processed;
        public List<IXRInteractable> validTargets { get; } = new List<IXRInteractable>();

        /// <inheritdoc />
        public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.PreprocessInteractor(updatePhase);
            preprocessed?.Invoke(updatePhase);
        }

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);
            processed?.Invoke(updatePhase);
        }

        /// <inheritdoc />
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            targets.AddRange(validTargets);
        }
    }

    /// <summary>
    /// An interactable that is a plain C# object that uses a given GameObject.
    /// </summary>
    class MockClassInteractable : IXRInteractable
    {
        /// <inheritdoc />
        public event Action<InteractableRegisteredEventArgs> registered;

        /// <inheritdoc />
        public event Action<InteractableUnregisteredEventArgs> unregistered;

        /// <summary>
        /// Invoked when <see cref="ProcessInteractable"/> is called.
        /// </summary>
        public event Action<XRInteractionUpdateOrder.UpdatePhase> processed;

        /// <inheritdoc />
        public InteractionLayerMask interactionLayers { get; set; } = 1;

        /// <inheritdoc />
        public List<Collider> colliders { get; } = new List<Collider>();

        /// <inheritdoc />
        public Transform transform { get; }

        /// <summary>
        /// Whether this interactable is registered;
        /// </summary>
        public bool isRegistered { get; private set; }

        /// <summary>
        /// Constructs a new interactable. Populates <see cref="colliders"/> with non-trigger colliders.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> associated with the Interactable.</param>
        public MockClassInteractable(Transform transform)
        {
            this.transform = transform;
            transform.GetComponentsInChildren(colliders);
            colliders.RemoveAll(col => col.isTrigger);
        }

        /// <inheritdoc />
        public Transform GetAttachTransform(IXRInteractor interactor)
        {
            return transform;
        }

        /// <inheritdoc />
        public void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            processed?.Invoke(updatePhase);
        }

        /// <inheritdoc />
        public float GetDistanceSqrToInteractor(IXRInteractor interactor)
        {
            var interactorAttachTransform = interactor?.GetAttachTransform(this);
            if (interactorAttachTransform == null)
                return float.MaxValue;

            return (transform.position - interactorAttachTransform.position).sqrMagnitude;
        }

        /// <inheritdoc />
        public void OnRegistered(InteractableRegisteredEventArgs args)
        {
            isRegistered = true;
            registered?.Invoke(args);
        }

        /// <inheritdoc />
        public void OnUnregistered(InteractableUnregisteredEventArgs args)
        {
            isRegistered = false;
            unregistered?.Invoke(args);
        }
    }

    enum TargetFilterCallback
    {
        Link,
        Unlink,
        Process,
    }

    class MockTargetFilter : IXRTargetFilter
    {
        public readonly List<TargetFilterCallback> callbackExecution = new List<TargetFilterCallback>();

        public bool canProcess { get; set; } = true;

        public void Link(IXRInteractor interactor)
        {
            callbackExecution.Add(TargetFilterCallback.Link);
        }

        public void Unlink(IXRInteractor interactor)
        {
            callbackExecution.Add(TargetFilterCallback.Unlink);
        }

        public void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
        {
            results.Clear();
            callbackExecution.Add(TargetFilterCallback.Process);
            results.AddRange(targets);
        }
    }
    
    class MockInversionTargetFilter : IXRTargetFilter
    {
        public readonly List<TargetFilterCallback> callbackExecution = new List<TargetFilterCallback>();

        public bool canProcess { get; set; } = true;

        public void Link(IXRInteractor interactor)
        {
            callbackExecution.Add(TargetFilterCallback.Link);
        }

        public void Unlink(IXRInteractor interactor)
        {
            callbackExecution.Add(TargetFilterCallback.Unlink);
        }

        public void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
        {
            results.Clear();
            callbackExecution.Add(TargetFilterCallback.Process);
            for(int i = targets.Count - 1; i >= 0; i--)
            {
                results.Add(targets[i]);
            }
        }
    }

    class MockGrabTransformer : IXRGrabTransformer
    {
        public enum MethodTrace
        {
            OnLink,
            OnGrab,
            OnGrabCountChanged,
            OnDrop,
            ProcessFixed,
            ProcessDynamic,
            ProcessLate,
            ProcessOnBeforeRender,
            OnUnlink,
        }

        public List<MethodTrace> methodTraces { get; } = new List<MethodTrace>();

        public Dictionary<XRInteractionUpdateOrder.UpdatePhase, bool> phasesTraced { get; } = new Dictionary<XRInteractionUpdateOrder.UpdatePhase, bool>
        {
            { XRInteractionUpdateOrder.UpdatePhase.Fixed, false},
            { XRInteractionUpdateOrder.UpdatePhase.Dynamic, true},
            { XRInteractionUpdateOrder.UpdatePhase.Late, false},
            { XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender, false},
        };

        /// <summary>
        /// The <see cref="Pose"/> value to set in <see cref="Process"/>.
        /// Set to <see langword="null"/> to skip using it.
        /// </summary>
        public Pose? targetPoseValue { get; set; }

        /// <summary>
        /// The <see cref="Vector3"/> local scale value to set in <see cref="Process"/>.
        /// Set to <see langword="null"/> to skip using it.
        /// </summary>
        public Vector3? localScaleValue { get; set; }

        /// <inheritdoc />
        public bool canProcess { get; set; } = true;

        /// <inheritdoc />
        public void OnLink(XRGrabInteractable grabInteractable)
        {
            methodTraces.Add(MethodTrace.OnLink);
        }

        /// <inheritdoc />
        public void OnGrab(XRGrabInteractable grabInteractable)
        {
            methodTraces.Add(MethodTrace.OnGrab);
        }

        /// <inheritdoc />
        public void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
            methodTraces.Add(MethodTrace.OnGrabCountChanged);
        }

        /// <inheritdoc />
        public void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
        {
            switch (updatePhase)
            {
                case XRInteractionUpdateOrder.UpdatePhase.Fixed:
                    if (phasesTraced[updatePhase])
                        methodTraces.Add(MethodTrace.ProcessFixed);
                    break;
                case XRInteractionUpdateOrder.UpdatePhase.Dynamic:
                    if (phasesTraced[updatePhase])
                        methodTraces.Add(MethodTrace.ProcessDynamic);
                    break;
                case XRInteractionUpdateOrder.UpdatePhase.Late:
                    if (phasesTraced[updatePhase])
                        methodTraces.Add(MethodTrace.ProcessLate);
                    break;
                case XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender:
                    if (phasesTraced[updatePhase])
                        methodTraces.Add(MethodTrace.ProcessOnBeforeRender);
                    break;
                default:
                    Assert.Fail($"Unhandled {nameof(XRInteractionUpdateOrder.UpdatePhase)}={updatePhase}");
                    break;
            }

            if (targetPoseValue.HasValue)
                targetPose = targetPoseValue.Value;

            if (localScaleValue.HasValue)
                localScale = localScaleValue.Value;
        }

        /// <inheritdoc />
        public void OnUnlink(XRGrabInteractable grabInteractable)
        {
            methodTraces.Add(MethodTrace.OnUnlink);
        }
    }

    class MockDropTransformer : MockGrabTransformer, IXRDropTransformer
    {
        /// <inheritdoc />
        public bool canProcessOnDrop { get; set; } = true;

        /// <inheritdoc />
        public void OnDrop(XRGrabInteractable grabInteractable, DropEventArgs args)
        {
            Assert.That(args, Is.Not.Null);
            Assert.That(args.selectExitEventArgs, Is.Not.Null);
            Assert.That(args.selectExitEventArgs.interactableObject, Is.SameAs(grabInteractable));
            Assert.That(args.selectExitEventArgs.manager, Is.SameAs(grabInteractable.interactionManager));

            methodTraces.Add(MethodTrace.OnDrop);
        }
    }

    // TODO: This is a placeholder until we can get mock touches to work with XRUIInputModule. See ARTests.
    class MockInputModule : BaseInputModule
    {
        public override void Process()
        {
        }

        public override bool IsPointerOverGameObject(int pointerId) => true;
    }

}
