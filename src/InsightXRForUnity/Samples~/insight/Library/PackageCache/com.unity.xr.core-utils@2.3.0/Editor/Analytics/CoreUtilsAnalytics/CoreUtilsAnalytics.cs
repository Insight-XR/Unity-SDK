#if ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
using UnityEditor;

#if UNITY_2023_2_OR_NEWER
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#endif

namespace Unity.XR.CoreUtils.Editor.Analytics
{
    static class CoreUtilsAnalytics
    {
        internal const string VendorKey = "unity.xrcoreutils";

#if UNITY_2023_2_OR_NEWER
        internal const string PackageName = "com.unity.xr.core-utils";
        internal static readonly string PackageVersion = PackageInfo.FindForPackageName(PackageName).version;
#endif

        internal static readonly ProjectValidationUsageEvent ProjectValidationUsageEvent = new ProjectValidationUsageEvent();
        internal static readonly BuildingBlocksUsageEvent BuildingBlocksUsageEvent = new BuildingBlocksUsageEvent();

#if !UNITY_2023_2_OR_NEWER
        [InitializeOnLoadMethod]
        static void RegisterEvents()
        {
            ProjectValidationUsageEvent.Register();
            BuildingBlocksUsageEvent.Register();
        }
#endif
    }
}
#endif // ENABLE_CLOUD_SERVICES_ANALYTICS || UNITY_2023_2_OR_NEWER
