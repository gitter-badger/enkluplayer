﻿using System;
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
        /// Maximum amount of time we should wait for an export in the case that an export is already in progress by another user.
        /// </summary>
        private const double MAX_EXPORT_TIMEOUT = 120.0;

        /// <summary>
        /// PRNG.
        /// </summary>
        //private static readonly Random _Prng = new Random();
        
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
        /// <param name="txns">Object to make txns with.</param>
        public void Export(string appId, string sceneId, IElementTxnManager txns)
        {
            _pollStatus = false;
            Status = WorldAnchorStatus.IsExporting;

            // check if anchor is already being exported
            var exportTime = Schema.GetOwn("export.time", "").Value;
            if (!string.IsNullOrEmpty(exportTime))
            {
                var lastExportTime = DateTime.Parse(exportTime);
                if (DateTime.UtcNow.Subtract(lastExportTime).TotalSeconds > MAX_EXPORT_TIMEOUT)
                {
                    Log.Info(this, "Attempted export while export is already underway.");
                    return;
                }
            }

            // lock down
            txns
                .Request(new ElementTxn(sceneId).Update(
                    Id,
                    "export.time",
                    DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)))
                .OnSuccess(_ =>
                {
                    var providerId = GetAnchorProviderId(Id, _versionProp.Value + 1);
                    _store
                        .Export(providerId, GameObject)
                        .OnSuccess(bytes =>
                        {
                            Log.Info(this, "Successfully exported from provider with id {0}. Uploading.", providerId);

                            //ExportAnchorData(txns, appId, sceneId, bytes, 3);
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this,
                                "Could not export anchor : {0}.",
                                exception);

                            txns
                                .Request(new ElementTxn(sceneId).Update(Id, "export.time", ""))
                                .OnFailure(ex => Log.Error(this,
                                    "Locked anchor to export, but export failed and we were unable to unlock. Timeout will save the day : {0}",
                                    exception));

                            Status = WorldAnchorStatus.IsError;
                        });
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not lock for export : {0}", exception);

                    Status = WorldAnchorStatus.IsError;
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
            
            // check version
            var version = _versionProp.Value;
            if (0 == version)
            {
                Log.Info(this, "Anchor has not yet been exported, so there is nothing to load.");
                return;
            }

            _store.UnAnchor(GameObject);
            _store.Anchor(Id, version, GameObject);
        }

        /*/// <summary>
        /// Downloads world anchor data and imports it.
        /// </summary>
        /// <param name="uri">Partial URI at which to download.</param>
        private void DownloadAndImport(string uri)
        {
            if (_config.Network.AnchorDownloadFailChance > Mathf.Epsilon)
            {
                if (_Prng.NextDouble() < _config.Network.AnchorDownloadFailChance)
                {
                    Log.Warning(this, "Random anchor download failure configured by ApplicationConfig.");

                    Status = WorldAnchorStatus.IsError;
                    return;
                }
            }
            
            _bootstrapper.BootstrapCoroutine(Download(uri));
        }

        /// <summary>
        /// Downloads world anchor data via a UnityWebRequest.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private IEnumerator Download(string uri)
        {
            // metrics
            var downloadId = _metrics.Timer(MetricsKeys.ANCHOR_DOWNLOAD).Start();
            
            var formattedUrl = _http.Urls.Url(string.Format("anchors://{0}", uri));
            var request = UnityWebRequest.Get(formattedUrl);

            _downloadAbort = () =>
            {
                request.Abort();
                _downloadAbort = null;
            };

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                _metrics.Timer(MetricsKeys.ANCHOR_DOWNLOAD).Abort(downloadId);
                
                Log.Error(this,
                    "Could not download {0} : {1}.",
                    uri,
                    request.error);

                Status = WorldAnchorStatus.IsError;
            }
            else
            {
                LogVerbose("Anchor downloaded. Importing.");
                Status = WorldAnchorStatus.IsImporting;

                Import(request.downloadHandler.data);
            }
            
            _metrics.Timer(MetricsKeys.ANCHOR_DOWNLOAD).Stop(downloadId);

            _downloadAbort = null;
        }
        
        /// <summary>
        /// Imports bytes into a world anchor.
        /// </summary>
        /// <param name="bytes">The world anchor bytes.</param>
        private void Import(byte[] bytes)
        {
            LogVerbose("Bytes available, starting import.");

            var providerId = GetAnchorProviderId(Id, _versionProp.Value);

            Log.Info(this, "Bytes downloaded. Importing {0}.", providerId);

            _store
                .Import(providerId, bytes, GameObject)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "Successfully imported anchor.");

                    _pollStatus = true;
                })
                .OnFailure(exception =>
                {
                    Log.Info(this,
                        "Could not import anchor : {0}.",
                        exception);

                    Status = WorldAnchorStatus.IsError;
                });
        }
        
        /// <summary>
        /// Exports anchor data.
        /// </summary>
        private void ExportAnchorData(
            IElementTxnManager txns,
            string appId,
            string sceneId,
            byte[] bytes,
            int retries)
        {
            LogVerbose("Starting export.");

            // metrics
            var uploadId = _metrics.Timer(MetricsKeys.ANCHOR_UPLOAD).Start();
            var url = string.Format(
                "/editor/app/{0}/scene/{1}/anchor/{2}",
                appId, sceneId, Id);

            IAsyncToken<HttpResponse<Response>> token;
            if (url != Schema.Get<string>("src").Value)
            {
                LogVerbose("Creating new anchor resource.");

                // create
                token = _http.PostFile<Response>(
                    _http.Urls.Url(url),
                    new Commons.Unity.DataStructures.Tuple<string, string>[0],
                    ref bytes);
            }
            else
            {
                LogVerbose("Updating anchor resource.");

                // update
                token = _http.PutFile<Response>(
                    _http.Urls.Url(url),
                    new Commons.Unity.DataStructures.Tuple<string, string>[0],
                    ref bytes);
            }

            token
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        Log.Info(this, "Successfully uploaded anchor.");

                        // metrics
                        _metrics.Timer(MetricsKeys.ANCHOR_UPLOAD).Stop(uploadId);

                        // complete, now send out network update
                        var version = _versionProp.Value + 1;
                        txns
                            .Request(new ElementTxn(sceneId)
                                .Update(Id, "src", string.Format(
                                    "{0}.{1}.{2}.anchor",
                                    sceneId,
                                    Id,
                                    version))
                                .Update(Id, "version", version)
                                .Update(Id, "autoexport", false)
                                .Update(Id, "export.time", ""))
                            .OnSuccess(_ => _pollStatus = true)
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not set src or version on anchor : {0}", exception);

                                Status = WorldAnchorStatus.IsError;
                            });
                    }
                    else
                    {
                        Log.Error(this,
                            "Anchor upload error : {0}.",
                            response.Payload.Error);

                        // metrics
                        _metrics.Timer(MetricsKeys.ANCHOR_UPLOAD).Abort(uploadId);

                        Status = WorldAnchorStatus.IsError;
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,
                        "Could not upload anchor : {0}.",
                        exception);

                    // metrics
                    _metrics.Timer(MetricsKeys.ANCHOR_UPLOAD).Abort(uploadId);

                    if (--retries > 0)
                    {
                        Log.Info(this, "Retry uploading anchor.");

                        ExportAnchorData(txns, appId, sceneId, bytes, retries);
                    }
                    else
                    {
                        Log.Error(this, "Too many retries, cannot upload anchor.");

                        Status = WorldAnchorStatus.IsError;
                    }
                });
        }
        */
        /// <summary>
        /// Called when the file id changes.
        /// </summary>
        private void Version_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            Log.Info(this, "Version updated from {0} -> {1}.", prev, next);
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

        /// <summary>
        /// Retrieves the anchor provider id.
        /// </summary>
        /// <returns></returns>
        private static string GetAnchorProviderId(string id, int version)
        {
            return string.Format("{0}.{1}", id, version);
        }
    }
}