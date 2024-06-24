# Interactor Events

These are events that can be hooked into in the editor the same way you would respond to a UI button press. These apply to Interactors - objects that can interact with Interactables.

| **Property** | **Description** |
|---|---|
| **Hover Entered** | The event that is called when this Interactor begins hovering over an Interactable.<br />The `HoverEnterEventArgs` passed to each listener is only valid while the event is invoked, do not hold a reference to it. |
| **Hover Exited** | The event that is called when this Interactor ends hovering over an Interactable.<br />The `HoverExitEventArgs` passed to each listener is only valid while the event is invoked, do not hold a reference to it. |
| **Select Entered** | The event that is called when this Interactor begins selecting an Interactable.<br />The `SelectEnterEventArgs` passed to each listener is only valid while the event is invoked, do not hold a reference to it. |
| **Select Exited** | The event that is called when this Interactor ends selecting an Interactable.<br />The `SelectEnterEventArgs` passed to each listener is only valid while the event is invoked, do not hold a reference to it. |