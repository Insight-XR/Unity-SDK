# InsightXR Analytics SDK for Unity

Welcome to the InsightXR Analytics SDK for Unity! This SDK seamlessly integrates into Unity projects, providing users with a unique VR analytics experience. With just a designated API Key, developers can effortlessly incorporate our SDK into their projects. Gain valuable insights into user experiences within virtual reality with our powerful analytics dashboard.  
[Watch the video walkthrough here](https://www.youtube.com/watch?v=B8F9lAj4jpo)
## Setup the Project for VR

### Install Dependencies
- Install the following packages via Unity Package Manager:
  - XR plugin management
  - OpenXR
  - XR Interaction toolkit
  - Sample Starter Assets for XR Interaction toolkit
    
<img width="956" alt="XR show" src="https://github.com/Insight-XR/Unity-SDK-dev/assets/161463369/c53a878d-5006-4154-aeec-065d2f2c3b91">

- Set up XR Plugin Management in Build Settings:
  - Select OpenXR Runtime for both PC and Android platforms
  - Tick "Initialize XR on Startup" for testing in the editor
  - Add desired VR headset's interaction profile under PC and Android tabs
  - We are adding `Oculus Touch Controller Profile`
- Set Compiler to IL2CPP, build mode to arm(64) [Android], and compression to ASTC

<img width="959" alt="Profiles" src="https://github.com/Insight-XR/Unity-SDK-dev/assets/161463369/9f4f5359-c3a2-4901-b8b1-ba392ed8803d">

### SDK Setup
- This SDK requires the following packages:
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
      
- Import the InsightXR Unity SDK into your project

### Usage
- Add the "InsightXRTrackedObject" script component to objects you want to track data for
- Otherwise, select the InsightXR tab and click Setup Environment

## Project Setup

### Prefabs:
- Navigate to "Assets > InsightXR > Scenes > InsightSampleScene" for an in-depth look at how the SDK is used.
- Required prefabs: "Datahandlelayer" and "Replay Camera".
- The third prefab in the Prefabs section is a template setup you can use instead or check as a reference.

### DataHandleLayer:
- In the `DataHandleLayer` script, there are many options to configure:
  - In the References Tab, the controller input is already added, so you can ignore that.
  - In the Player Option, drag your VR Player game Object into that reference, so that it can be disabled during mode execution.
  - In the replay Cam Reference, drag and drop the Replay Camera.
  - The Left hand and Right Hand Fields are references to the Animators of the hands of the "VR PLAYER". This SDK uses Oculus Hands as its default hands and tracks the animation data from them using these references.
  - Ensure to check the Template Setup to understand these references.
  - The Replay Mode toggle is used to determine if the game is to be played as a session or to be viewed as a recording. Running the game with the toggle does the respective disabling.
  - Provide your Customer ID, User ID, Replay Bucket URL (For WebGL Replay), and your API Key in the given data.
  - The details in the Network Uploader are Amazon S3 Information and are not to be edited.

https://github.com/Insight-XR/Unity-SDK-dev/assets/161463369/3c751a47-7ed2-4060-b5c9-df70730209b7

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
- For Android:
  - Comment out specific lines in the `LoadCamData` script in the `Replay Camera` GameObject before building
  ```csharp
  //DLL Import
  //Extern Function Call
  //Error caused by commenting out the function in the top
- For WebGL:
  - Ensure specific lines in the `LoadCamData` script are not commented out and set replay mode to true before building

## Dive In!
You're all set! With InsightXR Analytics SDK, you're not just building games or applications – you're creating data-driven experiences that engage, inform, and impress. We can't wait to see what you build.

Stay tuned for more updates, and happy developing! 🛠️
