using UnityEngine;
using UnityEngine.XR.iOS;

namespace CreateAR.SpirePlayer.AR
{
    public class IosArService : IArService
    {
        private readonly UnityARSessionNativeInterface _interface;
        private ArServiceConfiguration _config;
        private Camera _camera;

        public Camera Camera
        {
            get
            {
                return _camera;
            }
            set
            {
                if (value == _camera)
                {
                    return;
                }

                UninitializeCamera();

                _camera = value;

                InitializeCamera();
            }
        }

        public IosArService(UnityARSessionNativeInterface @interface)
        {
            _interface = @interface;
        }

        public void Setup(ArServiceConfiguration config)
        {
            _config = config;
            
            _interface.RunWithConfigAndOptions(
                new ARKitWorldTrackingSessionConfiguration
                {
                    planeDetection = _config.EnablePlaneDetection
                        ? UnityARPlaneDetection.Horizontal
                        : UnityARPlaneDetection.None,
                    alignment = UnityARAlignment.UnityARAlignmentGravityAndHeading,
                    enableLightEstimation = _config.EnableLightEstimation,
                    getPointCloudData = _config.EnablePointCloud
                    
                },
                UnityARSessionRunOption.ARSessionRunOptionResetTracking);
        }

        public void Teardown()
        {
            _interface.Pause();
        }

        private void UninitializeCamera()
        {
            if (null == _camera)
            {
                return;
            }

            var manager = _camera.GetComponent<UnityARCameraManager>();
            if (null != manager)
            {
                Object.Destroy(manager);
            }

            var video = _camera.GetComponent<UnityARVideo>();
            if (null != video)
            {
                Object.Destroy(video);
            }
        }

        private void InitializeCamera()
        {
            if (null == _camera)
            {
                return;
            }

            _camera.gameObject.AddComponent<UnityARCameraManager>();
            
            if (_config.ShowCameraFeed)
            {
                var video = _camera.gameObject.AddComponent<UnityARVideo>();
                video.m_ClearMaterial = _config.CameraMaterial;
            }
        }
    }
}