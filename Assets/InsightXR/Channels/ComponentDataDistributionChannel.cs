using UnityEngine;
using InsightXR.Utils;
using UnityEngine.Events;

namespace InsightXR.Channels
{
    [CreateAssetMenu(menuName = "Events/XR Component Distribution Channel")]
    public class ComponentDataDistributionChannel : ScriptableObject
    {
        public UnityAction<string, SpatialPathDataModel> CollectionRequestEvent;

        public void RaiseEvent(string objectName, SpatialPathDataModel dataPoints)
        {
            if (!(CollectionRequestEvent == null))
            {
                CollectionRequestEvent.Invoke(objectName, dataPoints);
            }
            else
            {
                Debug.LogWarning("A internal event channel is broadcasted, no one picked up");
            }
        }
    }
}