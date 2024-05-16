# Continuous Move Provider (Device-based)

Locomotion provider that allows the user to smoothly move their rig continuously over time using a specified 2D axis input.

![DeviceBasedContinuousMoveProvider component](images/continuous-move-provider-device-based.png)

| **Property** | **Description** |
|---|---|
| **System** | The [LocomotionSystem](locomotion-system.md) that this `LocomotionProvider` communicates with for exclusive access to an XR Origin. If one is not provided, the behavior will attempt to locate one during its Awake call. |
| **Move Speed** | The speed, in units per second, to move forward. |
| **Enable Strafe** | Controls whether to enable strafing (sideways movement). |
| **Enable Fly** | Controls whether to enable flying (unconstrained movement). This overrides **Use Gravity**. |
| **Use Gravity** | Controls whether gravity affects this provider when a `CharacterController` is used. This only applies when **Enable Fly** is disabled. |
| **Gravity Application Mode** | Controls when gravity begins to take effect. |
| &emsp;Attempting Move | Use this style when you don't want gravity to apply when the player physically walks away and off a ground surface. Gravity will only begin to move the player back down to the ground when they try to use input to move. |
| &emsp;Immediately | Apply gravity and apply locomotion every frame, even without move input. Use this style when you want gravity to apply when the player physically walks away and off a ground surface, even when there is no input to move. |
| **Forward Source** | The source `Transform` that defines the forward direction. |
| **Input Binding** | The 2D Input Axis on the controller devices that will be used to trigger a move. |
| &emsp;Primary 2D Axis | Use the primary touchpad or joystick on a device. |
| &emsp;Secondary 2D Axis | Use the secondary touchpad or joystick on a device. |
| **Controllers** | A list of controllers that allow movement.  If an XRController is not enabled, or does not have input actions enabled, movement will not work. |
| **Deadzone Min** | Value below which input values will be clamped. After clamping, values will be renormalized to [0, 1] between min and max. |
| **Deadzone Max** | Value above which input values will be clamped. After clamping, values will be renormalized to [0, 1] between min and max. |