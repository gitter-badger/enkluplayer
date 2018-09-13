using UnityEngine;

namespace CreateAR.EnkluPlayer.AR
{
    /// <summary>
    /// Monobehavior that updates the camera.
    /// </summary>
    public class IosCameraUpdater : MonoBehaviour
    {
        /// <summary>
        /// Camera!
        /// </summary>
        private Camera _camera;

#if UNITY_IOS
        /// <summary>
        /// Native interface for unity.
        /// </summary>
        private UnityEngine.XR.iOS.UnityARSessionNativeInterface _interface;

        /// <summary>
        /// Initializes the camera.
        /// </summary>
        /// <param name="cam">Camera to render with.</param>
        /// <param name="interface">Native interface.</param>
        public void Initialize(Camera cam, UnityEngine.XR.iOS.UnityARSessionNativeInterface @interface)
        {
            _camera = cam;
            _interface = @interface;
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update ()
        {
            var matrix = _interface.GetCameraPose();
            
            _camera.transform.localPosition = UnityEngine.XR.iOS.UnityARMatrixOps.GetPosition(matrix);
            _camera.transform.localRotation = UnityEngine.XR.iOS.UnityARMatrixOps.GetRotation(matrix);
            _camera.projectionMatrix = _interface.GetCameraProjection();
        }
#endif
    }
}