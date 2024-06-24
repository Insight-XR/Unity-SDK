using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    /// <summary>
    /// Makes the GameObject this component is attached to follow a target with a delay and some other layout options.
    /// </summary>
    [AddComponentMenu("XR/Lazy Follow", 22)]
    [HelpURL(XRHelpURLConstants.k_LazyFollow)]
    public class LazyFollow : MonoBehaviour
    {
        /// <summary>
        /// Defines the possible position follow modes for the lazy follow object.
        /// </summary>
        /// <seealso cref="positionFollowMode"/>
        public enum PositionFollowMode
        {
            /// <summary>
            /// The lazy follow object will not follow any position.
            /// </summary>
            None,

            /// <summary>
            /// The object will smoothly maintain the same position as the target.
            /// </summary>
            Follow,
        }

        /// <summary>
        /// Defines the possible rotation follow modes for the lazy follow object.
        /// </summary>
        /// <seealso cref="rotationFollowMode"/>
        public enum RotationFollowMode
        {
            /// <summary>
            /// The lazy follow object will not follow any rotation.
            /// </summary>
            None,

            /// <summary>
            /// The lazy follow object will rotate to face the target (designed for use with main camera as the target), maintaining its orientation relative to the target.
            /// </summary>
            LookAt,

            /// <summary>
            /// The lazy follow object will rotate to face the target (designed for use with main camera as the target), maintaining its orientation relative to the target.
            /// The up direction will be locked to the world up.
            /// </summary>
            LookAtWithWorldUp,

            /// <summary>
            /// The object will smoothly maintain the same rotation as the target.
            /// </summary>
            Follow,
        }

        const float k_LowerSpeedVariance = 0f;
        const float k_UpperSpeedVariance = 0.999f;
        
        [Header("Target Config")]
        [SerializeField, Tooltip("(Optional) The object being followed. If not set, this will default to the main camera when this component is enabled.")]
        Transform m_Target;

        /// <summary>
        /// The object being followed. If not set, this will default to the main camera when this component is enabled.
        /// </summary>
        public Transform target
        {
            get => m_Target;
            set => m_Target = value;
        }

        [SerializeField, Tooltip("The amount to offset the target's position when following. This position is relative/local to the target object.")]
        Vector3 m_TargetOffset = new Vector3(0f, 0f, 0.5f);

        /// <summary>
        /// The amount to offset the target's position when following. This position is relative/local to the target object.
        /// </summary>
        public Vector3 targetOffset
        {
            get => m_TargetOffset;
            set => m_TargetOffset = value;
        }
        
        [Space]
        [SerializeField]
        [Tooltip("If true, read the local transform of the target to lazy follow, otherwise read the world transform. If using look at rotation follow modes, only world-space follow is supported.")]
        bool m_FollowInLocalSpace;
        
        /// <summary>
        /// If true, read the local transform of the target to lazy follow, otherwise read the world transform.
        /// If using look at rotation follow modes, only world-space follow is supported.
        /// </summary>
        public bool followInLocalSpace
        {
            get => m_FollowInLocalSpace;
            set
            {
                m_FollowInLocalSpace = value;
                ValidateFollowMode();
            }
        }

        [SerializeField]
        [Tooltip("If true, apply the target offset in local space. If false, apply the target offset in world space.")]
        bool m_ApplyTargetInLocalSpace;
        
        /// <summary>
        /// If true, apply the target offset in local space. If false, apply the target offset in world space.
        /// </summary>
        public bool applyTargetInLocalSpace
        {
            get => m_ApplyTargetInLocalSpace;
            set => m_ApplyTargetInLocalSpace = value;
        }

        [Header("General Follow Params")]

        [SerializeField, Tooltip("Movement speed used when smoothing to new target. Lower values mean the lazy follow lags further behind the target.")]
        float m_MovementSpeed = 6f;

        /// <summary>
        /// Movement speed used when smoothing to new target. Lower values mean the lazy follow lags further behind the target.
        /// </summary>
        public float movementSpeed
        {
            get => m_MovementSpeed;
            set
            {
                m_MovementSpeed = value;
                UpdateUpperAndLowerSpeedBounds();
            }
        }

        [SerializeField]
        [Range(k_LowerSpeedVariance, k_UpperSpeedVariance)]
        [Tooltip("Adjust movement speed based on distance from the target using a tolerance percentage. 0% for constant speed.")]
            
        float m_MovementSpeedVariancePercentage = 0.25f;

        /// <summary>
        /// Adjust movement speed based on distance from the target using a tolerance percentage. 0% for constant speed.
        /// For example, with a variance of 25% (0.25), and a speed of 6, the upper bound is 7.5, which is reached as the target is approached.
        /// If the target is far from the object, the speed will trend toward the lower bound, which would be 4.5 in this case.
        /// </summary>
        public float movementSpeedVariancePercentage
        {
            get => m_MovementSpeedVariancePercentage;
            set
            {
                m_MovementSpeedVariancePercentage = Mathf.Clamp(value, k_LowerSpeedVariance, k_UpperSpeedVariance);   
                UpdateUpperAndLowerSpeedBounds();
            }
        }

        [SerializeField, Tooltip("Snap to target position when this component is enabled.")]
        bool m_SnapOnEnable = true;

        /// <summary>
        /// Snap to target position when this component is enabled.
        /// </summary>
        public bool snapOnEnable
        {
            get => m_SnapOnEnable;
            set => m_SnapOnEnable = value;
        }

        [Header("Position Follow Params")]

        [SerializeField, Tooltip("Determines the follow mode used to determine a new rotation. Look At is best used with the target being the main camera.")]
        PositionFollowMode m_PositionFollowMode = PositionFollowMode.Follow;

        /// <summary>
        /// Determines the follow mode used to determine a new rotation.
        /// </summary>
        public PositionFollowMode positionFollowMode
        {
            get => m_PositionFollowMode;
            set => m_PositionFollowMode = value;
        }

        [SerializeField, Tooltip("Minimum distance from target before which a follow lazy follow starts.")]
        float m_MinDistanceAllowed = 0.01f;

        /// <summary>
        /// Minimum distance from target before which a follow lazy follow starts.
        /// </summary>
        public float minDistanceAllowed
        {
            get => m_MinDistanceAllowed;
            set
            {
                m_MinDistanceAllowed = value;
                if (m_Vector3TweenableVariable != null)
                    m_Vector3TweenableVariable.minDistanceAllowed = value;
            }
        }

        [SerializeField, Tooltip("Maximum distance from target before lazy follow targets, when time threshold is reached.")]
        float m_MaxDistanceAllowed = 0.3f;

        /// <summary>
        /// Maximum distance from target before lazy follow targets, when time threshold is reached.
        /// </summary>
        public float maxDistanceAllowed
        {
            get => m_MaxDistanceAllowed;
            set
            {
                m_MaxDistanceAllowed = value;
                if (m_Vector3TweenableVariable != null)
                    m_Vector3TweenableVariable.maxDistanceAllowed = value;
            }
        }

        [SerializeField, Tooltip("Time required to elapse (in seconds) before the max distance allowed goes from the min distance to the max.")]
        float m_TimeUntilThresholdReachesMaxDistance = 3f;

        /// <summary>
        /// The time threshold (in seconds) where if max distance is reached the lazy follow capability will not be turned off.
        /// </summary>
        public float timeUntilThresholdReachesMaxDistance
        {
            get => m_TimeUntilThresholdReachesMaxDistance;
            set
            {
                m_TimeUntilThresholdReachesMaxDistance = value;
                if (m_Vector3TweenableVariable != null)
                    m_Vector3TweenableVariable.minToMaxDelaySeconds = value;
            }
        }

        [Header("Rotation Follow Params")]

        [SerializeField, Tooltip("Determines the follow mode used to determine a new rotation. Look At is best used with the target being the main camera.")]
        RotationFollowMode m_RotationFollowMode = RotationFollowMode.LookAt;

        /// <summary>
        /// Determines the follow mode used to determine a new rotation.
        /// </summary>
        public RotationFollowMode rotationFollowMode
        {
            get => m_RotationFollowMode;
            set
            {
                m_RotationFollowMode = value;
                ValidateFollowMode();
            }
        }

        [SerializeField, Tooltip("Minimum angle offset (in degrees) from target before which lazy follow starts.")]
        float m_MinAngleAllowed = 0.1f;

        /// <summary>
        /// Minimum angle offset (in degrees) from target before which lazy follow starts.
        /// </summary>
        public float minAngleAllowed
        {
            get => m_MinAngleAllowed;
            set
            {
                m_MinAngleAllowed = value;
                if (m_QuaternionTweenableVariable != null)
                    m_QuaternionTweenableVariable.minAngleAllowed = value;
            }
        }

        [SerializeField, Tooltip("Maximum angle offset (in degrees) from target before lazy follow targets, when time threshold is reached.")]
        float m_MaxAngleAllowed = 5f;

        /// <summary>
        /// Maximum angle offset (in degrees) from target before lazy follow targets, when time threshold is reached
        /// </summary>
        public float maxAngleAllowed
        {
            get => m_MaxAngleAllowed;
            set
            {
                m_MaxAngleAllowed = value;
                if (m_QuaternionTweenableVariable != null)
                    m_QuaternionTweenableVariable.maxAngleAllowed = value;
            }
        }


        [SerializeField, Tooltip("Time required to elapse (in seconds) before the max angle offset allowed goes from the min angle offset to the max.")]
        float m_TimeUntilThresholdReachesMaxAngle = 3f;

        /// <summary>
        /// Time required to elapse (in seconds) before the max angle offset allowed goes from the min angle offset to the max.
        /// </summary>
        public float timeUntilThresholdReachesMaxAngle
        {
            get => m_TimeUntilThresholdReachesMaxAngle;
            set
            {
                m_TimeUntilThresholdReachesMaxAngle = value;
                if (m_QuaternionTweenableVariable != null)
                    m_QuaternionTweenableVariable.minToMaxDelaySeconds = value;
            }
        }

        float m_LowerMovementSpeed;
        float m_UpperMovementSpeed;

        readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

        SmartFollowVector3TweenableVariable m_Vector3TweenableVariable;
        SmartFollowQuaternionTweenableVariable m_QuaternionTweenableVariable;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            UpdateUpperAndLowerSpeedBounds();
            ValidateFollowMode();

            if (m_Vector3TweenableVariable != null)
            {
                m_Vector3TweenableVariable.minDistanceAllowed = m_MinDistanceAllowed;
                m_Vector3TweenableVariable.maxDistanceAllowed = m_MaxDistanceAllowed;
                m_Vector3TweenableVariable.minToMaxDelaySeconds = m_TimeUntilThresholdReachesMaxDistance;
            }

            if (m_QuaternionTweenableVariable != null)
            {
                m_QuaternionTweenableVariable.minAngleAllowed = m_MinAngleAllowed;
                m_QuaternionTweenableVariable.maxAngleAllowed = m_MaxAngleAllowed;
                m_QuaternionTweenableVariable.minToMaxDelaySeconds = m_TimeUntilThresholdReachesMaxAngle;
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_Vector3TweenableVariable = new SmartFollowVector3TweenableVariable(m_MinDistanceAllowed, m_MaxDistanceAllowed, m_TimeUntilThresholdReachesMaxDistance);
            m_QuaternionTweenableVariable = new SmartFollowQuaternionTweenableVariable(m_MinAngleAllowed, m_MaxAngleAllowed, m_TimeUntilThresholdReachesMaxAngle);
            UpdateUpperAndLowerSpeedBounds();
            ValidateFollowMode();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            // Default to main camera
            if (m_Target == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    m_Target = mainCamera.transform;
            }

            var thisTransform = transform;
            var currentPosition = followInLocalSpace ? thisTransform.localPosition : thisTransform.position;
            var currentRotation = followInLocalSpace ? thisTransform.localRotation : thisTransform.rotation;

            m_Vector3TweenableVariable.target = currentPosition;
            m_QuaternionTweenableVariable.target = currentRotation;

            m_BindingsGroup.AddBinding(m_Vector3TweenableVariable.SubscribeAndUpdate(UpdatePosition));
            m_BindingsGroup.AddBinding(m_QuaternionTweenableVariable.SubscribeAndUpdate(UpdateRotation));

            if (m_SnapOnEnable)
            {
                if (m_PositionFollowMode != PositionFollowMode.None)
                {
                    if (TryGetThresholdTargetPosition(out var newPositionTarget))
                        m_Vector3TweenableVariable.target = newPositionTarget;
                }

                if (m_RotationFollowMode != RotationFollowMode.None)
                {
                    if (TryGetThresholdTargetRotation(out var newRotationTarget))
                        m_QuaternionTweenableVariable.target = newRotationTarget;
                }

                m_Vector3TweenableVariable.HandleTween(1f);
                m_QuaternionTweenableVariable.HandleTween(1f);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            m_BindingsGroup.Clear();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            m_Vector3TweenableVariable?.Dispose();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void LateUpdate()
        {
            if (m_Target == null)
                return;

            var deltaTime = Time.unscaledDeltaTime;

            if (m_PositionFollowMode != PositionFollowMode.None)
            {
                if (TryGetThresholdTargetPosition(out var newPositionTarget))
                    m_Vector3TweenableVariable.target = newPositionTarget;

                if (m_MovementSpeedVariancePercentage > 0f)
                    m_Vector3TweenableVariable.HandleSmartTween(deltaTime, m_LowerMovementSpeed, m_UpperMovementSpeed);
                else
                    m_Vector3TweenableVariable.HandleTween(deltaTime * movementSpeed);
            }

            if (m_RotationFollowMode != RotationFollowMode.None)
            {
                if (TryGetThresholdTargetRotation(out var newTargetRotation))
                    m_QuaternionTweenableVariable.target = newTargetRotation;

                if (m_MovementSpeedVariancePercentage > 0f)
                    m_QuaternionTweenableVariable.HandleSmartTween(deltaTime, m_LowerMovementSpeed, m_UpperMovementSpeed);
                else
                    m_QuaternionTweenableVariable.HandleTween(deltaTime * movementSpeed);
            }
        }

        void UpdatePosition(float3 position)
        {
            if(applyTargetInLocalSpace)
                transform.localPosition = position;
            else
                transform.position = position;
        }

        void UpdateRotation(Quaternion rotation)
        {
            if(applyTargetInLocalSpace)
                transform.localRotation = rotation;
            else
                transform.rotation = rotation;
        }

        /// <summary>
        /// Determines if the new target position is within a dynamically determined threshold based on the time since the last update,
        /// and outputs the new target position if it meets the threshold.
        /// </summary>
        /// <param name="newTarget">The output new target position as a <see cref="Vector3"/>, if within the allowed threshold.</param>
        /// <returns>Returns <see langword="true"/> if the squared distance between the current and new target positions is within the allowed threshold, <see langword="false"/> otherwise.</returns>
        protected virtual bool TryGetThresholdTargetPosition(out Vector3 newTarget)
        {
            switch (m_PositionFollowMode)
            {
                case PositionFollowMode.None:
                    newTarget = followInLocalSpace ? transform.localPosition : transform.position;
                    return false;

                case PositionFollowMode.Follow:
                {
                    if (followInLocalSpace)
                        newTarget = m_Target.localPosition + m_TargetOffset;
                    else
                        newTarget = m_Target.position + m_Target.TransformVector(m_TargetOffset);
                    
                    return m_Vector3TweenableVariable.IsNewTargetWithinThreshold(newTarget);
                }
                default:
                    Debug.LogError($"Unhandled {nameof(PositionFollowMode)}={m_PositionFollowMode}", this);
                    goto case PositionFollowMode.None;
            }
        }

        /// <summary>
        /// Determines if the new target rotation is within a dynamically determined threshold based on the time since the last update,
        /// and outputs the new target rotation if it meets the threshold.
        /// </summary>
        /// <param name="newTarget">The output new target rotation as a <see cref="Quaternion"/>, if within the allowed threshold.</param>
        /// <returns>Returns <see langword="true"/> if the angle difference between the current and new target rotations is within the allowed threshold, <see langword="false"/> otherwise.</returns>
        protected virtual bool TryGetThresholdTargetRotation(out Quaternion newTarget)
        {
            switch (m_RotationFollowMode)
            {
                case RotationFollowMode.None:
                    newTarget = followInLocalSpace ? transform.localRotation : transform.rotation;
                    return false;

                case RotationFollowMode.LookAt:
                {
                    var forward = (transform.position - m_Target.position).normalized;
                    BurstMathUtility.OrthogonalLookRotation(forward, Vector3.up, out newTarget);
                    break;
                }

                case RotationFollowMode.LookAtWithWorldUp:
                {
                    var forward = (transform.position - m_Target.position).normalized;
                    BurstMathUtility.LookRotationWithForwardProjectedOnPlane(forward, Vector3.up, out newTarget);
                    break;
                }

                case RotationFollowMode.Follow:
                    newTarget = followInLocalSpace ? m_Target.localRotation : m_Target.rotation;
                    break;

                default:
                    Debug.LogError($"Unhandled {nameof(RotationFollowMode)}={m_RotationFollowMode}", this);
                    goto case RotationFollowMode.None;
            }

            return m_QuaternionTweenableVariable.IsNewTargetWithinThreshold(newTarget);
        }

        void ValidateFollowMode()
        {
            if (!m_FollowInLocalSpace)
                return;
            
            // We cannot follow in local space if we are looking at the target.
            if (m_RotationFollowMode == RotationFollowMode.LookAt || m_RotationFollowMode == RotationFollowMode.LookAtWithWorldUp)
            {
                if (Application.isPlaying)
                {
                    m_FollowInLocalSpace = false;
                    XRLoggingUtils.LogWarning("Cannot follow in local space if Rotation Follow Mode set to look at the target. Turning off Follow In Local Space.", this);
                }
                else
                {
                    XRLoggingUtils.LogWarning("Cannot follow in local space if Rotation Follow Mode set to look at the target.", this);
                }
            }
        }

        void UpdateUpperAndLowerSpeedBounds()
        {
            if (m_MovementSpeedVariancePercentage > 0f)
            {
                m_LowerMovementSpeed = m_MovementSpeed - m_MovementSpeedVariancePercentage * m_MovementSpeed;
                m_UpperMovementSpeed = m_MovementSpeed * (1f + m_MovementSpeedVariancePercentage);
            }
            else
            {
                m_LowerMovementSpeed = m_MovementSpeed;
                m_UpperMovementSpeed = m_MovementSpeed;
            }
        }
    }
}