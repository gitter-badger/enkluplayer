using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA;

namespace CreateAR.SpirePlayer
{
    public class MeshCaptureApplicationState : IState
    {
        private const string SCENE_NAME = "WorldMeshCaptureMode";
        private const float UPDATE_INTERVAL = 0.2f;

        private SurfaceObserver _observer;

        private bool _isAlive = false;
        private GameObject _root;
        private readonly Dictionary<SurfaceId, GameObject> _surfaces = new Dictionary<SurfaceId, GameObject>();

        private readonly IBootstrapper _bootstrapper;

        public MeshCaptureApplicationState(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public void Enter(object context)
        {
            // load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                SCENE_NAME,
                LoadSceneMode.Additive);

            _root = new GameObject("Surfaces");

            _observer = new SurfaceObserver();
            _observer.SetVolumeAsAxisAlignedBox(Vector3.zero, 100 * Vector3.one);

            _bootstrapper.BootstrapCoroutine(UpdateObserver());
        }

        public void Update(float dt)
        {
            //
        }

        public void Exit()
        {
            _isAlive = false;

            _observer.Dispose();
            _observer = null;

            _surfaces.Clear();
            UnityEngine.Object.Destroy(_root);

            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }

        private IEnumerator UpdateObserver()
        {
            _isAlive = true;

            while (_isAlive)
            {
                _observer.Update(Observer_OnSurfaceChanged);

                yield return new WaitForSecondsRealtime(UPDATE_INTERVAL);
            }
        }

        private void Observer_OnSurfaceChanged(
            SurfaceId surfaceId,
            SurfaceChange changeType,
            Bounds bounds,
            DateTime updateTime)
        {
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

        private void SurfaceUpdated(SurfaceId surfaceId)
        {
            GameObject target;
            if (!_surfaces.TryGetValue(surfaceId, out target))
            {
                target = _surfaces[surfaceId] = new GameObject(surfaceId.ToString());
                target.transform.SetParent(_root.transform);
                target.AddComponent<MeshFilter>();
                target.AddComponent<MeshRenderer>();
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

            _observer.RequestMeshAsync(data, OnDataReady);
        }

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

        private void OnDataReady(
            SurfaceData bakedData,
            bool outputWritten,
            float elapsedBaketimeSeconds)
        {
            //
        }
    }
}