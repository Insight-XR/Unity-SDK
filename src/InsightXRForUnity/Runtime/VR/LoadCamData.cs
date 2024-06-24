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
        public List<ObjectData> MotionRecordLeft;
        public List<ObjectData> MotionRecordRight;
        // public List<ObjectData> OriginRecord;

        public List<(float, float, float, float)> handposes;

        public Animator Lefthand;
        public Animator RightHand;
        //public GameObject DummyOrigin;
        public string VRCamName;
        public string VRLeftCName;
        public string VRRightCName;

        // added transforms to get logged position and rotational data of the head and the controllers
        public Transform Head;
        public Transform LeftC;
        public Transform RightC;
        public string OriginName;
        public GameObject Endscreen;
        public GameObject ReplayCam;

        // prefabs of decals used
        public GameObject HeadHeatMapDecal;
        public GameObject LeftCHeatMapDecal;
        public GameObject RightCHeatMapDecal;

        // counter for object pooling purposes
        public int HeadDCount, LeftDCount, RightDCount;

        // List for object pooling of decals
        public List<GameObject> instantiatedHeadDecalPrefabs = new List<GameObject>();
        public List<GameObject> instantiatedLeftCDecalPrefabs = new List<GameObject>();
        public List<GameObject> instantiatedRightCDecalPrefabs = new List<GameObject>();

        // To show the heatmap data through Ui
        public TMP_Text HeadText, LeftText, RightText;
        


        public bool loaded;

        private string LoadBucket;
        
        public DataHandleLayer ObjectDataLoader;

        private void OnEnable()
        {
            ReplayCam.name = OriginName;
            gameObject.name = VRCamName;
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

            // Instatiation of Decals for object pooling
            for (int i = 0; i < 50; i++){
                GameObject Headdecal = Instantiate(HeadHeatMapDecal, Vector3.zero, Quaternion.identity);
                instantiatedHeadDecalPrefabs.Add(Headdecal);
            }

            for (int i = 0; i < 50; i++){
                GameObject LeftCdecal = Instantiate(LeftCHeatMapDecal, Vector3.zero, Quaternion.identity);
                instantiatedLeftCDecalPrefabs.Add(LeftCdecal);
            }

            for (int i = 0; i < 50; i++){
                GameObject RightCdecal = Instantiate(RightCHeatMapDecal, Vector3.zero, Quaternion.identity);
                instantiatedRightCDecalPrefabs.Add(RightCdecal);
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
                
                
                HeatMap();              // function takes care of the heatmap.

                if (frame == totalframes - 1)
                {
                    Endscreen.SetActive(true);
                    Debug.Log("Last Frame");

                    // resetting decal pool and UI
                    Reset();
                    
                }
                else
                {
                    Endscreen.SetActive(false);
                }
                if(frame == 0){

                    // resetting decal pool and UI
                    Reset();
                }

            }

            if (Input.GetKey(KeyCode.RightArrow) && loaded && frame < totalframes - 1)
            {
                frame++;
                
                transform.SetLocalPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
                ObjectDataLoader.DistributeData(frame);


                

                HeatMap();              // function takes care of the heatmap.

                if (frame == totalframes - 1)
                {
                    Endscreen.SetActive(true);
                    Debug.Log("Last Frame");

                    // resetting decal pool and UI
                    Reset();
                }
                else
                {
                    Endscreen.SetActive(false);
                }

                if(frame == 0){
                    // resetting decal pool and UI
                    Reset();
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
            MotionRecordLeft = DownloadedData.ObjectMotionData[VRLeftCName];
            MotionRecordRight = DownloadedData.ObjectMotionData[VRRightCName];
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

        void HeatMap(){
            // Getting the logged position and rotational data of head and controllers.
            
            Head.transform.SetLocalPositionAndRotation(MotionRecord[frame].GetPosition(),MotionRecord[frame].GetRotation());
            LeftC.transform.SetLocalPositionAndRotation(MotionRecordLeft[frame].GetPosition(),MotionRecordLeft[frame].GetRotation());
            RightC.transform.SetLocalPositionAndRotation(MotionRecordRight[frame].GetPosition(),MotionRecordRight[frame].GetRotation());

            if (Head != null)
            {
                // Cast a ray from the Head
                RaycastHit hit;
                if (Physics.Raycast(Head.position, Head.forward, out hit))
                {
                    Debug.DrawLine(Head.position, hit.point, Color.green);

                    // Moving the decal to the appropriate place to show thw heatmap.
                    instantiatedHeadDecalPrefabs[HeadDCount].transform.position = Head.position;
                    instantiatedHeadDecalPrefabs[HeadDCount].transform.rotation = Head.rotation;
                    instantiatedHeadDecalPrefabs[HeadDCount].SetActive(true);
                    HeadDCount++;

                    if(HeadDCount == 50){                                       // object pooling
                        HeadDCount = 0;
                    }

                    HeadText.text = hit.collider.gameObject.name;               // add data to UI
                }
            }
            if (LeftC != null)
            {
                // Cast a ray from the leftController
                RaycastHit hit;
                if (Physics.Raycast(LeftC.position, LeftC.forward, out hit))
                {
                    Debug.DrawLine(LeftC.position, hit.point, Color.blue);

                    // Moving the decal to the appropriate place to show thw heatmap.
                    instantiatedLeftCDecalPrefabs[LeftDCount].transform.position = LeftC.position;
                    instantiatedLeftCDecalPrefabs[LeftDCount].transform.rotation = LeftC.rotation;
                    instantiatedLeftCDecalPrefabs[LeftDCount].SetActive(true);
                    LeftDCount++;

                    if(LeftDCount == 50){                                       // object pooling
                        LeftDCount = 0;
                    }

                    LeftText.text = hit.collider.gameObject.name;               // add data to UI
                }
            }
            if (RightC != null)
            {
                // Cast a ray from the rightController
                RaycastHit hit;
                if (Physics.Raycast(RightC.position, RightC.forward, out hit))
                {
                    Debug.DrawLine(RightC.position, hit.point, Color.red);

                    // Moving the decal to the appropriate place to show thw heatmap.
                    instantiatedRightCDecalPrefabs[RightDCount].transform.position = RightC.position;
                    instantiatedRightCDecalPrefabs[RightDCount].transform.rotation = RightC.rotation;
                    instantiatedRightCDecalPrefabs[RightDCount].SetActive(true);
                    RightDCount++;

                    if(RightDCount == 50){                                  // object pooling
                        RightDCount = 0;
                    }

                    RightText.text = hit.collider.gameObject.name;               // add data to UI
                }
                
            }
        }

        void Reset(){
            foreach(GameObject decal in instantiatedHeadDecalPrefabs){
                decal.SetActive(false);
            }
            foreach(GameObject decal in instantiatedLeftCDecalPrefabs){
                decal.SetActive(false);
            }
            foreach(GameObject decal in instantiatedRightCDecalPrefabs){
                decal.SetActive(false);
            }
            HeadText.text = "";
            LeftText.text = "";
            RightText.text = "";
        }

    }
}