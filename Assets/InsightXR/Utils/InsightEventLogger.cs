using System.Collections.Generic;
using InsightXR.Network;
using UnityEngine;

public class InsightEventLogger : MonoBehaviour
{
    private DataHandleLayer Collector;
    
    // Start is called before the first frame update
    void Start()
    {
        Collector = GetComponent<DataHandleLayer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void InsightLogEvent(string Event)
    {
        Collector.EventLog.Add(((Time.time * 1000), Event));
    }
}
