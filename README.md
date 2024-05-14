# Welcome to InsightXR Analytics SDK for Unity! 🚀

Welcome to the InsightXR Analytics SDK for Unity! This SDK seamlessly integrates into Unity projects, providing users with a unique VR analytics experience. With just a designated API Key, developers can effortlessly incorporate our SDK into their projects. Gain valuable insights into user experiences within virtual reality with our powerful analytics dashboard.  

## Setting up a VR Project
We have tested the steps throughly with Unity versions 16f1 and 3D URP projects but this shall work with any higher version too.
- Skip this Section is your VR Project is already setup
- Go to the Package manager and install the `XR Plugin Management` and `OpenXR` Packages. It can be done by following below.
- Navigate to *File*>*Build Settings*>*Player Settings*:
  - Go `XR Plugin Management` and press `Install`  
  - Select OpenXR Runtime for both PC and Android platforms
  - It might ask for a Restart. Click Yes.
  - Tick "Initialize XR on Startup" for testing in the editor
  - Add desired VR headset's interaction profile under PC and Android tabs
  - We are adding `Oculus Touch Controller Profile`
  - Go to `Project Validation`. Click on `Fix all` for both PC and Android tabs. Even after clicking, it might be possible that error still appears. Its known issue. Please don't worry about the same.
- Set Compiler to IL2CPP, build mode to arm(64) [Android], and compression to ASTC. You can find this in `Player` tab on left and then `Other Settings`

## Installation

### Step 1: GLTF Importer for Unity

A Package created that allows Users to import and export GLTF models into Unity. Check out [GLTF-Unity]([https://github.com/GlitchEnzo/NuGetForUnity](https://github.com/KhronosGroup/UnityGLTF)) for the git repository.
<ins>Install Package via Git URL :</ins>
```bash
https://github.com/KhronosGroup/UnityGLTF.git/#release/2.11.0-rc
```

### Step 2: Adding the Analytics SDK

Next up is to install the InsightXR Analytics SDK
<ins>Install Package via Git URL :</ins>
```bash
https://github.com/Insight-XR/Unity-sdk.git?path=src/InsightXRForUnity
```
If you have forked it and wish to use the same. Replace Insight-XR with `your-github-id` in above url.  
*If you happen to face ssl disconnect error while installing sdk. You can always download the repo as zip, extract it, then `add project from disk` and select package.json file in `src/InsightXRForUnity` folder*
### Step 3: Import the Extra Assets

As a Versatile XR framework, we shall be installing and using assets from the XR Interaction Toolkit Package. We shall be using their starter assets and VR device Simulator
```bash
  Search Package : XR Interaction Toolkit
        > Select the Package
        > Click on Samples Tab
        > Import Sample Starter Assets
        > Import XR Device Simulator
```
### Step 4: Import Insight XR Samples
*Please ensure to install all samples. Some samples inherit some files from other samples and would work only if all of them are imported*
```bash
  Search Package: InsightXR Unity SDK
        > Select the Package
        > Click on Samples Tab
        > Import all samples 
```
For running project without headset, the installed `XR Device Simulator` will be useful. Once done, it can be activated as follows:

  - Go to `File` > `Build Settings` > `Player settings`
  - Cick on `XR Plug-in Management` > `XR Interaction Toolkit`
  - Checkbox `Use` XR Device Simulator in scenes &#9745;



## Project Setup


### Demo Setup:
- Navigate to "Assets > Samples > InsightXR > `<version>` > Demo Ultimate XR Scene > SampleScene" for an in-depth look at how the SDK is used. Make sure that sample is imported by going to Window > Package manager > Insight XR > Samples > Demo Ultimate XR Scene
- On the Hierarchy, Go to Template Setup > DataHandleLayer. You will find fields such as `[Customer ID, User ID, APIKEY, AWS Access Key, Aws Secret Access Key, Bucket Name]`
- You can get Customer ID and APIKEY from [Dashboard](https://console.getinsightxr.com/). 
- - You can set your User ID yourself. 
- - For getting AWS Keys and Bucket Name, ping akshat@getinsightxr.com. We shall start populating these on Dashboard shortly!
- On the Template Setup (Explained Below), An example is made for you to understand. This SampleScene also uses the template Setup

<ins>Data Handle Layer</ins>
|Value| Description/Use Case|
|---|---|
|Customer ID| The ID provided to you by the Customer Dashboard|
|User ID| A Custom User ID you can set to whatever you want to differentiate and identify sessions|
|API Key| The API provided by the console|
|Replay Bucket URL| Any public Url that contains a direct link to a save file compatible with the scene. Used to WebGL testing. *Can be ignored for now*.|


<ins>Network Uploader</ins>
- `AWS Access Key`, `AWS Secret Access Key`, and `Bucket Name` are to be provided to customers.

**For setting up SDK on your own VR project. Kindly use the setup below. Else jump to Recording and Viewing section to try out demo scene itself.**

### Prefabs:
- Navigate to "Assets > Samples > InsightXR > <version> > Demo Ultimate XR Scene > SampleScene" for an in-depth look at how the SDK is used. Make sure that sample is imported by going to Window > Package manager > Insight XR > Samples > Demo Ultimate XR Scene
- Required prefabs: "Datahandlelayer" and "Replay Camera".
- The third prefab in the Prefabs section is a template setup you can use instead or check as a reference.
- The Template Setup has the following:
  - The Player (XR Origin)
  - The Replay Camera (For Editor and WebGL replay)
  - The DatahandleLayer (For All the settings, references and Keys)
  -  Replay Logic (Example Code using the API to start and stop the session)

### Tracking
- Add the "InsightXRTrackedObject" script component to objects you want to track data for, making sure each tracked object has a `Unique Name`
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
 
### Recording and Viewing
- If You intend to View in Dashboard, then the follow this step, or skip
  - Navigate to `Assets`>`UnityGLTF`.`Export Active Scene as GLTF`
  - Save the Files to a folder outside your unity Project
  - Compress up the files into a `.zip` format, and head to the `Projects` tab in the console to upload the model (The Zip File)
  - Recordings made after the upload will be associated with this Model.

- Make sure the `Replay` tick-mark on the DataHandLayer os off, this indicates that the session is recording
- When a session is does recording, either press the `X Button on your Left Touch Controller`, or the `M Button the the Keyaboard`. This is to stop recording the session. This is purely game logic and part of the `Game Logic` Gameobject in the Template Setup.
- During the Process, the motion data is then packed and saved locally for viewing purposes. If Networking is setup, then the SDK shall upload the Save File to the cloud location for dashboard use. (For online integration, do add your Customer Key and API Key)

- To View the recording in the `Editor`, Click on the replay mode in the DataHandleLayer and use the `<` `>` keys to watch the recording in play mode.
- To View the recording on the Dashboard, just login, locate the user ID you uploaded under, play the 3D rendered captured Motion Data

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
