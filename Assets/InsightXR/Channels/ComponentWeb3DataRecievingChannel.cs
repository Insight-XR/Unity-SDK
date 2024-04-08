using UnityEngine;
using UnityEngine.Events;

namespace InsightXR.Channels
{
    [CreateAssetMenu(menuName = "Event/ XR Component Web3 distribution")]
    public class ComponentWeb3DataRecievingChannel : ScriptableObject
    {
        public UnityAction<string, ObjectData> DistributionRequestEvent;

        public void RaiseEvent(string objectName, ObjectData dataPoints){
            if (!(DistributionRequestEvent == null))
            {
                DistributionRequestEvent.Invoke(objectName, dataPoints);
            }
            else
            {
                Debug.LogWarning("A internal event channel is broadcasted, no one picked up");
            }
        }
    } 
} 