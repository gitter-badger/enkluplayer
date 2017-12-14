using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.WSA;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state for capturing the world mesh.
    /// </summary>
    public class MeshCaptureApplicationState : IState
    {
        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        private const string SCENE_NAME = "WorldMeshCaptureMode";

        /// <summary>
        /// How often, in seconds, to update the mesh.
        /// </summary>
        private const float UPDATE_INTERVAL_SECS = 0.2f;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Keeps track of surfaces.
        /// </summary>
        private readonly Dictionary<SurfaceId, GameObject> _surfaces = new Dictionary<SurfaceId, GameObject>();

        /// <summary>
        /// Observes surfaces.
        /// </summary>
        private SurfaceObserver _observer;

        /// <summary>
        /// True iff the observer should be updated.
        /// </summary>
        private bool _isObserverAlive = false;

        /// <summary>
        /// Root transform.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// Camera settings snapshot.
        /// </summary>
        private CameraSettingsSnapshot _snapshot;

        /// <summary>
        /// Config.
        /// </summary>
        private MeshCaptureConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshCaptureApplicationState(
            IBootstrapper bootstrapper,
            IVoiceCommandManager voice)
        {
            _bootstrapper = bootstrapper;
            _voice = voice;
        }

        /// <inheritdoc cref="IState"/>
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
            
            // load scene
            _bootstrapper.BootstrapCoroutine(LoadScene());
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            //
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _config = null;

            // kill voice commands
            if (!_voice.Unregister(VoiceKeywords.SAVE))
            {
                Log.Error(this, "Could not unregister save voice command.");
            }

            // destroy observer
            _isObserverAlive = false;
            _observer.Dispose();
            _observer = null;

            // destroy surfaces
            _surfaces.Clear();
            UnityEngine.Object.Destroy(_root);

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));

            // restore camera snapshot
            CameraSettingsSnapshot.Apply(Camera.main, _snapshot);
        }

        /// <summary>
        /// Loads the scene asynchronously.
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadScene()
        {
            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
                SCENE_NAME,
                LoadSceneMode.Additive);

            yield return op;

            // find config
            _config = UnityEngine.Object.FindObjectOfType<MeshCaptureConfig>();
            if (null == _config)
            {
                throw new Exception("No MeshCaptureConfig found!");
            }

            // create root for surfaces
            _root = new GameObject("Surfaces");

            // setup surface observer
            _observer = new SurfaceObserver();
            _observer.SetVolumeAsAxisAlignedBox(Vector3.zero, 1000 * Vector3.one);
            _bootstrapper.BootstrapCoroutine(UpdateObserver());

            // setup voice commands
            if (!_voice.Register(VoiceKeywords.SAVE, Voice_OnSave))
            {
                Log.Error(this, "Could not register save voice command.");
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
                _observer.Update(Observer_OnSurfaceChanged);

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

            _observer.RequestMeshAsync(data, SurfaceObserver_OnDataReady);
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
            //
        }

        /// <summary>
        /// Called when the voice processor received a save command.
        /// </summary>
        /// <param name="save">The command received.</param>
        private void Voice_OnSave(string save)
        {
            Log.Info(this, "Exporting...");

            var obj = new ObjExporter().Export(_surfaces.Values.ToArray());
            var path = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                string.Format("{0}_Export.obj", DateTime.Now.Ticks));

            Log.Info(this, "Saving to {0}...", path);

            using (var stream = File.OpenWrite(path))
            {
                var bytes = Encoding.UTF8.GetBytes(obj);
                stream.Write(bytes, 0, bytes.Length);
            }

            Log.Info(this, "Save complete.");
        }
    }
}