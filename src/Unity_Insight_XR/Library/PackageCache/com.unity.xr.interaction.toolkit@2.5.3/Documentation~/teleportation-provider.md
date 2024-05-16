# Teleportation Provider

The `TeleportationProvider` is responsible for moving the XR Origin to the desired location on the user's request.

![TeleportationProvider component](images/teleportation-provider.png)

| **Property** | **Description** |
|---|---|
| **System** | The [LocomotionSystem](locomotion-system.md) that this `LocomotionProvider` communicates with for exclusive access to an XR Origin. If one is not provided, the behavior will attempt to locate one during its `Awake` call. |
| **Delay Time** | The time (in seconds) to delay the teleportation once it is activated. This delay can be used, for example, as time to set a tunneling vignette effect as a VR comfort option. |