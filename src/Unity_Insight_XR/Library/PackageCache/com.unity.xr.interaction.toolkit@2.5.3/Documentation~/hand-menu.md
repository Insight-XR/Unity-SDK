# Hand Menu

Makes a GameObject follow a tracked hand or motion controller with logic for setting visibility of the menu based on the palm orientation. This can be used, for example, to show a preferences menu when the user is looking at their palm.

|**Property**|**Description**|
|---|---|
|**Hand Menu UI GameObject** | Child GameObject used to hold the hand menu UI. This is the transform that moves each frame.|
|**Menu Handedness** | Which hand should the menu anchor to.<br/><ul><li>**None** to make the menu not follow either hand. Effectively disables the hand menu.</li><li>**Left** to make the menu follow the left hand.</li><li>**Right** to make the menu follow the right hand.</li><li>**Either** to make the menu follow either hand, choosing the first hand that satisfies requirements.</li></ul>|
|**Hand Menu Up Direction**|Determines the up direction of the menu when the hand menu is looking at the camera.<br/><ul><li>**World Up** to use the global world up direction (`Vector3.up`).</li><li>**Transform Up** to use this GameObject's world up direction (`Transform.up`). Useful if this component is on a child GameObject of the XR Origin and the user can teleport to walls.</li><li>**Camera Up** to use the main camera up direction. The menu will stay oriented with the head when the user tilts their head left or right.</li></ul>|
|**Snap To Gaze Palm Alignment Degree Angle Threshold** | Degree angle threshold for hand menu to snap to align with gaze.|
|**Palm Facing User Degree Angle Threshold** | Degree angle separation between camera forward and palm up to consider whether the palm is facing the user.|
|**Left Palm Anchor** | Anchor associated with the left palm pose for the hand.|
|**Left Offset Child Anchor** | Offset from the left palm anchor where the UI should sit.|
|**Right Palm Anchor** | Anchor associated with the right palm pose for the hand.|
|**Right Offset Child Anchor** | Offset from the right palm anchor where the UI should sit.|
|**Follow Speed Multiplier** | Multiplier for delta time used when computing position and rotation tweens for this hand menu.|
|**Min Follow Distance** | Minimum distance in meters from target before which tween starts.|
|**Max Follow Distance** | Maximum distance in meters from target before tween targets, when time threshold is reached.|
|**Min To Max Delay Seconds** | Time required to elapse before the max distance allowed goes from the min distance to the max.|
