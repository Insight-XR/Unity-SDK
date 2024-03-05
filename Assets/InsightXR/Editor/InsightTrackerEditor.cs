using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;

public class InsightTrackerEditor : EditorWindow
{
    [MenuItem("InsightXR/Setup Environment")]
    private static void Init(){
        var window = (InsightTrackerEditor)EditorWindow.GetWindow(typeof(InsightTrackerEditor));
        window.Show();
        window.position = new Rect(0, 0, 400, 300);
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 260, 50), "Register Objects [In open scene]"))
        {
            // if (_registerSceneRoutine != null)
            // {
            //     EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
            // }
            // if (_registerAssetsRoutine != null)
            // {
            //     EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
            // }
            // _trackSceneProgress = 0;
            // _trackSceneTotal = 0;
            // _trackAssetsProgress = 0;
            // _trackAssetsTotal = 0;
            // _registerSceneRoutine = 
            EditorCoroutineUtility.StartCoroutine(RegisterTrackedSceneObjects(), this);
        }
        

    }
    private IEnumerator RegisterTrackedSceneObjects()
    {
        yield return null;
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();

        foreach(GameObject go in gameObjects){
            go.AddComponent<Component>();
        }

    }

    //     public class BonsaiManageTrackedObjectsEditor : EditorWindow
    //     {
    //         private const string AssetPrefabHashesFileName = "asset_prefab_hashes";
    //         private const string SceneObjectHashesFileNamePrefix = "scene_object_hashes_";

    //         private static ushort _lastUsedTrackedObjectPrefabId = 0;

    //         private static EditorCoroutine _registerSceneRoutine;
    //         private static EditorCoroutine _registerAssetsRoutine;

    //         private static int _trackSceneProgress = 0;
    //         private static int _trackSceneTotal = 0;
    //         private static int _trackAssetsProgress = 0;
    //         private static int _trackAssetsTotal = 0;

    //         private static UnityWebRequest _packageUploadUwr;
    //         private static UnityWebRequest _bundleUploadUwr;

    //         private static string _lastBundleHash = "";

    //         [MenuItem("Bonsai/Manage Tracked Objects")]
    //         static void Init()
    //         {
    //             var window =
    //                 (BonsaiManageTrackedObjectsEditor)EditorWindow.GetWindow(typeof(BonsaiManageTrackedObjectsEditor));
    //             window.Show();
    //             window.position = new Rect(20, 80, 400, 300);
    //         }

    //         void OnGUI()
    //         {
    //             if (GUI.Button(new Rect(10, 10, 260, 50), "Register Tracked Objects [In open scene]"))
    //             {
    //                 if (_registerSceneRoutine != null)
    //                 {
    //                     EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
    //                 }

    //                 if (_registerAssetsRoutine != null)
    //                 {
    //                     EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
    //                 }

    //                 _trackSceneProgress = 0;
    //                 _trackSceneTotal = 0;
    //                 _trackAssetsProgress = 0;
    //                 _trackAssetsTotal = 0;
    //                 _registerSceneRoutine = EditorCoroutineUtility.StartCoroutine(RegisterTrackedSceneObjects(), this);
    //             }

    //             if (GUI.Button(new Rect(10, 70, 260, 50), "Register Tracked Objects [In Assets]"))
    //             {
    //                 if (_registerSceneRoutine != null)
    //                 {
    //                     EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
    //                 }

    //                 if (_registerAssetsRoutine != null)
    //                 {
    //                     EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
    //                 }

    //                 _trackSceneProgress = 0;
    //                 _trackSceneTotal = 0;
    //                 _trackAssetsProgress = 0;
    //                 _trackAssetsTotal = 0;
    //                 _registerAssetsRoutine = EditorCoroutineUtility.StartCoroutine(RegisterTrackedAssetPrefabs(), this);
    //             }

    //             if (GUI.Button(new Rect(10, 130, 150, 50), "Cancel"))
    //             {
    //                 _trackSceneProgress = 0;
    //                 _trackSceneTotal = 0;
    //                 _trackAssetsProgress = 0;
    //                 _trackAssetsTotal = 0;
    //                 if (_registerSceneRoutine != null)
    //                 {
    //                     EditorCoroutineUtility.StopCoroutine(_registerSceneRoutine);
    //                     _registerSceneRoutine = null;
    //                 }

    //                 if (_registerAssetsRoutine != null)
    //                 {
    //                     EditorCoroutineUtility.StopCoroutine(_registerAssetsRoutine);
    //                     _registerAssetsRoutine = null;
    //                 }

    //                 if (_registerSceneRoutine != null || _registerAssetsRoutine != null)
    //                 {
    //                     Debug.Log("Register Tracked Objects was canceled");
    //                 }
    //             }

    //             var progressString = "Progress: (0/0)";
    //             if (_trackSceneTotal != 0)
    //             {
    //                 progressString = $"Progress: scene objects ({_trackSceneProgress}/{_trackSceneTotal})";
    //             }

    //             if (_trackAssetsTotal != 0)
    //             {
    //                 progressString = $"Progress: assets ({_trackAssetsProgress}/{_trackAssetsTotal})";
    //             }

    //             GUI.Label(new Rect(10, 170, 500, 50), progressString);

    //             if (GUI.Button(new Rect(10, 230, 200, 50), "Package Assets"))
    //             {
    //                 PackageAssets();
    //             }

    //             if (GUI.Button(new Rect(10, 290, 200, 50), "Upload Assets"))
    //             {
    //                 EditorCoroutineUtility.StartCoroutine(UploadPackagedAssets(), this);
    //             }

    //             if (GUI.Button(new Rect(10, 350, 150, 50), "Cancel"))
    //             {
    //                 if (_packageUploadUwr != null)
    //                 {
    //                     _packageUploadUwr.Abort();
    //                 }
    //             }

    // #if BONSAI
    //             // if (GUI.Button(new Rect(220, 450, 200, 50), "Convert Shaders"))
    //             // {
    //             //     ConvertShaders();
    //             // }

    //             if (GUI.Button(new Rect(10, 450, 200, 50), "Bundle Assets"))
    //             {
    //                 ConvertShaders();
    //                 _lastBundleHash = BundleAssets();
    //             }

    //             if (GUI.Button(new Rect(10, 510, 200, 50), "Upload Bundle"))
    //             {
    //                 if (_lastBundleHash != "")
    //                 {
    //                     EditorCoroutineUtility.StartCoroutine(UploadBundledAssets(_lastBundleHash), this);
    //                 }
    //             }

    //             if (GUI.Button(new Rect(10, 570, 200, 50), "Cancel"))
    //             {
    //                 _lastBundleHash = "";

    //                 if (_bundleUploadUwr != null)
    //                 {
    //                     _bundleUploadUwr.Abort();
    //                 }
    //             }
    // #endif
    //         }

    //         private IEnumerator RegisterTrackedSceneObjects()
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 yield break;
    //             }

    //             EditorSceneManager.SaveOpenScenes();
    //             AssetDatabase.SaveAssets();

    //             var scenePath = EditorSceneManager.GetActiveScene().path;
    //             var sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
    //             var sceneObjectHashesFileName = $"{SceneObjectHashesFileNamePrefix}{sceneGuid}";
    //             var currentSceneObjectHashes = ReadHashesFile(sceneObjectHashesFileName);
    //             var sceneObjectHashes = new List<(Hash128, List<(ushort, Hash128)>)>();

    //             var prefabsPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             Directory.CreateDirectory(prefabsPath);
    //             var usedPrefabIds = GetUsedPrefabIds(prefabsPath);

    //             var objects = FindObjectsOfType<BonsaiTrackObject>(true);
    //             _trackSceneTotal = objects.Length;
    //             var assetPrefabHashes = ReadHashesFile(AssetPrefabHashesFileName);
    //             var updatedAsset = false;
    //             var sceneObjectIds = new HashSet<ushort>();
    //             foreach (var foundObject in objects)
    //             {
    //                 if (PrefabUtility.IsPartOfAnyPrefab(foundObject))
    //                 {
    //                     var root = PrefabUtility.GetNearestPrefabInstanceRoot(foundObject);
    //                     if (root)
    //                     {
    //                         var overrides = PrefabUtility.GetObjectOverrides(root);
    //                         var hasNoOverrides = overrides.Count == 0 ||
    //                                              overrides.Count == 1 && overrides[0].instanceObject == root.transform;
    //                         var noAddedComponents = PrefabUtility.GetAddedComponents(root).Count == 0;
    //                         var noRemovedComponents = PrefabUtility.GetRemovedComponents(root).Count == 0;
    //                         var noAddedGameObjects = PrefabUtility.GetAddedGameObjects(root).Count == 0;
    //                         if (hasNoOverrides && noAddedComponents && noRemovedComponents && noAddedGameObjects)
    //                         {
    //                             _trackSceneProgress++;
    //                             continue;
    //                         }
    //                     }
    //                 }

    //                 var foundObjectHash = HashGameObject(foundObject.gameObject);

    //                 var copyHashesList = currentSceneObjectHashes.FindAll((value) => value.Item1 == foundObjectHash);
    //                 if (copyHashesList.Count > 0)
    //                 {
    //                     var allMatches = true;
    //                     var copyHashes = copyHashesList[0].Item2;
    //                     for (int i = 0; i < copyHashes.Count; i++)
    //                     {
    //                         var copyPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs",
    //                             Application.version,
    //                             $"{copyHashes[i].Item1}.prefab");
    //                         var copyHash = AssetDatabase.GetAssetDependencyHash(copyPath);
    //                         if (copyHash != copyHashes[i].Item2)
    //                         {
    //                             allMatches = false;
    //                         }
    //                     }

    //                     if (allMatches)
    //                     {
    //                         _trackSceneProgress++;
    //                         sceneObjectHashes.Add((foundObjectHash, copyHashes));
    //                         continue;
    //                     }
    //                 }

    //                 var idHashes = new List<(ushort, Hash128)>();

    //                 var prefabId = GetPrefabIdOrCreate(foundObject, usedPrefabIds);
    //                 var assetHashesHasId = false;
    //                 for (int i = 0; i < assetPrefabHashes.Count; i++)
    //                 {
    //                     var hashIds = assetPrefabHashes[i].Item2;
    //                     for (int j = 0; j < hashIds.Count; j++)
    //                     {
    //                         if (hashIds[j].Item1 == prefabId)
    //                         {
    //                             assetHashesHasId = true;
    //                         }
    //                     }
    //                 }

    //                 if (assetHashesHasId)
    //                 {
    //                     Debug.LogWarning(
    //                         $"({foundObject.name}) scene GameObject's BonsaiTrackObject Prefab Id has same id as previously processed prefab even though it does not match the prefab exactly. changing id.");
    //                     prefabId = GetPrefabIdOrCreate(foundObject, usedPrefabIds, true);
    //                 }

    //                 if (sceneObjectIds.Contains(prefabId))
    //                 {
    //                     Debug.LogWarning(
    //                         $"({foundObject.name}) scene GameObject's BonsaiTrackObject Prefab Id has same id another object in the scene even though it is not an unaltered prefab instance. changing id.");
    //                     prefabId = GetPrefabIdOrCreate(foundObject, usedPrefabIds, true);
    //                 }

    //                 sceneObjectIds.Add(prefabId);

    //                 var path = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version,
    //                     $"{prefabId}.prefab");
    //                 var copy = Instantiate(foundObject.gameObject);
    //                 copy.name = foundObject.name;
    //                 copy.transform.position = foundObject.transform.position;
    //                 copy.transform.rotation = foundObject.transform.rotation;
    //                 copy.transform.localScale = foundObject.transform.lossyScale;
    //                 PrefabUtility.SaveAsPrefabAsset(copy, path);
    //                 DestroyImmediate(copy);

    //                 using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
    //                 {
    //                     var prefabRoot = editingScope.prefabContentsRoot;
    //                     StripComponentsExceptRendering(prefabRoot, foundObject.name);
    //                 }

    //                 var bonsaiCachePrefabHash = AssetDatabase.GetAssetDependencyHash(path);
    //                 idHashes.Add((prefabId, bonsaiCachePrefabHash));
    //                 sceneObjectHashes.Add((foundObjectHash, idHashes));

    //                 _trackSceneProgress++;
    //                 updatedAsset = true;
    //                 yield return null;
    //             }

    //             WriteHashesFile(sceneObjectHashes, sceneObjectHashesFileName);

    //             AssetDatabase.SaveAssets();

    //             if (updatedAsset)
    //             {
    //                 Debug.Log("Registered all tracked objects in currently opened scene");
    //             }
    //             else
    //             {
    //                 Debug.Log("Everything is already up to date. No scene objects registered.");
    //             }
    //         }

    //         private IEnumerator RegisterTrackedAssetPrefabs()
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 yield break;
    //             }

    //             EditorSceneManager.SaveOpenScenes();
    //             AssetDatabase.SaveAssets();

    //             var prefabsPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             Directory.CreateDirectory(prefabsPath);
    //             var usedPrefabIds = GetUsedPrefabIds(prefabsPath);

    //             var assetGuids = AssetDatabase.FindAssets("BonsaiTrackObject");
    //             if (assetGuids.Length == 0)
    //             {
    //                 BonsaiUtility.LogError("Could not find BonsaiTrackObject script");
    //                 yield break;
    //             }

    //             if (assetGuids.Length > 1)
    //             {
    //                 Debug.LogWarning("Found multiple scripts with name BonsaiTrackObject");
    //             }

    //             var scenesHashes = new List<List<(Hash128, List<(ushort, Hash128)>)>>();
    //             var prefabPathFiles = Directory.GetFiles(prefabsPath);
    //             for (int i = 0; i < prefabPathFiles.Length; i++)
    //             {
    //                 var fileName = Path.GetFileNameWithoutExtension(prefabPathFiles[i]);
    //                 var fileExt = Path.GetExtension(prefabPathFiles[i]);
    //                 if (fileName.StartsWith(SceneObjectHashesFileNamePrefix) && fileExt == ".txt")
    //                 {
    //                     var sceneObjectHashes = ReadHashesFile(fileName);
    //                     scenesHashes.Add(sceneObjectHashes);
    //                 }
    //             }

    //             var currentAssetPrefabHashes = ReadHashesFile(AssetPrefabHashesFileName);
    //             var assetPrefabHashes = new List<(Hash128, List<(ushort, Hash128)>)>();

    //             var bonsaiTrackObjectPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);

    //             var prefabPaths = GetAllPrefabsWithDependency(bonsaiTrackObjectPath);
    //             _trackAssetsTotal = prefabPaths.Count;
    //             var assetIds = new HashSet<ushort>();
    //             var updatedAsset = false;
    //             foreach (var path in prefabPaths)
    //             {
    //                 var sourceHash = AssetDatabase.GetAssetDependencyHash(path);
    //                 var copyHashesList = currentAssetPrefabHashes.FindAll((value) => value.Item1 == sourceHash);
    //                 if (copyHashesList.Count > 0)
    //                 {
    //                     var allMatches = true;
    //                     var copyHashes = copyHashesList[0].Item2;
    //                     for (int i = 0; i < copyHashes.Count; i++)
    //                     {
    //                         var copyPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs",
    //                             Application.version,
    //                             $"{copyHashes[i].Item1}.prefab");
    //                         var copyHash = AssetDatabase.GetAssetDependencyHash(copyPath);
    //                         if (copyHash != copyHashes[i].Item2)
    //                         {
    //                             allMatches = false;
    //                         }
    //                     }

    //                     if (allMatches)
    //                     {
    //                         _trackAssetsProgress++;
    //                         assetPrefabHashes.Add((sourceHash, copyHashes));
    //                         continue;
    //                     }
    //                 }

    //                 var idHashes = new List<(ushort, Hash128)>();
    //                 using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
    //                 {
    //                     var prefabRoot = editingScope.prefabContentsRoot;

    //                     foreach (var prefabBonsaiTrackObject in prefabRoot.GetComponentsInChildren<BonsaiTrackObject>())
    //                     {
    //                         var prefabId = GetPrefabIdOrCreate(prefabBonsaiTrackObject, usedPrefabIds);
    //                         Debug.Log(prefabId);

    //                         var sceneObjectHashesHasId = false;
    //                         for (int n = 0; n < scenesHashes.Count; n++)
    //                         {
    //                             for (int i = 0; i < scenesHashes[n].Count; i++)
    //                             {
    //                                 var hashIds = scenesHashes[n][i].Item2;
    //                                 for (int j = 0; j < hashIds.Count; j++)
    //                                 {
    //                                     if (hashIds[j].Item1 == prefabId)
    //                                     {
    //                                         sceneObjectHashesHasId = true;
    //                                     }
    //                                 }
    //                             }
    //                         }

    //                         if (sceneObjectHashesHasId)
    //                         {
    //                             Debug.LogWarning(
    //                                 $"prefab with path {path} has same id as previously processed scene object. changing id.");
    //                             prefabId = GetPrefabIdOrCreate(prefabBonsaiTrackObject, usedPrefabIds, true);
    //                         }

    //                         if (assetIds.Contains(prefabId))
    //                         {
    //                             Debug.LogWarning(
    //                                 $"prefab with path {path} has same id as previously processed prefab. changing id.");
    //                             prefabId = GetPrefabIdOrCreate(prefabBonsaiTrackObject, usedPrefabIds, true);
    //                         }

    //                         assetIds.Add(prefabId);

    //                         var copy = Instantiate(prefabBonsaiTrackObject.gameObject);
    //                         // unpack copy?
    //                         copy.name = prefabBonsaiTrackObject.name;
    //                         var copyPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs",
    //                             Application.version,
    //                             $"{prefabId}.prefab");
    //                         PrefabUtility.SaveAsPrefabAsset(copy, copyPath);
    //                         DestroyImmediate(copy);

    //                         using (var copyEditingScope = new PrefabUtility.EditPrefabContentsScope(copyPath))
    //                         {
    //                             var copyRoot = copyEditingScope.prefabContentsRoot;
    //                             StripComponentsExceptRendering(copyRoot, path);
    //                         }

    //                         var bonsaiCachePrefabHash = AssetDatabase.GetAssetDependencyHash(copyPath);
    //                         idHashes.Add((prefabId, bonsaiCachePrefabHash));

    //                         yield return null;
    //                     }
    //                 }

    //                 assetPrefabHashes.Add((AssetDatabase.GetAssetDependencyHash(path), idHashes));

    //                 _trackAssetsProgress++;
    //                 updatedAsset = true;
    //                 yield return null;
    //             }

    //             WriteHashesFile(assetPrefabHashes, AssetPrefabHashesFileName);

    //             AssetDatabase.SaveAssets();

    //             if (updatedAsset)
    //             {
    //                 Debug.Log("Registered all tracked prefabs in assets");
    //             }
    //             else
    //             {
    //                 Debug.Log("Everything is already up to date. No prefab asset objects registered.");
    //             }
    //         }

    //         private static Hash128 HashGameObject(GameObject gameObject)
    //         {
    //             var hash = new Hash128();

    //             var goToProcess = new Queue<GameObject>();
    //             goToProcess.Enqueue(gameObject);

    //             while (goToProcess.Count > 0)
    //             {
    //                 var processingGo = goToProcess.Dequeue();
    //                 hash.Append(processingGo.name);
    //                 hash.Append(processingGo.tag);
    //                 hash.Append(processingGo.layer);
    //                 hash.Append(processingGo.activeSelf ? 1 : 0);

    //                 for (int i = 0; i < processingGo.transform.childCount; i++)
    //                 {
    //                     goToProcess.Enqueue(processingGo.transform.GetChild(i).gameObject);
    //                 }
    //             }

    //             foreach (var component in gameObject.GetComponentsInChildren<Component>())
    //             {
    //                 hash.Append(EditorJsonUtility.ToJson(component));
    //             }

    //             return hash;
    //         }

    //         private HashSet<ushort> GetUsedPrefabIds(string prefabsPath)
    //         {
    //             var cachedPrefabGuids = AssetDatabase.FindAssets("t:prefab", new[] { prefabsPath });

    //             _lastUsedTrackedObjectPrefabId = 0;
    //             var usedPrefabIds = new HashSet<ushort>();
    //             foreach (var cachedPrefabGuid in cachedPrefabGuids)
    //             {
    //                 var path = AssetDatabase.GUIDToAssetPath(cachedPrefabGuid);
    //                 var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    //                 if (ushort.TryParse(go.name, out ushort prefabId))
    //                 {
    //                     if (usedPrefabIds.Contains(prefabId))
    //                     {
    //                         BonsaiUtility.LogError($"Multiple assets in cache with id {prefabId}");
    //                     }
    //                     else
    //                     {
    //                         usedPrefabIds.Add(prefabId);
    //                     }
    //                 }
    //                 else
    //                 {
    //                     BonsaiUtility.LogError($"cached prefab at path{path} should have a ushort name");
    //                 }
    //             }

    //             return usedPrefabIds;
    //         }

    //         private List<(Hash128, List<(ushort, Hash128)>)> ReadHashesFile(string fileName)
    //         {
    //             var prefabsPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             var hashesPath = Path.Combine(prefabsPath, $"{fileName}.txt");
    //             string[] currentLines = new string[0];
    //             if (File.Exists(hashesPath))
    //             {
    //                 currentLines = File.ReadAllLines(hashesPath);
    //             }

    //             var assetHashes = new List<(Hash128, List<(ushort, Hash128)>)>();
    //             for (int i = 0; i < currentLines.Length; i++)
    //             {
    //                 var parts = currentLines[i].Split(' ');
    //                 if (parts.Length >= 3)
    //                 {
    //                     try
    //                     {
    //                         var hash = Hash128.Parse(parts[0]);
    //                         var hashIds = new List<(ushort, Hash128)>();
    //                         for (int j = 1; j < parts.Length; j += 2)
    //                         {
    //                             hashIds.Add((ushort.Parse(parts[j]), Hash128.Parse(parts[j + 1])));
    //                         }

    //                         assetHashes.Add((hash, hashIds));
    //                     }
    //                     catch
    //                     {
    //                         //
    //                     }
    //                 }
    //             }

    //             return assetHashes;
    //         }

    //         private void WriteHashesFile(List<(Hash128, List<(ushort, Hash128)>)> hashes, string fileName)
    //         {
    //             var prefabsPath = Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             var lines = new List<string>();
    //             for (int i = 0; i < hashes.Count; i++)
    //             {
    //                 var hash = hashes[i].Item1.ToString();
    //                 var ids = hashes[i].Item2;
    //                 var line = hash;
    //                 for (int j = 0; j < ids.Count; j++)
    //                 {
    //                     line += " " + ids[j].Item1 + " " + ids[j].Item2;
    //                 }

    //                 lines.Add(line);
    //             }

    //             var hashesPath = Path.Combine(prefabsPath, $"{fileName}.txt");
    //             File.WriteAllLines(hashesPath, lines);
    //         }

    //         private static List<string> GetAllPrefabsWithDependency(string dependencyPath)
    //         {
    //             var prefabPaths = new List<string>();
    //             var prefabGUIDs = AssetDatabase.FindAssets("t:prefab");
    //             foreach (var prefabGUID in prefabGUIDs)
    //             {
    //                 var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
    //                 var partOfBonsai = Path.GetFullPath(prefabPath).Split(Path.DirectorySeparatorChar)
    //                     .Any((pathPart) => pathPart == "BonsaiDesk" || pathPart == "BonsaiDeskCache");
    //                 if (partOfBonsai)
    //                 {
    //                     continue;
    //                 }

    //                 var prefabDependencyPaths = AssetDatabase.GetDependencies(prefabPath, false);
    //                 foreach (var prefabDependencyPath in prefabDependencyPaths)
    //                 {
    //                     if (prefabDependencyPath == dependencyPath)
    //                     {
    //                         prefabPaths.Add(prefabPath);
    //                     }
    //                 }
    //             }

    //             return prefabPaths;
    //         }

    //         private static ushort GetPrefabIdOrCreate(BonsaiTrackObject bonsaiTrackObject, HashSet<ushort> usedPrefabIds,
    //             bool forceChangeId = false)
    //         {
    //             if (bonsaiTrackObject.prefabId != 0 && !forceChangeId)
    //             {
    //                 return bonsaiTrackObject.prefabId;
    //             }

    //             var errorMsg =
    //                 $"Out of prefabIds. Cannot track more than {ushort.MaxValue} types of objects. Try deleting the BonsaiDeskCache for this version ({Application.version}) then re-registering, or removing BonsaiTrackObject from some objects.";
    //             if (_lastUsedTrackedObjectPrefabId == ushort.MaxValue)
    //             {
    //                 BonsaiUtility.LogError(errorMsg);
    //             }

    //             _lastUsedTrackedObjectPrefabId++;
    //             while (usedPrefabIds.Contains(_lastUsedTrackedObjectPrefabId))
    //             {
    //                 if (_lastUsedTrackedObjectPrefabId == ushort.MaxValue)
    //                 {
    //                     BonsaiUtility.LogError(errorMsg);
    //                 }

    //                 _lastUsedTrackedObjectPrefabId++;
    //             }

    //             Undo.RecordObject(bonsaiTrackObject, "Set prefabId");
    //             bonsaiTrackObject.prefabId = _lastUsedTrackedObjectPrefabId;
    //             PrefabUtility.RecordPrefabInstancePropertyModifications(bonsaiTrackObject);

    //             return bonsaiTrackObject.prefabId;
    //         }

    //         private static HashSet<System.Type> GetDependencies(GameObject go)
    //         {
    //             var dependencies = new HashSet<System.Type>();

    //             var goComponents = go.GetComponentsInChildren<Component>();
    //             foreach (var goComponent in goComponents)
    //             {
    //                 var componentType = goComponent.GetType();

    //                 var attributes = System.Attribute.GetCustomAttributes(componentType);
    //                 foreach (var attribute in attributes)
    //                 {
    //                     if (attribute is RequireComponent)
    //                     {
    //                         var requireComponentAttribute = (RequireComponent)attribute;

    //                         if (requireComponentAttribute.m_Type0 != null &&
    //                             !dependencies.Contains(requireComponentAttribute.m_Type0))
    //                         {
    //                             dependencies.Add(requireComponentAttribute.m_Type0);
    //                         }

    //                         if (requireComponentAttribute.m_Type1 != null &&
    //                             !dependencies.Contains(requireComponentAttribute.m_Type1))
    //                         {
    //                             dependencies.Add(requireComponentAttribute.m_Type1);
    //                         }

    //                         if (requireComponentAttribute.m_Type2 != null &&
    //                             !dependencies.Contains(requireComponentAttribute.m_Type2))
    //                         {
    //                             dependencies.Add(requireComponentAttribute.m_Type2);
    //                         }
    //                     }
    //                 }
    //             }

    //             return dependencies;
    //         }

    //         private static bool IsDependency(System.Type componentType, HashSet<System.Type> dependencies)
    //         {
    //             foreach (var dependency in dependencies)
    //             {
    //                 if (componentType.IsAssignableFrom(dependency))
    //                 {
    //                     return true;
    //                 }
    //             }

    //             return false;
    //         }

    //         private static void StripComponentsExceptRendering(GameObject go, string pathOrNameForErrorLog)
    //         {
    //             int numComponents = 0;
    //             while (go.GetComponent<BonsaiTrackObject>())
    //             {
    //                 numComponents++;
    //                 DestroyImmediate(go.GetComponent<BonsaiTrackObject>());
    //             }

    //             if (numComponents > 1)
    //             {
    //                 BonsaiUtility.LogError(
    //                     $"{pathOrNameForErrorLog} has {numComponents} BonsaiTrackObject scripts. There should only be 1.");
    //             }

    //             foreach (var bonsaiTrackObject in go.GetComponentsInChildren<BonsaiTrackObject>())
    //             {
    //                 if (bonsaiTrackObject && bonsaiTrackObject.gameObject)
    //                 {
    //                     DestroyImmediate(bonsaiTrackObject.gameObject);
    //                 }
    //                 else
    //                 {
    //                     Debug.LogWarning(
    //                         "Child BonsaiTrackObject does not exist. Where multiple BonsaiTrackObject on the same object? This can also be caused by other things.");
    //                 }
    //             }

    //             var attempts = 1000;

    //             var skippedDependency = false;
    //             do
    //             {
    //                 if (attempts == 0)
    //                 {
    //                     BonsaiUtility.LogError(
    //                         "Failed to delete scripts within 1000 iterations. Do you have a RequireComponent dependency cycle?");
    //                     break;
    //                 }

    //                 attempts--;
    //                 skippedDependency = false;
    //                 var dependencies = GetDependencies(go);
    //                 var prefabComponents = go.GetComponentsInChildren<Component>(true);
    //                 foreach (var prefabComponent in prefabComponents)
    //                 {
    //                     var componentType = prefabComponent.GetType();

    //                     var allowedTypes = new[]
    //                     {
    //                         typeof(Transform),
    //                         typeof(RectTransform),

    //                         typeof(MeshRenderer),
    //                         typeof(SkinnedMeshRenderer),
    //                         typeof(MeshFilter),
    //                         typeof(TextMesh),
    //                         typeof(TextMeshPro),

    //                         typeof(ParticleSystem),
    //                         typeof(ParticleSystemRenderer),

    //                         typeof(Terrain),

    //                         typeof(Canvas),
    //                         typeof(CanvasRenderer),
    //                         typeof(AspectRatioFitter),
    //                         typeof(Canvas),
    //                         typeof(CanvasGroup),
    //                         typeof(CanvasScaler),
    //                         typeof(ContentSizeFitter),
    //                         typeof(GridLayoutGroup),
    //                         typeof(HorizontalLayoutGroup),
    //                         typeof(LayoutElement),
    //                         typeof(VerticalLayoutGroup),
    //                         typeof(Dropdown),
    //                         typeof(TMP_Dropdown),
    //                         typeof(Image),
    //                         typeof(InputField),
    //                         typeof(Mask),
    //                         typeof(RawImage),
    //                         typeof(RectMask2D),
    //                         typeof(ScrollRect),
    //                         typeof(Scrollbar),
    //                         typeof(Slider),
    //                         typeof(Outline),
    //                         typeof(PositionAsUV1),
    //                         typeof(Shadow),
    //                         typeof(Text),
    //                         typeof(TMP_InputField),
    //                         typeof(TextMeshProUGUI),
    //                         typeof(Toggle),
    //                         typeof(ToggleGroup),

    //                         typeof(Animator),
    //                     };
    //                     var isAllowedType = false;
    //                     foreach (var allowedType in allowedTypes)
    //                     {
    //                         if (allowedType.IsAssignableFrom(componentType))
    //                         {
    //                             isAllowedType = true;
    //                             break;
    //                         }
    //                     }

    //                     // if (typeof(ParticleSystem).IsAssignableFrom(componentType))
    //                     // {
    //                     //     var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //                     //     DestroyImmediate(sphere.GetComponent<Collider>());
    //                     //     sphere.transform.parent = prefabComponent.transform;
    //                     //     sphere.name = "particle_placeholder";
    //                     //     sphere.transform.localPosition = Vector3.zero;
    //                     //     sphere.transform.localRotation = Quaternion.identity;
    //                     //     sphere.transform.localScale = Vector3.one * 0.1f;
    //                     //     sphere.SetActive(false);
    //                     // }

    //                     if (!isAllowedType)
    //                     {
    //                         if (IsDependency(componentType, dependencies))
    //                         {
    //                             skippedDependency = true;
    //                         }
    //                         else
    //                         {
    //                             DestroyImmediate(prefabComponent);
    //                         }
    //                     }
    //                 }
    //             } while (skippedDependency);
    //         }

    //         private void PackageAndUploadAssets()
    //         {
    //             PackageAssets();
    //             EditorCoroutineUtility.StartCoroutine(UploadPackagedAssets(), this);
    //         }

    //         private void PackageAssets()
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 return;
    //             }

    //             Debug.Log("packaging assets...");

    //             var prefabsFolderPath =
    //                 Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             var packageFolderPath = Path.Combine("Assets", "BonsaiDeskCache", "PackagedPrefabs", Application.version);
    //             Directory.CreateDirectory(prefabsFolderPath);
    //             Directory.CreateDirectory(packageFolderPath);

    //             AssetDatabase.ExportPackage(prefabsFolderPath,
    //                 Path.Combine(packageFolderPath, "BonsaiReplayAssets.unitypackage"),
    //                 ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);

    //             Debug.Log("done packaging assets.");
    //         }

    //         private IEnumerator UploadPackagedAssets()
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 yield break;
    //             }

    //             var packagePath =
    //                 Path.Combine("Assets", "BonsaiDeskCache", "PackagedPrefabs", Application.version,
    //                     "BonsaiReplayAssets.unitypackage");
    //             var data = File.ReadAllBytes(packagePath);
    //             if (_packageUploadUwr != null)
    //             {
    //                 _packageUploadUwr.Abort();
    //                 yield return null;
    //             }

    //             using (_packageUploadUwr = UnityWebRequest.Put(
    //                        $"{BonsaiTrackingManager.ApiInputBase}/asset_package?app_version={Application.version}&company_name={Application.companyName}&product_name={Application.productName}",
    //                        data))
    //             {
    //                 Debug.Log("uploading assets...");
    //                 var operation = _packageUploadUwr.SendWebRequest();

    //                 double lastLogTime = 0;

    //                 while (!operation.isDone)
    //                 {
    //                     if (EditorApplication.timeSinceStartup - lastLogTime > 0.5f)
    //                     {
    //                         lastLogTime = EditorApplication.timeSinceStartup;
    //                         Debug.Log($"upload progress: {Math.Floor(_packageUploadUwr.uploadProgress * 100f)}%");
    //                     }

    //                     yield return null;
    //                 }

    //                 if (_packageUploadUwr.result != UnityWebRequest.Result.Success)
    //                 {
    //                     Debug.Log(_packageUploadUwr.error);
    //                 }
    //                 else
    //                 {
    //                     Debug.Log("Upload complete!");
    //                 }
    //             }
    //         }

    //         private void ConvertShaders()
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 return;
    //             }

    //             EditorSceneManager.SaveOpenScenes();
    //             AssetDatabase.SaveAssets();

    //             var prefabsFolderPath =
    //                 Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             Directory.CreateDirectory(prefabsFolderPath);

    //             var standardShader = Shader.Find("Standard");

    //             var assets = AssetDatabase.FindAssets("t:prefab", new[] { prefabsFolderPath });
    //             for (int i = 0; i < assets.Length; i++)
    //             {
    //                 var path = AssetDatabase.GUIDToAssetPath(assets[i]);
    //                 var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    //                 if (HasUnsupportedShader(prefab))
    //                 {
    //                     ConvertPrefabShaders(path, standardShader);
    //                 }
    //             }

    //             AssetDatabase.SaveAssets();
    //         }

    //         private static bool HasUnsupportedShader(GameObject prefab)
    //         {
    //             var renderers = prefab.GetComponentsInChildren<Renderer>();
    //             for (int i = 0; i < renderers.Length; i++)
    //             {
    //                 var materials = renderers[i].sharedMaterials;
    //                 for (int j = 0; j < materials.Length; j++)
    //                 {
    //                     if (!materials[j].shader.isSupported)
    //                     {
    //                         return true;
    //                     }
    //                 }
    //             }

    //             return false;
    //         }

    //         private static void ConvertPrefabShaders(string prefabPath, Shader standardShader)
    //         {
    //             using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
    //             {
    //                 var prefabRoot = editingScope.prefabContentsRoot;
    //                 var renderers = prefabRoot.GetComponentsInChildren<Renderer>();
    //                 for (int i = 0; i < renderers.Length; i++)
    //                 {
    //                     var materials = renderers[i].sharedMaterials;
    //                     for (int j = 0; j < materials.Length; j++)
    //                     {
    //                         if (!materials[j].shader.isSupported)
    //                         {
    //                             materials[j].shader = standardShader;
    //                         }
    //                     }
    //                 }
    //             }
    //         }

    //         private void BundleAndUploadAssets()
    //         {
    //             var bundleHashString = BundleAssets();
    //             EditorCoroutineUtility.StartCoroutine(UploadBundledAssets(bundleHashString), this);
    //         }

    //         private string BundleAssets()
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 return "";
    //             }

    //             Debug.Log("bundling assets...");

    //             var prefabsFolderPath =
    //                 Path.Combine("Assets", "BonsaiDeskCache", "TrackedObjectsPrefabs", Application.version);
    //             Directory.CreateDirectory(prefabsFolderPath);

    //             var buildMap = new AssetBundleBuild[1];
    //             buildMap[0].assetBundleName = "bonsaireplayassets";

    //             var prefabPaths = Directory.GetFiles(prefabsFolderPath, "*.prefab");
    //             buildMap[0].assetNames = prefabPaths;

    //             var bundleFolderPath = Path.Combine("Assets", "BonsaiDeskCache", "BundledPrefabs", Application.version);
    //             Directory.CreateDirectory(bundleFolderPath);

    //             var bundleManifest = BuildPipeline.BuildAssetBundles(bundleFolderPath, buildMap,
    //                 BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle,
    //                 BuildTarget.WebGL);
    //             var bundleHashString = bundleManifest.GetAssetBundleHash("bonsaireplayassets").ToString();
    //             Debug.Log($"bundle hash: {bundleHashString}");

    //             Debug.Log("done bundling assets.");

    //             return bundleHashString;
    //         }

    //         private IEnumerator UploadBundledAssets(string bundleHashString)
    //         {
    //             if (Application.isPlaying)
    //             {
    //                 BonsaiUtility.LogError("Not allowed in play mode");
    //                 yield break;
    //             }

    //             var bundlePath =
    //                 Path.Combine("Assets", "BonsaiDeskCache", "BundledPrefabs", Application.version,
    //                     "bonsaireplayassets");
    //             var data = File.ReadAllBytes(bundlePath);
    //             if (_bundleUploadUwr != null)
    //             {
    //                 _bundleUploadUwr.Abort();
    //                 yield return null;
    //             }

    //             using (_bundleUploadUwr = UnityWebRequest.Put(
    //                        $"{BonsaiTrackingManager.ApiQueryBase}/asset_bundle?app_version={Application.version}&company_name={Application.companyName}&product_name={Application.productName}&bundle_hash={bundleHashString}",
    //                        data))
    //             {
    //                 Debug.Log("uploading assets...");
    //                 var operation = _bundleUploadUwr.SendWebRequest();

    //                 double lastLogTime = 0;

    //                 while (!operation.isDone)
    //                 {
    //                     if (EditorApplication.timeSinceStartup - lastLogTime > 0.5f)
    //                     {
    //                         lastLogTime = EditorApplication.timeSinceStartup;
    //                         Debug.Log($"upload progress: {Math.Floor(_bundleUploadUwr.uploadProgress * 100f)}%");
    //                     }

    //                     yield return null;
    //                 }

    //                 if (_bundleUploadUwr.result != UnityWebRequest.Result.Success)
    //                 {
    //                     Debug.Log(_bundleUploadUwr.error);
    //                 }
    //                 else
    //                 {
    //                     Debug.Log("Upload complete!");
    //                 }
    //             }
    //         }
    //     }
}
