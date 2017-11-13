using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.iOS;
using WebSocketSharp;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.AR
{
    public class IosArService : IArService
    {
        private readonly UnityARSessionNativeInterface _interface;
        private readonly List<ArAnchor> _anchors = new List<ArAnchor>();
        
        private ArCameraRig _rig;
        
        public ArAnchor[] Anchors { get { return _anchors.ToArray(); }}
        public ArServiceConfiguration Config { get; private set; }

        public IosArService(UnityARSessionNativeInterface @interface)
        {
            _interface = @interface;
        }

        public void Setup(ArServiceConfiguration config)
        {
            Config = config;
            _rig = config.Rig;
            
            // setup camera
            Camera.main.clearFlags = CameraClearFlags.Depth;
            
            // listen to the native interface
            UnityARSessionNativeInterface.ARAnchorAddedEvent += Interface_OnAnchorAdded;
            UnityARSessionNativeInterface.ARAnchorUpdatedEvent += Interface_OnAnchorUpdated;
            UnityARSessionNativeInterface.ARAnchorRemovedEvent += Interface_OnAnchorRemoved;
            
            // startup!
            _interface.RunWithConfigAndOptions(
                new ARKitWorldTrackingSessionConfiguration
                {
                    planeDetection = Config.EnablePlaneDetection
                        ? UnityARPlaneDetection.Horizontal
                        : UnityARPlaneDetection.None,
                    alignment = UnityARAlignment.UnityARAlignmentGravityAndHeading,
                    enableLightEstimation = Config.EnableLightEstimation,
                    getPointCloudData = Config.EnablePointCloud
                },
                UnityARSessionRunOption.ARSessionRunOptionResetTracking);
            
            InitializeCameraRig();
        }

        public void Teardown()
        {
            UninitializeCameraRig();
            
            UnityARSessionNativeInterface.ARAnchorAddedEvent -= Interface_OnAnchorAdded;
            UnityARSessionNativeInterface.ARAnchorUpdatedEvent -= Interface_OnAnchorUpdated;
            UnityARSessionNativeInterface.ARAnchorRemovedEvent -= Interface_OnAnchorRemoved;
            
            _interface.Pause();
        }

        private void UninitializeCameraRig()
        {
            if (null == _rig)
            {
                return;
            }

            var manager = _rig.GetComponent<IosCameraUpdater>();
            if (null != manager)
            {
                Object.Destroy(manager);
            }

            var video = _rig.Camera.GetComponent<UnityARVideo>();
            if (null != video)
            {
                Object.Destroy(video);
            }
        }

        private void InitializeCameraRig()
        {
            if (null == _rig)
            {
                return;
            }

            _rig.gameObject
                .AddComponent<IosCameraUpdater>()
                .Initialize(Camera.main, _interface);
            
            if (Config.ShowCameraFeed)
            {
                var video = _rig.Camera.gameObject.AddComponent<UnityARVideo>();
                video.m_ClearMaterial = Config.CameraMaterial;
            }
        }
        
        private void Interface_OnAnchorAdded(ARPlaneAnchor data)
        {
            var anchor = new ArAnchor(data.identifier)
            {
                Extents = data.extent
            };
            
            UpdateAnchorTransform(anchor, data);
            
            _anchors.Add(anchor);
        }
        
        private void Interface_OnAnchorUpdated(ARPlaneAnchor data)
        {
            var anchor = Anchor(data.identifier);
            if (null != anchor)
            {
                anchor.Extents = data.extent;
                
                UpdateAnchorTransform(anchor, data);
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
        
        private void UpdateAnchorTransform(
            ArAnchor anchor,
            ARPlaneAnchor data)
        {
            // Convert from ARKit's right-handed coordinate system to Unity's left-handed
            
            // position
            var position = (Vector3) data.transform.GetColumn(3); 
            position.z = -position.z;
            
            // offset position by center
            anchor.Position = position + new Vector3(
                  data.center.x,
                  data.center.y,
                  -data.center.z);
            
            // set rotation
            var rotation = QuaternionFromMatrix(data.transform);
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