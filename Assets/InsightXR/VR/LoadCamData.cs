using System;
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
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;

namespace InsightXR.VR
{

    public class LoadCamData : MonoBehaviour
    {
        // private List<VRPlayerRecord> MotionRecord;

        public int frame = 0;
        public int totalframes;
        public List<ObjectData> MotionRecord;
        public string VRCamName;


        public bool loaded;

        private string LoadBucket;
        // private string path;
        public DataHandleLayer ObjectDataLoader;

        private void OnEnable()
        {
            ObjectDataLoader = FindObjectOfType<DataHandleLayer>();
            LoadBucket = ObjectDataLoader.ReplayBucketURL;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (UnityEngine.Device.Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //GetCamData(Application.persistentDataPath + "/Saves", gameObject.name, "callback", "fallback", LoadBucket);
                Debug.Log("Cam data function if available was executed");
            }
            else
            {
                callback(File.ReadAllText(UnityEngine.Device.Application.persistentDataPath + "/Saves/Save.json"));
            }



           
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            if (Input.GetKey(KeyCode.LeftArrow) && loaded && frame > 0)
            {
                frame--;

                // transform.position = MotionRecord[frame].position;
                // transform.rotation = MotionRecord[frame].rotation;
                transform.SetPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
                ObjectDataLoader.DistributeData(frame);
            }

            if (Input.GetKey(KeyCode.RightArrow) && loaded && frame < totalframes - 1)
            {
                frame++;

                // transform.position = MotionRecord[frame].position;
                // transform.rotation = MotionRecord[frame].rotation;
                transform.SetPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
                ObjectDataLoader.DistributeData(frame);
            }
        }

        
        //[DllImport("__Internal")]
        //public static extern void GetCamData(string path, string ObjectName, string callback, string fallback, string url);
        
        public void callback(string camdata)
        {
            Debug.Log("JsLib works!");
            // Debug.Log(camdata);
            //Debug.Log(File.ReadAllText(Application.persistentDataPath + "/Saves/save.json"));
            var DownloadedData = JsonConvert.DeserializeObject<Dictionary<string, List<ObjectData>>>(camdata);
            
            ObjectDataLoader.LoadObjectData(DownloadedData);
            ObjectDataLoader.SetRigidbidyoff();
            loaded = true;
            totalframes = DownloadedData.First().Value.Count;
            frame = 0;

            MotionRecord = DownloadedData[VRCamName];
            transform.SetPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
            ObjectDataLoader.DistributeData(frame);
            
            Debug.Log("Loaded Data");
        
            loaded = true;
        }
        
        public void fallback()
        {
            Debug.Log("JsLib not working correctly");
        }

    }
}