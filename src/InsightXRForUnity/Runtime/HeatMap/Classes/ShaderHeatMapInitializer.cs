using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ShaderHeatMapInitializer:  MonoBehaviour{
    [SerializeField] Material heatMapMaterial;
    [SerializeField] Gradient heatMapGradient;
    [SerializeField] Texture2D brushTexture;

    void Awake(){
        var meshes = FindObjectsOfType<MeshRenderer>();
        foreach(var mesh in meshes){
            InitailizeHeatMapMaterialOnMesh(mesh);
        }
    }

    private void InitailizeHeatMapMaterialOnMesh(MeshRenderer mesh){
        var heatMapPainter = mesh.AddComponent<HeatMapTexturePainter>();
        heatMapPainter.Initialize(heatMapGradient, heatMapMaterial, brushTexture);
    }
}