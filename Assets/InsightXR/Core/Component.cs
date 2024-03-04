using System.Collections.Generic;
using InsightXR.Channels;
using InsightXR.Utils;
using UnityEngine;

namespace InsightXR.Core
{

    public class Component : MonoBehaviour
    {
        [Header("Listening to")]
        [SerializeField] private ComponentWeb3DataRecievingChannel DataCollectorWebMode;
        [Space]
        [Header ("Broadcasting to")]
        [SerializeField] private ComponentDataDistributionChannel DistributionChannel;
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
        private void FixedUpdate() {
            
            DistributionChannel.RaiseEvent(name, new(transform.position, transform.rotation));
        } 

        private void MoveObject(string name, ObjectData setToPoint){
            if(gameObject.name.Equals(name))
                transform.SetPositionAndRotation(setToPoint.ObjectPosition, setToPoint.ObjectRotation);
        }
    }
}
