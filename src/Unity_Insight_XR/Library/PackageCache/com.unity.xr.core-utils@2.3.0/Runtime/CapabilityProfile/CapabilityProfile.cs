using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.Capabilities
{
    /// <summary>
    /// Abstract base class from which all capability profiles derive.
    /// An asset of this type represents any abstraction that define or change capabilities. For example, this asset can
    /// be an abstraction for a platform capability, an OS capability, a device capability or a combination of them.
    /// </summary>
    /// <seealso cref="ICapabilityModifier"/>
    public abstract class CapabilityProfile : ScriptableObject
    {
        /// <summary>
        /// Event that is raised when the capabilities in this profile is changed.
        /// </summary>
        /// <seealso cref="ReportCapabilityChanged"/>
        public static event Action<CapabilityProfile> CapabilityChanged;

        /// <summary>
        /// This should be invoked from the editor (including custom editors) and runtime whenever a system changes
        /// the capabilities of this profile. This works as a dirty flag to keep all systems updated.
        /// </summary>
        public void ReportCapabilityChanged()
        {
            CapabilityChanged?.Invoke(this);
        }
    }
}
