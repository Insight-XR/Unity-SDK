using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPlayerRecord
{
    public Vector3 position;
    public Quaternion rotation;

    public VRPlayerRecord(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}
