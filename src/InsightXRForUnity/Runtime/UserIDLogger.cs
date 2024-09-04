using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class UserIDLogger : MonoBehaviour
    {
        private static InsightSettingsSO insightSettings;

        private void Awake()
        {
            if (insightSettings == null)
            {
                insightSettings = TrackingManager.instance.insightSettings;
            }

            if (insightSettings == null)
            {
                Debug.LogError("InsightSettingsSO asset not found!");
            }
        }

        public static void LogUserID(string userID)
        {
            if (insightSettings == null)
            {
                insightSettings = TrackingManager.instance.insightSettings;
                if (insightSettings == null)
                {
                    Debug.LogError("InsightSettingsSO asset not found!");
                    return;
                }
            }

            insightSettings.userID = userID;

            Insight.Log(insightSettings.userID);
        }
    }
}
