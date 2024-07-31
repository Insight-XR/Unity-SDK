using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidKillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Asteroid"))
        {
            Destroy(other.gameObject);
        }
    }
}
