# What's new in version 2.3

Summary of changes in XR Interaction Toolkit package version 2.3:

With the XR Interaction Toolkit version 2.3 comes the most requested features yet. The main features in this release include Poke and Gaze Interactors, Interaction Groups, Snap Volumes, Device Simulator usability improvements, and an Interaction Affordance System, which allows users to easily build high-performance interaction feedback indicators (visual, audio, haptics, etc).

## Added

### Poke Interactor

XRI now includes the [XR Poke Interactor](xr-poke-interactor.md) and [XR Poke Filter](xr-poke-filter.md) classes to provide basic poking functionality for both hands and controllers. The `XRPokeFilter` allows a developer to more strictly define settings such as the direction and depth required to initiate a select action using an `XRPokeInteractor`. In addition to the `XRPokeFilter`, the `TrackedDeviceGraphicsRaycaster` was updated with native support for poke-based UI interaction. This means you will not have to add a collider or interactable component to allow poke-based [UGUI](https://docs.unity3d.com/Manual/com.unity.ugui.html) interactions such as pressing buttons, moving sliders, toggling checkboxes, and interacting with most other types of UI controls.

### Gaze Interactor & Interaction Assistance

A new [XR Gaze Interactor](xr-gaze-interactor.md) is now available, driven by either eye-gaze or head-gaze pose data. This allows a developer to use eye or head gaze for interaction, such as to hover or select by dwelling on interactables. To best support eye gaze, we have also introduced an additional form of interaction assistance with the [XR Interactable Snap Volume](xr-interactable-snap-volume.md). This component is a perfect compliment to the `XRGazeInteractor`, as it allows snapping a ray-based interactor to a nearby target interactable when the ray hits a specific volume of influence around the interactable. This can be used without the `XRGazeInteractor` to enable easier object selection for users.

### Interaction Groups

In addition to the new Poke and Gaze Interactors, XRI 2.3 also provides an [Interaction Group](xr-interaction-group.md) component. This behavior allows a developer to group interactors together, allowing only a single interactor per group to interact at a time. Interactors are mediated based on which one is highest in the list (higher priority) or if an interactor is currently hovering or selecting. For example, when a Poke, Direct, and Ray Interactor are in a group together, a poke interaction may take priority over the other two when it is actively hovering or selecting and hide or disable the hover and select capabilities of the other interactors.

### Interaction Affordance System

One of the biggest underlying changes in XRI 2.3 is the new [Affordance System](affordance-system.md). The affordance system allows a developer to define a specific type of interaction feedback (affordance) for an interactable that can take many forms, including visual changes, such as material color or size, audio clips, or even trigger haptic pulses. In order for the Affordance System to work with XR Interactables, we added the [XR Interactable Affordance State Provider](xr-interactable-affordance-state-provider.md), which connects to an XR Interactable to trigger affordance state changes, which then power Affordance Receivers that handle animating tweens based on user-defined Affordance Theme ScriptableObjects. This can be used for audio, UI, materials, or other types of animation tweens. In order to make this new feature as high-performance as possible, it has been built to run on the [Job System](https://docs.unity3d.com/Manual/JobSystem.html).

## Updates

### Device Simulator improvements

The [XR Device Simulator](xr-device-simulator-overview.md) has received a major overhaul in terms of usability. A new on-screen runtime UI makes it much easier to see what inputs drive the simulator as well as which ones are currently active. In addition to the UI improvements, new simulation modes have been added, allowing you to quickly toggle between the most commonly used control modes. At startup now, the Device Simulator activates the FPS mode which will start manipulating the HMD and controllers as if the whole player was turning their torso, similar to a typical FPS style configuration, to simplify its use. Press `Tab` to cycle between manipulating all devices in this mode, the left controller individually, and the right controller individually. Reimport the XR Device Simulator sample to access this new functionality and UI.

## Changes and Fixes

For a full list of changes and updates in this version, see the [XR Interaction Toolkit package changelog](../changelog/CHANGELOG.html).