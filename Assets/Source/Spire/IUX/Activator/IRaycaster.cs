namespace CreateAR.SpirePlayer
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
    }
}