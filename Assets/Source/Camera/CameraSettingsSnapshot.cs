using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// A snapshot of a camera's settings.
    /// </summary>
    public struct CameraSettingsSnapshot
    {
        /// <summary>
        /// Camera::clearFlags.
        /// </summary>
        public CameraClearFlags ClearFlags;

        /// <summary>
        /// Camera::backgroundColor.
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// Camera::nearClipPlane.
        /// </summary>
        public float NearClipPlane;

        /// <summary>
        /// Camera::farFlipPlane.
        /// </summary>
        public float FarClipPlane;

        /// <summary>
        /// Creates a snapshot from a camera.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <returns></returns>
        public static CameraSettingsSnapshot Snapshot(Camera camera)
        {
            return new CameraSettingsSnapshot
            {
                ClearFlags = camera.clearFlags,
                BackgroundColor = camera.backgroundColor,
                NearClipPlane = camera.nearClipPlane,
                FarClipPlane = camera.farClipPlane
            };
        }

        /// <summary>
        /// Applies a snapshot to a camera.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="snapshot">The snapshot.</param>
        public static void Apply(Camera camera, CameraSettingsSnapshot snapshot)
        {
            camera.clearFlags = snapshot.ClearFlags;
            camera.backgroundColor = snapshot.BackgroundColor;
            camera.nearClipPlane = snapshot.NearClipPlane;
            camera.farClipPlane = snapshot.FarClipPlane;
        }
    }
}