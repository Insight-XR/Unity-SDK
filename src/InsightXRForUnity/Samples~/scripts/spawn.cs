using UnityEngine;

namespace InsightXR.VR
{
    public class VRSpawnOnHit : MonoBehaviour
    {
        public GameObject prefabToSpawn; // Reference to the prefab to spawn
        public LayerMask hitLayers; // Layers to perform raycast against
        public float raycastInterval = 0.5f; // Interval between each raycast in seconds
        private float timer = 0f; // Timer to track interval

        void Update()
        {
            // Increment the timer
            timer += Time.deltaTime;

            // Perform raycast at specified interval
            if (timer >= raycastInterval)
            {
                // Reset timer
                timer = 0f;

                // Create a ray from the center of the screen
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

                // Perform the raycast
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, hitLayers))
                {
                    // Spawn the prefab at the hit point position
                    Instantiate(prefabToSpawn, hit.point, Quaternion.identity);
                }
            }
        }
    }
}
