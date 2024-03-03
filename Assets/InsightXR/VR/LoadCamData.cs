using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using InsightXR.VR;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace InsightXR.VR
{

    public class LoadCamData : MonoBehaviour
    {
        private List<VRPlayerRecord> MotionRecord;

        public int frame = 0;
        public int totalframes;

        public bool loaded;
        private string path;


        // Start is called before the first frame update
        void Start()
        {
            // GetCamData(Application.persistentDataPath + "/Saves", gameObject.name, "callback", "fallback",
            //     "https://gist.githubusercontent.com/DhruvInsight/302c34cde8532b1d1a86256d241b4d21/raw/8787613553b406677993fd98949c5f6aa8a47b85/save.json");
            //StartCoroutine(LoadStreamingAsset());

            MotionPackage loadedData =
                JsonConvert.DeserializeObject<MotionPackage>(
                    File.ReadAllText(Application.dataPath + "/Saves/Save.json"));
            
            Debug.Log(loadedData.Playerdata);

            MotionRecord = loadedData.GetPlayerData();
            loaded = true;
            totalframes = MotionRecord.Count;
            frame = 0;
            Debug.Log("Loaded Data");
            
            loaded = true;

            foreach (var VARIABLE in GameObject.FindObjectsOfType<replayObject>())
            {
                
            }
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKey(KeyCode.LeftArrow) && loaded && frame > 0)
            {
                frame--;

                transform.position = MotionRecord[frame].position;
                transform.rotation = MotionRecord[frame].rotation;
            }

            if (Input.GetKey(KeyCode.RightArrow) && loaded && frame < MotionRecord.Count - 1)
            {
                frame++;

                transform.position = MotionRecord[frame].position;
                transform.rotation = MotionRecord[frame].rotation;
            }
        }

        //
        // [DllImport("__Internal")]
        // public static extern void GetCamData(string path, string ObjectName, string callback, string fallback,
        //     string url);
        //
        // public void callback(string camdata)
        // {
        //     Debug.Log("JsLib works!");
        //     Debug.Log(camdata);
        //     //Debug.Log(File.ReadAllText(Application.persistentDataPath + "/Saves/save.json"));
        //     MotionRecord = JsonConvert.DeserializeObject<List<VRPlayerRecord>>(camdata);
        //     loaded = true;
        //     totalframes = MotionRecord.Count;
        //     frame = 0;
        //     Debug.Log("Loaded Data");
        //
        //     loaded = true;
        // }
        //
        // public void fallback()
        // {
        //     Debug.Log("JsLib not working correctly");
        // }

    }
}