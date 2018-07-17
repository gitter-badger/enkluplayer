using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.WSA;

namespace CreateAR.SpirePlayer.IUX
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
            IsReadyLocated,
            IsReadyNotLocated,
            IsError
        }

        /// <summary>
        /// For downloading anchors.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Caches world anchor data.
        /// </summary>
        private readonly IWorldAnchorCache _cache;

        /// <summary>
        /// Abstracts anchoring method.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Token for anchor download.
        /// </summary>
        private IAsyncToken<HttpResponse<byte[]>> _downloadToken;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<int> _versionProp;
        private ElementSchemaProp<bool> _lockedProp;

        /// <summary>
        /// True iff the anchor is already imported.
        /// </summary>
        private bool _isImported = false;

        /// <summary>
        /// Status.
        /// </summary>
        public WorldAnchorStatus Status { get; private set; }

        /// <summary>
        /// Called on load success.
        /// </summary>
        public event Action OnAnchorLoadSuccess;

        /// <summary>
        /// Called on load error.
        /// </summary>
        public event Action OnAnchorLoadError;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldAnchorWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IHttpService http,
            IWorldAnchorCache cache,
            IWorldAnchorProvider provider)
            : base(gameObject, layers, tweens, colors)
        {
            _http = http;
            _cache = cache;
            _provider = provider;
        }

        /// <summary>
        /// Reloads world anchor.
        /// </summary>
        public void Reload()
        {
            UpdateWorldAnchor();
        }
        
        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            Status = WorldAnchorStatus.None;

            _versionProp = Schema.GetOwn("version", -1);
            _versionProp.OnChanged += Version_OnChanged;

            _lockedProp = Schema.GetOwn("locked", true);
            _lockedProp.OnChanged += Locked_OnChanged;

            UpdateWorldAnchor();

            // selection collider
            {
                var collider = GameObject.GetComponent<BoxCollider>();
                if (null == collider)
                {
                    collider = GameObject.AddComponent<BoxCollider>();
                }

                collider.center = Vector3.zero;
                collider.size = 0.5f * Vector3.one;
                collider.isTrigger = true;
            }
        }

        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            if (_isImported)
            {
                var anchor = GameObject.GetComponent<WorldAnchor>();
                Status = anchor.isLocated
                    ? WorldAnchorStatus.IsReadyLocated
                    : WorldAnchorStatus.IsReadyNotLocated;
            }
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            if (null != _downloadToken)
            {
                _downloadToken.Abort();
                _downloadToken = null;
            }

            _versionProp.OnChanged -= Version_OnChanged;
        }

        /// <inheritdoc />
        protected override void UpdateTransform()
        {
            if (!DeviceHelper.IsHoloLens())
            {
                base.UpdateTransform();
            }
        }

        /// <summary>
        /// Reloads the world anchor.
        /// </summary>
        [Conditional("NETFX_CORE")]
        private void UpdateWorldAnchor()
        {
            // abort previous
            if (null != _downloadToken)
            {
                _downloadToken.Abort();
                _downloadToken = null;
            }
            
            var version = _versionProp.Value;
            if (version < 0)
            {
                Status = WorldAnchorStatus.IsError;

                if (null != OnAnchorLoadError)
                {
                    OnAnchorLoadError();
                }

                Log.Error(this, "Invalid version : {0}.", version);

                return;
            }

            var url = Schema.Get<string>("src").Value;
            if (string.IsNullOrEmpty(url))
            {
                Log.Error(this, string.Format(
                    "Anchor [{0}] has invalid src prop.",
                    Id));

                Status = WorldAnchorStatus.IsError;

                if (null != OnAnchorLoadError)
                {
                    OnAnchorLoadError();
                }

                return;
            }

            // check cache
            if (_cache.Contains(Id, _versionProp.Value))
            {
                Log.Info(this, "World anchor {0} cache hit.", Id);

                Status = WorldAnchorStatus.IsLoading;

                _cache
                    .Load(Id, _versionProp.Value)
                    .OnSuccess(Import)
                    .OnFailure(exception =>
                    {
                        // on cache error, try downloading
                        Log.Error(this, "There was an error loading world anchor {0} from the cache : {1}.",
                            Id,
                            exception);

                        DownloadAndImport(url);
                    });
            }
            else
            {
                Log.Info(this, "World anchor {0} cache miss.");

                DownloadAndImport(url);
            }
        }

        /// <summary>
        /// Downloads world anchor data and imports it.
        /// </summary>
        /// <param name="url">Absolute url at which to download.</param>
        private void DownloadAndImport(string url)
        {
            Status = WorldAnchorStatus.IsLoading;

            _isImported = false;

            _downloadToken = _http
                .Download(_http.Urls.Url(url))
                .OnSuccess(response =>
                {
                    LogVerbose("Anchor downloaded. Importing.");

                    // cache
                    _cache.Save(Id, _versionProp.Value, response.Payload);

                    Import(response.Payload);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not download {0} : {1}.",
                        url,
                        exception);

                    Status = WorldAnchorStatus.IsError;

                    if (null != OnAnchorLoadError)
                    {
                        OnAnchorLoadError();
                    }
                })
                .OnFinally(token =>
                {
                    if (token == _downloadToken)
                    {
                        _downloadToken = null;
                    }
                });
        }

        /// <summary>
        /// Imports bytes into a world anchor.
        /// </summary>
        /// <param name="bytes">The world anchor bytes.</param>
        private void Import(byte[] bytes)
        {
            Log.Info(this, "{0}::Bytes available, starting import.", Id);

            Status = WorldAnchorStatus.IsImporting;

            _provider
                .Import(Id, GameObject, bytes)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "{0}::Successfully imported anchor.", Id);

                    _isImported = true;
                    
                    if (null != OnAnchorLoadSuccess)
                    {
                        OnAnchorLoadSuccess();
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "{0}::Could not import anchor : {1}.",
                        Id,
                        exception);

                    Status = WorldAnchorStatus.IsError;

                    if (null != OnAnchorLoadError)
                    {
                        OnAnchorLoadError();
                    }
                });
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
                // kill any imports in progress
                if (null != _downloadToken)
                {
                    _downloadToken.Abort();
                }

                // disable anchor
                _provider.Disable(GameObject);
            }
        }
    }
}