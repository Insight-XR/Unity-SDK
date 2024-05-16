using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Internal
{
    /// <summary>
    /// Add this attribute to a serialized Unity Object field (or its subclasses, including MonoBehaviours and ScriptableObjects)
    /// to allow the Inspector to validate if the referenced object implements the given interface.
    /// </summary>
    /// <remarks>
    /// This attribute does not support multiple interfaces nor generic interfaces.
    /// The interface implementation check is ignored for references dragged into the foldout array (or list) in the
    /// Inspector.
    /// </remarks>
    class RequireInterfaceAttribute : PropertyAttribute
    {
        /// <summary>
        /// The interface type that the referenced object should implement.
        /// </summary>
        public Type interfaceType { get; }

        /// <summary>
        /// Initializes the attribute specifying the interface that the reference Unity Object should implement.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        public RequireInterfaceAttribute(Type interfaceType)
        {
            this.interfaceType = interfaceType;
        }
    }
}
