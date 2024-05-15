using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Base class for a locomotion provider that allows for constrained movement with a <see cref="CharacterController"/>.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    public abstract class ConstrainedMoveProvider : LocomotionProvider
    {
        /// <summary>
        /// Defines when gravity begins to take effect.
        /// </summary>
        /// <seealso cref="gravityMode"/>
        public enum GravityApplicationMode
        {
            /// <summary>
            /// Only begin to apply gravity and apply locomotion when a move input occurs.
            /// When using gravity, continues applying each frame, even if input is stopped, until touching ground.
            /// </summary>
            /// <remarks>
            /// Use this style when you don't want gravity to apply when the player physically walks away and off a ground surface.
            /// Gravity will only begin to move the player back down to the ground when they try to use input to move.
            /// </remarks>
            AttemptingMove,

            /// <summary>
            /// Apply gravity and apply locomotion every frame, even without move input.
            /// </summary>
            /// <remarks>
            /// Use this style when you want gravity to apply when the player physically walks away and off a ground surface,
            /// even when there is no input to move.
            /// </remarks>
            Immediately,
        }

        [SerializeField]
        [Tooltip("Controls whether to enable unconstrained movement along the x-axis.")]
        bool m_EnableFreeXMovement = true;
        /// <summary>
        /// Controls whether to enable unconstrained movement along the x-axis.
        /// </summary>
        public bool enableFreeXMovement
        {
            get => m_EnableFreeXMovement;
            set => m_EnableFreeXMovement = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable unconstrained movement along the y-axis.")]
        bool m_EnableFreeYMovement;
        /// <summary>
        /// Controls whether to enable unconstrained movement along the y-axis.
        /// </summary>
        public bool enableFreeYMovement
        {
            get => m_EnableFreeYMovement;
            set => m_EnableFreeYMovement = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable unconstrained movement along the z-axis.")]
        bool m_EnableFreeZMovement = true;
        /// <summary>
        /// Controls whether to enable unconstrained movement along the z-axis.
        /// </summary>
        public bool enableFreeZMovement
        {
            get => m_EnableFreeZMovement;
            set => m_EnableFreeZMovement = value;
        }

        [SerializeField]
        [Tooltip("Controls whether gravity applies to constrained axes when a Character Controller is used.")]
        bool m_UseGravity = true;
        /// <summary>
        /// Controls whether gravity applies to constrained axes when a <see cref="CharacterController"/> is used.
        /// </summary>
        public bool useGravity
        {
            get => m_UseGravity;
            set => m_UseGravity = value;
        }

        [SerializeField]
        [Tooltip("Controls when gravity begins to take effect.")]
        GravityApplicationMode m_GravityApplicationMode;
        /// <summary>
        /// Controls when gravity begins to take effect.
        /// </summary>
        /// <seealso cref="GravityApplicationMode"/>
        public GravityApplicationMode gravityMode
        {
            get => m_GravityApplicationMode;
            set => m_GravityApplicationMode = value;
        }

        CharacterController m_CharacterController;
        bool m_AttemptedGetCharacterController;
        bool m_IsMovingXROrigin;
        Vector3 m_GravityDrivenVelocity;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            m_IsMovingXROrigin = false;

            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            var translationInWorldSpace = ComputeDesiredMove(out var attemptingMove);

            switch (m_GravityApplicationMode)
            {
                case GravityApplicationMode.Immediately:
                    MoveRig(translationInWorldSpace);
                    break;
                case GravityApplicationMode.AttemptingMove:
                    if (attemptingMove || m_GravityDrivenVelocity != Vector3.zero)
                        MoveRig(translationInWorldSpace);
                    break;
                default:
                    Assert.IsTrue(false, $"{nameof(m_GravityApplicationMode)}={m_GravityApplicationMode} outside expected range.");
                    break;
            }

            switch (locomotionPhase)
            {
                case LocomotionPhase.Idle:
                case LocomotionPhase.Started:
                    if (m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Moving;
                    break;
                case LocomotionPhase.Moving:
                    if (!m_IsMovingXROrigin)
                        locomotionPhase = LocomotionPhase.Done;
                    break;
                case LocomotionPhase.Done:
                    locomotionPhase = m_IsMovingXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={locomotionPhase}");
                    break;
            }
        }

        /// <summary>
        /// Determines how much to move the rig.
        /// </summary>
        /// <param name="attemptingMove">Whether the provider is attempting to move.</param>
        /// <returns>Returns the translation amount in world space to move the rig.</returns>
        protected abstract Vector3 ComputeDesiredMove(out bool attemptingMove);

        /// <summary>
        /// Creates a locomotion event to move the rig by <paramref name="translationInWorldSpace"/>,
        /// and optionally restricts movement along each axis and applies gravity.
        /// </summary>
        /// <param name="translationInWorldSpace">The translation amount in world space to move the rig
        /// (before restricting movement along each axis and applying gravity).</param>
        protected virtual void MoveRig(Vector3 translationInWorldSpace)
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            FindCharacterController();

            var motion = translationInWorldSpace;
            if (!m_EnableFreeXMovement)
                motion.x = 0f;
            if (!m_EnableFreeYMovement)
                motion.y = 0f;
            if (!m_EnableFreeZMovement)
                motion.z = 0f;

            if (m_CharacterController != null && m_CharacterController.enabled)
            {
                // Step vertical velocity from gravity
                if (m_CharacterController.isGrounded || !m_UseGravity)
                {
                    m_GravityDrivenVelocity = Vector3.zero;
                }
                else
                {
                    m_GravityDrivenVelocity += Physics.gravity * Time.deltaTime;
                    if (m_EnableFreeXMovement)
                        m_GravityDrivenVelocity.x = 0f;
                    if (m_EnableFreeYMovement)
                        m_GravityDrivenVelocity.y = 0f;
                    if (m_EnableFreeZMovement)
                        m_GravityDrivenVelocity.z = 0f;
                }

                motion += m_GravityDrivenVelocity * Time.deltaTime;

                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Note that calling Move even with Vector3.zero will have an effect by causing isGrounded to update
                    m_IsMovingXROrigin = true;
                    m_CharacterController.Move(motion);
                    EndLocomotion();
                }
            }
            else
            {
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    m_IsMovingXROrigin = true;
                    xrOrigin.transform.position += motion;
                    EndLocomotion();
                }
            }
        }

        void FindCharacterController()
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            // Save a reference to the optional CharacterController on the rig GameObject
            // that will be used to move instead of modifying the Transform directly.
            if (m_CharacterController == null && !m_AttemptedGetCharacterController)
            {
                // Try on the Origin GameObject first, and then fallback to the XR Origin GameObject (if different)
                if (!xrOrigin.TryGetComponent(out m_CharacterController) && xrOrigin != system.xrOrigin.gameObject)
                    system.xrOrigin.TryGetComponent(out m_CharacterController);

                m_AttemptedGetCharacterController = true;
            }
        }
    }
}