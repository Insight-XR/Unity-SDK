using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    public GameObject prefabToClone; // Clone the prefab at the hit point
    public float maxDistance; // Distance to hit from far or near

    private GameObject previousClone;

    private void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance))
        {
            // Check if Raycast hit a Wall
            if (hit.collider.CompareTag("Wall"))
            {
                Debug.Log("Hit a Wall");

                // If there is no clone already or the clone is not in the same position as the hit point, create a new clone
                if (previousClone == null || Vector3.Distance(previousClone.transform.position, hit.point) > 0.1f)
                {
                    if (previousClone != null)
                    {
                        Destroy(previousClone, 15f); // Destroy the previous clone
                    }

                    GameObject clone = Instantiate(prefabToClone, hit.point, Quaternion.identity); // Clone the prefab at the hit point
                    clone.transform.SetParent(hit.collider.transform); // Optionally, you can parent the clone to the wall
                    previousClone = clone; // Update the reference to the new clone
                }
            }
        }
        else
        {
            if (previousClone != null)
            {
                Destroy(previousClone);
                previousClone = null;
            }
        }
    }
}
