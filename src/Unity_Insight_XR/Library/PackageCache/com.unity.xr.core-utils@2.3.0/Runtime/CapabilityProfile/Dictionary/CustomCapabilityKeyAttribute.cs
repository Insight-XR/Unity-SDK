using System;

namespace Unity.XR.CoreUtils.Capabilities
{
    /// <summary>
    /// Use this attribute to define a custom capability key. You can tag the constant string fields definition of your custom capabilities with this attribute
    /// to allow them to be shown in the <see cref="CapabilityDictionary"/> Inspectors.
    /// </summary>
    /// <example>
    /// Below is an example of a custom device capability definition:
    /// <code>
    /// [CustomCapabilityKey(200)]
    /// public const string CustomFeatureCustomCapability = "Custom Feature/Custom Capability";
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CustomCapabilityKeyAttribute : Attribute
    {
        /// <summary>
        /// The order to show the custom capability.
        /// </summary>
        public readonly int Order;

        /// <summary>
        /// Constructor for attribute.
        /// </summary>
        /// <param name="order">The order to show the custom capability.</param>
        public CustomCapabilityKeyAttribute(int order = 1000)
        {
            Order = order;
        }
    }
}
