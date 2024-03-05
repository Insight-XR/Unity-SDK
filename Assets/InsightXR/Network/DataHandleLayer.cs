using System;
using UnityEngine;
using InsightXR.Channels;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        public int trackerupdate;
        //This class will be listening to the same object 
        //on which every other game object is making the 
        //the transaction of there data entry.
        public Dictionary<string, List<ObjectData>> UserInstanceData;
        // private void OnEnable()     => DataCollector.CollectionRequestEvent += SortAndStoreData;
        // private void OnDisable()    => DataCollector.CollectionRequestEvent -= SortAndStoreData;

        public void StartRecording()
        {
            DataCollector.CollectionRequestEvent += SortAndStoreData;
        }

        public void StopRecording()
        {
            DataCollector.CollectionRequestEvent -= SortAndStoreData;
            Debug.Log("Objects: "+trackerupdate);
        }

        // This funtion will listen on the data coming in every frame.
        private void SortAndStoreData(string gameObjectName, ObjectData gameObjectData){
            if (UserInstanceData == null) UserInstanceData = new();

            if(!UserInstanceData.ContainsKey(gameObjectName)){
                UserInstanceData.Add(gameObjectName, new());
            }

            UserInstanceData[gameObjectName].Add(gameObjectData);
            trackerupdate++;
        }

        //Supposed to return Json string thats is serialized from the UserInstanceData
        public string GetObjectData()
        {
            return JsonConvert.SerializeObject(UserInstanceData);
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
            if(Input.GetKeyDown(KeyCode.T)){
                Debug.Log("testing the data ");
                foreach(var i in UserInstanceData){
                    foreach(var k in i.Value){
                       Debug.Log(k.ObjectPosition);
                    }
                    Debug.Log(i.Key + " <= key || value => " + i.Value);
                }
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
                obj.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
        public bool CheckForNullsInUserInstanceData()
        {
            // Check if the dictionary itself is null
            if (UserInstanceData == null)
            {
                Debug.LogError("UserInstanceData is null");
                return true; // Found null
            }

            // Iterate through each key-value pair in the dictionary
            foreach (var entry in UserInstanceData)
            {
                // Check if the list for this key is null
                if (entry.Value == null)
                {
                    Debug.LogError($"List for key {entry.Key} is null");
                    return true; // Found null
                }

                // Iterate through the list associated with this key
                foreach (var objectData in entry.Value)
                {
                    // Check if any ObjectData in the list is null
                    if (objectData == null)
                    {
                        Debug.LogError($"Null ObjectData found in list for key {entry.Key}");
                        return true; // Found null
                    }

                    // Here you can also add checks for properties inside ObjectData if necessary
                    // For example, checking if ObjectPosition or ObjectRotation is null (though these being structs typically means they can't be null)
                }
            }

            // No nulls found
            return false;
        }
    }
}