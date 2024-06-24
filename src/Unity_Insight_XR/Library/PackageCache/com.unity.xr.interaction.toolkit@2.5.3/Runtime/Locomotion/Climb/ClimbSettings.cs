using System;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Settings for climb locomotion. These settings can be used globally as part of the <see cref="ClimbProvider"/>
    /// or as overrides per-instance of <see cref="ClimbInteractable"/>.
    /// </summary>
    [Serializable]
    public class ClimbSettings
    {
        [SerializeField]
        [Tooltip("Controls whether to allow unconstrained movement along the climb interactable's x-axis.")]
        bool m_AllowFreeXMovement = true;

        /// <summary>
        /// Controls whether to allow unconstrained movement along the <see cref="ClimbInteractable"/>'s x-axis.
        /// </summary>
        public bool allowFreeXMovement
        {
            get => m_AllowFreeXMovement;
            set => m_AllowFreeXMovement = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to allow unconstrained movement along the climb interactable's y-axis.")]
        bool m_AllowFreeYMovement = true;

        /// <summary>
        /// Controls whether to allow unconstrained movement along the <see cref="ClimbInteractable"/>'s y-axis.
        /// </summary>
        public bool allowFreeYMovement
        {
            get => m_AllowFreeYMovement;
            set => m_AllowFreeYMovement = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to allow unconstrained movement along the climb interactable's z-axis.")]
        bool m_AllowFreeZMovement = true;

        /// <summary>
        /// Controls whether to allow unconstrained movement along the <see cref="ClimbInteractable"/>'s z-axis.
        /// </summary>
        public bool allowFreeZMovement
        {
            get => m_AllowFreeZMovement;
            set => m_AllowFreeZMovement = value;
        }
    }
}