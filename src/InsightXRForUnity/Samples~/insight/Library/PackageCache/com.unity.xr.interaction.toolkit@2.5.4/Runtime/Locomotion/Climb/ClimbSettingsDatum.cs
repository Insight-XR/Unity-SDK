using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a <see cref="ClimbSettings"/> value.
    /// </summary>
    [CreateAssetMenu(fileName = "ClimbSettings", menuName = "XR/Locomotion/Climb Settings")]
    [HelpURL(XRHelpURLConstants.k_ClimbSettingsDatum)]
    public class ClimbSettingsDatum : Datum<ClimbSettings>
    {
        
    }
}