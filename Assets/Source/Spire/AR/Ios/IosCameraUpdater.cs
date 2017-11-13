using UnityEngine;
using UnityEngine.XR.iOS;

namespace CreateAR.SpirePlayer.AR
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
        
        /// <summary>
        /// Native interface for unity.
        /// </summary>
        private UnityARSessionNativeInterface _interface;
        
        /// <summary>
        /// Initializes the camera.
        /// </summary>
        /// <param name="cam">Camera to render with.</param>
        /// <param name="interface">Native interface.</param>
        public void Initialize(Camera cam, UnityARSessionNativeInterface @interface)
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
            
            _camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
            _camera.transform.localRotation = UnityARMatrixOps.GetRotation(matrix);
            _camera.projectionMatrix = _interface.GetCameraProjection();
        }
    }
}