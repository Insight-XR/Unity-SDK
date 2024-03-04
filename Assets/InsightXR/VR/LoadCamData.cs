using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using InsightXR.Network;
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
        public DataHandleLayer ObjectDataLoader;


        // Start is called before the first frame update
        void Start()
        {
            
            GetCamData(Application.persistentDataPath + "/Saves", gameObject.name, "callback", "fallback","https://shivam1807.s3.ap-south-1.amazonaws.com/Replay+Data");
            // Debug.Log("Check");
             //callback(File.ReadAllText(Application.dataPath+"/Saves/Save.json"));
            // MotionPackage loadedData =
            //     JsonConvert.DeserializeObject<MotionPackage>(
            //         File.ReadAllText(Application.dataPath + "/Saves/Save.json"));
            //
            // Debug.Log(loadedData.Playerdata);
            //
            // MotionRecord = loadedData.GetPlayerData();
            // loaded = true;
            // totalframes = MotionRecord.Count;
            // frame = 0;
            // Debug.Log("Loaded Data");
            //
            // loaded = true;
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKey(KeyCode.LeftArrow) && loaded && frame > 0)
            {
                frame--;

                transform.position = MotionRecord[frame].position;
                transform.rotation = MotionRecord[frame].rotation;
                ObjectDataLoader.DistributeData(frame);
            }

            if (Input.GetKey(KeyCode.RightArrow) && loaded && frame < MotionRecord.Count - 1)
            {
                frame++;

                transform.position = MotionRecord[frame].position;
                transform.rotation = MotionRecord[frame].rotation;
                ObjectDataLoader.DistributeData(frame);
            }
        }

        
        [DllImport("__Internal")]
        public static extern void GetCamData(string path, string ObjectName, string callback, string fallback,
            string url);
        
        public void callback(string camdata)
        {
            Debug.Log("JsLib works!");
            Debug.Log(camdata);
            //Debug.Log(File.ReadAllText(Application.persistentDataPath + "/Saves/save.json"));
            var DownloadedData = JsonConvert.DeserializeObject<MotionPackage>(camdata);

            MotionRecord = DownloadedData.GetPlayerData();
            Debug.Log("Player: ");
            Debug.Log(MotionRecord);
            Debug.Log("Object Data");
            Debug.Log("Object Data");
            ObjectDataLoader.LoadObjectData(DownloadedData.GetObjectData());
            ObjectDataLoader.SetRigidbidyoff();
            loaded = true;
            totalframes = MotionRecord.Count;
            frame = 0;
            Debug.Log("Loaded Data");
        
            loaded = true;
        }
        
        public void fallback()
        {
            Debug.Log("JsLib not working correctly");
        }

    }
}