using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities
{
    /// <summary>
    /// Utility methods for Interactables.
    /// </summary>
    public static class XRInteractableUtility
    {
        /// <summary>
        /// Struct to temporarily set <see cref="allowTriggerColliders"/> within a <see langword="using"/> block
        /// and restore the original value when disposed.
        /// </summary>
        internal struct AllowTriggerCollidersScope : IDisposable
        {
            bool m_Disposed;
            readonly bool m_OldValue;

            /// <summary>
            /// Initializes and returns and instance of <see cref="AllowTriggerCollidersScope"/>.
            /// </summary>
            /// <param name="newAllowTriggerColliders">The value to set <see cref="allowTriggerColliders"/> to.</param>
            public AllowTriggerCollidersScope(bool newAllowTriggerColliders)
            {
                m_Disposed = false;
                m_OldValue = allowTriggerColliders;
                allowTriggerColliders = newAllowTriggerColliders;
            }

            /// <summary>
            /// Dispose the scope instance and restore the original value of <see cref="allowTriggerColliders"/>.
            /// </summary>
            public void Dispose()
            {
                if (m_Disposed)
                    return;

                m_Disposed = true;
                allowTriggerColliders = m_OldValue;
            }
        }

        /// <summary>
        /// Allows for <see cref="TryGetClosestCollider"/> and <see cref="TryGetClosestPointOnCollider"/> to utilize trigger colliders.
        /// Should be set before the function call and reset after.
        /// </summary>
        static bool allowTriggerColliders { get; set; }

        /// <summary>
        /// Calculates the closest Interactable's Collider to the given location (based on the Collider Transform position).
        /// The Collider volume and surface are not considered for calculation, use <see cref="TryGetClosestPointOnCollider"/> in this case.
        /// </summary>
        /// <param name="interactable">Interactable to find the closest Collider position.</param>
        /// <param name="position">Location in world space to calculate the closest Collider position.</param>
        /// <param name="distanceInfo">If <see langword="true"/> is returned, <paramref name="distanceInfo"/> will contain the closest Collider, its position (in world space) and its distance squared to the given location.</param>
        /// <returns>Returns <see langword="true"/> if the closest Collider can be computed, for this the <paramref name="interactable"/> must have at least one active and enabled Collider. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// Only active and enabled non-trigger colliders are used in the calculation.
        /// </remarks>
        /// <seealso cref="DistanceInfo"/>
        /// <seealso cref="TryGetClosestPointOnCollider"/>
        public static bool TryGetClosestCollider(IXRInteractable interactable, Vector3 position, out DistanceInfo distanceInfo)
        {
            Vector3 closestColliderPosition = default;
            var minColDistanceSqr = float.MaxValue;
            Collider closestCollider = null;
            var hasCollider = false;
            foreach (var col in interactable.colliders)
            {
                if (col == null || !col.gameObject.activeInHierarchy || !col.enabled || (col.isTrigger && !allowTriggerColliders))
                    continue;

                var colPosition = col.transform.position;
                var offset = position - colPosition;
                var colDistanceSqr = offset.sqrMagnitude;
                if (colDistanceSqr >= minColDistanceSqr)
                    continue;

                hasCollider = true;
                minColDistanceSqr = colDistanceSqr;
                closestColliderPosition = colPosition;
                closestCollider = col;
            }

            distanceInfo = new DistanceInfo
            {
                point = closestColliderPosition,
                distanceSqr = minColDistanceSqr,
                collider = closestCollider,
            };

            return hasCollider;
        }

        /// <summary>
        /// Calculates the point on the Interactable's Colliders that is closest to the given location
        /// (based on the Collider volume and surface).
        /// </summary>
        /// <param name="interactable">Interactable to find the closest point.</param>
        /// <param name="position">Location in world space to calculate the closest point.</param>
        /// <param name="distanceInfo">
        /// If <see langword="true"/> is returned, <paramref name="distanceInfo"/> will contain the point (in world space) on the
        /// Collider that is closest to the given location, its distance squared, and the Collider that contains the point. If the given
        /// location is in the Collider, the closest point will be inside. Otherwise, the closest point will be on the
        /// surface of the Collider.
        /// </param>
        /// <returns> Returns <see langword="true"/> if the closest point can be computed, for this the <paramref name="interactable"/> must have at least one active and enabled Collider. Otherwise, returns <see langword="false"/>.</returns>
        /// <remarks>
        /// Only active and enabled non-trigger colliders are used in the calculation.
        /// The colliders can only be a <see cref="BoxCollider"/>, <see cref="SphereCollider"/>, <see cref="CapsuleCollider"/>, or convex <see cref="MeshCollider"/>.
        /// </remarks>
        /// <seealso cref="DistanceInfo"/>
        /// <seealso cref="TryGetClosestCollider"/>
        /// <seealso cref="Collider.ClosestPoint"/>
        public static bool TryGetClosestPointOnCollider(IXRInteractable interactable, Vector3 position, out DistanceInfo distanceInfo)
        {
            Vector3 closestPoint = default;
            Collider closestPointCollider = null;
            var minPointDistanceSqr = float.MaxValue;
            var hasCollider = false;
            foreach (var col in interactable.colliders)
            {
                if (col == null || !col.gameObject.activeInHierarchy || !col.enabled || (col.isTrigger && !allowTriggerColliders))
                    continue;

                var colClosestPoint = col.ClosestPoint(position);
                var offset = position - colClosestPoint;
                var pointDistanceSqr = offset.sqrMagnitude;
                if (pointDistanceSqr >= minPointDistanceSqr)
                    continue;

                hasCollider = true;
                minPointDistanceSqr = pointDistanceSqr;
                closestPoint = colClosestPoint;
                closestPointCollider = col;
            }

            distanceInfo = new DistanceInfo
            {
                point = closestPoint,
                distanceSqr = minPointDistanceSqr,
                collider = closestPointCollider,
            };

            return hasCollider;
        }
    }
}
