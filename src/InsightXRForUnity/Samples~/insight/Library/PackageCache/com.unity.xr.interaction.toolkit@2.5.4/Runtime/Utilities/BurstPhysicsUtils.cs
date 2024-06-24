#if BURST_PRESENT
using Unity.Burst;
#endif
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Provides utility methods for physics calculations in Burst-compiled code.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public static class BurstPhysicsUtils
    {
        /// <summary>
        /// Computes sphere overlap parameters given the start and end positions of the overlap.
        /// </summary>
        /// <param name="overlapStart">The starting position of the sphere overlap.</param>
        /// <param name="overlapEnd">The ending position of the sphere overlap.</param>
        /// <param name="normalizedOverlapVector">Output parameter containing the normalized overlap direction vector.</param>
        /// <param name="overlapSqrMagnitude">Output parameter containing the square of the magnitude of the overlap vector.</param>
        /// <param name="overlapDistance">Output parameter containing the distance of the overlap.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void GetSphereOverlapParameters(in Vector3 overlapStart, in Vector3 overlapEnd, out Vector3 normalizedOverlapVector, out float overlapSqrMagnitude, out float overlapDistance)
        {
            Vector3 overlapDirectionVector = overlapEnd - overlapStart;
            overlapSqrMagnitude = math.distancesq(overlapStart, overlapEnd);
            overlapDistance = math.sqrt(overlapSqrMagnitude);
            normalizedOverlapVector = overlapDirectionVector / overlapDistance;
        }

        /// <summary>
        /// Computes conecast parameters given the angle radius, offset, and direction.
        /// </summary>
        /// <param name="angleRadius">How wide the cone should be at a given distance.</param>
        /// <param name="offset">How far from the origin this conecast will be starting from.</param>
        /// <param name="maxOffset">The maximum distance this conecast will be allowed to travel.</param>
        /// <param name="direction">The direction the conecast is traveling.</param>
        /// <param name="originOffset">How much to offset the origin of the conecast.</param>
        /// <param name="radius">The maximum radius this conecast should cover.</param>
        /// <param name="castMax">The distance this conecast should travel, taking sphere size into account.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void GetConecastParameters(float angleRadius, float offset, float maxOffset, in Vector3 direction, out Vector3 originOffset, out float radius, out float castMax)
        {
            castMax = math.clamp(offset, 0.125f, maxOffset);
            radius = angleRadius * (offset + castMax);
            originOffset = direction * (offset - radius);
        }

        /// <summary>
        /// Gets the perpendicular distance from the given point to the nearest point on the given line.
        /// </summary>
        /// <param name="origin">The starting point of the line.</param>
        /// <param name="conePoint">The point to calculate horizontal distance to.</param>
        /// <param name="direction">The direction of the line.</param>
        /// <param name="coneOffset">The horizontal distance from <paramref name="conePoint"/> to the nearest point on the line defined by <paramref name="origin"/> and <paramref name="direction"/>.</param>
#if BURST_PRESENT
        [BurstCompile]
#endif
        public static void GetConecastOffset(in float3 origin, in float3 conePoint, in float3 direction, out float coneOffset)
        {
            var hitToOrigin = conePoint - origin;
            var distance = math.dot(hitToOrigin, direction);
            var hitToRay = hitToOrigin - (direction * distance);
            coneOffset = math.length(hitToRay);
        }
    }
}