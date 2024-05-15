# XR Ray Interactor

Interactor used for interacting with Interactables at a distance. This is handled via ray casts that update the current set of valid targets for this interactor.

![XRRayInteractor component](images/xr-ray-interactor.png)

| **Property** | **Description** |
|---|---|
| **Interaction Manager** | The [XRInteractionManager](xr-interaction-manager.md) that this Interactor will communicate with (will find one if **None**). |
| **Interaction Layer Mask** | Allows interaction with Interactables whose [Interaction Layer Mask](interaction-layers.md) overlaps with any Layer in this Interaction Layer Mask. |
| **Enable Interaction with UI GameObjects** | Enable to allow this Interactor to affect UI. |
| **Force Grab** | Force grab moves the object to your hand rather than interacting with it at a distance. |
| **Anchor Control** | Allows the user to move the attach anchor point using the joystick. |
| **Translate Speed** | Speed that the anchor is translated. Only used and displayed when **Anchor Control** is enabled. |
| **Rotate Reference Frame** | The optional reference frame to define the up axis when rotating the attach anchor point. When not set, rotates about the local up axis of the attach transform. Only used and displayed when **Anchor Control** is enabled. |
| **Rotation Mode** | Specifies how the anchor rotation is controlled. Only used and displayed when **Anchor Control** is enabled. |
| &emsp;Rotate Over Time | Set **Rotation Mode** to **Rotate Over Time** to control anchor rotation over time while rotation input is active. |
| &emsp;Match Direction | Set **Rotation Mode** to **Match Direction** to match the anchor rotation to the direction of the 2-dimensional rotation input. |
| **Rotate Speed** | Speed that the anchor is rotated. Only used and displayed when **Anchor Control** is enabled and **Rotation Mode** is set to **Rotate Over Time**. |
| **Scale Mode** | Determines how the Scale Value should be used by the interactable objects requesting it. |
| **Attach Transform** | The `Transform` that is used as the attach point for Interactables.<br />Automatically instantiated and set in `Awake` if **None**.<br />Setting this will not automatically destroy the previous object. |
| **Ray Origin Transform** | The starting position and direction of any ray casts.<br />Automatically instantiated and set in `Awake` if **None** and initialized with the pose of the `XRBaseInteractor.attachTransform`. Setting this will not automatically destroy the previous object. |
| **Disable Visuals When Blocked In Group** | Whether to disable visuals when this Interactor is part of an [Interaction Group](xr-interaction-group.md) and is incapable of interacting due to active interaction by another Interactor in the Group. |
| **Line Type** | The type of ray cast. |
| &emsp;Straight Line | Set **Line Type** to **Straight Line** to perform a single ray cast into the scene with a set ray length. |
| &emsp;Projectile Curve | Set **Line Type** to **Projectile Curve** to sample the trajectory of a projectile to generate a projectile curve. |
| &emsp;Bezier Curve | Set **Line Type** to **Bezier Curve** to use a control point and an end point to create a quadratic Bezier curve. |
| &emsp;Max Raycast Distance| Only used and displayed if **Line Type** is **Straight Line**.<br />Increasing this value will make the line reach further. |
| **Reference Frame** | Only used and displayed if **Line Type** is either **Projectile Curve** or **Bezier Curve**.<br />The reference frame of the curve to define the ground plane and up. If not set at startup it will try to find the `XROrigin.Origin` `GameObject`, and if that does not exist it will use global up and origin by default. |
| **Velocity** | Only used and displayed if **Line Type** is **Projectile Curve**. Initial velocity of the projectile.<br />Increasing this value will make the curve reach further. |
| **Acceleration** | Only used and displayed if **Line Type** is **Projectile Curve**.<br />Gravity of the projectile in the reference frame. |
| **Additional Ground Height** | Only used and displayed if **Line Type** is **Projectile Curve**.<br />Additional height below ground level that the projectile will continue to. Increasing this value will make the end point drop lower in height. |
| **Additional Flight Time** | Only used and displayed if **Line Type** is **Projectile Curve**.<br />Additional flight time after the projectile lands at the adjusted ground level. Increasing this value will make the end point drop lower in height. |
| **Sample Frequency** | Only used and displayed if **Line Type** is **Projectile Curve** or **Bezier Curve**.<br />The number of sample points Unity uses to approximate curved paths. Larger values produce a better quality approximate at the cost of reduced performance due to the number of ray casts.<br />A value of `n` will result in `n - 1` line segments for ray casting. This property is not used when using a **Line Type** of **Straight Line** since the effective value would always be 2. |
| **End Point Distance** | Only used and displayed if **Line Type** is **Bezier Curve**.<br />Increase this value distance to make the end of the curve further from the start point. |
| **End Point Height** | Only used and displayed if **Line Type** is **Bezier Curve**.<br />Decrease this value to make the end of the curve drop lower relative to the start point. |
| **Control Point Distance** | Only used and displayed if **Line Type** is **Bezier Curve**.<br />Increase this value to make the peak of the curve further from the start point. |
| **Control Point Height** | Only used and displayed if **Line Type** is **Bezier Curve**.<br />Increase this value to make the peak of the curve higher relative to the start point. |
| **Raycast Mask** | The layer mask used for limiting ray cast targets. |
| **Raycast Trigger Interaction** | The type of interaction with trigger colliders via ray cast. |
| **Raycast Snap Volume Interaction** | Whether ray cast should include or ignore hits on trigger colliders that are snap volume colliders, even if the ray cast is set to ignore triggers. If you are not using gaze assistance or XR Interactable Snap Volume components, you should set this property to Ignore to avoid the performance cost. |
| **Hit Detection Type** | Which type of hit detection to use for the ray cast. |
| &emsp;Raycast | Set **Hit Detection Type** to **Raycast** to use `Physics` Raycast to detect collisions. |
| &emsp;Sphere Cast | Set **Hit Detection Type** to **Sphere Cast** to use `Physics` Sphere Cast to detect collisions. |
| &emsp;Cone Cast | Set **Hit Detection Type** to **Cone Cast** to use cone casting to detect collisions. |
| **Hit Closest Only** | Whether Unity considers only the closest Interactable as a valid target for interaction.<br />Enable this to make only the closest Interactable receive hover events. Otherwise, all hit Interactables will be considered valid and this Interactor will multi-hover. |
| **Blend Visual Line Points** | Blend the line sample points Unity uses for ray casting with the current pose of the controller. Use this to make the line visual stay connected with the controller instead of lagging behind.<br />When the controller is configured to sample tracking input directly before rendering to reduce input latency, the controller may be in a new position or rotation relative to the starting point of the sample curve used for ray casting.<br />A value of `false` will make the line visual stay at a fixed reference frame rather than bending or curving towards the end of the ray cast line. |
| **Select Action Trigger** | Choose how Unity interprets the select input action from the controller. Controls between different input styles for determining if this Interactor can select, such as whether the button is currently pressed or just toggles the active state. When this is set to `State` and multiple interactors select an interactable set to `InteractableSelectMode.Single`, you may experience undesired behavior where the selection of the interactable is passed back and forth between the interactors each frame. This can also cause the select interaction events to fire each frame. This can be resolved by setting this to `State Change` which is the default and recommended option. |
| &emsp;State | Unity will consider the input active while the button is pressed. A user can hold the button before the interaction is possible and still trigger the interaction when it is possible. |
| &emsp;State Change | Unity will consider the input active only on the frame the button is pressed, and if successful remain engaged until the input is released. A user must press the button while the interaction is possible to trigger the interaction. They will not trigger the interaction if they started pressing the button before the interaction was possible. |
| &emsp;Toggle | The interaction starts on the frame the input is pressed and remains engaged until the second time the input is pressed. |
| &emsp;Sticky | The interaction starts on the frame the input is pressed and remains engaged until the second time the input is released. |
| **Keep Selected Target Valid** | Whether to keep selecting an Interactable after initially selecting it even when it is no longer a valid target.<br />Enable to make the `XRInteractionManager` retain the selection even if the Interactable is not contained within the list of valid targets. Disable to make the Interaction Manager clear the selection if it isn't within the list of valid targets.<br />A common use for disabling this is for Ray Interactors used for teleportation to make the teleportation Interactable no longer selected when not currently pointing at it. |
| **Hide Controller On Select** | Controls whether this Interactor should hide the controller model on selection. |
| **Allow Hovered Activate** | Controls whether to send activate and deactivate events to interactables that this interactor is hovered over but not selected when there is no current selection. By default, the interactor will only send activate and deactivate events to interactables that it's selected. |
| **Target Track Mode** | Specifies how many Interactables that should be tracked in the Targets For Selection property, useful for custom feedback. The options are in order of best performance. |
| **Hover To Select** | Enable to have Interactor automatically select an Interactable after hovering over it for a period of time. Will also select UI if Enable Interaction with UI GameObjects is also enabled. |
| **Hover Time To Select** | Number of seconds an Interactor must hover over an Interactable to select it. |
| **Auto Deselect** | Enable to have Interactor automatically deselect an Interactable after selecting it for a period of time. |
| **Time To Auto Deselect** | Number of seconds an Interactor must select an Interactable before it is automatically deselected when **Auto Deselect** is true. |
| **Starting Selected Interactable** | The Interactable that this Interactor automatically selects at startup (optional, may be **None**). |
| **Audio Events** | These tie into the same selection and hover events as the **Interactor Events** further below - these audio events provide a convenient way to play specified audio clips for any of those events you want. |
| &emsp;On Select Entered | If enabled, the Unity editor will display UI for supplying the audio clip to play when this Interactor begins selecting an Interactable. |
| &emsp;On Select Exited | If enabled, the Unity editor will display UI for supplying the audio clip to play when this Interactor successfully exits selection of an Interactable. |
| &emsp;On Select Canceled | If enabled, the Unity editor will display UI for supplying the audio clip to play when this Interactor cancels selection of an Interactable. |
| &emsp;On Hover Entered | If enabled, the Unity editor will display UI for supplying the audio clip to play when this Interactor begins hovering over an Interactable. |
| &emsp;On Hover Exited | If enabled, the Unity editor will display UI for supplying the audio clip to play when this Interactor successfully ends hovering over an Interactable. |
| &emsp;On Hover Canceled | If enabled, the Unity editor will display UI for supplying the audio clip to play when this Interactor cancels hovering over an Interactable. |
| &emsp;Allow Hover Audio While Selecting | Whether to allow playing audio from hover events if the hovered Interactable is currently selected by this Interactor. This is enabled by default. |
| **Haptic Events** | These tie into the same selection and hover events as the **Interactor Events** further below - these haptic events provide a convenient way to provide haptic feedback for any of those events you want. |
| &emsp;On Select Entered | If enabled, the Unity editor will display UI for supplying the duration (in seconds) and intensity (normalized) to play in haptic feedback when this Interactor begins selecting an Interactable. |
| &emsp;On Select Exited | If enabled, the Unity editor will display UI for supplying the duration (in seconds) and intensity (normalized) to play in haptic feedback when this Interactor successfully exits selection of an Interactable. |
| &emsp;On Select Canceled | If enabled, the Unity editor will display UI for supplying the duration (in seconds) and intensity (normalized) to play in haptic feedback when this Interactor cancels selection of an Interactable. |
| &emsp;On Hover Entered | If enabled, the Unity editor will display UI for supplying the duration (in seconds) and intensity (normalized) to play in haptic feedback when this Interactor begins hovering over an Interactable. |
| &emsp;On Hover Exited | If enabled, the Unity editor will display UI for supplying the duration (in seconds) and intensity (normalized) to play in haptic feedback when this Interactor successfully ends hovering over an Interactable. |
| &emsp;On Hover Canceled | If enabled, the Unity editor will display UI for supplying the duration (in seconds) and intensity (normalized) to play in haptic feedback when this Interactor cancels hovering over an Interactable. |
| &emsp;Allow Hover Haptics While Selecting | Whether to allow playing haptics from hover events if the hovered Interactable is currently selected by this Interactor. This is enabled by default. |
| **Interactor Events** | See the [Interactor Events](interactor-events.md) page. |

## Supporting XR Interactable Snap Volume

For an XR Ray Interactor to snap to an [XR Interactable Snap Volume](xr-interactable-snap-volume.md), the ray interactor must be properly configured. The **Raycast Snap Volume Interaction** property of the XR Ray Interactor must be set to **Collide**. Additionally, the XR Ray Interactor GameObject must have an [XR Interactor Line Visual](xr-interactor-line-visual.md) component with the **Snap Endpoint If Available** property enabled.