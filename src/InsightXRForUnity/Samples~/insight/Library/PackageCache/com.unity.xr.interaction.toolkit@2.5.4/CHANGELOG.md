# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

<!-- Headers should be listed in this order: Added, Changed, Deprecated, Removed, Fixed, Security -->

## [2.5.4] - 2024-04-05

### Fixed
- Fixed compilation errors on tvOS platform where `ENABLE_VR` is not defined when AR Foundation is installed. Also fixed when XR Hands is installed in the Hands Interaction Demo sample. (Backport from 3.0.0)
- Fixed `TrackedGraphicRaycaster` to clear poke interaction data when disabled. ([XRIT-142](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-142)) (Backport from 3.0.0)
- Fixed warning about use of deprecated `VersionsInfo.verified` by replacing with `VersionsInfo.recommended` in the Hands Interaction Demo sample in Unity 2022.2 and newer. (Backport from 3.0.0)
- Fixed the XR Interactor Line Visual from bending towards an XR Interactable Snap Volume behind UI when the valid UI hit is closest. (Backport from 3.0.2)
- Fixed tap gesture detection for selecting and spawning interactable objects in AR scenes. ([XRIT-145](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-145)) (Backport from 3.0.2)
  - Changed the Tap Start Position input action to remove the Tap interaction from the `tapStartPosition` binding in the Starter Assets sample `XRI Default Input Actions` asset. The sample will need to be reimported to remove the Tap interaction from the binding for taps to be functional. This change along with changes to the `XRScreenSpaceController` also fixes the selected object from staying selected and movable with the mobile device orientation for 0.2 seconds after a tap was released.

## [2.5.3] - 2024-02-28

### Fixed
- Fixed issue where rotating on vertical planes with the `ARTransformer` would snap to an unintended rotation. (Backport from 3.0.0-pre.1)
- Fixed failing unit tests caused by introduction of global actions in Input System 1.8.0. (Backport from 3.0.0-pre.1)
- Fixed a math error in the `XRDistanceEvaluator` where `CalculateNormalizedScore` would return an incorrect score. (Backport from 3.0.0-pre.1)
- Fixed an issue where the direct interactor wasn't correctly processing colliders after the first frame. ([XRIT-116](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-116)) (Backport from 3.0.0-pre.1)
- Fixed a bug where a `NullReferenceException` was thrown when `Affordance Theme Datum` was not set. If it is null, an error is logged and the `AudioAffordanceReceiver` is disabled, but no exception is thrown. ([XRIT-107](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-107)) (Backport from 3.0.0-pre.1)
- Fixed the `ActionBasedControllerManager` script in Starter Assets to not stop move locomotion when beginning to point at scrollable UI during locomotion. (Backport from 3.0.0-pre.1)
- Fixed the `ActionBasedControllerManager` script in Starter Assets to not scroll UI when beginning to point at scrollable UI during locomotion until stopping locomotion. (Backport from 3.0.0-pre.2)
- Fixed `XRPokeInteractor` and `TrackedGraphicRaycaster` to ensure poke interactor data is cleared when the UI element with a poke interaction is disabled. ([XRIT-115](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-115)) (Backport from 3.0.0-pre.2)
 - Fixed an issue where target filters could not influence the prioritized poke interactable. ([XRIT-114](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-114)) (Backport from 3.0.0-pre.2)
- Fixed an issue in `XRUIInputModule` where display index was not being set for mouse or touch in the `PointerEventData` object causing it to always default to the first display. ([XRIT-125](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-125)) (Backport from 3.0.0-pre.2)
- Fixed use of renamed Rigidbody properties in Unity 2023.3 to avoid script migration prompt. (Backport from 3.0.0-pre.2)
- Fixed a small performance issue at startup by making XR Ray Interactor only search for AR Raycast Manager when both AR Foundation is installed and Enable AR Raycasting is enabled. (Backport from 3.0.0-pre.2)
- Fixed an issue with two handed rotations using the XRGeneralGrabTransformer where some inconsistent motion would occur when the user would rotate more than 180 degrees in any axis. (Backport from 3.0.0-pre.2)
- Fixed compilation errors on tvOS platform where `ENABLE_VR` is not defined. (Backport from 3.0.0-pre.2)
- Fixed a bug in `XRSocketInteractor` that prevented the deselecting of the Starting Selected Interactable when Hover Socket Snapping was enabled. (Backport from 3.0.0-pre.2)
- Fixed broken markdown links within tables in samples documentation in the manual. (Backport from 3.0.0-pre.2)

## [2.5.2] - 2023-09-28

### Fixed
- Fixed an issue where the XR Ray Interactor can no longer interact with UI drag events correctly when the UI object is both a Tracked Device Graphics Raycaster and an XR Interactable. This fix introduces a property `blockUIOnInteractableSelection` to `XRRayInteractor` to control this behavior.
- Fixed an issue where the XR Ray Interactors internal UI cache can cause error log spamming when it is not initialized or becomes null at runtime.
- Fixed an issue where dropping a grab interactable, when scaling beyond min and max scale thresholds using an `ARTransformer`, didn't properly set the scale back within the threshold limits when Attach Ease In Time or Smooth Scale were enabled. As a part of this fix, `attachEaseInTime` and smoothing will only be considered when an `XRGrabInteractable` is selected.
- Fixed an issue where poking UI elements with the controller poke wand did not work in the starter assets demo scene.

## [2.5.1] - 2023-09-12

### Changed
- Split and moved [Samples](../manual/samples.html) documentation into a new area of the [Table of Contents](../manual/TableOfContents.html) to make discovery and navigation easier.
- Redesigned and improved [Hands Interaction Demo](../manual/samples-hands-interaction-demo.html) sample scene and prefabs.
  - Reworked colliders and interactions in sample scene.
  - Added new rim light material made using Shader Graph.
  - Added new blend shape pinch dropper for hand rays.
  - Added grab handle to reposition and reorient the table.
  - Added poke to drag chess piece examples.
  - Added socket interactor example to show socket snapping.
  - Added One Euro Filter algorithm to smooth hand root using XR Hands post processor.
  - Changed ray visual to now originate from the pinch point instead of a fixed offset from the wrist.
  - Moved hand menu prefab to be a child of the XR Origin to allow for locomotion.
- Changed `com.unity.inputsystem` dependency from 1.5.0 to 1.7.0.

### Fixed
- Fixed how XR Interactor Line Visual sets the reticle rotation when hovering a teleportation interactable with **Match Orientation** set to **World Space Up**, **Target Up**, or **None**, so that it is consistent with how the XR Origin and camera would be oriented upon teleport.
- Fixed `ObjectSpawner` script in Starter Assets sample applying extra y-axis rotation when **Apply Random Angle At Spawn** is enabled.
- Fixed incorrect models in AR Starter Assets prefabs for Pyramid and Wedge objects.
- Fixed incorrect theme asset reference in AR Starter Assets for Arch prefab.
- When overriding the ray origin, there used to be a significant mismatch between the ray end point and the actual target. To fix this, we now bend the ray towards the hit point found by the raycaster. This ensures the ray visually aligns, while still keeping our preferred ray origin.
- Fixed Add Component menu path to put Uniform Transform Scale Affordance Receiver in Affordance System &gt; Receiver &gt; Transformation instead of Rendering to match its namespace.

## [2.5.0] - 2023-08-17

### Added
- Added the canvas optimizer to reduce runtime load of UI-heavy scenes. See [UI Setup - Canvas optimizer](../manual/ui-setup.html#canvas-optimizer) in the manual for more details.
- Added the [`ARTransformer`](xref:UnityEngine.XR.Interaction.Toolkit.Transformers.ARTransformer) which allows users to move an `XRGrabInteractable` while constrained to AR Foundation planes. It also has functionality for scaling with touch pinch gestures when the XR Screen Space Controller is used.
- Added Track Scale and related smoothing options to XR Grab Interactable to allow developers to disable writing to the object's scale when grabbing.
- Added snap transformations to `XRSocketInteractor` on hover.
- Added scaling transformation to `XRSocketInteractor`.
- Added several scripts to the Starter Assets sample for object spawning in AR: `ObjectSpawner`, `ARInteractorSpawnTrigger`, `ARContactSpawnTrigger`, and `DestroySelf`.
- Added an AR demo scene to the AR Starter Assets that displays a set-up for mobile AR which includes a sliding menu of placeable and interactable objects.
- Added `GetCurrentGameObject` method to `UIInputModule` to get the UI GameObject currently being hovered by a tracked device or pointer device such as a mouse or touchscreen.
- Added Cone Cast hit detection type to the [XR Ray Interactor](../manual/xr-ray-interactor.html) to allow users to select small objects at a distance easily.
- Added ray endpoint stabilization to the [XR Transform Stabilizer](../manual/xr-transform-stabilizer.html).
- Added [XR Hand Skeleton Poke Displacer](../manual/xr-hand-skeleton-poke-displacer.html) component to allow for displacing the hand skeleton when poking an interactable in order to prevent the hand from phasing through objects while poking them.
- Added one handed scaling support to XR General Grab Transformer to allow grab interactables to be scaled, controlled by enabling Allow One Handed Scaling (which is enabled by default). For motion controller scaling with the XR Ray Interactor, Scale Mode (which is None by default) should be set to Input. For pinch scaling with the XR Ray Interactor, Scale Mode should be set to Distance.
- Added new Scale Toggle and Scale Delta input actions to the XR Controller (Action-based) which the XR Ray Interactor reads. The `XRRayInteractor` implements the new [`IXRScaleValueProvider`](xref:UnityEngine.XR.Interaction.Toolkit.IXRScaleValueProvider) by reading the scale delta values from the `ActionBasedController` or `XRScreenSpaceController` so that the `XRGeneralGrbTransformer` can scale using the controller and `ARTransformer` can be used to scale objects with touch pinch gestures.

### Changed
- The Tunneling Vignette sample was moved into the Starter Assets sample.
- Changed the Starter Assets sample to be reorganized so all `DemoScene` assets are located in a separate `DemoSceneAssets` folder that can be easily removed.
- Changed the automatic creation of the XR Device Simulator prefab to be excluded from standalone builds by default. A new **Instantiate In Editor Only** setting to control this behavior was added to the XR Interaction Toolkit project settings. ([XRIT-82](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-82))
- Changed the Ray Interactor prefab in the Starter Assets sample to use cone casting instead of ray casting for the hit detection type.
- Changed the XR Controller (Action-based) components in the Starter Assets sample to have empty action references instead of empty input actions for consistency and to avoid potential errors during the `ApplyProcessors` method of the current latest version of Input System.
- Converted math in XR General Grab Transformer to use the Burst compiler and the Mathematics package for performance improvements.
- Project Validation will automatically open if there are validation errors or missing dependencies to correct when importing [Hands Interaction Demo](../manual/samples-hands-interaction-demo.html) sample package.
- Converted math in XR Grab Interactable related to smoothing operations to use the Burst compiler and the Mathematics package for performance improvements.
- Changed `XRGrabInteractable` default property values:
  - Changed default value of `smoothPositionAmount` and `smoothRotationAmount` from `5` to `8`.
  - Changed default value of `tightenPosition` and `tightenRotation` from `0.5` to `0.1`.
- Changed `com.unity.xr.core-utils` dependency from 2.2.1 to 2.2.3.

### Fixed
- Fixed an issue where a teleport aim reticle would not rotate to point forward when the interactor ray origin rotated if the interactor was just hovering and not yet selecting.
- Fixed compiler warnings for use of `FindObjectOfType` and `FindObjectsOfType` by conditionally using the newer methods `FindAnyObjectByType` and `FindObjectsByType` for Unity 2023.1 and newer.
- Fixed the AR Configuration Inspector foldout in XR Ray Interactor to keep expanded state when clicking between GameObjects.

## [2.4.3] - 2023-07-21

### Fixed
- Fixed Starter Asset validation check that would trigger a race condition when accessing the XRI settings files, causing false console errors.

## [2.4.2] - 2023-07-20

### Changed
- Changed package version for internal release.

## [2.4.1] - 2023-07-12

### Added
- Added affordance theme validation logic so that affordance theme assets used in an affordance receiver are automatically upgraded with any missing theme states (such as the focused state).

### Changed
- Converted math in XR General Grab Transformer to use the Burst compiler and the Mathematics package for performance improvements.
- Changed XR General Grab Transformer by changing the default value of Threshold Move Ratio For Scale from `0.1` to `0.05`.
- Changed the climb example ladder to allow forward/z-axis movement at the top for more natural feeling controls.
- Changed the Starter Assets UI sample to include a dropdown example.

### Fixed
- Fixed compiler error in `ComponentLocatorUtility.cs` related to `FindFirstObjectByType` in some patch versions of the Unity Editor. It will now fallback to `FindObjectOfType` in Editor versions where the other method is not available.
- Fixed an issue in the `XRRayInteractor` where the teleport cursor does not stay aligned with the controller direction when moving around.
- Fixed an issue with the `UIInputModule` that would cause unintended drag events to fire for head/camera movement. ([XRIT-64](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-64))
- Fixed an issue with the Starter Assets sample that caused dropdowns to render over line renderer visuals.
- Fixed the Tunneling Vignette sample to render on top of dropdowns and the line renderer visuals.
- Fixed an issue in the `LazyFollow` where the object would not snap to the target position when the `snapOnEnabled` is true.
- Fixed various internal transform usages for XR Origin to use its `Origin` property for the transform since the Origin GameObject and the GameObject the component is on may differ.
- Fixed potential for undesired prefab override noise with serialized input action properties when the **GameObject** &gt; **XR** creation menu is used by ensuring the action name is set even if the Inspector window has never drawn the component.

## [2.4.0] - 2023-06-15

### Changed
- Project Settings for the XR Interaction Toolkit have been moved under the category **XR Plug-in Management** to consolidate XR configuration.
- Project validation rules for the XR Interaction Toolkit and samples have been updated to use global XR project validation and will now appear in the **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management** &gt; **Project Validation** window.
- Changed `Reset` method of behaviors to no longer assign a reference to an XR Interaction Manager or XR Origin.
- Changed XR Interactable Affordance State Provider component's default value to Ignore Focus Events.
- Changed affordance receivers to log a warning when the affordance theme data is missing potential affordance states. A new affordance state `focused` at array index 6 was added to the affordance theme assets in the samples. Users will need to reimport the Starter Assets and Hands Interaction Demo samples or add the `focused` state to their affordance theme assets.

### Fixed
- Fixed an issue with the XR Distance evaluator not calculating the proper distance from trigger colliders. ([XRIT-71](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-71))
- Fixed an issue where interactors and interactables could use a different default XR Interaction Manager reference, such as after instantiating a prefab that contains an XR Interaction Manager into a scene with interactables that previously created a default manager instance. ([XRIT-65](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-65))
- Fixed teleportation not working when the prefab containing the Teleportation Provider component is instantiated after the teleportation interactables are loaded by attempting to find that component upon each teleport request if needed instead of only upon `Awake`.
- Fixed climb not working when the prefab containing the Climb Provider component is instantiated after the climb interactables are loaded by attempting to find that component upon each climb attempt if needed instead of only upon `Awake`.
- Fixed `XR General Grab Transformer` scaling doing exponential scaling because it was using squared distance for ration calculation.
- Fixed AR Gesture Interactor Inspector window not showing Raycast Mask and Raycast Trigger Interaction properties when the version of AR Foundation is older than version 5.0.
- Fixed compiler warnings related to `ARSessionOrigin` when AR Foundation 5.0 or newer is installed.

## [2.4.0-pre.2] - 2023-05-31

### Changed
- Changed the `XR Interactable Affordance State Provider` order to put focus state after all other interaction states for consistency.
- Changed the required version of `com.unity.xr.hands` for the Hands Interaction Demo sample from 1.2.0 to 1.2.1.

### Fixed
- Fixed a null reference issue when accessing device and screen-space controllers caused by optimization changes made in the XR Ray Interactor.
- Fixed unwanted behavior for the Hand Interactions Demo > Hand Menu sample prefabs, specifically in the `ToggleGameObject` sample script.
- Fixed an issue with XR Interaction Manager where focused interactables would not be cleared in the correct order when selecting a new interactable.

## [2.4.0-pre.1] - 2023-05-25

### Added
- Added hands support to XR Device Simulator. You must have the [XR Hands package](https://docs.unity3d.com/Manual/com.unity.xr.hands.html) installed in your project to use this new functionality.
- Added Is Tracked action to XR Controller (Action-based), and updated presets and prefabs in Starter Assets sample to make use of the new Is Tracked input actions in the `XRI Default Input Actions`.
- Added properties in the [`ARGestureInteractor`](xref:UnityEngine.XR.Interaction.Toolkit.AR.ARGestureInteractor) class to control raycast behavior for gestures.
- Added the `IXRPokeFilter` interface to allow other classes to act as customized poke filters for the `XRPokeInteractor` instead of only supporting the `XRPokeFilter` component.
- Added XR Interactor Affordance State Provider component which can drive affordance receivers using interactor interactions events.
- Added Color Gradient Line Renderer Affordance Receiver to pair with an XR Interactor Affordance State Provider on a Ray interactor to improve visual coloring. Has a property to automatically disable coloring of XR Interactor Line Visual.
- Added [Hand Menu](../manual/hand-menu.html) component, as well as a sample prefab of a working hand menu in the Hands Interaction Demo sample.
  - `HandMenu` component has a split configuration for hands and controllers, with a new `FollowPresetDatum`. 
  - Added gaze activation settings and a reveal/ hide hand menu animation.
- Added [XR Input Modality Manager](../manual/xr-input-modality-manager.html) component which manages swapping between hand and controller hierarchies in the XR Origin. Updated prefabs in the package samples to make use of this component.
- Added ability for XR Interactor Line Visual to curve accurately and track interactable attach points during selection.
- Added Auto Adjust Line Length property to XR Interactor Line Visual to retract the line end after a delay when the ray interactor doesn't hit any valid targets.
- Added the [XR Gaze Assistance](../manual/xr-gaze-assistance.html) component to enable split interaction. Eye for aiming and controllers for selection.
- Added the [`IXRRayProvider`](xref:UnityEngine.XR.Interaction.Toolkit.IXRRayProvider) interface to allow other ray implementations to take advantage of split interaction.
- Added `Focus State` to interactables. An interactable that is selected is also focused; it remains focused until another interactable is focused instead. Useful for highlighting an object to later perform operations on.
- Added Visit Each Frame property to XR Controller Recorder to control whether each frame of the input recording must be played back regardless of the time period passed.
- Added [XR Transform Stabilizer](../manual/xr-transform-stabilizer.html) component that applies optimized stabilization techniques to remove pose jitter and makes aiming and selecting with rays easier for users. 
- Added Climb Provider, which provides locomotion counter to interactor movement while the user is selecting a Climb Interactable.
  - Added menu item **Assets > Create > XR > Locomotion > Climb Settings**, which creates a Climb Settings Datum asset.
  - Added a Climb Provider instance to `XR Origin Preconfigured` in the Starter Assets sample.
  - Added `Climb Sample` prefab to the Starter Assets sample, and added an instance of this prefab to `DemoScene`. This prefab includes preconfigured Climb Interactables.
- Added support for XRRayInteractors to scroll UI panels using the thumbstick.
  - IUInteractors now support UIHoverEnter and UIHoverExit events.
  - UIInputModule gains the trackedScrollDeltaMultiplier property to control scrolling speeds via thumbstick.
  - TrackedDeviceModel gains properties for current UI Selectable and if the selected UI is scrollable.
  - ActionBasedController gains a property for UI scrolling input, set to the thumbstick in the starter assets.
- Added configuration of interaction overrides to XR Interaction Group, allowing certain Group members to take control of interaction when attempting to select, regardless of priority.
- Added Direct Interactor as an interaction override for Poke Interactor in each XR Interaction Group in `XR Origin (XR Rig)` in Starter Assets sample.
- Added new Shader Graphs and Materials in `Hand Interaction Demo` for a transparent hand that supports highlighting fingers
- Added the [`TouchscreenGestureInputController`](xref:UnityEngine.XR.Interaction.Toolkit.AR.Inputs.TouchscreenGestureInputController) which allows users to surface touchscreen gesture data via the Input System.
- Added the [`XRScreenSpaceController`](xref:UnityEngine.XR.Interaction.Toolkit.XRScreenSpaceController) which enables usage of screen space input, from touchscreen or mouse, with interactors.
- Added the `enableARRaycasting` property to [`XRRayInteractor`](xref:UnityEngine.XR.Interaction.Toolkit.XRRayInteractor) which enables raycasting against the AR environment if AR Foundation is installed.

### Changed
- Changed [`XRControllerState`](xref:UnityEngine.XR.Interaction.Toolkit.XRControllerState) by adding an `isTracked` field. Deprecated old constructors, users should update their code to call the ones with the added parameter.
- Changed XRI project validation to only log errors to the console, not warnings.
- Changed XR Interactor Line Visual so it bends to the selected interactable by default. Set **Line Bend Ratio** to **1** to revert to the old behavior where the line would remain straight.
- Changed XR Interactor Line Visual default value of Line Width from 0.02 to 0.005.
- Improved performance of the line visual and ray interactor by optimizing most of the line computation math for the Burst compiler. The [Burst package](https://docs.unity3d.com/Manual/com.unity.burst.html) must be installed in your project to take advantage of the optimizations.
- Changed `XRInteractorLineVisual` by adding the `OnDestroy` and `LateUpdate` methods. Users who had already implemented either method in derived classes will need to call the base method.
- Changed `XRInteractorReticleVisual` by adding the `OnDisable` method so it disables the reticle when the component is disabled. Users who had already implemented that method in derived classes will need to call the base method.
- Changed `TeleportationAnchor.GetAttachTransform` method to return the `teleportAnchorTransform` value.
- Renamed Starter Assets sample prefabs:
  - Renamed `Complete XR Origin Set Up` prefab to `XR Interaction Setup`.
  - Renamed `XR Origin` prefab to `XR Origin (XR Rig)`.
  - Renamed `Complete Teleport Area Set Up` prefab to `Teleportation Environment`.
- Renamed Hands Interaction Demo sample prefabs:
  - Renamed `Complete XR Origin Hands Set Up` prefab to `XR Interaction Hands Setup`.
  - Renamed `XR Origin Hands` prefab to `XR Origin Hands (XR Rig)`.
- Changed `XR Origin (XR Rig)` to reorganize locomotion components to new GameObjects.
- Changed `XR Origin (XR Rig)` to disable grab move locomotion by default. Activate the **Grab Move** GameObject to re-enable that functionality.
- Changed `XR Origin Hands (XR Rig)` to be a prefab variant of the `XR Origin (XR Rig)` prefab.
- Changed Starter Assets sample prefabs by adding XR Gaze Fallback to the XR Origin GameObject.
- Changed `XRRayInteractor`, `XRInteractorLineVisual`, and `XRInteractorReticleVisual` to support mediation through split interaction.
- Changed XR Ray Interactor to no longer interact with UGUI Canvases while selecting an interactable.
- Changed XR Ray Interactor so the Hover To Select property will now activate with UI elements.
- Changed `com.unity.inputsystem` dependency from 1.4.4 to 1.5.0.
- Changed `com.unity.xr.core-utils` dependency from 2.2.0 to 2.2.1.
- Changed lowest supported Unity Editor version of XR Interaction Toolkit to 2021.3 now that 2020.3 has reached End of Life.
- Changed `XRPokeLogic` to resolve an issue where starting a poke from behind the object can trigger select.
- Updated the Hands Interaction Demo with new interaction reactive visuals:
  - Changed the required version of `com.unity.xr.hands` for the Hands Interaction Demo sample from 1.1.0 to 1.2.0.
  - Changed the `XR Origin Hands (XR Rig)` prefab to use prefabs for each hand visual with affordances to highlight the fingers during interaction.
  - Changed the hands model to use new components in `com.unity.xr.hands` to subscribe and expose hand tracking events: `XRHandsSkeletonDriver`, `XRHandTrackingEvents`, and `XRHandMeshController`.
- Added Sphere Collider optimized accuracy improvement for Direct Interactor that improves inter-frame reliability and latency.

### Removed
- Removed `HandsAndControllersManager` script from the Hands Interaction Demo sample and moved it into the package as [`XRInputModalityManager`](xref:UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager).

### Fixed
- Fixed XR Grab Interactables interfering with player movement by using `Physics.IgnoreCollision` to prevent collision between the Character Controller and the grabbed object's colliders.
- Fixed the Input Devices tab in the [XR Interaction Debugger window](../manual/debugger-window.html) so it doesn't rebuild the tree every Editor frame. This allows the input devices to be collapsed. Added additional columns.
- Fixed the teleport ray interactor getting stuck on after a teleport completes when the GameObject with the Action Based Controller Manager component was deactivated.
- Fixed XR Interactor Line Visual not working with Teleportation Anchor when an XR Interactable Snap Volume is used by no longer skipping the snapping behavior when the ray interactor has a selection. Use the Disable Snap Collider When Selected property of XR Interactable Snap Volume to control that behavior.
- Fixed bugs in Lazy Follow where threshold mechanics weren't being respected, and reworked class to leverage [`SmartFollowVector3TweenableVariable`](xref:UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables.SmartFollowVector3TweenableVariable) and [`SmartFollowQuaternionTweenableVariable`](xref:UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables.SmartFollowQuaternionTweenableVariable).
- Fixed bug with PokeFollowAffordance sample script that did not work when using two hands on the same canvas.
- Fixed so poke objects will push to the larger depth when both hands are poking at the same time.
- Fixed an issue in the `ActionBasedControllerManager` that would cause a null-reference exception if the Direct or Ray interactors were not assigned. ([XRIT-72](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-72))
- Fixed an issue with the state-change of the `XRInteractableAffordanceStateProvider` that would trigger Select and Hover effects when currently Activated and `ignoreActivateEvents` was set to true or Hover effects when actively Selected and `ignoreSelectEvents` was set to true.
- Fixed an Array Out of Bounds error when using Affordance Themes when accessing the last element in the list of states.
- Fixed an issue with `XRPokeLogic` where starting a poke from behind the object can trigger select.

## [2.3.2] - 2023-04-28

### Changed
- Changed XRI project validation to only log errors to the console, not warnings.
- Updated `XRPokeFollowAffordance` smoothing property default from 8 to 16 to make it feel more responsive.

### Fixed
- Added additional checks against AR Foundation 5.0 and newer so deprecation messages and the use of the newer XR Origin (AR) is hidden when using AR Foundation 4.2 and older.
- Fixed GC allocations produced each frame by `XRPokeInteractor.UpdateUIModel`.
- Fixed frame-timing for Locomotion Input Tests when running in batch mode.
- `XRInteractableAffordanceStateProvider` Fixes:
    - Activated state was lower priority than selected, which because select is not exited, this was causing issues with it not appearing.
    - There were some racing coroutines between select and activate, and now trigger a new animation blocks previous animations
    - Leaving the select state cancels select animations that might not have completed
    - Leaving activated state cancels activate animations that might not have completed
- Fixed repeat audio issue in the `AudioAffordanceReceiver` by adding extra conditions which treat select as a modifier to hover, and activated as a modifier to select. Doing this prevents triggering repeat audio clips that shouldn't fire, like releasing the activate trigger, or releasing the select trigger while still hovering.
- Fixed `XRPokeLogic` issue where poking from behind objects would sometimes trigger select incorrectly.
- Fixed `XRPokeLogic` issue where depth percent was incorrectly calculated using an exponential value which would result in poke buttons feeling disconnected from the poke interactor position.

## [2.3.1] - 2023-03-27

### Added
- Added System Gesture Detector component to the Hands Interaction Demo sample to add system gesture and menu palm pinch gesture events. Added sound upon menu press as an example. Added Aim Flags input actions to the `XRI Default Input Actions` in the Starter Assets sample to support this.
- Added [Interaction filters](../manual/interaction-filters.html) documentation for `IXRHoverFilter`, `IXRSelectFilter`, `IXRInteractionStrengthFilter`, and the corresponding filter delegates with examples.

### Changed
- Changed the Poke Gesture Detector component in the Hands Interaction Demo sample to no longer end the poke gesture when hand tracking is lost. This fixes the Ray Interactor line visual reappearing when hand tracking is lost while doing the poke gesture.
- Changed `XRInteractorReticleVisual` to ensure consistent attempts to align the reticle prefab's `z` axis with the `transform.up` of the XROrigin when `AlignPrefabWithSurfaceNormal` is `true` and aligning with a non-horizontal surface.
- Changed `XRInteractorReticleVisual` to align the reticle prefab's `z` axis with the forward direction of the reticle's interactor when `AlignPrefabWithSurfaceNormal` is `true` and aligning with a horizontal surface.
- Updated installation documentation with convenience links for installing the XRI package on older versions of Unity 2021 where the package was not included in the main Editor manifest.
- Changed so UGUI poke interactions are now considered to be blocking interactions for interaction groups. This allows rays to be properly hidden when hovering or selecting a UGUI canvas with poke.
- Changed to use velocity estimation of poke interactor to add an extra validation mechanism in the XR Poke Filter hover validation check to allow poke selection to occur in cases where it was previously rejected, while still preventing poking from behind and other non-desireable cases.
- Changed to cache poke selection validation check so that it's easier to hold a poke when the selection conditions are met. This makes scrolling UGUI canvases easier and makes poke interactions feel more consistent.
- Changed AR Scale Interactable so changing the Min Scale and/or Max Scale during runtime will keep the current object scale if still within range instead of resizing the object to keep the same scale ratio.

### Fixed
- Fixed the Hands Interaction Demo sample to wait to activate the controller GameObjects until they are reconnected instead of each time hand tracking is lost. Also fixed the controllers appearing at the origin if they have never been tracked.
- Fixed the Hands Interaction Demo sample so it disables the hand interactors while doing a system gesture (such as a user looking at their open palm at eye level).
- Fixed warning about a self-intersecting polygon in the `Frame.fbx` model in the Hands Interaction Demo sample.
- Fixed warning in Hands Interaction Demo sample about obsolete API usage coming from the hands subsystem. 
- Fixed `XRSimulatedController` and `XRSimulatedHMD` to have identifying characteristics information in the `capabilities` field of their corresponding `InputDeviceDescription`. ([XRIT-50](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-50))
- Fixed an issue in the `XRController` class where the `inputDevice` property was not reinitialized when the `controllerNode` property was changed. ([XRIT-52](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-52))
- UGUI ray interactions are now correctly blocked when interaction groups block ray interactions and the ray is hidden.
- Fixed an issue with `TrackedDeviceGraphicRaycaster`, when using the `XRPokeInteractor` on UGUI canvases with different sort orders, where interaction was blocked on all but the highest Order in Layer valued canvas. ([XRIT-48](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-48))
- Fixed an issue with `TrackedDeviceGraphicsRaycaster`, where opening a dropdown would block all other UGUI canvases to become non-interactable with an `XRPokeInteractor` until the dropdown was closed.
- Fixed the `Starter Assets` and `Hands Interaction Demo` prefabs which contained components and shaders with a mix of both Built-in Render Pipeline and Universal Render Pipeline. They are all now using Built-in Render Pipeline for consistency.
- Fixed `XRInteractorReticleVisual` incorrect rotation around `y` axis when aligning prefab to surface normal. ([XRIT-18](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-18))
- Fixed `XRInteractorReticleVisual` incorrect rotation when `AlignPrefabWithSurfaceNormal` is `false`.
- Fixed `XRInteractorReticleVisual` inconsistent rotation when `AlignPrefabWithSurfaceNormal` and `DrawOnNoHit` are `true` but there is no active hit.
- Fixed `UIInputModule` issue where tracked devices cannot drag on UI elements when `Cursor.lockState` is set to `Locked`.

## [2.3.0] - 2023-02-17
### Added
- Added a Raycast Snap Volume Interaction property to control whether the XR Ray Interactor will collide with or ignore trigger snap colliders from an XR Interactable Snap Volume (used by gaze assistance). This allows a user to set Raycast Trigger Interaction to Ignore but still collide with trigger colliders that are associated with a snap volume.
- Added options to XR Poke Follow Affordance in the Starter Assets sample to apply the follow animation if the poke target is a child and to clamp the follow target to a maximum distance from the poke target.
- Added an XR Poke Follow Affordance to the `TextButton` prefab in the Starter Assets sample so that the button graphics can move in response to poke.
- Added Tracking State and Is Tracked input actions to the `XRI Default Input Actions` in the Starter Assets sample.
- Added [Meta Gaze Adapter](../manual/samples-meta-gaze-adapter.html) sample to allow developers to request permission and initialize eye tracking for the Meta Quest Pro.
- Added [Hands Interaction Demo](../manual/samples-hands-interaction-demo.html) sample to demonstrate interactions with hand tracking.
- Added poke interaction examples to the `DemoScene` in the Starter Assets sample.
- Added ability to customize the layer mask and trigger interaction when the XR Poke Interactor performs the physics sphere overlap call.
- Added documentation regarding behavior when Select Action Trigger is set to State on XR Direct Interactor and XR Ray Interactor.

### Changed
- Changed the Stop Manipulation action (default binding `Escape`) in the XR Device Simulator to always stop manipulation every time it is pressed instead of cycling between None and FPS mode. Trigger the Cycle Devices action (default binding `Tab`) to switch back to FPS mode instead.
- Changed `Ray Interactor` prefab in the Starter Assets sample to enable Treat Selection As Valid State on the XR Interactor Line Visual.
- Changed the  **GameObject** &gt; **XR** &gt; **Grab Interactable** menu item to set the Rigidbody Interpolate property on the created GameObject to Interpolate.
- Changed the Rigidbody Interpolate property from None to Interpolate in each of the Starter Assets sample grab interactable prefabs.
- Changed the default value of the Color Property Name property on the Color Material Property Affordance Receiver component to an empty string instead of `"_BaseColor"`. An empty string will now use either `"_BaseColor"` or `"_Color"` depending on the current render pipeline to add support for the Built-In Render Pipeline.
- Changed `GetValidTargets` on each interactor type to return an empty list when the interactor is disabled.
- Changed `ActionBasedControllerManager` in Starter Assets sample to make use of XR Interaction Group and removed some unused serialized fields.
- Changed `XR Origin Preconfigured` in Starter Assets sample so it instantiates the controller model prefab at runtime instead of being in the prefab hierarchy to make it easier for users to override the model used.
- Changed `Teleport Interactor` in Starter Assets sample so it instantiates the reticle prefabs at runtime instead of being in the prefab hierarchy to make it easier for users to override the reticle used.
- Changed `com.unity.xr.core-utils` dependency to 2.2.0.

### Fixed
- Fixed Teleportation Anchor incorrectly triggering a teleport when the Ray Interactor stops pointing at the anchor when it no longer has any ray cast hits.
- Fixed Starter Assets sample prefabs and `DemoScene` to have the Gaze Interactor prefab.
- Fixed `XRPokeInteractor` so it uses the `targetFilter` for filtering the valid targets returned by `GetValidTargets`.
- Fixed the Fix button for project validation issue "Interaction Layer 31 is not set to 'Teleport'" not persisting to the settings asset when closing the Unity Editor.
- Fixed missing references in device simulator UI button images by assigning to null.
- Fixed the `PokeStateData` that is generated from UI poke interaction so that its `axisAlignedPokeInteractionPoint` is relative to the world position of the target transform.
- Fixed broken click animations in `XRInteractableAffordanceStateProvider`.
- Fixed XR Device Simulator so it will use the new main camera if the previous one is disabled or destroyed.
- Fixed issue where AR gestures did not take into account UI in `ARBaseGestureInteractable` behaviors (such as AR Placement Interactable) by adding an `excludeUITouches` property, which is enabled by default.
- Fixed potential invalid stayed colliders list in `XRDirectInteractor` and `XRSocketInteractor` when the interactor is disabled while in contact with a collider.
- Fixed hover and select events being incorrectly fired when an `XRDirectInteractor` or `XRSocketInteractor` GameObject or component is disabled while in contact with an interactable, moved away from the interactable, and then enabled.
- Fixed XR Poke Interactor to query the local [PhysicsScene](https://docs.unity3d.com/ScriptReference/PhysicsScene.html) instead of using static physics calls.
- Fixed poke in UI scrolling in a scroll view outside of the viewport by ensuring the Tracked Device Graphic Raycaster respects the alpha hit test threshold.
- Fixed the `Teleport Anchor` prefab in Starter Assets to place the teleport destination at the top of the platform, fixing a bump that would occur when moving with locomotion after teleporting.

## [2.3.0-pre.1] - 2022-12-07

### Added
- Added new Affordance System. This introduces the XRI Affordance state provider, which connects to an XR Interactable to determine new affordance states, which then power Affordance Receivers to animate tweens using Affordance Theme scriptable Objects. This can be used for audio, ui, material and other kinds of animation tweens, reactive to interaction state changes, all powered by the Job System.
- Added an option to **Edit** &gt; **Project Settings** &gt; **XR Interaction Toolkit** to automatically instantiate the prefab in the [XR Device Simulator](../manual/samples-xr-device-simulator.html) sample.
- Added XR Interaction Group component, which allows only one member Interactor or Group within the Group to be interacting at a time.
- Added the option **Disable Visuals When Blocked In Group** to XR Base Interactor, which controls whether to disable the Interactor's visuals when the Interactor is part of an Interaction Group and is unable to interact due to active interaction by another Interactor in the Group. This option is enabled by default.
- Added an XR Interaction Group to each hand in the `XR Origin Preconfigured` prefab in the `Starter Assets` sample. Each Group prioritizes Direct interaction over Ray interaction.
- Added a runtime UI for the XR Device Simulator. Users can now also move the player position around the physical play space using `WASD` and `Q`/`E` to simulate the user walking around. Reimport the XR Device Simulator sample to access this new functionality and UI.
- Added ability to control the tracking state of the simulated devices in the XR Device Simulator Inspector window.
- Added properties to XR Device Simulator to control the `[0.0, 1.0]` amount of the simulated grip and trigger inputs when those controls are activated.
- Added lazy follow functionality for UI which can be enabled by adding the [LazyFollow](xref:UnityEngine.XR.Interaction.Toolkit.UI.LazyFollow) component to a GameObject.
- Added [`IXRInteractionStrengthInteractor`](xref:UnityEngine.XR.Interaction.Toolkit.IXRInteractionStrengthInteractor) and [`IXRInteractionStrengthInteractable`](xref:UnityEngine.XR.Interaction.Toolkit.IXRInteractionStrengthInteractable) interfaces that are implemented by [`XRBaseInteractor`](xref:UnityEngine.XR.Interaction.Toolkit.XRBaseInteractor) and [`XRBaseInteractable`](xref:UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable), respectively. These add additional properties and methods related to interaction strength, which conveys a variable (that is, analog) selection interaction strength between an interactor and interactable. This is typically based on a motion controller's grip or trigger amount, or based on a poke depth for those interactable objects that support being poked.
- Added the [`IXRInteractionStrengthFilter`](xref:UnityEngine.XR.Interaction.Toolkit.Filtering.IXRInteractionStrengthFilter) interface. Instances of this interface can be added to Interactables to extend their interaction strength computation without needing to create a derived class.
- Added `IsHovered` and `IsSelected` methods to [`XRBaseInteractable`](xref:UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable) that works similarly to `IsHovering` and `IsSelecting` on [`XRBaseInteractor`](xref:UnityEngine.XR.Interaction.Toolkit.XRBaseInteractor) for querying whether a specific interactor is hovering or selecting that interactable.
- Added [XRPokeInteractor](xref:UnityEngine.XR.Interaction.Toolkit.XRPokeInteractor) and [XRPokeFilter](xref:UnityEngine.XR.Interaction.Toolkit.Filtering.XRPokeFilter) classes that provide basic poking functionality for both hands and controllers.
- Added XR Poke Follow Affordance component in the `Starter Assets` sample to control the smooth visual pressing of a Transform component (such as a button, for example) driven by the current select value of a poke interaction.
- Added XR General Grab Transformer which supports moving and rotating unconstrained with one or two interactors, and scaling when using two interactors.
- Added [`XRGazeInteractor`](xref:UnityEngine.XR.Interaction.Toolkit.XRGazeInteractor), driven by either eye-gaze or head-gaze pose information. This allows a developer to use eye or head gaze to hover or select by dwelling on interactables.
- Added [`XRInteractableSnapVolume`](xref:UnityEngine.XR.Interaction.Toolkit.XRInteractableSnapVolume) to allow snapping a ray interactor to a nearby target interactable. This can be combined with the `XRGazeInteractor` to achieve gaze-assisted hover and selection.
- Added Gaze Configuration properties to [`XRBaseInteractable`](xref:UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable) related to gaze interactions, automatic selection and deselection from hover, and gaze-assisted hover and selection.
- Added a `drawOnNoHit` property to `XRInteractorReticleVisual` that forces the reticle to draw when no ray cast hits are detected.
- Added a `snapEndpointIfAvailable` property to `XRInteractorLineVisual` to allow bending the visual ray towards a specified target point, such as guided by an `XRInteractableSnapVolume` for more user-friendly object selection.
- Added `Eye Gaze Position` and `Eye Gaze Rotation` actions to the `XRI Default Input Actions` asset along with corresponding `XRI Default Gaze Controller` preset to the `Starter Assets` sample.
- Added an Integer Fallback Composite binding for Input System input actions, which is useful for a tracking state action. This composite works similarly to the Vector 3 Fallback and Quaternion Fallback Composite bindings.
- Added `GetCustomReticle` method to `XRBaseInteractable` to allow lookup of the custom reticle associated with a particular Interactor.
- Added `Poke Interactor` to each hand in the `Complete XR Origin Set Up` prefab in `Starter Assets`

### Changed
- Changed the default grab transformers from XR Single Grab Free Transformer and XR Dual Grab Free Transformer to XR General Grab Transformer. This new grab transformer does not respond to the pose of the Attach Transform of the XR Grab Interactable changing while grabbed. If you need to modify the pose after being grabbed, you will need to add a different grab transformer from the **Component** &gt; **XR** &gt; **Transformers** menu.
- Changed **GameObject** &gt; **XR** &gt; **Grab Interactable** to add the XR General Grab Transformer component by default.
- Changed XR Grab Interactable to re-initialize the dynamic attach pose when changing from multiple grabs back to a single grab by default. Disable **Reinitialize Every Single Grab** (`reinitializeDynamicAttachEverySingleGrab`) for previous behavior.
- Changed `XRDeviceSimulator` to destroy itself if another instance already exists at runtime.
- Changed XR Device Simulator to initialize the simulated controllers to position them in front of the HMD instead of at (0, 0, 0).
- Changed XR Device Simulator to start manipulating the HMD and controllers as if the whole player was turning their torso, similar to a typical FPS style configuration, to simplify its use. Press `Tab` to cycle between manipulating all devices in this mode, the left controller individually, and the right controller individually.
- Changed **GameObject** &gt; **XR** &gt; **XR Origin (VR)** to set the Tracking State Input property on the Tracked Pose Driver of the Main Camera on versions of Input System that support it.
- Changed `XRSimulatedController` and `XRSimulatedHMD` to report support for updating during the before render phase.
- Changed `DefaultExecutionOrder` of the `XRInteractionManager` from `-100` to `-105`.
- Changed `InteractorRegisteredEventArgs` by adding a `containingGroupObject` property of type `IXRInteractionGroup` which is set when the Interactor is contained in an Interaction Group.
- Changed `XRBaseInteractable` initialization logic of the `IXRInteractable.colliders` list to exclude trigger colliders when no colliders are set in the Inspector window.
- Changed `XRInteractableUtility.TryGetClosestCollider` and `XRInteractableUtility.TryGetClosestPointOnCollider` to ignore trigger colliders.
- Changed the default value of Select Action Trigger (`XRBaseControllerInteractor.selectActionTrigger`) on interactors from State to State Change.
- Changed `com.unity.inputsystem` dependency to 1.4.4.
- Changed `com.unity.xr.core-utils` dependency to 2.2.0-pre.2.
- Changed `com.unity.xr.legacyinputhelpers` dependency to 2.1.10.

### Fixed
- Fixed Tracked Device Graphic Raycaster to use the correct ray cast method when Check for 2D Occlusion is enabled, and changed it to use the local [PhysicsScene2D](https://docs.unity3d.com/ScriptReference/PhysicsScene2D.html).
- Fixed issue with `RegistrationList` and `SmallRegistrationList` where unregistering and then registering an item that already exists in the registered snapshot would result in the item being counted twice due to being added to the buffered add list.
- Fixed issue with `RegistrationList` and `SmallRegistrationList` where unregistering an item that was not yet flushed to the registered snapshot would result in the list returning an incorrect `flushedCount` due to being incorrectly added to the buffered remove list instead of just removing from the buffered add list.
- Fixed issue with `RegistrationList` not reporting the registration status of items added via `MoveItemImmediately`.
- Fixed Grab Transformers (that derive from `XRBaseGrabTransformer`) to skip automatic registration specified by `registrationMode` when it has already been added to the `XRGrabInteractable`. It previously only checked the Starting Single/Multiple Grab Transformers lists.
- Fixed expansion state of Select Filters in the Inspector window reusing the Hover Filters state in some cases.
- Fixed `XRRayInteractor` null reference exception that causes editor spam when sample points are deleted upon hot-reload.
- Fixed incorrectly false return values for `AddCustomReticle` and `RemoveCustomReticle` on the `XRInteractorLineVisual` class.

## [2.2.0] - 2022-09-30

### Added
- Added ability for the target position, rotation, and scale of an XR Grab Interactable to be calculated by another component. Developers can derive from [XRBaseGrabTransformer](xref:UnityEngine.XR.Interaction.Toolkit.Transformers.XRBaseGrabTransformer) and add those components to the same GameObject as the XR Grab Interactable. The existing logic was moved to [XRSingleGrabFreeTransformer](xref:UnityEngine.XR.Interaction.Toolkit.Transformers.XRSingleGrabFreeTransformer). For more information, see [Grab transformers](../manual/xr-grab-interactable.html#grab-transformers).
- Added properties `allowHoverAudioWhileSelecting` and `allowHoverHapticsWhileSelecting` to `XRBaseControllerInteractor`, which control whether to allow the Interactor from playing audio and haptics, respectively, in response to a Hover event if the Hovered Interactable is currently Selected by the Interactor.
- Added property `stopLineAtSelection` to `XRInteractorLineVisual`, which controls whether the line will stop at the attach point of the closest interactable selected by the interactor, if there is one.
- Added property `treatSelectionAsValidState` to `XRInteractorLineVisual`, which forces the use of valid state visuals while the interactor is selecting an interactable, whether or not the interactor has any valid targets.
- Added support for teleportation directionality so that users can specify the direction they will face when teleportation finishes.
  - Added a `Teleport Direction` input action for each hand in the `XRI Default Input Actions` asset in the `Starter Assets` sample.
  - Added the property `anchorRotationMode` as an option for how `XRRayInteractor` controls anchor rotation. `RotateOverTime` is the existing and default behavior, which is useful for rotating a held object. The other option is `MatchDirection`, which is useful for controlling teleportation direction by matching rotation to the direction of the 2-dimensional input.
  - Added input options for directional anchor rotation to `ActionBasedController` and `XRController`.
  - Added the property `matchDirectionalInput` as an option for a `BaseTeleportationInteractable` to apply directional input (based on anchor rotation) when its `matchOrientation` is set to `WorldSpaceUp` or `TargetUp`.
  - Added an interface `IXRReticleDirectionProvider` for teleportation interactables to override the direction of the reticle.
- Added `Fixed` update type to [XRBaseController](xref:UnityEngine.XR.Interaction.Toolkit.XRBaseController) which is in sync with `MonoBehavior.FixedUpdate`.
- Added the `IXRTargetPriorityInteractor` interface that allow Interactors to monitor the Interactables priority for selection in the current frame. There are different monitoring options (None, Highest Priority Only, and All) with different performance tradeoffs.
- Added the `XRInteractionManager.IsHighestPriorityTarget` method to check if an Interactable is the highest priority candidate for selection of an Interactor (`IXRTargetPriorityInteractor`) in the current frame, which is useful for custom affordance feedback.
- Added the `IXRHoverFilter` interface. Instances of this interface can be added to Interactors, Interactables, or to the Interaction Manager to extend their hover validations without needing to create a derived class.
- Added the `IXRSelectFilter` interface. Instances of this interface can be added to Interactors, Interactables, or to the Interaction Manager to extend their select validations without needing to create a derived class.
- Added color gradient and reticle options to XR Interactor Line Visual for when the Interactor has a valid target but it selection is blocked. These can be configured by setting the properties **Blocked Color Gradient** and **Blocked Reticle**, respectively.
- Added the `public` method `CanHover` to `XRInteractionManager`, which checks whether a given Interactor is able to hover a given Interactable.
- Added the `public` method `CanSelect` to `XRInteractionManager`, which checks whether a given Interactor is able to select a given Interactable.
- Added the `public` method `IsHoverPossible` to `XRInteractionManager`, which checks whether a given Interactor would be able to hover a given Interactable if the Interactor were in a state where it could hover.
- Added the `public` method `IsSelectPossible` to `XRInteractionManager`, which checks whether a given Interactor would be able to select a given Interactable if the Interactor were in a state where it could select.
- Added **Enable Fly** option to Continuous Move Provider, which allows for unconstrained movement in any direction.
- Added properties `filterSelectionByHitNormal` and `upNormalToleranceDegrees` to `BaseTeleportationInteractable`, which are used to configure whether a user can teleport to a location if the hit normal is not aligned with the interactable's up vector.
- Added Grab Move Provider and Two Handed Grab Move Provider, which provide locomotion based on tracked controller position while a button is held. Grab Move Provider provides translation of the user, and Two Handed Grab Move Provider provides translation, yaw rotation, and uniform scaling of the user.
  - Added a `Grab Move` input action for each hand in the `XRI Default Input Actions` asset in the `Starter Assets` sample.
  - Added `XRI Default Left Grab Move` and `XRI Default Right Grab Move` presets to the `Starter Assets` sample.
  - Added `ConstrainedMoveProvider` base class for `GrabMoveProvider` and `TwoHandedGrabMoveProvider`, to hold logic for constrained movement with a `CharacterController`.

### Changed
- Changed XR Grab Interactable to support being selected by multiple Interactors. To enable that ability in the Inspector window, set **Select Mode** to **Multiple**. If your derived class does not properly handle multiple selections and you want to disable the Multiple option, you will need to add `[CanSelectMultiple(false)]` to your component script.
- Ray casts now query the local [PhysicsScene](https://docs.unity3d.com/ScriptReference/PhysicsScene.html) instead of using static Physics routines which uses the default physics scene. Gesture classes that perform ray casts use the camera's scene, and components that perform ray casts use its scene during `Awake`.
- Changed `XRInteractionManager` by adding the `Awake` method. Users who had already implemented `Awake` in derived classes will need to call the base method.
- Creating a new `XR Origin (VR)` now automatically adds an `Input Action Manager` component and sets the `Action Assets` to `XRI Default Input Action.inputactions` asset if available from the `Starter Assets` sample package.
- Move speed in Continuous Move Providers and line width in XR Interactor Line Visual are now scaled by the scale of the XR Origin.
- Changed the Ray Interactor GameObject created through the **GameObject** &gt; **XR** create menu to have a Sorting Group to make it render in front of UI and changed the `TunnelingVignette` prefab in the Tunneling Vignette sample to have a Sorting Group to make it render in front of the Line Renderer of a Ray Interactor.
- Changed minimum supported version of the Unity Editor from 2019.4 to 2020.3 (LTS).

### Fixed
- Fixed issue where the last point in the curve rendered by XR Interactor Line Visual would not always be continuous with the rest of the curve (for example when sphere cast is used).
- Fixed issue where using the [MockHMD XR Plugin package](https://docs.unity3d.com/Packages/com.unity.xr.mock-hmd@latest/) with the `XRDeviceSimulator` would cause the device simulator's rotation to be overwritten. This issue can also be fixed by upgrading the [Input System package](https://docs.unity3d.com/Manual/com.unity.inputsystem.html) to 1.4.1+.
- Fixed issue where `XRInteractorReticleVisual` script did not properly detect ray casts with UI objects. ([XRIT-18](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-18))
- Fixed an issue with throwing physics on Quest devices where bad frame-timing would cause unexpectedly high velocities to be applied.
- Fixed "Retrieving array element that was out of bounds" error when viewing the Tunneling Vignette Controller in the Inspector window when the Locomotion Vignette Providers list is empty.
- Fixed XR Interactor Line Visual so it deactivates the current reticle GameObject when the behavior is enabled or disabled.

## [2.1.1] - 2022-07-29

### Added
- Added `enableBuiltInActionsAsFallback` property on `XRUIInputModule` to provide a fallback default behavior out of the box when using the new Input System as the Active Input Handling.
- Added missing documentation for `UI Setup` that specifies how to perform object occlusion for UI objects when using the Tracked Device Graphics Raycaster component.
- Added a warning message to the Inspector of XR Interactor Line Visual and XR Interactor Reticle Visual when the Reticle has a Collider which may disrupt the ray cast. This added an `XRInteractorReticleVisualEditor` class.

### Changed
- Changed the dynamic attach behavior of XR Grab Interactable so it only ignores the Match Position property when being grabbed by an XR Ray Interactor with Force Grab enabled instead of ignoring both Match Position and Match Rotation. This lets you bring the object to your hand while keeping the rotation if configured. To make it easier to override the dynamic attach properties for a selecting interactor, the `protected` methods `ShouldMatchAttachPosition`, `ShouldMatchAttachRotation`, and `ShouldSnapToColliderVolume` were added to [`XRGrabInteractable`](xref:UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable).
- Updated Editor tooltip and documentation to clarify how the **Stop Line At First Raycast Hit** property works on the XR Interactor Line Visual component. ([XRIT-4](https://issuetracker.unity3d.com/product/unity/issues/guid/XRIT-4))

### Fixed
- Fixed rendering of the Tunneling Vignette causing issues like the Line Renderer cutting short by setting `ZWrite Off` in the shader. If the sample has already been imported into your project, you will need to import again to get the update.
- Fixed so the `TunnelingVignette` material asset does not get modified by the `TunnelingVignetteController`.
- Fixed `GetValidTargets` performance when there are several Interactors in the scene hovering one or no Interactables.
- Fixed missing input binding for the `Move` action in the `XRI RightHand Locomotion` Action Map in the XRI Default Input Actions asset inside the `Starter Assets` sample.
- Fixed the Rotate Anchor and Translate Anchor input bindings in the XRI Default Input Actions asset to be simpler by not using composite bindings.
- Fixed `CharacterControllerDriver` so it will try to find the `ContinuousMoveProviderBase` on other active GameObjects if it isn't on the same GameObject.
- Fixed regression introduced with version [2.1.0-pre.1](#210-pre1---2022-05-02) so Anchor Control on XR Ray Interactor does not base logic on an XR UI Input Module property.
- Fixed the **GameObject** &gt; **XR** create menu items which find existing components sometimes using invalid or deleted GameObjects.
- Fixed the **GameObject** &gt; **XR** &gt; **UI Event System** create menu item destroying the existing GameObject upon Undo when it already existed in the scene.
- Fixed `CreateUtilsTests` to be `internal` instead of `public`.

## [2.0.3] - 2022-07-26

### Fixed
- Fixed the simulated HMD and controllers so they no longer reset to origin after switching focus away and back to the Unity Editor. ([1409417](https://issuetracker.unity3d.com/product/unity/issues/guid/1409417))
- Fixed warnings about setting velocities of a kinematic body when updating an XR Grab Interactable with Movement Type set to Kinematic.
- Fixed `UIInputModule` so it will use the new Main Camera if the previous one is disabled when `UIInputModule.uiCamera` is not set.
- Fixed warning in the Inspector window for interactors missing the XR Controller component when the GameObject is deactivated by changing the find method to include inactive GameObjects.
- Fixed XR Interactor Line Visual so it will instantiate the Reticle GameObject when it is a Prefab asset so it does not modify the asset on disk.
- Fixed `UIInputModule` to support EventSystem `IPointerMoveHandler` for Unity 2021.1 and newer. Also added the corresponding `pointerMove` event to `UIInputModule` to allow the pointer move events to be globally handled.

## [2.1.0-pre.1] - 2022-05-02

### Added
- Added properties to XR Grab Interactable to use dynamic attach transforms so the grab pose will be based on the pose of the Interactor when the selection is made. You can enable the Use Dynamic Attach property to keep the object in the same position and rotation when grabbed. ([1373337](https://issuetracker.unity3d.com/product/unity/issues/guid/1373337))
- Added filtering for interactions to help determine the intent of the user. The new abstractions XR Target Filter and XR Target Evaluator let users configure and extend the logic of how an Interactor ranks an Interactable from a list of valid ones, specifically in the `GetValidTargets` method. Several different evaluators are included in this update, and custom ones can be created. This makes it easier to customize the Interactor without needing to create a derived behavior.
- Added the `XRBaseInteractable.distanceCalculationMode` property. This give users the ability to configure how an Interactable calculates its distance to a location, such as to an Interactor for sorting its valid targets, at varying tradeoffs between accuracy and performance.
- Added the `XRBaseInteractable.getDistanceOverride` property that lets users assign a method to be called when the Interactable is performing a distance calculation to a location, which is used when the Interactor is ordering its valid targets. This property makes it easier to customize the Interactable without needing to create a derived behavior.
- Added `IsOverUIGameObject` function to `XRRayInteractor` that does a simple check to see if the ray cast result is hitting a UI GameObject.
- Added `InputActionReference` properties to the `XRUIInputModule` for left/right/middle clicks, navigation move, submit, cancel, scroll and pointer movement actions. This allows for greater flexibility and customization of what devices can drive UI input when using the `XRUIInputModule`.
- The `XRI Default Input Actions` asset in the `Starter Assets` sample package now includes an `XRI UI` Action Map for UI-specific Input Actions. Also included is a Preset asset to quickly map the actions onto the `XRUIInputModule` component.
- Added a Tunneling Vignette sample. It contains assets to let users set up and configure the tunneling vignette as a comfort mode intended to mitigate motion sickness in VR.
- Added a Tunneling Vignette Controller component used for driving the vignette material included with the Tunneling Vignette sample. Locomotion Provider components can be drag-and-dropped into a list of Locomotion Providers that will trigger the tunneling vignette effect. A custom inspector allows previewing each effect for the corresponding Locomotion Provider.
  * Added `ITunnelingVignetteProvider` interface to allow custom behaviors to control the vignette effect.
  * Added a `LocomotionPhase` enum in `LocomotionProvider` that can be used to describe different phases of a locomotion for use with the tunneling vignette. Added code in `ContinuousMoveProviderBase`, `ContinuousTurnProviderBase`, `SnapTurnProviderBase`, and `TeleportationProvider` to compute their `LocomotionPhase`.
  * Added a Delay Time property to the Teleportation Provider and Snap Turn Provider components to support customization of timing for use with fading in the tunneling vignette.

### Changed
- Updated code paths with macro protections around `InputSystem` or `Input Manager` based code to prevent attempted usage when either one is not active.
- Scroll speed when using the ScrollWheel Input System Action is now being divided by 20 pixels per line instead of 120 pixels per line to match the `InputSystemUIInputModule` scrolling speed.
- Changed `XRSocketInteractor` hover mesh pose calculation to only ignore the current pose of the attach transform for `XRGrabInteractable` when Use Dynamic Attach is disabled instead of for all types of `IXRSelectInteractable`.
- Changed `XRControllerRecorder.recording` from `internal` to `public`.

### Fixed
- Fixed `UIInputModule` so pointer clicks set the correct button (left/right/middle) for the `EventSystem` in the `PointerEventData`.
- Fixed compilation errors on platforms such as Game Core where `ENABLE_VR` is not currently defined.

## [2.0.2] - 2022-04-29

### Fixed
- Fixed wrong offset when selecting an `XRGrabInteractable` with Track Rotation disabled when the Attach Transform had a different rotation than the Interactable's rotation. This configuration was not covered in the related fix made previously in version [2.0.0-pre.6](#200-pre6---2021-12-15). ([1361271](https://issuetracker.unity3d.com/product/unity/issues/guid/1361271))
- Fixed XR Socket Interactor hover mesh position and rotation for an XR Grab Interactable with Track Position and/or Track Rotation disabled.
- Fixed the simulated controllers not working in projects where the Scripting Backend was set to IL2CPP.
- Fixed the simulated HMD `deviceRotation` not being set. It now matches the `centerEyeRotation`.
- Fixed the **GameObject &gt; XR &gt; AR Annotation Interactable** menu item when AR Foundation is installed to add the correct component.
- Fixed **UIInputModule** so it uses and resets [`PointerEventData.useDragThreshold`](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.PointerEventData.html#UnityEngine_EventSystems_PointerEventData_useDragThreshold) to allow users to ignore the drag threshold by implementing [`IInitializePotentialDragHandler`](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.EventSystems.IInitializePotentialDragHandler.html). It was previously being ignored and causing sliders and scrollbars to incorrectly use a drag threshold.

## [2.0.1] - 2022-03-04

### Changed
- Changed the `XRI Default Input Actions` asset in the Starter Assets sample by removing the `primaryButton` bindings from Teleport Select and Teleport Mode Activate. If you want to restore the old behavior of both bindings, add an Up\Down\Left\Right Composite, reassign the Up composite part binding, and add the Sector interaction for that direction. The actions were also reorganized into additional Action Maps.

### Fixed
- Fixed regression introduced with version [2.0.0](#200---2022-02-16) so the hover mesh draws in the correct location when the Interactable's Attach Transform is not a child Transform or deep child Transform.
- Fixed the `XRI Default Input Actions` asset in the Starter Assets sample showing the warning "(Incompatible Value Type)" on the bindings for Teleport Select and Teleport Mode Activate by changing the action type from Button to Value with an expected control type of `Vector2`. The sample needs to be imported again if you already imported it into your project for you to see these changes.
- Fixed missing `UNITY_INCLUDE_TESTS` constraint in test assembly definition.

## [2.0.0] - 2022-02-16

### Added
- Added a warning message to the Inspector of `XRGrabInteractable` with non-uniformly scaled parent. A child `XRGrabInteractable` with non-uniformly scaled parent that is rotated relative to that parent may appear skewed when you grab it and then release it. See [Limitations with Non-Uniform Scaling](https://docs.unity3d.com/Manual/class-Transform.html). ([1228990](https://issuetracker.unity3d.com/product/unity/issues/guid/1228990))
- Added support for gamepad and joystick input when using the XR UI Input Module for more complete UGUI integration.

### Changed
- Changed sockets so selections are only maintained when exclusive. `XRSocketInteractor.CanSelect` changed so that sockets only maintain their selection when it is the sole interactor selecting the interactable. Previously, this was causing interactables that support multiple selection to not get released from the socket when grabbed by another interactor, which is not typically desired.
- Changed sockets so the hover mesh is positioned at the original attach transform pose for selected interactables. This fixes the case where the hover mesh would be at the wrong location when the attach transform is dynamically modified when an XR Grab Interactable is grabbed.
- Changed `XRDirectInteractor` and `XRSocketInteractor` by adding an `OnTriggerStay` method to fix an issue where those interactors did not detect when a Collider had exited in some cases where `OnTriggerExit` is not invoked, such as the Collider of the interactable being disabled. Users who had already implemented `OnTriggerStay` in derived classes will need to call the base method.
- Changed `GestureTransformationUtility.Raycast` default parameter value of `trackableTypes` from `TrackableType.All` to `TrackableType.AllTypes` to fix use of deprecated enum in AR Foundation 4.2. The new value includes `TrackableType.Depth`.
- Renamed the Default Input Actions sample to Starter Assets.
- Updated the manual to move most sections to separate pages.
- Moved some components from the **Component &gt; Scripts** menu into **Component &gt; XR**, **Component &gt; Event**, and **Component &gt; Input**.
- Changed `com.unity.xr.core-utils` dependency to 2.0.0.

### Fixed
- Fixed `XRDirectInteractor` and `XRSocketInteractor` still hovering an `XRGrabInteractable` after it was deactivated or destroyed.
- Fixed properties in event args for select and hover being incorrect when the same event is invoked again during the event due to the instance being reused for both. An object pool is now used by the `XRInteractionManager` to avoid the second event from overwriting the instance for the first event.
- GC.Alloc calls have been reduced: ray interactors with UI interaction disabled no longer allocate each frame, XR UI Input Module now avoids an allocating call, and AR gesture recognizers no longer re-allocate gestures when an old one is available.
- Fixed Editor classes improperly using `enumValueIndex` instead of `intValue` in some `SerializedProperty` cases. In practice, this bug did not affect users since the values matched in those cases.
- Fixed issue where `EventManager.current.IsPointerOverGameObject` would always return false when using `XRUIInputModule` for UI interaction. ([1387567](https://issuetracker.unity3d.com/product/unity/issues/guid/1387567))
- Fixed XR Tint Interactable Visual from clearing the tint in some cases when it was set to tint on both hover and selection. Also fixed the case when the interactable supports multiple selections so it only clears the tint when all selections end. It will now also set tint during `Awake` if needed.
- Fixed `ARTests` failing with Enhanced touches due to version upgrade of Input System.
- Fixed use of deprecated methods and enum values in `GestureTransformationUtility` when using AR Foundation 4.1 and 4.2.

## [2.0.0-pre.7] - 2022-01-31

### Fixed
- Fixed `ScriptableSettings` so it no longer logs to the console when creating the settings asset.

## [2.0.0-pre.6] - 2021-12-15

### Fixed
- Fixed wrong offset when selecting an `XRGrabInteractable` with Track Rotation disabled. ([1361271](https://issuetracker.unity3d.com/product/unity/issues/guid/1361271))
- Fixed `XRInteractorLineVisual` causing the error "Saving Prefab to immutable folder is not allowed". Also fixed the undo stack by no longer modifying the Line Renderer during `Reset`. ([1378651](https://issuetracker.unity3d.com/product/unity/issues/guid/1378651))
- Fixed UI interactions not clicking when simultaneously using multiple Ray Interactors. ([1336124](https://issuetracker.unity3d.com/product/unity/issues/guid/1336124))
- Fixed `Raycast Padding` of `Graphic` UI objects not being considered by `TrackedDeviceGraphicRaycaster`. ([1333300](https://issuetracker.unity3d.com/product/unity/issues/guid/1333300))
- Fixed `OnEndDrag` not being called on behaviors that implement `IEndDragHandler` when the mouse starts a drag, leaves the bounds of the object, and returns to the object without releasing the mouse button when using the `XRUIInputModule` upon finally releasing the mouse button.
- Fixed runtime crashing upon tapping the screen when using AR touch gestures in Unity 2021.2 in projects where the Scripting Backend was set to IL2CPP.
- Fixed `MissingReferenceException` caused by `XRBaseInteractable` when one of its Colliders was destroyed while it was hovering over a Direct Interactor or Socket Interactor.
- Fixed obsolete message for `XRControllerState.poseDataFlags` to reference the correct replacement field name.

## [2.0.0-pre.5] - 2021-11-17

### Fixed
- Fixed name of the profiler marker for `PreprocessInteractors`.

## [2.0.0-pre.4] - 2021-11-17

### Added
- Added ability to change the `MovementType` of an `XRGrabInteractable` while it is selected. The methods `SetupRigidbodyDrop` and `SetupRigidbodyGrab` will be invoked in this case, you can check if the `XRGrabInteractable` it's not selected or use the methods `Grab` and `Drop` to perform operations that should only occur once during the select state.

### Changed
- Changed so the Interaction Layer Mask check is done in the `XRInteractionManager` instead of within `XRBaseInteractor.CanSelect`/`XRBaseInteractable.IsSelectableBy` and `XRBaseInteractor.CanHover`/`XRBaseInteractable.IsHoverableBy`.
- Changed `com.unity.inputsystem` dependency from 1.0.2 to 1.2.0.
- Changed `com.unity.xr.core-utils` dependency from 2.0.0-pre.3 to 2.0.0-pre.5.
- Changed `com.unity.xr.legacyinputhelpers` dependency from 2.1.7 to 2.1.8.
- Changed `XRHelpURLConstants` from `public` to `internal`.

### Fixed
- Updated the property names of [XROrigin](https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.XROrigin.html) to adhere to PascalCase.

## [2.0.0-pre.3] - 2021-11-09

### Added
- Added XR Interaction Toolkit Settings to the **Edit &gt; Project Settings** window to allow for editing of the Interaction Layers. These settings are stored within a new `Assets/XRI` folder by default.
- Added a Select Mode property to Interactables that controls the number of Interactors that can select it at the same time. This allows Interactables that support it to be configured to allow multiple hands to interact with it at the same time. The Multiple option can be disabled in the Inspector window by adding `[CanSelectMultiple(false)]` to your component script.
- Added ability to double click a row in the XR Interaction Debugger window to select the Interactor or Interactable.
- Added the `ActionBasedController.trackingStateAction` property that allows users to bind the `InputTrackingState`. This new action is used when updating the controller's position and rotation. When not set, it falls back to the old behavior of using the tracked device's tracking state that is driving the position or rotation action.
- Added the interaction `float` value to the controller state. This will allow users to read the `float` value from `InteractionState`, not just the `bool` value, to drive visuals.
- Added methods to `XRBaseInteractor` and `XRBaseInteractable` to return the pose of the Attach Transform captured during the moment of selection (`GetAttachPoseOnSelect` and `GetLocalAttachPoseOnSelect`).
- Added a property to `XRBaseInteractor` and `XRBaseInteractable` to return the first interactor or interactable during the current select stack (`firstInteractableSelected` and `firstInteractorSelecting`).
- Added Allow Hovered Activate option to Ray Interactor and Direct Interactor to allow sending activate and deactivate events to interactables that the interactor is hovered over but not selected when there is no current selection. Override `GetActivateTargets(List<IXRActivateInteractable>)` to control which interactables can be activated.
- Added `teleporting` event to `BaseTeleportationInteractable` (`TeleportationAnchor`, `TeleportationArea`). Fires according to timing defined by that type's `teleportTrigger`.

### Changed
- Changed `ProcessInteractor` so that it is called after interaction events instead of before. Added a new `PreprocessInteractor` method to interactors which is called before interaction events. Scripts which used `ProcessInteractor` to compute valid targets should move that logic into `PreprocessInteractor` instead.
- Changed the signature of all methods with `XRBaseInteractor` or `XRBaseInteractable` parameters to instead take one of the new interfaces for Interactors (`IXRInteractor`, `IXRActivateInteractor`, `IXRHoverInteractor`, `IXRSelectInteractor`) or Interactables (`IXRInteractable`, `IXRActivateInteractable`, `IXRHoverInteractable`, `IXRSelectInteractable`). This change allows users to completely override and develop their own implementation of Interactors and Interactables instead of being required to derive from `XRBaseInteractor` or `XRBaseInteractable`.
  |Old Signature|New Signature|
  |---|---|
  |XRBaseInteractor<br/>`void GetValidTargets(List<XRBaseInteractable> targets)`|IXRInteractor<br/>`void GetValidTargets(List<IXRInteractable> targets)`|
  |XRBaseInteractor<br/>`bool CanHover(XRBaseInteractable interactable)`|IXRHoverInteractor<br/>`bool CanHover(IXRHoverInteractable interactable)`|
  |XRBaseInteractor<br/>`bool CanSelect(XRBaseInteractable interactable)`|IXRSelectInteractor<br/>`bool CanSelect(IXRSelectInteractable interactable)`|
  |XRBaseInteractable<br/>`bool IsHoverableBy(XRBaseInteractor interactor)`|IXRHoverInteractable<br/>`bool IsHoverableBy(IXRHoverInteractor interactor)`|
  |XRBaseInteractable<br/>`bool IsSelectableBy(XRBaseInteractor interactor)`|IXRSelectInteractable<br/>`bool IsSelectableBy(IXRSelectInteractor interactor)`|
  |BaseInteractionEventArgs<br/>`XRBaseInteractor interactor { get; set; }`<br/>`XRBaseInteractable interactable { get; set; }`|ActivateEventArgs and DeactivateEventArgs<br/>`IXRActivateInteractor interactorObject { get; set; }`<br/>`IXRActivateInteractable interactableObject { get; set; }`<br/><br/>HoverEnterEventArgs and HoverExitEventArgs<br/>`IXRHoverInteractor interactorObject { get; set; }`<br/>`IXRHoverInteractable interactableObject { get; set; }`<br/><br/>SelectEnterEventArgs and SelectExitEventArgs<br/>`IXRSelectInteractor interactorObject { get; set; }`<br/>`IXRSelectInteractable interactableObject { get; set; }`|
  ```csharp
  // Example Interactable that overrides an interaction event method.
  public class ExampleInteractable : XRBaseInteractable
  {
      // Old code
      protected override void OnSelectEntering(SelectEnterEventArgs args)
      {
          base.OnSelectEntering(args);
          XRBaseInteractor interactor = args.interactor;
          // Do something with interactor
      }

      // New code
      protected override void OnSelectEntering(SelectEnterEventArgs args)
      {
          base.OnSelectEntering(args);
          var interactor = args.interactorObject;
          // Do something with interactor
      }
  }

  // Example Interactor that overrides GetValidTargets.
  public class ExampleInteractor : XRRayInteractor
  {
      // Old code
      public override void GetValidTargets(List<XRBaseInteractable> targets)
      {
          base.GetValidTargets(targets);
          // Do additional filtering or prioritizing of Interactable candidates in targets list
      }

      // New code
      public override void GetValidTargets(List<IXRInteractable> targets)
      {
          base.GetValidTargets(targets);
          // Do additional filtering or prioritizing of Interactable candidates in targets list
      }
  }
  ```
- Changed Interactors and Interactables so they support having multiple selections, similarly to how they could have multiple components they were either hovering over or being hovered over by.
  |Old Pseudocode Snippets|New Pseudocode Snippets|
  |---|---|
  |`XRBaseInteractor.selectTarget != null`|`IXRSelectInteractor.hasSelection`|
  |`XRBaseInteractor.selectTarget`|`// Getting the first selected Interactable`<br/>`IXRSelectInteractor.hasSelection ? IXRSelectInteractor.interactablesSelected[0] : null`<br/>or<br/>`using System.Linq;`<br/>`IXRSelectInteractor.interactablesSelected.FirstOrDefault();`|
  |`var targets = new List<XRBaseInteractable>();`<br/>`XRBaseInteractor.GetHoverTargets(targets);`|`IXRHoverInteractor.interactablesHovered`|
  |`XRBaseInteractable.hoveringInteractors`|`IXRHoverInteractable.interactorsHovering`|
  |`XRBaseInteractable.selectingInteractor`|`IXRSelectInteractable.interactorsSelecting`|
  ```csharp
  // Example Interactor that overrides a predicate method.
  public class ExampleInteractor : XRBaseInteractor
  {
      // Old code
      public override bool CanSelect(XRBaseInteractable interactable)
      {
          return base.CanSelect(interactable) && (selectTarget == null || selectTarget == interactable);
      }

      // New code
      public override bool CanSelect(IXRSelectInteractable interactable)
      {
          return base.CanSelect(interactable) && (!hasSelection || IsSelecting(interactable));
      }
  }
  ```
- Changed `XRInteractionManager` methods `ClearInteractorSelection` and `ClearInteractorHover` from `public` to `protected`. These are invoked each frame automatically and were not intended to be called by external scripts.
- Changed behaviors that used the `attachTransform` property of `XRBaseInteractor` and `XRGrabInteractable` to instead use `IXRInteractor.GetAttachTransform(IXRInteractable)` and `IXRInteractable.GetAttachTransform(IXRInteractor)` when possible. Users can override the `GetAttachTransform` methods to customize which `Transform` should be used for a given Interactor or Interactable.
- Changed Interactor and Interactable interaction Layer checks to use the new `InteractionLayerMask` instead of the Unity physics `LayerMask`. Layers for the Interaction Layer Mask can be edited separately from Unity physics Layers. A migration tool was added to upgrade the field in all Prefabs and scenes. You will be prompted automatically after upgrading the package, and it can also be done at any time by opening **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management** &gt; **XR Interaction Toolkit** and clicking **Run Interaction Layer Mask Updater**.
- Changed Toggle and Sticky in Select Action Trigger so the toggled on state is now based on whether a selection actually occurred rather than whether there was simply a valid target. This means that a user that presses the select button while pointing at a valid target but one that can not be selected will no longer be in a toggled on state to select other interactables that can be selected.
- Changed Socket Interactor so the hover mesh can appear for all valid Interactable components, not just Grab Interactable components.
- Changed `XRRayInteractor.TranslateAnchor` so the Ray Origin Transform is passed instead of the Original Attach Transform, and renamed the parameter from `originalAnchor` to `rayOrigin`.
- Changed `HoverEnterEventArgs`, `HoverExitEventArgs`, `SelectEnterEventArgs`, and `SelectExitEventArgs` by adding a `manager` property of type `XRInteractionManager`.
- Changed minimum supported version of the Unity Editor from 2019.3 to 2019.4 (LTS).

### Deprecated
- Deprecated `XRRig` which was replaced by [XROrigin](https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.XROrigin.html) in a new dependent package [XR Core Utilities](https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/manual/index.html). `XROrigin` combines the functionality of `XRRig` and `ARSessionOrigin`.
- Deprecated `XRBaseInteractor.requireSelectExclusive` which was used by `XRSocketInteractor`. That logic was moved into `CanSelect` by utilizing the `isSelected` property of the interactable.
- Deprecated `XRRayInteractor.originalAttachTransform` and replaced with `rayOriginTransform`. The original pose of the Attach Transform can now be obtained with new methods (`GetAttachPoseOnSelect` and `GetLocalAttachPoseOnSelect`).
- Deprecated `GetControllerState` and `SetControllerState` from the `XRBaseController`. That logic was moved into the `currentControllerState` property.
- Deprecated `XRControllerState.poseDataFlags` due to being replaced by the new field `XRControllerState.inputTrackingState` to track the controller pose state.
- Deprecated the `XRControllerState` constructor; the `inputTrackingState` parameter is now required.
- Deprecated `AddRecordingFrame(double, Vector3, Quaternion, bool, bool, bool)` in the `XRControllerRecording`; use `AddRecordingFrame(XRControllerState)` or `AddRecordingFrameNonAlloc` instead.

### Fixed
- Fixed Teleportation Areas and Anchors causing undesired teleports when two different Ray Interactors are pointed at them by setting their default Select Mode to Multiple. By default, a teleport would be triggered On Select Exit, but that would occur when each Ray Interactor would take selection. Users with existing projects should change the Select Mode to Multiple.
- Fixed Sockets sometimes showing either the wrong hover mesh or appearing while selected for a single frame when the selection state changed that frame.
- Fixed Sockets sometimes showing the hover mesh for a single frame after another Interactor would take its selection when the Recycle Delay Time should have suppressed it from appearing.
- Fixed controller's recording serialization losing data when restarting the Unity Editor.
- Fixed releasing `XRGrabInteractable` objects after a teleport from having too much energy.
- Fixed `pixelDragThresholdMultiplier` not being squared when calculating the threshold in `UIInputModule`. To keep the same drag threshold you should update the `Tracked Device Drag Threshold Multiplier` property of your `XRUIInputModule` (and your subclasses of `UIInputModule`) to its square root in the Inspector window; for example, a value of `2` should be changed to `1.414214` (or `sqrt(2)`). ([1348680](https://issuetracker.unity3d.com/product/unity/issues/guid/1348680))

## [2.0.0-pre.2] - 2021-11-04

### Changed
- Changed package version for internal release.

## [2.0.0-pre.1] - 2021-10-22

### Changed
- Changed package version for internal release.

## [1.0.0-pre.8] - 2021-10-26

### Changed
- Changed the setter of `XRBaseInteractable.selectingInteractor` from `private` to `protected`.

### Fixed
- Fixed `XRBaseController` so its default `XRControllerState` is no longer constructed with a field initializer to avoid allocation when not needed, such as when it is replaced with `SetControllerState`.
- Fixed `XRUIInputModule` not processing clicks properly when using simulated touches, such as when using the Device Simulator view. This change means mouse input is not processed when there are touches, matching the behavior of other modules like the Standalone Input Module.
- Fixed Direct Interactor logging a warning about not having a required trigger Collider when it has a Rigidbody.
- Fixed missing dependency on `com.unity.modules.physics`.
- Fixed the sort order of ray casts returned by `TrackedDevicePhysicsRaycaster.Raycast` so that distance is in ascending order (closest first). It was previously returning in descending order (furthest first). In practice, this bug did not affect users since `EventSystem.RaycastAll` would correct the order.

## [1.0.0-pre.6] - 2021-09-10

### Changed
- Changed `ARGestureInteractor.GetValidTargets` to no longer filter out Interactable objects based on the camera direction. The optimization method used was faulty and could cause Interactable objects that were still visible to be excluded from the list. ([1354009](https://issuetracker.unity3d.com/product/unity/issues/guid/1354009))

### Fixed
- Fixed Tracked Device Physics Raycaster so it will include ray cast hits for GameObjects that did not have an event handler. This bug was causing events like `IPointerEnterHandler.OnPointerEnter` to not be invoked when the hit was on a child Collider that did not itself have an event handler. ([1356459](https://issuetracker.unity3d.com/product/unity/issues/guid/1356459))
- Fixed `XRBaseInteractable.isHovered` so it only gets set to `false` when all Interactors exit hovering. It was previously getting set to `false` when any Interactor would exit hovering even if another Interactor was still hovering.
- Fixed use of obsolete properties in `TrackedPoseDriver` when using Input System package version 1.1.0-pre.6 or newer.
- Fixed the Default Input Actions sample to be compatible with Input System package version 1.1.0 by merging the two bindings for the Turn action into one binding with both Sector interactions.
- Fixed the Socket Interactor hover mesh not matching the actual pose the Grab Interactable would attach to in the case when its attach transform was offset or rotated. Also fixed the pose of child meshes. ([1358567](https://issuetracker.unity3d.com/product/unity/issues/guid/1358567))
- Fixed Interactable objects not being considered valid targets for Direct and Socket Interactors when the Interactable was registered after it had entered the trigger collider of the Interactor. Note that Unity rules for [Colliders](https://docs.unity3d.com/Manual/CollidersOverview.html) and [OnTriggerEnter](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTriggerEnter.html)/[OnTriggerExit](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnTriggerExit.html) still applies where the Interactable GameObject being deactivated and then moved will cause the Interactor to miss the trigger enter/exit event. If the object is manipulated in that way, those trigger methods need to be manually called to inform the Direct or Socket Interactor. ([1340469](https://issuetracker.unity3d.com/product/unity/issues/guid/1340469))
- Fixed the Trigger Pressed and Grip Pressed buttons not working on the XR Controller (Device-based). They were also renamed to Trigger Button and Grip Button to match the corresponding `CommonUsages` name.

## [1.0.0-pre.5] - 2021-08-02

### Added
- Added public events to `UIInputModule` which correspond to calls to `EventSystem.Execute` and `EventSystem.ExecuteHierarchy` to allow the events to be globally handled.
- Added profiler markers to `XRInteractionManager` to help with performance analysis.
- Added ability for the Animation and Physics 2D built-in packages to be optional.

### Changed
- Changed `XRBaseInteractable.GetDistanceSqrToInteractor` to not consider disabled Colliders or Colliders on disabled GameObjects. This logic is used by `XRDirectInteractor` and `XRSocketInteractor` to find the closest interactable to select.

### Fixed
- Fixed poor performance scaling of `XRInteractionManager` as the number of valid targets and hover targets of an Interactor increased. AR projects with hundreds of gesture interactables should see a large speedup.
- Fixed AR Gesture Recognizers producing GC allocations each frame when there were no touches.
- Fixed issue involving multiple Interactables that reference the same Collider in their Colliders list. Unregistering an Interactable will now only cause the Collider association to be removed from the `XRInteractionManager` if it's actually associated with that same Interactable.
- Fixed the Inspector showing a warning about a missing XR Controller when the Interactor is able to find one on a parent GameObject.

## [1.0.0-pre.4] - 2021-05-14

### Added
- Added Tracked Device Physics Raycaster component to enable physics-based UI interaction through Unity's Event System. This is similar to Physics Raycaster from the Unity UI package, but with support for ray casts from XR Controllers.
- Added `finalizeRaycastResults` event to `UIInputModule` that allows a callback to modify ray cast results before they are used by the event system.
- Added column to XR Interaction Debugger to show an Interactor's valid targets from `XRBaseInteractor.GetValidTargets`.
- Added property to XR Controller to allow the model to be set to a child object instead of forcing it to be instantiated from prefab.

### Changed
- Changed Grab Interactable to have a consistent attach point between all Movement Type values, fixing it not attaching at the Attach Transform when using Instantaneous when the object's Transform position was different from the Rigidbody's center of mass. To use the old method of determining the attach point in order to avoid needing to modify the Attach Transform for existing projects, set Attach Point Compatibility Mode to Legacy. Legacy mode will be removed in a future version. ([1294410](https://issuetracker.unity3d.com/product/unity/issues/guid/1294410))
- Changed Grab Interactable to also set the Rigidbody to kinematic upon being grabbed when the Movement Type is Instantaneous, not just when Kinematic. This improves how it collides with other Rigidbody objects.
- Changed Grab Interactable to allow its Attach Transform to be updated while grabbed instead of only using its pose at the moment of being grabbed. This requires not using Legacy mode.
- Changed Grab Interactable to no longer use the scale of the selecting Interactor's Attach Transform. This often caused unintended offsets when grabbing objects. The position of the Attach Transform should be used for this purpose rather than the scale. Projects that depended on that functionality can use Legacy mode to revert to the old method.
- Changed Grab Interactable default Movement Type from Kinematic to Instantaneous.
- Changed Grab Interactable default values for damping and scale so Velocity Tracking moves more similar to the other Movement Type values, making the distinguishing feature instead be how it collides with other Colliders without Rigidbody components. Changed `velocityDamping` from 0.4 to 1, `angularVelocityDamping` from 0.4 to 1, and `angularVelocityScale` from 0.95 to 1.
- Changed Socket Interactor override of the Movement Type of Interactables from Kinematic to Instantaneous.
- Changed XR Controller so it does not modify the Transform position, rotation, or scale of the instantiated model prefab upon startup instead of resetting those values.
- Changed Controller Interactors to let the XR Controller be on a parent GameObject.
- Changed so XR Interaction Debugger's Input Devices view is off by default.
- Changed Tracked Device Graphic Raycaster to fallback to using `Camera.main` when the Canvas does not have an Event Camera set.
- Changed XR Rig property for the Tracking Origin Mode to only contain supported modes. A value of Not Specified will use the default mode of the XR device.
- Changed **GameObject &gt; XR** menu to only have a single XR Rig rather than separate menu items for Room-Scale and Stationary. Change the Tracking Origin Mode property on the created XR Rig to Floor or Device, respectively, for the same behavior as before.

### Deprecated
- Deprecated `XRBaseController.modelTransform` due to being renamed to `XRBaseController.modelParent`.
- Deprecated `XRRig.trackingOriginMode` due to being replaced with an enum type that only contains supported modes. Use `XRRig.requestedTrackingOriginMode` and `XRRig.currentTrackingOriginMode` instead.

### Fixed
- Fixed Interaction Manager throwing exception `InvalidOperationException: Collection was modified; enumeration operation may not execute.` when an Interactor or Interactable was registered or unregistered during processing and events.
- Fixed Windows Mixed Reality controllers having an incorrect pose when using the Default Input Actions sample. The Position and Rotation input actions will try to bind to `pointerPosition` and `pointerRotation`, and fallback to `devicePosition` and `deviceRotation`. If the sample has already been imported into your project, you will need to import again to get the update.
- Fixed Input System actions such as Select not being recognized as pressed in `ActionBasedController` when it was bound to an Axis control (for example '<XRController>/grip') rather than a Button control (for example '<XRController>/gripPressed').
- Fixed XR Interaction Debugger to display Interactors and Interactables from multiple Interaction Managers.
- Fixed XR Interaction Debugger having overlapping text when an Interactor was hovering over multiple Interactables.
- Fixed Tree View panels in the XR Interaction Debugger to be collapsible.
- Fixed `TestFixture` classes in the test assembly to be `internal` instead of `public`.
- Fixed Grab Interactable to use scaled time for easing and smoothing instead of unscaled time.
- Fixed Direct and Socket Interactor not being able to interact with an Interactable with multiple Colliders when any of the Colliders leaves the trigger instead of only when all of them leave. ([1325375](https://issuetracker.unity3d.com/product/unity/issues/guid/1325375))
- Fixed Direct and Socket Interactor not being able to interact with an Interactable when either were registered after the trigger collision occurred.
- Fixed `XRSocketInteractor` to include the select target in its list of valid targets returned by `GetValidTargets`.
- Fixed `XRBaseController` so it applies the controller state during Before Render even when Input Tracking is disabled.
- Fixed missing namespace of `InputHelpers` to be `UnityEngine.XR.Interaction.Toolkit`.

## [1.0.0-pre.3] - 2021-03-18

### Added
- Added ability for serialized fields added in derived behaviors to automatically appear in the Inspector. Users will no longer need to create a custom [Editor](https://docs.unity3d.com/ScriptReference/Editor.html) to be able to see those fields in the Inspector. See [Extending the XR Interaction Toolkit](../manual/extending-xri.html) in the manual for details about customizing how they are drawn.
- Added support for `EnhancedTouch` from the Input System for AR gesture classes. This means AR interaction is functional when the Active Input Handling project setting is set to Input System Package (New).
- Added registration events to `XRBaseInteractable` and `XRBaseInteractor` which work like those in `XRInteractionManager` but for just that object.
- Added new methods in `ARPlacementInteractable` to divide the logic in `OnEndManipulation` into `TryGetPlacementPose`, `PlaceObject`, and `OnObjectPlaced`.
- Added `XRRayInteractor.hitClosestOnly` property to limit the number of valid targets. Enable this to make only the closest Interactable receive hover events rather than all Interactables in the full length of the ray cast.
- Added new methods in `XRRayInteractor` for getting information about UI hits, and made more methods `virtual` or `public`.
- Added several properties to Grab Interactable (Damping and Scale) to allow for tweaking the velocity and angular velocity when the Movement Type is Velocity Tracking. These values can be adjusted to reduce oscillation and latency from the Interactor.

### Changed
- Changed script execution order so `LocomotionProvider` occurs before Interactors are processed, fixing Ray Interactor from casting with stale controller poses when moving or turning the rig and causing visual flicker of the line.
- Changed script execution order so `XRUIInputModule` processing occurs after `LocomotionProvider` and before Interactors are processed to fix the frame delay with UI hits due to using stale ray cast rays. `XRUIInputModule.Process` now does nothing, override `XRUIInputModule.DoProcess` which is called directly from `Update`.
- Changed `XRUIInputModule.DoProcess` from `abstract` to `virtual`. Overriding methods in derived classes should call `base.DoProcess` to ensure `IUpdateSelectedHandler` event sending occurs as before.
- Changed Ray Interactor's Reference Frame property to use global up as a fallback when not set instead of the Interactor's up.
- Changed Ray Interactor Projectile Curve to end at ground height rather than controller height. Additional Ground Height and Additional Flight Time properties can be adjusted to control how long the curve travels, but this change means the curve will be longer than it was in previous versions.
- Changed `TrackedDeviceGraphicRaycaster` to ignore Trigger Colliders by default when checking for 3D occlusion. Added `raycastTriggerInteraction` property to control this.
- Changed `XRBaseInteractor.allowHover` and `XRBaseInteractor.allowSelect` to retain their value instead of getting changed to `true` during `OnEnable`. Their initial values are unchanged, remaining `true`.
- Changed some AR behaviors to be more configurable rather than using some hardcoded values or requiring using MainCamera. AR Placement Interactable and AR Translation Interactable must now specify a Fallback Layer Mask to support non-trackables instead of always using Layer 9.
- Changed `IUIInteractor` to not inherit from `ILineRenderable`.

### Deprecated
- Deprecated `XRBaseInteractor.enableInteractions`, use `XRBaseInteractor.allowHover` and `XRBaseInteractor.allowSelect` instead.

### Removed
- Removed several MonoBehaviour message functions in AR behaviors to use `ProcessInteractable` and `ProcessInteractor` instead.

### Fixed
- Fixed issue where the end of a Projectile or Bezier Curve lags behind and appears bent when the controller is moved too fast. ([1291060](https://issuetracker.unity3d.com/product/unity/issues/guid/1291060))
- Fixed Ray Interactor interacting with Interactables that are behind UI. ([1312217](https://issuetracker.unity3d.com/product/unity/issues/guid/1312217))
- Fixed `XRRayInteractor.hoverToSelect` not being functional. ([1301630](https://issuetracker.unity3d.com/product/unity/issues/guid/1301630))
- Fixed Ray Interactor not allowing for valid targets behind an Interactable with multiple Collider objects when the ray hits more than one of those Colliders.
- Fixed Ray Interactor performance to only perform ray casts once per frame instead of each time `GetValidTargets` is called by doing it during `ProcessInteractor` instead.
- Fixed exception in `XRInteractorLineVisual` when changing the Sample Frequency or Line Type of a Ray Interactor.
- Fixed Ray Interactor anchor control rotation when the Rig plane was not up. Added a property `anchorRotateReferenceFrame` to control the rotation axis.
- Fixed Reference Frame missing from the Ray Interactor Inspector when the Line Type was Bezier Curve.
- Fixed mouse scroll amount being too large in `XRUIInputModule` when using Input System.
- Fixed Scrollbar initially scrolling to incorrect position at XR pointer down when using `TrackedDeviceGraphicRaycaster`, which was caused by `RaycastResult.screenPosition` never being set.
- Fixed `GestureRecognizer` skipping updating some gestures during the same frame when another gesture finished.
- Fixed namespace of several Editor classes to be in `UnityEditor.XR.Interaction.Toolkit` instead of `UnityEngine.XR.Interaction.Toolkit`.
- Fixed default value of Blocking Mask on Tracked Device Graphic Raycaster to be Everything (was skipping Layer 31).

## [1.0.0-pre.2] - 2021-01-20

### Added
- Added registration events to `XRInteractionManager` and `OnRegistered`/`OnUnregistered` methods to `XRBaseInteractable` and `XRBaseInteractor`.
- Added and improved XML documentation comments and tooltips.
- Added warnings to XR Controller (Action-based) when referenced Input Actions have not been enabled.
- Added warning to Tracked Device Graphic Raycaster when the Event Camera is not set on the World Space Canvas.

### Changed
- Changed `XRBaseInteractable` and `XRBaseInteractor` to no longer register with `XRInteractionManager` in `Awake` and instead register and unregister in `OnEnable` and `OnDisable`, respectively.
- Changed the signature of all interaction event methods (e.g. `OnSelectEntering`) to take event data through a class argument rather than being passed the `XRBaseInteractable` or `XRBaseInteractor` directly. This was done to allow for additional related data to be provided by the Interaction Manager without requiring users to handle additional methods. This also makes it easier to handle the case when the selection or hover is canceled (due to either the Interactor or Interactable being unregistered as a result of being disabled or destroyed) without needing to duplicate code in an `OnSelectCanceling` and `OnSelectCanceled`.
  |Old Signature|New Signature|
  |---|---|
  |`OnHoverEnter*(XRBaseInteractor interactor)`<br/>-and-<br/>`OnHoverEnter*(XRBaseInteractable interactable)`|`OnHoverEnter*(HoverEnterEventArgs args)`|
  |`OnHoverExit*(XRBaseInteractor interactor)`<br/>-and-<br/>`OnHoverExit*(XRBaseInteractable interactable)`|`OnHoverExit*(HoverExitEventArgs args)`|
  |`OnSelectEnter*(XRBaseInteractor interactor)`<br/>-and-<br/>`OnSelectEnter*(XRBaseInteractable interactable)`|`OnSelectEnter*(SelectEnterEventArgs args)`|
  |`OnSelectExit*(XRBaseInteractor interactor)`<br/>-and-<br/>`OnSelectExit*(XRBaseInteractable interactable)`|`OnSelectExit*(SelectExitEventArgs args)` and using `!args.isCanceled`|
  |`OnSelectCancel*(XRBaseInteractor interactor)`|`OnSelectExit*(SelectExitEventArgs args)` and using `args.isCanceled`|
  |`OnActivate(XRBaseInteractor interactor)`|`OnActivated(ActivateEventArgs args)`|
  |`OnDeactivate(XRBaseInteractor interactor)`|`OnDeactivated(DeactivateEventArgs args)`|
  ```csharp
  // Example Interactable that overrides an interaction event method.
  public class ExampleInteractable : XRBaseInteractable
  {
      // Old code -- delete after migrating to new method signature
      protected override void OnSelectEntering(XRBaseInteractor interactor)
      {
          base.OnSelectEntering(interactor);
          // Do something with interactor
      }

      // New code
      protected override void OnSelectEntering(SelectEnterEventArgs args)
      {
          base.OnSelectEntering(args);
          var interactor = args.interactor;
          // Do something with interactor
      }
  }

  // Example behavior that is the target of an Interactable Event set in the Inspector with a Dynamic binding.
  public class ExampleListener : MonoBehaviour
  {
      // Old code -- delete after migrating to new method signature and fixing reference in Inspector
      public void OnSelectEntered(XRBaseInteractor interactor)
      {
          // Do something with interactor
      }

      // New code
      public void OnSelectEntered(SelectEnterEventArgs args)
      {
          var interactor = args.interactor;
          // Do something with interactor
      }
  }
  ```
- Changed which methods are called by the Interaction Manager when either the Interactor or Interactable is unregistered. Previously `XRBaseInteractable` had `OnSelectCanceling` and `OnSelectCanceled` called on select cancel, and `OnSelectExiting` and `OnSelectExited` called when not canceled. This has been combined into `OnSelectExiting(SelectExitEventArgs)` and `OnSelectExited(SelectExitEventArgs)` and the `isCanceled` property is used to distinguish as needed. The **Select Exited** event in the Inspector is invoked in either case.
  ```csharp
  public class ExampleInteractable : XRBaseInteractable
  {
      protected override void OnSelectExiting(SelectExitEventArgs args)
      {
          base.OnSelectExiting(args);
          // Do something common to both.
          if (args.isCanceled)
              // Do something when canceled only.
          else
              // Do something when not canceled.
      }

  }
  ```
- Changed many custom Editors to also apply to child classes so they inherit the custom layout of the Inspector. If your derived class adds a `SerializeField` or public field, you will need to create a custom [Editor](https://docs.unity3d.com/ScriptReference/Editor.html) to be able to see those fields in the Inspector. For Interactor and Interactable classes, you will typically only need to override the `DrawProperties` method in `XRBaseInteractorEditor` or `XRBaseInteractableEditor` rather than the entire `OnInspectorGUI`. See [Extending the XR Interaction Toolkit](../manual/extending-xri.html) in the manual for a code example.
- Changed `XRInteractionManager.SelectCancel` to call `OnSelectExiting` and `OnSelectExited` on both the `XRBaseInteractable` and `XRBaseInteractor` in a similar interleaved order to other interaction state changes and when either is unregistered.
- Changed order of `XRInteractionManager.UnregisterInteractor` to first cancel the select state before canceling hover state for consistency with the normal update loop which exits select before exiting hover.
- Changed `XRBaseInteractor.StartManualInteraction` and `XRBaseInteractor.EndManualInteraction` to go through `XRInteractionManager` rather than bypassing constraints and events on the Interactable.
- Changed the **GameObject > XR > Grab Interactable** menu item to create a visible cube and use a Box Collider so that it is easier to use.
- Renamed `LocomotionProvider.startLocomotion` to `LocomotionProvider.beginLocomotion` for consistency with method name.

### Fixed
- Fixed Direct Interactor and Socket Interactor causing exceptions when a valid target was unregistered, such as from being destroyed.
- Fixed Ray Interactor clearing custom direction when initializing (fixed initialization of the Original Attach Transform so it copies values from the Attach Transform instead of setting position and rotation values to defaults). ([1291523](https://issuetracker.unity3d.com/product/unity/issues/guid/1291523))
- Fixed Socket Interactor so only an enabled Renderer is drawn while drawing meshes for hovered Interactables.
- Fixed Grab Interactable to respect Interaction Layer Mask for whether it can be hovered by an Interactor instead of always allowing it.
- Fixed Grab Interactable so it restores the Rigidbody's drag and angular drag values on drop.
- Fixed mouse input not working with Unity UI when Active Input Handling was set to Input System Package.
- Fixed issue where Interactables in AR were translated at the height of the highest plane regardless of where the ray is cast.
- Fixed so steps to setup camera in `XRRig` only occurs in Play mode in the Editor.
- Fixed file names of .asmdef files to match assembly name.
- Fixed broken links for the help button (?) in the Inspector so it opens Scripting API documentation for each behavior in the package. ([1291475](https://issuetracker.unity3d.com/product/unity/issues/guid/1291475))
- Fixed XR Rig so it handles the Tracking Origin Mode changing on the device.
- Fixed XR Controller so it only sets position and rotation while the controller device is being tracked instead of resetting to the origin (such as from the device disconnecting or opening a system menu).

## [1.0.0-pre.1] - 2020-11-14

### Removed
- Removed anchor control deadzone properties from XR Controller (Action-based) used by Ray Interactor, it should now be configured on the Actions themselves

## [0.10.0-preview.7] - 2020-11-03

### Added
- Added multi-object editing support to all Editors

### Fixed
- Fixed Inspector foldouts to keep expanded state when clicking between GameObjects

## [0.10.0-preview.6] - 2020-10-30

### Added
- Added support for haptic impulses in XR Controller (Action-based)

### Fixed
- Fixed issue with actions not being considered pressed the frame after triggered
- Fixed issue where an AR test would fail due to the size of the Game view
- Fixed exception when adding an Input Action Manager while playing

## [0.10.0-preview.5] - 2020-10-23

### Added
- Added sample containing default set of input actions and presets

### Fixed
- Fixed issue with PrimaryAxis2D input from mouse not moving the scroll bars on UI as expected. ([1278162](https://issuetracker.unity3d.com/product/unity/issues/guid/1278162))
- Fixed issue where Bezier Curve did not take into account controller tilt. ([1245614](https://issuetracker.unity3d.com/product/unity/issues/guid/1245614))
- Fixed issue where a socket's hover mesh was offset. ([1285693](https://issuetracker.unity3d.com/product/unity/issues/guid/1285693))
- Fixed issue where disabling parent before `XRGrabInteractable` child was causing an error in `OnSelectCanceling`

## [0.10.0-preview.4] - 2020-10-14

### Fixed
- Fixed migration of a renamed field in interactors

## [0.10.0-preview.3] - 2020-10-14

### Added
- Added ability to control whether the line will always be cut short at the first ray cast hit, even when invalid, to the Interactor Line Visual ([1252532](https://issuetracker.unity3d.com/product/unity/issues/guid/1252532))

### Changed
- Renamed `OnSelectEnter`, `OnSelectExit`, `OnSelectCancel`, `OnHoverEnter`, `OnHoverExit`, `OnFirstHoverEnter`, and `OnLastHoverExit` to `OnSelectEntered`, `OnSelectExited`, `OnSelectCanceled`, `OnHoverEntered`, `OnHoverExited`, `OnFirstHoverEntered`, and `OnLastHoverExited` respectively.
- Replaced some `ref` parameters with `out` parameters in `ILineRenderable`; callers should replace `ref` with `out`

### Fixed
- Fixed Tracked Device Graphic Raycaster not respecting the Raycast Target property of UGUI Graphic when unchecked ([1221300](https://issuetracker.unity3d.com/product/unity/issues/guid/1221300))
- Fixed XR Ray Interactor flooding the console with assertion errors when sphere cast is used ([1259554](https://issuetracker.unity3d.com/product/unity/issues/guid/1259554), [1266781](https://issuetracker.unity3d.com/product/unity/issues/guid/1266781))
- Fixed foldouts in the Inspector to expand or collapse when clicking the label, not just the icon ([1259683](https://issuetracker.unity3d.com/product/unity/issues/guid/1259683))
- Fixed created objects having a duplicate name of a sibling ([1259702](https://issuetracker.unity3d.com/product/unity/issues/guid/1259702))
- Fixed created objects not being selected automatically ([1259682](https://issuetracker.unity3d.com/product/unity/issues/guid/1259682))
- Fixed XRUI Input Module component being duplicated in EventSystem GameObject after creating it from UI Canvas menu option ([1218216](https://issuetracker.unity3d.com/product/unity/issues/guid/1218216))
- Fixed missing AudioListener on created XR Rig Camera ([1241970](https://issuetracker.unity3d.com/product/unity/issues/guid/1241970))
- Fixed several issues related to creating objects from the GameObject menu, such as broken undo/redo and proper use of context object
- Fixed issue where GameObjects parented under an `XRGrabInteractable` did not retain their local position and rotation when drawn as a Socket Interactor Hover Mesh ([1256693](https://issuetracker.unity3d.com/product/unity/issues/guid/1256693))
- Fixed issue where Interaction callbacks (`OnSelectEnter`, `OnSelectExit`, `OnHoverEnter`, and `OnHoverExit`) are triggered before interactor and interactable objects are updated. ([1231662](https://issuetracker.unity3d.com/product/unity/issues/guid/1231662), [1228907](https://issuetracker.unity3d.com/product/unity/issues/guid/1228907), [1231482](https://issuetracker.unity3d.com/product/unity/issues/guid/1231482))

## [0.10.0-preview.2] - 2020-08-26

### Added
- Added XR Device Simulator and sample assets for simulating an XR HMD and controllers using keyboard & mouse

## [0.10.0-preview.1] - 2020-08-10

### Added
- Added continuous move and turn locomotion

### Changed
- Changed accessibility levels to avoid `protected` fields, instead exposed through properties
- Components that use Input System actions no longer automatically enable or disable them. Add the `InputActionManager` component to a GameObject in a scene and use the Inspector to reference the `InputActionAsset` you want to automatically enable at startup.
- Some properties have been renamed from PascalCase to camelCase to conform with coding standard; the API Updater should update usage automatically in most cases

### Fixed
- Fixed compilation issue when AR Foundation package is also installed
- Fixed the Interactor Line Visual lagging behind the controller ([1264748](https://issuetracker.unity3d.com/product/unity/issues/guid/1264748))
- Fixed Socket Interactor not creating default hover materials, and backwards usage of the materials ([1225734](https://issuetracker.unity3d.com/product/unity/issues/guid/1225734))
- Fixed Tint Interactable Visual to allow it to work with objects that have multiple materials
- Improved Tint Interactable Visual to not create a material instance when Emission is enabled on the material

## [0.9.9-preview.3] - 2020-06-24

### Changed
- In progress changes to visibility

## [0.9.9-preview.2] - 2020-06-22

### Changed
- Hack week version push.

## [0.9.9-preview.1] - 2020-06-04

### Changed
- Swaps axis for feature API anchor manipulation

### Fixed
- Fixed controller recording not working
- Start controller recording at 0 time so you do not have to wait for the recording to start playing.

## [0.9.9-preview] - 2020-06-04

### Added
- Added Input System support
- Added ability to query the controller from the interactor

### Changed
- Changed a number of members and properties to be `protected` rather than `private`
- Changed to remove `sealed` from a number of classes.

## [0.9.4-preview] - 2020-04-01

### Fixed
- Fixed to allow 1.3.X or 2.X versions of legacy input helpers to work with the XR Interaction Toolkit.

## [0.9.3-preview] - 2020-01-23

### Added
- Added pose provider support to XR Controller
- Added ability to put objects back to their original hierarchy position when dropping them
- Made teleport configurable to use either activate or select
- Removed need for box colliders behind UI to stop line visuals from drawing through them

### Fixed
- Fixed minor documentation issues
- Fixed passing from hand to hand of objects using direct interactors
- Fixed null ref in controller states clear
- Fixed no "OnRelease" even for Activate on Grabbable

## [0.9.2-preview] - 2019-12-17

### Changed
- Rolled LIH version back until 1.3.9 is on production.

## [0.9.1-preview] - 2019-12-12

### Fixed
- Documentation image fix

## [0.9.0-preview] - 2019-12-06

### Changed
- Release candidate

## [0.0.9-preview] - 2019-12-06

### Changed
- Further release prep

## [0.0.8-preview] - 2019-12-05

### Changed
- Pre-release release.

## [0.0.6-preview] - 2019-10-15

### Changed
- Changes to README.md file

### Fixed
- Further CI/CD fixes.

## [0.0.5-preview] - 2019-10-03

### Changed
- Renamed everything to com.unity.xr.interaction.toolkit / XR Interaction Toolkit

### Fixed
- Setup CI correctly.

## [0.0.4-preview] - 2019-05-08

### Changed
- Bump package version for CI tests.

## [0.0.3-preview] - 2019-05-07

### Added
- Initial preview release of the XR Interaction framework.
