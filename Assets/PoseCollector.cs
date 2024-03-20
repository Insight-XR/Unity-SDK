using System.Collections;
using System.Collections.Generic;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;

public class PoseCollector : MonoBehaviour
{
    public List<(string,string)> handPoses;
    // Start is called before the first frame update
    void Start()
    {
        handPoses = new List<(string, string)>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log("Frame");
        
        Debug.Log("Both Hands have poses");
        // Debug.Log(UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left).PoseName);
        handPoses.Add((UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left).PoseName, UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Right).PoseName));
        
    }
}
