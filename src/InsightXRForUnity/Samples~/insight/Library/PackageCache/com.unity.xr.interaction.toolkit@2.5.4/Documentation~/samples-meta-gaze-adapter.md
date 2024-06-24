# Meta Gaze Adapter

This sample is installed into the default location for package samples, in the `Assets\Samples\XR Interaction Toolkit\[version]\Meta Gaze Adapter` folder. It provides a script to assist with eye tracking with the Meta Quest Pro and the [XR Gaze Interactor](xr-gaze-interactor.md).
Currently the `XRGazeInteractor` uses the [OpenXR bindings](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest/index.html?subfolder=/manual/features/eyegazeinteraction.html) for eye gaze position, tracking, and rotation. This binding is not currently supported for Quest Pro. When the OpenXR bindings are supported, this sample and adapter will be unnecessary.

|**Asset**| **Description**|
|---|---|
|**`OculusEyeGazeInputAdapter`**|Script which creates an Eye Gaze input device and updates the device's state based on data from the Oculus `OVRPlugin` API. This script can be placed in a scene with an [XR Gaze Interactor](xr-gaze-interactor.md) to enable the gaze interactor to work with the Meta Quest Pro eye tracking.|
|**`GazeAdapterSampleProjectValidation`**|Unity Editor script which adds Project Validation rules for the sample. Not necessary for the actual adapter.|

## Requirements

The [Oculus XR Plugin](https://docs.unity3d.com/Manual/com.unity.xr.oculus.html) package version `3.2.2` or newer is required for Meta Quest Pro support, and thus this sample requires at least the Unity Editor 2021.3.4f1 or newer.

Additionally, this sample requires the `OVRPlugin` found in the `VR` folder of the [Oculus Integration asset](https://developer.oculus.com/downloads/package/unity-integration/), 
`v47.0` or newer, and the [OpenXR Plugin](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest/) package version `1.6.0` or newer. The OpenXR `EyeGazeDevice` is used to update the pose of the `Gaze Interactor`.

## Setup

1. Download `v47.0` or newer of the [Oculus Integration asset](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022)
2. Import the Oculus Integration asset by using **Window** &gt; **Package Manager** &gt; **Packages: My Assets** &gt; **Oculus Integration** &gt; **Import** or by using **Assets** &gt; **Import Package** &gt; **Custom Package** to import the downloaded `.unitypackage` file. At a minimum, select the following assets to import:
    - `Oculus\OculusProjectConfig.asset`
    - `Oculus\VR\`
3. Click **Yes** to accept the Update Oculus Utilities Plugin prompt to update OVRPlugin
4. Click **Use OpenXR** to accept the OpenXR Backend prompt. If you canceled, enable it later using **Oculus** &gt; **Tools** &gt; **OVR Utilities Plugin** &gt; **Set OVRPlugin to OpenXR**.
5. Import Meta Gaze Adapter sample from **Window** &gt; **Package Manager** &gt; **XR Interaction Toolkit**
6. Install or update [OpenXR Plugin (com.unity.xr.openxr)](https://docs.unity3d.com/Manual/com.unity.xr.openxr.html) to version `1.6.0` or newer and install or update [Oculus XR Plugin (com.unity.xr.oculus)](https://docs.unity3d.com/Manual/com.unity.xr.oculus.html) to version `3.2.2` or newer by clicking **Fix** in **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management** &gt; **Project Validation** or by using **Window** &gt; **Package Manager**
7. Add **`USE_INPUT_SYSTEM_POSE_CONTROL`** to Scripting Defines Symbols (**Edit** &gt; **Project Settings** &gt; **Player** &gt; **Other Settings** &gt; **Scripting Compilation** &gt; **Scripting Defines Symbols**) and click **Apply**. Do this for both the PC and Android tabs in the Player settings. This resolves the error "ArgumentException: Expected control 'pose' to be of type 'PoseControl' but is of type 'PoseControl' instead!"
8. Enable **Oculus** plug-in provider in **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management**. Do this for both the PC and Android tabs in the XR Plug-in Management settings.
9. Add the Oculus Eye Gaze Input Adapter component to a GameObject in the scene
10. (Android platform only) Enable **Eye Tracking Support** in the Inspector window of the `Assets\Oculus\OculusProjectConfig` asset under section **Quest Features** &gt; **General** by setting it to **Supported** or **Required**

> [!TIP]
> To enable eye tracking support while using Oculus Link, open the Oculus desktop application and go to **Settings** &gt; **Beta**. Click to enable **Developer Runtime Features** and **Eye tracking over Oculus Link**. Then click **Restart Oculus**, then unplug and plug in the USB cable. Finally, restart the Unity Editor.

### Optional

- Enable **Quest Pro** under Target Devices in the Oculus settings Android tab in **Edit** &gt; **Project Settings** &gt; **XR Plug-in Management** &gt; **Oculus**
- Generate custom Android manifest file (**Oculus** &gt; **Tools** &gt; **Create store-compatible AndroidManifest.xml** to update `Assets\Plugins\Android\AndroidManifest.xml`, which also automatically updates **Edit** &gt; **Project Settings** &gt; **Player** &gt; **Android tab** &gt; **Publishing Settings** to enable **Custom Main Manifest**