using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InsightDesk
{
    public class UserIDLogger : MonoBehaviour
    {
        private InsightSettingsSO insightSettings;

        private void Awake()
        {
            if (insightSettings == null)
            {
                insightSettings = TrackingManager.instance.insightSettings;
            }

            if (insightSettings != null)
            {
                //Debug.Log($"User ID found: {insightSettings.userID}");
                Insight.Log(insightSettings.userID);
            }
            else
            {
                Debug.LogError("InsightSettingsSO asset not found!");
            }
        }
    }
}
