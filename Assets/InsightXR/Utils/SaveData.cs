using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

public class SaveData
{
    //Hand Pose Data
    public List<(UxrHandDescriptor, UxrHandDescriptor)> handPoseData;

    //The Object Motion data
    public Dictionary<string, List<ObjectData>> ObjectMotionData;

    public string RecordingSaveTimeStamp;
    

    //Initiation of the class
    public SaveData(List<(UxrHandDescriptor, UxrHandDescriptor)> hand, Dictionary<string, List<ObjectData>> obj)
    {
        handPoseData = hand;
        ObjectMotionData = obj;

        RecordingSaveTimeStamp = System.DateTime.Now.ToString();
    }
}
