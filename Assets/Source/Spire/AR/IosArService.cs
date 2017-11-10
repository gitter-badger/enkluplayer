using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.iOS;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.AR
{
    public class IosArService : IArService
    {
        private readonly UnityARSessionNativeInterface _interface;
        private readonly List<ArAnchor> _anchors = new List<ArAnchor>();
        
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
        
        public ArAnchor[] Anchors { get { return _anchors.ToArray(); }}

        public IosArService(UnityARSessionNativeInterface @interface)
        {
            _interface = @interface;
        }

        public void Setup(ArServiceConfiguration config)
        {
            _config = config;
            
            UnityARSessionNativeInterface.ARAnchorAddedEvent += Interface_OnAnchorAdded;
            UnityARSessionNativeInterface.ARAnchorUpdatedEvent += Interface_OnAnchorUpdated;
            UnityARSessionNativeInterface.ARAnchorRemovedEvent += Interface_OnAnchorRemoved;
            
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
            UnityARSessionNativeInterface.ARAnchorAddedEvent -= Interface_OnAnchorAdded;
            UnityARSessionNativeInterface.ARAnchorUpdatedEvent -= Interface_OnAnchorUpdated;
            UnityARSessionNativeInterface.ARAnchorRemovedEvent -= Interface_OnAnchorRemoved;
            
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
        
        private void Interface_OnAnchorAdded(ARPlaneAnchor data)
        {
            var anchor = new ArAnchor(data.identifier)
            {
                Center = data.center,
                Extents = data.extent
            };
            
            UpdateAnchorTransform(anchor, data.transform);
            
            _anchors.Add(anchor);
        }
        
        private void Interface_OnAnchorUpdated(ARPlaneAnchor data)
        {
            var anchor = Anchor(data.identifier);
            if (null != anchor)
            {
                anchor.Center = data.center;
                anchor.Extents = data.extent;
                
                UpdateAnchorTransform(anchor, data.transform);
            }
            else
            {
                Log.Warning(this, "Received an anchor update for an anchor we aren't tracking: {0}.", data.identifier);
                
                Interface_OnAnchorAdded(data);
            }
        }
        
        private void Interface_OnAnchorRemoved(ARPlaneAnchor data)
        {
            var anchor = Anchor(data.identifier);
            if (null != anchor)
            {
                _anchors.Remove(anchor);
            }
        }

        private ArAnchor Anchor(string id)
        {
            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
                if (anchor.Id == id)
                {
                    return anchor;
                }
            }

            return null;
        }
        
        private void UpdateAnchorTransform(ArAnchor anchor, Matrix4x4 transform)
        {
            var position = transform.GetColumn(3);
            position.z = -position.z;
            anchor.Position = position;
            
            // Convert from ARKit's right-handed coordinate system to Unity's left-handed
            var rotation = QuaternionFromMatrix(transform);
            rotation.z = -rotation.z;
            rotation.w = -rotation.w;
            anchor.Rotation = rotation;
        }
        
        private static Quaternion QuaternionFromMatrix(Matrix4x4 matrix)
        {
            // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            var quat = new Quaternion
            {
                w = Mathf.Sqrt(Mathf.Max(0, 1 + matrix[0, 0] + matrix[1, 1] + matrix[2, 2])) / 2,
                x = Mathf.Sqrt(Mathf.Max(0, 1 + matrix[0, 0] - matrix[1, 1] - matrix[2, 2])) / 2,
                y = Mathf.Sqrt(Mathf.Max(0, 1 - matrix[0, 0] + matrix[1, 1] - matrix[2, 2])) / 2,
                z = Mathf.Sqrt(Mathf.Max(0, 1 - matrix[0, 0] - matrix[1, 1] + matrix[2, 2])) / 2
            };
            
            quat.x *= Mathf.Sign( quat.x * ( matrix[2,1] - matrix[1,2] ) );
            quat.y *= Mathf.Sign( quat.y * ( matrix[0,2] - matrix[2,0] ) );
            quat.z *= Mathf.Sign( quat.z * ( matrix[1,0] - matrix[0,1] ) );
            
            return quat;
        }
    }
}