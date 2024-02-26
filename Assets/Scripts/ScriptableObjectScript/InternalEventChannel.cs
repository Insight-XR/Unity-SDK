using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/Network Event Channel")]
public class InternalEventChannel : ScriptableObject
{
    public UnityAction<bool> NetworkCallbackRequested;

    public void RaiseEvent(bool success = false)
    {
        if (!(NetworkCallbackRequested == null))
        {
            NetworkCallbackRequested.Invoke(success);
        }
        else
        {
            Debug.LogWarning("A internal event channel is broadcasted, no one picked up");
        }
    }
}
