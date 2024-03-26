using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

public class SaveData
{
    public List<(UxrHandDescriptor, UxrHandDescriptor)> handPoseData;

    public Dictionary<string, List<ObjectData>> ObjectMotionData;

    public string RecordingSaveTimeStamp;

    public int NumberOfFrames;
    // Start is called before the first frame update

    public SaveData(List<(UxrHandDescriptor, UxrHandDescriptor)> hand, Dictionary<string, List<ObjectData>> obj)
    {
        handPoseData = hand;
        ObjectMotionData = obj;

        RecordingSaveTimeStamp = System.DateTime.Now.ToString();
        //NumberOfFrames = ObjectMotionData.First().Value.Count;
    }
}
