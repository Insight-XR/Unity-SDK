using System;
using System.Collections;
using UnityEngine;
using InsightXR.Channels;
using System.Collections.Generic;
using System.IO;
using InsightXR.VR;
using Newtonsoft.Json;
// using UltimateXR.Avatar;
// using UltimateXR.Avatar.Rig;
// using UltimateXR.Core;
// using UltimateXR.Devices;
// using UltimateXR.Manipulation.HandPoses;
using Unity.XR.CoreUtils;
using UnityEditor;

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

        [Header("References")]
        public TriggerInputDetector ControllerInput;
        public GameObject Player;
        public GameObject ReplayCam;
        public HandAnimator Lefthand;
        public HandAnimator RightHand;
        
        private (float,float) readleft;
        private (float, float) readright;
        
        //This needed to hide in future.
        // [SerializeField] private InsightXRMODE SDK_MODE;

        private int distributeDataIndex;


        [Header("Play Mode")]
        public bool replay;
        private bool recording;
        //private PoseCollector PoseCollection;
        //public List<(UxrHandDescriptor, UxrHandDescriptor)> HandFrameData;
        
        
        //This class will be listening to the same object 
        //on which every other game object is making the 
        //the transaction of there data entry.
        private Dictionary<string, List<ObjectData>> UserInstanceData;

        private List<(float, float, float, float)> HandData;

        public List<(float, string)> EventLog;
        // private void OnEnable()     => DataCollector.CollectionRequestEvent += SortAndStoreData;
        // private void OnDisable()    => DataCollector.CollectionRequestEvent -= SortAndStoreData;

        [Header("Given Data")]
        public string CustomerID;
        public string UserID;
        public string ReplayBucketURL;
        public string APIKEY;

        private void OnEnable()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
            }
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Debug.Log("Not running on WebGL");
                if (replay)
                {
                    Debug.Log("Replay is on, Loading the Data");
                    ReplayCam.SetActive(true);
                    // if (TryGetComponent(out PoseCollector col))
                    // {
                    //     col.enabled = true;
                    // }
                    Player.SetActive(false);
                }
                else
                {
                    // Debug.Log("Replay is Off, Recording the Session");
                    // Player.SetActive(true);
                    // ReplayCam.SetActive(false);
                    // //StartCoroutine(StartRecordingSession());
                    // // recording = true;
                    // StartRecording();
                }
            }
            else
            {
                Debug.Log("Running on WebGL");
                ReplayCam.SetActive(true);
            }
            
        }

        private void Start()
        {
            HandData = new List<(float, float, float, float)>();
            readleft = new();
            readright = new();
            EventLog = new List<(float, string)>();
            
            Debug.Log("Location:  "+ Input.location.lastData);
        }

        // public UxrControllerInput UXRcontrollerInput;
        public void StartRecording()
        {
            Debug.Log("Started Recording");
            DataCollector.CollectionRequestEvent += SortAndStoreData;
            recording = true;
            
            Player.SetActive(true);
            ReplayCam.SetActive(false);
            //UXRcontrollerInput = FindObjectOfType<UxrControllerInput>();
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

        private void OnDisable()
        {
            if (UnityEngine.Device.Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (!replay)
                {
                    //Create a Save File incase the Application wants to close
                    
                    
                    // Debug.Log("Record Count: "+ UserInstanceData.First().Value.Count);
                    
                    // PoseCollection.savePosedata();
                    //File.WriteAllText(Application.persistentDataPath + "/Saves/SavedReplayData.json", JsonConvert.SerializeObject(new SaveData(PoseCollection.HandFrameData, UserInstanceData)));
                    //File.WriteAllText(Application.streamingAssetsPath + "/Saves/SavedReplayData.json", JsonConvert.SerializeObject(new SaveData(PoseCollection.HandFrameData, UserInstanceData)));
                    // //We can instead call it directly with the file path and create a stream like that, but for now, this will do
                    // GetComponent<NetworkUploader>().UploadFileToServerAsync(File.ReadAllText(Application.dataPath + "/Saves/Save.json"));

                    // foreach (var Posepair in PoseCollection.handPoses)
                    // {
                    //     Debug.Log(Posepair.Item1 + "   " + Posepair.Item2);
                    // }
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
            Debug.Log("Save Data Loaded");
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

            // if (Input.GetKeyDown(KeyCode.M))
            // {
            //     Debug.Log("M Pressed");
            //     foreach (var glove in FindObjectsOfType<gloveCheck>())
            //     {
            //         glove.sethands();
            //     }
            //     Debug.Log("Set Hands");
            // }
            if (ControllerInput.GetLeftPrimaryDown() || Input.GetKeyDown(KeyCode.M))
            {
                Debug.Log("LEFT WORKS");
            
            // if(UXRcontrollerInput.GetButtonsPress(UxrHandSide.Left, UxrInputButtons.Button1)){Debug.Log("Button1");}
            // if(UXRcontrollerInput.GetButtonsPress(UxrHandSide.Left, UxrInputButtons.Button2)){Debug.Log("Button2");}
            // if (UXRcontrollerInput.GetButtonsPress(UxrHandSide.Left, UxrInputButtons.Button1) && Application.platform != RuntimePlatform.WebGLPlayer)
            
                 if (!recording)
                 {
                     StartRecording();
                     Debug.Log("Recording Started");
                     recording = true;

                 }
                 else
                 {
                     Debug.Log("Ending Application");
                     
                     
                     //EditorApplication.isPlaying = false;
                     
                     
                     
                 }
                 Debug.Log("X Button Pressed");
                 //The below Script is uploading an object with all the data. It gets serialized and sent to the Cloud
                 
                 
            
            }

            // if (UxrAvatar.LocalAvatar != null)
            // {
            //     Debug.Log("Left :" + UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left).PoseName);
            //     Debug.Log("Right :" + UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Right).PoseName); 
            // }
            

            // if (UxrAvatar.LocalAvatar != null)
            // {
            //     // UxrAvatar.LocalAvatar.GetRuntimeHandPose();
            //     Debug.Log(UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left).PoseName);
            // }

            // if (Input.GetKeyDown(KeyCode.M))
            // {
            //     // UxrAvatar.LocalAvatar.SetCurrentHandPoseImmediately(UxrHandSide.Left,"GrabStandard");
            //     
            //     // UxrAvatarRig.UpdateHandUsingDescriptor();
            // }

            

        }

        private void FixedUpdate(){
            // HandData.Add((Lefthand.GetFloat("Trigger"),Lefthand.GetFloat("Grab"),RightHand.GetFloat("Trigger"),RightHand.GetFloat("Grab")));
            // Debug.Log((Lefthand.GetFloat("Trigger"),Lefthand.GetFloat("Grab"),RightHand.GetFloat("Trigger"),RightHand.GetFloat("Grab")).ToString());
            readleft = Lefthand.GetData();
            readright = RightHand.GetData();
            
            HandData.Add((readleft.Item1,readleft.Item2,readright.Item1,readright.Item2));
            Debug.Log((readleft.Item1,readleft.Item2,readright.Item1,readright.Item2).ToString());
        }
        

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

        // IEnumerator StartRecordingSession()
        // {
        //     while (UxrAvatar.LocalAvatar != null && UxrAvatar.LocalAvatar.GetCurrentRuntimeHandPose(UxrHandSide.Left) == null)
        //     {
        //         Debug.Log("Not Exist");
        //         yield return null;
        //     }
        //     
        //     Debug.Log("Exists");
        //     if (gameObject.TryGetComponent(out PoseCollector collector))
        //     {
        //         PoseCollection = collector;
        //         PoseCollection.enabled = true;
        //     }
        //     else
        //     { 
        //         PoseCollection = gameObject.AddComponent<PoseCollector>();
        //     }
        //
        //     var hands = FindObjectsOfType<gloveCheck>();
        //
        //     while (hands[0].transform.localPosition.x != 0)
        //     {
        //         Debug.Log("Setting hands");
        //         foreach (var glove in hands)
        //         {
        //             glove.sethands();
        //         }
        //     }
        //
        //     Debug.Log("Hands Set");
        //     StartRecording();
        //     
        // }


        
    }
}