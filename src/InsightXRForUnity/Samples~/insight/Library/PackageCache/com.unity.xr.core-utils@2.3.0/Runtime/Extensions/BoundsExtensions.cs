using UnityEngine;

namespace Unity.XR.CoreUtils
{
    /// <summary>
    /// Extension methods for the <see cref="Bounds"/> type.
    /// </summary>
    public static class BoundsExtensions
    {
        /// <summary>
        /// Returns a whether the given bounds are contained completely within this one.
        /// </summary>
        /// <remarks>If a boundary value is the same for both <see cref="Bounds"/> objects,
        /// that boundary is considered to be within the <paramref name="outerBounds"/>.</remarks>
        /// <param name="outerBounds">The outer bounds which may contain the inner bounds.</param>
        /// <param name="innerBounds">The inner bounds that may or may not fit within outerBounds.</param>
        /// <returns>True if outerBounds completely encloses innerBounds.</returns>
        public static bool ContainsCompletely(this Bounds outerBounds, Bounds innerBounds)
        {
            var outerBoundsMax = outerBounds.max;
            var outerBoundsMin = outerBounds.min;
            var innerBoundsMax = innerBounds.max;
            var innerBoundsMin = innerBounds.min;
            return outerBoundsMax.x >= innerBoundsMax.x && outerBoundsMax.y >= innerBoundsMax.y && outerBoundsMax.z >= innerBoundsMax.z
                && outerBoundsMin.x <= innerBoundsMin.x && outerBoundsMin.y <= innerBoundsMin.y && outerBoundsMin.z <= innerBoundsMin.z;
        }
    }
}
