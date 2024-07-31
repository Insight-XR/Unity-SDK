#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace InsightDesk
{
    public static class InsightSettings
    {
        private static InsightSettingsSO settings;

        private static InsightSettingsSO Settings
        {
            get
            {
                if (settings == null)
                {
                    LoadSettings();
                }
                return settings;
            }
        }

        public static string CustomerID
        {
            get => Settings.customerID;
            set => Settings.customerID = value;
        }

        public static string UserID
        {
            get => Settings.userID;
            set => Settings.userID = value;
        }

        public static string ApiKey
        {
            get => Settings.apiKey;
            set => Settings.apiKey = value;
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider()
        {
            LoadSettings();

            var provider = new SettingsProvider("Project/InsightXR", SettingsScope.Project)
            {
                label = "InsightXR",
                guiHandler = (searchContext) =>
                {
                    EditorGUILayout.LabelField("User Configuration", EditorStyles.boldLabel);

                    CustomerID = EditorGUILayout.TextField("Customer ID", CustomerID);
                    UserID = EditorGUILayout.TextField("User ID", UserID);
                    ApiKey = EditorGUILayout.TextField("API Key", ApiKey);

                    EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(CustomerID) || string.IsNullOrEmpty(UserID) || string.IsNullOrEmpty(ApiKey));
                    if (GUILayout.Button("Save"))
                    {
                        SaveSettings();
                    }
                    EditorGUI.EndDisabledGroup();
                },

                keywords = new[] { "Customer", "User", "ID", "API", "Key" }
            };

            return provider;
        }

        private static void SaveSettings()
        {
            EditorUtility.SetDirty(Settings);
            AssetDatabase.SaveAssets();
            Debug.Log("Settings Saved");
            Debug.Log("Customer ID: " + CustomerID);
            Debug.Log("User ID: " + UserID);
            Debug.Log("API Key: " + ApiKey);
        }

        private static void LoadSettings()
        {
            settings = AssetDatabase.LoadAssetAtPath<InsightSettingsSO>("Assets/InsightSettings.asset");
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<InsightSettingsSO>();
                AssetDatabase.CreateAsset(settings, "Assets/InsightSettings.asset");
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif