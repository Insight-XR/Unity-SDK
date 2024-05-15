using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
#if BURST_PRESENT
using Unity.Burst;
#endif

namespace UnityEngine.XR.Interaction.Toolkit.Inputs
{
    /// <summary>
    /// Provides low-latency stabilization for XR pose inputs, especially useful on rays.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    [AddComponentMenu("XR/XR Transform Stabilizer", 11)]
    [HelpURL(XRHelpURLConstants.k_XRTransformStabilizer)]
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_TransformStabilizer)]
    public class XRTransformStabilizer : MonoBehaviour
    {
        const float k_90FPS = 1f / 90f;

        [SerializeField]
        [Tooltip("The Transform component whose position and rotation will be matched and stabilized.")]
        Transform m_Target;

        [SerializeField]
        [RequireInterface(typeof(IXRRayProvider))]
        [Tooltip("Optional - When provided a ray, the stabilizer will calculate the rotation that keeps a ray's endpoint stable.")]
        Object m_AimTargetObject;

        /// <summary>
        /// The <see cref="Transform"/> component whose position and rotation will be matched and stabilized.
        /// </summary>
        public Transform targetTransform
        {
            get => m_Target;
            set => m_Target = value;
        }

        /// <summary>
        /// When provided a ray, the stabilizer will calculate the rotation that keeps a ray's endpoint stable. 
        /// When stabilizing rotation, it uses whatever value is most optimal - either the last rotation (minimizing rotation), 
        /// or the rotation that keeps the endpoint in place.
        /// </summary>
        public IXRRayProvider aimTarget
        {
            get => m_AimTarget;
            set
            {
                m_AimTarget = value;
                m_AimTargetObject = value as Object;
            }
        }

        IXRRayProvider m_AimTarget;

        [SerializeField]
        [Tooltip("If enabled, will read the target and apply stabilization in local space. Otherwise, in world space.")]
        bool m_UseLocalSpace;

        /// <summary>
        /// If enabled, will read the target and apply stabilization in local space. Otherwise, in world space.
        /// </summary>
        public bool useLocalSpace
        {
            get => m_UseLocalSpace;
            set => m_UseLocalSpace = value;
        }

        [Header("Stabilization Parameters")]
        [SerializeField]
        [Tooltip("Maximum distance (in degrees) that stabilization will be applied.")]
        float m_AngleStabilization = 20f;

        /// <summary>
        /// Maximum distance (in degrees) that stabilization will be applied.
        /// </summary>
        public float angleStabilization
        {
            get => m_AngleStabilization;
            set => m_AngleStabilization = value;
        }

        [SerializeField]
        [Tooltip("Maximum distance (in meters) that stabilization will be applied.")]
        float m_PositionStabilization = 0.25f;

        /// <summary>
        /// Maximum distance (in meters) that stabilization will be applied.
        /// </summary>
        public float positionStabilization
        {
            get => m_PositionStabilization;
            set => m_PositionStabilization = value;
        }

        Transform m_ThisTransform;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_ThisTransform = transform;
            if (m_AimTarget == null)
                m_AimTarget = m_AimTargetObject as IXRRayProvider;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            if (m_AimTarget == null)
                m_AimTarget = m_AimTargetObject as IXRRayProvider;

            if (m_UseLocalSpace)
            {
                m_ThisTransform.localPosition = m_Target.localPosition;
                m_ThisTransform.localRotation = m_Target.localRotation;
            }
            else
            {
                m_ThisTransform.SetPositionAndRotation(m_Target.position, m_Target.rotation);
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            var currentPosition = m_ThisTransform.position;
            var currentRotation = m_ThisTransform.rotation;
            var targetPosition = m_Target.position;
            var targetRotation = m_Target.rotation;

            // Processing in local space means we want to scale the position stabilization to keep it normalized
            var localScale = m_UseLocalSpace ? m_ThisTransform.lossyScale.x : 1f;
            localScale = Mathf.Abs(localScale) < 0.01f ? 0.01f : localScale;
            var invScale = 1f / localScale;

            if (m_AimTarget == null)
            {
                StabilizeTransform(currentPosition, currentRotation, targetPosition, targetRotation, Time.deltaTime, m_PositionStabilization * localScale, m_AngleStabilization,
                out var resultPosition, out var resultRotation);
                m_ThisTransform.SetPositionAndRotation(resultPosition, resultRotation);
            }
            else
            {
                // Calculate the stabilized position
                StabilizePosition(currentPosition, targetPosition, Time.deltaTime, m_PositionStabilization * localScale, out var resultPosition);

                // Use that to come up with the rotation that would put the endpoint of the ray at it's last position
                // Stabilize rotation to whatever value is closer - keeping the endpoint stable or the ray itself stable
                CalculateRotationParams(currentPosition, resultPosition, m_ThisTransform.forward, m_ThisTransform.up, m_AimTarget.rayEndPoint, invScale, m_AngleStabilization, 
                                        out var antiRotation, out var scaleFactor, out var targetAngleScale);

                StabilizeOptimalRotation(currentRotation, targetRotation, antiRotation, Time.deltaTime, m_AngleStabilization, targetAngleScale, scaleFactor, out var resultRotation);
                m_ThisTransform.SetPositionAndRotation(resultPosition, resultRotation);
            }
        }
        
#if BURST_PRESENT
        [BurstCompile]
#endif
        static void StabilizeTransform(in float3 startPos, in quaternion startRot, in float3 targetPos, in quaternion targetRot, float deltaTime, float positionStabilization, float angleStabilization, out float3 resultPos, out quaternion resultRot)
        {
            // Calculate the stabilized position
            var positionOffset = targetPos - startPos;
            var positionDistance = math.length(positionOffset);
            var positionLerp = CalculateStabilizedLerp(positionDistance / positionStabilization, deltaTime);

            // Calculate the stabilized rotation
            BurstMathUtility.Angle(targetRot, startRot, out var rotationOffset);
            var rotationLerp = CalculateStabilizedLerp(rotationOffset / angleStabilization, deltaTime);

            resultPos = math.lerp(startPos, targetPos, positionLerp);
            resultRot = math.slerp(startRot, targetRot, rotationLerp);
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void StabilizePosition(in float3 startPos,in float3 targetPos, float deltaTime, float positionStabilization, out float3 resultPos)
        {
            // Calculate the stabilized position
            var positionOffset = targetPos - startPos;
            var positionDistance = math.length(positionOffset);
            var positionLerp = CalculateStabilizedLerp(positionDistance / positionStabilization, deltaTime);
            
            resultPos = math.lerp(startPos, targetPos, positionLerp);
        }

#if BURST_PRESENT
        [BurstCompile]
#endif
        static void StabilizeOptimalRotation(in quaternion startRot, in quaternion targetRot, in quaternion alternateStartRot, float deltaTime, float angleStabilization, float alternateStabilization, float scaleFactor, out quaternion resultRot)
        {
            // Calculate the stabilized rotation
            BurstMathUtility.Angle(targetRot, startRot, out var rotationOffset);
            var rotationLerp = rotationOffset / angleStabilization;

            BurstMathUtility.Angle(targetRot, alternateStartRot, out var alternateRotationOffset);
            var alternateRotationLerp = alternateRotationOffset / alternateStabilization;

            if (alternateRotationLerp < rotationLerp)
            {
                alternateRotationLerp = CalculateStabilizedLerp(alternateRotationLerp, deltaTime * scaleFactor);
                resultRot = math.slerp(alternateStartRot, targetRot, alternateRotationLerp);
            }
            else
            {
                rotationLerp = CalculateStabilizedLerp(rotationLerp, deltaTime * scaleFactor);
                resultRot = math.slerp(startRot, targetRot, rotationLerp);
            }
        }

        /// <summary>
        /// Calculates a lerp value for stabilizing between a historic and current value based on their distance.
        /// The historic value is weighted more heavily the closer the distance is to 0.
        /// At a distance greater than 1, the current value is used.
        /// This filters out jitter when input is trying to be held still or moved slowly while preserving low latency for large movement.
        /// </summary>
        /// <param name="distance">The distance between a historic and current value of motion or input.</param>
        /// <param name="timeSlice">How much time has passed between when these values were recorded.</param>
        /// <returns>Returns the stabilized lerp value.</returns>
#if BURST_PRESENT
        [BurstCompile]
#endif
        static float CalculateStabilizedLerp(float distance, float timeSlice)
        {
            // The original angle stabilization code just used distance directly
            // This feels great in VR but is frame-dependent on experiences running at 90 fps
            //return Mathf.Clamp01(distance);

            // We can estimate a time-independent analog
            var originalLerp = distance;

            // If the distance has moved far enough, just use the current value for low latency movement
            if (originalLerp >= 1f)
                return 1f;

            // If the values haven't changed, then it doesn't matter what the value is so we'll just use the historic one
            if (originalLerp <= 0f)
                return 0f;

            // For fps higher than 90 fps, we scale this value
            // For fps lower than 90 fps, we take advantage of the fact that each time this algorithm
            // runs with the same values, the remaining lerp distance squares itself
            // We estimate this up to 3 time slices.  At that point the numbers just get too small to be useful
            // (and any VR experience running at 30 fps is going to be pretty rough, even with re-projection)
            var doubleFrameLerp = originalLerp - originalLerp * originalLerp;
            var tripleFrameLerp = doubleFrameLerp * doubleFrameLerp;

            var localTimeSlice = timeSlice / k_90FPS;

            var firstSlice = math.clamp(localTimeSlice, 0f, 1f);
            var secondSlice = math.clamp(localTimeSlice - 1f, 0f, 1f);
            var thirdSlice = math.clamp(localTimeSlice - 2f, 0f, 1f);

            return originalLerp * firstSlice + doubleFrameLerp * secondSlice + tripleFrameLerp * thirdSlice;
        }

        /// <summary>
        /// Helper function that calculates the rotation values needed for <see cref="StabilizeOptimalRotation"/>.
        /// </summary>
        /// <param name="currentPosition">The pre-stabilized position of the ray.</param>
        /// <param name="resultPosition">The stabilized position of the ray.</param>
        /// <param name="forward">The pre-stabilized ray forward.</param>
        /// <param name="up">The pre-stabilized ray up.</param>
        /// <param name="rayEnd">The calculated ray endpoint of the last frame.</param>
        /// <param name="invScale">The scalar that preserves local scaling.</param>
        /// <param name="angleStabilization">Maximum range (in degrees) that angle stabilization is applied.</param>
        /// <param name="antiRotation">The rotation that will make the stabilized ray point to the previous endpoint.</param>
        /// <param name="scaleFactor">Scalar to apply additional stabilization over the default calculation.</param>
        /// <param name="targetAngleScale">Maximum range (in degrees) that angle stabilization is applied, for returning the stabilized ray to the previous endpoint.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        static void CalculateRotationParams(in float3 currentPosition, in float3 resultPosition, in float3 forward, in float3 up, in float3 rayEnd, float invScale, float angleStabilization,
                                                out quaternion antiRotation, out float scaleFactor, out float targetAngleScale)
        {
            var rayLength = math.length(rayEnd - currentPosition);
            var linearRayEnd = currentPosition + forward * rayLength;

            antiRotation = quaternion.LookRotationSafe(linearRayEnd - resultPosition, up);
            scaleFactor = 1f + math.log(math.max(rayLength * invScale, 1f));
            targetAngleScale = angleStabilization * math.clamp(scaleFactor, 1f, 3f);
        }
    }
}

