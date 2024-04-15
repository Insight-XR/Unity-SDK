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
        

        private void OnEnable() => DataCollectorWebMode.DistributionRequestEvent += MoveObject;

        private void OnDisable() => DataCollectorWebMode.DistributionRequestEvent -= MoveObject;

        private void Start()
        {
        }

        private void FixedUpdate() {
            
            DistributionChannel.RaiseEvent(name, new(transform.position, transform.rotation));
        } 

        private void MoveObject(string name, ObjectData setToPoint){
            if(gameObject.name.Equals(name)) transform.SetPositionAndRotation(setToPoint.GetPosition(), setToPoint.GetRotation());
            
        }
    }
}
