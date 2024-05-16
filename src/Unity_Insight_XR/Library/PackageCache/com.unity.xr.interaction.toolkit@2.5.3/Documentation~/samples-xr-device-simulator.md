# XR Device Simulator

This sample is installed into the default location for package samples, in the `Assets\Samples\XR Interaction Toolkit\[version]\XR Device Simulator` folder. You can move these Assets to a different location.

The XR Interaction Toolkit package provides an example implementation of an XR Device Simulator to allow for manipulating an HMD and a pair of controllers using mouse and keyboard input. This sample contains example bindings for use with that simulator, and a Prefab which you can add to your scene to quickly start using the simulator. Please see the [XR Device Simulator Overview](xr-device-simulator-overview.md) for more information on how to use the device simulator.

![xr-device-simulator-overall](images/xr-device-simulator/xr-device-simulator-overall.gif)

|**Asset**|**Description**|
|---|---|
|**`Hand Expression Captures\`**|Asset folder containing captured hand poses for the simulator when simulating tracked hands.|
|**`Scripts\`**|Asset folder containing scripts for the runtime UI for the simulator.|
|**`UI\`**|Asset folder containing prefabs and textures for the runtime UI for the simulator.|
|**`XR Device Controller Controls.inputactions`**|Asset that contains actions with default bindings for use with the XR Device Simulator focused on controls for the simulated controllers.|
|**`XR Device Hand Controls.inputactions`**|Asset that contains actions with default bindings for use with the XR Device Simulator focused on controls for the simulated hands.|
|**`XR Device Simulator Controls.inputactions`**|Asset that contains actions with default bindings for use with the XR Device Simulator focused on controls for the simulator itself.|
|**`XR Device Simulator.prefab`**|Prefab with the XR Device Simulator component with references to actions configured.|