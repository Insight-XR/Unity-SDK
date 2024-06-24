---
uid: xr-core-utils-changelog
---
# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/) and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

<!-- Headers should be listed in this order: Added, Changed, Deprecated, Removed, Fixed, Security -->
<!-- Changelog for 2.3.0 -->
## [2.3.0] - 2024-01-17

### Added

- Added a new collection [ReadOnlyList\<T\>](xref:Unity.XR.CoreUtils.ReadOnlyList`1) as a more performant alternative to `ReadOnlyCollection`. `ReadOnlyList` improves upon `ReadOnlyCollection` by removing the heap allocations associated with `GetEnumerator()`.

### Fixed

- Fixed the XmlDocs issues for API documentation.

## [2.3.0-pre.3] - 2023-12-11

### Fixed

- Fix conditional compilation of analytics API on 2023.2 and newer editor versions.

## [2.3.0-pre.2] - 2023-11-27

### Added

- Added the [Building Blocks system](xref:xr-core-utils-building-blocks), an overlay window in the scene view with quick access to commonly used items in the project.
- Added the [Capability Profile system](xref:xr-core-utils-capability-profile) that allows the creation of assets with key-value pairs to abstract the capabilities of a platform, device, OS, or a combination of them.
- Added `EditorAnalyticsEvent` class that can be extended to create editor analytics events. This class supports the new analytics APIs introduced in editor version 2023.2 as well as the analytics APIs from older editor versions.

### Changed

- Changed Project Validation to query `IsRuleEnabled` and `CheckPredicate` before invoking `FixIt` action of a `BuildValidationRule` via **Fix All** button. This prevents `FixIt` action being called unexpectedly via **Fix All** button while the changes by another rule are in progress.

### Fixed

- Fixed the sort order of enabled issues in the Project Validation window to keep them in the original order added in code.
- Fixed to prevent the progress bar from getting stuck when an exception is thrown during the `BuildValidationRule.FixIt` method.
- Fixed a bug in `ScriptableSettingsBase` that would attempt to create a new instance of the settings asset even when the asset already existed.
- Fixed an issue in the Project Validation window where opening a Unity project without window focus could throw an exception.

## [2.3.0-pre.1] - 2023-08-14

### Added

- Added `BuildValidationRule.HighlighterFocus` property in project validation rules to allow for the searching and highlighting of text in the editor.
- Added ability to the Datum property drawer to allow the Use Asset/Use Value property to be reverted separately from the parent property when right-clicking the More menu (`⋮`) button.

### Fixed

- Fixed Datum property drawer so the Use Asset/Use Value dropdown appears directly under the button instead of under the multiline value.

## [2.2.3] - 2023-08-01

### Fixed

- Fixed [`TransformExtensions`](xref:Unity.XR.CoreUtils.TransformExtensions) methods to use `Transform.GetPositionAndRotation`/`Transform.GetLocalPositionAndRotation` and `Transform.SetPositionAndRotation`/`Transform.SetLocalPositionAndRotation` when available to improve performance.

## [2.2.2] - 2023-07-12

### Fixed

- Fixed bug with Datum property editor incorrectly reporting the height of datum properties in the inspector.

## [2.2.1] - 2023-05-02

### Changed

- Renamed the following display names of the properties in `XROrigin` inspector:
  - **Camera GameObject** to **Camera** for `Camera` property
  - **Camera Floor Offset Object** to **Camera Floor Offset GameObject** for `CameraFloorOffsetObject` property

## [2.2.0] - 2023-02-10

### Changed

- Promoted package from prerelease to verified.

## [2.2.0-pre.2] - 2022-11-10

### Added

- Added `SetValueWithoutNotify` method to `BindableVariableBase<T>` to let users set the value without broadcasting to subscribers.
- Added `BuildValidationRule.OnClick` lambda function that is invoked when the rule is clicked in the validator. Also added the `BuildValidator.SelectObject` method to perform the object select logic for rules.
- Added `BuildValidator.FixIssues` method to process and fix a batch of validation rules.

### Changed

- Renamed `UnBindAction` to `UnbindAction` in [EventBinding](xref:Unity.XR.CoreUtils.Bindings.EventBinding).
- The `Fix All` button, in the `Project Validation`, now processes and fixes all issues in a single frame. Set `BuildValidationRule.FixItAutomatic` to `false` if the issue cannot be processed with others in the same frame (Ex. if the fix requires a Unity Editor restart).

### Removed

- Removed `SetValue` method from `BindableVariableBase<T>`. Use the `Value` property setter instead.

### Fixed

- Fixed GC alloc caused by the value comparison in `BindableVariable<T>`.
- Fixed error when calling `GameObjectUtils.GetComponentsInAllScenes<T>` with unloaded scenes in the Hierarchy window.

## [2.2.0-pre.1] - 2022-09-21

### Added

- Added `SwapAtIndices<T>()` function to `ListExtensions` that performs an index-based element swap on any `List<T>`.
- Added bindable variable classes which allow a typed variable to be observed for value changes.
- Added value datum classes, which store data in a `ScriptableObject` or directly within a serializable class. These can be utilized to share common configuration across multiple objects and are used by the affordance system.
- Added common primitive types of `UnityEvent<T>` to allow serialized typed Unity Editor events.
- Added `HashSetList`, which is basically a wrapper for both a `HashSet` and `List` that allows the benefits of O(1) `Contains` checks, while allowing deterministic iteration without allocation.
- Added Multiply, Divide, and SafeDivide Vector3 extensions.

## [2.1.0] - 2022-08-22

### Fixed

- Organized alignment for help button on project validation rules window.
- Fixed alignment issues with the error icon.

## [2.1.0-pre.1] - 2022-04-21

### Added

- Add Project Validation for validating packages against package configuration correctness. See the [manual entry for project validation](xref:xr-core-utils-project-validation) for more details.

### Removed

- Removed the **GameObject** &gt; **XR** &gt; **XR Origin** menu item. To create a new XR Origin, users should instead use the menu items provided by [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.0/manual/index.html#scene-setup) and/or [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest?subfolder=/manual/general-setup.html).

### Fixed

- [NativeArrayUtils.EnsureCapacity](xref:Unity.XR.CoreUtils.NativeArrayUtils.EnsureCapacity*) now checks for unallocated array before disposing it and reallocating for a larger capacity.
- Fixed compilation errors on platforms such as Stadia where the XR module is not available.

## [2.0.0] - 2022-02-16

### Added

- Define constants for documentation to include missing APIs.

### Fixed

- Fixed XR Origin so it appears in the **Component &gt; XR** menu.
- Fixed so test behaviors and [OnDestroyNotifier](xref:Unity.XR.CoreUtils.OnDestroyNotifier) do not show up in the **Component &gt; Add** menu since those are not intended to be used directly by users.

## [2.0.0-pre.6] - 2021-12-10

### Changed

- Stopped firing warnings when users attach the [TrackedPoseDriver](https://docs.unity3d.com/Packages/com.unity.xr.legacyinputhelpers@2.1/api/UnityEngine.SpatialTracking.TrackedPoseDriver.html) from `com.unity.xr.legacyinputhelpers`, but strongly recommend that users attach the [TrackedPoseDriver](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest?subfolder=/api/UnityEngine.InputSystem.XR.TrackedPoseDriver.html) from `com.unity.inputsystem` to the `camera` property of [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) instead. ([1388617](https://issuetracker.unity3d.com/product/unity/issues/guid/1388617))

## [2.0.0-pre.5] - 2021-11-16

### Changed

- Changed property names so that they adhere to PascalCase.

### Added

- Added `XROrigin` menu item to SceneInspector that creates and configures a basic `XROrigin` in the scene.

### Fixed

- Fixed serialization issue where upgrading to [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) from [XRRig](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@1.0/api/UnityEngine.XR.Interaction.Toolkit.XRRig.html) may cause references to be broken in the component.
- Fixed issue where [XROrigin.CameraInOriginSpacePos](xref:Unity.XR.CoreUtils.XROrigin.CameraInOriginSpacePos) was being miscalculated.
- Fixed issue where the custom Inspector for [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) was not being used.
- Fixed warning message referencing an old property name when a Camera could not be found.
- Fixed a reflection issue with [ScriptableSettingsBase.GetInstanceByType](xref:Unity.XR.CoreUtils.ScriptableSettingsBase.GetInstanceByType(System.Type)) by renaming `EditorScriptableSettings<T>.instance` to `EditorScriptableSettings<T>.Instance`.

## [2.0.0-pre.3] - 2021-11-03

This is the first release of *XR Core Utilities* package.

### Added

- XROrigin is a new XR agnostic setup that handles session space. It will be replacing [ARSessionOrigin](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@4.2/api/UnityEngine.XR.ARFoundation.ARSessionOrigin.html?q=ARSessionOrigin) and [XRRig](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@1.0/api/UnityEngine.XR.Interaction.Toolkit.XRRig.html?q=XRRig).
- All the utilities from [XR Tools Utilities](https://docs.unity3d.com/Packages/com.unity.xrtools.utils@latest?preview=1&subfolder=/manual/) (`com.unity.xrtools.utils`) package has been migrated in this package. This includes common utilities used by XR packages like MARS, AR Foundation, and Spatial Framework.

### Changed

- The minimum Unity version for this package is now 2019.4.

### Removed

- Removed `CONTRIBUTING.md` since the package is not accepting external contribution.

### Fixed

- Fixed company name in `LICENSE.md` file from "Unity Technologies ApS" to "Unity Technologies".
