using UnityEngine;

public class AssignLayer : MonoBehaviour
{
    public string layerName = "HeatmapLayer"; // Name of the layer to assign

    void Start()
    {
        // Find all objects that should receive the heatmap material
        GameObject[] objectsToAssignLayer = GameObject.FindGameObjectsWithTag("HeatmapObject");

        // Assign the layer to each object
        foreach (GameObject obj in objectsToAssignLayer)
        {
            obj.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
