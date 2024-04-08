using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsightXR.Network;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SaveData
{
    //Hand Pose Data
    // public List<(UxrHandDescriptor, UxrHandDescriptor)> handPoseData;
    
    public string sessionID;
    public string UserID;
    public string CustomerID;
    public string EndDateTime;
    public string ApiKey;
    public string StartDateTime;
    public string SessionDuration;

    //The List of Registered Events
    public List<(float, string)> EventLog;

    //The Hand Pose Data
    public List<(float, float, float, float)> handPoseData;

    //The Object Motion data
    public Dictionary<string, List<ObjectData>> ObjectMotionData;

    
    

    //Initiation of the class
    public SaveData(List<(float, float, float, float)> hand, Dictionary<string, List<ObjectData>> obj, DataHandleLayer controller)
    {
        handPoseData = hand;
        ObjectMotionData = obj;
        
        if (controller != null)
        {
            ApiKey = controller.APIKEY;
            sessionID = Random.Range(12345, 99999).ToString();
            UserID = controller.UserID;
            CustomerID = controller.CustomerID;
            EndDateTime = System.DateTime.UtcNow.ToString("G");
            StartDateTime = System.DateTime.UtcNow.Subtract(new TimeSpan(0, 0, (int)Time.time)).ToString("G");
            SessionDuration = (Time.time * 1000).ToString("F2");
            EventLog = controller.EventLog; 
        }
        
    }
}
