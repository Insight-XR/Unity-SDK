using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils.Capabilities;
using UnityEngine;
using Unity.XR.CoreUtils.Capabilities.Editor;
using UnityEditor;

namespace Unity.XR.CoreUtils.Editor
{
    /// <summary>
    /// Class containing utility methods for the scene validation UI.
    /// </summary>
    public static class SceneValidationUIUtils
    {
        const string k_TurnOffSelectionName = "Turn Off";
        const string k_EverythingSelectionName = "Everything";
        const string k_NoCapabilityProfilesInProjectName = "No capability profiles in project";

        static readonly GUIContent s_CapabilitiesSelectionButtonContent =
            new GUIContent("", "Select the capability profiles to validate your project or scene objects. Some packages will have capabilities that can influence the validation.");

        // For local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static List<CapabilityProfile> s_CapabilityProfiles = new List<CapabilityProfile>();

        static SceneValidationUIUtils()
        {
            CapabilityProfileSelection.SelectionSaved += UpdateCapabilitiesSelectionButtonText;
        }

        /// <summary>
        /// Draws a capabilities dropdown that allows the user to select which capability profiles to validate against.
        /// </summary>
        /// <param name="askIfUserWantsToTurnOffValidation">When set to true a window will pop up in the editor asking if the user really wants to turn off scene validation</param>
        public static void DrawCapabilitiesSelectionDropdown(bool askIfUserWantsToTurnOffValidation = true)
        {
            if (string.IsNullOrEmpty(s_CapabilitiesSelectionButtonContent.text))
                UpdateCapabilitiesSelectionButtonText();

            var dropdownRect = GUILayoutUtility.GetRect(s_CapabilitiesSelectionButtonContent, UnityEngine.GUI.skin.button, GUILayout.MinWidth(180));
            if (!EditorGUI.DropdownButton(dropdownRect, s_CapabilitiesSelectionButtonContent, FocusType.Passive))
                return;

            s_CapabilityProfiles.Clear();
            foreach (var assetGuid in AssetDatabase.FindAssets("t:CapabilityProfile"))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                var capabilityProfile = AssetDatabase.LoadAssetAtPath<CapabilityProfile>(assetPath);
                if (capabilityProfile != null)
                    s_CapabilityProfiles.Add(capabilityProfile);
            }

            s_CapabilityProfiles.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            var menu = new GenericMenu();

            if (s_CapabilityProfiles.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent(k_NoCapabilityProfilesInProjectName));
            }
            else
            {
                menu.AddItem(new GUIContent(k_TurnOffSelectionName), CapabilityProfileSelection.Selected.Count == 0,
                    () =>
                    {
                        if (askIfUserWantsToTurnOffValidation &&
                            !EditorUtility.DisplayDialog("Turn Off Scene Validation",
                                "Do you want to turn off scene validation?\n" +
                                "You can always turn scene validation on/off in Edit > Project Settings > Project Validation",
                                "OK", "Cancel"))
                        {
                            return;
                        }

                        CapabilityProfileSelection.Clear();
                        CapabilityProfileSelection.Save();
                    });
            }

            if (s_CapabilityProfiles.Count > 0)
            {
                var isSelectingEverything = CapabilityProfileSelection.Selected.Count >= s_CapabilityProfiles.Count;
                menu.AddItem(new GUIContent(k_EverythingSelectionName), isSelectingEverything, () =>
                {
                    foreach (var capabilityProfile in s_CapabilityProfiles)
                        CapabilityProfileSelection.Add(capabilityProfile);

                    CapabilityProfileSelection.Save();
                });

                menu.AddSeparator("");
            }

            foreach (var capabilityProfile in s_CapabilityProfiles)
            {
                menu.AddItem(new GUIContent(capabilityProfile.name), CapabilityProfileSelection.IsSelected(capabilityProfile), () =>
                {
                    if (CapabilityProfileSelection.IsSelected(capabilityProfile))
                        CapabilityProfileSelection.Remove(capabilityProfile);
                    else
                        CapabilityProfileSelection.Add(capabilityProfile);

                    CapabilityProfileSelection.Save();
                });
            }

            menu.DropDown(dropdownRect);
        }

        static void UpdateCapabilitiesSelectionButtonText()
        {
            if (CapabilityProfileSelection.Selected.Count == 0)
            {
                s_CapabilitiesSelectionButtonContent.text = k_TurnOffSelectionName;
                return;
            }
            if (CapabilityProfileSelection.Selected.Count >= AssetDatabase.FindAssets("t:CapabilityProfile").Length)
            {
                s_CapabilitiesSelectionButtonContent.text = k_EverythingSelectionName;
                return;
            }

            s_CapabilityProfiles.Clear();
            s_CapabilityProfiles.AddRange(CapabilityProfileSelection.Selected);
            s_CapabilityProfiles.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            s_CapabilitiesSelectionButtonContent.text = string.Join(", ", s_CapabilityProfiles.Select(c => c.name));
        }
    }
}
