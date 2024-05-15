using System;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands
{
    /// <summary>
    /// A name for a hand expression in the XR Device Simulator.
    /// This struct wraps the <see cref="InternedString"/> struct from the Input System to allow for strings to be compared by reference
    /// for better performance.
    /// When compared, the strings are case-insensitive and culture-insensitive.
    /// When converting back to a string, the original casing will be preserved.
    /// </summary>
    readonly struct HandExpressionName : IEquatable<HandExpressionName>
    {
        /// <summary>
        /// The name for a default hand name, represents a natural resting hand shape.
        /// </summary>
        public static readonly HandExpressionName Default = new HandExpressionName("Default");
        
        readonly InternedString m_InternedString;
        
        /// <summary>
        /// Constructs a new name from a string value to be used with the XR Device Simulator.
        /// This allows for strings to be compared by reference and will only allocate memory if the string is not already interned.
        /// </summary>
        /// <param name="value">The string value for the name.</param>
        public HandExpressionName(string value)
        {
            m_InternedString = new InternedString(value);
        }

        /// <summary>
        /// Compares the name to another object. The strings used when creating the names are compared case-insensitive and culture-insensitive.
        /// If the other object is not another <see cref="HandExpressionName"/>, this will always return false.
        /// Otherwise this will compare the interned strings for equality, avoiding a costly string comparison.
        /// </summary>
        /// <param name="obj">The other object to compare with.</param>
        /// <returns>True if the other object is a name with the same string value ignoring case and culture. Otherwise false.</returns>
        public override bool Equals(object obj) 
        {
            if (obj is HandExpressionName name)
                return Equals(name);

            return false;
        }

        /// <summary>
        /// Compares the name to another name. The strings used when creating the names are compared case-insensitive and culture-insensitive.
        /// This compares the interned strings for equality, avoiding a costly string comparison.
        /// </summary>
        /// <param name="other">The other name to compare with.</param>
        /// <returns>True if the other name has the same string value, ignoring case and culture. Otherwise false.</returns>
        public bool Equals(HandExpressionName other)
        {
            return m_InternedString.Equals(other.m_InternedString);
        }

        /// <summary>
        /// Converts the name to a string. This will preserve the original casing of the string.
        /// </summary>
        /// <returns>The original string used to create the name.</returns>
        public override string ToString()
        {
            return m_InternedString.ToString();
        }
        
        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return m_InternedString.GetHashCode();
        }
        
        /// <summary>
        /// Compares two names for equality using the == operator.
        /// </summary>
        /// <param name="lhs">The left-hand side of the == operator.</param>
        /// <param name="rhs">The right-hand side of the == operator.</param>
        /// <returns>True if the other name has the same string value, ignoring case and culture. Otherwise false.</returns>
        public static bool operator ==(HandExpressionName lhs, HandExpressionName rhs)
        {
            return lhs.m_InternedString == rhs.m_InternedString;
        }

        /// <summary>
        /// Compares two names for inequality using the != operator.
        /// </summary>
        /// <param name="lhs">The left-hand side of the != operator.</param>
        /// <param name="rhs">The right-hand side of the != operator.</param>
        /// <returns>True if the other name has a different string value, ignoring case and culture. Otherwise false.</returns>
        public static bool operator !=(HandExpressionName lhs, HandExpressionName rhs)
        {
            return lhs.m_InternedString != rhs.m_InternedString;
        }

        /// <summary>
        /// Implicitly converts the name to a string. This will preserve the original casing of the string.
        /// </summary>
        /// <param name="value">The name that contains the string value.</param>
        /// <returns>The original string used to create the name.</returns>
        public static implicit operator string(HandExpressionName value)
        {
            return value.m_InternedString.ToString();
        }

        /// <summary>
        /// Implicitly converts a string to a name.
        /// </summary>
        /// <param name="value">The string value for the name.</param>
        /// <returns>The name object that contains the string that can be compared while avoiding costly string comparison.</returns>
        public static implicit operator HandExpressionName(string value)
        {
            return new HandExpressionName(value);
        }
    }
}
