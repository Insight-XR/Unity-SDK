using System;
using UnityEngine;

namespace InsightXR.Utils
{
    public class SpatialPathDataModel
    {
        private readonly float posX;
        private readonly float posY;
        private readonly float posZ;
        private readonly float rotx;
        private readonly float roty;
        private readonly float rotz;
        private readonly float rotw;

        public SpatialPathDataModel(Vector3 pos, Quaternion quaternion)
        {
            posX = pos.x;
            posY = pos.y;
            posZ = pos.z;

            rotx = quaternion.x;
            roty = quaternion.y;
            rotz = quaternion.z;
            rotw = quaternion.w;
        }

        public TransformRotation GetData(){
            TransformRotation transformRotation = new()
            {
                position = new Vector3(posX, posY, posZ),
                rotation = new Quaternion(rotx, roty, rotz, rotw)
            };
            return transformRotation;
        }

        public Vector3 GetPosition() => new(posX, posY, posZ);

        public Quaternion GetQuaternion() => new(rotx, roty, rotz, rotw);
        /*
        Utils helper funtion for debugging
        Will discuss on this if needed to ship.
        */

        public void Print(){
            Debug.Log(
                " posX " + posX
              + " posY " + posY
              + " posZ " + posZ
              + " rotx " + rotx
              + " roty " + roty
              + " rotz " + rotz
              + " rotw " + rotw
            );
        }
    }
}