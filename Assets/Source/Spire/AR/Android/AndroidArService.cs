#if UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Commons.Unity.Http;
using GoogleARCore;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Android AR implementation
    /// </summary>
    public class AndroidArService : IArService
    {
        /// <inheritdoc />
        public List<ArAnchor> Anchors { get { return _anchorList; } }

        /// <inheritdoc />
        public ArServiceConfiguration Config { get; private set; }

        /// <inheritdoc />
        public bool IsSetup { get; private set; }

        /// <inheritdoc />
        public event Action OnTrackingOffline;

        /// <inheritdoc />
        public event Action OnTrackingOnline;

        /// <summary>
        /// A list that ARCore will use to supply trackables
        /// </summary>
        private readonly List<DetectedPlane> _tempPlanes = new List<DetectedPlane>();

        /// <summary>
        /// List allocated to ensure collection stability during iteration through trackables that need removing
        /// </summary>
        private readonly List<DetectedPlane> _removeList = new List<DetectedPlane>();

        /// <summary>
        /// Map between ARCore planes and anchors to allow easy updating of moved anchors
        /// </summary>
        private readonly Dictionary<DetectedPlane, ArAnchor> _anchorMap = new Dictionary<DetectedPlane, ArAnchor>();

        /// <summary>
        /// Parallel list of anchors
        /// </summary>
        private readonly List<ArAnchor> _anchorList = new List<ArAnchor>();

        /// <summary>
        /// Message routing
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Used for monobehaviour-like lifecycle
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Session state tracking
        /// </summary>
        private SessionStatus _oldStatus;

        /// <summary>
        /// The camera rig that ARCore will drive
        /// </summary>
        private ArCameraRig _rig;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messages">Message system</param>
        /// <param name="bootstrapper">Bootstrapper for retrieving update loop</param>
        public AndroidArService(IMessageRouter messages, IBootstrapper bootstrapper)
        {
            _messages = messages;
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public void Setup(ArServiceConfiguration config)
        {
            Config = config;

            Log.Error(this, "Setting up android camera");

            _rig = Config.Rig;
            var camGO = _rig.Camera.gameObject;
            camGO.AddComponent<ARCoreSession>();
            camGO.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();

            if (Config.ShowCameraFeed)
            {
                var bgRenderer = camGO.AddComponent<ARCoreBackgroundRenderer>();
                bgRenderer.BackgroundMaterial = Config.CameraMaterial;
            }

            IsSetup = true;
            _bootstrapper.BootstrapCoroutine(UpdateLoop());
        }

        /// <inheritdoc />
        public void Teardown()
        {
            IsSetup = false;

            Log.Info(this, "Tearing down android camera");

            if (_rig == null || _rig.Camera == null)
            {
                return;
            }

            var camGO = _rig.Camera.gameObject;
            DestroyIfNotNull(camGO.GetComponent<ARCoreSession>());
            DestroyIfNotNull(camGO.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>());
            DestroyIfNotNull(camGO.GetComponent<ARCoreBackgroundRenderer>());
        }

        /// <summary>
        /// Called on update to gauge trackable and session status changes
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateLoop()
        {
            while (IsSetup)
            {
                UpdateSessionState();
                UpdateTrackables();
                yield return null;
            }
        }

        /// <summary>
        /// Track our session state and emit errors as they are encountered
        /// </summary>
        private void UpdateSessionState()
        {
            var status = Session.Status;
            if (status != _oldStatus)
            {
                if (status == SessionStatus.Tracking && OnTrackingOnline != null)
                {
                    OnTrackingOnline();
                }
                else if (status != SessionStatus.Tracking && OnTrackingOffline != null)
                {
                    OnTrackingOffline();
                }

                if (status == SessionStatus.ErrorApkNotAvailable
                    || status == SessionStatus.ErrorPermissionNotGranted
                    || status == SessionStatus.ErrorSessionConfigurationNotSupported
                    || status == SessionStatus.FatalError)
                {
                    Log.Info(this, "ARCore session failed");

                    _messages.Publish(
                        MessageTypes.ARSERVICE_EXCEPTION,
                        new Exception(status.ToString()));
                }
                _oldStatus = status;
            }
        }

        /// <summary>
        /// Update our list of trackables. Manually ascertain which trackables have been removed.
        /// </summary>
        private void UpdateTrackables()
        {            
            //Generate new anchors in mapping for all new planes
            Session.GetTrackables<DetectedPlane>(_tempPlanes, TrackableQueryFilter.New);
            for (int i = 0; i < _tempPlanes.Count; i++)
            {
                var plane = _tempPlanes[i];
                var anchor = GetAnchor(plane);
                _anchorMap.Add(plane, anchor);
                _anchorList.Add(anchor);
            }

            //Update all old anchors
            Session.GetTrackables<DetectedPlane>(_tempPlanes, TrackableQueryFilter.Updated);
            for (int i = 0; i < _tempPlanes.Count; i++)
            {
                var plane = _tempPlanes[i];
                var anchor = _anchorMap[plane];
                anchor.Position = plane.CenterPose.position.ToVec();
                anchor.Rotation = plane.CenterPose.rotation.ToQuat();
                anchor.Extents = new Vec3(plane.ExtentX, 0.1f, plane.ExtentZ);
            }

            //Get any anchors remaining in the plane set that are no longer reported by ARCore
            //and remove them from the anchor map
            Session.GetTrackables<DetectedPlane>(_tempPlanes, TrackableQueryFilter.All);
            for (var enumerator = _anchorMap.GetEnumerator(); enumerator.MoveNext();)
            {
                if (!_tempPlanes.Contains(enumerator.Current.Key)) _removeList.Add(enumerator.Current.Key);
            }
            for (int i = 0; i < _removeList.Count; i++)
            {
                var anchor = _anchorMap[_removeList[i]];
                _anchorMap.Remove(_removeList[i]);
                _anchorList.Remove(anchor);
            }
            _removeList.Clear();
        }

        /// <summary>
        /// Helper method to retrieve an anchor with position and extents applied
        /// </summary>
        /// <param name="plane">The ARCore plane on which to base our anchor</param>
        /// <returns>A new anchor with accurate data</returns>
        private ArAnchor GetAnchor(DetectedPlane plane)
        {
            var anchor = new ArAnchor(plane.m_TrackableNativeHandle.ToString());
            anchor.Position = plane.CenterPose.position.ToVec();
            anchor.Rotation = plane.CenterPose.rotation.ToQuat();
            anchor.Extents = new Vec3(plane.ExtentX, 0.1f, plane.ExtentZ);
            return anchor;
        }

        /// <summary>
        /// Helper method to destroy objects
        /// </summary>
        /// <param name="obj">The object to destroy</param>
        private void DestroyIfNotNull(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }
    }
}

#endif
