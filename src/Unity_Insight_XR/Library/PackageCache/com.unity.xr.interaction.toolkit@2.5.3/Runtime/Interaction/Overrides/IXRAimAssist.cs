namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// An interface that allows for computing an altered projectile velocity to allow for assisted aiming.
    /// This can be used to allow a user to throw a grab interactable to where they are looking.
    /// </summary>
    public interface IXRAimAssist
    {
        /// <summary>
        /// Takes a projectile's velocity and adjusts it to more closely hit a given target.
        /// </summary>
        /// <param name="source">The starting position of the projectile.</param>
        /// <param name="velocity">The starting velocity of the projectile.</param>
        /// <param name="gravity">How much gravity the projectile is experiencing.</param>
        /// <returns>Returns a velocity based on the source, but adjusted to hit a given target.</returns>
        public Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity);

        /// <summary>
        /// Takes a projectile's velocity and adjusts it to more closely hit a given target.
        /// </summary>
        /// <param name="source">The starting position of the projectile.</param>
        /// <param name="velocity">The starting velocity of the projectile.</param>
        /// <param name="gravity">How much gravity the projectile is experiencing.</param>
        /// <param name="maxAngle">If the angle between the initial velocity and adjusted velocity is greater than this value, no adjustment will occur.</param>
        /// <returns>Returns a velocity based on the source, but adjusted to hit a given target.</returns>
        public Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity, float maxAngle);
    }
}
