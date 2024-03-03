using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData
{
    public Vector3 ObjectPosition;

    public Quaternion ObjectRotation;

    public ObjectData(Vector3 pos, Quaternion rot)
    {
        ObjectPosition = pos;
        ObjectRotation = rot;
    }
}
