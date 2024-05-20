using UnityEngine;
using System;

namespace InsightXR.VR
{
    public class VRCameraRaycaster : MonoBehaviour
    {
        public static Action<Vector3, Transform> OnHit; // Event to raise when hit occurs
        private Camera vrCamera; // Reference to the camera
        public Color rayColor = Color.red; // Color of the ray
        public float rayDistance = 2f; // Maximum distance of the ray
        //public float maxDistance = 3f; // Maximum distance of the ray
       // public HeatmapController myheatmapController; // Reference to HeatmapController
        void Start()
        {
            // Get the Camera component attached to this GameObject
            vrCamera = GetComponent<Camera>();

            // Find HeatmapController in the scene if not assigned
            /*if (heatmapController == null)
            {
                heatmapController = FindObjectOfType<HeatmapController>();
                if (heatmapController == null)
                {
                    Debug.LogError("HeatmapController not found in the scene.");
                }
            }*/
        }
 //void HandleRaycastHit(Vector3 hitPoint)
//{
    // Calculate distance from the camera or hand to the hit point
   // float distance = Vector3.Distance(transform.position, hitPoint);

    // Call the SetTemperature method of HeatmapController
   // heatmapController.SetTemperature(distance);
//}

        void Update()
        {
            // Create a ray from the center of the screen
            Ray cameraRay = vrCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
            // Cast a ray from the camera and check if it hits something within the specified distance
           /* RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance); // Raycast with maximum distance
            foreach (var hit in hits)
            {
                // If the ray hits something, log the name of the hit object to the console
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
                // Invoke the OnHit event and pass the hit point as a parameter
                OnHit?.Invoke(hit.point); // Raise hit event
                // Perform actions based on the hit object
            }*/
             // Create a ray from the center of the screen
            Ray ray = vrCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            
            // Cast a ray from the camera and check if it hits something within the specified distance
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance))
            {
                // If the ray hits something, invoke the OnHit event and pass the hit point and ray origin as parameters
                OnHit?.Invoke(hit.point, transform);
            }
            /*RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
            foreach (var hit in hits)
            {
                // Log the name of the hit object to the console for debugging
                Debug.Log("Hit object: " + hit.collider.gameObject.name);

                // Invoke the OnHit event and pass the hit point as a parameter
                OnHit?.Invoke(hit.point); // Raise hit event
                // Calculate distance from the camera to the hit point
                float distance = Vector3.Distance(transform.position, hit.point);

                // Call the SetTemperature method of HeatmapController
                myheatmapController.SetTemperature(distance);
            }*/
            // Draw the ray in the Scene view for visualization
            Debug.DrawRay(ray.origin, ray.direction * rayDistance, rayColor, 0.1f);
        }
    }
}
