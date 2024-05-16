# XR Poke Interactor

Interactor used for interacting with interactables through poking.

| **Property** | **Description** |
|---|---|
| **Interaction Manager** | The [XRInteractionManager](xr-interaction-manager.md) that this Interactor will communicate with (will find one if **None**). |
| **Interaction Layer Mask** | Allows interaction with Interactables whose [Interaction Layer Mask](interaction-layers.md) overlaps with any Layer in this Interaction Layer Mask. |
| **Attach Transform** | The `Transform` that is used as the attach point for Interactables.<br />Automatically instantiated and set in `Awake` if **None**.<br />Setting this will not automatically destroy the previous object. |
| **Disable Visuals When Blocked In Group** | Whether to disable visuals when this Interactor is part of an [Interaction Group](xr-interaction-group.md) and is incapable of interacting due to active interaction by another Interactor in the Group. |
| **Starting Selected Interactable** | The Interactable that this Interactor automatically selects at startup (optional, may be **None**). |
| **Keep Selected Target Valid** | Whether to keep selecting an Interactable after initially selecting it even when it is no longer a valid target.<br />Enable to make the `XRInteractionManager` retain the selection even if the Interactable is not contained within the list of valid targets. Disable to make the Interaction Manager clear the selection if it isn't within the list of valid targets.<br />A common use for disabling this is for Ray Interactors used for teleportation to make the teleportation Interactable no longer selected when not currently pointing at it. |
| **Poke Depth** | The depth threshold within which an interaction can begin to be evaluated as a poke. |
| **Poke Width** | The width threshold within which an interaction can begin to be evaluated as a poke. |
| **Poke Select Width** | The width threshold within which an interaction can be evaluated as a poke select. |
| **Poke Hover Radius** | The radius threshold within which an interaction can be evaluated as a poke hover. |
| **Poke Interaction Offset** | Distance along the poke interactable interaction axis that allows for a poke to be triggered sooner/with less precision. |
| **Physics Layer Mask** | Physics layer mask used for limiting poke sphere overlap. |
| **Physics Trigger Interaction** | Determines whether the poke sphere overlap will hit triggers. |
| **Require Poke Filter** | Denotes whether or not valid targets will only include objects with a poke filter. |
| **Debug Visualizations Enabled** | Whether to display the debug visuals for the poke interaction. The visuals include a sphere that changes to green when hover is triggered, and a smaller sphere behind it that turns green when select is triggered. |