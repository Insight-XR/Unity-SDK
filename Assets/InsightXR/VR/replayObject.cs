using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class replayObject : MonoBehaviour
{
    public List<ObjectData> ObjectRecord;

    public void OnEnable()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }
}
