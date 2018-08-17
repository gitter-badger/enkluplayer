using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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

        public static bool AreAllAnchorsReady { get; private set; }

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
        /// Pub/sub.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Configuration for entire application.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Lookup from surface id to GameObject.
        /// </summary>
        private readonly Dictionary<int, SurfaceRecord> _surfaces = new Dictionary<int, SurfaceRecord>();

        /// <summary>
        /// List of anchors in scene.
        /// </summary>
        private readonly List<WorldAnchorWidget> _anchors = new List<WorldAnchorWidget>();

        /// <summary>
        /// Callbacks for ready.
        /// </summary>
        private readonly List<Action> _onReady = new List<Action>();

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

        /// <summary>
        /// Unsubscribe delegate for auto-export event.
        /// </summary>
        private Action _autoExportUnsub;

        /// <summary>
        /// Unsubscribe delegate for reset.
        /// </summary>
        private Action _resetUnsub;

        /// <summary>
        /// UI root.
        /// </summary>
        private Element _rootUI;

        /// <summary>
        /// Caption on UI.
        /// </summary>
        private CaptionWidget _cpn;

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

        /// <inheritdoc />
        public WorldAnchorWidget Anchor
        {
            get { return _primaryAnchor; }
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
            IMessageRouter messages,
            IElementFactory elements,
            ApplicationConfig config)
        {
            _scenes = scenes;
            _txns = txns;
            _intention = intentions;
            _capture = capture;
            _exportService = exportService;
            _bootstrapper = bootstrapper;
            _messages = messages;
            _elements = elements;
            _config = config;
        }

        /// <inheritdoc />
        public void Setup()
        {
            if (UnityEngine.Application.isEditor)
            {
                return;
            }

            if (_config.Play.Edit)
            {
                _autoExportUnsub = _messages.Subscribe(MessageTypes.ANCHOR_AUTOEXPORT, Messages_OnAutoExport);
                _resetUnsub = _messages.Subscribe(MessageTypes.ANCHOR_RESETPRIMARY, Messages_OnResetPrimary);
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

            if (DeviceHelper.IsHoloLens())
            {
                OpenStatusUI();
            }
            
            FindPrimaryAnchor(root);

            if (_config.Play.Edit && DeviceHelper.IsHoloLens())
            {
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

            OnPrimaryLocated(() =>
            {
                // attempt to move unlocated world anchors into place
                var anchors = new List<WorldAnchorWidget>();
                root.Find("..(@type==WorldAnchorWidget)", anchors);

                foreach (var anchor in anchors)
                {
                    if (anchor == _primaryAnchor)
                    {
                        continue;
                    }

                    if (anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated)
                    {
                        continue;
                    }

                    PositionAnchorRelativeToPrimary(anchor);
                }
            });
        }

        /// <inheritdoc />
        public void Teardown()
        {
            CloseStatusUI();

            if (null != _autoExportUnsub)
            {
                _autoExportUnsub();
                _resetUnsub();
            }

            if (null != _primaryAnchor)
            {
                _primaryAnchor.OnLocated -= Primary_OnLocated;
                _primaryAnchor = null;
            }

            if (null != _createToken)
            {
                _createToken.Abort();
            }

            TeardownMeshScan();
        }

        /// <inheritdoc />
        public void OnPrimaryLocated(Action ready)
        {
            if (Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated)
            {
                ready();
            }
            else
            {
                _onReady.Add(ready);
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
            _anchors.Clear();
            root.Find("..(@type=WorldAnchorWidget)", _anchors);

            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
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
                        _primaryAnchor.OnLocated += Primary_OnLocated;
                    }
                }
                else if (anchor.Schema.GetOwn("autoexport", false).Value)
                {
                    Messages_OnAutoExport(anchor);
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
                        Vectors = { { "position", position }, { "rotation", rotation } },
                        Bools = { { "locked", true } }
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
                    var anchor = (WorldAnchorWidget) response.Elements[0];

                    // on hololens, position + rotation don't do anything-- so set the transform by hand
                    anchor.GameObject.transform.position = position.ToVector();
                    anchor.GameObject.transform.rotation = Quaternion.Euler(rotation.ToVector());

                    _primaryAnchor = anchor;
                    _primaryAnchor.OnLocated += Primary_OnLocated;
                    _scan = (ScanWidget) anchor.Children[0];

                    SaveAnchor(_primaryAnchor);
                    SetupMeshScan(true);
                })
                .OnFailure(exception => Log.Error(this, "Could not create primary anchor : {0}", exception));
        }

        /// <summary>
        /// Opens the status UI.
        /// </summary>
        private void OpenStatusUI()
        {
            _rootUI = _elements.Element(@"
<?Vine>
<Screen distance=3.8>
    <Caption id='cpn' position=(0, 0.4, 0) label='Locating anchors.' width=1200.0 alignment='MidCenter' fontSize=100 />
</Screen>");
            _cpn = _rootUI.FindOne<CaptionWidget>("..cpn");

            _bootstrapper.BootstrapCoroutine(UpdateStatusUI());
        }

        /// <summary>
        /// Updates the status UI every frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateStatusUI()
        {
            AreAllAnchorsReady = false;

            if (null == _primaryAnchor)
            {
                AreAllAnchorsReady = true;
                CloseStatusUI();
                yield break;
            }

            while (null != _cpn)
            {
                if (Status != WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated)
                {
                    switch (Status)
                    {
                        case WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated:
                        {
                            _cpn.Label = "Primary anchor loaded and imported but not locating... Are you sure you're in the right space?";
                            break;
                        }
                        case WorldAnchorWidget.WorldAnchorStatus.IsError:
                        {
                            _cpn.Label = "Primary anchor is in an error state. Try reloading the experience.";
                            break;
                        }
                        case WorldAnchorWidget.WorldAnchorStatus.IsImporting:
                        {
                            _cpn.Label = "Primary anchor is importing.";
                            break;
                        }
                    }
                }
                else
                {
                    var count = _anchors.Count(anchor => anchor.Status != WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated && anchor.Status != WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated);
                    if (0 == count)
                    {
                        AreAllAnchorsReady = true;
                        CloseStatusUI();
                    }
                    else
                    {
                        if (_anchors.Any(anchor => anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsError))
                        {
                            _cpn.Label = "One of the anchors is in an error state.";
                        }
                        else
                        {
                            _cpn.Label = string.Format("Importing {0} anchor(s).", count);
                        }
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Closes the status UI.
        /// </summary>
        private void CloseStatusUI()
        {
            if (null != _rootUI)
            {
                _rootUI.Destroy();
                _rootUI = null;
                _cpn = null;
            }
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

            if (null != _capture)
            {
                _capture.Stop();
            }

            if (null != _exportService)
            {
                _exportService.OnFileUrlChanged -= ExportService_OnFileUrlChanged;
                _exportService.OnFileCreated -= ExportService_OnFileCreated;
                _exportService.Stop();
                _surfaces.Clear();
            }
        }

        /// <summary>
        /// Passes mesh surfaces to exporter.
        /// </summary>
        private IEnumerator ExportMeshScan()
        {
            _isAutoExportAlive = true;

            while (_isAutoExportAlive)
            {
                if (_capture.IsRunning)
                {
                    int tris;
                    if (!_exportService.Export(
                        out tris,
                        _surfaces.Values.Select(record => record.GameObject).ToArray()))
                    {
                        Log.Error(this, "Could not export!");
                    }
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
        /// Moves an anchor to where it should be relative to the primary.
        /// </summary>
        /// <param name="anchor">The anchor in question.</param>
        private void PositionAnchorRelativeToPrimary(WorldAnchorWidget anchor)
        {
            if (null == anchor || !anchor.GameObject || null == _primaryAnchor || !_primaryAnchor.GameObject)
            {
                return;
            }

            var anchorSchemaPos = anchor.Schema.Get<Vec3>("position").Value.ToVector();
            var anchorSchemaEul = anchor.Schema.Get<Vec3>("rotation").Value.ToVector();

            var primarySchemaPos = _primaryAnchor.Schema.Get<Vec3>("position").Value.ToVector();
            var primarySchemaEul = _primaryAnchor.Schema.Get<Vec3>("rotation").Value.ToVector();

            var primaryTransformQuat = _primaryAnchor.GameObject.transform.rotation;

            var localToWorld = _primaryAnchor.GameObject.transform.localToWorldMatrix;
            anchor.GameObject.transform.position = localToWorld.MultiplyPoint3x4(anchorSchemaPos - primarySchemaPos);
            anchor.GameObject.transform.rotation = Quaternion.Euler(anchorSchemaEul) *
                                                   Quaternion.Inverse(Quaternion.Euler(primarySchemaEul)) *
                                                   primaryTransformQuat;
        }

        /// <summary>
        /// Called when the primary anchor has been located.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        private void Primary_OnLocated(WorldAnchorWidget anchor)
        {
            var temp = _onReady.ToArray();
            _onReady.Clear();

            for (int i = 0, len = temp.Length; i < len; i++)
            {
                temp[i]();
            }
        }

        /// <summary>
        /// Called when a widget wants to auto-export.
        /// </summary>
        /// <param name="object">Message.</param>
        private void Messages_OnAutoExport(object @object)
        {
            Log.Info(this, "AutoExport requested. Waiting for primary to be located.");

            var anchor = (WorldAnchorWidget) @object;

            OnPrimaryLocated(() =>
            {
                Log.Info(this, "Primary is located. Positioning AutoExport anchor.");

                // position relative to primary
                PositionAnchorRelativeToPrimary(anchor);

                // export in this new position
                anchor.Export(_config.Play.AppId, _sceneId, _txns);
            });
        }

        /// <summary>
        /// Called when the primary anchor should be destroyed and reset.
        /// </summary>
        private void Messages_OnResetPrimary(object _)
        {
            Log.Info(this, "Reset primary anchor requested.");

            if (null == _primaryAnchor)
            {
                Log.Warning(this, "No primary anchor to destroy.");
                return;
            }

            TeardownMeshScan();

            // destroy primary anchor
            _txns
                .Request(new ElementTxn(_sceneId).Delete(_primaryAnchor.Id))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Destroyed primary anchor. Recreating.");

                    CreatePrimaryAnchor(_sceneId, _scenes.Root(_sceneId));
                });
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