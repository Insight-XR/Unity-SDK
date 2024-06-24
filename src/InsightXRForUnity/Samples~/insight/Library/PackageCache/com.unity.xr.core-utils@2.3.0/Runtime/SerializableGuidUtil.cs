using System;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Utility for creating a <see cref="SerializableGuid"/>.
    /// Unity can serialize a <c>SerializableGuid</c>, but not a <c>System.Guid</c>.
    /// </summary>
    public static class SerializableGuidUtil
    {
        /// <summary>
        /// Creates a <c>SerializableGuid</c> from a <c>System.Guid</c>.
        /// </summary>
        /// <param name="guid">The <c>Guid</c> to represent as a <c>SerializableGuid</c>.</param>
        /// <returns>A serializable version of <paramref name="guid"/>.</returns>
        public static SerializableGuid Create(Guid guid)
        {
            guid.Decompose(out var low, out var high);
            return new SerializableGuid(low, high);
        }
    }
}
