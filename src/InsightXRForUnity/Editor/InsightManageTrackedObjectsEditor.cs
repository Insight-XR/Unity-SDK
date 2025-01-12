#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Networking;
using UnityEditor.Formats.Fbx.Exporter;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO.Compression;


namespace InsightDesk
{
    public static class UnityWebRequestExtensions
    {
        public static TaskAwaiter<UnityWebRequest.Result> GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<UnityWebRequest.Result>();
            asyncOp.completed += _ => tcs.SetResult(asyncOp.webRequest.result);
            return tcs.Task.GetAwaiter();
        }
    }
    public class InsightManageTrackedObjectsEditor : EditorWindow
    {
        private const string AssetPrefabHashesFileName = "asset_prefab_hashes";
        private const string SceneObjectHashesFileNamePrefix = "scene_object_hashes_";
        private Material skyboxMaterial;

        private static EditorCoroutine _registerSceneRoutine;
        private static EditorCoroutine _registerAssetsRoutine;
        private static ushort _lastUsedTrackedObjectPrefabId = 3;
        private static readonly Dictionary<string, ushort> ReservedPrefabIds = new Dictionary<string, ushort>
{
    { "camera", 1 },
    { "left hand", 2 },
    { "right hand", 3 }
};

        private static int _trackSceneProgress = 0;
        private static int _trackSceneTotal = 0;
        private static int _trackAssetsProgress = 0;
        private static int _trackAssetsTotal = 0;
        private static string customerId;
        private static InsightSettingsSO insightSettings;

        private static UnityWebRequest _packageUploadUwr;
        private static UnityWebRequest _bundleUploadUwr;

        private static string _lastBundleHash = "";
        private List<GameObject> manuallyAssignedObjects = new List<GameObject>();

        private GameObject newObjectSelection;

        private const string TrackedPrefabFBXPath = "Assets/InsightDeskCache/TrackedPrefabFBX/models";

[MenuItem("InsightXR/Manage Tracked Objects")]
static void Init()
{
    var window = (InsightManageTrackedObjectsEditor)EditorWindow.GetWindow(typeof(InsightManageTrackedObjectsEditor));
    window.Show();
    window.position = new Rect(20, 80, 400, 300);
    insightSettings = AssetDatabase.LoadAssetAtPath<InsightSettingsSO>("Assets/InsightSettings.asset");
    if (insightSettings == null)
    {
        Debug.LogError("InsightSettingsSO is not assigned. Please create and assign it in the Inspector.");
    }
    else
    {
        customerId = insightSettings.customerID;
    }
}

        private bool HasExcludedComponents(GameObject go)
        {
            return go.GetComponent<InsightTrackObject>() != null ||
                   go.GetComponent<TrackingManager>() != null ||
                   go.GetComponent<Camera>() != null && !go.activeSelf;


        }

        private void AddGameObjectAndChildren(GameObject go)
        {
            if (!manuallyAssignedObjects.Contains(go))
            {
                manuallyAssignedObjects.Add(go);
            }

            foreach (Transform child in go.transform)
            {
                AddGameObjectAndChildren(child.gameObject);
            }
        }



        private IEnumerator SelectStaticObjects()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                yield break;
            }

            List<GameObject> objectsWithoutTrackScript;
            try
            {
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();

                var allGameObjects = FindObjectsOfType<GameObject>().ToList();
                var excludedObjects = manuallyAssignedObjects.Where(go => go != null && HasExcludedComponents(go)).ToHashSet();

                
                objectsWithoutTrackScript = allGameObjects
    .Where(go =>
        // 1) must be active
        go.activeSelf

        // 2) must NOT have InsightTrackObject
        && go.GetComponent<InsightTrackObject>() == null

        // 3) doesn't have camera/tracking components
        && !HasExcludedComponents(go)
        && !excludedObjects.Contains(go)

        // 4) no camera in the hierarchy
        && !HasCameraComponentInHierarchy(go)

        // 5) must have a MeshRenderer somewhere
        && go.GetComponentInChildren<MeshRenderer>() != null
    )
    .ToList();



            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while gathering objects: {ex.Message}");
                yield break;
            }

            var activeSceneName = SceneManager.GetActiveScene().name;
            var root = new GameObject(activeSceneName);
            var processedObjects = new HashSet<GameObject>();
            var objectsToDuplicate = new HashSet<GameObject>();

            foreach (var go in objectsWithoutTrackScript)
            {
                if (!processedObjects.Contains(go))
                {
                    objectsToDuplicate.Add(go);
                    MarkAsProcessed(go, processedObjects);
                }
            }

            foreach (var go in objectsWithoutTrackScript)
            {
                if (objectsToDuplicate.Contains(go))
                {
                    RemoveChildrenFromList(go, objectsToDuplicate);
                }
            }

            objectsToDuplicate.RemoveWhere(HasParentWithInsightTrackObject);

            foreach (var go in objectsToDuplicate)
            {
                try
                {
                    // 1) If it's a Terrain, export to OBJ
                    if (go.GetComponent<Terrain>())
                    {
                        var terrainExportPath = Path.Combine("Assets", "InsightDeskCache", "TrackedTerrainObj", Application.version);
                        Directory.CreateDirectory(terrainExportPath);
                        ExportTerrain(go, terrainExportPath);
                    }
                    // 2) Otherwise, duplicate it and exclude things you don't want
                    else
                    {
                        var duplicate = Instantiate(go, root.transform, true);
                        duplicate.name = go.name;

                        // Remove objects with InsightTrackObject
                        ExcludeChildrenWithInsightTrackObject(duplicate);

                        // Remove inactive children if they do NOT have InsightTrackObject
                        ExcludeInactiveChildrenWithoutTrack(duplicate);

                        // Remove *all* inactive children (regardless of track object)
                        ExcludeInactiveChildren(duplicate);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing object {go.name}: {ex.Message}");
                    continue;
                }

                // Yield to let the editor breathe each iteration
                yield return null;
            }

        }

        private bool HasCameraComponentInHierarchy(GameObject go)
        {
            return go.GetComponentInChildren<Camera>(true) != null;
        }

        private void ExcludeInactiveChildren(GameObject parent)
        {
            // Loop backwards so we can remove children safely while iterating
            for (int i = parent.transform.childCount - 1; i >= 0; i--)
            {
                var childTransform = parent.transform.GetChild(i);
                var childGO = childTransform.gameObject;

                if (!childGO.activeSelf)
                {
                    // Remove the inactive child immediately
                    DestroyImmediate(childGO);
                }
                else
                {
                    // Recursively check deeper children
                    ExcludeInactiveChildren(childGO);
                }
            }
        }

        private void ExcludeInactiveChildrenWithoutTrack(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                var childGO = child.gameObject;
                // If child is inactive *and* doesn't have InsightTrackObject, remove it
                if (!childGO.activeSelf && childGO.GetComponent<InsightTrackObject>() == null)
                {
                    DestroyImmediate(childGO);
                    // No need to recurse if we destroyed it
                }
                else
                {
                    // Otherwise, keep checking deeper children
                    ExcludeInactiveChildrenWithoutTrack(childGO);
                }
            }
        }



        private void ExportTerrain(GameObject go, string exportPath)
        {
            Terrain terrain = go.GetComponent<Terrain>();
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPos = terrain.transform.position;

            string sceneName = Path.GetFileNameWithoutExtension(EditorSceneManager.GetActiveScene().name);
            string fileName = Path.Combine(exportPath, $"{sceneName}.obj");
            int w = terrainData.heightmapResolution;
            int h = terrainData.heightmapResolution;
            Vector3 meshScale = terrainData.size;
            int tRes = 1; // Full resolution
            meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
            Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
            float[,] tData = terrainData.GetHeights(0, 0, w, h);

            w = (w - 1) / tRes + 1;
            h = (h - 1) / tRes + 1;
            Vector3[] tVertices = new Vector3[w * h];
            Vector2[] tUV = new Vector2[w * h];

            int[] tPolys = new int[(w - 1) * (h - 1) * 6];

            // Build vertices and UVs
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-y, tData[x * tRes, y * tRes], x)) + terrainPos;
                    tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
                }
            }

            int index = 0;
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;

                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = (y * w) + x + 1;
                }
            }

            // Export to .obj
            StreamWriter sw = new StreamWriter(fileName);
            try
            {
                sw.WriteLine("# Unity terrain OBJ File");

                // Write vertices
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                for (int i = 0; i < tVertices.Length; i++)
                {
                    sw.WriteLine($"v {tVertices[i].x} {tVertices[i].y} {tVertices[i].z}");
                }
                // Write UVs
                for (int i = 0; i < tUV.Length; i++)
                {
                    sw.WriteLine($"vt {tUV[i].x} {tUV[i].y}");
                }
                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                    sw.WriteLine($"f {tPolys[i] + 1}/{tPolys[i] + 1} {tPolys[i + 1] + 1}/{tPolys[i + 1] + 1} {tPolys[i + 2] + 1}/{tPolys[i + 2] + 1}");
                }
            }
            catch (Exception err)
            {
                Debug.LogError("Error saving file: " + err.Message);
            }
            sw.Close();

            AssetDatabase.Refresh();
        }

        private bool HasParentWithInsightTrackObject(GameObject go)
        {
            var parent = go.transform.parent;
            while (parent != null)
            {
                if (parent.GetComponent<InsightTrackObject>() != null)
                {
                    return true;
                }
                parent = parent.parent;
            }
            return false;
        }



        private void MarkAsProcessed(GameObject parent, HashSet<GameObject> processedObjects)
        {
            processedObjects.Add(parent);
            foreach (Transform child in parent.transform)
            {
                if (!processedObjects.Contains(child.gameObject))
                {
                    processedObjects.Add(child.gameObject);
                    MarkAsProcessed(child.gameObject, processedObjects);
                }
            }
        }

        private void RemoveChildrenFromList(GameObject parent, HashSet<GameObject> objectsToDuplicate)
        {
            foreach (Transform child in parent.transform)
            {
                if (objectsToDuplicate.Contains(child.gameObject))
                {
                    objectsToDuplicate.Remove(child.gameObject);
                }
                RemoveChildrenFromList(child.gameObject, objectsToDuplicate);
            }
        }

        private void ExcludeChildrenWithInsightTrackObject(GameObject parent)
        {
            foreach (Transform child in parent.transform)
            {
                if (child.GetComponent<InsightTrackObject>() != null)
                {
                    DestroyImmediate(child.gameObject);  // Remove from hierarchy to exclude from export
                }
                else
                {
                    ExcludeChildrenWithInsightTrackObject(child.gameObject);  // Recursively check children
                }
            }
        }


        private Vector2 scrollPosition = Vector2.zero; // Add this to keep track of scroll position

        void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 20));

            if (GUI.Button(new Rect(10, 10, 260, 50), "Register Tracked Objects [In open scene]"))
            {
                if (_registerSceneRoutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
                }

                if (_registerAssetsRoutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
                }

                _trackSceneProgress = 0;
                _trackSceneTotal = 0;
                _trackAssetsProgress = 0;
                _trackAssetsTotal = 0;
                _registerSceneRoutine = EditorCoroutineUtility.StartCoroutine(RegisterTrackedSceneObjects(), this);
            }

            if (GUI.Button(new Rect(10, 70, 260, 50), "Register Tracked Objects [In Assets]"))
            {
                if (_registerSceneRoutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
                }

                if (_registerAssetsRoutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
                }

                _trackSceneProgress = 0;
                _trackSceneTotal = 0;
                _trackAssetsProgress = 0;
                _trackAssetsTotal = 0;
                _registerAssetsRoutine = EditorCoroutineUtility.StartCoroutine(RegisterTrackedAssetPrefabs(), this);
            }

            if (GUI.Button(new Rect(10, 130, 150, 50), "Cancel"))
            {
                _trackSceneProgress = 0;
                _trackSceneTotal = 0;
                _trackAssetsProgress = 0;
                _trackAssetsTotal = 0;
                if (_registerSceneRoutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
                    _registerSceneRoutine = null;
                }

                if (_registerAssetsRoutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
                    _registerAssetsRoutine = null;
                }

                if (_registerSceneRoutine != null || _registerAssetsRoutine != null)
                {
                    Debug.Log("Register Tracked Objects was canceled");
                }
            }

            var progressString = "Progress: (0/0)";
            if (_trackSceneTotal != 0)
            {
                progressString = $"Progress: scene objects ({_trackSceneProgress}/{_trackSceneTotal})";
            }

            if (_trackAssetsTotal != 0)
            {
                progressString = $"Progress: assets ({_trackAssetsProgress}/{_trackAssetsTotal})";
            }
            GUI.Label(new Rect(10, 170, 500, 50), progressString);

            GUILayout.Space(400); // Add space before the manually assigned objects section

            // Manual assignment of GameObjects
            GUILayout.Label("Manually Assigned Objects", EditorStyles.boldLabel);
            for (int i = 0; i < manuallyAssignedObjects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                manuallyAssignedObjects[i] = (GameObject)EditorGUILayout.ObjectField(manuallyAssignedObjects[i], typeof(GameObject), true);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    manuallyAssignedObjects.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }

            // Display the field for selecting a new object outside of the button condition
            newObjectSelection = (GameObject)EditorGUILayout.ObjectField(newObjectSelection, typeof(GameObject), true);

            // Button to add the selected object
            if (GUILayout.Button("Add Object") && newObjectSelection != null)
            {
                AddGameObjectAndChildren(newObjectSelection);
                newObjectSelection = null; // Reset the selection after adding
            }

            if (GUILayout.Button("Clear All"))
            {
                manuallyAssignedObjects.Clear();
            }

            if (GUI.Button(new Rect(10, 220, 200, 50), "Select Static Objects"))
            {
                EditorCoroutineUtility.StartCoroutine(SelectStaticObjects(), this);
            }
            if (GUI.Button(new Rect(10, 280, 200, 50), "Upload FBX Models"))
            {
                _ = UploadFBXModels();
            }
            GUILayout.Label("Drag and Drop a 6-Sided Skybox Material", EditorStyles.boldLabel);
            skyboxMaterial = (Material)EditorGUILayout.ObjectField(skyboxMaterial, typeof(Material), false);

            if (GUILayout.Button("Register Skybox") && skyboxMaterial != null)
            {
                RegisterSkybox(skyboxMaterial);
            }

            GUILayout.EndScrollView();
        }


      private async Task UploadFBXModels()
{
    if (insightSettings == null)
    {
        Debug.LogError("InsightSettingsSO is not assigned. Please create and assign it in the Inspector.");
        return;
    }

    string cacheDirectory = "Assets/InsightDeskCache/TrackedPrefabFBX";
    string zipFilePath = "Assets/InsightDeskCache/assets.zip";

    CreateZipFromDirectory(cacheDirectory, zipFilePath);
    // ZipFileLogger.LogFilesInZip("Assets/InsightDeskCache/assets.zip");

    byte[] zipData = File.ReadAllBytes(zipFilePath);

    string url = $"http://35.193.4.57/input/model/{insightSettings.customerID}";

    WWWForm form = new WWWForm();
    form.AddBinaryData("zipfile", zipData, "assets.zip", "application/zip");

    using (UnityWebRequest www = UnityWebRequest.Put(url, form.data))
    {
        www.method = UnityWebRequest.kHttpVerbPUT;
        www.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + form.headers["Content-Type"].Split('=')[1]);

        foreach (var header in form.headers)
        {
            www.SetRequestHeader(header.Key, header.Value);
        }

        var operation = www.SendWebRequest();

        double lastLogTime = 0;
        int lastProgress = 0;

        while (!operation.isDone)
        {
            if (EditorApplication.timeSinceStartup - lastLogTime > 0.5f || lastProgress != Mathf.FloorToInt(www.uploadProgress * 100f))
            {
                lastLogTime = EditorApplication.timeSinceStartup;
                lastProgress = Mathf.FloorToInt(www.uploadProgress * 100f);
                Debug.Log($"Uploading Models... {lastProgress}%");
            }

            await Task.Yield(); // Yield to other tasks to prevent blocking the UI thread
        }

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("<color=green>Successfully uploaded assets.zip</color>");

            try
            {
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                    // Debug.Log("Successfully deleted models.zip");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error deleting assets.zip: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"Failed to upload assets.zip: {www.error}");
        }
    }
}
         private void RegisterSkybox(Material skybox)
        {
            if (skybox == null)
            {
                Debug.LogError("No skybox material assigned.");
                return;
            }

            string skyboxName = skybox.name;
            string savePath = Path.Combine("Assets", "InsightDeskCache", "TrackedPrefabFBX", "skyboxes", skyboxName);
            string materialSavePath = Path.Combine("Assets", "InsightDeskCache", "SkyboxMaterials");

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            if (!Directory.Exists(materialSavePath))
            {
                Directory.CreateDirectory(materialSavePath);
            }

            // Save the material itself
            string materialPath = Path.Combine(materialSavePath, skyboxName + ".mat");
            AssetDatabase.CreateAsset(new Material(skybox), materialPath);

            string[] sides = { "_FrontTex", "_BackTex", "_LeftTex", "_RightTex", "_UpTex", "_DownTex" };
            string[] sideNames = { "Front", "Back", "Left", "Right", "Up", "Down" };

            for (int i = 0; i < sides.Length; i++)
            {
                Texture2D texture = (Texture2D)skybox.GetTexture(sides[i]);
                if (texture == null)
                {
                    Debug.LogError($"Skybox side {sides[i]} not found.");
                    continue;
                }

                // Ensure the texture is read/write enabled and in an uncompressed format
                string texturePath = AssetDatabase.GetAssetPath(texture);
                TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(texturePath);
                if (textureImporter != null)
                {
                    textureImporter.isReadable = true;
                    if (textureImporter.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    }
                    textureImporter.SaveAndReimport();
                }

                // Re-import the texture to apply changes
                Texture2D reimportedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                byte[] bytes = reimportedTexture.EncodeToPNG();
                File.WriteAllBytes(Path.Combine(savePath, $"{sideNames[i]}.png"), bytes);
            }

            AssetDatabase.Refresh();
            Debug.Log($"Skybox {skyboxName} has been registered and saved to {savePath}, and material saved to {materialSavePath}");
        }



        public static void CreateZipFromDirectory(string sourceDirectory, string destinationZipFilePath)
{
    if (File.Exists(destinationZipFilePath))
    {
        File.Delete(destinationZipFilePath);
    }

    using (var zipArchive = ZipFile.Open(destinationZipFilePath, ZipArchiveMode.Create))
    {
        foreach (var filePath in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            if (!filePath.EndsWith(".meta"))
            {
                var relativePath = Path.GetRelativePath(sourceDirectory, filePath);

                // Ensure forward slashes in the relative path
                relativePath = relativePath.Replace("\\", "/");

                zipArchive.CreateEntryFromFile(filePath, relativePath);
            }
        }
    }
}




        private IEnumerator RegisterTrackedSceneObjects()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                yield break;
            }
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            var scenePath = EditorSceneManager.GetActiveScene().path;
            var sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            var sceneObjectHashesFileName = $"{SceneObjectHashesFileNamePrefix}{sceneGuid}";
            var currentSceneObjectHashes = ReadHashesFile(sceneObjectHashesFileName);
            var sceneObjectHashes = new List<(Hash128, List<(ushort, Hash128)>)>();
            var prefabsPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            var fbxExportPath = TrackedPrefabFBXPath;
            Directory.CreateDirectory(prefabsPath);
            Directory.CreateDirectory(fbxExportPath);
            var usedPrefabIds = GetUsedPrefabIds(prefabsPath);
            var objects = FindObjectsOfType<InsightTrackObject>(true);
            _trackSceneTotal = objects.Length;
            var assetPrefabHashes = ReadHashesFile(AssetPrefabHashesFileName);
            var updatedAsset = false;
            var sceneObjectIds = new HashSet<ushort>();

            // Ensure the tags exist in the project
            EnsureTagExists("Left_AutoHand");
            EnsureTagExists("Right_AutoHand");

            foreach (var foundObject in objects)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(foundObject))
                {
                    var root = PrefabUtility.GetNearestPrefabInstanceRoot(foundObject);
                    if (root)
                    {
                        var overrides = PrefabUtility.GetObjectOverrides(root);
                        var hasNoOverrides = overrides.Count == 0 ||
                                             overrides.Count == 1 && overrides[0].instanceObject == root.transform;
                        var noAddedComponents = PrefabUtility.GetAddedComponents(root).Count == 0;
                        var noRemovedComponents = PrefabUtility.GetRemovedComponents(root).Count == 0;
                        var noAddedGameObjects = PrefabUtility.GetAddedGameObjects(root).Count == 0;
                        if (hasNoOverrides && noAddedComponents && noRemovedComponents && noAddedGameObjects)
                        {
                            _trackSceneProgress++;
                            continue;
                        }
                    }
                }
                var foundObjectHash = HashGameObject(foundObject.gameObject);
                var copyHashesList = currentSceneObjectHashes.FindAll((value) => value.Item1 == foundObjectHash);
                if (copyHashesList.Count > 0)
                {
                    var allMatches = true;
                    var copyHashes = copyHashesList[0].Item2;
                    for (int i = 0; i < copyHashes.Count; i++)
                    {
                        var copyPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs",
                            Application.version,
                            $"{copyHashes[i].Item1}.prefab");
                        var copyHash = AssetDatabase.GetAssetDependencyHash(copyPath);
                        if (copyHash != copyHashes[i].Item2)
                        {
                            allMatches = false;
                        }
                    }
                    if (allMatches)
                    {
                        _trackSceneProgress++;
                        sceneObjectHashes.Add((foundObjectHash, copyHashes));
                        continue;
                    }
                }
                var idHashes = new List<(ushort, Hash128)>();
                var prefabId = GetPrefabIdOrCreate(foundObject, usedPrefabIds);
                var assetHashesHasId = false;
                for (int i = 0; i < assetPrefabHashes.Count; i++)
                {
                    var hashIds = assetPrefabHashes[i].Item2;
                    for (int j = 0; j < hashIds.Count; j++)
                    {
                        if (hashIds[j].Item1 == prefabId)
                        {
                            assetHashesHasId = true;
                        }
                    }
                }
                if (assetHashesHasId)
                {
                    Debug.LogWarning(
                        $"({foundObject.name}) scene GameObject's InsightTrackObject Prefab Id has same id as previously processed prefab even though it does not match the prefab exactly. changing id.");
                    prefabId = GetPrefabIdOrCreate(foundObject, usedPrefabIds, true);
                }
                if (sceneObjectIds.Contains(prefabId))
                {
                    Debug.LogWarning(
                        $"({foundObject.name}) scene GameObject's InsightTrackObject Prefab Id has same id another object in the scene even though it is not an unaltered prefab instance. changing id.");
                    prefabId = GetPrefabIdOrCreate(foundObject, usedPrefabIds, true);
                }
                sceneObjectIds.Add(prefabId);
                // Export as FBX directly from the game object
                var fbxFilePath = Path.Combine(fbxExportPath, $"{prefabId}.fbx");
                ExportBinaryFBX(fbxFilePath, foundObject.gameObject);
                
                // Save prefab in the cache
                var path = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version, $"{prefabId}.prefab");
                var copy = Instantiate(foundObject.gameObject);
                copy.name = foundObject.name;
                copy.transform.position = foundObject.transform.position;
                copy.transform.rotation = foundObject.transform.rotation;
                copy.transform.localScale = foundObject.transform.lossyScale;

                // Set the tag for the prefab copy if it has InsightTrackHandAnchor
                if (copy.TryGetComponent<InsightTrackHandAnchor>(out var handAnchor))
                {
                    var tagName = handAnchor.hand == InsightTrackHandAnchor.Hand.Left ? "Left_AutoHand" : "Right_AutoHand";
                    copy.tag = tagName;
                }

                PrefabUtility.SaveAsPrefabAsset(copy, path);
                DestroyImmediate(copy);
                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var prefabRoot = editingScope.prefabContentsRoot;
                    StripComponentsExceptRendering(prefabRoot, foundObject.name);
                }
                var insightCachePrefabHash = AssetDatabase.GetAssetDependencyHash(path);
                idHashes.Add((prefabId, insightCachePrefabHash));
                sceneObjectHashes.Add((foundObjectHash, idHashes));
                _trackSceneProgress++;
                updatedAsset = true;
                yield return null;
            }
            WriteHashesFile(sceneObjectHashes, sceneObjectHashesFileName);
            AssetDatabase.SaveAssets();
            if (updatedAsset)
            {
                Debug.Log("Registered all tracked objects in currently opened scene");
            }
            else
            {
                Debug.Log("Everything is already up to date. No scene objects registered.");
            }
        }

        // Helper method to ensure tag exists in the project
        private void EnsureTagExists(string tagName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            bool tagExists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(tagName)) { tagExists = true; break; }
            }

            if (!tagExists)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
                newTagProp.stringValue = tagName;
                tagManager.ApplyModifiedProperties();
                Debug.Log($"Tag '{tagName}' has been created.");
            }
        }




        // private void Export(string filePath, GameObject exportObject)
        // {
        //     ExportBinaryFBX(filePath, exportObject);
        // }
        private static void ExportBinaryFBX(string filePath, UnityEngine.Object singleObject)
        {
            // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
            filePath = filePath.Replace("\\", "/");
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
            Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
            Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");
            // Instantiate a settings object instance
            MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
            object optionsInstance = optionsProperty.Invoke(null, null);
            // Set the export options
            FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
            exportFormatField.SetValue(optionsInstance, 1); // Binary format
            FieldInfo modelOptionsField = optionsType.GetField("modelOptions", BindingFlags.Instance | BindingFlags.NonPublic);
            if (modelOptionsField != null)
            {
                modelOptionsField.SetValue(optionsInstance, true); // Models only
            }
            FieldInfo lodLevelField = optionsType.GetField("lodLevel", BindingFlags.Instance | BindingFlags.NonPublic);
            if (lodLevelField != null)
            {
                lodLevelField.SetValue(optionsInstance, 2); // All levels, assuming 2 represents "All Levels"
            }
            FieldInfo objectPositionField = optionsType.GetField("objectPosition", BindingFlags.Instance | BindingFlags.NonPublic);
            if (objectPositionField != null)
            {
                objectPositionField.SetValue(optionsInstance, 0); // Local centered, assuming 0 represents "Local Centered"
            }
            FieldInfo compatibleNamesField = optionsType.GetField("compatibleNames", BindingFlags.Instance | BindingFlags.NonPublic);
            if (compatibleNamesField != null)
            {
                compatibleNamesField.SetValue(optionsInstance, true); // Compatible names: true
            }
            FieldInfo exportUnrenderedField = optionsType.GetField("exportUnrendered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (exportUnrenderedField != null)
            {
                exportUnrenderedField.SetValue(optionsInstance, true); // Export unrendered: true
            }
            FieldInfo keepInstancesField = optionsType.GetField("keepInstances", BindingFlags.Instance | BindingFlags.NonPublic);
            if (keepInstancesField != null)
            {
                keepInstancesField.SetValue(optionsInstance, true); // Keep instances: true
            }
            FieldInfo embedTexturesField = optionsType.GetField("embedTextures", BindingFlags.Instance | BindingFlags.NonPublic);
            if (embedTexturesField != null)
            {
                embedTexturesField.SetValue(optionsInstance, true); // Embed textures: true
            }
            // Invoke the ExportObject method with the settings param
            MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
            exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });
        }

        private IEnumerator RegisterTrackedAssetPrefabs()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                yield break;
            }

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            var prefabsPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            var fbxExportPath = TrackedPrefabFBXPath; // Add this line to specify the FBX export path
            Directory.CreateDirectory(prefabsPath);
            Directory.CreateDirectory(fbxExportPath); // Create the directory for FBX files

            var usedPrefabIds = GetUsedPrefabIds(prefabsPath);

            var assetGuids = AssetDatabase.FindAssets("InsightTrackObject");
            if (assetGuids.Length == 0)
            {
                InsightUtility.LogError("Could not find InsightTrackObject script");
                yield break;
            }

            if (assetGuids.Length > 1)
            {
                Debug.LogWarning("Found multiple scripts with name InsightTrackObject");
            }

            var scenesHashes = new List<List<(Hash128, List<(ushort, Hash128)>)>>();
            var prefabPathFiles = Directory.GetFiles(prefabsPath);
            for (int i = 0; i < prefabPathFiles.Length; i++)
            {
                var fileName = Path.GetFileNameWithoutExtension(prefabPathFiles[i]);
                var fileExt = Path.GetExtension(prefabPathFiles[i]);
                if (fileName.StartsWith(SceneObjectHashesFileNamePrefix) && fileExt == ".txt")
                {
                    var sceneObjectHashes = ReadHashesFile(fileName);
                    scenesHashes.Add(sceneObjectHashes);
                }
            }

            var currentAssetPrefabHashes = ReadHashesFile(AssetPrefabHashesFileName);
            var assetPrefabHashes = new List<(Hash128, List<(ushort, Hash128)>)>();

            var insightTrackObjectPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);

            var prefabPaths = GetAllPrefabsWithDependency(insightTrackObjectPath);
            _trackAssetsTotal = prefabPaths.Count;
            var assetIds = new HashSet<ushort>();
            var updatedAsset = false;
            foreach (var path in prefabPaths)
            {
                var sourceHash = AssetDatabase.GetAssetDependencyHash(path);
                var copyHashesList = currentAssetPrefabHashes.FindAll((value) => value.Item1 == sourceHash);
                if (copyHashesList.Count > 0)
                {
                    var allMatches = true;
                    var copyHashes = copyHashesList[0].Item2;
                    for (int i = 0; i < copyHashes.Count; i++)
                    {
                        var copyPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs",
                            Application.version,
                            $"{copyHashes[i].Item1}.prefab");
                        var copyHash = AssetDatabase.GetAssetDependencyHash(copyPath);
                        if (copyHash != copyHashes[i].Item2)
                        {
                            allMatches = false;
                        }
                    }

                    if (allMatches)
                    {
                        _trackAssetsProgress++;
                        assetPrefabHashes.Add((sourceHash, copyHashes));
                        continue;
                    }
                }

                var idHashes = new List<(ushort, Hash128)>();
                using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
                {
                    var prefabRoot = editingScope.prefabContentsRoot;

                    foreach (var prefabInsightTrackObject in prefabRoot.GetComponentsInChildren<InsightTrackObject>())
                    {
                        var prefabId = GetPrefabIdOrCreate(prefabInsightTrackObject, usedPrefabIds);
                        // Debug.Log(prefabId);

                        var sceneObjectHashesHasId = false;
                        for (int n = 0; n < scenesHashes.Count; n++)
                        {
                            for (int i = 0; i < scenesHashes[n].Count; i++)
                            {
                                var hashIds = scenesHashes[n][i].Item2;
                                for (int j = 0; j < hashIds.Count; j++)
                                {
                                    if (hashIds[j].Item1 == prefabId)
                                    {
                                        sceneObjectHashesHasId = true;
                                    }
                                }
                            }
                        }

                        if (sceneObjectHashesHasId)
                        {
                            Debug.LogWarning(
                                $"prefab with path {path} has same id as previously processed scene object. changing id.");
                            prefabId = GetPrefabIdOrCreate(prefabInsightTrackObject, usedPrefabIds, true);
                        }

                        if (assetIds.Contains(prefabId))
                        {
                            Debug.LogWarning(
                                $"prefab with path {path} has same id as previously processed prefab. changing id.");
                            prefabId = GetPrefabIdOrCreate(prefabInsightTrackObject, usedPrefabIds, true);
                        }

                        assetIds.Add(prefabId);

                        var copy = Instantiate(prefabInsightTrackObject.gameObject);
                        // unpack copy?
                        copy.name = prefabInsightTrackObject.name;
                        var copyPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs",
                            Application.version,
                            $"{prefabId}.prefab");
                        PrefabUtility.SaveAsPrefabAsset(copy, copyPath);
                        DestroyImmediate(copy);

                        using (var copyEditingScope = new PrefabUtility.EditPrefabContentsScope(copyPath))
                        {
                            var copyRoot = copyEditingScope.prefabContentsRoot;
                            StripComponentsExceptRendering(copyRoot, path);
                        }

                        var insightCachePrefabHash = AssetDatabase.GetAssetDependencyHash(copyPath);
                        idHashes.Add((prefabId, insightCachePrefabHash));

                        // Export as FBX directly from the prefab root
                        var fbxFilePath = Path.Combine(fbxExportPath, $"{prefabId}.fbx");
                        ExportBinaryFBX(fbxFilePath, prefabRoot);

                        yield return null;
                    }
                }

                assetPrefabHashes.Add((AssetDatabase.GetAssetDependencyHash(path), idHashes));

                _trackAssetsProgress++;
                updatedAsset = true;
                yield return null;
            }

            WriteHashesFile(assetPrefabHashes, AssetPrefabHashesFileName);

            AssetDatabase.SaveAssets();

            if (updatedAsset)
            {
                Debug.Log("Registered all tracked prefabs in assets");
            }
            else
            {
                Debug.Log("Everything is already up to date. No prefab asset objects registered.");
            }
        }


        private static Hash128 HashGameObject(GameObject gameObject)
        {
            var hash = new Hash128();

            var goToProcess = new Queue<GameObject>();
            goToProcess.Enqueue(gameObject);

            while (goToProcess.Count > 0)
            {
                var processingGo = goToProcess.Dequeue();
                hash.Append(processingGo.name);
                hash.Append(processingGo.tag);
                hash.Append(processingGo.layer);
                hash.Append(processingGo.activeSelf ? 1 : 0);

                for (int i = 0; i < processingGo.transform.childCount; i++)
                {
                    goToProcess.Enqueue(processingGo.transform.GetChild(i).gameObject);
                }
            }

            foreach (var component in gameObject.GetComponentsInChildren<Component>())
            {
                hash.Append(EditorJsonUtility.ToJson(component));
            }

            return hash;
        }

        private HashSet<ushort> GetUsedPrefabIds(string prefabsPath)
        {
            var cachedPrefabGuids = AssetDatabase.FindAssets("t:prefab", new[] { prefabsPath });

            _lastUsedTrackedObjectPrefabId = 0;
            var usedPrefabIds = new HashSet<ushort>();
            foreach (var cachedPrefabGuid in cachedPrefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(cachedPrefabGuid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (ushort.TryParse(go.name, out ushort prefabId))
                {
                    if (usedPrefabIds.Contains(prefabId))
                    {
                        InsightUtility.LogError($"Multiple assets in cache with id {prefabId}");
                    }
                    else
                    {
                        usedPrefabIds.Add(prefabId);
                    }
                }
                else
                {
                    InsightUtility.LogError($"cached prefab at path{path} should have a ushort name");
                }
            }

            return usedPrefabIds;
        }

        private List<(Hash128, List<(ushort, Hash128)>)> ReadHashesFile(string fileName)
        {
            var prefabsPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            var hashesPath = Path.Combine(prefabsPath, $"{fileName}.txt");
            string[] currentLines = new string[0];
            if (File.Exists(hashesPath))
            {
                currentLines = File.ReadAllLines(hashesPath);
            }

            var assetHashes = new List<(Hash128, List<(ushort, Hash128)>)>();
            for (int i = 0; i < currentLines.Length; i++)
            {
                var parts = currentLines[i].Split(' ');
                if (parts.Length >= 3)
                {
                    try
                    {
                        var hash = Hash128.Parse(parts[0]);
                        var hashIds = new List<(ushort, Hash128)>();
                        for (int j = 1; j < parts.Length; j += 2)
                        {
                            hashIds.Add((ushort.Parse(parts[j]), Hash128.Parse(parts[j + 1])));
                        }

                        assetHashes.Add((hash, hashIds));
                    }
                    catch
                    {
                        //
                    }
                }
            }

            return assetHashes;
        }

        private void WriteHashesFile(List<(Hash128, List<(ushort, Hash128)>)> hashes, string fileName)
        {
            var prefabsPath = Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            var lines = new List<string>();
            for (int i = 0; i < hashes.Count; i++)
            {
                var hash = hashes[i].Item1.ToString();
                var ids = hashes[i].Item2;
                var line = hash;
                for (int j = 0; j < ids.Count; j++)
                {
                    line += " " + ids[j].Item1 + " " + ids[j].Item2;
                }

                lines.Add(line);
            }

            var hashesPath = Path.Combine(prefabsPath, $"{fileName}.txt");
            File.WriteAllLines(hashesPath, lines);
        }

        private static List<string> GetAllPrefabsWithDependency(string dependencyPath)
        {
            var prefabPaths = new List<string>();
            var prefabGUIDs = AssetDatabase.FindAssets("t:prefab");
            foreach (var prefabGUID in prefabGUIDs)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
                var partOfInsight = Path.GetFullPath(prefabPath).Split(Path.DirectorySeparatorChar)
                    .Any((pathPart) => pathPart == "InsightDesk" || pathPart == "InsightDeskCache");
                if (partOfInsight)
                {
                    continue;
                }

                var prefabDependencyPaths = AssetDatabase.GetDependencies(prefabPath, false);
                foreach (var prefabDependencyPath in prefabDependencyPaths)
                {
                    if (prefabDependencyPath == dependencyPath)
                    {
                        prefabPaths.Add(prefabPath);
                    }
                }
            }

            return prefabPaths;
        }

        private static ushort GetPrefabIdOrCreate(InsightTrackObject insightTrackObject, HashSet<ushort> usedPrefabIds, bool forceChangeId = false)
        {
            if (insightTrackObject.prefabId != 0 && !forceChangeId)
            {
                return insightTrackObject.prefabId;
            }

            // Check for reserved prefab IDs based on specific components
            if (insightTrackObject.GetComponent<InsightTrackCenterEye>() != null)
            {
                insightTrackObject.prefabId = 1; // Camera
                return insightTrackObject.prefabId;
            }
            else if (insightTrackObject.GetComponent<InsightTrackHandAnchor>() != null)
            {
                var handAnchor = insightTrackObject.GetComponent<InsightTrackHandAnchor>();
                if (handAnchor.hand == InsightTrackHandAnchor.Hand.Left)
                {
                    insightTrackObject.prefabId = 2; // Left hand
                }
                else if (handAnchor.hand == InsightTrackHandAnchor.Hand.Right)
                {
                    insightTrackObject.prefabId = 3; // Right hand
                }
                return insightTrackObject.prefabId;
            }

            // Original logic for assigning new IDs
            var errorMsg = $"Out of prefabIds. Cannot track more than {ushort.MaxValue} types of objects. Try deleting the InsightDeskCache for this version ({Application.version}) then re-registering, or removing InsightTrackObject from some objects.";
            if (_lastUsedTrackedObjectPrefabId == ushort.MaxValue)
            {
                InsightUtility.LogError(errorMsg);
            }

            _lastUsedTrackedObjectPrefabId++;
            while (usedPrefabIds.Contains(_lastUsedTrackedObjectPrefabId) || ReservedPrefabIds.ContainsValue(_lastUsedTrackedObjectPrefabId))
            {
                if (_lastUsedTrackedObjectPrefabId == ushort.MaxValue)
                {
                    InsightUtility.LogError(errorMsg);
                }

                _lastUsedTrackedObjectPrefabId++;
            }

            Undo.RecordObject(insightTrackObject, "Set prefabId");
            insightTrackObject.prefabId = _lastUsedTrackedObjectPrefabId;
            PrefabUtility.RecordPrefabInstancePropertyModifications(insightTrackObject);

            return insightTrackObject.prefabId;
        }


        private static HashSet<System.Type> GetDependencies(GameObject go)
        {
            var dependencies = new HashSet<System.Type>();

            var goComponents = go.GetComponentsInChildren<Component>();
            foreach (var goComponent in goComponents)
            {
                var componentType = goComponent.GetType();

                var attributes = System.Attribute.GetCustomAttributes(componentType);
                foreach (var attribute in attributes)
                {
                    if (attribute is RequireComponent)
                    {
                        var requireComponentAttribute = (RequireComponent)attribute;

                        if (requireComponentAttribute.m_Type0 != null &&
                            !dependencies.Contains(requireComponentAttribute.m_Type0))
                        {
                            dependencies.Add(requireComponentAttribute.m_Type0);
                        }

                        if (requireComponentAttribute.m_Type1 != null &&
                            !dependencies.Contains(requireComponentAttribute.m_Type1))
                        {
                            dependencies.Add(requireComponentAttribute.m_Type1);
                        }

                        if (requireComponentAttribute.m_Type2 != null &&
                            !dependencies.Contains(requireComponentAttribute.m_Type2))
                        {
                            dependencies.Add(requireComponentAttribute.m_Type2);
                        }
                    }
                }
            }

            return dependencies;
        }

        private static bool IsDependency(System.Type componentType, HashSet<System.Type> dependencies)
        {
            foreach (var dependency in dependencies)
            {
                if (componentType.IsAssignableFrom(dependency))
                {
                    return true;
                }
            }

            return false;
        }

        private static void StripComponentsExceptRendering(GameObject go, string pathOrNameForErrorLog)
        {
            int numComponents = 0;
            while (go.GetComponent<InsightTrackObject>())
            {
                numComponents++;
                DestroyImmediate(go.GetComponent<InsightTrackObject>());
            }

            if (numComponents > 1)
            {
                InsightUtility.LogError(
                    $"{pathOrNameForErrorLog} has {numComponents} InsightTrackObject scripts. There should only be 1.");
            }

            foreach (var insightTrackObject in go.GetComponentsInChildren<InsightTrackObject>())
            {
                if (insightTrackObject && insightTrackObject.gameObject)
                {
                    DestroyImmediate(insightTrackObject.gameObject);
                }
                else
                {
                    Debug.LogWarning(
                        "Child InsightTrackObject does not exist. Where multiple InsightTrackObject on the same object? This can also be caused by other things.");
                }
            }

            var attempts = 1000;

            var skippedDependency = false;
            do
            {
                if (attempts == 0)
                {
                    InsightUtility.LogError(
                        "Failed to delete scripts within 1000 iterations. Do you have a RequireComponent dependency cycle?");
                    break;
                }

                attempts--;
                skippedDependency = false;
                var dependencies = GetDependencies(go);
                var prefabComponents = go.GetComponentsInChildren<Component>(true);
                foreach (var prefabComponent in prefabComponents)
                {
                    var componentType = prefabComponent.GetType();

                    if (componentType.Namespace != null && componentType.Namespace.Contains("Autohand"))
                    {
                        continue;
                    }

                    var allowedTypes = new[]
                    {
                        typeof(Transform),
                        typeof(RectTransform),
                        typeof(Camera),
                        typeof(MeshRenderer),
                        typeof(SkinnedMeshRenderer),
                        typeof(MeshFilter),
                        typeof(TextMesh),
                        typeof(TextMeshPro),
                        typeof(TextMeshProUGUI),

                        typeof(ParticleSystem),
                        typeof(ParticleSystemRenderer),

                        typeof(Terrain),

                        typeof(Canvas),
                        typeof(CanvasRenderer),
                        typeof(AspectRatioFitter),
                        typeof(Canvas),
                        typeof(CanvasGroup),
                        typeof(CanvasScaler),
                        typeof(ContentSizeFitter),
                        typeof(GridLayoutGroup),
                        typeof(HorizontalLayoutGroup),
                        typeof(LayoutElement),
                        typeof(VerticalLayoutGroup),
                        typeof(Dropdown),
                        typeof(TMP_Dropdown),
                        typeof(Image),
                        typeof(InputField),
                        typeof(Mask),
                        typeof(RawImage),
                        typeof(RectMask2D),
                        typeof(ScrollRect),
                        typeof(Scrollbar),
                        typeof(Slider),
                        typeof(Outline),
                        typeof(PositionAsUV1),
                        typeof(Shadow),
                        typeof(Text),
                        typeof(TMP_InputField),
                        typeof(TextMeshProUGUI),
                        typeof(Toggle),
                        typeof(ToggleGroup),

                        typeof(Animator),

                        //typeof(),
                        //typeof();
                    };
                    var isAllowedType = false;
                    foreach (var allowedType in allowedTypes)
                    {
                        if (allowedType.IsAssignableFrom(componentType))
                        {
                            isAllowedType = true;
                            break;
                        }
                    }

                    // if (typeof(ParticleSystem).IsAssignableFrom(componentType))
                    // {
                    //     var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //     DestroyImmediate(sphere.GetComponent<Collider>());
                    //     sphere.transform.parent = prefabComponent.transform;
                    //     sphere.name = "particle_placeholder";
                    //     sphere.transform.localPosition = Vector3.zero;
                    //     sphere.transform.localRotation = Quaternion.identity;
                    //     sphere.transform.localScale = Vector3.one * 0.1f;
                    //     sphere.SetActive(false);
                    // }

                    if (!isAllowedType)
                    {
                        if (IsDependency(componentType, dependencies))
                        {
                            skippedDependency = true;
                        }
                        else
                        {
                            DestroyImmediate(prefabComponent);
                        }
                    }
                }
            } while (skippedDependency);
        }

        private void PackageAndUploadAssets()
        {
            PackageAssets();
            EditorCoroutineUtility.StartCoroutine(UploadPackagedAssets(), this);
        }

        private void PackageAssets()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                return;
            }

            Debug.Log("packaging assets...");

            var prefabsFolderPath =
                Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            var packageFolderPath = Path.Combine("Assets", "InsightDeskCache", "PackagedPrefabs", Application.version);
            Directory.CreateDirectory(prefabsFolderPath);
            Directory.CreateDirectory(packageFolderPath);

            AssetDatabase.ExportPackage(prefabsFolderPath,
                Path.Combine(packageFolderPath, "InsightReplayAssets.unitypackage"),
                ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

            Debug.Log("done packaging assets.");
        }

        private IEnumerator UploadPackagedAssets()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                yield break;
            }

            var packagePath =
                Path.Combine("Assets", "InsightDeskCache", "PackagedPrefabs", Application.version,
                    "InsightReplayAssets.unitypackage");
            var data = File.ReadAllBytes(packagePath);
            if (_packageUploadUwr != null)
            {
                _packageUploadUwr.Abort();
                yield return null;
            }

            using (_packageUploadUwr = UnityWebRequest.Put(
                       $"{TrackingManager.ApiInputBase}/asset_package?app_version={Application.version}&company_name={Application.companyName}&product_name={Application.productName}",
                       data))
            {
                Debug.Log("uploading assets...");
                var operation = _packageUploadUwr.SendWebRequest();

                double lastLogTime = 0;

                while (!operation.isDone)
                {
                    if (EditorApplication.timeSinceStartup - lastLogTime > 0.5f)
                    {
                        lastLogTime = EditorApplication.timeSinceStartup;
                        Debug.Log($"upload progress: {Math.Floor(_packageUploadUwr.uploadProgress * 100f)}%");
                    }

                    yield return null;
                }

                if (_packageUploadUwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(_packageUploadUwr.error);
                }
                else
                {
                    Debug.Log("Upload complete!");
                }
            }
        }

        private void ConvertShaders()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                return;
            }

            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            var prefabsFolderPath =
                Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            Directory.CreateDirectory(prefabsFolderPath);

            var standardShader = Shader.Find("Standard");

            var assets = AssetDatabase.FindAssets("t:prefab", new[] { prefabsFolderPath });
            for (int i = 0; i < assets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (HasUnsupportedShader(prefab))
                {
                    ConvertPrefabShaders(path, standardShader);
                }
            }

            AssetDatabase.SaveAssets();
        }

        private static bool HasUnsupportedShader(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var materials = renderers[i].sharedMaterials;
                for (int j = 0; j < materials.Length; j++)
                {
                    if (!materials[j].shader.isSupported)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void ConvertPrefabShaders(string prefabPath, Shader standardShader)
        {
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var prefabRoot = editingScope.prefabContentsRoot;
                var renderers = prefabRoot.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    var materials = renderers[i].sharedMaterials;
                    for (int j = 0; j < materials.Length; j++)
                    {
                        if (!materials[j].shader.isSupported)
                        {
                            materials[j].shader = standardShader;
                        }
                    }
                }
            }
        }

        [Obsolete]
        private void BundleAndUploadAssets()
        {
            var bundleHashString = BundleAssets();
            EditorCoroutineUtility.StartCoroutine(UploadBundledAssets(bundleHashString), this);
        }

        [Obsolete]
        private string BundleAssets()
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                return "";
            }

            Debug.Log("bundling assets...");

            var prefabsFolderPath =
                Path.Combine("Assets", "InsightDeskCache", "TrackedObjectsPrefabs", Application.version);
            Directory.CreateDirectory(prefabsFolderPath);

            var buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = "insightreplayassets";

            var prefabPaths = Directory.GetFiles(prefabsFolderPath, "*.prefab");
            buildMap[0].assetNames = prefabPaths;

            var bundleFolderPath = Path.Combine("Assets", "InsightDeskCache", "BundledPrefabs", Application.version);
            Directory.CreateDirectory(bundleFolderPath);

            var bundleManifest = BuildPipeline.BuildAssetBundles(bundleFolderPath, buildMap,
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle,
                BuildTarget.WebGL);
            var bundleHashString = bundleManifest.GetAssetBundleHash("insightreplayassets").ToString();
            Debug.Log($"bundle hash: {bundleHashString}");

            Debug.Log("done bundling assets.");

            return bundleHashString;
        }

        private IEnumerator UploadBundledAssets(string bundleHashString)
        {
            if (Application.isPlaying)
            {
                InsightUtility.LogError("Not allowed in play mode");
                yield break;
            }

            var bundlePath =
                Path.Combine("Assets", "InsightDeskCache", "BundledPrefabs", Application.version,
                    "insightreplayassets");
            var data = File.ReadAllBytes(bundlePath);
            if (_bundleUploadUwr != null)
            {
                _bundleUploadUwr.Abort();
                yield return null;
            }

            using (_bundleUploadUwr = UnityWebRequest.Put(
                       $"{TrackingManager.ApiQueryBase}/asset_bundle?app_version={Application.version}&company_name={Application.companyName}&product_name={Application.productName}&bundle_hash={bundleHashString}",
                       data))
            {
                Debug.Log("uploading assets...");
                var operation = _bundleUploadUwr.SendWebRequest();

                double lastLogTime = 0;

                while (!operation.isDone)
                {
                    if (EditorApplication.timeSinceStartup - lastLogTime > 0.5f)
                    {
                        lastLogTime = EditorApplication.timeSinceStartup;
                        Debug.Log($"upload progress: {Math.Floor(_bundleUploadUwr.uploadProgress * 100f)}%");
                    }

                    yield return null;
                }

                if (_bundleUploadUwr.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(_bundleUploadUwr.error);
                }
                else
                {
                    Debug.Log("Upload complete!");
                }
            }
        }
    }
}
#endif