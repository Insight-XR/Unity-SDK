using UnityEngine;
using UnityEngine.Events;

namespace InsightXR.Channels
{
    [CreateAssetMenu(menuName = "Events/XR Component Distribution Channel")]
    public class ComponentDataDistributionChannel : ScriptableObject
    {
        public UnityAction<string, ObjectData> CollectionRequestEvent;

        public void RaiseEvent(string objectName, ObjectData dataPoints)
        {
            if (!(CollectionRequestEvent == null))
            {
                CollectionRequestEvent.Invoke(objectName, dataPoints);
            }
            else
            {
                Debug.LogWarning("A internal event channel is broadcasted, no one picked up  "+objectName);
            }
        }
    }
}