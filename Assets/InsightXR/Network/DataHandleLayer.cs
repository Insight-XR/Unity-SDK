using System;
using System.Collections;
using UnityEngine;
using InsightXR.Channels;
using System.Collections.Generic;
using System.IO;
using InsightXR.VR;
using Newtonsoft.Json;
using Unity.XR.CoreUtils;

namespace InsightXR.Network
{
    public enum InsightXRMODE{
        Recording,
        Normal,
        Replay
    }
    
    public class DataHandleLayer : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private ComponentDataDistributionChannel DataCollector;
        [Space]
        [Header("Broadcasting to")]
        [SerializeField] private ComponentWeb3DataRecievingChannel DataDistributor;

        //This needed to hide in future.
        [SerializeField] private InsightXRMODE SDK_MODE;

        private int distributeDataIndex;

        public TriggerInputDetector ControllerInput;
        public GameObject Player;
        public GameObject ReplayCam;

        public bool replay;
        //This class will be listening to the same object 
        //on which every other game object is making the 
        //the transaction of there data entry.
        private Dictionary<string, List<ObjectData>> UserInstanceData;
        // private void OnEnable()     => DataCollector.CollectionRequestEvent += SortAndStoreData;
        // private void OnDisable()    => DataCollector.CollectionRequestEvent -= SortAndStoreData;

        private void OnEnable()
        {
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("Not running on WebGL");
                if (replay)
                {
                    Debug.Log("Replay is on, Loading the Data");
                    ReplayCam.SetActive(true);
                }
                else
                {
                    Debug.Log("Replay is Off, Recording the Session");
                    Player.SetActive(true);
                    DataCollector.CollectionRequestEvent += SortAndStoreData;
                }
            }
            else
            {
                Debug.Log("Running on WebGL");
                ReplayCam.SetActive(true);
            }
            
        }

        private void OnDisable()
        {
            if (UnityEngine.Device.Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (!replay)
                {
                    //Create a Save File incase the Application wants to close
                    DataCollector.CollectionRequestEvent -= SortAndStoreData;
                    if (Directory.Exists(Application.persistentDataPath + "/Saves"))
                    {
                        Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
                    }
                    Debug.Log("Record Count: "+ UserInstanceData.First().Value.Count);
                    File.WriteAllText(Application.persistentDataPath + "/Saves/Save.json",JsonConvert.SerializeObject(UserInstanceData));
                    // //We can instead call it directly with the file path and create a stream like that, but for now, this will do
                    // GetComponent<NetworkUploader>().UploadFileToServerAsync(File.ReadAllText(Application.dataPath + "/Saves/Save.json"));
                    
                }
                
            }

        }


        // public void StartRecording()
        // {
        //     DataCollector.CollectionRequestEvent += SortAndStoreData;
        // }
        //
        // public void StopRecording()
        // {
        //     DataCollector.CollectionRequestEvent -= SortAndStoreData;
        //     Debug.Log("Objects: "+trackerupdate);
        // }

        // This funtion will listen on the data coming in every frame.
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
            Debug.Log("Data Loaded");
        }
        /*
        * This is for debbuging this part of the code will not ship.
        */
        private void Update(){
            // if(Input.GetKeyDown(KeyCode.T)){
            //     Debug.Log("testing the data ");
            //     foreach(var i in UserInstanceData){
            //         foreach(var k in i.Value){
            //            Debug.Log(k.ObjectPosition);
            //         }
            //         Debug.Log(i.Key + " <= key || value => " + i.Value);
            //     }
            // }

            if (ControllerInput.GetLeftPrimary() && Application.platform != RuntimePlatform.WebGLPlayer)
            {
                GetComponent<NetworkUploader>().UploadFileToServerAsync(JsonConvert.SerializeObject(UserInstanceData));
            }
        }

        // private void FixedUpdate(){
        //     
        //     if (Input.GetKey(KeyCode.R))
        //     {
        //         Debug.Log("In Replay Mode");
        //         SDK_MODE = InsightXRMODE.Replay;
        //         DistributeData(0);
        //         distributeDataIndex++;
        //     }else{
        //         distributeDataIndex = 0;
        //     }
        // }
        

        public void DistributeData(int index){
            foreach(var k in UserInstanceData){
                DataDistributor.RaiseEvent(k.Key.ToString(), k.Value[index]);
                Debug.Log(k.Key);
            }
        }


        public void SetRigidbidyoff()
        {
            foreach (var obj in GameObject.FindObjectsOfType<InsightXR.Core.Component>())
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