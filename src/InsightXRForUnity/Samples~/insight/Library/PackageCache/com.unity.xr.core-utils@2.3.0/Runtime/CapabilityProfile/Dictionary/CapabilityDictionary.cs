using System;
using Unity.XR.CoreUtils.Collections;

namespace Unity.XR.CoreUtils.Capabilities
{
    /// <summary>
    /// Class used to store profile capabilities.
    /// </summary>
    /// <remarks>
    /// This class can be used in an <see cref="CapabilityProfile"/> that implements the interface
    /// <see cref="ICapabilityModifier"/> to define the profile capabilities.
    /// </remarks>
    [Serializable]
    public sealed class CapabilityDictionary : SerializableDictionary<string, bool>
    {
        /// <summary>
        /// Force save the dictionary entries into the <see cref="SerializableDictionary{T,T}.SerializedItems"/> list.
        /// </summary>
        public void ForceSerialize()
        {
            base.OnBeforeSerialize();
        }

        /// <inheritdoc />
        public override void OnBeforeSerialize()
        {
            // This method is intentionally left blank, this prevents this dictionary to serialize its entries back to the
            // SerializedItems list and allows this list to be resized and have items with duplicated keys in the Inspector view.
        }
    }
}
