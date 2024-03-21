using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData
{
    // public Vector3 ObjectPosition;
    //
    // public Quaternion ObjectRotation;

    public float posx, posy, posz, rotx, roty, rotz, rotw;

    public ObjectData(Vector3 pos, Quaternion rot)
    {
        (posx, posy, posz) = (pos.x,pos.y,pos.z);
        // (rotx, roty, rotz, rotw) = (rot.z, rot.y, rot.z, rot.w);
        var rota = rot.eulerAngles;
        (rotx, roty, rotz) = (rota.x, rota.y, rota.z);
    }

    public Vector3 GetPosition()
    {
        return new Vector3(posx, posy, posz);
    }

    public Quaternion GetRotation()
    {
        //return new Quaternion(rotx, roty, rotz, rotw);
        return Quaternion.Euler(rotx, roty, rotz);
    }
}
