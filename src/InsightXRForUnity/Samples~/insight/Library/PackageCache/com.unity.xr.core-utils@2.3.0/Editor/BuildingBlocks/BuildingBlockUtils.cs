using Unity.XR.CoreUtils.Capabilities;
using Unity.XR.CoreUtils.Capabilities.Editor;

namespace Unity.XR.CoreUtils.Editor.BuildingBlocks
{
    /// <summary>
    /// This class contains a set of utility methods when working with Building Blocks.
    /// </summary>
    public static class BuildingBlockUtils
    {
        /// <summary>
        /// Generates a string message with a set of capability keys that are required for a building block to be enabled.
        /// </summary>
        /// <param name="capabilityKeys">The capability keys which should be included in the message.</param>
        /// <returns>The formatted string.</returns>
        public static string GenerateMissingCapabilitiesRequiredTooltip(params string[] capabilityKeys)
        {
            var tooltip = "The following capabilities are required for this building block to be enabled:\n";
            foreach (var key in capabilityKeys)
            {
                tooltip += $"- {key}\n";
            }

            tooltip += "\nPlease select a capability profile that supports these capabilities in the Project Validation window under Project Settings > Project Validation.";

            return tooltip;
        }

        /// <summary>
        /// Checks if any capability profiles are currently selected.
        /// </summary>
        /// <returns>returns true when there are capability profiles selected in the project validation window under Project settings.
        /// Returns false if there are no capability assets in the project or no capability profile is/are selected</returns>
        public static bool AnyCapabilityProfileSelected()
        {
            return CapabilityProfileSelection.Selected.Count > 0;
        }
    }
}
