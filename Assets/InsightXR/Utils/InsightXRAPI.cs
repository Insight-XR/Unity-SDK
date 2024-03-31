using System.Collections.Generic;
using InsightXR.Network;
using UnityEngine;

public class InsightXRAPI : MonoBehaviour
{
    private DataHandleLayer Collector;

    //Creates a reference to the Main Data Handler
    void Start()
    {
        Collector = GetComponent<DataHandleLayer>();
    }

    //Lets you start Recording the Session.
    public void RecordSession()
    {
        Collector.StartRecording();
    }

    //Lets you stop the session with the choice to upload the save
    public void StopSession(bool uploadSaveFile)
    {
        Collector.StopRecording(uploadSaveFile, false);
    }

    //Lets you stop the session with the choice to upload the save and toggle if the application closes afterwards
    public void StopSession(bool uploadSaveFile, bool CloseApplicationAfterSave)
    {
        Collector.StopRecording(uploadSaveFile, CloseApplicationAfterSave);
    }
    

    //Log an Event
    public void InsightLogEvent(string Event)
    {
        Collector.EventLog.Add(((Time.time * 1000), Event));
    }
}
