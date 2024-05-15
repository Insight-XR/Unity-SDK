#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
#if UNITY_2023_2_OR_NEWER
using System;
using System.Collections.Generic;
#endif
using UnityEditor;
using UnityEngine.Analytics;

namespace Unity.XR.CoreUtils.Editor.Analytics
{
    /// <summary>
    /// Base class for editor analytics events.
    /// </summary>
    /// <seealso cref="EditorAnalytics"/>
    public abstract class EditorAnalyticsEvent<T>
#if UNITY_2023_2_OR_NEWER
        : IAnalytic where T : IAnalytic.IData
#endif
    {
        /// <summary>
        /// The event name determines which database table it goes into in the backend.
        /// All events which we want grouped into a table must share the same event name.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// The event/table version.
        /// </summary>
        public int EventVersion { get; }

        /// <summary>
        /// Returns <see langword="true"/> if the event is already registered with Unity analytics API, otherwise returns <see langword="false"/>.
        /// </summary>
        public bool Registered { get; private set; }

#if UNITY_2023_2_OR_NEWER
        /// <summary>
        /// The data to send to the analytics server. Fill this data in the <see cref="Send"/> method, and use it in the
        /// <see cref="TryGatherData"/> implementation.
        /// </summary>
        protected Queue<T> m_QueuedData = new Queue<T>();

        /// <summary>
        /// The implementation to gather the data to send to the analytics server.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public virtual bool TryGatherData(out IAnalytic.IData data, out Exception error)
        {
            if (m_QueuedData.Count == 0)
            {
                data = null;
                error = new Exception("No data to send. Do not invoke this method directly, call Send() when " +
                                      "you want to send new analytics data.");
                return false;
            }

            data = m_QueuedData.Dequeue();
            error = null;
            return true;
        }

        /// <summary>
        /// The class constructor.
        /// </summary>
        public EditorAnalyticsEvent() { }
#endif

        /// <summary>
        /// The class constructor.
        /// </summary>
        /// <param name="eventName">This event name.</param>
        /// <param name="eventVersion">This event version.</param>
        public EditorAnalyticsEvent(string eventName, int eventVersion)
        {
            EventName = eventName;
            EventVersion = eventVersion;
        }

        /// <summary>
        /// The implementation to register this event with the analytics server.
        /// </summary>
        /// <returns>The analytics result of the event registration.</returns>
        /// <remarks>
        /// It's recommended to implement this method inside the package editor <c>asmdef/DLL</c> that owns this event.
        /// The editor analytics API may add its invocation info to the payload message.
        /// </remarks>
        protected abstract AnalyticsResult RegisterWithAnalyticsServer();

        /// <summary>
        /// Register this event with the analytics server.
        /// </summary>
        /// <returns>
        /// Returns whenever this event was successfully registered with the analytics server.
        /// Returns <see langword="false"/> if the user disabled analytics.
        /// </returns>
        public bool Register()
        {
            if (!EditorAnalytics.enabled)
                return false;

            if (Registered)
                return Registered;

            var result = RegisterWithAnalyticsServer();

            // AnalyticsResult.TooManyRequests means that we have already registered for this event
            Registered = result == AnalyticsResult.Ok || result == AnalyticsResult.TooManyRequests;
            return Registered;
        }

        /// <summary>
        /// The implementation to send the given parameter as payload to the analytics server.
        /// </summary>
        /// <param name="parameter">The parameter to send.</param>
        /// <returns>The analytics result of the send invocation.</returns>
        /// <remarks>
        /// It's recommended to implement this method inside the package editor <c>asmdef/DLL</c> that owns this event.
        /// The editor analytics API may add its invocation info to the payload message.
        /// </remarks>
        protected abstract AnalyticsResult SendToAnalyticsServer(T parameter);

        /// <summary>
        /// Send the given parameter as payload to the analytics server.
        /// </summary>
        /// <param name="parameter">The parameter object within the event.</param>
        /// <returns>Returns whenever the event was successfully sent. Returns <see langword="false"/> if this event was not registered yet.</returns>
        public bool Send(T parameter)
        {
#if !UNITY_2023_2_OR_NEWER
            if (!Registered)
                return false;
#else
            if (parameter is IAnalytic.IData)
            {
                m_QueuedData.Enqueue(parameter);
            }
            else if (!Registered)
            {
                return false;
            }
#endif

            return SendToAnalyticsServer(parameter) == AnalyticsResult.Ok;
        }
    }
}
#endif // ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
