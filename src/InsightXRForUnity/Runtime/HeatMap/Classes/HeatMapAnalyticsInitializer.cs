using UnityEngine;

public class HeatMapAnalyticsInitializer : MonoBehaviour{
    void Awake(){
        var meshes = FindObjectsOfType<MeshRenderer>();
        foreach(var mesh in meshes){
            if(mesh.GetComponent<MeshCollider>() == null){
                mesh.gameObject.AddComponent<MeshCollider>();
            }
        }
    }
}