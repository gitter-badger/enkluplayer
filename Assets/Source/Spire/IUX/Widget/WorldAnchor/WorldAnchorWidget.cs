﻿using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Widget that anchors in world space.
    /// </summary>
    public class WorldAnchorWidget : Widget
    {
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
        /// True iff anchor data is loaded.
        /// </summary>
        public bool IsAnchorLoaded { get; private set; }

        /// <summary>
        /// True iff anchor data is loading.
        /// </summary>
        public bool IsAnchorLoading { get; private set; }
        
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

            IsAnchorLoaded = false;
            IsAnchorLoading = true;

            var version = _versionProp.Value;
            if (version < 0)
            {
                IsAnchorLoading = false;

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

                IsAnchorLoading = false;

                if (null != OnAnchorLoadError)
                {
                    OnAnchorLoadError();
                }

                return;
            }

            // check cache
            if (_cache.Contains(Id, _versionProp.Value))
            {
                Log.Info(this, "World anchor cache hit.");

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
                Log.Info(this, "World anchor cache miss.");

                DownloadAndImport(url);
            }
        }

        /// <summary>
        /// Downloads world anchor data and imports it.
        /// </summary>
        /// <param name="url">Absolute url at which to download.</param>
        private void DownloadAndImport(string url)
        {
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

                    IsAnchorLoading = false;

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
            Log.Info(this, "Bytes available, starting import.");

            _provider
                .Import(Id, GameObject, bytes)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "Successfully imported anchor.");

                    IsAnchorLoading = false;
                    IsAnchorLoaded = true;

                    if (null != OnAnchorLoadSuccess)
                    {
                        OnAnchorLoadSuccess();
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not import anchor : {0}.",
                        exception);

                    IsAnchorLoading = false;

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