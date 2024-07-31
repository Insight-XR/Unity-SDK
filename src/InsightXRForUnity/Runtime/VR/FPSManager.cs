using InsightDesk;
using UnityEngine;

public class FPSManager : MonoBehaviour
{
    private int _frameCount;
    private float _nextUpdate;
    private float _fps;
    private float _lastReportedFPS;
    private const int fpsThreshold = 5; // Threshold for reporting FPS change
    private int currentfps=0;
    private void Start()
    {
        _frameCount = 0;
        _nextUpdate = Time.time + 1f;
        _fps = 0f;
        _lastReportedFPS = 0f;
        GetFPSIfChangedByThreshold();
    }

    private void Update()
    {
        _frameCount++;
        if (Time.time >= _nextUpdate)
        {
            _fps = _frameCount;
            _frameCount = 0;
            _nextUpdate = Time.time + 1f;
        }
        GetFPSIfChangedByThreshold();
    }

    public int GetCurrentFPS()
    {
        return (int)_fps;
    }

    public void GetFPSIfChangedByThreshold()
    {
        if (Mathf.Abs(_fps - _lastReportedFPS) >= fpsThreshold)
        {
            _lastReportedFPS = _fps;
            currentfps = (int)_fps;
            TrackingManagerWorker.setfps(currentfps);
            // Debug.Log("fps is" + currentfps);
        }
        else
        {
            
            currentfps = (int)_lastReportedFPS;
        }
        
        // Return null if the change is not significant
    }
}