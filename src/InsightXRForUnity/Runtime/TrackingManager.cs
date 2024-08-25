using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

namespace InsightDesk
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1)]
    public class TrackingManager : MonoBehaviour
    {
        private const short TickRate = 20;
        private bool _recording = false;
        public static string PersistentDataPath;
        public enum InsightLogLevel
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            PerformanceProfiling = 4
        }
        private float _recordingStartTime;
        public string token = "";
        public InsightLogLevel logLevel = InsightLogLevel.Error;
        public bool recordOnStart = true;
        public float recordOnStartMaxLengthMinutes = 1000;
        private float _recordingAutoStopUnscaledTime = float.MaxValue;
        private string _userId;
        public const string ApiInputBase = "https://api.insightdesk.com/input/v1";
        public const string ApiQueryBase = "https://api.insightdesk.com/query/v1";
        public const int NumTrackedObjectsExpectedUpperEnd = 250;
        public const int TicksInPipelineExpectedUpperEnd = 5;
        public const int EventsInPacketExpectedUpperEnd = 100;
        public Transform centerEye { get; set; }
        public Transform leftHandAnchor { get; set; }
        public List<GameObject> Left_AutoHandsBendableObjects { get; set; }
        public Transform rightHandAnchor { get; set; }
        public List<GameObject> Right_AutoHandsBendableObjects { get; set; }
        public enum ReservedTrackedObjectInstanceIds
        {
            CenterEye = 1,
            LeftHand = 2,
            RightHand = 3,
        }

        private uint _lastUsedTrackedObjectInstanceId = 100;

        private static TrackingManager _instance;

        public static TrackingManager instance => _instance;

        // keeping direct references to Transform and GameObject saves some time during LateUpdate
        private readonly List<InsightTrackObject> _trackedObjects = new List<InsightTrackObject>();
        private readonly List<Transform> _trackedObjectTransforms = new List<Transform>();
        private readonly List<GameObject> _trackedObjectGameObjects = new List<GameObject>();

        private Queue<uint> _destroyedObjectIds;

        private readonly Queue<InsightEvent> _insightEvents = new Queue<InsightEvent>();

        private InsightObjectPool<InsightEvent> _insightEventsPool;

        private float _lastTickTime = 0;

        private TrackingManagerWorker _worker;
        public InsightSettingsSO insightSettings;
        public InsightSettingsSO InsightSettings { get { return insightSettings; } }

        private void Awake()
        {
            PersistentDataPath = Application.persistentDataPath;
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                _instance = this;
            }

            DontDestroyOnLoad(gameObject);

            _insightEventsPool = new InsightObjectPool<InsightEvent>(() => new InsightEvent(), EventsInPacketExpectedUpperEnd);
            _destroyedObjectIds = new Queue<uint>(NumTrackedObjectsExpectedUpperEnd);

            if (!PlayerPrefs.HasKey("insight_id"))
            {
                PlayerPrefs.SetString("insight_id", Guid.NewGuid().ToString());
                PlayerPrefs.Save();
            }

            _userId = PlayerPrefs.GetString("insight_id");

            var sessionId = Guid.NewGuid().ToString();
            _worker = new TrackingManagerWorker(insightSettings, token, sessionId, Time.unscaledTime, TickRate,
                Application.version, logLevel, _userId);
            recordOnStart = true;
            // Debug.Log("recordOnStartMaxLengthMinutes  " + recordOnStartMaxLengthMinutes);
            if (recordOnStart)
            {
                StartRecording(recordOnStartMaxLengthMinutes);
            }

            AssignHandAnchors();
        }

        private void AssignHandAnchors()
        {
            var leftHandAnchorObj = GameObject.FindObjectOfType<InsightTrackHandAnchor>();
            if (leftHandAnchorObj != null && leftHandAnchorObj.hand == InsightTrackHandAnchor.Hand.Left)
            {
                leftHandAnchor = leftHandAnchorObj.transform;
                if (leftHandAnchorObj.IsAutoHands)
                {
                    Left_AutoHandsBendableObjects = new List<GameObject>
                    {
                        FindDeepChild(leftHandAnchor, "thumb_01")?.gameObject,
                        FindDeepChild(leftHandAnchor, "index_01")?.gameObject,
                        FindDeepChild(leftHandAnchor, "middle_01")?.gameObject,
                        FindDeepChild(leftHandAnchor, "ring_01")?.gameObject,
                        FindDeepChild(leftHandAnchor, "pinky_01")?.gameObject
                    };
                }
            }

            var rightHandAnchorObj = GameObject.FindObjectOfType<InsightTrackHandAnchor>();
            if (rightHandAnchorObj != null && rightHandAnchorObj.hand == InsightTrackHandAnchor.Hand.Right)
            {
                rightHandAnchor = rightHandAnchorObj.transform;
                if (rightHandAnchorObj.IsAutoHands)
                {
                    Right_AutoHandsBendableObjects = new List<GameObject>
                    {
                        FindDeepChild(rightHandAnchor, "thumb_01")?.gameObject,
                        FindDeepChild(rightHandAnchor, "index_01")?.gameObject,
                        FindDeepChild(rightHandAnchor, "middle_01")?.gameObject,
                        FindDeepChild(rightHandAnchor, "ring_01")?.gameObject,
                        FindDeepChild(rightHandAnchor, "pinky_01")?.gameObject
                    };
                }
            }
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                var result = FindDeepChild(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void Update()
        {
            if (_worker == null)
            {
                return;
            }

            _worker.unscaledTime = Time.unscaledTime;

            if (Input.GetKeyDown(KeyCode.S))
            {

            }

            _worker.sceneName = SceneManager.GetActiveScene().name;
        }

        private void LateUpdate()
        {
            if (_worker == null)
            {
                return;
            }

            var startTime = Time.realtimeSinceStartup;
            var ticked = false;

            if (Time.unscaledTime >= _recordingAutoStopUnscaledTime && _recording)
            {
                StopRecording();
            }

            var tickInterval = 1f / TickRate;
            var timeSinceLastTick = Time.unscaledTime - _lastTickTime;

            _worker.WaitOne();

            if (timeSinceLastTick >= tickInterval && _recording)
            {
                _lastTickTime = Time.unscaledTime;

                HandleTick(startTime);
                ticked = true;
            }

            while (_insightEvents.Count > 0)
            {
                var insightEvent = _insightEvents.Dequeue();

                var workerInsightEvent = _worker.AddInsightEvent();
                workerInsightEvent.userId = _userId;
                workerInsightEvent.name = insightEvent.name;
                workerInsightEvent.session = _worker.SessionId;
                workerInsightEvent.timestamp = insightEvent.timestamp;

                _insightEventsPool.Return(insightEvent);
            }

            _worker.ReleaseMutex();

            var time = Time.realtimeSinceStartup - startTime;
            if (ticked && logLevel >= InsightLogLevel.PerformanceProfiling)
            {
                InsightUtility.Log(time * 1000f + "ms");
            }
        }

        public void StartRecording()
        {
            StartRecording(float.MaxValue);
        }

        public void StartRecording(float maxLengthMinutes)
        {
            if (_worker == null && logLevel >= InsightLogLevel.Warning)
            {
                InsightUtility.LogWarning("No worker.");
            }

            if (!_recording)
            {
                _recording = true;
                _recordingAutoStopUnscaledTime = Time.unscaledTime + (maxLengthMinutes * 60f);
                _recordingStartTime = Time.realtimeSinceStartup;  // Use realtimeSinceStartup
                Insight.RecordingStartTime = _recordingStartTime; // Set recording start time in Insight class
            }
        }

        public void StopRecording()
        {
            if (_recording)
            {
                _recording = false;
                _recordingAutoStopUnscaledTime = float.MaxValue;
            }
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
            _worker.SetUserId(userId);
        }

        public uint GetTrackedObjectInstanceId()
        {
            if (_lastUsedTrackedObjectInstanceId == uint.MaxValue && logLevel >= InsightLogLevel.Warning)
            {
                InsightUtility.LogWarning(
                    $"Out of tracked object ids! Overflowing!!! Why are you trying to track {uint.MaxValue} different objects!?!?!?!?");
            }

            _lastUsedTrackedObjectInstanceId++;
            return _lastUsedTrackedObjectInstanceId;
        }

        public void RegisterObject(InsightTrackObject trackObject)
        {
            if (_trackedObjects.Count == ushort.MaxValue)
            {
                if (logLevel >= InsightLogLevel.Warning)
                {
                    InsightUtility.LogWarning($"cannot track more than {ushort.MaxValue} objects.");
                }

                return;
            }

            _trackedObjects.Add(trackObject);
            _trackedObjectTransforms.Add(trackObject.transform);
            _trackedObjectGameObjects.Add(trackObject.gameObject);
        }

        public void UnregisterObject(InsightTrackObject trackObject)
        {
            if (_trackedObjects.Contains(trackObject))
            {
                _destroyedObjectIds.Enqueue(trackObject.instanceId);
                _trackedObjects.Remove(trackObject);
                _trackedObjectTransforms.Remove(trackObject.transform);
                _trackedObjectGameObjects.Remove(trackObject.gameObject);
            }
        }

        public void TrackEvent(string eventName)
        {
            if (_worker == null)
            {
                return;
            }

            var insightEvent = _insightEventsPool.Get();
            insightEvent.name = eventName;
            insightEvent.timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            _insightEvents.Enqueue(insightEvent);

            // Log when an event is enqueued
            if (logLevel >= InsightLogLevel.Info)
            {
                Debug.Log($"Event '{eventName}' enqueued at {insightEvent.timestamp}");
            }
        }


        private void HandleTick(float startTime)
        {
            var tickData = _worker.AddInsightTickData()
                .Init(DateTime.UtcNow.Ticks, Time.unscaledTime, Time.deltaTime, 0);
            var numObjectsToTrack = _trackedObjects.Count;
            if (numObjectsToTrack >= NumTrackedObjectsExpectedUpperEnd)
            {
                if (logLevel >= InsightLogLevel.Warning)
                {
                    InsightUtility.LogWarning(
                        $"Tick contains {numObjectsToTrack} entries. Only tracking first {NumTrackedObjectsExpectedUpperEnd} objects, any extra may not be tracked.");
                }

                numObjectsToTrack = NumTrackedObjectsExpectedUpperEnd;
            }

            string sceneName = SceneManager.GetActiveScene().name; // Get the active scene name

            for (int i = 0; i < numObjectsToTrack; i++)
            {
                var trackedObject = _trackedObjects[i];
                InsightTrackedObjectData insightTrackedObjectData = _worker.AddInsightTrackedObjectData(tickData);
                insightTrackedObjectData.Init(trackedObject.instanceId, trackedObject.prefabId,
                    _trackedObjectTransforms[i], _trackedObjectGameObjects[i].activeInHierarchy,
                    trackedObject.animator, sceneName);

                // Track BendOffset for left hand objects if IsAutoHands is true and Left_AutoHandsBendableObjects is not null
                if (InsightTrackHandAnchor.LeftHandInstance != null && InsightTrackHandAnchor.LeftHandInstance.IsAutoHands && Left_AutoHandsBendableObjects != null)
                {
                    foreach (var obj in Left_AutoHandsBendableObjects)
                    {
                        if (obj != null)
                        {
                            var bendableComponent = obj.GetComponent<IBendable>();
                            if (bendableComponent != null)
                            {
                                insightTrackedObjectData.leftHandBendOffsets[bendableComponent.Name] = bendableComponent.BendOffset;
                            }
                        }
                    }
                }

                // Track BendOffset for right hand objects if IsAutoHands is true and Right_AutoHandsBendableObjects is not null
                if (InsightTrackHandAnchor.RightHandInstance != null && InsightTrackHandAnchor.RightHandInstance.IsAutoHands && Right_AutoHandsBendableObjects != null)
                {
                    foreach (var obj in Right_AutoHandsBendableObjects)
                    {
                        if (obj != null)
                        {
                            var bendableComponent = obj.GetComponent<IBendable>();
                            if (bendableComponent != null)
                            {
                                insightTrackedObjectData.rightHandBendOffsets[bendableComponent.Name] = bendableComponent.BendOffset;
                            }
                        }
                    }
                }
            }

            if (centerEye)
            {
                _worker.AddInsightTrackedObjectData(tickData).Init((uint)ReservedTrackedObjectInstanceIds.CenterEye, 0,
                    centerEye, centerEye.gameObject.activeInHierarchy, null, sceneName);
            }

            if (leftHandAnchor)
            {
                _worker.AddInsightTrackedObjectData(tickData).Init((uint)ReservedTrackedObjectInstanceIds.LeftHand, 0,
                    leftHandAnchor, leftHandAnchor.gameObject.activeInHierarchy, null, sceneName);
            }

            if (rightHandAnchor)
            {
                _worker.AddInsightTrackedObjectData(tickData).Init((uint)ReservedTrackedObjectInstanceIds.RightHand, 0,
                    rightHandAnchor, rightHandAnchor.gameObject.activeInHierarchy, null, sceneName);
            }

            while (_destroyedObjectIds.Count > 0)
            {
                var id = _destroyedObjectIds.Dequeue();
                tickData.destroyedObjectIds.Add(id);
            }

            tickData.handleTickTime = Time.realtimeSinceStartup - startTime;
        }

        private void OnDestroy()
        {
            if (_worker == null)
            {
                return;
            }

            _worker.ShouldStop();
            const int timeout = 5000;
            if (!_worker.Join(timeout) && logLevel >= InsightLogLevel.Warning)
            {
                InsightUtility.LogWarning($"Unable to stop worker in {timeout}ms. Thread terminated.");
            }
        }
    }
}

namespace InsightDesk
{
    public interface IBendable
    {
        float BendOffset { get; set; }
        string Name { get; }
    }
}
