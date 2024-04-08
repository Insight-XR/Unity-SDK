using UnityEditor;
using UnityEngine;
using System.Collections;
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
            EditorCoroutineUtility.StartCoroutine(RegisterTrackedSceneObjects(), this);
        }
    }
    private IEnumerator RegisterTrackedSceneObjects()
    {
        yield return null;
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();

        foreach(GameObject go in gameObjects){
            //Check if the component is not being attached to the code data handler.
            if (go.name.Equals("DataHandleLayer")) continue;

            //Check if the component is not already present.
            if(go.GetComponent<InsightXR.Core.InsightXRTrackedObject>() == null)
                go.AddComponent<InsightXR.Core.InsightXRTrackedObject>();
        }
        Debug.Log("InsightXR, Track success");
    }
}
