---
uid: xri-architecture
---
# Architecture

This section describes the relationship between the core components of the interaction system and the states that make up the lifecycle of an interaction.

<a id="states"></a>
## States

The Interaction system has three common states: Hover, Select, and Activate. These states can mean different things to different objects. Hover and Select are loosely related to the traditional GUI concepts of mouse-over and mouse-down. Activate is specific to XR and is a contextual command.

These interaction states always involve both an [Interactor](#interactors) and [Interactable](#interactables), and both are notified upon entering or exiting the state.

|State|Function|
|---|---|
|**Hover**|If an Interactable is a valid target for the Interactor its state changes to Hover. Hovering on an object signifies an intention to interact with it, but doesn't typically change the behavior of that object, though it might create a visual indicator for this change of state, like how a hovered button changes tint.|
|**Select**|Selection requires an action such as a button or trigger press from the user to enable the Select state. When an Interactable is in the Select state, Unity considers the selecting Interactor to be interacting with it. For example, Selection can simulate picking up a grabbable object, holding a lever, or preparing to push a door that has focus via hovering.|
|**Focus**|An Interactable is focused when it is selected. This focus persists until another Interactable is selected or the Interactable explicitly attempts to select nothing. This state is useful for performing actions on an object. For example - gaining focus of an object and then manipulating its color in a menu.|
|**Activate**|Activation is an extra action, typically mapped to a button or trigger that affects the currently selected object. This lets the user further interact with an object they've selected. The Activate action depends on the Interactable. For example, you can use Activate to toggle a grabbable flashlight on/off or shoot a ball launcher. You can hook the component to process Activate into an action without any additional code by hooking an existing callback using the Inspector window under **Interactable Events** and then add to **Activated** via UnityEvents.|

## Components

### Interactors
Interactor components handle the actions of hovering and selecting Interactable objects in the world. This component is responsible for creating a list of Interactables (called Valid Target) that it could potentially hover or select each frame. The Valid Target list is sorted by priority, and by default the closest Interactables have highest priority. This priority criteria can be changed or extended using [Target filters](target-filters.md).

### Interactables
Interactables are objects in a scene that an Interactor can hover, select, focus, and/or activate. This component is responsible for defining the behavior of those interaction states. The same Interactor might be able to pick up and throw a ball, shoot a gun, or press a 3D button on a keypad.

### Interaction Manager
The Interaction Manager acts as an intermediary between Interactors and Interactables. This component is responsible for actually causing the interaction state changes among its group of registered Interactors and Interactables.

The typical setup is to have a single Interaction Manager, where all Interactables can potentially be affected by all Interactors. You can have multiple complementary Interaction Managers, each with their own set of Interactors and Interactables, and turn them on and off to enable or disable sets of interaction. The collection of loaded scenes needs to have at least one Interaction Manager for interactions to work.

Upon being enabled (during the behavior's `OnEnable`), both Interactors and Interactables register with an Interaction Manager automatically. They will use the first found Interaction Manager if you don't specify one in the Inspector window. Upon being disabled (during the behavior's `OnDisable`), both Interactors and Interactables unregister from the Interaction Manager they are registered with.

For more detail about the exact mechanism the Interaction Manager uses to trigger the state changes, see [Update loop](#update-loop).

<a id="interaction-groups"></a>
### Interaction Groups
Interaction Groups are mediators for Interactors. A Group contains multiple member Interactors, sorted by priority, and only allows one Interactor in the Group to interact (hover or select) at a time. Groups first prioritize continuous selection - so if a member Interactor was interacting the previous frame and can start or continue selection in the current frame, then that Interactor will be chosen for interaction even if a higher priority Interactor tries to interact.

An Interaction Group can also contain other Groups in its sorted list of members. A Group within a Group is treated like a single Interactor when it comes to how the containing Group prioritizes interactions. The Interactor chosen for interaction within a Group is bubbled up to the next containing Group, until it is ultimately either chosen for interaction in the top-level Group or skipped over just like other Interactors.

Interaction Groups also register with an Interaction Manager. A member Interactor or Group must be registered with the same Interaction Manager as its containing Group for the member to function as part of the Group. A Group first registers with an Interaction Manager during the behavior's `Awake`. Upon being disabled (during the behavior's `OnDisable`) the Group will unregister from the Interaction Manager. If the Group is then re-enabled (during the behavior's `OnEnable`) it again registers with an Interaction Manager. If an Interactor or Group is added to a Group at runtime, it will unregister from and then re-registers with its Interaction Manager so that it can be treated as a Group member in the update loop.

### Controllers
The XR Controller component provides a way to abstract input data, which Interactors then use to translate into interaction states, notably for selection and activation. The input actions are bound to a tracked device's controls and are read from each frame, which Interactors then query to determine if they should select or activate.

When there are multiple Interactors associated with a hand, it is typical for there to also be multiple XR Controller components also associated with that same hand. In other words, you do not need to only have a single XR Controller component to represent all interactors for a hand. For example, you may want different inputs to be used for a ray-based teleportation interactor and a close range direct interactor for grabbing, and both Interactor components can reference different XR Controller components to read different input values from the same tracked controller device.

This component is also responsible for reading the position and rotation of the tracked device, and applying it to the Transform component.

#### Controller Recorder
You can also use the Controller Recorder component to record and play back input data. You can use this to to test out interactions or run test scenes without an XR device attached.

> [!Note]
> Controller Recorder currently only works in the Unity Editor.

## Update loop

The update loop of the Interaction Manager queries Interactors and Interactables, and handles the hover, focus and selection states. First, it asks Interactors for a valid list of targets (used for both hover and selection). It then checks both Interactors and Interactables to see if their existing hover, focus and selection objects are still valid. After invalid previous states have been cleared (exited via `OnSelectExiting` and `OnSelectExited`/`OnHoverExiting` and `OnHoverExited`/`OnFocusExiting` and `OnFocusExited`), it queries both objects for valid selection, focus and hover states, and the objects enter a new state via `OnSelectEntering` and `OnSelectEntered`/`OnHoverEntering` and `OnHoverEntered`/`OnFocusEntering` and `OnFocusEntered`.

![interaction-update](images/interaction-update.svg)

All registered Interactables and Interactors are updated before and after interaction state changes by the Interaction Manager explicitly using `PreprocessInteractor`, `ProcessInteractor`, and `ProcessInteractable`. Interactors are always notified before Interactables for both processing and state changes, and Interactors contained within Interaction Groups are always notified before Interactors that are not contained within Groups. Interactables and Interactors are not limited from using the normal `MonoBehaviour` `Update` call, but per-frame logic should typically be done in one of the process methods instead so that Interactors are able to update before Interactables.

### Interaction strength

Interactors and Interactables can report a variable (that is, analog) selection interaction strength, which is a normalized `[0.0, 1.0]` amount of selection that the interactor is performing. For interactors that use motion controller input, this is typically the analog trigger or grip press amount. For interactables that can be poked, it can be based on the depth of a poke interactor.

Interaction strength values are updated after all interaction state changes have occurred by the Interaction Manager explicitly using `ProcessInteractionStrength`. In this case, Interactables are notified before Interactors to allow a poke depth to be computed before gathering the overall interaction strength in the interactor.
