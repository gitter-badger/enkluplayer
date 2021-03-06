﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
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
        public const string PROP_LOCATING_MESSAGE_KEY = "anchoring.locatingMessage";
        public const string PROP_DISABLE_BYPASS_KEY = "anchoring.disableBypass";

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
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

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
        private ReadOnlyCollection<WorldAnchorWidget> _anchors = new ReadOnlyCollection<WorldAnchorWidget>(new List<WorldAnchorWidget>());

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
        private TextWidget _cpnProgress;

        /// <summary>
        /// Caption on UI.
        /// </summary>
        private TextWidget _cpnLocating;

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

        /// <summary>
        /// Count of different anchor statuses.
        /// </summary>
        private int _pollUnlocated;
        private int _pollLocated;

        /// <summary>
        /// A custom locating message.
        /// </summary>
        private ElementSchemaProp<string> _locatingMessageProp;

        /// <summary>
        /// Whether the bypass button should be disabled or not.
        /// </summary>
        private ElementSchemaProp<bool> _disableBypassProp;

        /// <summary>
        /// Read only collection of currently tracked anchors.
        /// </summary>
        public ReadOnlyCollection<WorldAnchorWidget> Anchors
        {
            get { return _anchors; }
        }

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

        /// <inheritdoc />
        public event Action OnAnchorElementUpdate;

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
            IMetricsService metrics,
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
            _metrics = metrics;
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
            _anchors = new ReadOnlyCollection<WorldAnchorWidget>(new List<WorldAnchorWidget>());
            if (OnAnchorElementUpdate != null)
            {
                OnAnchorElementUpdate();
            }

            // see if we need to use anchors
            _anchorsEnabledProp = root.Schema.GetOwn(PROP_ENABLED_KEY, false);
            _anchorsEnabledProp.OnChanged += Anchors_OnEnabledChanged;

            if (_anchorsEnabledProp.Value)
            {
                _locatingMessageProp = root.Schema.GetOwn(PROP_LOCATING_MESSAGE_KEY, "Attempting to locate content.\nPlease walk around space.");
                _disableBypassProp = root.Schema.GetOwn(PROP_DISABLE_BYPASS_KEY, false);

                _locatingMessageProp.OnChanged += (prop, prev, next) => UpdateLocatingUI();
                _disableBypassProp.OnChanged += (prop, prev, next) => UpdateLocatingUI();

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
                UnityEngine.XR.InputTracking.Recenter();
#endif
            });
            _voice.Register("bypass", _ =>
            {
                if (!AreAllAnchorsReady)
                {
                    BypassAnchorRequirement();
                }
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

            Anchor = null;

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
        /// Modifies a position/rotation relative to a located anchor. The primary anchor is prioritized.
        /// The anchor used for relative positioning is returned. If all anchors aren't located, null is returned.
        /// TODO: Remove UnityEngine dependencies
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public WorldAnchorWidget RelativeTransform(ref Vector3 position, ref Quaternion rotation)
        {
            WorldAnchorWidget refAnchor = null;

            // Attempt to use the primary... primarily
            if (Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated)
            {
                refAnchor = Anchor;
            }
            else
            {
                // Fallback to any located anchor
                var located = FirstLocatedAnchor();
                if (null != located)
                {
                    refAnchor = located;
                }
            }

            if (null != refAnchor)
            {
                position = refAnchor.GameObject.transform.InverseTransformPoint(position);
                rotation = Quaternion.Inverse(rotation) * refAnchor.GameObject.transform.rotation;
            }

            return refAnchor;
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
            var anchors = new List<WorldAnchorWidget>();
            root.Find("..(@type=WorldAnchorWidget)", anchors);
            _anchors = new ReadOnlyCollection<WorldAnchorWidget>(anchors);

            if (OnAnchorElementUpdate != null)
            {
                OnAnchorElementUpdate();
            }

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
<Float focus.visible=false>
    <Caption id='cpn-progress' position=(0, 0.25, 0) label='Locating anchors.' width=1400.0 alignment='MidCenter' fontSize=100 />
    <Caption visible=false id='cpn-locating' position=(0, 0.25, 0) label='Locating anchors.' width=1400.0 alignment='MidCenter' fontSize=100 />
    <Button id='btn' position=(0, -0.1, 0) label='Bypass' visible=false />
</Float>");
            _cpnProgress = _rootUI.FindOne<TextWidget>("..cpn-progress");
            _cpnLocating = _rootUI.FindOne<TextWidget>("..cpn-locating");
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
                    UpdateRelativePositioning();

                    int errors, downloading, importing, notLocated, numLocated;
                    CountAnchors(
                        out errors,
                        out downloading,
                        out importing,
                        out notLocated,
                        out numLocated);

                    UpdateStatusUI(errors, downloading, importing, notLocated, numLocated);

                    if (!AreAllAnchorsReady && numLocated > 0 && downloading == 0 && importing == 0)
                    {
                        Ready();
                    }

                    // metrics
                    if (0 == importing && 0 == errors
                        && (_pollUnlocated != notLocated || _pollLocated != numLocated))
                    {
                        _pollUnlocated = notLocated;
                        _pollLocated = numLocated;

                        _metrics.Value(MetricsKeys.ANCHOR_STATE_LOCATEDRATIO).Value((float) _pollLocated / _anchors.Count);
                        _metrics.Value(MetricsKeys.ANCHOR_STATE_UNLOCATEDRATIO).Value((float) _pollUnlocated / _anchors.Count);
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Updates relative positioning of all un-located anchors.
        /// </summary>
        private void UpdateRelativePositioning()
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
                if (ShouldPositionRelatively(anchor))
                {
                    if (null != located)
                    {
                        PositionAnchorRelative(anchor, T);
                    }
                    else
                    {
                        anchor.GameObject.transform.position = anchor.Schema.GetOwn("position", Vec3.Zero).Value.ToVector();
                        anchor.GameObject.transform.rotation =
                            Quaternion.Euler(anchor.Schema.GetOwn("rotation", Vec3.Zero).Value.ToVector());
                    }
                }
            }
        }

        /// <summary>
        /// Determines if an anchor should be manually repositioned or not.
        /// </summary>
        /// <param name="anchor">The anchor to check.</param>
        /// <returns></returns>
        private bool ShouldPositionRelatively(WorldAnchorWidget anchor)
        {
            return anchor.Status == WorldAnchorWidget.WorldAnchorStatus.None
                   || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsError
                   || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsImporting
                   || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsLoading
                   || anchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated;
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
            var A_located = transformation * A_anchor;

            anchor.GameObject.transform.position = A_located.GetColumn(3);
            anchor.GameObject.transform.rotation = A_located.rotation;
        }

        /// <summary>
        /// Updates the status UI.
        /// </summary>
        private void UpdateStatusUI(int errors, int downloading, int importing, int unlocated, int located)
        {
            if (null == _cpnProgress || null == _rootUI)
            {
                return;
            }

            if (downloading + importing + errors > 0)
            {
                _cpnProgress.LocalVisible = true;
                _cpnLocating.LocalVisible = false;

                UpdateProgressUI(errors, downloading, importing, unlocated, located);
            }
            else if (0 == located)
            {
                _cpnLocating.LocalVisible = true;
                _cpnProgress.LocalVisible = false;

                UpdateLocatingUI();
            }
            else
            {
                _rootUI.Schema.Set("visible", false);
            }
        }

        /// <summary>
        /// Updates the progress UI.
        /// </summary>
        private void UpdateProgressUI(int errors, int downloading, int importing, int unlocated, int located)
        {
            _cpnProgress.Label = string.Format(
                @"Please wait...

Downloading: {1} / {0}
Importing: {2} / {0}
Errors: {3} / {0}",
                _anchors.Count,
                downloading,
                importing,
                errors);

            _rootUI.Schema.Set("visible", true);

            if (unlocated + errors == _anchors.Count)
            {
                _bypassBtn.Schema.Set("visible", !_disableBypassProp.Value);
            }
        }

        /// <summary>
        /// Updates the locating UI.
        /// </summary>
        private void UpdateLocatingUI()
        {
            if (_cpnLocating == null)
            {
                // Safety in case custom messages are set before the vine has been built.
                return;
            }

            _cpnLocating.Label = _locatingMessageProp.Value;

            _rootUI.Schema.Set("visible", true);
            _bypassBtn.Schema.Set("visible", !_disableBypassProp.Value);
        }

        /// <summary>
        /// Counts up anchor types.
        /// </summary>
        /// <param name="errors">Number of anchors in an error state.</param>
        /// <param name="downloading">Number of anchors currently downloading.</param>
        /// <param name="importing">Number of anchors currently importing.</param>
        /// <param name="notLocated">Number of anchors currently not located.</param>
        /// <param name="located">Number of located anchors.</param>
        private void CountAnchors(out int errors, out int downloading, out int importing, out int notLocated, out int located)
        {
            errors = 0;
            downloading = 0;
            importing = 0;
            notLocated = 0;
            located = 0;

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
                        notLocated += 1;
                        break;
                    }
                    case WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated:
                    {
                        located += 1;
                        break;
                    }
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
                _cpnProgress = null;
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

                // TODO: Anchoring Refactor - Manage invocation of this better so anchors can't be double added.
                // This occurs when anchors are created after the initial scene graph is searched for anchors.
                if (!_anchors.Contains(anchor))
                {
                    // Super ugly. I'm the worst.
                    var tmp = new List<WorldAnchorWidget>(_anchors);
                    tmp.Add(anchor);
                    _anchors = new ReadOnlyCollection<WorldAnchorWidget>(tmp);

                    if (OnAnchorElementUpdate != null)
                    {
                        OnAnchorElementUpdate();
                    }
                }

                // Trigger relative positioning update so the anchor pending export
                //    has the right position before it is saved.
                UpdateRelativePositioning();

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