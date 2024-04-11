using System.Collections.Generic;
using InsightXR.Network;
using UnityEngine;

public class InsightXRAPI : MonoBehaviour
{
    public DataHandleLayer Collector;
    
    public void RecordSession()
    {
        if (!InReplayMode())
        {
            Collector.StartRecording();
        }
        else
        {
            Debug.LogError("Replay Mode is set to active, aborting recording");
        }
        
    }

    //Lets you stop the session with the choice to upload the save
    public void StopSession(bool uploadSaveFile)
    {
        if (IsRecording())
        {
            Collector.StopRecording(uploadSaveFile, false);
        }
        else
        {
            Debug.LogError("Session is not being recorded, cannot stop an event that does not exist");
        }
        
    }

    //Lets you stop the session with the choice to upload the save and toggle if the application closes afterwards
    public void StopSession(bool uploadSaveFile, bool CloseApplicationAfterSave)
    {
        Collector.StopRecording(uploadSaveFile, CloseApplicationAfterSave);
    }

    //Checks if the session is recording
    public bool IsRecording()
    {
        return Collector.IsRecording();
    }

    //Checks is the Session is in Replay Mode or not
    public bool InReplayMode()
    {
        return Collector.IsReplayMode();
    }

    //Log an Event
    public void InsightLogEvent(string Event)
    {
        Collector.EventLog.Add(((Time.time * 1000), Event));
    }
}
