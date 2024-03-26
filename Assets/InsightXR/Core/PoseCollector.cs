using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Manipulation.HandPoses;
using UnityEngine;

public class PoseCollector : MonoBehaviour
{
    // public List<(string,string)> handPoses;
    public List<(UxrHandDescriptor, UxrHandDescriptor)> HandFrameData;
    //Start is called before the first frame update
    void Start()
    {
        // handPoses = new List<(string, string)>();
        HandFrameData = new List<(UxrHandDescriptor, UxrHandDescriptor)>();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        //handPoses.Add((UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left).PoseName, UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Right).PoseName));
        HandFrameData.Add((new UxrHandDescriptor(UxrAvatar.LocalAvatar, UxrHandSide.Left),new UxrHandDescriptor(UxrAvatar.LocalAvatar, UxrHandSide.Right)));
    }
}
