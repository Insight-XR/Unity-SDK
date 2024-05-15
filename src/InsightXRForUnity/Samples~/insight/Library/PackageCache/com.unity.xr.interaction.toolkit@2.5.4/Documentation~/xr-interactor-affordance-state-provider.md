# XR Interactor Affordance State Provider

State Machine component that derives an interaction affordance state from an associated interactor.

| **Property** | **Description** |
|---|---|
| **Transition Duration** | Duration of transition in seconds. 0 means no smoothing. |
| **Interactable Source** | Interactable component for which to provide affordance states. If null, will try and find an interactable component attached. |
| **Ignore Hover Events** | When hover events are registered and this is true, the state will fallback to idle or disabled. |
| **Ignore Select Events** | When select events are registered and this is true, the state will fallback to idle or disabled. |
| **Ignore Activate Events** | When activate events are registered and this is true, the state will fallback to idle or disabled. |
| **Ignore UGUI Hover** | With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas. |
| **Ignore UGUI Select** | With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas and triggering the select input. |
| **Ignore XR Interaction Events** | This option will prevent Hover, Select, and Activate events from being triggered when they come from the XR Interaction Manager. UGUI hover and select events will still come through. |
| **Select Click Animation Mode** | Condition to trigger click animation for Selected interaction events. |
| **Activate Click Animation Mode** | Condition to trigger click animation for activated interaction events. |
| **Click Animation Duration** | Duration of click animations for selected and activated events. |
| **Click Animation Curve** | Animation curve reference for click animation events. Select the More menu (&#8942;) to choose between a direct reference and a reusable scriptable object animation curve datum. |