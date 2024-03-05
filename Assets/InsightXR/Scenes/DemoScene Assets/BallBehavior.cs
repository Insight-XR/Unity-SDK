using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShootBall()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.back * 4,ForceMode.Impulse);
    }
}
