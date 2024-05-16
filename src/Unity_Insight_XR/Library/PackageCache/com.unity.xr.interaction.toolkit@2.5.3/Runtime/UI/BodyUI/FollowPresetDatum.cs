using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.UI.BodyUI
{
    /// <summary>
    /// Represents the reference axis relative to the tracking anchor used to compare up and camera facing direction. 
    /// </summary>
    public enum FollowReferenceAxis
    {
        /// <summary>
        /// Represents the positive X axis.
        /// </summary>
        Right,

        /// <summary>
        /// Represents the positive Y axis.
        /// </summary>
        Up,

        /// <summary>
        /// Represents the positive Z axis.
        /// </summary>
        Forward,

        /// <summary>
        /// Represents the negative X axis.
        /// </summary>
        Left,

        /// <summary>
        /// Represents the negative Y axis.
        /// </summary>
        Down,

        /// <summary>
        /// Represents the negative Z axis.
        /// </summary>
        Back,
    }

    /// <summary>
    /// Class that defines the configuration of a following behaviour for a hand or object.
    /// It determines how an object should follow the hand and includes specifications about local position and rotation,
    /// angle constraints, gaze snapping, and smoothing settings.
    /// </summary>
    [Serializable]
    public class FollowPreset
    {
        /// <summary>
        /// Local space anchor position for the right hand.
        /// </summary>
        [Header("Local Space Anchor Transform")]
        [Tooltip("Local space anchor position for the right hand.")]
        public Vector3 rightHandLocalPosition;

        /// <summary>
        /// Local space anchor position for the left hand.
        /// </summary>
        [Tooltip("Local space anchor position for the left hand.")]
        public Vector3 leftHandLocalPosition;

        /// <summary>
        /// Local space anchor rotation for the right hand.
        /// </summary>
        [Tooltip("Local space anchor rotation for the right hand.")]
        public Vector3 rightHandLocalRotation;

        /// <summary>
        /// Local space anchor rotation for the left hand.
        /// </summary>
        [Tooltip("Local space anchor rotation for the left hand.")]
        public Vector3 leftHandLocalRotation;

        /// <summary>
        /// Reference axis equivalent used for comparisons with the user's gaze direction and the world up direction.
        /// </summary>
        [Header("Hand anchor angle constraints")]
        [Tooltip("Reference axis equivalent used for comparisons with the user's gaze direction and the world up direction.")]
        public FollowReferenceAxis palmReferenceAxis = FollowReferenceAxis.Down;

        /// <summary>
        /// Given that the default reference hand for menus is the left hand, it may be required to mirror the reference axis for the right hand.
        /// This is not necessary if using the up or down axis as a reference, which is the default for hand tracking. Controllers work best with the right and left axies.
        /// </summary>
        [Tooltip("Given that the default reference hand for menus is the left hand, it may be required to mirror the reference axis for the right hand.")]
        public bool invertAxisForRightHand;

        /// <summary>
        /// Check status of palm reference axis facing the user.
        /// </summary>
        [Tooltip("Whether or not check if the palm reference axis is facing the user.")]
        public bool requirePalmFacingUser;

        /// <summary>
        /// Angle threshold to check if the palm reference axis is facing the user.
        /// </summary>
        [Tooltip("The angle threshold in degrees to check if the palm reference axis is facing the user.")]
        public float palmFacingUserDegreeAngleThreshold;

        /// <summary>
        /// The dot product equivalent to the angle threshold used to check if the palm reference axis is facing the user.
        /// </summary>
        public float palmFacingUserDotThreshold => m_PalmFacingUserDotThreshold;

        float m_PalmFacingUserDotThreshold;

        /// <summary>
        /// Check status of palm reference axis facing up.
        /// </summary>
        [Tooltip("Whether or not check if the palm reference axis is facing up.")]
        public bool requirePalmFacingUp;

        /// <summary>
        /// Angle threshold to check if the palm reference axis is facing up.
        /// </summary>
        [Tooltip("The angle threshold in degrees to check if the palm reference axis is facing up.")]
        public float palmFacingUpDegreeAngleThreshold;

        /// <summary>
        /// The dot product equivalent to the angle threshold used to check if the palm reference axis is facing up.
        /// </summary>
        public float palmFacingUpDotThreshold => m_PalmFacingUpDotThreshold;

        float m_PalmFacingUpDotThreshold;

        /// <summary>
        /// Configures the snap to gaze option.
        /// </summary>
        [Header("Snap To gaze config")]
        [Tooltip("Whether to snap the following element to the gaze direction.")]
        public bool snapToGaze;

        /// <summary>
        /// The angle threshold in degrees to snap the following element to the gaze direction.
        /// </summary>
        [Tooltip("The angle threshold in degrees to snap the following element to the gaze direction.")]
        public float snapToGazeAngleThreshold;

        /// <summary>
        /// Dot product threshold for snap to gaze.
        /// </summary>
        public float snapToGazeDotThreshold => m_SnapToGazeDotThreshold;

        float m_SnapToGazeDotThreshold;

        /// <summary>
        /// The amount of time in seconds to wait before hiding the following element after the hand is no longer tracked.
        /// </summary>
        [Header("Hide delay config")]
        [Tooltip("The amount of time in seconds to wait before hiding the following element after the hand is no longer tracked.")]
        public float hideDelaySeconds = 0.25f;

        /// <summary>
        /// Whether to allow smoothing of the following element position and rotation.
        /// </summary>
        [Header("Smoothing Config")]
        [Tooltip("Whether to allow smoothing of the following element position and rotation.")]
        public bool allowSmoothing = true;

        /// <summary>
        /// The lower bound of smoothing to apply.
        /// </summary>
        [Tooltip("The lower bound of smoothing to apply.")]
        public float followLowerSmoothingValue = 10f;

        /// <summary>
        /// The upper bound of smoothing to apply.
        /// </summary>
        [Tooltip("The upper bound of smoothing to apply.")]
        public float followUpperSmoothingValue = 16f;

        /// <summary>
        /// Applies this preset to the specified tracking offsets for the left and right local positions and rotations.
        /// Also recomputes dot product thresholds.
        /// </summary>
        /// <param name="leftTrackingOffset">The transform object that represents the left tracking offset.</param>
        /// <param name="rightTrackingOffset">The transform object that represents the right tracking offset.</param>
        public void ApplyPreset(Transform leftTrackingOffset, Transform rightTrackingOffset)
        {
            leftTrackingOffset.transform.localPosition = leftHandLocalPosition;
            leftTrackingOffset.transform.localRotation = Quaternion.Euler(leftHandLocalRotation);

            rightTrackingOffset.transform.localPosition = rightHandLocalPosition;
            rightTrackingOffset.transform.localRotation = Quaternion.Euler(rightHandLocalRotation);
            ComputeDotProductThresholds();
        }

        /// <summary>
        /// Update the dot product thresholds based on the current angle thresholds.
        /// </summary>
        public void ComputeDotProductThresholds()
        {
            m_PalmFacingUserDotThreshold = AngleToDot(palmFacingUserDegreeAngleThreshold);
            m_PalmFacingUpDotThreshold = AngleToDot(palmFacingUpDegreeAngleThreshold);
            m_SnapToGazeDotThreshold = AngleToDot(snapToGazeAngleThreshold);
        }

        static float AngleToDot(float angleDeg)
        {
            return Mathf.Cos(Mathf.Deg2Rad * angleDeg);
        }

        /// <summary>
        /// Gets the reference axis relative to the specified tracking root.
        /// Adjusts the return value depending on whether or not this is for the user's right hand.
        /// </summary>
        /// <param name="trackingRoot">Tracking root transform.</param>
        /// <param name="isRightHand">Whether this is for the user's right hand or not.</param>
        /// <returns></returns>
        public Vector3 GetReferenceAxisForTrackingAnchor(Transform trackingRoot, bool isRightHand)
        {
            return trackingRoot.TransformDirection(GetLocalAxis(isRightHand));
        }

        Vector3 GetLocalAxis(bool isRightHand)
        {
            Vector3 axis = Vector3.zero;
            bool invert = isRightHand && invertAxisForRightHand;
            switch (palmReferenceAxis)
            {
                case FollowReferenceAxis.Right:
                    axis = invert ? Vector3.left : Vector3.right;
                    break;
                case FollowReferenceAxis.Up:
                    axis = invert ? Vector3.down : Vector3.up;
                    break;
                case FollowReferenceAxis.Forward:
                    axis = invert ? Vector3.back : Vector3.forward;
                    break;
                case FollowReferenceAxis.Left:
                    axis = invert ? Vector3.right : Vector3.left;
                    break;
                case FollowReferenceAxis.Down:
                    axis = invert ? Vector3.up : Vector3.down;
                    break;
                case FollowReferenceAxis.Back:
                    axis = invert ? Vector3.forward : Vector3.back;
                    break;
            }

            return axis;
        }
    }

    /// <summary>
    /// Serializable container class that holds a <see cref="FollowPreset"/> value or container asset reference.
    /// </summary>
    /// <seealso cref="FollowPresetDatum"/>
    [Serializable]
    public class FollowPresetDatumProperty : DatumProperty<FollowPreset, FollowPresetDatum>
    {
        /// <inheritdoc/>
        public FollowPresetDatumProperty(FollowPreset value) : base(value)
        {
        }

        /// <inheritdoc/>
        public FollowPresetDatumProperty(FollowPresetDatum datum) : base(datum)
        {
        }
    }

    /// <summary>
    /// <see cref="ScriptableObject"/> container class that holds a float affordance theme value.
    /// </summary>
    [CreateAssetMenu(fileName = "Follow Preset Datum", menuName = "XR/Value Datums/Body UI Follow Preset Datum", order = 0)]
    [HelpURL(XRHelpURLConstants.k_FollowPresetDatum)]
    public class FollowPresetDatum : Datum<FollowPreset>
    {
    }
}