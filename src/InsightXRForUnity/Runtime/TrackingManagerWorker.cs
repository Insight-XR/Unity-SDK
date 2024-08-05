using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace InsightDesk
{
    public class TrackingManagerWorker
    {
        public const short ReplayVersion = 1;
        private readonly InsightSettingsSO settings;
        private float _lastFrameTime;
        private int _frameCount;

        private readonly Thread _thread;
        private bool _shouldStop = false;
        public float unscaledTime = 0;
        private readonly short _tickRate;
        private readonly string _appVersion;
        private string _sessionId;
        public string SessionId => _sessionId;
        private int _nextChunkId = 0;
        private string _token = "";
        private int _numTicksWrittenToBuffer = 0;
        private long _firstTickTimeTicks = 0;
        private long _lastTickTimeTicks = 0;
        private const string FailedChunksDir = "FailedChunks";
        private const float MaxTimeBeforeProcessingBuffer = 0.5f;
        private const int AllowedNumOutstandingHttpRequests = 1000;
        private const string InsightDeskCacheDir = "Assets/InsightDeskCache";
        private static bool _isImmersion = false;
        private static int _immersionCount = 0;

        private GeoLocationProvider geoLocationProvider;
        private bool isGeolocationReady = false;
        private GeolocationData geolocationData;

        string customerID;
        string userID;
        string _apiKey;

        private string _startDateTime;
        private string _endDateTime;
        private string _sessionDuration;
        private TrackingManager.InsightLogLevel _logLevel;

        private string _userId = "";

        private TextMeshProUGUI _statusText;

        public string sceneName;
        private string currentSceneName = "null";
        private Material currentSkybox = null; // New field to track the current skybox
        private static string currentSkyboxName = null; // New field to store the current skybox name
        private static bool newSkybox = false; // New field to track if the skybox has changed
        private static int currentfps = 0;
        private static bool newfps = false;
        // VR Header Data
        private VRHeaderEntry _vrHeaderEntry;

        private bool _vrHeaderWritten = false;

        // replay buffer stuff
        private const int ReplayBufferLength = 1024 * 1024; // 1 MiB
        private readonly InsightBuffer _replayBuffer = new InsightBuffer(ReplayBufferLength);
        private float _lastReplayBufferProcessUnscaledTime = 0;

        private readonly InsightObjectPool<byte[]> _replayByteArrayPool =
            new InsightObjectPool<byte[]>(() => new byte[ReplayBufferLength], AllowedNumOutstandingHttpRequests);

        // event buffer stuff
        private const int EventBufferLength = 1024 * 200; // 200 KiB
        private readonly InsightBuffer _eventBuffer = new InsightBuffer(EventBufferLength);
        private float _lastEventBufferProcessUnscaledTime = 0;

        private readonly InsightObjectPool<byte[]> _eventByteArrayPool =
            new InsightObjectPool<byte[]>(() => new byte[EventBufferLength], AllowedNumOutstandingHttpRequests);

        private const string JsonStart = "{\"batch\":[";

        // fake data just so we know if the real data will fit in the buffer later
        private const string JsonEnd =
            "],\"timestamp\":\"yyyy-MM-ddTHH:mm:ss.fffZ\",\"sentAt\":\"yyyy-MM-ddTHH:mm:ss.fffZ\"}";

        private int _headerLength = 0;

        private bool _sentInitialChunk = false;

        private readonly List<TickData> _tickDataWorkerCopy =
            new List<TickData>(TrackingManager.TicksInPipelineExpectedUpperEnd);

        private readonly List<InsightEvent> _eventsWorkerCopy =
            new List<InsightEvent>(TrackingManager.EventsInPacketExpectedUpperEnd);

        private readonly InsightTrackedObjectDataChangeTracker insightTrackedObjectDataChangeTracker =
            new InsightTrackedObjectDataChangeTracker();

        private HttpClient _httpClient = new HttpClient();

        //---shared resources--- (make sure to use mutex before accessing any of these)
        private readonly Mutex _mutex = new Mutex();

        private readonly List<TickData> _tickData =
            new List<TickData>(TrackingManager.TicksInPipelineExpectedUpperEnd);

        private readonly List<InsightEvent> _events =
            new List<InsightEvent>(TrackingManager.EventsInPacketExpectedUpperEnd);

        private readonly InsightObjectPool<TickData> _insightTickDataPool =
            new InsightObjectPool<TickData>(() => new TickData(),
                TrackingManager.TicksInPipelineExpectedUpperEnd);

        private readonly InsightObjectPool<InsightTrackedObjectData> _insightTrackedObjectDataPool =
            new InsightObjectPool<InsightTrackedObjectData>(() => new InsightTrackedObjectData(),
                TrackingManager.NumTrackedObjectsExpectedUpperEnd *
                TrackingManager.TicksInPipelineExpectedUpperEnd);

        private readonly InsightObjectPool<InsightEvent> _insightEventsPool =
            new InsightObjectPool<InsightEvent>(() => new InsightEvent(),
                TrackingManager.EventsInPacketExpectedUpperEnd);
        //----------------------

        // Reference to the FPSManager
        //private FPSManager _fpsManager;

        public TrackingManagerWorker(InsightSettingsSO settings, string token, string sessionId, float currentTime, short tickRate,
    string appVersion, TrackingManager.InsightLogLevel logLevel, string userId)
        {
            if (settings == null)
            {
                Debug.LogError("Error: InsightSettingsSO is null. Please go to Project Settings -> InsightXR, add Customer ID, API Key, and User ID, and click save. A ScriptableObject will be created. Assign that ScriptableObject to the 'settings' field in the TrackingManager script component on the InsightTrackingManager GameObject.");
                return; // Exit the constructor to prevent further initialization
            }

            var geoProviderObject = new GameObject("GeoLocationProvider");
            geoLocationProvider = geoProviderObject.AddComponent<GeoLocationProvider>();
            geoLocationProvider.OnGeolocationFetched += OnGeolocationFetched;

            // Add the GeoLocationProvider to the scene
            UnityEngine.Object.DontDestroyOnLoad(geoProviderObject);
            _frameCount = 0;

            this.settings = settings; // Assign the ScriptableObject
            _token = token;
            _sessionId = sessionId;
            unscaledTime = currentTime;
            _tickRate = tickRate;
            _appVersion = appVersion;
            _logLevel = logLevel;
            _userId = userId;

            _startDateTime = DateTime.UtcNow.ToString("MM-dd-yyyy HH:mm:ss");

            // Set customerID, userID, and _apiKey from the ScriptableObject
            customerID = settings.customerID;
            userID = settings.userID;
            _apiKey = settings.apiKey;

            // Initialize device details
            InitializeDeviceDetails();
            // Upload locally saved chunks at the start of the session
            Task.Run(async () => await RetryFailedChunks());

            _thread = new Thread(Process);
            _thread.Start();

            initTimeStamp = DateTime.Now.ToString();

            PeriodicRetryFailedChunks();
            Debug.Log("Started Recording");
            // Initialize FPSManager
            Debug.Log($"UserID: {userID}\n SessionID: {_sessionId}");


        }

        private void OnGeolocationFetched(GeolocationData data)
        {
            geolocationData = data;
            isGeolocationReady = true;
        }

        ~TrackingManagerWorker()
        {
            _replayBuffer.Free();
            _eventBuffer.Free();
        }

        private void Process()
        {
            Start();

            while (!_shouldStop)
            {
                Update();
                Thread.Sleep(1);
            }
            Stop();
        }

        private void Start()
        {
        }

        private void Stop()
        {
            if (_numTicksWrittenToBuffer > 0)
            {
                ProcessReplayBuffer(1);
            }

            // write empty end chunk
            _firstTickTimeTicks = DateTime.UtcNow.Ticks;
            _lastTickTimeTicks = DateTime.UtcNow.Ticks;
            if (_sentInitialChunk)
            {
                ProcessReplayBuffer(2);
            }

            if (_eventBuffer.ofs > 0)
            {
                ProcessEventBuffer();
            }

            _replayBuffer.Free();
            _eventBuffer.Free();

            while (_replayByteArrayPool.NumOutstandingObjects() > 0 ||
                   _eventByteArrayPool.NumOutstandingObjects() > 0)
            {
                Thread.Sleep(1);
            }
            Debug.Log("Stopped Recording");
        }

        private void Update()
        {
            WaitOne();

            for (int i = 0; i < _tickData.Count; i++)
            {
                _tickDataWorkerCopy.Add(_tickData[i]);
            }

            for (int i = 0; i < _events.Count; i++)
            {
                _eventsWorkerCopy.Add(_events[i]);
            }

            _tickData.Clear();
            _events.Clear();

            ReleaseMutex();

            if (_tickDataWorkerCopy.Count > TrackingManager.TicksInPipelineExpectedUpperEnd &&
                _logLevel >= TrackingManager.InsightLogLevel.Warning)
            {
                InsightUtility.LogWarning(
                    $"_tickDataWorkerCopy grew too large (Count: {_tickDataWorkerCopy.Count}). Removing oldest tick(s) in copy");
            }

            while (_tickDataWorkerCopy.Count > TrackingManager.TicksInPipelineExpectedUpperEnd)
            {
                _tickDataWorkerCopy.RemoveAt(0);
            }

            if (_tickDataWorkerCopy.Count > 0 && !_sentInitialChunk)
            {
                _sentInitialChunk = true;
                SendInitialChunkAndPrepareBuffer();
            }

            for (int i = 0; i < _tickDataWorkerCopy.Count; i++)
            {
                HandleTick(_tickDataWorkerCopy[i]);
            }

            for (int i = 0; i < _eventsWorkerCopy.Count; i++)
            {
                HandleEvent(_eventsWorkerCopy[i]);
            }

            WaitOne();

            for (int i = 0; i < _tickDataWorkerCopy.Count; i++)
            {
                var tickData = _tickDataWorkerCopy[i];
                for (int j = 0; j < tickData.objectData.Count; j++)
                {
                    _insightTrackedObjectDataPool.Return(tickData.objectData[j]);
                }

                _insightTickDataPool.Return(tickData);
            }

            for (int i = 0; i < _eventsWorkerCopy.Count; i++)
            {
                _insightEventsPool.Return(_eventsWorkerCopy[i]);
            }

            ReleaseMutex();

            _tickDataWorkerCopy.Clear();
            _eventsWorkerCopy.Clear();

            if (unscaledTime - _lastReplayBufferProcessUnscaledTime > MaxTimeBeforeProcessingBuffer &&
                _headerLength > 0 &&
                _replayBuffer.ofs > _headerLength)
            {
                ProcessReplayBuffer(1);
            }

            if (unscaledTime - _lastEventBufferProcessUnscaledTime > MaxTimeBeforeProcessingBuffer &&
                _eventBuffer.ofs > 0)
            {
                ProcessEventBuffer();
            }

            // Add null check for FPSManager


            // Check and set skybox if changed
        }

        public static void SetSkybox(string skyboxName)
        {
            currentSkyboxName = skyboxName;
            newSkybox = true;
        }
        public static void setfps(int fpsnow)
        {
            currentfps = fpsnow;
            newfps = true;
        }
        public static void SetImmersionData(bool isImmersion)
        {
            _isImmersion = isImmersion;
        }

        private static string ConvertUtcToIst(DateTime utcDateTime)
        {
            DateTime istDateTime = utcDateTime;
            return istDateTime.ToString("MM-dd-yyyy HH:mm:ss");
        }

        private void InitializeDeviceDetails()
        {
            string deviceName = OVRPlugin.GetSystemHeadsetType().ToString();
            float displayFrequency = OVRPlugin.systemDisplayFrequency;
            string pcName = SystemInfo.deviceName;
            string cpuDetails = SystemInfo.processorType;
            string gpuDetails = SystemInfo.graphicsDeviceName;
            float batteryLevel = SystemInfo.batteryLevel * 100; // Convert to percentage
            string operatingsystem = SystemInfo.operatingSystem.ToString();
            string processortype = SystemInfo.processorType.ToString();
            string processorfrequency = SystemInfo.processorFrequency.ToString();
            string memorysize = SystemInfo.systemMemorySize.ToString();
            string engine = "Unity"; // Example value
            string engineVersion = Application.unityVersion; // Example value
            string projectName = Application.productName; // Example value
            // Initialize VRHeaderEntry with placeholder geolocation data
            _vrHeaderEntry = new VRHeaderEntry(deviceName, displayFrequency, pcName, cpuDetails, gpuDetails, batteryLevel, operatingsystem, memorysize, processorfrequency, "", "", "", 0, 0, engine, engineVersion, projectName);
        }

        private void SendInitialChunkAndPrepareBuffer()
        {
            _numTicksWrittenToBuffer = 0;
            ChunkHeaderEntry.Write(_replayBuffer, ReplayVersion, _appVersion, _tickRate, _numTicksWrittenToBuffer);
            // Debug.Log("Chunk");
            // Wait for geolocation data if not ready
            while (!isGeolocationReady)
            {
                Thread.Sleep(10);
            }

            if (!_vrHeaderWritten)
            {
                float latitude = 0;
                float longitude = 0;
                string city = "";
                string region = "";
                string country = "";

                if (geolocationData != null && !string.IsNullOrEmpty(geolocationData.loc))
                {
                    string[] locParts = geolocationData.loc.Split(',');
                    if (locParts.Length == 2)
                    {
                        float.TryParse(locParts[0], out latitude);
                        float.TryParse(locParts[1], out longitude);
                    }
                    city = geolocationData.city;
                    region = geolocationData.region;
                    country = geolocationData.country;
                }

                _vrHeaderEntry.city = city;
                _vrHeaderEntry.country = country;
                _vrHeaderEntry.region = region;
                _vrHeaderEntry.latitude = latitude;
                _vrHeaderEntry.longitude = longitude;

                VRHeaderEntry.Write(_replayBuffer, _vrHeaderEntry.deviceName, _vrHeaderEntry.displayFrequency, _vrHeaderEntry.pcName,
                    _vrHeaderEntry.cpuDetails, _vrHeaderEntry.gpuDetails, _vrHeaderEntry.batteryLevel, _vrHeaderEntry.operatingsystem,
                    _vrHeaderEntry.memorysize, _vrHeaderEntry.processorfrequency, city, country, region, latitude, longitude, _vrHeaderEntry.engine, _vrHeaderEntry.engineVersion, _vrHeaderEntry.projectName);
                // Debug.Log("VR");
                _vrHeaderWritten = true;
            }

            // Debug.Log(_vrHeaderEntry.deviceName + _vrHeaderEntry.displayFrequency + _vrHeaderEntry.pcName +
            //           _vrHeaderEntry.cpuDetails + _vrHeaderEntry.gpuDetails + _vrHeaderEntry.batteryLevel + _vrHeaderEntry.operatingsystem +
            //           _vrHeaderEntry.memorysize + _vrHeaderEntry.processorfrequency);
            _headerLength = _replayBuffer.ofs;
            _firstTickTimeTicks = DateTime.UtcNow.Ticks;
            _lastTickTimeTicks = DateTime.UtcNow.Ticks;
            ProcessReplayBuffer(0);
            if (_logLevel >= TrackingManager.InsightLogLevel.Info)
            {
                InsightUtility.Log("Starting session: " + _sessionId);
            }
        }

        private void HandleTick(TickData insightTickData)
        {
            bool newScene = false;

            // Check if the scene has changed
            if (!string.IsNullOrEmpty(sceneName) && sceneName != currentSceneName)
            {
                newScene = true;
                currentSceneName = sceneName;
            }

            // Use the current skybox name and reset the flag
            string skyboxName = currentSkyboxName;
            bool isNewSkybox = newSkybox;

            newSkybox = false;
            int fpsnow = currentfps;
            bool isnewfps = newfps;
            newfps = false;

            if (!Insight.HasEventWritten && Insight.IsEvent)
            {
                if (!TryWriteTick(insightTickData, newScene, currentSceneName, Insight.IsEvent, Insight.EventName, isNewSkybox, skyboxName, isnewfps, fpsnow))
                {
                    ProcessReplayBuffer(1);
                    if (!TryWriteTick(insightTickData, newScene, currentSceneName, Insight.IsEvent, Insight.EventName, isNewSkybox, skyboxName, isnewfps, fpsnow) && _logLevel >= TrackingManager.InsightLogLevel.Warning)
                    {
                        InsightUtility.LogWarning("Could not write tick even after clearing buffer");
                    }
                }
                Insight.HasEventWritten = true; // Mark the event as written
            }
            else
            {
                if (!TryWriteTick(insightTickData, newScene, currentSceneName, false, null, isNewSkybox, skyboxName, isnewfps, fpsnow))
                {
                    ProcessReplayBuffer(1);
                    if (!TryWriteTick(insightTickData, newScene, currentSceneName, false, null, isNewSkybox, skyboxName, isnewfps, fpsnow) && _logLevel >= TrackingManager.InsightLogLevel.Warning)
                    {
                        InsightUtility.LogWarning("Could not write tick even after clearing buffer");
                    }
                }
            }
        }


        private bool TryWriteTick(TickData insightTickData, bool newScene, string sceneName, bool isEvent, string eventName, bool newSkybox, string skyboxName, bool isnewfps, int fpsnow)
        {
            var startOfs = _replayBuffer.ofs;
            try
            {
                WriteTick(insightTickData, newScene, sceneName, isEvent, eventName, newSkybox, skyboxName, isnewfps, fpsnow);
                _numTicksWrittenToBuffer++;
                if (_numTicksWrittenToBuffer == 1)
                {
                    _firstTickTimeTicks = insightTickData.timeTicks;
                }

                _lastTickTimeTicks = insightTickData.timeTicks;
            }
            catch (InvalidOperationException)
            {
                _replayBuffer.ofs = startOfs;
                return false;
            }

            return true;
        }

        private void WriteTick(TickData insightTickData, bool newScene, string sceneName, bool isEvent, string eventName, bool newSkybox, string skyboxName, bool isnewfps, int fpsnow)
        {
            var startOfs = _replayBuffer.ofs;
            TickEntry.Write(_replayBuffer, insightTickData.timeTicks, insightTickData.unscaledTime,
                insightTickData.deltaTime, insightTickData.handleTickTime, 0, 0, newScene, sceneName, _isImmersion, isEvent, eventName, newSkybox, skyboxName, isnewfps, fpsnow);
            ushort numObjects = 0;
            ushort numDeleted = 0;

            for (int i = 0; i < insightTickData.objectData.Count; i++)
            {
                var trackedObject = insightTickData.objectData[i];
                if (trackedObject.prefabId == 0)
                {
                    continue;
                }
                var (newActive, newPos, newRot, newScale, newFloats, newInts, newBools, newTriggers, newTexts, newLeftHandBendOffsets, newRightHandBendOffsets, textSize) =
                    insightTrackedObjectDataChangeTracker.GetTrackedObjectIsNew(trackedObject);
                var newAnimations = newFloats.Count > 0 || newInts.Count > 0 || newBools.Count > 0 || newTexts.Count > 0 || newLeftHandBendOffsets.Count > 0 || newRightHandBendOffsets.Count > 0;
                if (newActive || (trackedObject.activeInHierarchy && (newPos || newRot || newScale || newAnimations)))
                {
                    bool isAutoHands = false;
                    if (InsightTrackHandAnchor.LeftHandInstance != null && InsightTrackHandAnchor.LeftHandInstance.IsAutoHands)
                    {
                        isAutoHands = true;
                    }
                    else if (InsightTrackHandAnchor.RightHandInstance != null && InsightTrackHandAnchor.RightHandInstance.IsAutoHands)
                    {
                        isAutoHands = true;
                    }

                    ObjectEntry.Write(_replayBuffer, trackedObject.instanceId, trackedObject.prefabId, trackedObject.parentPrefabId,
                        trackedObject, newPos, newRot, newScale, newFloats, newInts, newBools, newTriggers, newTexts, textSize, newLeftHandBendOffsets, newRightHandBendOffsets, _logLevel, isAutoHands);

                    numObjects++;
                }
            }

            for (int i = 0; i < insightTickData.destroyedObjectIds.Count; i++)
            {
                var id = insightTickData.destroyedObjectIds[i];
                DestroyObjectEntry.Write(_replayBuffer, id);
                numDeleted++;

                insightTrackedObjectDataChangeTracker.RemoveLastDataFor(id);
            }

            var endOfs = _replayBuffer.ofs;
            _replayBuffer.ofs = startOfs;
            TickEntry.Write(_replayBuffer, insightTickData.timeTicks, insightTickData.unscaledTime,
                insightTickData.deltaTime, insightTickData.handleTickTime, numObjects, numDeleted, newScene, sceneName, _isImmersion, isEvent, eventName, newSkybox, skyboxName, isnewfps, fpsnow);
            _replayBuffer.ofs = endOfs;
        }





        private void HandleEvent(InsightEvent insightEvent)
        {
            if (!TryWriteEvent(insightEvent))
            {
                ProcessEventBuffer();
                if (!TryWriteEvent(insightEvent) && _logLevel >= TrackingManager.InsightLogLevel.Warning)
                {
                    InsightUtility.LogWarning("Could not write event even after clearing buffer");
                }
            }
        }

        private bool TryWriteEvent(InsightEvent insightEvent)
        {
            var startOfs = _eventBuffer.ofs;
            try
            {
                WriteEvent(insightEvent);
            }
            catch (InvalidOperationException)
            {
                _eventBuffer.ofs = startOfs;
                return false;
            }

            return true;
        }

        private void WriteEvent(InsightEvent insightEvent)
        {
            if (_eventBuffer.ofs == 0)
            {
                _eventBuffer.WriteCharArray(JsonStart);
                _eventBuffer.WriteCharArray(JsonEnd);
            }

            _eventBuffer.ofs -= JsonEnd.Length;

            if (_eventBuffer.ofs != JsonStart.Length)
            {
                _eventBuffer.WriteCharArray(",");
            }

            _eventBuffer.WriteCharArray("{\"userId\":\"");
            _eventBuffer.WriteCharArray(insightEvent.userId);
            _eventBuffer.WriteCharArray("\",\"name\":\"");
            _eventBuffer.WriteCharArray(insightEvent.name);
            _eventBuffer.WriteCharArray("\",\"session\":\"");
            _eventBuffer.WriteCharArray(insightEvent.session);
            _eventBuffer.WriteCharArray("\",\"timestamp\":\"");
            _eventBuffer.WriteCharArray(insightEvent.timestamp);
            _eventBuffer.WriteCharArray("\"}");
            _eventBuffer.WriteCharArray(JsonEnd);
        }

        public TickData AddInsightTickData()
        {
            var insightTickData = _insightTickDataPool.Get();
            _tickData.Add(insightTickData);
            return insightTickData;
        }

        public InsightTrackedObjectData AddInsightTrackedObjectData(TickData insightTickData)
        {
            var insightTrackedObjectData = _insightTrackedObjectDataPool.Get();
            insightTickData.objectData.Add(insightTrackedObjectData);
            return insightTrackedObjectData;
        }

        public InsightEvent AddInsightEvent()
        {
            var insightEvent = _insightEventsPool.Get();
            _events.Add(insightEvent);
            return insightEvent;
        }

        private async void ProcessReplayBuffer(int chunkType)
        {
            var chunkId = _nextChunkId;
            var start = ConvertUtcToIst(new DateTime(_firstTickTimeTicks));
            var end = ConvertUtcToIst(new DateTime(_lastTickTimeTicks));
            _nextChunkId++;

            _lastReplayBufferProcessUnscaledTime = unscaledTime;
            _firstTickTimeTicks = 0;
            _lastTickTimeTicks = 0;

            if (_replayByteArrayPool.PoolSize() == 0)
            {
                _replayBuffer.ofs = 0;
                _numTicksWrittenToBuffer = 0;
                insightTrackedObjectDataChangeTracker.ClearLastData();
                ChunkHeaderEntry.Write(_replayBuffer, ReplayVersion, _appVersion, _tickRate, _numTicksWrittenToBuffer);

                if (_logLevel >= TrackingManager.InsightLogLevel.Warning)
                {
                    InsightUtility.LogWarning($"_byteArrayPool has no objects. Skipping chunk {_nextChunkId}");
                }

                return;
            }

            // Re-write header to have correct num ticks, then reset ofs back to original value
            var ofs = _replayBuffer.ofs;
            _replayBuffer.ofs = 0;
            ChunkHeaderEntry.Write(_replayBuffer, ReplayVersion, _appVersion, _tickRate, _numTicksWrittenToBuffer);
            _replayBuffer.ofs = ofs;

            var bytes = _replayByteArrayPool.Get();
            var bytesLength = _replayBuffer.ofs; // Only copy the used portion of the buffer
            Marshal.Copy(_replayBuffer.unmanagedBuffer, bytes, 0, bytesLength);
            _replayBuffer.ofs = 0;
            _numTicksWrittenToBuffer = 0;
            insightTrackedObjectDataChangeTracker.ClearLastData();

            // Write header to new buffer so ticks go after the header
            ChunkHeaderEntry.Write(_replayBuffer, ReplayVersion, _appVersion, _tickRate, _numTicksWrittenToBuffer);

            using var content = new ByteArrayContent(bytes, 0, bytesLength);
            content.Headers.Add("Content-Type", "application/octet-stream");
            content.Headers.Add("token", _token);

            int isLastChunk = 1;
            content.Headers.Add("is_last", isLastChunk.ToString());

            // Try to post the chunk to the server
            bool success = await PostChunkToServer(content, chunkId);
            // Debug.Log(success);
            // Write the chunk to the local cache file regardless of post success
#if UNITY_EDITOR
            AppendChunkToLocalCache(bytes, bytesLength, chunkId);
#endif
            if (!success)
            {
                // Debug.Log("inside1");
                SaveChunkLocally(bytes, chunkId, bytesLength); // Pass the actual length
            }
            _replayByteArrayPool.Return(bytes);
        }
#if UNITY_EDITOR
        private void AppendChunkToLocalCache(byte[] chunk, int length, int chunkId)
        {
            string directoryPath = Path.Combine("Assets", "Sessions", _sessionId);

            string filePath = Path.Combine(directoryPath, $"{_sessionId}_chunk_{chunkId}.txt");

            try
            {
                // Ensure the Sessions directory exists
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Convert the byte array to a base64 string
                var base64String = Convert.ToBase64String(chunk, 0, length);

                // Write the base64 string to a new text file
                using (var streamWriter = new StreamWriter(filePath, false))
                {
                    streamWriter.WriteLine(base64String);
                    // Debug.Log($"Stored chunk {chunkId} to local cache file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                // Debug.LogError($"Failed to store chunk {chunkId} to local cache file: {filePath}. Error: {ex.Message}");
            }
        }
#endif
        private static string initTimeStamp;
        private static int index = 0;

        private static async Task AppendByteArrayContentToFile(ByteArrayContent content)
        {

            string filePath = Path.Combine(TrackingManager.PersistentDataPath, "exampleFile.txt");

            try
            {
                // Read the content as a byte array
                byte[] bytes = await content.ReadAsByteArrayAsync();

                // Open the file stream in append mode
                using (FileStream stream = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                {
                    // Write the bytes to the end of the file
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }

                Console.WriteLine("Content appended to file: " + filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error appending content to file: " + ex.Message);
            }
        }

        private async Task<bool> PostChunkToServer(ByteArrayContent content, int chunkId, string sessionId = null)
        {
            // Use the provided sessionId if available, otherwise use the current sessionId
            var sessionToUse = sessionId ?? _sessionId;

            // Use UTC dates for session duration calculation
            _endDateTime = DateTime.UtcNow.ToString("MM-dd-yyyy HH:mm:ss");

            var startDateTimeUtc = DateTime.ParseExact(_startDateTime, "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            var endDateTimeUtc = DateTime.ParseExact(_endDateTime, "MM-dd-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            _sessionDuration = ((endDateTimeUtc - startDateTimeUtc).TotalMilliseconds).ToString();

            var url = $"http://35.193.4.57/input/session/{customerID}/{userID}/{sessionToUse}/{chunkId}";

            try
            {
                content.Headers.Add("api_key", _apiKey);
                content.Headers.Add("start_date_time", _startDateTime);
                content.Headers.Add("end_date_time", _endDateTime);
                content.Headers.Add("session_duration", _sessionDuration);

                int isLastChunk = 1; // Ensure only one value, either 0 or 1
                content.Headers.Add("is_last", isLastChunk.ToString());

                var response = await _httpClient.PostAsync(url, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //Debug.Log($"Successfully posted chunk {chunkId} for session {sessionToUse} to server.");
                    UpdateStatusText($"Successfully posted chunk {chunkId} for session {sessionToUse} to server.");
                    return true;
                }
                else
                {
                    Debug.LogError($"Post error {(int)response.StatusCode} ({response.StatusCode}) for chunk {chunkId}.");
                    UpdateStatusText($"Failed to post chunk {chunkId} for session {sessionToUse}. Error: {(int)response.StatusCode} ({response.StatusCode}).");
                    return false;
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError($"HttpRequestException: {e.Message} for chunk {chunkId}.");
                UpdateStatusText($"Failed to post chunk {chunkId} for session {sessionToUse}. HttpRequestException: {e.Message}.");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message} for chunk {chunkId}.");
                UpdateStatusText($"Failed to post chunk {chunkId} for session {sessionToUse}. Exception: {e.Message}.");
                return false;
            }
        }

        private async Task RetryFailedChunks()
        {
            string directoryPath;

#if UNITY_EDITOR
    // If running in Unity Editor, use the specific directory on the computer
    directoryPath = Path.Combine(FailedChunksDir);
#else
            // If running in APK build, use persistent data path
            directoryPath = Path.Combine(Application.persistentDataPath, "FailedChunks");
#endif

            if (!Directory.Exists(directoryPath))
            {
                //Debug.Log($"Directory does not exist: {directoryPath}");
                return;
            }

            var files = Directory.GetFiles(directoryPath);
            if (files.Length == 0)
            {
                //Debug.Log($"No files to retry in directory: {directoryPath}");
                return;
            }

            bool anyChunkUploaded = false; // Variable to track successful uploads

            foreach (var file in files)
            {
                var bytes = File.ReadAllBytes(file);

                using var content = new ByteArrayContent(bytes);
                content.Headers.Add("Content-Type", "application/octet-stream");
                content.Headers.Add("token", _token);

                var fileNameParts = Path.GetFileNameWithoutExtension(file).Split('_');
                var sessionId = fileNameParts[0];
                var chunkId = int.Parse(fileNameParts[1]);

                bool success = await PostChunkToServer(content, chunkId, sessionId);
                if (success)
                {
                    File.Delete(file);
                    //Debug.Log($"<color=yellow>Successfully retried and posted chunk {chunkId} for session {sessionId} from local storage.</color>");
                    anyChunkUploaded = true; // Mark that at least one chunk was uploaded
                }
                else
                {
                    Debug.LogError($"Failed to retry chunk {chunkId} for session {sessionId}.");
                }
            }

            if (anyChunkUploaded)
            {
                Debug.Log("<color=green>All previous session data has been uploaded to the server.</color>");
            }
        }


        private void PeriodicRetryFailedChunks()
        {
            var retryThread = new Thread(async () =>
            {
                while (!_shouldStop)
                {
                    await Task.Delay(60000); // Retry every minute
                    await RetryFailedChunks();
                }
            });

            retryThread.Start();
        }

        private void UpdateStatusText(string message)
        {
            if (_statusText != null)
            {
                MainThreadDispatcher.Instance().Enqueue(() =>
                {
                    _statusText.text = message;
                });
            }
        }

        private void SaveChunkLocally(byte[] bytes, int chunkId, int bytesLength)
        {
            string directoryPath;

#if UNITY_EDITOR
    // If running in Unity Editor, save to a specific directory on the computer
    directoryPath = Path.Combine(FailedChunksDir);
#else
            // If running in APK build, save to persistent data path
            directoryPath = Path.Combine(Application.persistentDataPath, "FailedChunks");
#endif

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, $"{_sessionId}_{chunkId}.bin");
            var trimmedBytes = new byte[bytesLength];
            Array.Copy(bytes, 0, trimmedBytes, 0, bytesLength);
            File.WriteAllBytes(filePath, trimmedBytes);

#if UNITY_EDITOR
    Debug.Log($"Saved failed chunk {chunkId} for session {_sessionId} to {filePath} (Editor).");
#else
            Debug.Log($"Saved failed chunk {chunkId} for session {_sessionId} to {filePath}.");
#endif
        }


        private async void ProcessEventBuffer()
        {
            _lastEventBufferProcessUnscaledTime = unscaledTime;

            if (_eventByteArrayPool.PoolSize() == 0)
            {
                _eventBuffer.ofs = 0;

                if (_logLevel >= TrackingManager.InsightLogLevel.Warning)
                {
                    InsightUtility.LogWarning($"_eventByteArrayPool has no objects. Skipping event batch.");
                }

                return;
            }

            _eventBuffer.ofs -= JsonEnd.Length;
            _eventBuffer.WriteCharArray("],\"timestamp\":\"");
            var isoTime = ConvertUtcToIst(DateTime.UtcNow);
            _eventBuffer.WriteCharArray(isoTime);
            _eventBuffer.WriteCharArray("\",\"sentAt\":\"");
            _eventBuffer.WriteCharArray(isoTime);
            _eventBuffer.WriteCharArray("\"}");

            var bytes = _eventByteArrayPool.Get();
            var bytesLength = _eventBuffer.ofs;
            Marshal.Copy(_eventBuffer.unmanagedBuffer, bytes, 0, _eventBuffer.ofs);
            _eventBuffer.ofs = 0;

            var url = $"{TrackingManager.ApiInputBase}/track";

            using var content = new ByteArrayContent(bytes, 0, bytesLength);
            content.Headers.Add("Content-Type", "application/json");
            content.Headers.Add("token", _token);

            int isLastChunk = 1;
            content.Headers.Add("is_last", isLastChunk.ToString());

            // Add additional headers
            content.Headers.Add("api_key", _apiKey);
            content.Headers.Add("start_date_time", _startDateTime);
            content.Headers.Add("end_date_time", _endDateTime);
            content.Headers.Add("session_duration", _sessionDuration);

            var response = await _httpClient.PostAsync(url, content);
            if (response.StatusCode != HttpStatusCode.OK && _logLevel >= TrackingManager.InsightLogLevel.Warning)
            {
                InsightUtility.LogWarning(
                    $"Post error {(int)response.StatusCode} ({response.StatusCode}) for event batch");
            }

            _eventByteArrayPool.Return(bytes);
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
        }

        public void WaitOne()
        {
            _mutex.WaitOne();
        }

        public void ReleaseMutex()
        {
            _mutex.ReleaseMutex();
        }

        public void ShouldStop()
        {
            _shouldStop = true;
            //Debug.Log("Stopped Recording");
        }

        public bool Join(int millisecondsTimeout)
        {
            return _thread.Join(millisecondsTimeout);
        }
    }
}
