using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsightXR.VR;
using UnityEngine;

public class ReplayController : MonoBehaviour
{
    private TriggerInputDetector XRInput;

    private InsightXRAPI API;
    public int test;

    [Header("This script only starts and ends the session,\n This is Game Logic")]
    public KeyCode DebugKeyBoardButton;
    // Start is called before the first frame update
    void Start()
    {
        XRInput = FindObjectOfType<TriggerInputDetector>();
        API = FindObjectOfType<InsightXRAPI>();

        if (!API.InReplayMode())
        {
            API.RecordSession();  
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (XRInput.GetLeftPrimaryDown() || Input.GetKeyDown(DebugKeyBoardButton))
        {
            if (API.IsRecording())
            {
                Debug.Log("Stopping Session");
                API.StopSession(true,true);
            }
        }
    }

    public void kill()
    {
        API.StopSession(true,true);
    }
}
