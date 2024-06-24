#if BURST_PRESENT
using Unity.Burst;
#else
using System.Runtime.CompilerServices;
#endif
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Provides utility functions related to calculations for determining if things can be seen from a viewpoint.
    /// </summary>
#if BURST_PRESENT
    [BurstCompile]
#endif
    public static class BurstGazeUtility
    {
        /// <summary>
        /// Returns if a given position is outside of a specific viewpoint
        /// </summary>
        /// <param name="gazePosition">The position of the viewer</param>
        /// <param name="gazeDirection">The direction the viewer is facing</param>
        /// <param name="targetPosition">The position of the object being viewed</param>
        /// <param name="angleThreshold">How wide a field of view the viewer has</param>
        /// <returns></returns>
        public static bool IsOutsideGaze(in float3 gazePosition, in float3 gazeDirection, in float3 targetPosition, float angleThreshold)
        {
            var outsideThreshold = false;

            var testVector = math.normalize(targetPosition - gazePosition);

            outsideThreshold = !IsAlignedToGazeForward(gazeDirection, testVector, angleThreshold);

            return outsideThreshold;
        }
       
        /// <summary>
        /// Returns if a given direction is aligned with a viewer (looking at it)
        /// </summary>
        /// <param name="gazeDirection">The direction the viewer is facing</param>
        /// <param name="targetDirection">The direction the target is facing</param>
        /// <param name="angleThreshold">How far the viewer and target can diverge and still be considered looking at one another</param>
        /// <returns></returns>
        public static bool IsAlignedToGazeForward(in float3 gazeDirection, in float3 targetDirection, float angleThreshold)
        {
            var insideThreshold = false;
            var angleThresholdConvertedToDot = math.cos(math.radians(angleThreshold));
            var angularComparison = math.dot(targetDirection, gazeDirection);
            insideThreshold = angularComparison > angleThresholdConvertedToDot;

            return insideThreshold;
        }

        /// <summary>
        /// Returns if a given position is outside of a given view range
        /// </summary>
        /// <param name="gazePosition">The position of the viewer</param>
        /// <param name="targetPosition">The position of the target</param>
        /// <param name="distanceThreshold">How far away a target can be before it is outside the viewing range</param>
        /// <returns></returns>
        public static bool IsOutsideDistanceRange(in float3 gazePosition, in float3 targetPosition, float distanceThreshold)
        {
            return math.length(targetPosition - gazePosition) > distanceThreshold;
        }
    }
}