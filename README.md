# InsightXR Analytics SDK for Unity

Welcome to the InsightXR Analytics SDK for Unity! This SDK seamlessly integrates into Unity projects, providing users with a unique VR analytics experience. With just a designated API Key, developers can effortlessly incorporate our SDK into their projects. Gain valuable insights into user experiences within virtual reality with our powerful analytics dashboard.

## Setting up a VR Project

We have thoroughly tested these steps with Unity versions 2021.3.26f1 and higher. Follow the instructions to set up your VR project.

## Installation

### Step 1: Install the InsightXR Analytics SDK

1. **Install the Package from GitHub:**
   - Install the package via Git URL:
     ```
     https://github.com/Insight-XR/Unity-sdk.git?path=src/InsightXRForUnity
     ```
   - If you encounter SSL disconnect errors while installing the SDK, you can download the repository as a zip, extract it, then add the project from disk and select the `package.json` file in the `src/InsightXRForUnity` folder.

2. **Install Oculus Integration (Deprecated) or Meta XR Interaction SDK:**
   - If your Unity version is under 2021.3.26f1, install the Oculus Integration package:
     [Oculus Integration](https://assetstore.unity.com/packages/tools/integration/oculus-integration-deprecated-82022)
   - For Unity 2021.3.26f1 and above, install the Meta XR Interaction SDK from the Asset Store:
     [Meta XR Interaction SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-interaction-sdk-265014)

3. **Configure Project Settings:**
   - Go to `File > Build Settings > Player Settings > InsightXR`.
   - There are three fields: Customer ID, User ID, and API Key.
     - Customer ID and API Key can be obtained from the dashboard.
     - Ensure a unique User ID.
   - Click Save.
4. - Go to `File > Build Settings > Player Settings > Player`.
   - `Allow downloads over HTTP should` be changed to `always allowed`.
### Step 2: Adding the Analytics SDK

1. **Add the InsightTrackingManager Prefab:**
   - Search for the `InsightTrackingManager` prefab from the InsightXRReplayTool package and add it to your scene or scenes.

2. **Configure Tracking Manager:**
   - Add the `InsightSettingsSO` in the `Insight Settings` field in the `TrackingManager` script present in the `InsightTrackingManager`.
   - Assign the `InsightTrackCenterEye` and `InsightTrackObject` to the camera GameObject.
   - Assign the `InsightTrackHandAnchor` script and `InsightTrackObject` to the left and right hand controllers.Make sure you assign which hand is right or left. If you are using AutoHands, tick the AutoHands field.
   - For `AutoHands`, you have to add 3 lines in the `Finger script` available in the `AutoHands folder`.
   - The highlighted lines should be added to the script.
   ![image](https://github.com/user-attachments/assets/cf8d065d-c65a-4c2a-b5a4-abeac9ce3e3d)


   - For any other objects you want to track, assign the `InsightTrackObject` to them.

3. **Register Tracked Objects:**
   - In the Unity editor, click on `InsightXR > Manage Tracked Objects`.
   - Click on `Register Tracked Objects in Open Scene` and `Register Tracked Objects in Assets`.

4. **Export Static Objects:**
   - Click on `Select Static Objects`.
   - In your hierarchy, there will be a GameObject with the scene name. Right-click on it and select `Export to FBX`.
   - Set the path to `InsightDeskCache/TrackedPrefabFBX/Models`. Ensure the export format is binary and embed textures are ticked. Click Export.
   - `Delete` the GameObject (the one you exported to fbx) from the hierarchy.

5. **Register Skybox: (if skybox present)** 
   - If you have a skybox or multiple skyboxes, ensure it is a 6-sided skybox.
   - Drag the material to the “Drag and Drop a 6-sided Skybox Material” field and click on `Register Skybox` for all your skyboxes.

6. **Upload FBX Models:**
   - Before upload ensure that the `InsightTrackingManager` is present in the hierarchy and has the `InsightSettingsSO` assigned. 
   - Click on `Upload FBX Models` under `InsightXR > Manage Tracked Objects` in the editor. This creates a zip folder of all the models and uploads it to the dashboard server.

## Recording and Viewing Sessions

1. **Start the Session:**
   - After models are successfully uploaded, you can start the session. The session starts recording, and when you stop the game, it will stop recording.

2. **Local Replay:**
   - To play in local replay, copy the session ID from the sessions folder in assets or from the console.
   - Search for the replay scene or find it in `Assets/Samples/InsightXRReplayTool/ReplayScene`.
   - Open the replay scene and click on `InsightReplayManager` in the hierarchy.
   - Enter the session ID in the session ID field in the inspector.
   - Run the scene to play the local replay of your session.

3. **Dashboard Replay:**
   - The same session with the same session ID will be present in the dashboard.

## Key Features

- **Handles Failure Cases:**
  - If the network gets disconnected, the session is saved locally and then sent to the dashboard once the network is retrieved. It will try to send the failed data every minute periodically. If it fails to send it in the same session, it will send it in the next session.
- **Supports:**
  - Multiple instantiation
  - Late instantiation
  - Hand animation
  - Text tracking
  - Multiple scenes

## Sample Scenes

- **Import Samples:**
  - You can import samples from our scenes and test them to know more about the SDK.

For more information, refer to our documentation and stay tuned for updates. Happy developing!
