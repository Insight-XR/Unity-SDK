using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="Quaternion"/> structs.
    /// </summary>
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Returns a rotation that only contains the yaw component of the specified rotation.
        /// The resulting rotation is not normalized.
        /// </summary>
        /// <param name="rotation">The source rotation.</param>
        /// <returns>A yaw-only rotation that matches the input rotation's yaw.</returns>
        public static Quaternion ConstrainYaw(this Quaternion rotation)
        {
            rotation.x = 0;
            rotation.z = 0;
            return rotation;
        }

        /// <summary>
        /// Returns a normalized rotation that only contains the yaw component of the specified rotation.
        /// </summary>
        /// <param name="rotation">The source rotation.</param>
        /// <returns>A yaw-only rotation that matches the input rotation's yaw.</returns>
        public static Quaternion ConstrainYawNormalized(this Quaternion rotation)
        {
            rotation.x = 0;
            rotation.z = 0;
            rotation.Normalize();
            return rotation;
        }

        /// <summary>
        /// Returns a normalized rotation that only contains the yaw and pitch components of the specified rotation
        /// </summary>
        /// <param name="rotation">The source rotation.</param>
        /// <returns>A yaw- and pitch-only rotation that matches the input rotation's yaw and pitch.</returns>
        public static Quaternion ConstrainYawPitchNormalized(this Quaternion rotation)
        {
            var euler = rotation.eulerAngles;
            euler.z = 0;
            return Quaternion.Euler(euler);
        }
    }
}
