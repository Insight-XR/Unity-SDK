using System;
using UnityEngine;
namespace InsightDesk
{
    public class SimpleOVRManager : MonoBehaviour
    {
        public static SimpleOVRManager Instance { get; private set; }

        public static event Action TrackingLost;
        public static event Action TrackingAcquired;

        private static bool _isHmdPresentCached = false;
        private static bool _isHmdPresent = false;
        private static bool _wasHmdPresent = false;
        private static bool wasPositionTracked = false;

        private OVRTracker _tracker;

        // Counter for how many times the user went outside the boundary
        private int outsideBoundaryCount = 0;
        private bool isOutsideBoundary = false;

        // Fields for FPS calculation
        private float deltaTime = 0.0f;
        private string fpsText;

        private void Awake()
        {
            // Ensure only one instance exists
            if (Instance != null)
            {
                DestroyImmediate(this);
                return;
            }

            Instance = this;
            Initialize();
        }

        private void Initialize()
        {
            // Debug.Log("Simple OVR Manager initialized.");
            _tracker = new OVRTracker();
        }

        private void Update()
        {
            // Update HMD presence status
            isHmdPresent = OVRPlugin.userPresent;

            // Check if HMD status has changed
            if (_wasHmdPresent && !isHmdPresent)
            {
                Debug.Log("[SimpleOVRManager] HMD lost.");
                TrackingLost?.Invoke();
            }

            if (!_wasHmdPresent && isHmdPresent)
            {
                Debug.Log("[SimpleOVRManager] HMD acquired.");
                TrackingAcquired?.Invoke();
            }

            _wasHmdPresent = isHmdPresent;

            // Update tracking status
            bool isPositionTracked = _tracker.isPositionTracked;

            if (wasPositionTracked && !isPositionTracked)
            {
                Debug.Log("[SimpleOVRManager] Tracking lost.");
                TrackingManagerWorker.SetImmersionData(true);
                TrackingLost?.Invoke();
            }

            if (!wasPositionTracked && isPositionTracked)
            {
                Debug.Log("[SimpleOVRManager] Tracking acquired.");
                TrackingManagerWorker.SetImmersionData(false);
                TrackingAcquired?.Invoke();
            }

            wasPositionTracked = isPositionTracked;

            // Check if HMD is present and tracking is lost
            if (isHmdPresent && !isPositionTracked)
            {
                if (!isOutsideBoundary)
                {
                    isOutsideBoundary = true;
                    outsideBoundaryCount++;
                    Debug.Log("[SimpleOVRManager] User went outside the boundary. Count: " + outsideBoundaryCount);

                    // Set immersion data

                }
            }
            else
            {
                if (isOutsideBoundary)
                {
                    isOutsideBoundary = false;
                    Debug.Log("[SimpleOVRManager] User returned inside the boundary.");

                    // Set immersion data
                }
            }

            // FPS calculation
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            fpsText = string.Format("{0:0.} FPS", fps);
            // Debug.Log(fpsText);
        }

        public bool isHmdPresent
        {
            get
            {
                if (!_isHmdPresentCached)
                {
                    _isHmdPresentCached = true;
                    _isHmdPresent = OVRPlugin.userPresent;
                }

                return _isHmdPresent;
            }

            private set
            {
                _isHmdPresentCached = true;
                _isHmdPresent = value;
            }
        }
    }
}