using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.XR.CoreUtils.Capabilities.Editor
{
    /// <summary>
    /// Class that stores the user selected <see cref="CapabilityProfile"/>s.
    /// </summary>
    public static class CapabilityProfileSelection
    {
        const string k_CapabilityProfileSelectionKey = "CapabilityProfileSelection";
        const string k_GuidSeparator = ",";
        const string k_UndoSelectedProfileName = "Select Capability Profile";

        [Serializable]
        class CapabilityProfileStorage : ScriptableObject
        {
            static CapabilityProfileStorage s_Instance;
            internal static CapabilityProfileStorage Instance
            {
                get
                {
                    if (s_Instance != null)
                        return s_Instance;

                    s_Instance = Resources.FindObjectsOfTypeAll<CapabilityProfileStorage>().FirstOrDefault();
                    if (s_Instance != null)
                        return s_Instance;

                    s_Instance = CreateInstance<CapabilityProfileStorage>();
                    s_Instance.LoadFromUserSettings();
                    s_Instance.LoadFromSerializedList();

                    return s_Instance;
                }
            }

            [SerializeField]
            List<CapabilityProfile> m_SerializedProfiles = new List<CapabilityProfile>();

            [NonSerialized]
            internal List<CapabilityProfile> Profiles = new List<CapabilityProfile>();

            void OnEnable()
            {
                hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;

                EditorApplication.quitting += OnEditorApplicationQuitting;
                Undo.undoRedoPerformed += OnUndoRedoPerformed;

                LoadFromSerializedList();
            }

            void OnDisable()
            {
                EditorApplication.quitting -= OnEditorApplicationQuitting;
                Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            }

            void OnEditorApplicationQuitting()
            {
                DestroyImmediate(this);
            }

            void OnUndoRedoPerformed()
            {
                if (Profiles.SequenceEqual(m_SerializedProfiles))
                    return;

                LoadFromSerializedList();
                SaveToUserSettings();
            }

            internal void LoadFromUserSettings()
            {
                var serializedProfileGuids = EditorUserSettings.GetConfigValue(k_CapabilityProfileSelectionKey);
                var profileGuids = serializedProfileGuids == null ? AssetDatabase.FindAssets("t:CapabilityProfile") : serializedProfileGuids.Split(k_GuidSeparator[0]);
                foreach (var profileGuid in profileGuids)
                {
                    var profilePath = AssetDatabase.GUIDToAssetPath(profileGuid);
                    if (string.IsNullOrEmpty(profilePath))
                        continue;

                    var profile = AssetDatabase.LoadAssetAtPath<CapabilityProfile>(profilePath);
                    if (profile == null)
                        continue;

                    m_SerializedProfiles.Add(profile);
                }
            }

            internal void SaveToUserSettings()
            {
                var profileGuids = new List<string>();
                foreach (var profile in Profiles)
                {
                    if (profile == null)
                        continue;

                    var profilePath = AssetDatabase.GetAssetPath(profile);
                    if (string.IsNullOrEmpty(profilePath))
                        continue;

                    var profileGuid = AssetDatabase.AssetPathToGUID(profilePath);
                    if (string.IsNullOrEmpty(profileGuid))
                        continue;

                    profileGuids.Add(profileGuid);
                }

                EditorUserSettings.SetConfigValue(k_CapabilityProfileSelectionKey, string.Join(k_GuidSeparator, profileGuids));

                SelectionSaved?.Invoke();
            }

            void LoadFromSerializedList()
            {
                Profiles.Clear();
                Profiles.AddRange(m_SerializedProfiles);
            }

            internal void SaveToSerializedList()
            {
                Undo.RecordObject(this, k_UndoSelectedProfileName);
                m_SerializedProfiles.Clear();
                m_SerializedProfiles.AddRange(Profiles);
                EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// Contains the currently selected capability profiles.
        /// </summary>
        public static IReadOnlyList<CapabilityProfile> Selected => CapabilityProfileStorage.Instance.Profiles;

        /// <summary>
        /// Action for when the selection is saved.
        /// </summary>
        public static event Action SelectionSaved;

        /// <summary>
        /// Checks if a given profile is currently one of the selected profiles
        /// </summary>
        /// <param name="profile">The profile to check</param>
        /// <returns>True if this profile is in the active selection, false otherwise.</returns>
        public static bool IsSelected(CapabilityProfile profile) => CapabilityProfileStorage.Instance.Profiles.Contains(profile);

        /// <summary>
        /// Adds a capability profile to the selected capability profiles.
        /// </summary>
        /// <param name="profile">The profile to add to the active set.</param>
        public static void Add(CapabilityProfile profile)
        {
            if (CapabilityProfileStorage.Instance.Profiles.Contains(profile))
                return;

            CapabilityProfileStorage.Instance.Profiles.Add(profile);
        }

        /// <summary>
        /// Removes the capability profile from the selected capability profiles.
        /// </summary>
        /// <param name="profile">The profile to attempt to remove from selected profiles.</param>
        /// <returns>Returns true if successful</returns>
        public static bool Remove(CapabilityProfile profile)
        {
            return CapabilityProfileStorage.Instance.Profiles.Remove(profile);
        }

        /// <summary>
        /// Checks if a given capability key is currently available in the current selected profiles 
        /// </summary>
        /// <param name="capabilityKey">The capability key to check</param>
        /// <returns>Returns true if the capability key is available and enabled.</returns>
        public static bool IsCapabilityAvailableInSelectedProfiles(string capabilityKey)
        {
            foreach (var profile in Selected)
            {
                if (profile is ICapabilityModifier modifier &&
                    modifier.TryGetCapabilityValue(capabilityKey, out var capabilityValue) &&
                    capabilityValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears the selected capability profiles
        /// </summary>
        public static void Clear()
        {
            CapabilityProfileStorage.Instance.Profiles.Clear();
        }

        /// <summary>
        /// Saves the selected capability profiles to the user settings.
        /// </summary>
        public static void Save()
        {
            CapabilityProfileStorage.Instance.SaveToSerializedList();
            CapabilityProfileStorage.Instance.SaveToUserSettings();
        }
    }
}
