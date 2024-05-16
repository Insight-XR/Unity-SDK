using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Struct used to get information back from a distance calculation between an object and a location.
    /// </summary>
    /// <seealso cref="XRBaseInteractable.GetDistance"/>
    /// <seealso cref="XRInteractableUtility.TryGetClosestCollider"/>
    /// <seealso cref="XRInteractableUtility.TryGetClosestPointOnCollider"/>
    public struct DistanceInfo
    {
        /// <summary>
        /// The location on the object (in world space) where the distance was calculated from.
        /// </summary>
        /// <remarks>
        /// When used with the method <see cref="XRInteractableUtility.TryGetClosestPointOnCollider"/>, <see cref="point"/>
        /// contains the <see cref="collider"/>'s position.
        ///
        /// When used with the method <see cref="XRInteractableUtility.TryGetClosestCollider"/>, this property contains the point
        /// on the <see cref="collider"/> closest to the location used for calculation.
        /// </remarks>
        public Vector3 point { get; set; }

        /// <summary>
        /// The distance squared between <see cref="point"/> and the location used for calculation.
        /// </summary>
        public float distanceSqr { get; set; }

        /// <summary>
        /// The collider associated with the <see cref="point"/>.
        /// Returns <see langword="null"/> if the distance calculation doesn't involve colliders, or if there is no valid collider for calculation.
        /// </summary>
        public Collider collider { get; set; }
    }
}
