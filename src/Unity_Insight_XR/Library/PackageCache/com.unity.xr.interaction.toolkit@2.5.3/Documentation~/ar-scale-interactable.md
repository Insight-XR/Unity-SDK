# AR Scale Interactable

Controls the scale of an object via a Pinch gesture. If an object is selected, then doing a pinch/zoom will modify the scale of the object.

![ARScaleInteractable component](images/ar-scale-interactable.png)

| **Property** | **Description** |
|---|---|
| **Interaction Manager** | The [XRInteractionManager](xr-interaction-manager.md) that this Interactable will communicate with (will find one if **None**). |
| **Interaction Layer Mask** | Allows interaction with Interactors whose [Interaction Layer Mask](interaction-layers.md) overlaps with any Layer in this Interaction Layer Mask. |
| **Colliders** | Colliders to use for interaction with this Interactable (if empty, will use any child Colliders). |
| **Custom Reticle** | The reticle that appears at the end of the line when valid. |
| **Select Mode** | Indicates the selection policy of an Interactable. This controls how many Interactors can select this Interactable.<br />The value is only read by the Interaction Manager when a selection attempt is made, so changing this value from **Multiple** to **Single** will not cause selections to be exited. |
| &emsp;Single | Set **Select Mode** to **Single** to prevent additional simultaneous selections from more than one Interactor at a time. |
| &emsp;Multiple | Set **Select Mode** to **Multiple** to allow simultaneous selections on the Interactable from multiple Interactors. |
| **XR Origin** | The `XROrigin` that this Interactable will use (such as to get the `Camera` or to transform from Session space). Will find one if **None**. |
| **AR Session Origin** | This is deprecated. Use the above **XR Origin** instead. |
| **Exclude UI Touches** | Enable to exclude touches that are over UI. Used to make screen space canvas elements block touches from hitting planes behind it. |
| **Min Scale** | The minimum scale of the object. |
| **Max Scale** | The maximum scale of the object. |
| **Elastic Ratio Limit** | The limit of the elastic ratio. |
| **Sensitivity** | Sensitivity to movement being translated into scale. |
| **Elasticity** | Amount that the scale bounces back after hitting min/max of range. |
| **Interactable Events** | See the [Interactable Events](interactable-events.md) page. |