using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (FindObjectOfType<InsightXRAPI>().InReplayMode())
        {
            foreach (var door in FindObjectsOfType<AutoDoor>())
            {
                door.enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
