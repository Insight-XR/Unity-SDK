using System;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// A <c>Guid</c> that can be serialized by Unity.
    /// </summary>
    /// <remarks>
    /// The 128-bit <c>Guid</c>
    /// is stored as two 64-bit <c>ulong</c>s. See the creation utility,
    /// <see cref="SerializableGuidUtil"/>, for additional information.
    /// </remarks>
    [Serializable]
    public struct SerializableGuid : IEquatable<SerializableGuid>
    {
        static readonly SerializableGuid k_Empty = new SerializableGuid(0, 0);

        [SerializeField]
        [HideInInspector]
        ulong m_GuidLow;

        [SerializeField]
        [HideInInspector]
        ulong m_GuidHigh;

        /// <summary>
        /// Represents <c>System.Guid.Empty</c>, a GUID whose value is all zeros.
        /// </summary>
        public static SerializableGuid Empty => k_Empty;

        /// <summary>
        /// Reconstructs the <c>Guid</c> from the serialized data.
        /// </summary>
        public Guid Guid => GuidUtil.Compose(m_GuidLow, m_GuidHigh);

        /// <summary>
        /// Constructs a <see cref="SerializableGuid"/> from two 64-bit <c>ulong</c> values.
        /// </summary>
        /// <param name="guidLow">The low 8 bytes of the <c>Guid</c>.</param>
        /// <param name="guidHigh">The high 8 bytes of the <c>Guid</c>.</param>
        public SerializableGuid(ulong guidLow, ulong guidHigh)
        {
            m_GuidLow = guidLow;
            m_GuidHigh = guidHigh;
        }

        /// <summary>
        /// Gets the hash code for this SerializableGuid.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = m_GuidLow.GetHashCode();
                return hash * 486187739 + m_GuidHigh.GetHashCode();
            }
        }

        /// <summary>
        /// Checks if this SerializableGuid is equal to an object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if <paramref name="obj"/> is a SerializableGuid with the same field values.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SerializableGuid serializableGuid))
                return false;

            return Equals(serializableGuid);
        }

        /// <summary>
        /// Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid.ToString()"/>.
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.7.2#System_Guid_ToString">Microsoft's documentation</a>
        /// for more details.
        /// </summary>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        public override string ToString() => Guid.ToString();

        /// <summary>
        /// Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid.ToString(string)"/>.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of the <c>Guid</c>.
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.7.2#System_Guid_ToString_System_String_">Microsoft's documentation</a>
        /// for more details.</param>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        public string ToString(string format) => Guid.ToString(format);

        /// <summary>
        /// Generates a string representation of the <c>Guid</c>. Same as <see cref="Guid.ToString(string, IFormatProvider)"/>.
        /// </summary>
        /// <param name="format">A single format specifier that indicates how to format the value of the <c>Guid</c>.
        /// See <a href="https://docs.microsoft.com/en-us/dotnet/api/system.guid.tostring?view=netframework-4.7.2#System_Guid_ToString_System_String_System_IFormatProvider_">Microsoft's documentation</a>
        /// for more details.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>A string representation of the <c>Guid</c>.</returns>
        public string ToString(string format, IFormatProvider provider) => Guid.ToString(format, provider);

        /// <summary>
        /// Check if this SerializableGuid is equal to another SerializableGuid.
        /// </summary>
        /// <param name="other">The other SerializableGuid</param>
        /// <returns>True if this SerializableGuid has the same field values as the other one.</returns>
        public bool Equals(SerializableGuid other)
        {
            return m_GuidLow == other.m_GuidLow &&
                m_GuidHigh == other.m_GuidHigh;
        }

        /// <summary>
        /// Perform an equality operation on two SerializableGuids.
        /// </summary>
        /// <param name="lhs">The left-hand SerializableGuid.</param>
        /// <param name="rhs">The right-hand SerializableGuid.</param>
        /// <returns>True if the SerializableGuids are equal to each other.</returns>
        public static bool operator ==(SerializableGuid lhs, SerializableGuid rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Perform an inequality operation on two SerializableGuids.
        /// </summary>
        /// <param name="lhs">The left-hand SerializableGuid.</param>
        /// <param name="rhs">The right-hand SerializableGuid.</param>
        /// <returns>True if the SerializableGuids are not equal to each other.</returns>
        public static bool operator !=(SerializableGuid lhs, SerializableGuid rhs) => !lhs.Equals(rhs);
    }
}
