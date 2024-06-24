namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that represents a <see cref="Transform"/>-driven XR Ray.
    /// </summary>
    /// <seealso cref="XRRayInteractor"/>
    /// <seealso cref="XRGazeAssistance"/>
    public interface IXRRayProvider
    {
        /// <summary>
        /// Ensures a <see cref="Transform"/> exists for the ray origin and returns it.
        /// </summary>
        /// <returns>The <see cref="Transform"/> that is the starting position and direction of any ray casts.</returns>
        Transform GetOrCreateRayOrigin();

        /// <summary>
        /// Ensures a <see cref="Transform"/> exists for the ray attach point and returns it.
        /// </summary>
        /// <returns>The <see cref="Transform"/> that is used as the attach point for Interactables.</returns>
        Transform GetOrCreateAttachTransform();

        /// <summary>
        /// Assigns a <see cref="Transform"/> to be the source of this ray.
        /// </summary>
        /// <param name="newOrigin">The <see cref="Transform"/> that is the starting position and direction of any ray casts.</param>
        void SetRayOrigin(Transform newOrigin);

        /// <summary>
        /// Assigns a <see cref="Transform"/> to be the attach point of this ray.
        /// </summary>
        /// <param name="newAttach">The <see cref="Transform"/> that is used as the attach point for Interactables.</param>
        void SetAttachTransform(Transform newAttach);

        /// <summary>
        /// The last endpoint of this ray, either its maximum distance or a collision point.
        /// </summary>
        Vector3 rayEndPoint { get; }

        /// <summary>
        /// The <see cref="Transform"/> of the object this ray has collided with, if any.
        /// </summary>
        Transform rayEndTransform { get; }
    }
}
