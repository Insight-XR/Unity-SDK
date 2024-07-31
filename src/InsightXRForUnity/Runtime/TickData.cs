using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class TickData
    {
        public long timeTicks;
        public float unscaledTime;
        public float deltaTime;
        public float handleTickTime;
        public bool isEventWritten; // Add this flag
        public string skyboxName; // New field
        public List<InsightTrackedObjectData> objectData =
            new List<InsightTrackedObjectData>(TrackingManager.NumTrackedObjectsExpectedUpperEnd);

        public List<uint> destroyedObjectIds = new List<uint>(TrackingManager.NumTrackedObjectsExpectedUpperEnd);

        public TickData()
        {
        }

        public TickData Init(long timeTicks, float unscaledTime, float deltaTime, float handleTickTime)
        {
            this.timeTicks = timeTicks;
            this.unscaledTime = unscaledTime;
            this.deltaTime = deltaTime;
            this.handleTickTime = handleTickTime;
            objectData.Clear();
            destroyedObjectIds.Clear();
            return this;
        }
    }
}