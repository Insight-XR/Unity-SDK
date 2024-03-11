using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace InsightXR.VR
{
    public class MotionPackage
    {
        public string Playerdata;
        public string objectdata;

        public List<VRPlayerRecord> GetPlayerData()
        {
            return JsonConvert.DeserializeObject<List<VRPlayerRecord>>(Playerdata);
        }

        public Dictionary<string, List<ObjectData>> GetObjectData()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, List<ObjectData>>>(objectdata);
        }
        
        
    }
}