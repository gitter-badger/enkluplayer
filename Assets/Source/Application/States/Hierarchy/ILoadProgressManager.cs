using CreateAR.SpirePlayer.Assets;
using UnityEngine;

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
        /// <param name="transform">The transform.</param>
        /// <param name="bounds">Asset bounds.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>A unique id corresponding to the load progress indicator.</returns>
        uint ShowIndicator(Transform transform, Bounds bounds, LoadProgress progress);

        /// <summary>
        /// Hides a load progress indicator.
        /// </summary>
        /// <param name="id">The unique id.</param>
        void HideIndicator(uint id);
    }
}