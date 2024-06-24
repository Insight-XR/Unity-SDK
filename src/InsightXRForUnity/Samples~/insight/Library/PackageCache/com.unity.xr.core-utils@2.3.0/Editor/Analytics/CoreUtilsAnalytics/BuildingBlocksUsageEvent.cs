#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
using System;
using UnityEngine;

#if UNITY_2023_2_OR_NEWER
using UnityEngine.Analytics;
#endif

namespace Unity.XR.CoreUtils.Editor.Analytics
{
    /// <summary>
    /// The building blocks usage analytics event.
    /// </summary>
#if UNITY_2023_2_OR_NEWER
    [AnalyticInfo(k_EventName, CoreUtilsAnalytics.VendorKey, k_EventVersion, k_MaxEventPerHour, k_MaxItems)]
#endif
    class BuildingBlocksUsageEvent : CoreUtilsEditorAnalyticsEvent<BuildingBlocksUsageEvent.Payload>
    {
        const string k_EventName = "xrcoreutils_buildingblocks_usage";
        const int k_EventVersion = 1;

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
            internal const string OverlayButtonClickedName = "OverlayButtonClicked";
            internal const string ToolbarButtonClickedName = "ToolbarButtonClicked";

            [SerializeField]
            internal string Name;

            [SerializeField]
            internal string SectionId;

            [SerializeField]
            internal string BuildingBlockId;

#if UNITY_2023_2_OR_NEWER
            [SerializeField]
            internal string package;

            [SerializeField]
            internal string package_ver;
#endif

            internal Payload(string name, string sectionId, string buildingBlockId)
            {
                Name = name;
                SectionId = sectionId;
                BuildingBlockId = buildingBlockId;

#if UNITY_2023_2_OR_NEWER
                package = CoreUtilsAnalytics.PackageName;
                package_ver = CoreUtilsAnalytics.PackageVersion;
#endif
            }
        }

#if !UNITY_2023_2_OR_NEWER
        internal BuildingBlocksUsageEvent() : base(k_EventName, k_EventVersion)
        {
        }
#endif

        internal bool SendOverlayButtonClicked(string sectionId, string buildingBlockId) =>
            Send(Payload.OverlayButtonClickedName, sectionId, buildingBlockId);

        internal bool SendToolbarButtonClicked(string sectionId, string buildingBlockId) =>
            Send(Payload.ToolbarButtonClickedName, sectionId, buildingBlockId);

        bool Send(string name, string sectionId, string buildingBlockId) =>
            Send(new Payload (name, sectionId, buildingBlockId));
    }
}
#endif
