﻿using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Locomotion provider that allows the user to smoothly rotate their rig continuously over time.
    /// </summary>
    /// <seealso cref="LocomotionProvider"/>
    /// <seealso cref="SnapTurnProviderBase"/>
    public abstract class ContinuousTurnProviderBase : LocomotionProvider
    {
        [SerializeField]
        [Tooltip("The number of degrees/second clockwise to rotate when turning clockwise.")]
        float m_TurnSpeed = 60f;
        /// <summary>
        /// The number of degrees/second clockwise to rotate when turning clockwise.
        /// </summary>
        public float turnSpeed
        {
            get => m_TurnSpeed;
            set => m_TurnSpeed = value;
        }

        bool m_IsTurningXROrigin;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            m_IsTurningXROrigin = false;

            // Use the input amount to scale the turn speed.
            var input = ReadInput();
            var turnAmount = GetTurnAmount(input);

            TurnRig(turnAmount);

            switch (locomotionPhase)
            {
                case LocomotionPhase.Idle:
                case LocomotionPhase.Started:
                    if (m_IsTurningXROrigin)
                        locomotionPhase = LocomotionPhase.Moving;
                    break;
                case LocomotionPhase.Moving:
                    if (!m_IsTurningXROrigin)
                        locomotionPhase = LocomotionPhase.Done;
                    break;
                case LocomotionPhase.Done:
                    locomotionPhase = m_IsTurningXROrigin ? LocomotionPhase.Moving : LocomotionPhase.Idle;
                    break;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(LocomotionPhase)}={locomotionPhase}");
                    break;
            }
        }

        /// <summary>
        /// Reads the current value of the turn input.
        /// </summary>
        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
        protected abstract Vector2 ReadInput();

        /// <summary>
        /// Determines the turn amount in degrees for the given <paramref name="input"/> vector.
        /// </summary>
        /// <param name="input">Input vector, such as from a thumbstick.</param>
        /// <returns>Returns the turn amount in degrees for the given <paramref name="input"/> vector.</returns>
        protected virtual float GetTurnAmount(Vector2 input)
        {
            if (input == Vector2.zero)
                return 0f;

            var cardinal = CardinalUtility.GetNearestCardinal(input);
            switch (cardinal)
            {
                case Cardinal.North:
                case Cardinal.South:
                    break;
                case Cardinal.East:
                case Cardinal.West:
                    return input.magnitude * (Mathf.Sign(input.x) * m_TurnSpeed * Time.deltaTime);
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(Cardinal)}={cardinal}");
                    break;
            }

            return 0f;
        }

        /// <summary>
        /// Rotates the rig by <paramref name="turnAmount"/> degrees.
        /// </summary>
        /// <param name="turnAmount">The amount of rotation in degrees.</param>
        protected void TurnRig(float turnAmount)
        {
            if (Mathf.Approximately(turnAmount, 0f))
                return;

            if (CanBeginLocomotion() && BeginLocomotion())
            {
                var xrOrigin = system.xrOrigin;
                if (xrOrigin != null)
                {
                    m_IsTurningXROrigin = true;
                    xrOrigin.RotateAroundCameraUsingOriginUp(turnAmount);
                }

                EndLocomotion();
            }
        }
    }
}
