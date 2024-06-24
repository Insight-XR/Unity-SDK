# XR Interaction Toolkit

The XR Interaction Toolkit package is a high-level, component-based, interaction system for creating VR and AR experiences. It provides a framework that makes 3D and UI interactions available from Unity input events. The core of this system is a set of base Interactor and Interactable components, and an Interaction Manager that ties these two types of components together. It also contains components that you can use for locomotion and drawing visuals.

XR Interaction Toolkit contains a set of components that support the following Interaction tasks:
- Cross-platform XR controller input: Meta Quest (Oculus), OpenXR, Windows Mixed Reality, and more.
- Basic object hover, select and grab
- Haptic feedback through XR controllers
- Visual feedback (tint/line rendering) to indicate possible and active interactions
- Basic canvas UI interaction with XR controllers
- Utility for interacting with XR Origin, a VR camera rig for handling stationary and room-scale VR experiences

To use the AR interaction components in the package, you must have the [AR Foundation](https://docs.unity3d.com/Manual/com.unity.xr.arfoundation.html) package in your Project. The AR functionality provided by the XR Interaction Toolkit includes:
- AR gesture system to map screen touches to gesture events
- AR interactable can place virtual objects in the real world
- AR gesture interactor and interactables to translate gestures such as place, select, translate, rotate, and scale into object manipulation
- AR annotations to inform users about AR objects placed in the real world

Finally, its possible to simulate all of your interactions with the [XR Device Simulator](xr-device-simulator.md) in case you don't have the hardware for the project you are working on, or just want to test interactions without entering the headset. For more information, see [XR Device Simulator overview](xr-device-simulator-overview.md).

## Technical details

### Requirements

This version of the XR Interaction Toolkit is compatible with the following versions of the Unity Editor:

* 2021.3 and later

### Dependencies

The XR Interaction Toolkit package has several dependencies which are automatically added to your project when installing:

* [Input System (com.unity.inputsystem)](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)
* [Unity UI (com.unity.ugui)](https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/index.html)
* [XR Core Utilities (com.unity.xr.core-utils)](https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.2/manual/index.html)
* [XR Legacy Input Helpers (com.unity.xr.legacyinputhelpers)](https://docs.unity3d.com/Packages/com.unity.xr.legacyinputhelpers@2.1/manual/index.html)
* Built-in modules
  * [Audio](https://docs.unity3d.com/Manual/com.unity.modules.audio.html)
  * [IMGUI](https://docs.unity3d.com/Manual/com.unity.modules.imgui.html)
  * [Physics](https://docs.unity3d.com/Manual/com.unity.modules.physics.html)

#### Optional dependencies

To enable additional AR interaction components included in the package, [AR Foundation (com.unity.xr.arfoundation)](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest/) must be added to your project using Package Manager.

To enable additional properties in some behaviors, the [Animation](https://docs.unity3d.com/Manual/com.unity.modules.animation.html) module must be added to your project using Package Manager.

### Known limitations

* When using multiple interactor support on XR Grab Interactables and transferring between a socket interactor and direct/ray interactor, if **Attach Ease In Time** is set to 0, there can be a 1 frame visual skip that can occur. To mitigate this visual disturbance, set the **Attach Ease In Time** to a minimum of 0.15. You can also resolve this issue by loading the scene containing the socket interactors after the controller interactors are registered with the XR Interaction Manager if your project does not enable or disable the direct/ray interactors at runtime in order to make the sockets registered last.

* Mouse inputs don't interact with world space UIs when an XR Plug-in Provider in **Edit &gt; Project Settings &gt; XR Plug-in Management** is enabled and running. For more information, please follow the issue tracker. ([1400186](https://issuetracker.unity3d.com/product/unity/issues/guid/1400186/))

* The Poke Point visual in the Poke Interactor prefab in the Starter Assets sample does not hide with the controller model when the Hide Controller On Select property is enabled on the direct/ray interactor.

* The provided Unity Hand shaders and materials in the Hand Interaction Demo sample render only in the left eye when using **Built-in render pipeline and Single-pass instanced render mode** because they are made using Shader Graph. It is recommended to either switch to multi-pass rendering, switch to Universal Render Pipeline, or create a custom shader for hand interaction that does not use Shader Graph.

### Helpful links

If you have a question after reading the documentation, you can:

* Join our [support forum](https://forum.unity.com/forums/xr-interaction-toolkit-and-input.519/).
* Search the [issue tracker](https://issuetracker.unity3d.com/product/unity/issues?project=192&status=1&unity_version=&view=newest) for active issues.
* View our [public roadmap](https://portal.productboard.com/brs5gbymuktquzeomnargn2u) and submit feature requests.
* Download [example projects](https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples) that demonstrates functionality.

### Document revision history

|Date|Reason|
|---|---|
|**February 10, 2023**|Updated known limitations and package dependency versions. Matches package version 2.3.0.|
|**March 4, 2022**|Samples updated and added link to example projects. Matches package version 2.0.1.|
|**February 10, 2022**|Documentation split into multiple pages, added known limitations, and updated for transition from pre-release to released version 2.0.0.|
|**November 17, 2021**|Documentation updated due to change in Input System package related to Game view focus, interaction interfaces, and multiple selections. Matches package version 2.0.0-pre.4.|
|**March 15, 2021**|Documentation updated to reflect change that custom Editor classes are no longer needed to show additional serialized fields. Matches package version 1.0.0-pre.3.|
|**December 14, 2020**|Documentation updated to reflect change to when registration with Interaction Manager occurs and for changes to event signatures. Matches package version 1.0.0-pre.2.|
|**October 20, 2020**|Documentation updated. Matches package version 0.10.0.|
|**January 10, 2020**|Removed private github link.|
|**December 12, 2019**|Fixed image linking.|
|**December 4, 2019**|Document revised with documentation team feedback.|
|**October 3, 2019**|Document update to reflect package naming for release.|
|**September 4, 2019**|Document revised with updated images and component names.|
|**August 13, 2019**|Document edits.|
|**July 31, 2019**|Finalized Locomotion documentation.|
|**July 18, 2019**|Document revised with Locomotion documentation.|
|**July 10, 2019**|Document revised with AR interaction documentation.|
|**June 6, 2019**|Document revised with updated UI module documentation.|
|**May 30, 2018**|Document revised with commands and updated component names.|
|**May 1, 2018**|Document created.|
