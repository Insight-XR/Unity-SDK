using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gloveCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(transform.position);
        // Debug.Log(transform.localToWorldMatrix.GetPosition());

        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     transform.localPosition = Vector3.zero;
        //     transform.localRotation = Quaternion.identity;
        // }
    }

    public void sethands()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
