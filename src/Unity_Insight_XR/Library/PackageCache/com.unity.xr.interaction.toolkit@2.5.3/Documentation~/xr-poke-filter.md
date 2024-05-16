# XR Poke Filter

Filter component that allows for basic poke functionality and to define constraints for when the interactable will be selected.

| **Property** | **Description** |
|---|---|
| **Poke Data** ||
| &emsp;**Use Asset** | Enable to reference an externally defined `PokeThresholdData` asset using the accompanying field. |
| &emsp;**Use Value** |  The `PokeThresholdData` object associated with this filter which comes with default values editable in the component editor. |
| &emsp;Poke Direction | The axis along which the poke interaction will be constrained. |
| &emsp;Interaction Depth Offset | Distance along the poke interactable interaction axis that allows for a poke to be triggered sooner/with less precision. |
| &emsp;Enable Poke Angle Threshold | When enabled, the filter will check that a poke action is started and moves within the poke angle threshold along the poke direction axis. |
| &emsp;Poke Angle Threshold | The maximum allowed angle (in degrees) from the poke direction axis that will trigger a select interaction. Only used when Enable Poke Angle Threshold is enabled. |
| **Interactable** | The interactable associated with this poke filter.|
| **Poke Collider** | The collider used to compute bounds of the poke interaction. |