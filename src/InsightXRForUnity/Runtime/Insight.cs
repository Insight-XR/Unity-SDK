using System;
using UnityEngine;

namespace InsightDesk
{
    public static class Insight
    {
        public static bool IsEvent { get; private set; } = false;
        public static float Timestamp { get; private set; } = 0f;
        public static string EventName { get; private set; } = null;
        public static bool HasEventWritten { get; set; } = false;

        public static void Log(string eventName)
        {
            IsEvent = true;
            Timestamp = Time.realtimeSinceStartup - RecordingStartTime;  // Use realtimeSinceStartup
            EventName = eventName;
            HasEventWritten = false;  // Reset the flag when a new event is logged
            // Debug.Log($"Event logged: {eventName} at {Timestamp} seconds since recording started");
        }

        public static void Reset()
        {
            IsEvent = false;
            Timestamp = 0f;
            EventName = null;
            HasEventWritten = false;
        }

        public static float RecordingStartTime { get; set; }
    }

}
