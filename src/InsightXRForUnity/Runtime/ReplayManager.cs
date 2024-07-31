using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
// using InsightDesk;

#if !INSIGHT_NO_XR
using UnityEngine.XR.Management;
#endif

namespace InsightDesk
{
    public class ReplayManager : MonoBehaviour
    {
        public bool IsNetworkFeatureAvailable;
        public TextAsset session_file;
        public string sessionID;
        public GameObject cameraGameObject;
        public GameObject leftControllerGameObject;
        public GameObject rightControllerGameObject;
        public Slider slider;
        public Button togglePauseButton;
        public Sprite playSprite;
        public Sprite pauseSprite;
        public Text progressText;
        public SliderManager sliderManager;
        public GameObject missingTexturePrefab;
        private bool _useAssetBundle = false;
        private string _orgId;
        private string _project_id;
        private string _appVersion = "";
        private int _tickRate = 0;
        private readonly Dictionary<uint, GameObject> _objectInstances = new Dictionary<uint, GameObject>();
        private List<(TickEntry tickEntry, Dictionary<uint, ObjectEntry> objectEntries)> _replay = new();
        private bool _isPaused = true;
        private float _replayTime = 0;
        private float _lastProgressLogTime = 0;
        private AssetBundle _assetBundle;
        private bool _readyToStartPlaying = false;
        private string _accessToken;
        private string _apiQueryBase = TrackingManager.ApiQueryBase;
        private GameObject currentFBX;
        private GameObject currentOBJ;
        private GameObject previousFBX;
        private string currentSkyboxName = "";
        private bool firstskybox = true;

        private void Start()
        {
            Debug.Log("Start Read File For Memory Stream");

#if UNITY_WEBGL && !UNITY_EDITOR
            _useAssetBundle = true;
#else
            _useAssetBundle = false;
#endif

            sessionID = sessionID.Trim();

#if UNITY_EDITOR
            if (sessionID != "a" && !_useAssetBundle)
            {
                _readyToStartPlaying = true;
                TogglePause();
                DownloadAndLoadReplay(true);
            }
            
#endif
        }




        private void Update()
        {
#if !INSIGHT_NO_XR
            var xrSettings = XRGeneralSettings.Instance;
            if (xrSettings != null && xrSettings.Manager != null)
            {
                if (xrSettings.Manager.isInitializationComplete)
                {
                    xrSettings.Manager.StopSubsystems();
                    xrSettings.Manager.DeinitializeLoader();
                    Debug.Log("Stopping XR for replay scene.");
                }
            }
            else
            {
                // Debug.LogWarning("XRGeneralSettings or XR Manager is not initialized.");
            }
#endif

            if (!_readyToStartPlaying)
            {
                return;
            }

            if (_replay.Count < 2)
            {
                return;
            }

            if (!_isPaused && !sliderManager.pointerDown)
            {
                slider.value = Mathf.Clamp(_replayTime + Time.deltaTime, 0, TotalReplayTime()) / TotalReplayTime();
            }

            int replayIndex = 0;
            var startTime = ReplayStartTime();
            for (int i = 0; i < _replay.Count; i++)
            {
                var entryTime = _replay[i].tickEntry.unscaledTime - startTime;
                replayIndex = Mathf.Clamp(i - 1, 0, _replay.Count - 2);
                if (entryTime >= _replayTime)
                {
                    break;
                }
            }

            var (tickEntry1, objectEntries1) = _replay[replayIndex];
            var (tickEntry2, objectEntries2) = _replay[replayIndex + 1];

            var objectIdsInThisTick = new HashSet<uint>();

            foreach (var entry in objectEntries2.Values)
            {
                if (!objectEntries1.TryGetValue(entry.instanceId, out var lastEntry))
                {
                    continue;
                }

                objectIdsInThisTick.Add(entry.instanceId);
                var objectInstance = GetOrCreateObjectInstance(entry.instanceId, entry.prefabId, entry.parentPrefabId, tickEntry1.sceneName, tickEntry1.skyboxName);

                if (objectInstance == null)
                {
                    continue;
                }

                objectInstance.SetActive(entry.activeInHierarchy && lastEntry.activeInHierarchy);

                var animator = objectInstance.GetComponent<Animator>();

                var tickLength = tickEntry2.unscaledTime - tickEntry1.unscaledTime;
                var lerpTime = (_replayTime + ReplayStartTime() - tickEntry1.unscaledTime) / tickLength;

                if (tickLength > (1f / _tickRate) + (1f / 5))
                {
                    lerpTime = 0;
                }

                objectInstance.transform.position = Vector3.Lerp(lastEntry.position, entry.position, lerpTime);
                objectInstance.transform.rotation = Quaternion.Slerp(lastEntry.rotation, entry.rotation, lerpTime);
                objectInstance.transform.localScale = Vector3.Lerp(lastEntry.localScale, entry.localScale, lerpTime);
                // Debug.Log("");
                for (int i = 0; i < entry.newAnimationFloats.Count; i++)
                {
                    var parameter = entry.newAnimationFloats[i];
                    float lastParameterValue;
                    try
                    {
                        lastParameterValue = lastEntry.newAnimationFloats.First(lastEntry => lastEntry.Item1 == parameter.Item1).Item2;
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.LogWarning("Last tick missing animation parameters. Was a parameter added at runtime during the recording?");
                        lastParameterValue = parameter.Item2;
                    }

                    var value = Mathf.Lerp(lastParameterValue, parameter.Item2, lerpTime);
                    if (animator)
                    {
                        animator.SetFloat(parameter.Item1, value);
                    }
                    else
                    {
                        LogError("Missing animator on prefab in cache.");
                    }
                }

                for (int i = 0; i < entry.newAnimationInts.Count; i++)
                {
                    var parameter = entry.newAnimationInts[i];
                    if (animator)
                    {
                        animator.SetInteger(parameter.Item1, parameter.Item2);
                    }
                    else
                    {
                        LogError("Missing animator on prefab in cache.");
                    }
                }

                for (int i = 0; i < entry.newAnimationBools.Count; i++)
                {
                    var parameter = entry.newAnimationBools[i];
                    if (animator)
                    {
                        animator.SetBool(parameter.Item1, parameter.Item2);
                    }
                    else
                    {
                        LogError("Missing animator on prefab in cache.");
                    }
                }
                for (int i = 0; i < entry.newAnimationTriggers.Count; i++)
                {
                    var parameter = entry.newAnimationTriggers[i];
                    if (animator)
                    {
                        animator.SetTrigger(parameter);
                    }
                    else
                    {
                        LogError("Missing animator on prefab in cache.");
                    }
                }





                for (int i = 0; i < entry.newTexts.Count; i++)
                {
                    var textEntry = entry.newTexts[i];
                    TextMeshPro tmp3D = objectInstance.GetComponent<TextMeshPro>();
                    TextMeshProUGUI tmpUI = objectInstance.GetComponent<TextMeshProUGUI>();

                    if (tmp3D != null)
                    {
                        tmp3D.text = textEntry.Item2;
                    }
                    else if (tmpUI != null)
                    {
                        tmpUI.text = textEntry.Item2;
                    }
                    else
                    {
                        LogError("Missing TextMeshPro component on prefab in cache.");
                    }
                }

                // Apply left hand bend offsets
                foreach (var bendOffset in entry.newLeftHandBendOffsets)
                {
                    ApplyBendOffset(objectInstance, bendOffset.Item1, bendOffset.Item2, true);
                }

                // Apply right hand bend offsets
                foreach (var bendOffset in entry.newRightHandBendOffsets)
                {
                    ApplyBendOffset(objectInstance, bendOffset.Item1, bendOffset.Item2, false);
                }
            }

            var idsToDelete = new Queue<uint>();
            foreach (var gameObjectInstanceId in _objectInstances.Keys)
            {
                if (gameObjectInstanceId == 0)
                {
                    Debug.LogWarning($"Instance ID 0 detected in _objectInstances: {gameObjectInstanceId}");
                    continue;
                }
                if (!objectIdsInThisTick.Contains(gameObjectInstanceId))
                {
                    var objectToCheck = _objectInstances[gameObjectInstanceId];
                    Debug.Log($"Queueing instance {gameObjectInstanceId} for removal.");
                    idsToDelete.Enqueue(gameObjectInstanceId);
                }
            }

            while (idsToDelete.Count > 0)
            {
                var id = idsToDelete.Dequeue();
                if (_objectInstances.ContainsKey(id) && _objectInstances[id] != null)
                {
                    Debug.Log($"Removing instance {id} from _objectInstances and destroying the GameObject.");
                    Destroy(_objectInstances[id]);
                    _objectInstances.Remove(id);
                }
            }
        }

        public void ApplySkybox(string skyboxName)
        {
            // Define the path to the skybox material
            string materialPath = Path.Combine("Assets", "InsightDeskCache", "SkyboxMaterials", skyboxName + ".mat");

            // Load the material
            Material skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (skyboxMaterial == null)
            {
                Debug.LogWarning($"Skybox material '{materialPath}' not found.");
                return;
            }

            // Apply the skybox material
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();

            // Debug.Log($"Skybox '{skyboxName}' has been applied.");
        }

        private void ApplyBendOffset(GameObject objectInstance, string boneName, float bendOffset, bool isLeftHand)
        {
            // Find the game object with the specified tag
            string tagName = isLeftHand ? "Left_AutoHand" : "Right_AutoHand";
            GameObject handObject = GameObject.FindWithTag(tagName);

            if (handObject == null)
            {
                Debug.LogWarning($"Hand object with tag '{tagName}' not found.");
                return;
            }

            // Find the bone with the specified name within the hand object using the recursive method
            Transform boneTransform = FindChildRecursive(handObject.transform, boneName);
            if (boneTransform == null)
            {
                Debug.LogWarning($"Bone '{boneName}' not found on hand object '{handObject.name}'");
                return;
            }

            // Get the IBendable component and apply the bend offset
            IBendable bendableComponent = boneTransform.GetComponent<IBendable>();
            if (bendableComponent != null)
            {
                bendableComponent.BendOffset = bendOffset;
                // Assuming there is a method to update the bend in IBendable
                (bendableComponent as MonoBehaviour).SendMessage("UpdateFinger");
            }
            else
            {
                Debug.LogWarning($"IBendable component not found on bone '{boneName}' of hand object '{handObject.name}'");
            }
        }

        // Recursive method to find a child transform by name
        private Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform found = FindChildRecursive(child, childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }


#if UNITY_EDITOR
private GameObject LoadFBXModel(string directoryPath, string sceneName)
{
    // Check if the directory exists
    if (!Directory.Exists(directoryPath))
    {
        Debug.LogError($"Directory not found at path: {directoryPath}");
        return null;
    }

    // Search for the FBX file in the directory
    var fbxFilePath = Path.Combine(directoryPath, $"{sceneName}.fbx");
    if (!File.Exists(fbxFilePath))
    {
        Debug.LogWarning($"No FBX file found for scene '{sceneName}' in the specified directory.");
        return null;
    }

    // Debug.Log($"Loading FBX file: {fbxFilePath}");

    GameObject fbxAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fbxFilePath);
    if (fbxAsset == null)
    {
        Debug.LogError("Failed to load FBX asset.");
        return null;
    }

    // Destroy the previous FBX model if it exists and the scene name has changed
    if (previousFBX != null && previousFBX.name != sceneName)
    {
        Destroy(previousFBX);
        previousFBX = null;
    }

    GameObject fbxInstance = Instantiate(fbxAsset);
    if (fbxInstance == null)
    {
        Debug.LogError("Failed to instantiate FBX asset.");
        return null;
    }

    fbxInstance.name = sceneName; // Set the name to the scene name for comparison
    Debug.Log("FBX model loaded and instantiated successfully.");
    previousFBX = fbxInstance; // Store the current FBX model

    return fbxInstance;
}
#endif



#if UNITY_EDITOR
private GameObject LoadOBJModel(string directoryPath, string sceneName)
{
    // Check if the directory exists
    if (!Directory.Exists(directoryPath))
    {
        Debug.LogError($"Directory not found at path: {directoryPath}");
        return null;
    }

    // Search for any OBJ file in the directory
    var objFiles = Directory.GetFiles(directoryPath, $"{sceneName}.obj");
    if (objFiles.Length == 0)
    {
        Debug.LogError("No OBJ files found in the specified directory.");
        return null;
    }

            _ = Directory.GetFiles(directoryPath, $"{sceneName}.obj");
            if (objFiles.Length == 0)
            {
                Debug.LogWarning("No OBJ files found in the specified directory.");
                return null;
            }

            var objFilePath = objFiles[0];
            // Debug.Log($"Loading OBJ file: {objFilePath}");

            var objAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(objFilePath);
            if (objAsset == null)
            {
                Debug.LogError("Failed to load OBJ asset.");
                return null;
            }

            var objInstance = Instantiate(objAsset);
            if (objInstance == null)
            {
                Debug.LogError("Failed to instantiate OBJ asset.");
                return null;
            }

            // Debug.Log("OBJ model loaded and instantiated successfully.");
            return objInstance;
        }
#endif


        private IEnumerator GetAssetBundle()
        {
            if (_orgId == "" || _project_id == "")
            {
                LogError("org or project id is empty.");
                yield break;
            }

            Debug.Log($"Getting bundle hash for version {_appVersion}");
#if UNITY_WEBGL && !UNITY_EDITOR
                InsightBundleDownloadProgress(0, _appVersion);
#endif
            var bundleHash = "";
            using (UnityWebRequest uwr =
                   UnityWebRequest.Get(
                       $"{_apiQueryBase}/{_orgId}/{_project_id}/get_asset_bundle_hash?app_version={_appVersion}"))
            {
                uwr.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
                yield return uwr.SendWebRequest();

                if (uwr.responseCode == 200)
                {
                    bundleHash = uwr.downloadHandler.text;
                }
                else
                {
                    InsightUtility.LogError("Could not get bundle hash.");
#if UNITY_WEBGL && !UNITY_EDITOR
                    InsightCouldNotGetBundle();
#endif
                    yield break;
                }
            }

            Debug.Log($"Got bundle hash: {bundleHash}");

            var bundleUrl =
                $"{_apiQueryBase}/{_orgId}/{_project_id}/get_asset_bundle?bundle_hash={bundleHash}";
            var bundleHash128 = Hash128.Parse(bundleHash);
            var bundleIsCached = Caching.IsVersionCached(bundleUrl, bundleHash128);
            if (bundleIsCached)
            {
                Debug.Log("Bundle was in cache. No need to download.");
            }
            else
            {
                Debug.Log("Downloading bundle...");
            }

            using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl, bundleHash128))
            {
                uwr.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
                var operation = uwr.SendWebRequest();

                if (!bundleIsCached)
                {
                    Debug.Log("Waiting for server to start sending replay...");
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                InsightBundleDownloadProgress(0, _appVersion);
#endif

                var downloadStarted = false;
                while (!operation.isDone)
                {
                    if (uwr.downloadProgress > 0 && uwr.downloadProgress < 1)
                    {
                        if (!downloadStarted)
                        {
                            downloadStarted = true;
                            Debug.Log("Starting download...");
                        }

#if UNITY_WEBGL && !UNITY_EDITOR
                        InsightBundleDownloadProgress(uwr.downloadProgress, _appVersion);
#endif

                        if (Time.unscaledTime - _lastProgressLogTime > 0.5f)
                        {
                            LogProgress(uwr.downloadProgress);
                        }
                    }

                    yield return null;
                }

                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(uwr.error);
#if UNITY_WEBGL && !UNITY_EDITOR
                    InsightCouldNotGetBundle();
#endif
                }
                else
                {
                    // Get downloaded (or cached) asset bundle
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    if (bundle == null)
                    {
                        InsightUtility.LogError("Failed to load AssetBundle!");
#if UNITY_WEBGL && !UNITY_EDITOR
                        InsightCouldNotGetBundle();
#endif
                        yield break;
                    }

#if UNITY_WEBGL && !UNITY_EDITOR
                    InsightBundleDownloadProgress(1, _appVersion);
#endif

                    Debug.Log("Finished getting bundle");
                    _assetBundle = bundle;
                }
            }
        }

        struct SetSessionMsg
        {
            public string accessToken;
            public string apiQueryBase;
            public string sessionID;
            public string orgId;
            public string projectId;
        }

        // #if UNITY_WEBGL && !UNITY_EDITOR
        // [DllImport("__Internal")]
        // private static extern void InsightLoaded();

        // [DllImport("__Internal")]
        // private static extern void InsightReplayDownloadProgress(float progress);

        // [DllImport("__Internal")]
        // private static extern void InsightBundleDownloadProgress(float progress, string version);

        // [DllImport("__Internal")]
        // private static extern void InsightError(string msg);

        // [DllImport("__Internal")]
        // private static extern void InsightCouldNotGetBundle();
        // #endif

        public void SetSession(string msgJson)
        {
            if (sessionID != "" || msgJson == "")
            {
                return;
            }

            var setSessionMsg = JsonUtility.FromJson<SetSessionMsg>(msgJson);

            _accessToken = setSessionMsg.accessToken;
            _apiQueryBase = setSessionMsg.apiQueryBase;
            sessionID = setSessionMsg.sessionID.Trim();
            _orgId = setSessionMsg.orgId;
            _project_id = setSessionMsg.projectId;

            StartCoroutine(SetSessionCoroutine());
        }

        private IEnumerator SetSessionCoroutine()
        {
            yield return DownloadAndLoadReplay();

            if (_replay.Count < 2)
            {
                yield break;
            }

            yield return GetAssetBundle();

            _readyToStartPlaying = true;
            TogglePause();
        }

        public void ClearAssetCache()
        {
            Caching.ClearCache();
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            togglePauseButton.image.sprite = _isPaused ? playSprite : pauseSprite;
        }

        public void SliderChange()
        {
            _replayTime = slider.value * TotalReplayTime();
            UpdateSlider();
        }

        private void UpdateSlider()
        {
            // Debug.Log(TotalReplayTime() + "TotalReplayTime()");
            var totalTime = TotalReplayTime();

            var currentTime =
                TimeSpan.FromSeconds(Mathf.Clamp(Mathf.RoundToInt(slider.value * totalTime), 0, totalTime));
            var currentTimeString =
                currentTime.TotalHours >= 1 ? Mathf.RoundToInt(totalTime) + "s" : currentTime.ToString(@"m\:ss");

            var time = TimeSpan.FromSeconds(totalTime);
            var totalTimeString = time.TotalHours >= 1 ? Mathf.RoundToInt(totalTime) + "s" : time.ToString(@"m\:ss");

            progressText.text = $"{currentTimeString} / {totalTimeString}";
        }

        private float ReplayStartTime()
        {
            if (_replay.Count >= 1)
            {
                return _replay[0].tickEntry.unscaledTime;
            }

            return 0;
        }

        private float ReplayEndTime()
        {
            if (_replay.Count >= 1)
            {
                return _replay[_replay.Count - 1].tickEntry.unscaledTime;
            }

            return 0;
        }

        private float TotalReplayTime()
        {
            // Debug.Log(ReplayEndTime() + " rt " + ReplayStartTime());
            return Mathf.Abs(ReplayEndTime() - ReplayStartTime());
        }

        private int LoadPrefabFromAssets(uint instanceId, uint prefabId)
        {
            if (_useAssetBundle)
            {
                if (_assetBundle)
                {
                    var prefabAsset = _assetBundle.LoadAsset<GameObject>(prefabId.ToString());
                    if (prefabAsset)
                    {
                        var newInstance = Instantiate(prefabAsset, transform);
                        if (newInstance == null)
                        {
                            LogError($"Failed to instantiate prefab {prefabId} for instance {instanceId}");
                            return 0;
                        }

                        _objectInstances[instanceId] = newInstance;
                        return 1;
                    }
                    else
                    {
                        LogError($"Failed to load asset from bundle: {prefabId}");
                        return 0;
                    }
                }

                return 0;
            }

#if UNITY_EDITOR
            int matchCount = 0;
            var guids = UnityEditor.AssetDatabase.FindAssets($"t:prefab {prefabId}",
                new[] { Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", _appVersion) });

    foreach (var guid in guids)
    {
        string path = "";
        try
        {
            path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab.name == prefabId.ToString())
            {
                matchCount++;
                if (matchCount == 1)
                {
                    var newInstance = Instantiate(prefab, transform);
                    if (newInstance == null)
                    {
                        LogError($"Failed to instantiate prefab {prefabId} for instance {instanceId}");
                        continue;
                    }

                    _objectInstances[instanceId] = newInstance;
                }
            }
        }
        catch
        {
            Debug.LogWarning($"Failed to check asset with path {path}");
        }
    }

    return matchCount;
#else
            return 0;
#endif
        }
        // private Canvas GetOrCreateCanvas()
        // {
        //     var canvas = GameObject.FindGameObjectWithTag("ReplayCanvas")?.GetComponent<Canvas>();
        //     // if (canvas == null)
        //     // {
        //     //     var canvasGameObject = new GameObject("ReplayCanvas");
        //     //     canvasGameObject.tag = "ReplayCanvas";
        //     //     canvas = canvasGameObject.AddComponent<Canvas>();
        //     //     canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        //     //     canvasGameObject.AddComponent<CanvasScaler>();
        //     //     canvasGameObject.AddComponent<GraphicRaycaster>();
        //     // }
        //     return canvas;
        // }


        private GameObject GetOrCreateParentObject(ushort parentPrefabId, uint parentInstanceId)
        {
            if (!_objectInstances.TryGetValue(parentInstanceId, out var parentObject))
            {
                int matchCount = LoadPrefabFromAssets(parentInstanceId, parentPrefabId);
                if (matchCount == 0)
                {
                    LogError($"Parent object with prefab ID {parentPrefabId} not found. Using a placeholder.");
                    parentObject = Instantiate(missingTexturePrefab, transform);
                    _objectInstances[parentInstanceId] = parentObject;
                }
            }

            // Debug.Log($"Returning parent object instance {parentInstanceId} for prefab {parentPrefabId}");

            return _objectInstances[parentInstanceId];
        }

        private uint FindParentInstanceId(ushort parentPrefabId)
        {
            foreach (var kvp in _objectInstanceDetails)
            {
                if (kvp.Value.prefabId == parentPrefabId)
                {
                    return kvp.Key;
                }
            }
            return 0; // Return 0 if no matching parentPrefabId is found
        }


        private readonly Dictionary<uint, (ushort prefabId, uint parentInstanceId)> _objectInstanceDetails = new();

        private GameObject GetOrCreateObjectInstance(uint instanceId, ushort prefabId, ushort parentPrefabId, string sceneName, string skyboxName)
        {
            if (skyboxName != null)
            {
                currentSkyboxName = skyboxName;
                ApplySkybox(currentSkyboxName);

            }
            switch (instanceId)
            {
                case (uint)TrackingManager.ReservedTrackedObjectInstanceIds.CenterEye:
                    return cameraGameObject;
                case (uint)TrackingManager.ReservedTrackedObjectInstanceIds.LeftHand:
                    return leftControllerGameObject;
                case (uint)TrackingManager.ReservedTrackedObjectInstanceIds.RightHand:
                    return rightControllerGameObject;
            }

            if (!_objectInstances.ContainsKey(instanceId))
            {
                int matchCount = LoadPrefabFromAssets(instanceId, prefabId);
                if (matchCount == 0)
                {
                    if (prefabId == 0)
                    {
                        LogError($"Tracked object has Prefab Id of 0. This probably means you didn't register this tracked object so the id is filled in.");
                    }
                    else
                    {
                        LogError($"Found 0 asset(s) with name {prefabId} for object with id {instanceId}. There should be 1. Returning empty placeholder Cube.");
                    }

                    var newInstance = Instantiate(missingTexturePrefab, transform);
                    if (newInstance == null)
                    {
                        LogError($"Failed to instantiate missing texture prefab for instance {instanceId}");
                        return null;
                    }
                    Destroy(newInstance.GetComponent<BoxCollider>());
                    newInstance.transform.localScale = Vector3.one;
                    _objectInstances[instanceId] = newInstance;

                    Debug.Log($"Instantiated missing texture prefab for instance {instanceId}");
                }
                else if (matchCount > 1)
                {
                    LogError($"Found {matchCount} asset(s) with name {prefabId} for object with id {instanceId}. There should be 1. Returning first found asset.");
                }
            }

            if (!_objectInstances.TryGetValue(instanceId, out var objectInstance) || objectInstance == null)
            {
                LogError($"Failed to create or find instance for {instanceId} with prefab {prefabId}. Removing from dictionary.");
                _objectInstances.Remove(instanceId);
                return null;
            }

            // Load the FBX model for the active scene if not already loaded or if the scene has changed
#if UNITY_EDITOR
if (currentFBX == null || !currentFBX.name.Equals(sceneName))
{
    string directoryPath = "Assets/InsightDeskCache/TrackedPrefabFBX/models";
    currentFBX = LoadFBXModel(directoryPath, sceneName);
    if (currentFBX != null)
    {
        currentFBX.name = sceneName; // Set the name to the scene name for comparison
        // Debug.Log($"Loaded FBX model for scene {sceneName}");
    }
}
#endif


            // Store instance ID and prefab ID for Canvas objects
            if (objectInstance.GetComponent<Canvas>() != null)
            {
                // Debug.Log($"Storing Canvas instance {instanceId} with prefab {prefabId}");
                _objectInstanceDetails[instanceId] = (prefabId, instanceId);
            }

            // Check for TextMeshPro components and reparent if necessary
            TextMeshPro tmp3D = objectInstance.GetComponent<TextMeshPro>();
            TextMeshProUGUI tmpUI = objectInstance.GetComponent<TextMeshProUGUI>();

            if (tmp3D != null || tmpUI != null)
            {
                uint parentInstanceId = FindParentInstanceId(parentPrefabId);
                var parentObject = GetOrCreateParentObject(parentPrefabId, parentInstanceId);
                if (tmp3D != null)
                {
                    if (tmp3D.transform.parent == null || tmp3D.transform.parent.gameObject != parentObject)
                    {
                        tmp3D.transform.SetParent(parentObject.transform, false);
                        // Debug.Log($"Reparented TextMeshPro instance {instanceId} to parentPrefabId {parentPrefabId}");
                    }
                }
                else if (tmpUI != null)
                {
                    if (tmpUI.transform.parent == null || tmpUI.transform.parent.gameObject != parentObject)
                    {
                        tmpUI.transform.SetParent(parentObject.transform, false);
                        // Debug.Log($"Reparented TextMeshProUGUI instance {instanceId} to parentPrefabId {parentPrefabId}");
                    }
                }
            }

            // Ensure the object instance remains in the hierarchy
            objectInstance.SetActive(true);

            // Store instance ID and prefab ID along with parent instance ID for non-Canvas objects
            if (tmp3D != null || tmpUI != null)
            {
                _objectInstanceDetails[instanceId] = (prefabId, parentPrefabId);
            }

            // Log details about the instantiated object
            // Debug.Log($"Created object instance {instanceId} with prefab {prefabId}.");

            return objectInstance;
        }









        private List<(TickEntry tickEntry, Dictionary<uint, ObjectEntry> objectEntries)> ReadReplaySafe(MemoryStream memoryStream)
        {
            using (var binaryReader = new BinaryReader(memoryStream, System.Text.Encoding.UTF8))
            {
                try
                {
                    Debug.Log($"Memory stream length: {memoryStream.Length}");
                    return ReadReplay(memoryStream, binaryReader);
                }
                catch (EndOfStreamException e)
                {
                    LogError($"Failed to read replay file: End of stream reached unexpectedly.\n{e}");
                }
                catch (Exception e)
                {
                    LogError($"Failed to read replay file: {e}");
                }
            }

            return new List<(TickEntry, Dictionary<uint, ObjectEntry>)>();
        }


        private async void DownloadAndLoadReplay(bool check)
        {
            if (!string.IsNullOrEmpty(sessionID))
            {
                await ReadFileToMemoryStream(sessionID);
            }
            else
            {
                Debug.LogError("Session ID is not specified.");
            }
        }



        private IEnumerator DownloadAndLoadReplay()
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "InsightReplayCache"));

            Debug.Log($"Fetching replay {sessionID} from server...");
            var url =
                $"{_apiQueryBase}/session?dashboardString={UnityWebRequest.EscapeURL(sessionID)}";

            using var webRequest = new UnityWebRequest(url);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            var operation = webRequest.SendWebRequest();

#if UNITY_WEBGL && !UNITY_EDITOR
            InsightReplayDownloadProgress(0);
#endif

            Debug.Log("Waiting for server to start sending replay...");
            var downloadStarted = false;
            while (!operation.isDone)
            {
                if (webRequest.downloadProgress > 0 && webRequest.downloadProgress < 1)
                {
                    if (!downloadStarted)
                    {
                        downloadStarted = true;
                        Debug.Log("Starting download...");
                    }

#if UNITY_WEBGL && !UNITY_EDITOR
                    InsightReplayDownloadProgress(webRequest.downloadProgress);
#endif

                    if (Time.unscaledTime - _lastProgressLogTime > 0.5f)
                    {
                        LogProgress(webRequest.downloadProgress);
                    }
                }

                yield return null;
            }

            if (webRequest.responseCode == 200 || webRequest.responseCode == 416)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                InsightReplayDownloadProgress(1);
#endif
                LogProgress(1);
                var size = webRequest.downloadedBytes / 1024f / 1024f;
                Debug.Log($"Download complete. Replay size: {size.ToString("0.00")} MiB");

                if (webRequest.downloadedBytes == 0 || webRequest.downloadHandler.data == null)
                {
                    LogError("Downloaded replay is empty.");
                    yield break;
                }

                if (webRequest.responseCode == 416)
                {
                    LogError(
                        "Downloaded relay is incomplete or corrupt. This could happen because the user lost internet connection while recording. Attempting to play replay anyway.");
                }

                // var replayPath = Path.Combine(Application.persistentDataPath, "InsightReplayCache",
                //     UnityWebRequest.EscapeURL(sessionID));
                // File.WriteAllBytes(replayPath, webRequest.downloadHandler.data);

                using var memoryStream = new MemoryStream(webRequest.downloadHandler.data);
                ReadAndLoadReplay(memoryStream);

                // await ReadFileToMemoryStream();
            }
            else
            {
                var msg = $"error {webRequest.responseCode} fetching replay \"{sessionID}\"";
                if (webRequest.responseCode == 404)
                {
                    msg += ". Was StartRecording() never called during the session or is the session id invalid?";
                }

                LogError(msg);
            }
        }


        private async Task<MemoryStream> ReadFileToMemoryStream(string sessionId)
        {
            Debug.Log("Reading session files for session ID: " + sessionId);

            try
            {
                // Construct the directory path
                string directoryPath = Path.Combine("Assets", "Sessions", sessionId);

                // Check if the directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Debug.LogError("Session directory not found: " + directoryPath);
                    return null;
                }

                // Get all chunk files in the session directory
                string[] chunkFiles = Directory.GetFiles(directoryPath, "*.txt");

                // Sort chunk files by chunk ID
                Array.Sort(chunkFiles, (file1, file2) =>
                {
                    int chunkId1 = int.Parse(Path.GetFileNameWithoutExtension(file1).Split('_').Last());
                    int chunkId2 = int.Parse(Path.GetFileNameWithoutExtension(file2).Split('_').Last());
                    return chunkId1.CompareTo(chunkId2);
                });

                // Initialize a MemoryStream
                MemoryStream memoryStream = new MemoryStream();

                // Decode each base64 string and write to the MemoryStream
                foreach (string chunkFile in chunkFiles)
                {
                    string base64String = File.ReadAllText(chunkFile).Trim();
                    if (IsValidBase64String(base64String))
                    {
                        byte[] bytes = Convert.FromBase64String(base64String);
                        memoryStream.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid Base-64 string detected in file: {chunkFile}");
                    }
                }

                // Reset the MemoryStream position to the beginning
                memoryStream.Position = 0;

                Debug.Log("Memory stream file loaded");
                ReadAndLoadReplay(memoryStream);

                return memoryStream;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error reading session files to MemoryStream: " + ex.Message);
                return null;
            }
        }

        private bool IsValidBase64String(string base64)
        {
            // Check if the string length is a multiple of 4
            if (base64.Length % 4 != 0)
            {
                return false;
            }

            // Check for valid Base-64 characters
            return System.Text.RegularExpressions.Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", System.Text.RegularExpressions.RegexOptions.None);
        }





        public void ReadAndLoadReplay(MemoryStream memoryStream)
        {
            _replay = ReadReplaySafe(memoryStream);
            UpdateSlider();
        }

        private void LogProgress(float progress)
        {
            _lastProgressLogTime = Time.unscaledTime;
            const int progressBarLength = 30;
            var progressBar = "";
            var progressInt = Mathf.FloorToInt(progress * progressBarLength);
            for (int i = 0; i < progressBarLength; i++)
            {
                progressBar += i < progressInt || i == 0 ? "l" : ".";
            }

            Debug.Log($"download progress: [{progressBar}]");
        }

        private bool ProcessHeader(ChunkHeaderEntry header)
        {
            if (header.endianness != 1)
            {
                LogError(
                    "Replay was written on processor with different endianness than current machine. Replay can still be converted, but that has not been implemented yet.");
                return false;
            }

            if (header.version != 1)
            {
                LogError($"unknown replay version {header.version}");
                return false;
            }

            _appVersion = header.appVersion;
            _tickRate = header.tickRate;

            return true;
        }

        private List<(TickEntry tickEntry, Dictionary<uint, ObjectEntry> objectEntries)> ReadReplay(
            MemoryStream memoryStream,
            BinaryReader binaryReader)
        {
            var replay = new List<(TickEntry tickEntry, Dictionary<uint, ObjectEntry> objectEntries)>();

            var processedFirstHeader = false;
            var foundTooLargeTick = false;

            while (memoryStream.Position != memoryStream.Length)
            {
                var header = new ChunkHeaderEntry(binaryReader);

                if (!processedFirstHeader)
                {
                    processedFirstHeader = true;
                    if (!ProcessHeader(header))
                    {
                        return replay;
                    }

                    // Read VRHeaderEntry after the first ChunkHeaderEntry
                    var vrHeader = new VRHeaderEntry(binaryReader);
                    // Debug.Log($"VRHeaderEntry read: {vrHeader.deviceName}, {vrHeader.displayFrequency}, {vrHeader.pcName}, {vrHeader.cpuDetails}, {vrHeader.gpuDetails}, {vrHeader.batteryLevel}, {vrHeader.operatingsystem}, {vrHeader.memorysize}, {vrHeader.processorfrequency}");
                }

                for (int j = 0; j < header.numTicksInChunk; j++)
                {
                    var lastReplayEntries = replay.Count > 0 ? replay[replay.Count - 1].objectEntries : null;

                    var tick = new TickEntry(binaryReader);
                    var entries = new Dictionary<uint, ObjectEntry>();

                    for (int i = 0; i < tick.numObjects; i++)
                    {
                        var insightObjectEntry = new ObjectEntry(binaryReader);

                        if (lastReplayEntries != null &&
                            lastReplayEntries.TryGetValue(insightObjectEntry.instanceId, out var lastObjectEntry))
                        {
                            if (!insightObjectEntry.newPos)
                            {
                                insightObjectEntry.position = lastObjectEntry.position;
                            }

                            if (!insightObjectEntry.newRot)
                            {
                                insightObjectEntry.rotation = lastObjectEntry.rotation;
                            }

                            if (!insightObjectEntry.newScale)
                            {
                                insightObjectEntry.localScale = lastObjectEntry.localScale;
                            }

                            foreach (var lastValue in lastObjectEntry.newAnimationFloats)
                            {
                                var newEntryHasLastValue =
                                    insightObjectEntry.newAnimationFloats.Any(entry => entry.Item1 == lastValue.Item1);
                                if (!newEntryHasLastValue)
                                {
                                    insightObjectEntry.newAnimationFloats.Add(lastValue);
                                }
                            }

                            foreach (var lastValue in lastObjectEntry.newAnimationInts)
                            {
                                var newEntryHasLastValue =
                                    insightObjectEntry.newAnimationInts.Any(entry => entry.Item1 == lastValue.Item1);
                                if (!newEntryHasLastValue)
                                {
                                    insightObjectEntry.newAnimationInts.Add(lastValue);
                                }
                            }

                            foreach (var lastValue in lastObjectEntry.newAnimationBools)
                            {
                                var newEntryHasLastValue =
                                    insightObjectEntry.newAnimationBools.Any(entry => entry.Item1 == lastValue.Item1);
                                if (!newEntryHasLastValue)
                                {
                                    insightObjectEntry.newAnimationBools.Add(lastValue);
                                }
                            }
                            foreach (var lastValue in lastObjectEntry.newTexts)
                            {
                                var newEntryHasLastValue =
                                    insightObjectEntry.newTexts.Any(entry => entry.Item1 == lastValue.Item1);
                                if (!newEntryHasLastValue)
                                {
                                    insightObjectEntry.newTexts.Add(lastValue);
                                }
                            }
                        }

                        entries.Add(insightObjectEntry.instanceId, insightObjectEntry);
                    }

                    if (!foundTooLargeTick &&
                        entries.Count >= TrackingManager.NumTrackedObjectsExpectedUpperEnd)
                    {
                        foundTooLargeTick = true;
                        LogError(
                            $"Tick contains {entries.Count} entries. If tracked session has more than {TrackingManager.NumTrackedObjectsExpectedUpperEnd} objects, any extra may not be tracked.");
                    }

                    var destroyedIds = new Queue<uint>();
                    for (int i = 0; i < tick.numDeleted; i++)
                    {
                        var insightDestroyObjectEntry = new DestroyObjectEntry(binaryReader);
                        destroyedIds.Enqueue(insightDestroyObjectEntry.instanceId);
                    }

                    if (lastReplayEntries != null)
                    {
                        foreach (var lastObjectEntry in lastReplayEntries.Values)
                        {
                            if (entries.TryGetValue(lastObjectEntry.instanceId, out var entry))
                            {
                                if (!entry.activeInHierarchy)
                                {
                                    entry.position = lastObjectEntry.position;
                                    entry.rotation = lastObjectEntry.rotation;
                                    entry.localScale = lastObjectEntry.localScale;
                                }

                                if (!lastObjectEntry.activeInHierarchy && entry.activeInHierarchy)
                                {
                                    lastObjectEntry.position = entry.position;
                                    lastObjectEntry.rotation = entry.rotation;
                                    lastObjectEntry.localScale = entry.localScale;
                                }
                            }
                            else
                            {
                                var lastObjectEntryCopy = new ObjectEntry(lastObjectEntry);
                                entries.Add(lastObjectEntryCopy.instanceId, lastObjectEntryCopy);
                            }
                        }
                    }

                    while (destroyedIds.Count > 0)
                    {
                        var destroyedId = destroyedIds.Dequeue();
                        if (entries.ContainsKey(destroyedId))
                        {
                            entries.Remove(destroyedId);
                        }
                    }

                    replay.Add((tick, entries));
                }
            }

            return replay;
        }

        private static void LogError(string msg)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            InsightError(msg);
#endif
            InsightUtility.LogError(msg);
        }
    }
}