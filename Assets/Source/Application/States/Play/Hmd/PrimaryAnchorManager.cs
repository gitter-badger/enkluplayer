using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic implementation of the primary anchor manager.
    /// </summary>
    public class PrimaryAnchorManager : IPrimaryAnchorManager, IMeshCaptureObserver
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
            /// Constructor.
            /// </summary>
            public SurfaceRecord(GameObject gameObject)
            {
                GameObject = gameObject;
            }
        }

        /// <summary>
        /// K/V for the tag prop.
        /// </summary>
        public const string PROP_TAG_KEY = "tag";
        public const string PROP_TAG_VALUE = "primary";

        /// <summary>
        /// Time between exports.
        /// </summary>
        private const float EXPORT_POLL_SEC = 1f;

        /// <summary>
        /// Manages the scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Makes changes to the scene.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Manages intentions.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Captures surfaces.
        /// </summary>
        private readonly IMeshCaptureService _capture;

        /// <summary>
        /// Exports captured surfaces to Trellis.
        /// </summary>
        private readonly IMeshCaptureExportService _exportService;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Configuration for entire application.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Lookup from surface id to GameObject.
        /// </summary>
        private readonly Dictionary<int, SurfaceRecord> _surfaces = new Dictionary<int, SurfaceRecord>();
        
        /// <summary>
        /// The primary anchor.
        /// </summary>
        private WorldAnchorWidget _primaryAnchor;

        /// <summary>
        /// Immediate child of anchor.
        /// </summary>
        private ScanWidget _scan;

        /// <summary>
        /// Token used to create the primary anchor.
        /// </summary>
        private IAsyncToken<ElementResponse> _createToken;

        /// <summary>
        /// Id of the scene.
        /// </summary>
        private string _sceneId;

        /// <summary>
        /// True iff autoexport coroutine should be running.
        /// </summary>
        private bool _isAutoExportAlive;

        /// <inheritdoc />
        public WorldAnchorWidget.WorldAnchorStatus Status
        {
            get
            {
                if (null == _primaryAnchor)
                {
                    return WorldAnchorWidget.WorldAnchorStatus.None;
                }

                return _primaryAnchor.Status;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PrimaryAnchorManager(
            IAppSceneManager scenes,
            IElementTxnManager txns,
            IIntentionManager intentions,
            IMeshCaptureService capture,
            IMeshCaptureExportService exportService,
            IBootstrapper bootstrapper,
            ApplicationConfig config)
        {
            _scenes = scenes;
            _txns = txns;
            _intention = intentions;
            _capture = capture;
            _exportService = exportService;
            _bootstrapper = bootstrapper;
            _config = config;
        }

        /// <inheritdoc />
        public void Setup()
        {
            if (UnityEngine.Application.isEditor)
            {
                return;
            }

            _sceneId = _scenes.All.FirstOrDefault();
            if (string.IsNullOrEmpty(_sceneId))
            {
                Log.Warning(this, "Cannot setup PrimaryAnchorManager: no tracked scenes.");
                return;
            }

            var root = _scenes.Root(_sceneId);
            if (null == root)
            {
                Log.Warning(this, "Cannot setup PrimaryAnchorManager: could not find scene root.");
                return;
            }
            
            FindPrimaryAnchor(root);

            if (null == _primaryAnchor)
            {
                Log.Info(this, "No primary anchor found. Will create one!");

                CreatePrimaryAnchor(_sceneId, root);
            }
            else
            {
                Log.Info(this, "Primary anchor found.");

                SetupMeshScan(false);
            }
        }
        
        /// <inheritdoc />
        public void Teardown()
        {
            _primaryAnchor = null;

            if (null != _createToken)
            {
                _createToken.Abort();
            }

            TeardownMeshScan();
        }

        /// <inheritdoc />
        public void CalculateOffsets(
            Vec3 position,
            Vec3 eulerAngles,
            Action<Vec3, Vec3> callback)
        {
            Action<WorldAnchorWidget> calculate = null;
            calculate = anchor =>
            {
                anchor.OnLocated -= calculate;

                // calculate diffs
                var primaryTransform = _primaryAnchor.GameObject.transform;

                callback(
                    (position.ToVector() - primaryTransform.position).ToVec(),
                    (Quaternion.Euler(eulerAngles.ToVector()) * Quaternion.Inverse(primaryTransform.rotation)).eulerAngles.ToVec());
            };

            // wait for it to be located
            if (Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated)
            {
                calculate(_primaryAnchor);
            }
            else
            {
                _primaryAnchor.OnLocated += calculate;
            }
        }

        /// <inheritdoc />
        public void OnData(int id, MeshFilter filter)
        {
            if (!_surfaces.ContainsKey(id))
            {
                _surfaces[id] = new SurfaceRecord(filter.gameObject);
            }
        }

        /// <summary>
        /// Locates the primary anchor in a hierarchy.
        /// </summary>
        /// <param name="root">The root element.</param>
        private void FindPrimaryAnchor(Element root)
        {
            var anchors = new List<WorldAnchorWidget>();
            root.Find("..(@type=WorldAnchorWidget)", anchors);

            for (int i = 0, len = anchors.Count; i < len; i++)
            {
                var anchor = anchors[i];
                if (PROP_TAG_VALUE == anchor.Schema.Get<string>(PROP_TAG_KEY).Value)
                {
                    if (null != _primaryAnchor)
                    {
                        Log.Error(this, "Found multiple primary anchors! Choosing first by id.");

                        // compare id so we at least pick the same primary each time
                        if (string.Compare(
                                _primaryAnchor.Id,
                                anchor.Id,
                                StringComparison.Ordinal) < 0)
                        {
                            _primaryAnchor = anchor;
                        }
                    }
                    else
                    {
                        _primaryAnchor = anchor;
                    }
                }
            }

            if (null != _primaryAnchor)
            {
                _scan = (ScanWidget) _primaryAnchor.Children[0];
            }
        }

        /// <summary>
        /// Creates primary anchor.
        /// </summary>
        /// <param name="sceneId">The scene in which to create the anchor.</param>
        /// <param name="root">The root of the scene.</param>
        private void CreatePrimaryAnchor(string sceneId, Element root)
        {
            var position = _intention.Origin + 2 * _intention.Forward;
            var rotation = new Vector3(
                _intention.Forward.x,
                0,
                _intention.Forward.z).normalized.ToVec();

            _createToken = _txns
                .Request(new ElementTxn(sceneId).Create(root.Id, new ElementData
                {
                    Type = ElementTypes.WORLD_ANCHOR,
                    Schema = new ElementSchemaData
                    {
                        Strings = { { PROP_TAG_KEY, PROP_TAG_VALUE }, { "name", "Primary Anchor" } },
                        Vectors = { { "position", position }, { "rotation", rotation } }
                    },
                    Children = new[]
                    {
                        new ElementData
                        {
                            Type = ElementTypes.SCAN,
                            Schema = new ElementSchemaData
                            {
                                Strings = { { "name", "Scan" } }
                            }
                        }
                    }
                }))
                .OnSuccess(response =>
                {
                    var anchor = response.Elements.FirstOrDefault() as WorldAnchorWidget;
                    if (null == anchor)
                    {
                        Log.Error(this, "Scene txn successful but no WorldAnchorWidget was created.");
                    }
                    else
                    {
                        _primaryAnchor = anchor;
                        _scan = (ScanWidget) anchor.Children[0];
                        
                        SaveAnchor(_primaryAnchor);
                        SetupMeshScan(true);
                    }
                })
                .OnFailure(exception => Log.Error(this, "Could not create primary anchor : {0}", exception));
        }

        /// <summary>
        /// Starts scanning the room and uploading through pipeline.
        /// </summary>
        private void SetupMeshScan(bool start)
        {
            Log.Info(this, "Setting up mesh scan.");

            _capture.Observer = this;

            if (start)
            {
                Log.Info(this, "Starting capture.");

                _capture.IsVisible = true;
                _capture.Start();
            }
            else
            {
                Log.Info(this, "Capture not started.");
            }

            var srcUrl = _scan.Schema.Get<string>("srcId").Value;
            _exportService.OnFileUrlChanged += ExportService_OnFileUrlChanged;
            _exportService.OnFileCreated += ExportService_OnFileCreated;
            _exportService.Start(_config.Play.AppId, srcUrl);

            // start long-running poll for export
            _bootstrapper.BootstrapCoroutine(ExportMeshScan());
        }

        /// <summary>
        /// Stops scanning the room.
        /// </summary>
        private void TeardownMeshScan()
        {
            Log.Info(this, "Tearing down mesh scan.");

            _isAutoExportAlive = false;

            _capture.Stop();
            _exportService.OnFileUrlChanged -= ExportService_OnFileUrlChanged;
            _exportService.OnFileCreated -= ExportService_OnFileCreated;
            _exportService.Stop();
            _surfaces.Clear();
        }

        /// <summary>
        /// Passes mesh surfaces to exporter.
        /// </summary>
        private IEnumerator ExportMeshScan()
        {
            _isAutoExportAlive = true;

            while (_isAutoExportAlive)
            {
                int tris;
                if (!_exportService.Export(
                    out tris,
                    _surfaces.Values.Select(record => record.GameObject).ToArray()))
                {
                    Log.Error(this, "Could not export!");
                }

                yield return new WaitForSecondsRealtime(EXPORT_POLL_SEC);
            }
        }

        /// <summary>
        /// Saves the primary anchor.
        /// </summary>
        private void SaveAnchor(WorldAnchorWidget anchor)
        {
            // export
            Log.Info(this, "Beginning export of primary anchor.");

            anchor.Export(_config.Play.AppId, _sceneId, _txns);
        }
        
        /// <summary>
        /// Called when a file is initially created.
        /// </summary>
        /// <param name="id">The id of the file.</param>
        private void ExportService_OnFileCreated(string id)
        {
            _txns
                .Request(new ElementTxn(_sceneId).Update(_scan.Id, "srcId", id))
                .OnFailure(ex => Log.Error(this, "Could not set file id of scan element : {0}", ex));
        }

        /// <summary>
        /// Called when a file URL is updated.
        /// </summary>
        /// <param name="url">The url of the file.</param>
        private void ExportService_OnFileUrlChanged(string url)
        {
            _txns
                .Request(new ElementTxn(_sceneId).Update(_scan.Id, "srcUrl", url))
                .OnFailure(ex => Log.Error(this, "Could not set url of scan element : {0}", ex));
        }
    }
}