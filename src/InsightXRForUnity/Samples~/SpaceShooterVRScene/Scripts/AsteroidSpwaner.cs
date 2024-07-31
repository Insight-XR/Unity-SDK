using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpwaner : MonoBehaviour
{
    [Header("Size of the Spawner area")]
    public Vector3 spawnerSize;

    [Header("Rate Of Spawn")]
    public float spawnRate = 1f;

    [Header("Model To Spawn")]
    [SerializeField] private GameObject asteroidModel;

    private float spawnTimer = 0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(transform.position, spawnerSize);
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if(spawnTimer > spawnRate)
        {
            spawnTimer = 0;
            spawnAsteroid();
        }
    }

    private void spawnAsteroid()
    {
        //get a random position for the asteroid
        Vector3 spawnPoint = transform.position + new Vector3(UnityEngine.Random.Range(-spawnerSize.x/2, spawnerSize.x/2),
                                                              UnityEngine.Random.Range(-spawnerSize.x / 2, spawnerSize.x / 2),
                                                              UnityEngine.Random.Range(-spawnerSize.x / 2, spawnerSize.x / 2));

        GameObject asteroid = Instantiate(asteroidModel, spawnPoint, transform.rotation);

        asteroid.transform.SetParent(this.transform);
    }
}
