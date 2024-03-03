using UnityEngine;
using InsightXR.Utils;
using InsightXR.Channels;
using System.Collections.Generic;

namespace InsightXR.Core
{
    public class DataHandleLayer : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private ComponentDataDistributionChannel DataCollector;
        [Space]
        [Header("Broadcasting to")]
        [SerializeField] private ComponentWeb3DataRecievingChannel DataDistributor;

        //This class will be listening to the same object 
        //on which every other game object is making the 
        //the transaction of there data entry.
        private Dictionary<string, List<SpatialPathDataModel>> UserInstanceData;

        private void OnEnable()     => DataCollector.CollectionRequestEvent += SortAndStoreData;
        private void OnDisable()    => DataCollector.CollectionRequestEvent -= SortAndStoreData;


        // This funtion will listen on the data coming in every frame.
        private void SortAndStoreData(string gameObjectName, SpatialPathDataModel gameObjectData){
            if (UserInstanceData == null) UserInstanceData = new();

            if(!UserInstanceData.ContainsKey(gameObjectName)){
                UserInstanceData.Add(gameObjectName, new());
            }

            UserInstanceData[gameObjectName].Add(gameObjectData);
        }



        /*
        * This is for debbuging this part of the code will not ship.
        */
        private void Update(){
            if(Input.GetKeyDown(KeyCode.T)){
                Debug.Log("testing the data ");
                foreach(var i in UserInstanceData){
                    foreach(var k in i.Value){
                        k.Print();
                    }
                    Debug.Log(i.Key + " <= key || value => " + i.Value);

                }
            }
        }
    }
}