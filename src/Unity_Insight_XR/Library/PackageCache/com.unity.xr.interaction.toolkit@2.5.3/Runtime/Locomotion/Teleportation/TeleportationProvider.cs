using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// The <see cref="TeleportationProvider"/> is responsible for moving the XR Origin
    /// to the desired location on the user's request.
    /// </summary>
    [AddComponentMenu("XR/Locomotion/Teleportation Provider", 11)]
    [HelpURL(XRHelpURLConstants.k_TeleportationProvider)]
    public class TeleportationProvider : LocomotionProvider
    {
        /// <summary>
        /// The current teleportation request.
        /// </summary>
        protected TeleportRequest currentRequest { get; set; }

        /// <summary>
        /// Whether the current teleportation request is valid.
        /// </summary>
        protected bool validRequest { get; set; }

        [SerializeField]
        [Tooltip("The time (in seconds) to delay the teleportation once it is activated.")]
        float m_DelayTime;

        /// <summary>
        /// The time (in seconds) to delay the teleportation once it is activated.
        /// This delay can be used, for example, as time to set a tunneling vignette effect as a VR comfort option.
        /// </summary>
        public float delayTime
        {
            get => m_DelayTime;
            set => m_DelayTime = value;
        }

        /// <summary>
        /// This function will queue a teleportation request within the provider.
        /// </summary>
        /// <param name="teleportRequest">The teleportation request to queue.</param>
        /// <returns>Returns <see langword="true"/> if successfully queued. Otherwise, returns <see langword="false"/>.</returns>
        public virtual bool QueueTeleportRequest(TeleportRequest teleportRequest)
        {
            currentRequest = teleportRequest;
            validRequest = true;
            return true;
        }

        bool m_HasExclusiveLocomotion;
        float m_TimeStarted = -1f;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            if (system != null && m_DelayTime > 0f && m_DelayTime > system.timeout)
                Debug.LogWarning($"Delay Time ({m_DelayTime}) is longer than the Locomotion System's Timeout ({system.timeout}).", this);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Update()
        {
            if (!validRequest)
            {
                locomotionPhase = LocomotionPhase.Idle;
                return;
            }

            if (!m_HasExclusiveLocomotion)
            {
                if (!BeginLocomotion())
                    return;

                m_HasExclusiveLocomotion = true;
                locomotionPhase = LocomotionPhase.Started;
                m_TimeStarted = Time.time;
            }

            // Wait for configured Delay Time
            if (m_DelayTime > 0f && Time.time - m_TimeStarted < m_DelayTime)
                return;

            locomotionPhase = LocomotionPhase.Moving;

            var xrOrigin = system.xrOrigin;
            if (xrOrigin != null)
            {
                switch (currentRequest.matchOrientation)
                {
                    case MatchOrientation.WorldSpaceUp:
                        xrOrigin.MatchOriginUp(Vector3.up);
                        break;
                    case MatchOrientation.TargetUp:
                        xrOrigin.MatchOriginUp(currentRequest.destinationRotation * Vector3.up);
                        break;
                    case MatchOrientation.TargetUpAndForward:
                        xrOrigin.MatchOriginUpCameraForward(currentRequest.destinationRotation * Vector3.up, currentRequest.destinationRotation * Vector3.forward);
                        break;
                    case MatchOrientation.None:
                        // Change nothing. Maintain current origin rotation.
                        break;
                    default:
                        Assert.IsTrue(false, $"Unhandled {nameof(MatchOrientation)}={currentRequest.matchOrientation}.");
                        break;
                }

                var heightAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;

                var cameraDestination = currentRequest.destinationPosition + heightAdjustment;

                xrOrigin.MoveCameraToWorldLocation(cameraDestination);
            }

            EndLocomotion();
            m_HasExclusiveLocomotion = false;
            validRequest = false;
            locomotionPhase = LocomotionPhase.Done;
        }
    }
}
