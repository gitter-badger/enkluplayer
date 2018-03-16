using CreateAR.SpirePlayer.Assets;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that shows objects loading.
    /// </summary>
    public interface ILoadProgressManager
    {
        /// <summary>
        /// Shows load progress for an area. When the progress object is complete,
        /// the load indicator is automatically removed.
        /// </summary>
        /// <param name="center">Center of the load.</param>
        /// <param name="min">Minimum of bounds in world space.</param>
        /// <param name="max">Maximum of bounds in world space.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>A unique id corresponding to the load progress indicator.</returns>
        uint ShowIndicator(Vec3 center, Vec3 min, Vec3 max, LoadProgress progress);

        /// <summary>
        /// Updates a load progress indicator with new bounds.
        /// </summary>
        /// <param name="id">The unique id returned by <c>Show</c>.</param>
        /// <param name="min">Minimum of bounds in world space.</param>
        /// <param name="max">Maximum of bounds in world space.</param>
        void UpdateIndicator(uint id, Vec3 min, Vec3 max);

        /// <summary>
        /// Hides a load progress indicator.
        /// </summary>
        /// <param name="id">The unique id.</param>
        void HideIndicator(uint id);
    }
}