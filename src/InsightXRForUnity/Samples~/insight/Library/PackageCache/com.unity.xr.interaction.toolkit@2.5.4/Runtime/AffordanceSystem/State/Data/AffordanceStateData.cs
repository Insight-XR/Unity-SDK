using System;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State
{
    /// <summary>
    /// Struct to store affordance state data used to compute affordance transition tweens.
    /// </summary>
    public readonly struct AffordanceStateData : IEquatable<AffordanceStateData>
    {
        /// <summary>
        /// Total number of supported increments for the affordance state transition amount float conversion.
        /// </summary>
        public const byte totalStateTransitionIncrements = 255;

        /// <summary>
        /// Affordance state index.
        /// </summary>
        public byte stateIndex { get; }

        /// <summary>
        /// State transition amount represented as a byte. Converted to float by dividing over <see cref="totalStateTransitionIncrements"/>.
        /// </summary>
        public byte stateTransitionIncrement { get; }

        /// <summary>
        /// 0-1 Float representation of <see cref="stateTransitionIncrement"/>.
        /// </summary>
        public float stateTransitionAmountFloat => (float)stateTransitionIncrement / totalStateTransitionIncrements;

        /// <summary>
        /// Constructor for affordance state data.
        /// </summary>
        /// <param name="stateIndex">State index reference.</param>
        /// <param name="transitionAmount">Float representation of transition amount.</param>
        public AffordanceStateData(byte stateIndex, float transitionAmount)
            : this(stateIndex, (byte)(Mathf.Clamp01(transitionAmount) * totalStateTransitionIncrements))
        {
        }

        /// <summary>
        /// Constructor for affordance state data.
        /// </summary>
        /// <param name="stateIndex">State index reference.</param>
        /// <param name="transitionIncrement">Byte increment amount used for computing the float transition amount representation.</param>
        public AffordanceStateData(byte stateIndex, byte transitionIncrement)
        {
            this.stateIndex = stateIndex;
            stateTransitionIncrement = transitionIncrement;
        }

        // IEquatable API
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>Returns <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AffordanceStateData other)
        {
            return stateIndex == other.stateIndex && stateTransitionIncrement == other.stateTransitionIncrement;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>Returns <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is AffordanceStateData other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>Returns a 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            // NOTE HashCode.Combine was used before, but it is not available in older versions of dotNet
            int hash = 17;
            hash = hash * 31 + stateIndex.GetHashCode();
            hash = hash * 31 + stateTransitionIncrement.GetHashCode();
            return hash;
        }
        // End IEquatable API
    }
}