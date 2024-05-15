#region custom_capability_sample
using Unity.XR.CoreUtils.Capabilities;
using UnityEngine;

[CreateAssetMenu]
class MySystemCapability : CapabilityProfile, ICapabilityModifier
{
    [SerializeField]
    CapabilityDictionary m_CapabilityDictionary;

    public bool TryGetCapabilityValue(string capabilityKey, out bool capabilityValue)
    {
        return m_CapabilityDictionary.TryGetValue(capabilityKey, out capabilityValue);
    }
}

#endregion // custom_capability_sample

class MyCapabilityKeyClass
{
    #region custom_capability_key_sample
    [CustomCapabilityKey(100)]
    public const string MyCapabilityKey = "MyFeature/MyCapability";
    #endregion
}

#region custom_capability_type_sample
interface IMaxScreenSize
{
    Vector2Int MaxScreenSize { get; }
}

[CreateAssetMenu]
class MySystemCapabilityWithScreenSize : CapabilityProfile, IMaxScreenSize
{
    [SerializeField]
    Vector2Int m_MaxScreenSize;

    public Vector2Int MaxScreenSize => m_MaxScreenSize;
}

static class CapabilityProfileExtension
{
    // This method is not required, it's just to illustrate how to get a capability by interface
    public static bool TryGetMaxScreenSizeCapability(this CapabilityProfile profile, out Vector2Int maxScreenSize)
    {
        if (profile is IMaxScreenSize maxScreenSizeProfile)
        {
            maxScreenSize = maxScreenSizeProfile.MaxScreenSize;
            return true;
        }

        maxScreenSize = default;
        return false;
    }
}
#endregion // custom_capability_type_sample
