# Canvas Optimizer

Keeps track of canvases in a scene and optimizes them by removing unnecessary components in nested canvases and canvases out of view.

See [UI Setup - Canvas optimizer](ui-setup.md#canvas-optimizer) for more details.

|**Property**|**Description**|
|---|---|
| **Ray Position Ignore Angle** | How wide of an field-of-view to use when determining if a canvas is in view. |
| **Ray Facing Ignore Angle** | How much the camera and canvas rotate away from one another and still be considered facing. |
| **Ray Position Ignore Distance** | How far away a canvas can be from this camera and still receive input. |
