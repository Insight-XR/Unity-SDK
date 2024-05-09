
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



  



public class raycast : MonoBehaviour
{
    public GameObject particle;
    // See Order of Execution for Event Functions for information on FixedUpdate() and Update() related to physics queries
    void FixedUpdate()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 6;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            if (hit.transform.CompareTag("hittag"))
            {

                // Print the name of the object hit by the raycast
                // Debug.DrawRay(ray.origin, ray.direction * 1000, Color.yellow);
                Debug.Log("Hit object: " + hit.transform.name);
                particle.SetActive(false);
                // Do something with the hit object, such as triggering an event, etc.
            }
            
            
            else
            {
                particle.SetActive(true);
                particle.transform.position = hit.point;
            }
           
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.Log("Did Hit");
        }
        else
        {
            particle.SetActive(false);
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }
}

