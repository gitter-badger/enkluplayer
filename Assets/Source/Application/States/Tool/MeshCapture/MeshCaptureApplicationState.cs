using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state for capturing the world mesh.
    /// </summary>
    public class MeshCaptureApplicationState : IState, IMeshCaptureObserver
    {
        /// <summary>
        /// Tracks information about a surface.
        /// </summary>
        private class SurfaceRecord
        {
            /// <summary>
            /// The associated GameObject.
            /// </summary>
            public readonly GameObject GameObject;

            /// <summary>
            /// The Mesh filter.
            /// </summary>
            public readonly MeshFilter Filter;
            
            /// <summary>
            /// Constructor.
            /// </summary>
            public SurfaceRecord(MeshFilter filter)
            {
                GameObject = filter.gameObject;
                Filter = filter;
            }
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IUIManager _ui;
        private readonly IMessageRouter _messages;
        private readonly IMeshCaptureService _capture;
        private readonly MeshCaptureExportService _exportService;

        /// <summary>
        /// Lookup from surface id to GameObject.
        /// </summary>
        private readonly Dictionary<int, SurfaceRecord> _surfaces = new Dictionary<int, SurfaceRecord>();

        /// <summary>
        /// Tracks surfaces that have changed.
        /// </summary>
        private readonly HashSet<int> _dirtySurfaces = new HashSet<int>();

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Camera settings snapshot.
        /// </summary>
        private CameraSettingsSnapshot _snapshot;

        /// <summary>
        /// Splash view.
        /// </summary>
        private MeshCaptureSplashUIView _view;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshCaptureApplicationState(
            IUIManager ui,
            IMessageRouter messages,
            IMeshCaptureService capture,
            MeshCaptureExportService exportService)
        {
            _ui = ui;
            _messages = messages;
            _capture = capture;
            _exportService = exportService;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // setup camera
            var camera = Camera.main;
            
            // save snapshot first
            _snapshot = CameraSettingsSnapshot.Snapshot(camera);

            // then set camera settings
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.15f;
            camera.farClipPlane = 1000f;
            camera.transform.position = Vector3.zero;
            
            // start capture
            _capture.Start(this);

            // start export pipeline
            _exportService.Start();

            _ui.Open<MeshCaptureSplashUIView>(new UIReference
                {
                    UIDataId = "MeshCapture.Splash"
                })
                .OnSuccess(el =>
                {
                    _view = el;

                    _view.OnBack += MeshCapture_OnBack;
                    _view.OnSave += MeshCapture_OnSave;
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open MeshCapture.Splash UI : {0}", exception);

                    _messages.Publish(MessageTypes.USER_PROFILE);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            var meshes = 0;
            var verts = 0;
            foreach (var pair in _surfaces)
            {
                meshes++;

                if (null != pair.Value
                    && null != pair.Value.Filter
                    && null != pair.Value.Filter.sharedMesh)
                {
                    verts += pair.Value.Filter.sharedMesh.vertexCount;
                }
            }

            _view.UpdateStats(meshes, verts);
        }

        /// <inheritdoc />
        public void Exit()
        {
            _capture.Stop();
            _exportService.Stop();
            
            _surfaces.Clear();
            _frame.Release();

            // restore camera snapshot
            CameraSettingsSnapshot.Apply(Camera.main, _snapshot);
        }

        /// <inheritdoc />
        public void OnData(int id, MeshFilter filter)
        {
            if (!_surfaces.ContainsKey(id))
            {
                _surfaces[id] = new SurfaceRecord(filter);
            }
            
            _dirtySurfaces.Add(id);
        }

        /// <summary>
        /// Returns to User Profile.
        /// </summary>
        private void MeshCapture_OnBack()
        {
            _messages.Publish(MessageTypes.USER_PROFILE);
        }

        /// <summary>
        /// Called to save data.
        /// </summary>
        private void MeshCapture_OnSave()
        {
            if (_dirtySurfaces.Count == 0)
            {
                return;
            }

            _dirtySurfaces.Clear();

            var dirty = _surfaces
                .Select(pair => _surfaces[pair.Key].GameObject)
                .ToArray();

            int tris;
            if (!_exportService.Export(out tris, dirty))
            {
                Log.Error(this, "Could not export!");
            }
        }
    }
}