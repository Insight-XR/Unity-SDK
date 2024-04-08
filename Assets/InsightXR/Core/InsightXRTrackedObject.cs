using System;
using System.Collections.Generic;
using InsightXR.Channels;
using InsightXR.Network;
using UnityEngine;

namespace InsightXR.Core
{

    public class InsightXRTrackedObject : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private ComponentWeb3DataRecievingChannel DataCollectorWebMode;
        [Space]
        [Header ("Broadcasting to")]
        [SerializeField] private ComponentDataDistributionChannel DistributionChannel;

        public bool hand;
        
        // private List<SpatialPathDataModel> componentHistory;
        //TODO :- 
        //we are making a queue here and the data is collected it will be send to the server.
        //this will not slow down the data collection system.
        // private readonly Queue<SpatialPathDataModel> componentHistoryQueus;

        private void OnEnable() => DataCollectorWebMode.DistributionRequestEvent += MoveObject;

        private void OnDisable() => DataCollectorWebMode.DistributionRequestEvent -= MoveObject;
        //We are not maintaing the history for the time being on the component itself.
        // private void OnEnable(){
        //     componentHistory = new();
        // }

        private void Start()
        {
            // if (Camera.main != null && gameObject == Camera.main.gameObject)
            // {
            //     Debug.Log(gameObject.name + " " + Time.time);
            //     // FindObjectOfType<DataHandleLayer>().StartRecording();
            // }
            // else
            // {
            //     Debug.Log(gameObject.name+" "+ Time.time);
            // }
        }

        private void FixedUpdate() {
            
            DistributionChannel.RaiseEvent(name, new(transform.position, transform.rotation));
            // if (hand)
            // {
            //     Debug.Log("recording : "+transform.localToWorldMatrix.rotation);
            // }
        } 

        private void MoveObject(string name, ObjectData setToPoint){
            if(gameObject.name.Equals(name)) transform.SetPositionAndRotation(setToPoint.GetPosition(), setToPoint.GetRotation());
            
            // if (hand)
            // {
            //     Debug.Log("viewing : "+transform.rotation.eulerAngles);
            // }
        }
    }
}
