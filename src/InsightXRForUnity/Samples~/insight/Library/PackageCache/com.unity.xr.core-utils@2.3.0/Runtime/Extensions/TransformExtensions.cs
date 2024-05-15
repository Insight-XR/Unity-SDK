using System;
using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for <see cref="Transform"/> components.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Gets the local position and rotation as a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform from which to get the pose.</param>
        /// <returns>The local pose.</returns>
        public static Pose GetLocalPose(this Transform transform)
        {
#if HAS_GET_POSITION_AND_ROTATION
            transform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            return new Pose(localPosition, localRotation);
#else
            return new Pose(transform.localPosition, transform.localRotation);
#endif
        }

        /// <summary>
        /// Gets the world position and rotation as a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform from which to get the pose.</param>
        /// <returns>The world pose.</returns>
        public static Pose GetWorldPose(this Transform transform)
        {
#if HAS_GET_POSITION_AND_ROTATION
            transform.GetPositionAndRotation(out var position, out var rotation);
            return new Pose(position, rotation);
#else
            return new Pose(transform.position, transform.rotation);
#endif
        }

        /// <summary>
        /// Sets the local position and rotation from a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform on which to set the pose.</param>
        /// <param name="pose">Pose specifying the new position and rotation.</param>
        public static void SetLocalPose(this Transform transform, Pose pose)
        {
#if HAS_SET_LOCAL_POSITION_AND_ROTATION
            transform.SetLocalPositionAndRotation(pose.position, pose.rotation);
#else
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
#endif
        }

        /// <summary>
        /// Sets the world position and rotation from a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform on which to set the pose.</param>
        /// <param name="pose">Pose specifying the new position and rotation.</param>
        public static void SetWorldPose(this Transform transform, Pose pose)
        {
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        /// <summary>
        /// Transforms a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="pose">The <c>Pose</c> to transform.</param>
        /// <returns>A new <c>Pose</c> representing the transformed <paramref name="pose"/>.</returns>
        public static Pose TransformPose(this Transform transform, Pose pose)
        {
            return pose.GetTransformedBy(transform);
        }

        /// <summary>
        /// Inverse transforms a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="pose">The <c>Pose</c> to inversely transform.</param>
        /// <returns>A new <c>Pose</c> representing the inversely transformed <paramref name="pose"/>.</returns>
        public static Pose InverseTransformPose(this Transform transform, Pose pose)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Pose
            {
                position = transform.InverseTransformPoint(pose.position),
                rotation = Quaternion.Inverse(transform.rotation) * pose.rotation
            };
        }

        /// <summary>
        /// Inverse transforms a <see cref="Ray"/>.
        /// </summary>
        /// <param name="transform">The <c>Transform</c> component.</param>
        /// <param name="ray">The <c>Ray</c> to inversely transform.</param>
        /// <returns>A new <c>Ray</c> representing the inversely transformed <paramref name="ray"/>.</returns>
        public static Ray InverseTransformRay(this Transform transform, Ray ray)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            return new Ray(
                transform.InverseTransformPoint(ray.origin),
                transform.InverseTransformDirection(ray.direction));
        }

    }
}
