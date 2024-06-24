using UnityEngine;

public class AudioManager : MonoBehaviour
{
    void Start()
    {
        // Get all AudioListener components in the scene
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();

        // If there is more than one AudioListener, disable all but the first one
        if (audioListeners.Length > 1)
        {
            for (int i = 1; i < audioListeners.Length; i++)
            {
                audioListeners[i].enabled = false;
            }
        }
    }
}
