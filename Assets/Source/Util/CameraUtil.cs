using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Utilities when working with Cameras.
    /// </summary>
    public static class CameraUtil
    {
        /// <summary>
        /// Casts a ray through a screenpoint and into the floor.
        /// </summary>
        /// <param name="camera">The camera to use.</param>
        /// <param name="screenPoint">The screen point.</param>
        /// <returns></returns>
        public static Vector3 ScreenSpaceToFloorIntersection(
            Camera camera,
            Vector2 screenPoint)
        {
            var ray = camera.ScreenPointToRay(screenPoint);
            var s = -ray.origin.y / ray.direction.y;
            return ray.origin + s * ray.direction;
        }
    }
}