#if UNITY_IOS

using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.XR.iOS;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Provides AR implementation for iOS.
    /// </summary>
    public class IosArService : IArService
    {
        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;
        
        /// <summary>
        /// Native interface, provided by Unity.
        /// </summary>
        private readonly UnityARSessionNativeInterface _interface;
        
        /// <summary>
        /// List of all anchors.
        /// </summary>
        private readonly List<ArAnchor> _anchors = new List<ArAnchor>();
        
        /// <summary>
        /// The camera rig.
        /// </summary>
        private ArCameraRig _rig;

        /// <inheritdoc />
        public List<ArAnchor> Anchors { get { return _anchors; }}
        
        /// <inheritdoc />
        public ArServiceConfiguration Config { get; private set; }
        
        /// <inheritdoc />
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Video.
        /// </summary>
        public UnityARVideo Video { get; private set; }
        
        /// <inheritdoc />
        public event Action OnTrackingOffline;
        
        /// <inheritdoc />
        public event Action OnTrackingOnline;

        /// <summary>
        /// Constructor.
        /// </summary>
        public IosArService(
            IMessageRouter messages,
            UnityARSessionNativeInterface @interface)
        {
            _messages = messages;
            _interface = @interface;
        }
    
        /// <inheritdoc />
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
            UnityARSessionNativeInterface.ARSessionFailedEvent += Interface_OnSessionFailed;
            UnityARSessionNativeInterface.ARSessionInterruptedEvent += Interface_OnInterrupted;
            UnityARSessionNativeInterface.ARSessioninterruptionEndedEvent += Interface_OnInterruptEnded;
            
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

            IsSetup = true;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            IsSetup = false;
            UninitializeCameraRig();
            
            UnityARSessionNativeInterface.ARAnchorAddedEvent -= Interface_OnAnchorAdded;
            UnityARSessionNativeInterface.ARAnchorUpdatedEvent -= Interface_OnAnchorUpdated;
            UnityARSessionNativeInterface.ARAnchorRemovedEvent -= Interface_OnAnchorRemoved;
            UnityARSessionNativeInterface.ARSessionFailedEvent -= Interface_OnSessionFailed;
            UnityARSessionNativeInterface.ARSessionInterruptedEvent -= Interface_OnInterrupted;
            UnityARSessionNativeInterface.ARSessioninterruptionEndedEvent -= Interface_OnInterruptEnded;
            
            _interface.Pause();
        }

        /// <summary>
        /// Uninitializes all the stuff we did to the camera.
        /// </summary>
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

        /// <summary>
        /// Sets up the camera.
        /// </summary>
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
                Video = _rig.Camera.gameObject.AddComponent<UnityARVideo>();
                Video.m_ClearMaterial = Config.GetCameraMaterialForPlatform(RuntimePlatform.IPhonePlayer);
            }
        }
        
        /// <summary>
        /// Called if the AR the session fails.
        /// </summary>
        /// <param name="error">The error.</param>
        private void Interface_OnSessionFailed(string error)
        {
            _messages.Publish(
                MessageTypes.ARSERVICE_EXCEPTION,
                new Exception(error));
        }
        
        /// <summary>
        /// Called when an anchor has been added.
        /// </summary>
        /// <param name="data">The native data.</param>
        private void Interface_OnAnchorAdded(ARPlaneAnchor data)
        {
            var anchor = new ArAnchor(data.identifier)
            {
                Extents = data.extent.ToVec()
            };
            
            UpdateAnchorTransform(anchor, data);
            
            _anchors.Add(anchor);
        }
        
        /// <summary>
        /// Called when an anchor has been updated.
        /// </summary>
        /// <param name="data">The native data.</param>
        private void Interface_OnAnchorUpdated(ARPlaneAnchor data)
        {
            var anchor = Anchor(data.identifier);
            if (null != anchor)
            {
                anchor.Extents = data.extent.ToVec();
                
                UpdateAnchorTransform(anchor, data);
            }
            else
            {
                Log.Warning(this, "Received an anchor update for an anchor we aren't tracking: {0}.", data.identifier);
                
                Interface_OnAnchorAdded(data);
            }
        }
        
        /// <summary>
        /// Called when an anchor has been removed.
        /// </summary>
        /// <param name="data">The native data.</param>
        private void Interface_OnAnchorRemoved(ARPlaneAnchor data)
        {
            var anchor = Anchor(data.identifier);
            if (null != anchor)
            {
                _anchors.Remove(anchor);
            }
        }
        
        /// <summary>
        /// Called by the native interface when tracking is lost.
        /// </summary>
        private void Interface_OnInterrupted()
        {
            if (null != OnTrackingOffline)
            {
                OnTrackingOffline();
            }
        }
        
        /// <summary>
        /// Called by the native interface when tracking is reestablished.
        /// </summary>
        private void Interface_OnInterruptEnded()
        {
            if (null != OnTrackingOnline)
            {
                OnTrackingOnline();
            }
        }
    
        /// <summary>
        /// Finds an anchor by id.
        /// </summary>
        /// <param name="id">Unique identifier for an id.</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Updates an anchor's transform based on the underlying native data.
        /// </summary>
        /// <param name="anchor">The Spire anchor.</param>
        /// <param name="data">Native data.</param>
        private void UpdateAnchorTransform(
            ArAnchor anchor,
            ARPlaneAnchor data)
        {
            // Convert from ARKit's right-handed coordinate system to Unity's left-handed
            
            // position
            var position = ((Vector3) data.transform.GetColumn(3)).ToVec(); 
            position.z = -position.z;
            
            // offset position by center
            anchor.Position = position + new Vec3(
                  data.center.x,
                  data.center.y,
                  -data.center.z);
            
            // set rotation
            var rotation = QuaternionFromMatrix(data.transform).ToQuat();
            rotation.z = -rotation.z;
            rotation.w = -rotation.w;
            anchor.Rotation = rotation;
        }
        
        /// <summary>
        /// Creates a <c>Quaternion</c> from a matrix.
        /// </summary>
        /// <param name="matrix">The input matrix.</param>
        /// <returns></returns>
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

#endif
