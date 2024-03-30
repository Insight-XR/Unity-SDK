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
// using UltimateXR.Avatar;
// using UltimateXR.Avatar.Rig;
// using UltimateXR.Core;
// using UltimateXR.Examples.UltimateXR.Examples.FullScene.Scripts.Doors;
// using UltimateXR.Manipulation.HandPoses;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Networking;
using Component = InsightXR.Core.Component;

namespace InsightXR.VR
{

    public class LoadCamData : MonoBehaviour
    {
        // private List<VRPlayerRecord> MotionRecord;

        public int frame = 0;
        public int totalframes;
        public List<ObjectData> MotionRecord;

        public List<(float, float, float, float)> handposes;

        public Animator Lefthand;

        public Animator RightHand;
        //public List<(string,string)> handPoses;
        //public List<(UxrHandDescriptor, UxrHandDescriptor)> HandFrameData;
        public string VRCamName;
        public GameObject Endscreen;


        public bool loaded;

        private string LoadBucket;
        // private string path;
        public DataHandleLayer ObjectDataLoader;

        private void OnEnable()
        {
            ObjectDataLoader = FindObjectOfType<DataHandleLayer>();
            LoadBucket = ObjectDataLoader.ReplayBucketURL;
        }

        IEnumerator LoaddatalocallyWebGL()
        {
            
            // Replace "SavedReplayData.json" with the actual file name in your streaming assets folder
            string filePath = Application.streamingAssetsPath + "/Saves/SavedReplayData.json";

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
            if (UnityEngine.Device.Application.platform == RuntimePlatform.WebGLPlayer)
            {
                GetCamData(Application.persistentDataPath + "/Saves", gameObject.name, "callback", "fallback", LoadBucket);
                Debug.Log("Cam data function if available was executed");


                //StartCoroutine(LoaddatalocallyWebGL());
            }
            else
            {
                // callback(File.ReadAllText(UnityEngine.Device.Application.persistentDataPath + "/Saves/SavedReplayData.json"));
                callback(File.ReadAllText(UnityEngine.Device.Application.persistentDataPath + "/Saves/Save.json"));
                // File.WriteAllText(Application.dataPath+"/Saves/check.json",File.ReadAllText(UnityEngine.Device.Application.persistentDataPath + "/Saves/HandPoses.json") );
                // File.WriteAllText(Application.dataPath+"/Saves/Save.json", File.ReadAllText(UnityEngine.Device.Application.persistentDataPath + "/Saves/Save.json"));
                 // handPoses = JsonConvert.DeserializeObject<List<(string, string)>>(File.ReadAllText(Application.persistentDataPath +
                 //     "/Saves/HandPoses.json"));
                // HandFrameData = JsonConvert.DeserializeObject<List<(UxrHandDescriptor,UxrHandDescriptor)>>(File.ReadAllText(Application.persistentDataPath +
                //     "/Saves/HandPoses.json"));

                // Debug.Log("Hand pose count "+ HandFrameData.Count);
                // // foreach (var handpose in handPoses)
                // // {
                // //     Debug.Log(handpose.Item1 + "  "+ handpose.Item2);
                // // }
                // Debug.Log(FindObjectsOfType<Component>().Length);
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
                
                Lefthand.SetFloat("Trigger", handposes[frame].Item1);
                Lefthand.SetFloat("Grip", handposes[frame].Item2);
                
                
                RightHand.SetFloat("Trigger", handposes[frame].Item3);
                RightHand.SetFloat("Grip", handposes[frame].Item4);
                
                if (frame == totalframes - 1)
                {
                    Endscreen.SetActive(true);
                }
                else
                {
                    Endscreen.SetActive(false);
                }
                // UxrAvatar.LocalAvatar.SetCurrentHandPoseImmediately(UxrHandSide.Left, handPoses[frame].Item1);
                // UxrAvatar.LocalAvatar.SetCurrentHandPoseImmediately(UxrHandSide.Right, handPoses[frame].Item2);
                
                // UxrAvatarRig.UpdateHandUsingDescriptor(UxrAvatar.LocalAvatar, UxrHandSide.Left, HandFrameData[frame].Item1);
                // UxrAvatarRig.UpdateHandUsingDescriptor(UxrAvatar.LocalAvatar, UxrHandSide.Right, HandFrameData[frame].Item2);
                

            }

            if (Input.GetKey(KeyCode.RightArrow) && loaded && frame < totalframes - 1)
            {
                frame++;

                // transform.position = MotionRecord[frame].position;
                // transform.rotation = MotionRecord[frame].rotation;
                transform.SetPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
                ObjectDataLoader.DistributeData(frame);
                
                Lefthand.SetFloat("Trigger", handposes[frame].Item1);
                Lefthand.SetFloat("Grip", handposes[frame].Item2);
                
                
                RightHand.SetFloat("Trigger", handposes[frame].Item3);
                RightHand.SetFloat("Grip", handposes[frame].Item4);

                if (frame == totalframes - 1)
                {
                    Endscreen.SetActive(true);
                }
                else
                {
                    Endscreen.SetActive(false);
                }
                // UxrAvatarRig.UpdateHandUsingDescriptor(UxrAvatar.LocalAvatar, UxrHandSide.Left, HandFrameData[frame].Item1);
                // UxrAvatarRig.UpdateHandUsingDescriptor(UxrAvatar.LocalAvatar, UxrHandSide.Right, HandFrameData[frame].Item2);
            }
        }

        // private void ViewDisableGameobjects()
        // {
        //     foreach (var door in FindObjectsOfType<AutomaticDoor>())
        //     {
        //         door.enabled = false;
        //     }
        // }
        
        [DllImport("__Internal")]
        public static extern void GetCamData(string path, string ObjectName, string callback, string fallback, string url);
        
        public void callback(string camdata)
        {
            
            //ViewDisableGameobjects();
            
            Debug.Log("JsLib works!");
            // Debug.Log(camdata);
            //Debug.Log(File.ReadAllText(Application.persistentDataPath + "/Saves/save.json"));
            var DownloadedData = JsonConvert.DeserializeObject<SaveData>(camdata);
            
            ObjectDataLoader.LoadObjectData(DownloadedData.ObjectMotionData);
            handposes = DownloadedData.handPoseData;
            // HandFrameData = DownloadedData.handPoseData;
            ObjectDataLoader.SetRigidbidyoff();
            loaded = true;
            totalframes = DownloadedData.ObjectMotionData.First().Value.Count;
            frame = 0;
            
            MotionRecord = DownloadedData.ObjectMotionData[VRCamName];
            transform.SetPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
            ObjectDataLoader.DistributeData(frame);
            
            // Debug.Log(DownloadedData[VRCamName].Count);
            // Debug.Log(DownloadedData["BatteryGeo1"].Count);
            // foreach (var data in DownloadedData)
            // {
            //     Debug.Log(data.Key + " "+data.Value.Count);
            // }
            
            Debug.Log("Loaded Data");
        
            loaded = true;
        }
        
        public void fallback()
        {
            Debug.Log("JsLib not working correctly");
        }

    }
}