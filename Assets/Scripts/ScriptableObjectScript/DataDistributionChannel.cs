using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Events/Data Distribution Channel")]
public class DataDistributionChannel : ScriptableObject
{
    public UnityAction<Dictionary<string, List<ActionReplayRecord>>> DistributionRequestEvent;

    public void RaiseEvent(Dictionary<string, List<ActionReplayRecord>> data)
    {
        if (!(DistributionRequestEvent == null))
        {
            DistributionRequestEvent.Invoke(data);
        }
        else
        {
            Debug.LogWarning("A internal event channel is broadcasted, no one picked up");
        }
    }
}