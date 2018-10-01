namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Raycasting interface.
    /// </summary>
    public interface IRaycaster
    {
        /// <summary>
        /// Raycast.
        /// </summary>
        /// <param name="origin">Start position.</param>
        /// <param name="direction">Direction.</param>
        /// <returns></returns>
        bool Raycast(Vec3 origin, Vec3 direction);

        /// <summary>
        /// Raycast. Includes hit/collider positions.
        /// </summary>
        /// <param name="origin">Start position.</param>
        /// <param name="direction">Direction.</param>
        /// <param name="hitPosition">Hit position.</param>
        /// <param name="colliderCenter">Collider position.</param>
        /// <returns></returns>
        bool Raycast(Vec3 origin, Vec3 direction, out Vec3 hitPosition, out Vec3 colliderCenter);
    }
}