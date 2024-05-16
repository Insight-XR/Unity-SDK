# AR Starter Assets

This sample is installed into the default location for package samples, in the `Assets\Samples\XR Interaction Toolkit\[version]\AR Starter Assets` folder. You can move these Assets to a different location after importing it.

The XR Interaction Toolkit package provides an example implementation for mobile AR development. This includes the a demo scene and some prefabs to get you started with your own projects.

|**Asset**|**Description**|
|---|---|
|**`ARDemoSceneAssets\`**|Asset folder containing assets only used for the `ARDemoScene`.|
|**`Editor\Scripts\`**|Asset folder containing Unity Editor scripts which adds Project Validation rules for the sample.|
|**`Materials\FeatheredPlaneMaterial`**|This material provides a visual representation used for AR plane detection in the XR Simulator and in builds.|
|**`Prefabs\AR Feathered Plane`**|This prefab uses the `FeatheredPlaneMaterial` to show where the AR plane is detected or simulated.|
|**`Prefabs\XR Origin (AR Rig)`**|This contains the basic configuration to use XRI with an mobile device, leveraging the XR Screen Space Controller prefab and the AR settings on the XR Ray Interactor.|
|**`Prefabs\XR Screen Space Controller`**|This controller prefab contains the screen-space controller and XR Ray Interactor setup to work with mobile AR devices.|
|**`Scripts\ARContactSpawnTrigger`**|Component that spawns an object from the designated list at the physical contact point with an AR plane.|
|**`Scripts\ARFeatheredPlaneMeshVisualizer`**|Component that supports a feathering effect at the edge of a detected plane, which reduces the visual impression of a hard edge.|
|**`Scripts\ARInteractorSpawnTrigger`**|Component that spawns an object from the designated list at the point where the `ARInteractor` hits the AR plane when the specific `spawnAction` is triggered from the Input System.|
|**`ARDemoScene`**|Scene that illustrates a basic AR rig setup that supports screen space touch gestures along with example interactable objects that can be spawned and UI.|
