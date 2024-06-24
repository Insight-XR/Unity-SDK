using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ParticleHeatMapView : MonoBehaviour {
    [SerializeField] HeatMapController heatMapController;
    [SerializeField] ParticleSystem heatParticleSystem;
    [SerializeField] Gradient gradient;
    [SerializeField] bool useRaw;

    void OnEnable() => heatMapController.OnHeatMapUpdate += OnHeatMapUpdate;
    void OnDisable() => heatMapController.OnHeatMapUpdate -= OnHeatMapUpdate;

    void Awake(){
        heatParticleSystem.GetComponent<ParticleSystemRenderer>().material.renderQueue = 5000;
    }

    void OnHeatMapUpdate(HeatMap heatMap){
        heatParticleSystem.Clear();
        Dictionary<Vector3, float> heatMapData = useRaw?  heatMap.RawMap : heatMap.GenerateProcessedHeatMap();
        foreach(var kvp in heatMapData){ 
            EmitHeatParticleAtPoint(kvp.Key, kvp.Value, 0.1f, 5);
        }
    }

    void EmitHeatParticleAtPoint(Vector3 position, float maxHeat, float radius, int numberOfParticles)
    {
        int particlesPerLayer = Mathf.CeilToInt(Mathf.Sqrt(numberOfParticles));
        float polarStep = Mathf.PI / particlesPerLayer;  // Step between each layer
        float azimuthalStep = 2 * Mathf.PI / particlesPerLayer;  // Step around each layer

        for (int i = 0; i < particlesPerLayer; i++)
        {
            for (int j = 0; j < particlesPerLayer; j++)
            {
                float polarAngle = i * polarStep;
                float azimuthalAngle = j * azimuthalStep;

                // Spherical to Cartesian conversion
                float x = radius * Mathf.Sin(polarAngle) * Mathf.Cos(azimuthalAngle);
                float y = radius * Mathf.Sin(polarAngle) * Mathf.Sin(azimuthalAngle);
                float z = radius * Mathf.Cos(polarAngle);

                // Position calculation based on spherical coordinates
                Vector3 particlePosition = new Vector3(position.x + x, position.y + y, position.z + z);

                // Calculate heat based on distance from the center
                float distance = Mathf.Sqrt(x*x + y*y + z*z);
                float heat = maxHeat;

                // Emit a particle at the calculated position
                heatParticleSystem.Emit(particlePosition, 
                    Vector3.zero,
                    0.6f, 
                    float.PositiveInfinity, 
                    gradient.Evaluate(heat));
            }
        }
    }
}