using System;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Add this attribute to an XR Interaction component to control whether to allow or disallow multiple focus mode.
    /// </summary>
    /// <seealso cref="InteractableFocusMode.Multiple"/>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CanFocusMultipleAttribute : Attribute
    {
        /// <summary>
        /// Whether to allow multiple focus mode. The default value is <see langword="true"/> to allow.
        /// </summary>
        public bool allowMultiple { get; }

        /// <summary>
        /// Initializes the attribute specifying whether to allow or disallow multiple focus mode.
        /// </summary>
        /// <param name="allowMultiple"></param>
        public CanFocusMultipleAttribute(bool allowMultiple = true)
        {
            this.allowMultiple = allowMultiple;
        }
    }
}