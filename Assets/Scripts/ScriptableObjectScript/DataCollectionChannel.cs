using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/Data Event Channel")]
public class DataCollectionChannel : ScriptableObject
{
    public UnityAction<string, List<ActionReplayRecord>> CollectionRequestEvent;

    public void RaiseEvent(string objectName, List<ActionReplayRecord> dataPoints)
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