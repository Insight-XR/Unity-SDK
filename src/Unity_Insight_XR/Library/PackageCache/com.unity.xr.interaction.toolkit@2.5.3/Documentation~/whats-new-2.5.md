# What's new in version 2.5

Summary of changes in XR Interaction Toolkit package version 2.5:

## Added

### AR interaction refactor

The new [XR Screen Space Controller](xr-screen-space-controller.md) makes use of screen space gesture data to define select states, and use the `XRRayInteractor` for translation and rotation of objects. The `TouchscreenGestureInputController` is an input system controller that provides gesture data that can be used in input action maps. Additionally, the `ARTransformer` can be added to XR Grab Interactables and will provide awareness of AR planes in the scene. The existing XR Origin prefabs in the [Starter Assets](samples-starter-assets.md) sample combined with the AR Transformer can also be used for Mixed Reality applications as well. For a mobile AR specific solution, there is a new [AR Starter Assets](samples-ar-starter-assets.md) sample. These new classes will replace the `ARGestureInteractor` and the translate, rotate, and scale interactables. For more on how to set up applications for touchscreen AR with this new system, refer to the [AR interaction overview](ar-interaction-overview.md).

### More ray-based interaction stabilization

#### Cone casting

The [XR Ray Interactor](xr-ray-interactor.md) has been upgraded with a new cone casting hit detection type. This effectively creates a cone-shaped ray originating from the interactor and returns hits for anything within [cone casting angle](xref:UnityEngine.XR.Interaction.Toolkit.XRRayInteractor#coneCastAngle). This can make it much more user friendly to grab interactables from a distance. It can also be set up with the gaze interactor to provide meaningful interactions when using eye tracking or head rotation.

#### Ray endpoint stabilization

Building upon the [XR ray stabilization and visual improvements from XRI 2.4.0](whats-new-2.4.md#xr-ray-stabilization-and-visual-improvements), ray endpoint stabilization has been added to the [XR Transform Stabilizer](xr-transform-stabilizer.md) component. This effectively smooths where the endpoint is calculated for the ray interactor, reducing ray jitter significantly for users while maintaining performance for all-in-one XR devices.

### XR Socket Interactor auto scaling and snapping

The [XR Socket Interactor](xr-socket-interactor.md) has been updated with snapping and scaling interactions. This allows interactable objects to snap into the socket when reaching a certain hover threshold. In addition to the snapping behavior, the objects can also be scaled to fit within the constraints of the socket itself. This is very useful for creating complex interactions such as a 3D inventory system.

## Changes and fixes

### Updated sample folder structures

The [Starter Assets](samples-starter-assets.md) and [Hands Interaction Demo](samples-hands-interaction-demo.md) samples have been reorganized to make it easier to remove the demo-specific assets from your project, leaving only the foundational elements you might need to jump-start your projects. For the Starter Assets, you can safely delete the `DemoScene.unity` scene asset and related `DemoSceneAssets` folder without impacting functionality of the primary prefabs. Similarly, for the Hands Interaction Demo, you can safely delete the `HandsDemoScene.unity` scene asset and `HandsDemoSceneAssets` folder without impacting the primary hand related prefabs and scripts. For the AR Starter Assets, you can safely delete the `ARDemoScene.unity` scene asset and `ARDemoSceneAssets` folder.

> [!IMPORTANT]
> There is also a known issue when upgrading from an older version of the Starter Assets and Hands Interaction Demo to a newer version. Script references in the Demo Scene and Hands Demo Scene for scripts included in the Starter Assets become disconnected when upgrading in-place. It is recommended that you delete the `Starter Assets` and `Hands Interaction Demo` folders from your Samples directory before importing the new Starter Assets and Hands Interaction Demo samples.


### Tunneling vignette sample has been moved

The assets in the [Tunneling Vignette](samples-starter-assets.md#tunneling-vignette) sample have moved into the main [Starter Assets](samples-starter-assets.md) sample. As a part of this move, the asset GUIDs have been regenerated so the old tunneling vignette assets will not conflict with the newly imported assets from the main Starter Assets sample folder. Additionally, if you have previously used the tunneling vignette in your project, you will need to change the reference to the new asset located in `Starter Assets` &gt; `Prefabs` &gt; `TunnelingVignette`.

For a full list of changes and updates in this version, see the [XR Interaction Toolkit package changelog](../changelog/CHANGELOG.html).