using System;
using System.Collections;
using System.Collections.Generic;
using InsightXR.Core;
using InsightXR.Network;
using UnityEngine;

public class DebugLogger : MonoBehaviour
{
    int CurrentFrame = 0;
    private int UpdateFrame;
    public bool DisableSDK;
    private float FPS;
    private string msg;
    private float TotalFPS;
    
    // Start is called before the first frame update
    void Start()
    {
        if (DisableSDK)
        {
            foreach (var trackedObject in FindObjectsOfType<InsightXRTrackedObject>())
            {
                trackedObject.enabled = false;
            }

            FindObjectOfType<DataHandleLayer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < 0.1)
        {
            return;
        }
        UpdateFrame++;
        FPS = 1 / Time.unscaledDeltaTime;
        TotalFPS += FPS;
        if (DisableSDK)
        {
            msg = $"Mode: SDK Disabled\nFrame Rate: {(int)FPS}\nAverage FrameRate: {TotalFPS/UpdateFrame}\nFrames: {CurrentFrame}";
        }
        else
        {
            msg = $"Mode: SDK Enabled\nFrame Rate: {(int)FPS}\nAverage FrameRate: {TotalFPS/UpdateFrame}\nFrames: {CurrentFrame}";
        }
        // Debug.Log(msg);
        Debug.Log(msg);
    }

    public void FixedUpdate()
    {
        CurrentFrame++;
    }
}
