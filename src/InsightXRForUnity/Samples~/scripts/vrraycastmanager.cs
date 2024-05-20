using UnityEngine;
using System;

namespace InsightXR.VR
{
    public class VRRaycastManager : MonoBehaviour
    {
        public Material thermalMaterial; // Reference to the thermal material (thermal shader graph material)
        public float maxTemperature = 100f; // Maximum temperature value
        public float rayDistance = 2f; // Maximum distance of the ray

        private void OnEnable()
        {
            VRCameraRaycaster.OnHit += HandleRaycastHit;
            VRHandRaycaster.OnHit += HandleRaycastHit;
        }

        private void OnDisable()
        {
            VRCameraRaycaster.OnHit -= HandleRaycastHit;
            VRHandRaycaster.OnHit -= HandleRaycastHit;
        }

        private void HandleRaycastHit(Vector3 hitPoint, Transform rayOrigin)
        {
            // Get the object hit by the raycast
            RaycastHit raycastHit;
            if (Physics.Raycast(rayOrigin.position, (hitPoint - rayOrigin.position).normalized, out raycastHit, rayDistance))
            {
                // Check if the hit object has the "Thermal" tag
                if (raycastHit.collider.CompareTag("Thermal"))
                {
                    Debug.Log($"Hit object with Thermal tag: {raycastHit.collider.gameObject.name}");

                    MeshRenderer meshRenderer = raycastHit.collider.gameObject.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        // Calculate the distance from the ray origin to the hit point
                        float distance = Vector3.Distance(rayOrigin.position, raycastHit.point);

                        // Calculate the temperature value based on the distance
                        float temperature = Mathf.Lerp(maxTemperature, 0, distance / rayDistance);

                        // Log the temperature value for debugging
                        Debug.Log($"Setting temperature to {temperature} based on distance {distance}");

                        // Create a new material instance to avoid modifying the shared material
                        Material instanceMaterial = new Material(thermalMaterial);
                        instanceMaterial.SetFloat("_Temperature", temperature);

                        // Assign the new material instance to the mesh renderer
                        meshRenderer.material = instanceMaterial;

                        Debug.Log("Material updated");
                    }
                    else
                    {
                        Debug.Log("MeshRenderer component not found on the hit object.");
                    }
                }
                else
                {
                    Debug.Log("Hit object does not have the Thermal tag.");
                }
            }
            else
            {
                Debug.Log("Raycast did not hit any object.");
            }
        }
    }
}
