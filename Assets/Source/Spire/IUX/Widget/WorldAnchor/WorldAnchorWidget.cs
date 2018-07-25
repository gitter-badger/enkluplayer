using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        /// Abstracts anchoring method.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Token for anchor download.
        /// </summary>
        private IAsyncToken<HttpResponse<byte[]>> _downloadToken;

        /// <summary>
        /// Token returned from IWorldAnchorProvider::Anchor.
        /// </summary>
        private IAsyncToken<Void> _anchorToken;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<int> _versionProp;
        private ElementSchemaProp<bool> _lockedProp;

        /// <summary>
        /// True iff the anchor is already imported.
        /// </summary>
        private bool _isImported;

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
            IWorldAnchorProvider provider,
            IMetricsService metrics)
            : base(gameObject, layers, tweens, colors)
        {
            _http = http;
            _provider = provider;
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
        /// Retrieves the anchor provider id.
        /// </summary>
        /// <returns></returns>
        public static string GetAnchorProviderId(string id, int version)
        {
            return string.Format("{0}.{1}", id, version);
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

            if (_isImported)
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
        [Conditional("NETFX_CORE"), Conditional("UNITY_EDITOR")]
        private void UpdateWorldAnchor()
        {
            Status = WorldAnchorStatus.IsLoading;
            _isImported = false;

            // abort previous tokens
            if (null != _anchorToken)
            {
                _anchorToken.Abort();
                _anchorToken = null;
            }

            if (null != _downloadToken)
            {
                _downloadToken.Abort();
                _downloadToken = null;
            }

            // version check (there may be no data for this anchor)
            var version = _versionProp.Value;
            if (version < 0)
            {
                Log.Warning(this, "Anchor [{0}] has an invalid version.", Id);

                Status = WorldAnchorStatus.IsError;

                if (null != OnAnchorLoadError)
                {
                    OnAnchorLoadError();
                }
                
                return;
            }

            // url check (there may be bad data for this anchor)
            var url = Schema.Get<string>("src").Value;
            if (string.IsNullOrEmpty(url))
            {
                Log.Warning(this, "Anchor [{0}] has invalid src prop.", Id);

                Status = WorldAnchorStatus.IsError;

                if (null != OnAnchorLoadError)
                {
                    OnAnchorLoadError();
                }

                return;
            }

            // see if the provider can anchor this version
            _anchorToken = _provider
                .Anchor(GetAnchorProviderId(Id, Schema.Get<int>("version").Value), GameObject)
                .OnSuccess(_ =>
                {
                    // done
                    _isImported = true;
                })
                .OnFailure(exception =>
                {
                    Log.Warning(this, "Could not anchor : {0}", exception);
                    
                    // anchor has not been imported before, apparently
                    DownloadAndImport(url);
                });
        }

        /// <summary>
        /// Downloads world anchor data and imports it.
        /// </summary>
        /// <param name="url">Absolute url at which to download.</param>
        private void DownloadAndImport(string url)
        {
            // metrics
            var downloadId = _metrics.Timer(MetricsKeys.ANCHOR_DOWNLOAD).Start();

            _downloadToken = _http
                .Download(_http.Urls.Url(url))
                .OnSuccess(response =>
                {
                    LogVerbose("Anchor downloaded. Importing.");

                    Status = WorldAnchorStatus.IsImporting;

                    Import(response.Payload);
                })
                .OnFailure(exception =>
                {
                    LogVerbose(
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
                    // metrics
                    _metrics.Timer(MetricsKeys.ANCHOR_DOWNLOAD).Stop(downloadId);

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
            LogVerbose("Bytes available, starting import.");

            var providerId = GetAnchorProviderId(Id, Schema.Get<int>("version").Value);

            _provider
                .Import(providerId, bytes)
                .OnSuccess(_ =>
                {
                    LogVerbose("Successfully imported anchor.");

                    _provider
                        .Anchor(providerId, GameObject)
                        .OnSuccess(__ =>
                        {
                            _isImported = true;

                            if (null != OnAnchorLoadSuccess)
                            {
                                OnAnchorLoadSuccess();
                            }
                        })
                        .OnFailure(ex =>
                        {
                            LogVerbose("Could not get anchor after import: {0}", ex);

                            Status = WorldAnchorStatus.IsError;

                            if (null != OnAnchorLoadError)
                            {
                                OnAnchorLoadError();
                            }
                        });
                })
                .OnFailure(exception =>
                {
                    LogVerbose(
                        "Could not import anchor : {0}.",
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
                _provider.UnAnchor(GameObject);
            }
        }
    }
}