using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using RLD;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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
        public const string PROP_ENABLED_KEY = "worldanchor.enabled";

        /// <summary>
        /// True iff all anchors are ready to go.
        /// </summary>
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
        /// Voices....
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Configuration for entire application.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Lookup from surface id to GameObject.
        /// </summary>
        private readonly Dictionary<int, SurfaceRecord> _surfaces = new Dictionary<int, SurfaceRecord>();

        /// <summary>
        /// List of anchors in scene, including the primary anchor.
        /// </summary>
        private readonly List<WorldAnchorWidget> _anchors = new List<WorldAnchorWidget>();

        /// <summary>
        /// Callbacks for ready.
        /// </summary>
        private readonly List<Action> _onReady = new List<Action>();

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

        /// <summary>
        /// True iff world anchors are enabled.
        /// </summary>
        private bool _isAnchoringEnabled;

        /// <summary>
        /// True iff anchors are enabled.
        /// </summary>
        private ElementSchemaProp<bool> _anchorsEnabledProp;

        /// <summary>
        /// True iff we're bypassing anchor requirements.
        /// </summary>
        private bool _isBypass;

        /// <summary>
        /// The bypass button.
        /// </summary>
        private ButtonWidget _bypassBtn;

        /// <summary>
        /// True iff anchors are enabled.
        /// </summary>
        private bool _pollAnchors;

        /// <inheritdoc />
        public WorldAnchorWidget.WorldAnchorStatus Status
        {
            get
            {
                if (_isBypass)
                {
                    return WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated;
                }

                if (null != _anchorsEnabledProp && !_anchorsEnabledProp.Value)
                {
                    return WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated;
                }

                if (null == Anchor)
                {
                    return WorldAnchorWidget.WorldAnchorStatus.None;
                }

                return Anchor.Status;
            }
        }

        /// <inheritdoc />
        public WorldAnchorWidget Anchor { get; private set; }

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
            IVoiceCommandManager voice,
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
            _voice = voice;
            _config = config;
        }

        /// <inheritdoc />
        public void Setup()
        {
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

            // reset anchor bypass
            _isBypass = false;

            // reset flag
            AreAllAnchorsReady = false;

            // reset anchors
            _anchors.Clear();

            // see if we need to use anchors
            _anchorsEnabledProp = root.Schema.GetOwn(PROP_ENABLED_KEY, false);
            _anchorsEnabledProp.OnChanged += Anchors_OnEnabledChanged;

            if (_anchorsEnabledProp.Value)
            {
                SetupAnchors();
            }
            else
            {
                Ready();
            }

            // origin command
            _voice.Register("origin", str =>
            {
#if NETFX_CORE
                //if (_isBypass || null == Anchor)
                {
                    UnityEngine.XR.InputTracking.Recenter();
                }
#endif
            });
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _voice.Unregister("origin");

            TeardownAnchors();

            if (null != _anchorsEnabledProp)
            {
                _anchorsEnabledProp.OnChanged -= Anchors_OnEnabledChanged;
                _anchorsEnabledProp = null;
            }

            if (null != Anchor)
            {
                Anchor.OnLocated -= Primary_OnLocated;
                Anchor = null;
            }

            if (null != _createToken)
            {
                _createToken.Abort();
            }
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
            if (!_surfaces.ContainsKey(id) && filter && filter.gameObject)
            {
                _surfaces[id] = new SurfaceRecord(filter.gameObject);
            }
        }

        /// <summary>
        /// Called when ready methods should be called.
        /// </summary>
        private void Ready()
        {
            AreAllAnchorsReady = true;

            var temp = _onReady.ToArray();
            _onReady.Clear();

            for (int i = 0, len = temp.Length; i < len; i++)
            {
                temp[i]();
            }
        }

        /// <summary>
        /// Tears down anchors.
        /// </summary>
        private void TeardownAnchors()
        {
            _pollAnchors = false;
            CloseStatusUI();

            if (null != _createToken)
            {
                _createToken.Abort();
                _createToken = null;
            }

            if (null != _autoExportUnsub)
            {
                _autoExportUnsub();
                _autoExportUnsub = null;
            }

            if (null != _resetUnsub)
            {
                _resetUnsub();
                _resetUnsub = null;
            }

            TeardownMeshScan();
        }

        /// <summary>
        /// Sets up anchors.
        /// </summary>
        private void SetupAnchors()
        {
            if (!DeviceHelper.IsHoloLens())
            {
                Ready();

                return;
            }
            
            var root = _scenes.Root(_sceneId);
            FindPrimaryAnchor(root);
            
            // poll for anchors
            _pollAnchors = true;
            _bootstrapper.BootstrapCoroutine(PollAnchors());

            if (_config.Play.Edit)
            {
                // in edit mode, create a primary anchor
                if (null == Anchor)
                {
                    Log.Info(this, "No primary anchor found. Will create one!");

                    CreatePrimaryAnchor(_sceneId, root);
                }
                // or don't!
                else
                {
                    Log.Info(this, "Primary anchor found.");

                    SetupMeshScan(false);
                }

                // subscribe for export and reset
                _autoExportUnsub = _messages.Subscribe(
                    MessageTypes.ANCHOR_AUTOEXPORT,
                    Messages_OnAutoExport);
                _resetUnsub = _messages.Subscribe(
                    MessageTypes.ANCHOR_RESETPRIMARY,
                    Messages_OnResetPrimary);
            }
            else
            {
                OpenStatusUI();
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
                    if (null != Anchor)
                    {
                        Log.Error(this, "Found multiple primary anchors! Choosing first by id.");

                        // compare id so we at least pick the same primary each time
                        if (string.Compare(
                                Anchor.Id,
                                anchor.Id,
                                StringComparison.Ordinal) < 0)
                        {
                            Anchor = anchor;
                        }
                    }
                    else
                    {
                        Anchor = anchor;
                        Anchor.OnLocated += Primary_OnLocated;
                    }
                }
                else if (anchor.Schema.GetOwn("autoexport", false).Value)
                {
                    Messages_OnAutoExport(anchor);
                }
            }

            if (null != Anchor)
            {
                _scan = (ScanWidget) Anchor.Children[0];
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

                    Anchor = anchor;
                    Anchor.OnLocated += Primary_OnLocated;
                    _scan = (ScanWidget) anchor.Children[0];

                    SaveAnchor(Anchor);
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
<Float>
    <Caption id='cpn' position=(0, 0.25, 0) label='Locating anchors.' width=1400.0 alignment='MidCenter' fontSize=100 />
    <Button id='btn' position=(0, -0.1, 0) label='Bypass' visible=false />
</Float>");
            _cpn = _rootUI.FindOne<CaptionWidget>("..cpn");
            _bypassBtn = _rootUI.FindOne<ButtonWidget>("..btn");
            _bypassBtn.OnActivated += _ => BypassAnchorRequirement();
        }

        /// <summary>
        /// Forcibly bypasses the requirement for anchors to be placed properly.
        /// </summary>
        private void BypassAnchorRequirement()
        {
            Log.Info(this, "Bypassing anchoring requirements.");

            CloseStatusUI();
            _isBypass = true;

            Ready();
        }

        /// <summary>
        /// Updates the status UI every frame.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PollAnchors()
        {
            while (_pollAnchors)
            {
                // no primary anchor
                if (null == Anchor)
                {
                    _rootUI.Schema.Set("visible", false);
                }
                // anchors
                else
                {
                    var located = FirstLocatedAnchor();
                    var T = Matrix4x4.identity;

                    // calculate inverse transformation
                    if (null != located)
                    {
                        var locatedSchemaPos = located.Schema.Get<Vec3>("position").Value.ToVector();
                        var locatedSchemaRot = Quaternion.Euler(located.Schema.Get<Vec3>("rotation").Value.ToVector());

                        var locatedPos = located.GameObject.transform.position;
                        var locatedRot = located.GameObject.transform.rotation;

                        var A = Matrix4x4.TRS(locatedSchemaPos, locatedSchemaRot, Vector3.one);
                        var B = Matrix4x4.TRS(locatedPos, locatedRot, Vector3.one);

                        // T * A = B
                        T = B * Matrix4x4.Inverse(A);
                    }

                    // place non-located anchors relative to the located anchor
                    for (int i = 0, len = _anchors.Count; i < len; i++)
                    {
                        var anchor = _anchors[i];
                        if (anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsError
                            || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsImporting
                            || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsLoading
                            || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated)
                        {
                            if (null != located)
                            {
                                PositionAnchorRelative(anchor, T);
                            }
                            else
                            {
                                anchor.GameObject.transform.position = anchor.Schema.GetOwn("position", Vec3.Zero).Value.ToVector();
                                anchor.GameObject.transform.rotation = Quaternion.Euler(anchor.Schema.GetOwn("rotation", Vec3.Zero).Value.ToVector());
                            }
                        }
                    }
                    
                    UpdateStatusUI();
                }
                
                yield return null;
            }
        }

        /// <summary>
        /// Postions an anchor relative to a located anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="transformation">The tranformation.</param>
        private void PositionAnchorRelative(
            WorldAnchorWidget anchor,
            Matrix4x4 transformation)
        {
            var anchorSchemaPos = anchor.Schema.Get<Vec3>("position").Value.ToVector();
            var anchorSchemaRot = Quaternion.Euler(anchor.Schema.Get<Vec3>("rotation").Value.ToVector());

            // T * A_anchor = A_located
            var A_anchor = Matrix4x4.TRS(anchorSchemaPos, anchorSchemaRot, Vector3.one);
            var A_located = T * A_anchor;

            anchor.GameObject.transform.position = A_located.GetColumn(3);
            anchor.GameObject.transform.rotation = A_located.rotation;
        }

        /// <summary>
        /// Updates the status UI.
        /// </summary>
        private void UpdateStatusUI()
        {
            if (null == _cpn || null == _rootUI)
            {
                return;
            }
            
            var errors = 0;
            var downloading = 0;
            var importing = 0;
            var unlocated = 0;
            var located = 0;

            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                switch (_anchors[i].Status)
                {
                    case WorldAnchorWidget.WorldAnchorStatus.IsError:
                    {
                        errors += 1;
                        break;
                    }
                    case WorldAnchorWidget.WorldAnchorStatus.IsLoading:
                    {
                        downloading += 1;
                        break;
                    }
                    case WorldAnchorWidget.WorldAnchorStatus.IsImporting:
                    {
                        importing += 1;
                        break;
                    }
                    case WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated:
                    {
                        unlocated += 1;
                        break;
                    }
                    case WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated:
                    {
                        located += 1;
                        break;
                    }
                }
            }

            _cpn.Label = string.Format(
@"Downloading: {1} / {0}
Importing: {2} / {0}
Locating: {3} / {0}
Errors: {4} / {0}",
                _anchors.Count,
                downloading,
                importing,
                unlocated,
                errors);

            if (located > 0)
            {
                _rootUI.Schema.Set("visible", false);
            }
            else
            {
                _rootUI.Schema.Set("visible", true);

                if ((unlocated + errors) == _anchors.Count)
                {
                    _bypassBtn.Schema.Set("visible", true);
                }
            }
        }

        /// <summary>
        /// Retrieves the first located anchor or null if no anchors are located.
        /// </summary>
        private WorldAnchorWidget FirstLocatedAnchor()
        {
            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
                if (anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated)
                {
                    return anchor;
                }
            }

            return null;
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
        /// Called when anchors have been enabled/disabled.
        /// </summary>
        private void Anchors_OnEnabledChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            if (prev == next)
            {
                return;
            }

            if (next)
            {
                SetupAnchors();
            }
            else
            {
                TeardownAnchors();
                Ready();
            }
        }

        /// <summary>
        /// Called when the primary anchor has been located.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        private void Primary_OnLocated(WorldAnchorWidget anchor)
        {
            Ready();
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

                // add
                _anchors.Add(anchor);

                // export from this new position
                anchor.Export(_config.Play.AppId, _sceneId, _txns);
            });
        }

        /// <summary>
        /// Called when the primary anchor should be destroyed and reset.
        /// </summary>
        private void Messages_OnResetPrimary(object _)
        {
            Log.Info(this, "Reset primary anchor requested.");

            if (null == Anchor)
            {
                Log.Warning(this, "No primary anchor to destroy.");
                return;
            }

            TeardownMeshScan();

            // destroy primary anchor
            _txns
                .Request(new ElementTxn(_sceneId).Delete(Anchor.Id))
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