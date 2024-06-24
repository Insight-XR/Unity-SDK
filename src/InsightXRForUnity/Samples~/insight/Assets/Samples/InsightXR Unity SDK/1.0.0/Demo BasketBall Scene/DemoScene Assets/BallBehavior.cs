using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BallBehavior : MonoBehaviour
{

    public ActionBasedController Controller;
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
        GetComponent<Rigidbody>().AddForce( Controller.transform.forward* 12,ForceMode.Impulse);
    }
}
