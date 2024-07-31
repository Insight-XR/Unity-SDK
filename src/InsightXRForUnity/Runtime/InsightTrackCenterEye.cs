using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class InsightTrackCenterEye : MonoBehaviour
    {
        private void Awake()
        {
            TrackingManager.instance.centerEye = transform;
        }
    }
}