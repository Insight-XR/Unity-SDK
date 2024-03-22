using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UnityEngine;

public class PoseCollector : MonoBehaviour
{
    public List<(string,string)> handPoses;
    //Start is called before the first frame update
    void Start()
    {
        handPoses = new List<(string, string)>();
    }

    public void savePosedata()
    {
        Debug.Log("Pose Collection: "+ handPoses.Count);
        File.WriteAllText(Application.persistentDataPath+"/Saves/HandPoses.json", JsonConvert.SerializeObject(handPoses));
    }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        handPoses.Add((UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left).PoseName, UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Right).PoseName));
        
    }
}
