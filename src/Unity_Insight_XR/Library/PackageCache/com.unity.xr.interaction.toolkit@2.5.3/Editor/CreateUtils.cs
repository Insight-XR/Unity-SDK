using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Unity.XR.CoreUtils;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.UI;

#if AR_FOUNDATION_PRESENT
using UnityEngine.XR.Interaction.Toolkit.AR;
#endif

using Object = UnityEngine.Object;

namespace UnityEditor.XR.Interaction.Toolkit
{
    static class CreateUtils
    {
        internal enum HardwareTarget
        {
            AR,
            VR,
        }

        internal enum InputType
        {
            ActionBased,
            DeviceBased,
        }

        const string k_LineMaterial = "Default-Line.mat";
        const string k_UILayerName = "UI";

        [MenuItem("GameObject/XR/Ray Interactor (Action-based)", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateRayInteractorActionBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateRayInteractor(menuCommand?.GetContextTransform(), InputType.ActionBased));
        }

        [MenuItem("GameObject/XR/Device-based/Ray Interactor", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateRayInteractorDeviceBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateRayInteractor(menuCommand?.GetContextTransform(), InputType.DeviceBased));
        }

        [MenuItem("GameObject/XR/Direct Interactor (Action-based)", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateDirectInteractorActionBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateDirectInteractor(menuCommand?.GetContextTransform(), InputType.ActionBased));
        }

        [MenuItem("GameObject/XR/Device-based/Direct Interactor", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateDirectInteractorDeviceBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateDirectInteractor(menuCommand?.GetContextTransform(), InputType.DeviceBased));
        }
        
        [MenuItem("GameObject/XR/Gaze Interactor (Action-based)", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateGazeInteractorActionBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateGazeInteractor(menuCommand?.GetContextTransform(), InputType.ActionBased));
        }

        [MenuItem("GameObject/XR/Socket Interactor", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateSocketInteractor(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            var socketInteractableGO = CreateAndPlaceGameObject("Socket Interactor", menuCommand?.GetContextTransform(),
                typeof(SphereCollider),
                typeof(XRSocketInteractor));

            var sphereCollider = socketInteractableGO.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = GetScaledRadius(sphereCollider, 0.1f);
            Finalize(socketInteractableGO);
        }

        [MenuItem("GameObject/XR/Grab Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateGrabInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            var grabInteractableGO = CreateAndPlacePrimitive("Grab Interactable", menuCommand?.GetContextTransform(),
                PrimitiveType.Cube,
                typeof(XRGrabInteractable), typeof(XRGeneralGrabTransformer));

            var transform = grabInteractableGO.transform;
            var localScale = InverseTransformScale(transform, new Vector3(0.1f, 0.1f, 0.1f));
            transform.localScale = Abs(localScale);

            var boxCollider = grabInteractableGO.GetComponent<BoxCollider>();
            // BoxCollider does not support a negative effective size,
            // so ensure the size accounts for any negative scaling.
            boxCollider.size = Vector3.Scale(boxCollider.size, Sign(localScale));

            var rigidbody = grabInteractableGO.GetComponent<Rigidbody>();
            // Enable interpolation on the Rigidbody to smooth movement
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            
            Finalize(grabInteractableGO);
        }

        [MenuItem("GameObject/XR/Interactable Snap Volume", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateInteractableSnapVolume(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            var snapVolumeGO = CreateAndPlaceGameObject("Interactable Snap Volume", menuCommand?.GetContextTransform(),
                typeof(SphereCollider),
                typeof(XRInteractableSnapVolume));

            var sphereCollider = snapVolumeGO.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = GetScaledRadius(sphereCollider, 0.2f);

            // The Reset method will not find the Interactable up the hierarchy because it runs before being re-parented,
            // so the initialization of the property is repeated here.
            var snapVolume = snapVolumeGO.GetComponent<XRInteractableSnapVolume>();
            var interactable = snapVolumeGO.GetComponentInParent<IXRInteractable>();
            snapVolume.interactableObject = interactable as Object;
            if (snapVolume.interactableObject != null)
            {
                var col = interactable.transform.GetComponent<Collider>();
                if (col != null && col.enabled && !col.isTrigger)
                    snapVolume.snapToCollider = col;
            }

            Finalize(snapVolumeGO);
        }

        [MenuItem("GameObject/XR/Interaction Manager", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateInteractionManager(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            Finalize(CreateInteractionManager(menuCommand?.GetContextTransform()));
        }

        [MenuItem("GameObject/XR/Locomotion System (Action-based)", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateLocomotionSystemActionBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            Finalize(CreateLocomotionSystem(menuCommand?.GetContextTransform(), InputType.ActionBased));
        }

        [MenuItem("GameObject/XR/Device-based/Locomotion System", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateLocomotionSystemDeviceBased(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            Finalize(CreateLocomotionSystem(menuCommand?.GetContextTransform(), InputType.DeviceBased));
        }

        [MenuItem("GameObject/XR/Teleportation Area", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateTeleportationArea(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlacePrimitive("Teleportation Area", menuCommand?.GetContextTransform(),
                PrimitiveType.Plane,
                typeof(TeleportationArea)));
        }

        [MenuItem("GameObject/XR/Teleportation Anchor", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateTeleportationAnchor(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            var anchorGO = CreateAndPlacePrimitive("Teleportation Anchor", menuCommand?.GetContextTransform(),
                PrimitiveType.Plane,
                typeof(TeleportationAnchor));

            var destinationGO = ObjectFactory.CreateGameObject("Anchor");
            Place(destinationGO, anchorGO.transform);

            var teleportationAnchor = anchorGO.GetComponent<TeleportationAnchor>();
            teleportationAnchor.teleportAnchorTransform = destinationGO.transform;
            Finalize(anchorGO);
        }

        [MenuItem("GameObject/XR/UI Canvas", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateXRUICanvas(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            var parentOfNewGameObject = menuCommand?.GetContextTransform();

            var currentStage = StageUtility.GetCurrentStageHandle();
            var editingPrefabStage = currentStage != StageUtility.GetMainStageHandle();

            var canvasGO = CreateAndPlaceGameObject("Canvas", parentOfNewGameObject,
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(TrackedDeviceGraphicRaycaster));

            // Either inherit the layer of the parent object, or use the same default that GameObject/UI/Canvas uses.
            if (parentOfNewGameObject == null)
                canvasGO.layer = LayerMask.NameToLayer(k_UILayerName);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            if (!editingPrefabStage)
                canvas.worldCamera = Camera.main;
            else
                Debug.LogWarning("You have just added an XR UI Canvas to a prefab." +
                    " To function properly with an XR Ray Interactor, you must also set the Canvas component's Event Camera in your scene.",
                    canvasGO);

            // Ensure there is at least one EventSystem setup properly
            var inputModule = currentStage.FindComponentOfType<XRUIInputModule>();
            if (inputModule == null || !inputModule.gameObject.scene.IsValid())
            {
                if (!editingPrefabStage)
                    CreateXRUIEventSystemWithParent(parentOfNewGameObject, out _);
                else
                    Debug.LogWarning("You have just added an XR UI Canvas to a prefab." +
                        " To function properly with an XR Ray Interactor, you must also add an XR UI Event System to your scene.",
                        canvasGO);
            }

            Finalize(canvasGO);
        }

        [MenuItem("GameObject/XR/UI Event System", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        public static void CreateXRUIEventSystem(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            var eventSystemGO = CreateXRUIEventSystemWithParent(menuCommand?.GetContextTransform(), out var changeSelectionOnly);

            // If there was no serialization change (it already existed), only update the selection.
            // Passing it to Undo.RegisterCreatedObjectUndo in Finalize would cause the GameObject to be destroyed
            // upon Undo, which should not happen. This matches the behavior of GameObject > UI > Event System.
            if (changeSelectionOnly)
                Selection.activeGameObject = eventSystemGO;
            else
                Finalize(eventSystemGO);
        }

        [MenuItem("GameObject/XR/XR Origin (VR)", false, 10), UsedImplicitly]
        public static void CreateXROriginForVR(MenuCommand menuCommand)
        {
            Finalize(CreateXROriginWithParent(menuCommand?.GetContextTransform(), HardwareTarget.VR, InputType.ActionBased));
        }

        [MenuItem("GameObject/XR/Device-based/XR Origin (VR)", false, 10), UsedImplicitly]
        public static void CreateXROriginDeviceBased(MenuCommand menuCommand)
        {
            Finalize(CreateXROriginWithParent(menuCommand?.GetContextTransform(), HardwareTarget.VR, InputType.DeviceBased));
        }

        /// <summary>
        /// Registers <paramref name="gameObject"/> on the Undo stack as the root of a newly created GameObject hierarchy and selects it.
        /// Components on <paramref name="gameObject"/> and its children, if destroyed and recreated via Undo/Redo, will be recreated
        /// in their state from when this method was called.
        /// </summary>
        /// <param name="gameObject">The newly created root GameObject.</param>
        static void Finalize(GameObject gameObject)
        {
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {gameObject.name}");
            Selection.activeGameObject = gameObject;
        }

        /// <summary>
        /// Create the <see cref="XRInteractionManager"/> if necessary.
        /// </summary>
        /// <param name="parent">The parent <see cref="Transform"/> to use.</param>
        static GameObject CreateInteractionManager(Transform parent = null)
        {
            var currentStage = StageUtility.GetCurrentStageHandle();

            var interactionManager = currentStage.FindComponentOfType<XRInteractionManager>();
            if (interactionManager == null || !interactionManager.gameObject.scene.IsValid())
                return CreateAndPlaceGameObject("XR Interaction Manager", parent, typeof(XRInteractionManager));

            return interactionManager.gameObject;
        }

        static GameObject CreateLocomotionSystem(Transform parent, InputType inputType, string name = "Locomotion System")
        {
            var locomotionSystemGO = CreateAndPlaceGameObject(name, parent,
                typeof(LocomotionSystem),
                typeof(TeleportationProvider),
                GetSnapTurnType(inputType));

            var locomotionSystem = locomotionSystemGO.GetComponent<LocomotionSystem>();

            var teleportationProvider = locomotionSystemGO.GetComponent<TeleportationProvider>();
            teleportationProvider.system = locomotionSystem;

            var snapTurnProvider = locomotionSystemGO.GetComponent<SnapTurnProviderBase>();
            snapTurnProvider.system = locomotionSystem;
            return locomotionSystemGO;
        }

        static GameObject CreateRayInteractor(Transform parent, InputType inputType, string name = "Ray Interactor")
        {
            var rayInteractableGO = CreateAndPlaceGameObject(name, parent,
                GetControllerType(inputType),
                typeof(XRRayInteractor),
                typeof(LineRenderer),
                typeof(XRInteractorLineVisual),
                typeof(SortingGroup));

            SetupLineRenderer(rayInteractableGO.GetComponent<LineRenderer>());

            // Add a Sorting Group with a custom sorting order to make it render in front of UGUI
            var sortingGroup = rayInteractableGO.GetComponent<SortingGroup>();
            sortingGroup.sortingOrder = 5;

            return rayInteractableGO;
        }

        static GameObject CreateGazeInteractor(Transform parent, InputType inputType, string name = "Gaze Interactor")
        {
            var gazeInteractableGO = CreateAndPlaceGameObject(name, parent,
                GetControllerType(inputType),
                typeof(XRGazeInteractor),
                typeof(SortingGroup));

            // Add a Sorting Group with a custom sorting order to make it render in front of UGUI
            var sortingGroup = gazeInteractableGO.GetComponent<SortingGroup>();
            sortingGroup.sortingOrder = 5;

            return gazeInteractableGO;
        }

        static void SetupLineRenderer(LineRenderer lineRenderer)
        {
            var materials = new Material[1];
            materials[0] = AssetDatabase.GetBuiltinExtraResource<Material>(k_LineMaterial);
            lineRenderer.materials = materials;
            lineRenderer.loop = false;
            lineRenderer.widthMultiplier = 0.005f;
            lineRenderer.numCornerVertices = 4;
            lineRenderer.numCapVertices = 4;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.useWorldSpace = true;
        }

        static GameObject CreateDirectInteractor(Transform parent, InputType inputType, string name = "Direct Interactor")
        {
            var directInteractorGO = CreateAndPlaceGameObject(name, parent,
                GetControllerType(inputType),
                typeof(SphereCollider),
                typeof(XRDirectInteractor));

            var sphereCollider = directInteractorGO.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = GetScaledRadius(sphereCollider, 0.1f);

            return directInteractorGO;
        }

        static GameObject CreateXRUIEventSystemWithParent(Transform parent, out bool changeSelectionOnly)
        {
            var currentStage = StageUtility.GetCurrentStageHandle();

            var inputModule = currentStage.FindComponentOfType<XRUIInputModule>();
            if (inputModule != null && inputModule.gameObject.scene.IsValid())
            {
                changeSelectionOnly = true;
                return inputModule.gameObject;
            }

            // Ensure there is at least one EventSystem setup properly
            var eventSystem = currentStage.FindComponentOfType<EventSystem>();
            GameObject eventSystemGO;
            if (eventSystem == null || !eventSystem.gameObject.scene.IsValid())
            {
                eventSystemGO = CreateAndPlaceGameObject("EventSystem", parent,
                    typeof(EventSystem),
                    typeof(XRUIInputModule));
            }
            else
            {
                eventSystemGO = eventSystem.gameObject;

                // Remove the Standalone Input Module if already implemented, since it will block the XRUIInputModule
                var standaloneInputModule = eventSystemGO.GetComponent<StandaloneInputModule>();
                if (standaloneInputModule != null)
                    Undo.DestroyObjectImmediate(standaloneInputModule);

                Undo.AddComponent<XRUIInputModule>(eventSystemGO);
            }

            changeSelectionOnly = false;
            return eventSystemGO;
        }

        static GameObject CreateXROriginWithParent(Transform parent, HardwareTarget target, InputType inputType)
        {
            CreateInteractionManager();

            var originGo = CreateAndPlaceGameObject("XR Origin (XR Rig)", parent, typeof(XROrigin));
            var offsetGo = CreateAndPlaceGameObject("Camera Offset", originGo.transform);
            var offsetTransform = offsetGo.transform;

            var xrCamera = XRMainCameraFactory.CreateXRMainCamera(target, inputType);
            Place(xrCamera.gameObject, offsetTransform);

            var origin = originGo.GetComponent<XROrigin>();
            origin.CameraFloorOffsetObject = offsetGo;
            origin.Camera = xrCamera;

            // Set the Camera Offset y position based on the default height.
            // This will make the Scene view of the Camera when not in Play mode more closely match
            // what the position will be when entering Play mode. In Device mode, it will be this value.
            // In Floor mode, it will get reset to 0, but will at least be higher than the XROrigin position.
            var desiredPosition = offsetTransform.localPosition;
            desiredPosition.y = origin.CameraYOffset;
            offsetGo.transform.localPosition = desiredPosition;

            AddXRControllersToOrigin(origin, inputType);

            if (inputType == InputType.ActionBased)
            {
                var inputActionManager = originGo.AddComponent<InputActionManager>();

                const string assetName = "XRI Default Input Actions";
                const string searchFilter = "\"" + assetName + "\" t:InputActionAsset";
                foreach (var guid in AssetDatabase.FindAssets(searchFilter))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
                    // The search filter string will return all assets that contains the name,
                    // so ensure an exact match to the expected asset we want to set.
                    if (asset.name.Equals(assetName, StringComparison.OrdinalIgnoreCase))
                    {
                        inputActionManager.actionAssets = new List<InputActionAsset> { asset };
                        break;
                    }
                }
            }

            return originGo;
        }

        static void AddXRControllersToOrigin(XROrigin origin, InputType inputType)
        {
            var cameraOffsetTransform = origin.CameraFloorOffsetObject.transform;

            var leftHandRayInteractorGo = CreateRayInteractor(cameraOffsetTransform, inputType, "Left Controller");
            var leftHandController = leftHandRayInteractorGo.GetComponent<XRController>();
            if (leftHandController != null)
                leftHandController.controllerNode = XRNode.LeftHand;

            var rightHandRayInteractorGo = CreateRayInteractor(cameraOffsetTransform, inputType, "Right Controller");
            var rightHandController = rightHandRayInteractorGo.GetComponent<XRController>();
            if (rightHandController != null)
                rightHandController.controllerNode = XRNode.RightHand;

            Place(leftHandRayInteractorGo, cameraOffsetTransform);
            Place(rightHandRayInteractorGo, cameraOffsetTransform);
        }

        static Type GetControllerType(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.ActionBased:
                    return typeof(ActionBasedController);
                case InputType.DeviceBased:
                    return typeof(XRController);
                default:
                    throw new InvalidEnumArgumentException(nameof(inputType), (int)inputType, typeof(InputType));
            }
        }

        static Type GetSnapTurnType(InputType inputType)
        {
            switch (inputType)
            {
                case InputType.ActionBased:
                    return typeof(ActionBasedSnapTurnProvider);
                case InputType.DeviceBased:
                    return typeof(DeviceBasedSnapTurnProvider);
                default:
                    throw new InvalidEnumArgumentException(nameof(inputType), (int)inputType, typeof(InputType));
            }
        }

        /// <summary>
        /// Gets the <see cref="Transform"/> associated with the <see cref="MenuCommand.context"/>.
        /// </summary>
        /// <param name="menuCommand">The object passed to custom menu item functions to operate on.</param>
        /// <returns>Returns the <see cref="Transform"/> of the object that is the target of a menu command,
        /// or <see langword="null"/> if there is no context.</returns>
        static Transform GetContextTransform(this MenuCommand menuCommand)
        {
            var context = menuCommand.context as GameObject;
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return context != null ? context.transform : null;
#pragma warning restore IDE0031
        }

        static GameObject CreateAndPlaceGameObject(string name, Transform parent, params Type[] types)
        {
            var go = ObjectFactory.CreateGameObject(name, types);
            Place(go, parent);
            return go;
        }

        static GameObject CreateAndPlacePrimitive(string name, Transform parent, PrimitiveType primitiveType, params Type[] types)
        {
            var go = ObjectFactory.CreatePrimitive(primitiveType);
            go.name = name;
            go.SetActive(false);
            foreach (var type in types)
                ObjectFactory.AddComponent(go, type);
            go.SetActive(true);

            Place(go, parent);
            return go;
        }

        static void Place(GameObject go, Transform parent)
        {
            var transform = go.transform;

            if (parent != null)
            {
                Undo.SetTransformParent(transform, parent, "Reparenting");
                ResetTransform(transform);
                go.layer = parent.gameObject.layer;
            }
            else
            {
                // Puts it at the scene pivot, and otherwise world origin if there is no Scene view.
                var view = SceneView.lastActiveSceneView;
                if (view != null)
                    view.MoveToView(transform);
                else
                    transform.position = Vector3.zero;

                StageUtility.PlaceGameObjectInCurrentStage(go);
            }

            // Only at this point do we know the actual parent of the object and can modify its name accordingly.
            GameObjectUtility.EnsureUniqueNameForSibling(go);
        }

        static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (transform.parent is RectTransform)
            {
                var rectTransform = transform as RectTransform;
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// Returns the absolute value of each component of the vector.
        /// </summary>
        /// <param name="value">The vector.</param>
        /// <returns>Returns the absolute value of each component of the vector.</returns>
        /// <seealso cref="Mathf.Abs(float)"/>
        static Vector3 Abs(Vector3 value) => new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));

        /// <summary>
        /// Returns the sign of each component of the vector.
        /// </summary>
        /// <param name="value">The vector.</param>
        /// <returns>Returns the sign of each component of the vector; 1 when the component is positive or zero, -1 when the component is negative.</returns>
        /// <seealso cref="Mathf.Sign"/>
        static Vector3 Sign(Vector3 value) => new Vector3(Mathf.Sign(value.x), Mathf.Sign(value.y), Mathf.Sign(value.z));

        /// <summary>
        /// Transforms a vector from world space to local space.
        /// Differs from <see cref="Transform.InverseTransformVector(Vector3)"/> in that
        /// this operation is unaffected by rotation.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> the operation is relative to.</param>
        /// <param name="scale">The scale to transform.</param>
        /// <returns>Returns the scale in local space.</returns>
        static Vector3 InverseTransformScale(Transform transform, Vector3 scale)
        {
            var lossyScale = transform.lossyScale;
            return new Vector3(
                !Mathf.Approximately(lossyScale.x, 0f) ? scale.x / lossyScale.x : scale.x,
                !Mathf.Approximately(lossyScale.y, 0f) ? scale.y / lossyScale.y : scale.y,
                !Mathf.Approximately(lossyScale.z, 0f) ? scale.z / lossyScale.z : scale.z);
        }

        static float GetRadiusScaleFactor(SphereCollider collider)
        {
            // Copied from SphereColliderEditor
            var result = 0f;
            var lossyScale = collider.transform.lossyScale;

            for (var axis = 0; axis < 3; ++axis)
                result = Mathf.Max(result, Mathf.Abs(lossyScale[axis]));

            return result;
        }

        static float GetScaledRadius(SphereCollider collider, float radius)
        {
            var scaleFactor = GetRadiusScaleFactor(collider);
            return !Mathf.Approximately(scaleFactor, 0f) ? radius / scaleFactor : 0f;
        }

#if AR_FOUNDATION_PRESENT
#if AR_FOUNDATION_5_0_OR_NEWER
        [MenuItem("GameObject/XR/XR Origin (AR)", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateXROriginForAR(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            Finalize(CreateXROriginWithParent(menuCommand?.GetContextTransform(), HardwareTarget.AR, InputType.ActionBased));
        }
#endif

        [MenuItem("GameObject/XR/AR Gesture Interactor", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARGestureInteractor(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Gesture Interactor",
                menuCommand?.GetContextTransform(),
                typeof(ARGestureInteractor)));
        }

        [MenuItem("GameObject/XR/AR Placement Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARPlacementInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Placement Interactable",
                menuCommand?.GetContextTransform(),
                typeof(ARPlacementInteractable)));
        }

        [MenuItem("GameObject/XR/AR Selection Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARSelectionInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Selection Interactable",
                menuCommand?.GetContextTransform(),
                typeof(ARSelectionInteractable)));
        }

        [MenuItem("GameObject/XR/AR Translation Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARTranslationInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Translation Interactable",
                menuCommand?.GetContextTransform(),
                typeof(ARTranslationInteractable)));
        }

        [MenuItem("GameObject/XR/AR Scale Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARScaleInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Scale Interactable",
                menuCommand?.GetContextTransform(),
                typeof(ARScaleInteractable)));
        }

        [MenuItem("GameObject/XR/AR Rotation Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARRotationInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Rotation Interactable",
                menuCommand?.GetContextTransform(),
                typeof(ARRotationInteractable)));
        }

        [MenuItem("GameObject/XR/AR Annotation Interactable", false, 10), UsedImplicitly]
#pragma warning disable IDE0051 // Remove unused private members -- Editor Menu Item
        static void CreateARAnnotationInteractable(MenuCommand menuCommand)
#pragma warning restore IDE0051
        {
            CreateInteractionManager();

            Finalize(CreateAndPlaceGameObject("AR Annotation Interactable",
                menuCommand?.GetContextTransform(),
                typeof(ARAnnotationInteractable)));
        }

#endif // AR_FOUNDATION_PRESENT
    }
}