using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [Header("Control the speed of the Asteroid")]
    public float maxSpeed;
    public float minSpeed;

    [Header("Control the Rotational Speed")]
    public float rotationalSpeedMin;
    public float rotationalSpeedMax ;

    private float rotationalSpeed;
    private float xAngle, yAngle, zAngle;

    public Vector3 movementDirection;

    private float asteroidSpeed;

    // Start is called before the first frame update
    void Start()
    {
        //get random speed
        asteroidSpeed = Random.Range(minSpeed, maxSpeed);

        //get random rotation
        xAngle = Random.Range(0, 360);
        yAngle = Random.Range(0, 360);
        zAngle = Random.Range(0, 360);

        transform.Rotate(xAngle, yAngle, zAngle);

        rotationalSpeed = Random.Range(rotationalSpeedMin, rotationalSpeedMax);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(movementDirection * Time.deltaTime * asteroidSpeed, Space.World);
        transform.Rotate(Vector3.up * Time.deltaTime * rotationalSpeed); 
    }
}
