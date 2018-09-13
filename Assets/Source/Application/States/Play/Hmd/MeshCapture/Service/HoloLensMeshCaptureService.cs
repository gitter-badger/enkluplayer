#if !UNITY_EDITOR && UNITY_WSA
using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;
using UnityEngine.XR.WSA;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Implementation for HoloLens.
    /// </summary>
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
        /// Manages intention.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Config.
        /// </summary>
        private readonly MeshCaptureConfig _config;

        /// <summary>
        /// Keeps track of surfaces.
        /// </summary>
        private readonly Dictionary<SurfaceId, GameObject> _surfaces = new Dictionary<SurfaceId, GameObject>();
        
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
        private bool _isObserverAlive;

        /// <summary>
        /// Backing variable for IsVisible.
        /// </summary>
        private bool _isVisible;

        /// <inheritdoc />
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;

                foreach (var gameObject in _surfaces.Values)
                {
                    gameObject.SetActive(_isVisible);
                }
            }
        }

        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <inheritdoc />
        public IMeshCaptureObserver Observer { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensMeshCaptureService(
            IBootstrapper bootstrapper,
            IIntentionManager intention,
            MeshCaptureConfig config)
        {
            _bootstrapper = bootstrapper;
            _intention = intention;
            _config = config;
        }

        /// <inheritdoc />
        public void Start()
        {
            IsRunning = true;

            _root = new GameObject("Mesh Capture Root");

            // setup surface observer
            _surfaceObserver = new SurfaceObserver();
            _surfaceObserver.SetVolumeAsAxisAlignedBox(
                _intention.Origin.ToVector(),
                100000 * Vector3.one);
            _bootstrapper.BootstrapCoroutine(UpdateObserver());

            foreach (var surface in _surfaces.Values)
            {
                surface.SetActive(_isVisible);
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            IsRunning = false;

            // destroy observer
            if (null != _surfaceObserver)
            {
                _isObserverAlive = false;
                _surfaceObserver.Dispose();
                _surfaceObserver = null;
            }

            foreach (var surface in _surfaces.Values)
            {
                surface.SetActive(false);
            }
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
            if (!_isObserverAlive)
            {
                return;
            }

            Log.Info(this, "Surface changed : [{0}, {1}, {2}, {3}]",
                surfaceId,
                changeType,
                bounds,
                updateTime);

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
                target.SetActive(_isVisible);
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
            if (!IsRunning)
            {
                return;
            }

            Observer?.OnData(
                bakedData.id.handle,
                bakedData.outputMesh);
        }
    }
}
#endif