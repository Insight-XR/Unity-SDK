# Welcome to InsightXR Analytics SDK for Unity! 🚀

Welcome to the InsightXR Analytics SDK for Unity! This SDK seamlessly integrates into Unity projects, providing users with a unique VR analytics experience. With just a designated API Key, developers can effortlessly incorporate our SDK into their projects. Gain valuable insights into user experiences within virtual reality with our powerful analytics dashboard.  

## Setting up a VR Project

- Skip this Section is your VR Project is already setup
- Set up XR Plugin Management in Build Settings:
  - Select OpenXR Runtime for both PC and Android platforms
  - Tick "Initialize XR on Startup" for testing in the editor
  - Add desired VR headset's interaction profile under PC and Android tabs
  - We are adding `Oculus Touch Controller Profile`
- Set Compiler to IL2CPP, build mode to arm(64) [Android], and compression to ASTC

## Installation

### Step 1: GLTF Importer for Unity

A Package created that allows Users to import and export GLTF models into Unity. Check out [GLTF-Unity]([https://github.com/GlitchEnzo/NuGetForUnity](https://github.com/KhronosGroup/UnityGLTF)) for the git repository.
```bash
Install Package via Git URL : https://github.com/KhronosGroup/UnityGLTF.git
```

### Step 2: Adding the Analytics SDK

Next up is to install the InsightXR Analytics SDK
```bash
Install Package via Git URL : https://github.com/Insight-XR/Unity-sdk.git?path=src/InsightXRForUnity
```
### Step 3: Import the Extra Assets

For VR sample usage, we are using the XR Interaction Toolkit Package. Once the SDK is installed, head over to the package manager and import the XRIT Sample starter assets
```bash
  Search Package : XR Interaction Toolkit
        > Select the Package
        > Click on Samples Tab
        > Import Sample Starter Assets
```

## Project Setup

### Prefabs:
- Navigate to "Assets > InsightXR > Scenes > InsightSampleScene" for an in-depth look at how the SDK is used.
- Required prefabs: "Datahandlelayer" and "Replay Camera".
- The third prefab in the Prefabs section is a template setup you can use instead or check as a reference.

### Tracking
- Add the "InsightXRTrackedObject" script component to objects you want to track data for
- Data is Tracked using Local Transforms, therefore, make sure the tracking script is added to all related objects

### DataHandleLayer And Networking
- Feel Free to use the Template Setup for Pre-Configured Use or follow the following for manual setup
  
- In the `DataHandleLayer` script, there are many options to configure:
  - In the References Tab, the controller input is already added, so you can ignore that.
  - In the Player Option, drag your VR Player game Object into that reference, so that it can be disabled during mode execution.
  - In the replay Cam Reference, drag and drop the Replay Camera.
  - The Left hand and Right Hand Fields are references to the Animators of the hands of the "VR PLAYER". This SDK uses Oculus Hands as its default hands and tracks the animation data from them using these references.
  - The Replay Mode toggle is used to determine if the game is to be played as a session or to be viewed as a recording. Running the game with the toggle does the respective disabling.
  - Provide your Customer ID, User ID, Replay Bucket URL (For WebGL Replay), and your API Key in the given data.
  - The details in the Network Uploader are Amazon S3 Information.

## Using the SDK
- Utilize the `InsightXRAPI` script on the `DatahandleLayer`:
  ```csharp
  var API = FindObjectOfType<InsightXRAPI>();
  API.RecordSession();
  API.StopSession(bool uploadSaveFile);
  API.StopSession(bool uploadSaveFile, bool CloseApplicationAfterSave);
  API.IsRecording();  // bool
  API.InReplayMode(); // bool
  API.InsightLogEvent(string Event);
- More API function are to come!

## Building
- When Building for `Android`, make sure that the Replay Toggle is false, otherwise the session wont be recorded
- When Building for `WebGL`, ensure the Replay Toggle is On, and a Bucket URL is provided so that the file can be pulled from there.
  - The URL can be any publicly available Replay File, even Github Gists are fine

## Dive In!
You're all set! With InsightXR Analytics SDK, you're not just building games or applications – you're creating data-driven experiences that engage, inform, and impress. We can't wait to see what you build.

## Extra Information
This SDK requires the following packages:
  - **Unity Package Manager**:
    - XR plugin management: Manages XR plugin configuration for different platforms
    - OpenXR: Provides OpenXR support for XR applications
    - XR Interaction toolkit: Facilitates interaction with VR controllers and objects
    - Newtonsoft.Json: Handles JSON Serialization and Deserialization
        - Package Name: `com.unity.nuget.newtonsoft-json`
    - Editor Coroutines: Provides support for coroutines in editor scripts
        - Package Name: `com.unity.editorcoroutines`
  - **Built - In**:
    - Amazon S3: Handles Uploading the Saved Recordings for Sessions
    - Oculus Hands: For Showing Hands and Animations in VR.
  - **External Sources**
    - XR Interaction Toolkit Sample Starter Assets
    - Git Hosted GLTF Importer and Exporter from `The Khronos Group`

Stay tuned for more updates, and happy developing! 🛠️
