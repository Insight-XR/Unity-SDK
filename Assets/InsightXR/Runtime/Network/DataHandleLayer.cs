using System;
using System.Collections;
using UnityEngine;
using InsightXR.Channels;
using System.Collections.Generic;
using System.IO;
using InsightXR.VR;
using Newtonsoft.Json;
using Unity.XR.CoreUtils;
using UnityEditor;

namespace InsightXR.Network
{

    public class DataHandleLayer : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private ComponentDataDistributionChannel DataCollector;
        [Space]
        [Header("Broadcasting to")]
        [SerializeField] private ComponentWeb3DataRecievingChannel DataDistributor;

        [Header("References")]
        public TriggerInputDetector ControllerInput;
        public GameObject Player;
        public GameObject ReplayCam;
        public HandAnimator Lefthand;
        public HandAnimator RightHand;
        
        private (float,float) readleft;
        private (float, float) readright;

        private int distributeDataIndex;


        [Header("Play Mode")]
        public bool replay;
        private bool recording;
        
        private Dictionary<string, List<ObjectData>> UserInstanceData;

        private List<(float, float, float, float)> HandData;

        public List<(float, string)> EventLog;

        [Header("Given Data")]
        public string CustomerID;
        public string UserID;
        public string ReplayBucketURL;
        public string APIKEY;

        private void OnEnable(){
            
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("Not running on WebGL");
                if (replay)
                {
                    Debug.Log("Replay is on, Loading the Data");
                    ReplayCam.SetActive(true);
                    Player.SetActive(false);
                }
                else
                {
                    ReplayCam.SetActive(false);
                    Player.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Running on WebGL");
                Player.SetActive(false);
                ReplayCam.SetActive(true);
                
            }
            
            if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
            }
            
        }

        private void Start()
        {
            HandData = new List<(float, float, float, float)>();
            readleft = new();
            readright = new();
            EventLog = new List<(float, string)>();
            
        }

        public bool IsRecording()
        {
            return recording;
        }

        public bool IsReplayMode()
        {
            return replay;
        }

        public void StartRecording()
        {
            Debug.Log("Started Recording");
            DataCollector.CollectionRequestEvent += SortAndStoreData;
            recording = true;
            
        }

        public void StopRecording(bool save, bool close)
        {
            Debug.Log("Frame Count: "+ UserInstanceData.First().Value.Count);
            DataCollector.CollectionRequestEvent -= SortAndStoreData;
            File.WriteAllText(Application.persistentDataPath + "/Saves/Save.json",JsonConvert.SerializeObject(new SaveData(HandData,UserInstanceData,this)));
            

            if (save)
            {
                GetComponent<NetworkUploader>().UploadFileToServerAsync(new SaveData(HandData,UserInstanceData,this), close);
            }
            else
            {
                if (close)
                {
                    if (Application.platform == RuntimePlatform.LinuxEditor ||
                        Application.platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.OSXEditor)
                    {
                        Debug.Log("Save File only stored Locally");
                        Debug.Log("Application Quit");
                    }
                    else
                    {
                        Application.Quit();
                    }
                }
            }
        }
        
        private void SortAndStoreData(string gameObjectName, ObjectData gameObjectData){
            if (UserInstanceData == null) UserInstanceData = new();

            if(!UserInstanceData.ContainsKey(gameObjectName)){
                UserInstanceData.Add(gameObjectName, new());
            }

            UserInstanceData[gameObjectName].Add(gameObjectData);
        }
        
        public void LoadObjectData(Dictionary<string, List<ObjectData>> loadedData)
        {
            UserInstanceData = loadedData;
        }
        

        private void FixedUpdate(){
            
            readleft = Lefthand.GetData();
            readright = RightHand.GetData();

            HandData.Add((readleft.Item1, readleft.Item2, readright.Item1, readright.Item2));
        }
        

        public void DistributeData(int index){
            foreach(var k in UserInstanceData){
                DataDistributor.RaiseEvent(k.Key.ToString(), k.Value[index]);
            }
        }


        public void SetRigidbidyoff()
        {
            foreach (var obj in GameObject.FindObjectsOfType<InsightXR.Core.InsightXRTrackedObject>())
            {
                // obj.GetComponent<Rigidbody>().isKinematic = true;
                if (obj.TryGetComponent<Rigidbody>(out Rigidbody Robj))
                {
                    Robj.isKinematic = true;
                }
            }
        }
        }


    
}
