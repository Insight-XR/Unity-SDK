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
using Component = InsightXR.Core.InsightXRTrackedObject;

namespace InsightXR.VR
{

    public class LoadCamData : MonoBehaviour
    {
        public int frame = 0;
        public int totalframes;
        public List<ObjectData> MotionRecord;
        // public List<ObjectData> OriginRecord;

        public List<(float, float, float, float)> handposes;

        public Animator Lefthand;
        public Animator RightHand;
        //public GameObject DummyOrigin;
        public string VRCamName;
        public string OriginName;
        public GameObject Endscreen;
        public GameObject ReplayCam;
        


        public bool loaded;

        private string LoadBucket;
        
        public DataHandleLayer ObjectDataLoader;

        private void OnEnable()
        {
            ReplayCam.name = OriginName;
            ObjectDataLoader = FindObjectOfType<DataHandleLayer>();
            LoadBucket = ObjectDataLoader.ReplayBucketURL;
        }

        IEnumerator ExternalFileWebGL()
        {
            
            // Replace "SavedReplayData.json" with the actual file name in your streaming assets folder
            string filePath = "https://publicpoints.s3.ap-south-1.amazonaws.com/Replay.json";

            // Create a UnityWebRequest to load the file
            UnityWebRequest www = UnityWebRequest.Get(filePath);

            Debug.Log("getting save data [WEBGL]");
            yield return www.SendWebRequest(); // Wait for the request to complete

            if (www.result == UnityWebRequest.Result.Success)
            {
                // Successfully loaded the data
                callback(www.downloadHandler.text);
                Debug.Log("Replay data loaded successfully.");
            }
            else
            {
                Debug.LogError("Error loading replay data: " + www.error);
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            Endscreen.SetActive(true);
            if (UnityEngine.Device.Application.platform == RuntimePlatform.WebGLPlayer)
            {
                //GetCamData(Application.persistentDataPath + "/Saves", gameObject.name, "callback", "fallback", LoadBucket);
                Endscreen.SetActive(true);
                StartCoroutine(ExternalFileWebGL());

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
                
                transform.SetLocalPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
                ObjectDataLoader.DistributeData(frame);
                
                Lefthand.SetFloat("Trigger", handposes[frame].Item1);
                Lefthand.SetFloat("Grip", handposes[frame].Item2);
                
                
                RightHand.SetFloat("Trigger", handposes[frame].Item3);
                RightHand.SetFloat("Grip", handposes[frame].Item4);
                
                
                
                if (frame == totalframes - 1)
                {
                    Endscreen.SetActive(true);
                    Debug.Log("Last Frame");
                }
                else
                {
                    Endscreen.SetActive(false);
                }


            }

            if (Input.GetKey(KeyCode.RightArrow) && loaded && frame < totalframes - 1)
            {
                frame++;
                
                transform.SetLocalPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
                ObjectDataLoader.DistributeData(frame);


                if (frame == totalframes - 1)
                {
                    Endscreen.SetActive(true);
                    Debug.Log("Last Frame");
                }
                else
                {
                    Endscreen.SetActive(false);
                }
            }
        }


        public void callback(string camdata)
        {

            var DownloadedData = JsonConvert.DeserializeObject<SaveData>(camdata);

            ObjectDataLoader.LoadObjectData(DownloadedData.ObjectMotionData);
            handposes = DownloadedData.handPoseData;


            loaded = true;
            totalframes = DownloadedData.ObjectMotionData.First().Value.Count;
            frame = 0;

            MotionRecord = DownloadedData.ObjectMotionData[VRCamName];
            // OriginRecord = DownloadedData.ObjectMotionData[OriginName];
            transform.SetLocalPositionAndRotation(MotionRecord[frame].GetPosition(), MotionRecord[frame].GetRotation());
            // DummyOrigin.transform.SetLocalPositionAndRotation(OriginRecord[frame].GetPosition(),OriginRecord[frame].GetRotation());

            foreach (var D in DownloadedData.ObjectMotionData)
            {
                Debug.Log(D.Key + " " + D.Value.Count);
            }

            ObjectDataLoader.DistributeData(frame);

            Debug.Log("Save Data Loaded Successfully");
            Endscreen.SetActive(false);

            ObjectDataLoader.SetRigidbidyoff();
            loaded = true;
        }

    }
}