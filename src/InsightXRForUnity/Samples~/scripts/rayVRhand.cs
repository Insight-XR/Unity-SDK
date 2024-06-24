using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;

namespace InsightXR.VR
{
    public class VRHandRaycaster : MonoBehaviour
    {
        public static Action<Vector3, Transform> OnHit; // Event to raise when hit occurs
        private Transform handTransform; // Reference to the transform of the hand
        private XRBaseInteractor interactor; // Reference to the XRBaseInteractor component
        public float rayDistance = 2f; // Maximum distance of the ray (modify this value as needed)
        //public float maxDistance = 3f; // Maximum distance of the ray (modify this value as needed)
        //public HeatmapController heatmapController; // Reference to HeatmapController
        void Start()
        {
            handTransform = GetComponent<Transform>(); // Get the Transform component attached to this GameObject
            
            // Get the XRBaseInteractor component attached to the same GameObject
            interactor = GetComponent<XRBaseInteractor>(); 
            if (interactor == null)
            {
                Debug.LogError("XRBaseInteractor component not found on the hand GameObject.");
            }

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
    //float distance = Vector3.Distance(transform.position, hitPoint);

    // Call the SetTemperature method of HeatmapController
    //heatmapController.SetTemperature(distance);
//}

        void Update()
        {
            // Create a ray from the hand position in the forward direction
            Ray handray = new Ray(handTransform.position, handTransform.forward);
            
            // Cast a ray from the hand and check if it hits something within the specified distance
            RaycastHit hit;
            if (Physics.Raycast(handray, out hit, rayDistance))
            {
                // If the ray hits something, log the name of the hit object to the console
                Debug.Log("Hit object: " + hit.collider.gameObject.name);
                // Invoke the OnHit event and pass the hit point as a parameter
                OnHit?.Invoke(hit.point, transform); // Raise hit event
                // Perform actions based on the hit object
            }
            /*RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
            foreach (var hit in hits)
            {
                // Log the name of the hit object to the console for debugging
                Debug.Log("Hit object: " + hit.collider.gameObject.name);

                // Invoke the OnHit event and pass the hit point as a parameter
                OnHit?.Invoke(hit.point); // Raise hit event
                // Calculate distance from the hand to the hit point
                float distance = Vector3.Distance(transform.position, hit.point);

                // Call the SetTemperature method of HeatmapController
                myheatmapController.SetTemperature(distance);
            }*/
        }
    }
}
