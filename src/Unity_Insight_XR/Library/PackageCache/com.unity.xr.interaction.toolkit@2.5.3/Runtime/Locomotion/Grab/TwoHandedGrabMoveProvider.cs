namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Allows the user to combine two <see cref="GrabMoveProvider"/> instances for locomotion. This allows the user to
    /// translate, scale, and rotate themselves counter to transformations of the line segment between both hands.
    /// </summary>
    /// <seealso cref="GrabMoveProvider"/>
    /// <seealso cref="LocomotionProvider"/>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_TwoHandedGrabMoveProviders)]
    [AddComponentMenu("XR/Locomotion/Two-Handed Grab Move Provider", 11)]
    [HelpURL(XRHelpURLConstants.k_TwoHandedGrabMoveProvider)]
    public class TwoHandedGrabMoveProvider : ConstrainedMoveProvider
    {
        [SerializeField]
        [Tooltip("The left hand grab move instance which will be used as one half of two-handed locomotion.")]
        GrabMoveProvider m_LeftGrabMoveProvider;
        /// <summary>
        /// The left hand grab move instance which will be used as one half of two-handed locomotion.
        /// </summary>
        public GrabMoveProvider leftGrabMoveProvider
        {
            get => m_LeftGrabMoveProvider;
            set => m_LeftGrabMoveProvider = value;
        }

        [SerializeField]
        [Tooltip("The right hand grab move instance which will be used as one half of two-handed locomotion.")]
        GrabMoveProvider m_RightGrabMoveProvider;
        /// <summary>
        /// The right hand grab move instance which will be used as one half of two-handed locomotion.
        /// </summary>
        public GrabMoveProvider rightGrabMoveProvider
        {
            get => m_RightGrabMoveProvider;
            set => m_RightGrabMoveProvider = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to override the settings for individual handed providers with this provider's settings on initialization.")]
        bool m_OverrideSharedSettingsOnInit = true;
        /// <summary>
        /// Controls whether to override the settings for individual handed providers with this provider's settings on initialization.
        /// </summary>
        public bool overrideSharedSettingsOnInit
        {
            get => m_OverrideSharedSettingsOnInit;
            set => m_OverrideSharedSettingsOnInit = value;
        }

        [SerializeField]
        [Tooltip("The ratio of actual movement distance to controller movement distance.")]
        float m_MoveFactor = 1f;
        /// <summary>
        /// The ratio of actual movement distance to controller movement distance.
        /// </summary>
        public float moveFactor
        {
            get => m_MoveFactor;
            set => m_MoveFactor = value;
        }

        [SerializeField]
        [Tooltip("Controls whether translation requires both grab move inputs to be active.")]
        bool m_RequireTwoHandsForTranslation;
        /// <summary>
        /// Controls whether translation requires both grab move inputs to be active.
        /// </summary>
        public bool requireTwoHandsForTranslation
        {
            get => m_RequireTwoHandsForTranslation;
            set => m_RequireTwoHandsForTranslation = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable yaw rotation of the user.")]
        bool m_EnableRotation = true;
        /// <summary>
        /// Controls whether to enable yaw rotation of the user.
        /// </summary>
        public bool enableRotation
        {
            get => m_EnableRotation;
            set => m_EnableRotation = value;
        }

        [SerializeField]
        [Tooltip("Controls whether to enable uniform scaling of the user.")]
        bool m_EnableScaling;
        /// <summary>
        /// Controls whether to enable uniform scaling of the user.
        /// </summary>
        public bool enableScaling
        {
            get => m_EnableScaling;
            set => m_EnableScaling = value;
        }

        [SerializeField]
        [Tooltip("The minimum user scale allowed.")]
        float m_MinimumScale = 0.2f;
        /// <summary>
        /// The minimum user scale allowed.
        /// </summary>
        public float minimumScale
        {
            get => m_MinimumScale;
            set => m_MinimumScale = value;
        }

        [SerializeField]
        [Tooltip("The maximum user scale allowed.")]
        float m_MaximumScale = 100f;
        /// <summary>
        /// The maximum user scale allowed.
        /// </summary>
        public float maximumScale
        {
            get => m_MaximumScale;
            set => m_MaximumScale = value;
        }

        bool m_IsMoving;

        Vector3 m_PreviousMidpointBetweenControllers;

        float m_InitialOriginYaw;
        Vector3 m_InitialLeftToRightDirection;
        Vector3 m_InitialLeftToRightOrthogonal;

        float m_InitialOriginScale;
        float m_InitialDistanceBetweenHands;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (m_LeftGrabMoveProvider == null || m_RightGrabMoveProvider == null)
            {
                Debug.LogError("Left or Right Grab Move Provider is not set or has been destroyed.", this);
                enabled = false;
                return;
            }

            if (m_RequireTwoHandsForTranslation)
            {
                m_LeftGrabMoveProvider.canMove = false;
                m_RightGrabMoveProvider.canMove = false;
            }

            if (m_OverrideSharedSettingsOnInit)
            {
                m_LeftGrabMoveProvider.system = system;
                m_LeftGrabMoveProvider.enableFreeXMovement = enableFreeXMovement;
                m_LeftGrabMoveProvider.enableFreeYMovement = enableFreeYMovement;
                m_LeftGrabMoveProvider.enableFreeZMovement = enableFreeZMovement;
                m_LeftGrabMoveProvider.useGravity = useGravity;
                m_LeftGrabMoveProvider.gravityMode = gravityMode;
                m_LeftGrabMoveProvider.moveFactor = m_MoveFactor;
                m_RightGrabMoveProvider.system = system;
                m_RightGrabMoveProvider.enableFreeXMovement = enableFreeXMovement;
                m_RightGrabMoveProvider.enableFreeYMovement = enableFreeYMovement;
                m_RightGrabMoveProvider.enableFreeZMovement = enableFreeZMovement;
                m_RightGrabMoveProvider.useGravity = useGravity;
                m_RightGrabMoveProvider.gravityMode = gravityMode;
                m_RightGrabMoveProvider.moveFactor = m_MoveFactor;
            }

            beginLocomotion += OnBeginLocomotion;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            if (m_LeftGrabMoveProvider != null)
                m_LeftGrabMoveProvider.canMove = true;
            if (m_RightGrabMoveProvider != null)
                m_RightGrabMoveProvider.canMove = true;

            beginLocomotion -= OnBeginLocomotion;
        }

        /// <inheritdoc/>
        protected override Vector3 ComputeDesiredMove(out bool attemptingMove)
        {
            attemptingMove = false;
            var wasMoving = m_IsMoving;
            var xrOrigin = system.xrOrigin?.Origin;
            m_IsMoving = m_LeftGrabMoveProvider.IsGrabbing() && m_RightGrabMoveProvider.IsGrabbing() && xrOrigin != null;
            if (!m_IsMoving)
            {
                // Enable one-handed movement
                if (!m_RequireTwoHandsForTranslation)
                {
                    m_LeftGrabMoveProvider.canMove = true;
                    m_RightGrabMoveProvider.canMove = true;
                }

                return Vector3.zero;
            }

            // Prevent individual grab locomotion since we perform our own translation
            m_LeftGrabMoveProvider.canMove = false;
            m_RightGrabMoveProvider.canMove = false;

            var originTransform = xrOrigin.transform;
            var leftHandLocalPosition = m_LeftGrabMoveProvider.controllerTransform.localPosition;
            var rightHandLocalPosition = m_RightGrabMoveProvider.controllerTransform.localPosition;
            var midpointLocalPosition = (leftHandLocalPosition + rightHandLocalPosition) * 0.5f;
            if (!wasMoving && m_IsMoving) // Cannot simply check locomotionPhase because it might always be in moving state, due to gravity application mode
            {
                m_InitialOriginYaw = originTransform.eulerAngles.y;
                m_InitialLeftToRightDirection = rightHandLocalPosition - leftHandLocalPosition;
                m_InitialLeftToRightDirection.y = 0f; // Only use yaw rotation
                m_InitialLeftToRightOrthogonal = Quaternion.AngleAxis(90f, Vector3.down) * m_InitialLeftToRightDirection;

                m_InitialOriginScale = originTransform.localScale.x;
                m_InitialDistanceBetweenHands = Vector3.Distance(leftHandLocalPosition, rightHandLocalPosition);

                // Do not move the first frame of grab
                m_PreviousMidpointBetweenControllers = midpointLocalPosition;
                return Vector3.zero;
            }

            attemptingMove = true;
            var move = originTransform.TransformVector(m_PreviousMidpointBetweenControllers - midpointLocalPosition) * m_MoveFactor;
            m_PreviousMidpointBetweenControllers = midpointLocalPosition;
            return move;
        }

        void OnBeginLocomotion(LocomotionSystem otherSystem)
        {
            var xrOrigin = system.xrOrigin?.Origin;
            if (xrOrigin == null)
                return;

            var originTransform = xrOrigin.transform;
            var leftHandLocalPosition = m_LeftGrabMoveProvider.controllerTransform.localPosition;
            var rightHandLocalPosition = m_RightGrabMoveProvider.controllerTransform.localPosition;

            if (m_EnableRotation)
            {
                var leftToRightDirection = rightHandLocalPosition - leftHandLocalPosition;
                leftToRightDirection.y = 0f; // Only use yaw rotation
                var yawSign = Mathf.Sign(Vector3.Dot(m_InitialLeftToRightOrthogonal, leftToRightDirection));
                var targetYaw = m_InitialOriginYaw + Vector3.Angle(m_InitialLeftToRightDirection, leftToRightDirection) * yawSign;
                originTransform.rotation = Quaternion.AngleAxis(targetYaw, Vector3.up);
            }

            if (m_EnableScaling)
            {
                var distanceBetweenHands = Vector3.Distance(leftHandLocalPosition, rightHandLocalPosition);
                var targetScale = distanceBetweenHands != 0f
                    ? m_InitialOriginScale * (m_InitialDistanceBetweenHands / distanceBetweenHands)
                    : originTransform.localScale.x;

                targetScale = Mathf.Clamp(targetScale, m_MinimumScale, m_MaximumScale);
                originTransform.localScale = Vector3.one * targetScale;
            }
        }
    }
}