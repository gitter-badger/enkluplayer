using UnityEngine;
using UnityEngine.XR.iOS;

namespace CreateAR.SpirePlayer.AR
{
    public class IosCameraUpdater : MonoBehaviour
    {
        private Camera _camera;
        private UnityARSessionNativeInterface _interface;
        
        public void Initialize(Camera cam, UnityARSessionNativeInterface @interface)
        {
            _camera = cam;
            _interface = @interface;
        }

        private void Update ()
        {
            var matrix = _interface.GetCameraPose();
            
            _camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
            _camera.transform.localRotation = UnityARMatrixOps.GetRotation(matrix);
            _camera.projectionMatrix = _interface.GetCameraProjection();
        }
    }
}