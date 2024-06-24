# What's new in version 2.4

Summary of changes in XR Interaction Toolkit package version 2.4:

## Added

### XR Ray Stabilization and Visual Improvements

The XR Interactor Line Visual has been updated so that it bends to the selected interactable by default. This helps improve the natural feeling of the Ray Interactor along with easier interactions for users. Along with the visual changes, the performance has also been improved for the line visual and ray interactor by optimizing most of the line computation math for the Burst compiler. The [Burst package](https://docs.unity3d.com/Manual/com.unity.burst.html) must be installed in your project to take advantage of the optimizations. In addition to the visual and performance updates, ray stabilization has been added by the use of the [XR Transform Stabilizer](xr-transform-stabilizer.md) component. This applies optimized stabilization techniques to remove pose jitter and makes aiming and selecting with rays easier for users. These new updates have been added to the [Starter Assets](samples-starter-assets.md) prefabs and set up for immediate use.

### XR Gaze and Aim Assistance

Building upon gaze interaction support from XRI 2.3, [XR Gaze Assistance](xr-gaze-assistance.md) allows specified controller-based ray interactors to fallback to eye-gaze for primary aiming and selection when they are off screen or pointing off screen. This component enables split interaction functionality to allow the user to aim with eye gaze and select with a controller. The XR Gaze Assistance component also has an aim-assist feature. This works by auto adjusting trajectories for thrown objects or projectiles to help them hit the gazed-at targets. 

### Hand Interaction Additions & Updates

#### XR Input Modality Manager

The new [XR Input Modality Manager](xr-input-modality-manager.md) manages swapping between hands and controllers at runtime based on whether hands and controllers are tracked. Updated prefabs in the package samples to make use of this component.

#### Reactive Hand Visuals

Updated the [Hands Interaction Demo](samples-hands-interaction-demo.md) with new interaction-reactive visuals which respond to interaction strength visually for each finger. The `XR Origin Hands (XR Rig)` prefab in the Hands Interaction Demo was updated to use prefabs for each hand visual with affordances to highlight the fingers during interaction.

#### Hand Tracking for Device Simulator

The [XR Device Simulator](xr-device-simulator-overview.md) has received another big update with the added support for simulating Hand Tracking. This comes with a number of standard, pre-defined poses that can be used to test hand-based interactions from inside the editor. 

### Climb Locomotion Provider

A new [Climb Locomotion Provider](climb-provider.md) and [Climb Interactables](climb-interactable.md) allow users to grab and pull themselves along a set of climbable objects. This works in any direction to create ladders, climbing walls or even monkey bars. A Climb Provider instance has been added to `XR Origin Preconfigured` in the [Starter Assets](samples-starter-assets.md) sample along with a `Climb Sample` prefab that can be tested out in the `DemoScene`. This prefab includes preconfigured Climb Interactables.

### Interaction focus state

A new `Focus State` has been added to interactables. An interactable that is selected is also focused, where it will remain focused until another interactable is selected or the the interactor attempts to select a non-interactable object, essentially clicking away from the object. This can be useful for highlighting an object to perform operations on, such as adjusting size or color in a separate UI panel.

## Changes and Fixes

### XR Interaction Toolkit settings

The XR Interaction Toolkit project settings have been moved underneath the XR Plug-in Management category. Along with this, any project validation rules will now also show up under **XR Plug-in Management** &gt; **Project Validation** area in project settings.

### Legacy XR Interaction Toolkit interaction layers

If you are migrating from any 1.x version of the XR Interaction Toolkit, the interaction layer popup box will no longer appear automatically and instead you will have to manually start the tool by going to **Project Settings** &gt; **XR Plug-in Management** &gt; **XR Interaction Toolkit** and clicking on the **Run Interaction Layer Mask Updater** button to migrate your layer mask settings.

For a full list of changes and updates in this version, see the [XR Interaction Toolkit package changelog](../changelog/CHANGELOG.html).
