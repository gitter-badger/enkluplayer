using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.XR.WSA;

namespace CreateAR.SpirePlayer
{
    public interface IMeshCaptureObserver
    {
        void OnData(int id, MeshFilter filter);
    }

    public interface IMeshCaptureService
    {
        void Start(IMeshCaptureObserver observer);
        void Stop();
    }

    public class HoloLensMeshCaptureService : IMeshCaptureService
    {
        /// <summary>
        /// How often, in seconds, to update the mesh.
        /// </summary>
        private const float UPDATE_INTERVAL_SECS = 0.2f;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Config.
        /// </summary>
        private readonly MeshCaptureConfig _config;

        /// <summary>
        /// Keeps track of surfaces.
        /// </summary>
        private readonly Dictionary<SurfaceId, GameObject> _surfaces = new Dictionary<SurfaceId, GameObject>();

        /// <summary>
        /// External observer.
        /// </summary>
        private IMeshCaptureObserver _captureObserver;

        /// <summary>
        /// Observes surfaces.
        /// </summary>
        private SurfaceObserver _surfaceObserver;

        /// <summary>
        /// Root transform.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// True iff the observer should be updated.
        /// </summary>
        private bool _isObserverAlive = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensMeshCaptureService(
            IBootstrapper bootstrapper,
            MeshCaptureConfig config)
        {
            _bootstrapper = bootstrapper;
            _config = config;
        }

        /// <inheritdoc />
        public void Start(IMeshCaptureObserver observer)
        {
            _captureObserver = observer;
            _root = new GameObject("Mesh Capture Root");

            // setup surface observer
            _surfaceObserver = new SurfaceObserver();
            _surfaceObserver.SetVolumeAsAxisAlignedBox(
                Vector3.zero,
                1000 * Vector3.one);
            _bootstrapper.BootstrapCoroutine(UpdateObserver());
        }

        /// <inheritdoc />
        public void Stop()
        {
            // destroy observer
            _isObserverAlive = false;
            _surfaceObserver.Dispose();
            _surfaceObserver = null;

            // destroy surfaces
            _surfaces.Clear();
            UnityEngine.Object.Destroy(_root);
        }

        /// <summary>
        /// Coroutine that updates the observer according to an interval.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateObserver()
        {
            _isObserverAlive = true;

            while (_isObserverAlive)
            {
                _surfaceObserver.Update(Observer_OnSurfaceChanged);

                yield return new WaitForSecondsRealtime(UPDATE_INTERVAL_SECS);
            }
        }

        /// <summary>
        /// Called when the observer changes.
        /// </summary>
        private void Observer_OnSurfaceChanged(
            SurfaceId surfaceId,
            SurfaceChange changeType,
            Bounds bounds,
            DateTime updateTime)
        {
            if (_isObserverAlive)
            {
                return;
            }

            switch (changeType)
            {
                case SurfaceChange.Added:
                case SurfaceChange.Updated:
                {
                    SurfaceUpdated(surfaceId);

                    break;
                }
                case SurfaceChange.Removed:
                {
                    SurfaceRemoved(surfaceId);

                    break;
                }
            }
        }

        /// <summary>
        /// Called when a surface has been updated.
        /// </summary>
        /// <param name="surfaceId">Id of the surface.</param>
        private void SurfaceUpdated(SurfaceId surfaceId)
        {
            GameObject target;
            if (!_surfaces.TryGetValue(surfaceId, out target))
            {
                target = _surfaces[surfaceId] = new GameObject(surfaceId.ToString());
                target.transform.SetParent(_root.transform);
                target.AddComponent<MeshFilter>();
                target.AddComponent<MeshRenderer>().sharedMaterial = _config.SurfaceMaterial;
                target.AddComponent<WorldAnchor>();
            }

            var data = new SurfaceData(
                surfaceId,
                target.GetComponent<MeshFilter>(),
                target.GetComponent<WorldAnchor>(),
                null,
                1000,
                false
            );

            _surfaceObserver.RequestMeshAsync(data, SurfaceObserver_OnDataReady);

            //PushToPipeline();
        }

        /// <summary>
        /// Called when a surface has been removed.
        /// </summary>
        /// <param name="surfaceId">Id of the surface.</param>
        private void SurfaceRemoved(SurfaceId surfaceId)
        {
            GameObject target;
            if (!_surfaces.TryGetValue(surfaceId, out target))
            {
                Log.Error(this, "Called to remove a surface we are not currently tracking.");
                return;
            }

            _surfaces.Remove(surfaceId);

            UnityEngine.Object.Destroy(target);
        }

        /// <summary>
        /// Called when data is ready.
        /// </summary>
        private void SurfaceObserver_OnDataReady(
            SurfaceData bakedData,
            bool outputWritten,
            float elapsedBaketimeSeconds)
        {
            _captureObserver.OnData(bakedData.id.handle, bakedData.outputMesh);
        }
    }

    /// <summary>
    /// Application state for capturing the world mesh.
    /// </summary>
    public class MeshCaptureApplicationState : IState, IMeshCaptureObserver
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IVoiceCommandManager _voice;
        private readonly IMessageRouter _messages;
        private readonly WorldScanPipeline _pipeline;

        /// <summary>
        /// Camera settings snapshot.
        /// </summary>
        private CameraSettingsSnapshot _snapshot;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshCaptureApplicationState(
            IVoiceCommandManager voice,
            IMessageRouter messages,
            WorldScanPipeline pipeline)
        {
            _voice = voice;
            _messages = messages;
            _pipeline = pipeline;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            // setup camera
            var camera = Camera.main;
            
            // save snapshot first
            _snapshot = CameraSettingsSnapshot.Snapshot(camera);

            // then set camera settings
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.85f;
            camera.farClipPlane = 1000f;
            camera.transform.position = Vector3.zero;

            // setup voice commands
            if (!_voice.Register(VoiceKeywords.EXIT, Voice_OnExit))
            {
                Log.Error(this, "Could not register exit voice command.");
            }

            // start save pipeline
            _pipeline.Start();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            //
        }

        /// <inheritdoc />
        public void Exit()
        {
            _pipeline.Stop();

            // kill voice commands
            if (!_voice.Unregister(VoiceKeywords.EXIT))
            {
                Log.Error(this, "Could not unregister exit voice command.");
            }
            
            // restore camera snapshot
            CameraSettingsSnapshot.Apply(Camera.main, _snapshot);
        }

        /// <inheritdoc />
        public void OnData(int id, MeshFilter filter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when the voice processor received a save command.
        /// </summary>
        /// <param name="save">The command received.</param>
        private void Voice_OnExit(string save)
        {
            _messages.Publish(MessageTypes.TOOLS);
        }
    }
}