---
uid: xr-core-utils-xr-origin-reference
---
# XR Origin component 

The purpose of the XR Origin is to transform objects and trackable features to their final position, orientation, and scale in the Unity scene.

![XR Origin](images/xr-origin.png "XR Origin")<br />*XROrigin Component properties*

<table>
<tr><td colspan="2" ><strong>Property</strong></td><td><strong>Description</strong></td></tr>
  <tr>
   <td colspan="2" ><strong>Origin Base GameObject</strong></td>
   <td>The GameObject whose Transform serves as the origin for trackables or device-relative elements in an XR scene.</td>
  </tr>
  <tr>
   <td colspan="2" ><strong>Camera Floor Offset GameObject</strong></td>
   <td>GameObject that offsets the [Camera](xref:UnityEngine.Camera) position from the XR Origin. The XR Origin component controls the Y coordinate of the [Transform](xref:UnityEngine.Transform) of this GameObject according to the chosen <strong>Tracking Origin Mode</strong> option:
   <ul>
      <li><strong>Device</strong>: initialized to the value specified by <strong>Camera Y Offset</strong>. Reset when the user resets the view.</li>
      <li><strong>Floor</strong>: initialized to zero.</li>
  </ul></td></tr>
  <tr>
   <td colspan="2" ><strong>Camera</strong></td>
   <td>The Camera for the XR Origin. The GameObject containing this Camera must be underneath the <strong>Origin Base GameObject</strong> in the Scene hierarchy. It should be a child of the <strong>Camera Floor Offset GameObject</strong>. 
   <p>This [Camera](xref:UnityEngine.Camera) is used to render the XR scene.</p>
   </td>
  </tr>
  <tr>
   <td colspan="2" ><strong>Tracking Origin Mode</strong></td>
   <td>Specifies spatial relationship between the XR Origin and the XR device.</td>
  </tr>
  <tr>
   <td rowspan="3"></td>
   <td><strong>Not Specified</strong></td>
   <td>Use the default tracking mode of the device (either <strong>Device</strong> or <strong>Floor</strong>).</td>
  </tr>
  <tr>
   <td><strong>Device</strong></td>
   <td>In this mode, you manually set the height of the user (for VR) or their hand-held device (for AR) with the **Camera Y Offset** value.  
   <p>In this mode, the height is not included in the [Poses](xref:UnityEngine.Pose) returned by [XR Input Subsystem](xref:xrsdk-input). At runtime, you must make any needed adjustments manually, which you can do by changing the [XROrigin.CameraYOffset](xref:Unity.XR.CoreUtils.XROrigin.CameraYOffset) property.</p> 
   </td>
  </tr>
  <tr>
   <td><strong>Floor</strong></td>
   <td>Differs from the <strong>Device</strong> mode by deriving the height based on the "floor" or other surface determined by the XR device.
   <p>In this mode, the height of the user (for VR) or the device (for AR) is included in the [Poses](xref:UnityEngine.Pose) returned by [XR Input Subsystem](xref:xrsdk-input).</p>
   </td>
  </tr>
  <tr>
   <td colspan="2" ><strong>Camera Y Offset</strong></td>
   <td>The distance to offset the [Camera](xref:UnityEngine.Camera) from the XR Origin when the <strong>Device </strong>tracking origin mode is active.
   <p>Only displayed when either <strong>Not Specified</strong> or <strong>Device</strong> is enabled.</p>
   </td>
  </tr>
</table>

The [XROrigin](xref:Unity.XR.CoreUtils.XROrigin) component is designed to work in a specific hierarchy of GameObjects and related components. A typical, recommended setup for an XR Scene includes the following GameObjects and Components:

<table>
<tr><td colspan="3" ><strong>GameObjects</strong></td><td><strong>Components</strong></td></tr>
<tr><td colspan="3" >XR Origin</td><td><ul><li>[XROrigin](xref:Unity.XR.CoreUtils.XROrigin)</li></ul></td></tr>
<tr><td rowspan="4" ></td><td colspan="2" >Camera Offset</td><td><ul><li>None</li></ul></td></tr>
<tr><td rowspan="3" ></td><td>Main Camera</td><td>
<ul>
<li>[Camera](xref:UnityEngine.Camera)</li>
<li>[TrackedPoseDriver](xref:UnityEngine.InputSystem.XR.TrackedPoseDriver)</li>
<li>[ARCameraManager](xref:UnityEngine.XR.ARFoundation.ARCameraManager) (AR)</li>
<li>[ARCameraBackground](xref:UnityEngine.XR.ARFoundation.ARCameraBackground) (AR)</li>
</ul>
</td></tr>
<tr><td>LeftHand Controller</td><td>
<ul>
<li>[XRController](xref:UnityEngine.XR.Interaction.Toolkit.ActionBasedController)</li>
<li>[XRRayInteractor](xref:UnityEngine.XR.Interaction.Toolkit.XRRayInteractor)</li>
<li>[LineRenderer](xref:UnityEngine.LineRenderer)</li>
<li>[XRInteractorLineVisual](xref:UnityEngine.XR.Interaction.Toolkit.XRInteractorLineVisual)</li>
</ul>
</td></tr>
<tr><td>RightHand Controller</td><td>
<ul>
<li>[XRController](xref:UnityEngine.XR.Interaction.Toolkit.ActionBasedController)</li>
<li>[XRRayInteractor](xref:UnityEngine.XR.Interaction.Toolkit.XRRayInteractor)</li>
<li>[LineRenderer](xref:UnityEngine.LineRenderer)</li>
<li>[XRInteractorLineVisual](xref:UnityEngine.XR.Interaction.Toolkit.XRInteractorLineVisual)</li>
</ul>
</td></tr>
</table>

> [!NOTE]
> You can have more than one XR Origin in a scene, but only one should be enabled at any given time. For example, if you need different XR Origin configurations in the same scene, you can add them to the scene and choose the one to enable as needed.

Depending on which XR packages you have added to your project, Unity provides a few menu options that add the recommended XR Origin configurations to a scene. You can add the desired configuration to the scene using the **GameObject** > **XR** menu. The XR Origin configurations include:

* **XR Origin (VR)**: Adds the XR Origin, Camera Offset GameObject, Camera, and left and right controllers to the scene. Use for Virtual Reality scenes. Included with the [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest) package.
* **XR Origin (AR)**: Similar to the VR version, but sets the Camera properties appropriately for Mixed and Augmented Reality and adds related AR Camera components. Included with the [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest) package (only shown when the XR Interaction Toolkit package is also installed).
* **XR Origin (Mobile AR)**: The same as the AR version, but does not include GameObjects for the controllers. Included with the [AR Foundation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@latest) package.
* **Device based** > **XR Origin (VR)**: Adds the XR Origin, Camera Offset GameObject, Camera, and left and right controllers to the scene. The controllers in this configuration use device-based input components that map app behaviors directly to the controller buttons and joysticks. Use for Virtual Reality scenes when using the legacy Input Manager. Included with the [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest) package (not all Interaction Toolkit features are supported when using device-based input).

> [!TIP]
> You can simply delete the controller GameObjects from the VR or AR configurations if your app doesn't need controllers.

> [!IMPORTANT]
> Additional setup is required to configure the left- and right-hand controller objects to process tracking data and user input. See [General setup](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest?subfolder=/manual/general-setup.html) in the [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@latest) documentation for more information.
