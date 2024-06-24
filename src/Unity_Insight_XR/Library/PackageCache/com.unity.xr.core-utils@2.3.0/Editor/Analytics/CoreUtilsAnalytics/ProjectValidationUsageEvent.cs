#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
using System;
using UnityEngine;

#if UNITY_2023_2_OR_NEWER
using UnityEngine.Analytics;
#endif

namespace Unity.XR.CoreUtils.Editor.Analytics
{
    /// <summary>
    /// The project validation usage analytics event.
    /// </summary>
#if UNITY_2023_2_OR_NEWER
    [AnalyticInfo(k_EventName, CoreUtilsAnalytics.VendorKey, k_EventVersion, k_MaxEventPerHour, k_MaxItems)]
#endif
    class ProjectValidationUsageEvent : CoreUtilsEditorAnalyticsEvent<ProjectValidationUsageEvent.Payload>
    {
        const string k_EventName = "xrcoreutils_projectvalidation_usage";
        const int k_EventVersion = 1;

        internal const string NoneCategoryName = "[NONE]";

        /// <summary>
        /// The event parameter.
        /// Do not rename any field, the field names are used the identify the table/event column of this event payload.
        /// </summary>
        [Serializable]
        internal struct Payload
#if UNITY_2023_2_OR_NEWER
            : IAnalytic.IData
#endif
        {
            internal const string FixIssuesName = "FixIssues";

            [SerializeField]
            internal string Name;

            [SerializeField]
            internal IssuesStatus[] IssuesStatusByCategory;

#if UNITY_2023_2_OR_NEWER
            [SerializeField]
            internal string package;

            [SerializeField]
            internal string package_ver;
#endif

            internal Payload(string name, IssuesStatus[] issuesStatus)
            {
                Name = name;
                IssuesStatusByCategory = issuesStatus;

#if UNITY_2023_2_OR_NEWER
                package = CoreUtilsAnalytics.PackageName;
                package_ver = CoreUtilsAnalytics.PackageVersion;
#endif
            }
        }

        /// <summary>
        /// The fixed issues status parameter.
        /// Do not rename any field, the field names are used the identify the table/event data of this event payload.
        /// </summary>
        [Serializable]
        internal struct IssuesStatus
        {
            [SerializeField]
            internal string Category;

            [SerializeField]
            internal int SuccessfullyFixed;

            [SerializeField]
            internal int FailedToFix;
        }

#if !UNITY_2023_2_OR_NEWER
        internal ProjectValidationUsageEvent() : base(k_EventName, k_EventVersion) { }
#endif
        internal bool SendFixIssues(IssuesStatus[] issuesStatusByCategory) =>
            Send(new Payload(Payload.FixIssuesName, issuesStatusByCategory));
    }
}
#endif //ENABLE_CLOUD_SERVICES_ANALYTICS
