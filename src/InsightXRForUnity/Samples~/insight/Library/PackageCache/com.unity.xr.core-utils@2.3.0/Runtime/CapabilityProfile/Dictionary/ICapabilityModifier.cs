namespace Unity.XR.CoreUtils.Capabilities
{
    /// <summary>
    /// Implement this interface in derived classes from <see cref="CapabilityProfile"/> to modify a capability value.
    /// </summary>
    /// <seealso cref="CapabilityDictionary"/>
    public interface ICapabilityModifier
    {
        /// <summary>
        /// Gets the capability value associated with the given key.
        /// </summary>
        /// <param name="capabilityKey">The capability key to get the value.</param>
        /// <param name="capabilityValue">Returns the capability value if the given key is found.</param>
        /// <returns>Returns <see langword="true"/> when the given capability key is found. Otherwise, returns <see langword="false"/>.</returns>
        bool TryGetCapabilityValue(string capabilityKey, out bool capabilityValue);
    }
}
