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
        

        private void FixedUpdate() {
            if (transform.parent != null)
            {
                DistributionChannel.RaiseEvent(name, new(transform.localPosition, transform.localRotation,transform.parent.name)); 
            }
            else
            {
                DistributionChannel.RaiseEvent(name, new(transform.localPosition, transform.localRotation,"World"));
            }
            
        } 

        private void MoveObject(string name, ObjectData setToPoint)
        {
            if (setToPoint.ParentObject == "World")
            {
                transform.parent = null;
            }
            else
            {
                transform.parent = GameObject.Find(setToPoint.ParentObject).transform;
            }
            if(gameObject.name.Equals(name)) transform.SetLocalPositionAndRotation(setToPoint.GetPosition(), setToPoint.GetRotation());
            
        }
    }
}
