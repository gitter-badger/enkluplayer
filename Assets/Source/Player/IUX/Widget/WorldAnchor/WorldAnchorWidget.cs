using System;
using System.Diagnostics;
using System.Globalization;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Widget that anchors in world space.
    /// </summary>
    public class WorldAnchorWidget : Widget
    {
        public enum WorldAnchorStatus
        {
            None,
            IsLoading,
            IsImporting,
            IsExporting,
            IsReadyLocated,
            IsReadyNotLocated,
            IsError
        }
        
        /// <summary>
        /// Abstracts anchoring method.
        /// </summary>
        private readonly IAnchorStore _store;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;
        
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<int> _versionProp;
        private ElementSchemaProp<bool> _lockedProp;

        /// <summary>
        /// True iff the anchor is already imported.
        /// </summary>
        private bool _pollStatus;

        /// <summary>
        /// Timer whilst unlocated.
        /// </summary>
        private int _unlocatedTimerId = -1;

        /// <summary>
        /// Backing variable for Status.
        /// </summary>
        private WorldAnchorStatus _status;

        /// <summary>
        /// Status.
        /// </summary>
        public WorldAnchorStatus Status
        {
            get { return _status; }
            set
            {
                if (value == _status)
                {
                    return;
                }

                var prev = _status;
                _status = value;

                if (_status == WorldAnchorStatus.IsReadyLocated)
                {
                    _metrics.Timer(MetricsKeys.ANCHOR_UNLOCATED).Stop(_unlocatedTimerId);

                    if (null != OnLocated)
                    {
                        OnLocated(this);
                    }

                    UnlocatedStartTime = 0;
                }
                else if (prev == WorldAnchorStatus.IsReadyLocated)
                {
                    _unlocatedTimerId = _metrics.Timer(MetricsKeys.ANCHOR_UNLOCATED).Start();

                    if (null != OnUnlocated)
                    {
                        OnUnlocated(this);
                    }

                    UnlocatedStartTime = Time.realtimeSinceStartup;
                }
            }
        }
        
        /// <summary>
        /// The Time.realtimeSinceStartup value when this anchor lost tracking. 0 if anchor is located.
        /// </summary>
        public float UnlocatedStartTime { get; private set; }

        /// <summary>
        /// Called when switching into located.
        /// </summary>
        public event Action<WorldAnchorWidget> OnLocated;

        /// <summary>
        /// Called when was located and switched to not located.
        /// </summary>
        public event Action<WorldAnchorWidget> OnUnlocated;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldAnchorWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IAnchorStore store,
            IMetricsService metrics)
            : base(gameObject, layers, tweens, colors)
        {
            _store = store;
            _metrics = metrics;
        }

        /// <summary>
        /// Reloads world anchor.
        /// </summary>
        public void Reload()
        {
            UpdateWorldAnchor();
        }

        /// <summary>
        /// Exports the anchor.
        /// </summary>
        /// <param name="appId">App anchor is in.</param>
        /// <param name="sceneId">Scene anchor is in.</param>
        public void Export(string appId, string sceneId)
        {
            _pollStatus = false;
            Status = WorldAnchorStatus.IsExporting;

            _versionProp.OnChanged -= Version_OnChanged;

            _store
                .Export(Id, Mathf.Max(1, _versionProp.Value + 1), GameObject)
                .OnFinally(_ =>
                {
                    _versionProp.OnChanged += Version_OnChanged;
                });
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            Status = WorldAnchorStatus.None;

            _versionProp = Schema.GetOwn("version", -1);
            _versionProp.OnChanged += Version_OnChanged;

            _lockedProp = Schema.GetOwn("locked", false);
            _lockedProp.OnChanged += Locked_OnChanged;
            
            UpdateWorldAnchor();
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            // selection collider
            if (DeviceHelper.IsWebGl())
            {
                var collider = EditCollider;
                if (null != collider)
                {
                    collider.center = Vector3.zero;
                    collider.size = 0.5f * Vector3.one;
                }
            }
        }

        /// <inheritdoc />
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            if (_pollStatus)
            {
#if NETFX_CORE
                var anchor = GameObject.GetComponent<UnityEngine.XR.WSA.WorldAnchor>();
                if (null != anchor)
                {
                    Status = anchor.isLocated
                        ? WorldAnchorStatus.IsReadyLocated
                        : WorldAnchorStatus.IsReadyNotLocated;
                }
                else
                {
                    Status = WorldAnchorStatus.None;
                }
#else
                Status = WorldAnchorStatus.IsReadyLocated;
#endif
            }
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();
            
            _versionProp.OnChanged -= Version_OnChanged;
            _lockedProp.OnChanged -= Locked_OnChanged;

            _store.UnAnchor(GameObject);
        }

        /// <inheritdoc />
        protected override void UpdateTransform()
        {
            // on hololens, we let the MS api set the position
            if (!DeviceHelper.IsHoloLens())
            {
                base.UpdateTransform();
            }
        }

        /// <summary>
        /// Reloads the world anchor.
        /// </summary>
        [
            Conditional("NETFX_CORE"),
            Conditional("UNITY_EDITOR")
        ]
        private void UpdateWorldAnchor()
        {
            Status = WorldAnchorStatus.IsLoading;
            _pollStatus = false;
            
            var version = _versionProp.Value;
            if (0 == version)
            {
                // backward compat: check src prop
                var src = Schema.GetOwn("src", string.Empty).Value;
                if (string.IsNullOrEmpty(src))
                {
                    Log.Info(this, "Anchor has not yet been exported, so there is nothing to load.");
                    return;
                }
            }

            Log.Info(this, "Anchor data exists, proceeding to import.");

            // anchor this game object
            _store.Anchor(Id, version, GameObject);
        }
        
        /// <summary>
        /// Called when the file id changes.
        /// </summary>
        private void Version_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            UpdateWorldAnchor();
        }

        /// <summary>
        /// Called when locked property changes.
        /// </summary>
        private void Locked_OnChanged(
            ElementSchemaProp<bool> prop, 
            bool prev,
            bool next)
        {
            if (next)
            {
                UpdateWorldAnchor();
            }
            else
            {
                _store.UnAnchor(GameObject);
            }
        }
    }
}