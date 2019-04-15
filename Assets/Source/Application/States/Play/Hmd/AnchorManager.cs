using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Basic implementation of the anchor manager.
    /// </summary>
    /// <inheritdoc />
    public class AnchorManager : IAnchorManager
    {
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
        /// Dependencies.
        /// </summary>
        private readonly IAppSceneManager _scenes;
        private readonly IBootstrapper _bootstrapper;
        private readonly IMetricsService _metrics;
        private readonly IUIManager _ui;
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Configuration for entire application.
        /// </summary>
        private readonly ApplicationConfig _config;
        
        /// <summary>
        /// List of anchors in scene, including the primary anchor.
        /// </summary>
        private ReadOnlyCollection<WorldAnchorWidget> _anchors = new ReadOnlyCollection<WorldAnchorWidget>(new List<WorldAnchorWidget>());

        /// <summary>
        /// Callbacks for ready.
        /// </summary>
        private readonly List<CancelableCallback> _onReady = new List<CancelableCallback>();
        
        /// <summary>
        /// True iff anchors are enabled.
        /// </summary>
        private ElementSchemaProp<bool> _anchorsEnabledProp;
        
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
        /// Displays information to the user about anchor status.
        /// </summary>
        private AnchorStatusUIView _view;

        /// <summary>
        /// UI id for the view.
        /// </summary>
        private int _viewId;

        /// <summary>
        /// Read only collection of currently tracked anchors.
        /// </summary>
        public ReadOnlyCollection<WorldAnchorWidget> Anchors
        {
            get { return _anchors; }
        }
        
        /// <inheritdoc />
        public WorldAnchorWidget Primary { get; private set; }

        /// <inheritdoc />
        public IAnchorStore Store { get; private set; }

        /// <inheritdoc />
        public bool IsReady
        {
            get { return AreAllAnchorsReady; }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorManager(
            IAppSceneManager scenes,
            IBootstrapper bootstrapper,
            IMetricsService metrics,
            IUIManager ui,
            IAnchorStore store,
            IElementTxnManager txns,
            ApplicationConfig config)
        {
            _scenes = scenes;
            _bootstrapper = bootstrapper;
            _metrics = metrics;
            _ui = ui;
            _config = config;
            _txns = txns;

            Store = store;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Setup()
        {
            // shortcut
            if (!DeviceHelper.IsHoloLens())
            {
                Ready();
                return new AsyncToken<Void>(Void.Instance);
            }

            // listen for scene creation
            _scenes.OnSceneCreated += Scenes_OnCreated;

            // setup store
            return Store.Setup(_txns, _scenes);
        }
        
        /// <inheritdoc />
        public void Teardown()
        {
            TeardownAnchors();

            // clear props
            if (null != _anchorsEnabledProp)
            {
                _anchorsEnabledProp.OnChanged -= Anchors_OnEnabledChanged;
                
                _anchorsEnabledProp = null;
                _locatingMessageProp = null;
                _disableBypassProp = null;
            }
            
            Primary = null;
            Store.Teardown();

            _scenes.OnSceneCreated -= Scenes_OnCreated;
        }

        /// <inheritdoc />
        public ICancelable OnReady(Action ready)
        {
            var cb = new CancelableCallback(ready);

            if (AreAllAnchorsReady)
            {
                cb.Invoke();
            }
            else
            {
                _onReady.Add(cb);
            }

            return cb;
        }

        /// <summary>
        /// Disabled anchors.
        ///
        /// Used by DebugService, not visible on interface.
        /// </summary>
        public void BypassAnchorRequirement()
        {
            _anchorsEnabledProp.Value = false;
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
                temp[i].Invoke();
            }
        }
        
        /// <summary>
        /// Sets up anchors for a scene.
        /// </summary>
        private void SetupAnchors(Element root)
        {
            // update anchors
            var anchors = new List<WorldAnchorWidget>();
            root.Find("..(@type=WorldAnchorWidget)", anchors);
            _anchors = new ReadOnlyCollection<WorldAnchorWidget>(anchors);
            
            // find primary
            FindPrimaryAnchor();

            // poll for anchors if we are not already
            if (!_pollAnchors)
            {
                _pollAnchors = true;
                _bootstrapper.BootstrapCoroutine(PollAnchors());
            }
        }

        /// <summary>
        /// Tears down anchors.
        /// </summary>
        private void TeardownAnchors()
        {
            AreAllAnchorsReady = false;

            _pollAnchors = false;

            CloseStatusUI();

            _anchors = new ReadOnlyCollection<WorldAnchorWidget>(new List<WorldAnchorWidget>());
        }
        
        /// <summary>
        /// Opens the status UI.
        /// </summary>
        private void OpenStatusUI()
        {
            Log.Info(this, "Opening status UI.");

            _ui
                .OpenOverlay<AnchorStatusUIView>(new UIReference
                {
                    UIDataId = "Anchor.Status"
                }, out _viewId)
                .OnSuccess(el =>
                {
                    _view = el;
                    _view.OnBypass += () => _anchorsEnabledProp.Value = false;
                })
                .OnFailure(ex => Log.Error(this,
                    "Could not open anchor status UI: {0}",
                    ex));
        }

        /// <summary>
        /// Closes the status UI.
        /// </summary>
        private void CloseStatusUI()
        {
            Log.Info(this, "Closing status UI.");

            if (0 != _viewId)
            {
                _ui.Close(_viewId);
                _view = null;
                _viewId = 0;
            }
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
                if (null == Primary)
                {
                    if (null != _view)
                    {
                        _view.Root.Schema.Set("visible", false);
                    }
                }
                // anchors
                else
                {
                    UpdateRelativePositioning();

                    int errors, downloading, importing, notLocated, numLocated;
                    CountAnchors(
                        _anchors,
                        out errors,
                        out downloading,
                        out importing,
                        out notLocated,
                        out numLocated);

                    UpdateStatusUI(errors, downloading, importing, notLocated, numLocated);
                    
                    // ready
                    if (!AreAllAnchorsReady && numLocated > 0 && downloading == 0 && importing == 0)
                    {
                        Ready();
                    }

                    // report metrics
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
            var located = FindFirstLocatedAnchor();
            var T = Matrix4x4.identity;

            // calculate inverse transformation
            if (null != located)
            {
                var locatedSchemaPos = located.Schema.Get<Vec3>("position").Value.ToVector();
                var locatedSchemaRot = Quaternion.Euler(located.Schema.Get<Vec3>("rotation").Value.ToVector());

                var locatedPos = located.GameObject.transform.position;
                var locatedRot = located.GameObject.transform.rotation;

                var a = Matrix4x4.TRS(locatedSchemaPos, locatedSchemaRot, Vector3.one);
                var b = Matrix4x4.TRS(locatedPos, locatedRot, Vector3.one);

                // T * a = b
                T = b * Matrix4x4.Inverse(a);
            }

            // place non-located anchors relative to the located anchor
            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
                if (ShouldPositionRelatively(anchor))
                {
                    if (null != located)
                    {
                        PositionAnchorRelatively(anchor, T);
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
        /// Updates the status UI.
        /// </summary>
        private void UpdateStatusUI(int errors, int downloading, int importing, int unlocated, int located)
        {
            if (null == _view)
            {
                return;
            }

            // show import progress
            if (downloading + importing + errors > 0)
            {
                _view.Root.Schema.Set("visible", true);
                _view.TxtProgress.LocalVisible = true;
                _view.TxtLocating.LocalVisible = false;

                _view.TxtProgress.Label = string.Format(
                    @"Please wait...

Downloading: {1} / {0}
Importing: {2} / {0}
Errors: {3} / {0}",
                    _anchors.Count,
                    downloading,
                    importing,
                    errors);

                if (unlocated + errors == _anchors.Count)
                {
                    _view.BtnBypass.Schema.Set("visible", !_disableBypassProp.Value);
                }
            }
            // show custom message
            else if (0 == located)
            {
                _view.Root.Schema.Set("visible", true);
                _view.TxtProgress.LocalVisible = false;
                _view.TxtLocating.LocalVisible = true;

                _view.TxtLocating.Label = _locatingMessageProp.Value;
                _view.BtnBypass.Schema.Set("visible", !_disableBypassProp.Value);
            }
            // hide status ui completely
            else
            {
                _view.Root.Schema.Set("visible", false);
            }
        }
        
        /// <summary>
        /// Locates the primary anchor.
        /// </summary>
        private void FindPrimaryAnchor()
        {
            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
                if (PROP_TAG_VALUE == anchor.Schema.Get<string>(PROP_TAG_KEY).Value)
                {
                    if (null != Primary)
                    {
                        Log.Error(this, "Found multiple primary anchors! Choosing first by id.");

                        // compare id so we at least pick the same primary each time
                        if (string.Compare(
                                Primary.Id,
                                anchor.Id,
                                StringComparison.Ordinal) < 0)
                        {
                            Primary = anchor;
                        }
                    }
                    else
                    {
                        Primary = anchor;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the first located anchor or null if no anchors are located.
        /// </summary>
        private WorldAnchorWidget FindFirstLocatedAnchor()
        {
            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
                if (anchor.Status == WorldAnchorStatus.IsReadyLocated)
                {
                    return anchor;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Called when a scene has been created.
        /// </summary>
        /// <param name="root">The scene.</param>
        private void Scenes_OnCreated(Element root)
        {
            Log.Info(this, "Scene was created. Tracking anchors.");

            // the first scene loaded gets to dictate global props
            if (null == _anchorsEnabledProp)
            {
                // watch enabled prop
                _anchorsEnabledProp = root.Schema.GetOwn(PROP_ENABLED_KEY, false);
                _anchorsEnabledProp.OnChanged += Anchors_OnEnabledChanged;

                // props for UI
                _locatingMessageProp = root.Schema.GetOwn(
                    PROP_LOCATING_MESSAGE_KEY,
                    "Attempting to locate content.\nPlease walk around space.");
                _disableBypassProp = root.Schema.GetOwn(PROP_DISABLE_BYPASS_KEY, false);
            }

            if (!_config.Play.Edit && 0 == _viewId)
            {
                OpenStatusUI();
            }

            // if anchors are enabled, setup anchors for this scene
            if (_anchorsEnabledProp.Value)
            {
                SetupAnchors(root);
            }
            // otherwise, if there are no anchors already, proceed
            else if (0 == _anchors.Count)
            {
                Ready();
            }
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

            if (!next)
            {
                TeardownAnchors();
                Ready();
            }
        }

        /// <summary>
        /// Counts up anchor types.
        /// </summary>
        /// <param name="anchors">Collection of world anchors to count.</param>
        /// <param name="errors">Number of anchors in an error state.</param>
        /// <param name="downloading">Number of anchors currently downloading.</param>
        /// <param name="importing">Number of anchors currently importing.</param>
        /// <param name="notLocated">Number of anchors currently not located.</param>
        /// <param name="located">Number of located anchors.</param>
        private void CountAnchors(
            IList<WorldAnchorWidget> anchors,
            out int errors, out int downloading, out int importing, out int notLocated, out int located)
        {
            errors = 0;
            downloading = 0;
            importing = 0;
            notLocated = 0;
            located = 0;
            
            for (int i = 0, len = anchors.Count; i < len; i++)
            {
                switch (anchors[i].Status)
                {
                    case WorldAnchorStatus.IsError:
                    {
                        errors += 1;
                        break;
                    }
                    case WorldAnchorStatus.IsLoading:
                    {
                        downloading += 1;
                        break;
                    }
                    case WorldAnchorStatus.IsImporting:
                    {
                        importing += 1;
                        break;
                    }
                    case WorldAnchorStatus.IsReadyNotLocated:
                    {
                        notLocated += 1;
                        break;
                    }
                    case WorldAnchorStatus.IsReadyLocated:
                    {
                        located += 1;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if an anchor should be manually repositioned or not.
        /// </summary>
        /// <param name="anchor">The anchor to check.</param>
        /// <returns></returns>
        private static bool ShouldPositionRelatively(WorldAnchorWidget anchor)
        {
            return anchor.Status == WorldAnchorStatus.None
                   || anchor.Status == WorldAnchorStatus.IsError
                   || anchor.Status == WorldAnchorStatus.IsImporting
                   || anchor.Status == WorldAnchorStatus.IsLoading
                   || anchor.Status == WorldAnchorStatus.IsReadyNotLocated;
        }

        /// <summary>
        /// Positions an anchor relative to a located anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <param name="transformation">The transformation.</param>
        private static void PositionAnchorRelatively(
            WorldAnchorWidget anchor,
            Matrix4x4 transformation)
        {
            var anchorSchemaPos = anchor.Schema.Get<Vec3>("position").Value.ToVector();
            var anchorSchemaRot = Quaternion.Euler(anchor.Schema.Get<Vec3>("rotation").Value.ToVector());

            // T * aAnchor = aLocated
            var aAnchor = Matrix4x4.TRS(anchorSchemaPos, anchorSchemaRot, Vector3.one);
            var aLocated = transformation * aAnchor;

            anchor.GameObject.transform.position = aLocated.GetColumn(3);
            anchor.GameObject.transform.rotation = aLocated.rotation;
        }
    }
}