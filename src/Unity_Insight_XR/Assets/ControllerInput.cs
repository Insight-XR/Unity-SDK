using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class ControllerInput : MonoBehaviour
{
    // Reference to the heatmap particle system
    public ParticleSystem heatmapParticleSystem;

    // Prefab of the sphere mark to be left on objects
    public GameObject sphereMarkPrefab;

    // Time in seconds for the mark to last
    public float markDuration = 5f;

    // Cooldown timer for emitting particles
    private float particleCooldown = 0f;
    public float particleCooldownTime = 0.1f; // Time between emitting particles

    // Dictionary to store the count of spheres in each area
    private Dictionary<Vector3, List<GameObject>> sphereCounts = new Dictionary<Vector3, List<GameObject>>();

    // Update is called once per frame
    void Update()
    {
        // Update particle cooldown timer
        particleCooldown -= Time.deltaTime;

        // Emit particles if cooldown is expired
        if (particleCooldown <= 0f)
        {
            // Emit multiple particles
            for (int i = 0; i < 5; i++)
            {
                EmitHeatmapParticle();
            }

            // Reset particle cooldown timer
            particleCooldown = particleCooldownTime;
        }

        // Change sphere color based on count
        ChangeSphereColor();
    }

    // Method to emit a heatmap particle
    void EmitHeatmapParticle()
    {
        // Perform raycasting from controller position
        Ray ray = new Ray(transform.position, transform.forward); // Assuming the script is attached to the controller/hand
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.green); // Draw the ray in the scene view

        RaycastHit hit;

        // Check if the raycast hits any objects
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);
            // Get hit position
            Vector3 hitPosition = hit.point;

            // Spawn heatmap particle at hit position
            SpawnHeatmapParticle(hitPosition);

            // Spawn sphere mark on hit object
            SpawnSphereMark(hit.collider.gameObject, hitPosition);
        }
    }

    // Method to spawn a heatmap particle at a given position
    void SpawnHeatmapParticle(Vector3 position)
    {
        // Instantiate a new particle at the hit position
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = position;
        heatmapParticleSystem.Emit(emitParams, 1);
    }

    // Method to spawn a sphere mark on the hit object
    void SpawnSphereMark(GameObject hitObject, Vector3 position)
    {
        // Instantiate the sphere mark prefab at the hit position as a child of the hit object
        GameObject sphereMark = Instantiate(sphereMarkPrefab, position, Quaternion.identity, hitObject.transform);

        // Add the sphere to the list for the hit position
        if (sphereCounts.ContainsKey(position))
        {
            sphereCounts[position].Add(sphereMark);
        }
        else
        {
            sphereCounts[position] = new List<GameObject> { sphereMark };
        }

        // Destroy the mark after the specified duration
        Destroy(sphereMark, markDuration);
    }

    // Method to change the color of the spheres based on count
    void ChangeSphereColor()
    {
        foreach (KeyValuePair<Vector3, List<GameObject>> entry in sphereCounts)
        {
            Vector3 position = entry.Key;
            List<GameObject> spheres = entry.Value;
            int count = spheres.Count;

            // If the count exceeds a threshold, change the color of the spheres at this position to red
            if (count > 5)
            {
                foreach (GameObject sphere in spheres)
                {
                    Renderer renderer = sphere.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.red;
                    }
                }
            }
        }
    }
}
