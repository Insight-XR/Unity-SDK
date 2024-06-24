# What's new in version 2.1

Summary of changes in XR Interaction Toolkit package version 2.1:

With the XR Interaction Toolkit version 2.1 comes a few new highly-requested features and updates. The main features in this release include Dynamic Attachment for XR Grab Interactables, Intention Filtering, and Comfort Mode Vignette components and sample. 

## Added

### Dynamic attachment feature
This allows users to grab (via Direct or Ray Interactor) an XR Grab Interactable so it pivots at the point of contact rather than a predetermined attachment point (typically the middle of the object). This makes the interactions feel much more natural and realistic when compared to their real-world counterparts. When using the Ray Interactor with Force Grab disabled, the object attaches at the point of the ray intersection rather than snapping to the middle of the object. Information for this setting can be found in the [XR Grab Interactable](xr-grab-interactable.md) documentation.

### Intention filtering
This feature allows users to configure interaction filters based on certain criteria. This makes it so that selection of interactables is determined by a weighted score against other interactables nearby or within the same selection path (sphere collider or ray cast hit). There are 3 evaluators that are included in this release that can be used with the XR Target Filter: a Last Selected evaluator, a Distance evaluator, and an Angle evaluator (mostly used with head rotation to determine intent based on gaze). The user can also create and extend evaluators to create their own custom filtering as well. For more information, read the documentation on [Target filters](target-filters.md).

### Comfort mode vignette
This feature wires up to the Main Camera and the locomotion system to provide a visual tunneling effect when the user is locomoting in order to mitigate motion sickness in VR. Each [locomotion provider component](components.md#locomotion) can be set up to provide a custom visual effect depending on the type of locomotion. There are many parameters that can be adjusted to fully customize the vignette experience including aperture size, feathering/blending size, ease in/out timing, and the colors used to fade in/out of the vignette. Please review the [Tunneling Vignette Controller](tunneling-vignette-controller.md) documentation for more information on installation and configuration.

## Updated

### XR UI Input Module updates
These updates allow users to customize how XRI interacts with the standard Unity UI (UGUI) system. The user is now able to set each UI Action on the XR UI Input Module to suit their needs, similar to how the Input System UI Input Module allows customization of these input types. Legacy input is also supported by this Input Module. Additionally, the `XRI Default Input Actions` asset in the `Starter Assets` sample package now includes an `XRI UI` Action Map for UI-specific Input Actions. Also included is a Preset asset to quickly map the actions onto the XR UI Input Module component. Check out the [UI setup](ui-setup.md) documentation for more information.

For a full list of changes and updates in this version, see the [XR Interaction Toolkit package changelog](../changelog/CHANGELOG.html).