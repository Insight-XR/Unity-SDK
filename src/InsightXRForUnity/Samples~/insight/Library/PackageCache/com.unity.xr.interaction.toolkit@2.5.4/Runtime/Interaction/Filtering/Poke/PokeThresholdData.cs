using System;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering
{
    /// <summary>
    /// Represents the poke evaluation axis used.
    /// </summary>
    /// <seealso cref="PokeThresholdData.pokeDirection"/>
    public enum PokeAxis
    {
        /// <summary>
        /// No axis is allowed for poking.
        /// </summary>
        None,

        /// <summary>
        /// Allow poking in the positive X-axis direction.
        /// </summary>
        X,

        /// <summary>
        /// Allow poking in the positive Y-axis direction.
        /// </summary>
        Y,

        /// <summary>
        /// Allow poking in the positive Z-axis direction.
        /// </summary>
        Z,

        /// <summary>
        /// Allow poking in the negative X-axis direction.
        /// </summary>
        NegativeX,

        /// <summary>
        /// Allow poking in the negative Y-axis direction.
        /// </summary>
        NegativeY,

        /// <summary>
        /// Allow poking in the negative Z-axis direction.
        /// </summary>
        NegativeZ,
    }

    /// <summary>
    /// The settings used to fine tune the vector and offsets which dictate how the poke interaction will be evaluated.
    /// </summary>
    [Serializable]
    public class PokeThresholdData
    {
        [SerializeField]
        [Tooltip("The axis along which the poke interaction will be constrained.")]
        PokeAxis m_PokeDirection = PokeAxis.Z;

        /// <summary>
        /// The axis along which the poke interaction will be constrained.
        /// </summary>
        /// <seealso cref="PokeAxis"/>
        public PokeAxis pokeDirection
        {
            get => m_PokeDirection;
            set => m_PokeDirection = value;
        }

        [SerializeField]
        [Tooltip("Distance along the poke interactable interaction axis that allows for a poke to be triggered sooner/with less precision.")]
        float m_InteractionDepthOffset;

        /// <summary>
        /// Distance along the poke interactable interaction axis that allows for a poke to be triggered sooner/with less precision.
        /// </summary>
        public float interactionDepthOffset
        {
            get => m_InteractionDepthOffset;
            set => m_InteractionDepthOffset = value;
        }

        [SerializeField]
        [Tooltip("When enabled, the filter will check that a poke action is started and moves within the poke angle threshold along the poke direction axis.")]
        bool m_EnablePokeAngleThreshold = true;

        /// <summary>
        /// When enabled, the filter will check that a poke action is started and moves within the poke angle threshold along the poke direction axis.
        /// </summary>
        /// <seealso cref="pokeAngleThreshold"/>
        public bool enablePokeAngleThreshold
        {
            get => m_EnablePokeAngleThreshold;
            set => m_EnablePokeAngleThreshold = value;
        }

        [SerializeField]
        [Tooltip("The maximum allowed angle (in degrees) from the poke direction axis that will trigger a select interaction.")]
        [Range(0f, 89.9f)]
        float m_PokeAngleThreshold = 45f;

        /// <summary>
        /// The maximum allowed angle (in degrees) from the poke direction axis that will trigger a select interaction.
        /// Only used when <see cref="enablePokeAngleThreshold"/> is enabled.
        /// </summary>
        /// <remarks>The angle must be greater than or equal to 0 degrees and less than 90 degrees.</remarks>
        /// <seealso cref="enablePokeAngleThreshold"/>
        public float pokeAngleThreshold
        {
            get => m_PokeAngleThreshold;
            set => m_PokeAngleThreshold = value;
        }

        /// <summary>
        /// This returns the dot-product threshold value based on the <see cref="pokeAngleThreshold"/> configured.
        /// Only used when <see cref="enablePokeAngleThreshold"/> is enabled.
        /// </summary>
        /// <returns>A float value representing a dot-product threshold value.</returns>
        /// <seealso cref="pokeAngleThreshold"/>
        public float GetSelectEntranceVectorDotThreshold()
        {
            return Mathf.Cos(Mathf.Deg2Rad * m_PokeAngleThreshold);
        }
    }
}