using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projector : MonoBehaviour
{
    // Reference to the projectile prefab
    public GameObject projectilePrefab;

    // Speed of the projectile
    public float projectileSpeed = 5f;

    // Time interval between projectile releases
    public float releaseInterval = 1f;

    // Timer to track the time since the last release
    private float releaseTimer = 0f;

    void Update()
    {
        // Update the release timer
        releaseTimer += Time.deltaTime;

        // Check if it's time to release a projectile
        if (releaseTimer >= releaseInterval)
        {
            // Reset the release timer
            releaseTimer = 0f;

            // Instantiate a new projectile at the player's position
            GameObject newProjectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

            // Get the forward direction of the player
            Vector3 playerForward = transform.forward;

            // Get the rigidbody component of the projectile
            Rigidbody rb = newProjectile.GetComponent<Rigidbody>();


            // Set the initial velocity of the projectile
            rb.velocity = playerForward * projectileSpeed;
        }
    }
}
