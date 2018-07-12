﻿using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// World mesh scan.
    /// </summary>
    public class ScanWidget : Widget
    {
        /// <summary>
        /// HTTP interface.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Imports meshes.
        /// </summary>
        private readonly MeshImporter _importer;

        /// <summary>
        /// Url to download from.
        /// </summary>
        private ElementSchemaProp<string> _srcUrlProp;
        private ElementSchemaProp<bool> _hideProp;

        /// <summary>
        /// In progress download.
        /// </summary>
        private IAsyncToken<HttpResponse<byte[]>> _meshDownload;

        /// <summary>
        /// GameObject the importer uses.
        /// </summary>
        private GameObject _meshCaptureGameObject;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ScanWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IHttpService http,
            MeshImporter importer)
            : base(
                gameObject,
                layers,
                tweens,
                colors)
        {
            _http = http;
            _importer = importer;
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            _srcUrlProp = Schema.Get<string>("srcUrl");
            _srcUrlProp.OnChanged += MeshCapture_OnChanged;
            UpdateMeshCapture();

            _hideProp = Schema.GetOwn("hide", false);
            _hideProp.OnChanged += Hide_OnChanged;
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _srcUrlProp.OnChanged -= MeshCapture_OnChanged;

            if (null != _meshDownload)
            {
                _meshDownload.Abort();
                _meshDownload = null;
            }

            if (null != _meshCaptureGameObject)
            {
                Object.Destroy(_meshCaptureGameObject);
                _meshCaptureGameObject = null;
            }
        }

        /// <summary>
        /// Updates mesh capture settings.
        /// </summary>
        private void UpdateMeshCapture()
        {
            if (null != _meshDownload)
            {
                _meshDownload.Abort();
            }

            if (string.IsNullOrEmpty(_srcUrlProp.Value))
            {
                Log.Info(this, "No mesh capture to download.");
                return;
            }

            var url = _http.Urls.Url("meshcapture://" + _srcUrlProp.Value);

            Log.Info(this, "Downloading mesh capture from {0}...", url);

            // download
            _meshDownload = _http
                .Download(url)
                .OnSuccess(response =>
                {
                    Log.Info(this, "Mesh capture download complete. Starting import.");

                    if (null != _meshCaptureGameObject)
                    {
                        Object.Destroy(_meshCaptureGameObject);
                    }

                    _meshCaptureGameObject = new GameObject("MeshCapture");
                    _meshCaptureGameObject.transform.SetParent(GameObject.transform);
                    _meshCaptureGameObject.SetActive(!_hideProp.Value);

                    // import
                    _importer.Import(response.Payload, (exception, action) =>
                    {
                        if (null != exception)
                        {
                            Log.Error(this, "Could not import mesh : {0}", exception);
                            return;
                        }

                        if (null == _meshCaptureGameObject)
                        {
                            return;
                        }

                        Log.Info(this, "Import complete. Constructing mesh.");

                        var bounds = action(_meshCaptureGameObject);

                        // update collider with new bounds
                        var collider = GameObject.GetComponent<BoxCollider>();
                        if (null == collider)
                        {
                            collider = GameObject.AddComponent<BoxCollider>();
                        }

                        collider.center = bounds.center;
                        collider.size = bounds.size;
                    });
                })
                .OnFailure(exception => Log.Error(this, "Could not download mesh capture : {0}", exception));
        }

        /// <summary>
        /// Updates visibility of mesh.
        /// </summary>
        private void UpdateMeshVisibility()
        {
            if (null != _meshCaptureGameObject)
            {
                _meshCaptureGameObject.SetActive(!_hideProp.Value);
            }
        }

        /// <summary>
        /// Called when mesh capture has changed.
        /// </summary>
        private void MeshCapture_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateMeshCapture();
        }

        /// <summary>
        /// Called when hide prop has changed.
        /// </summary>
        private void Hide_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            UpdateMeshVisibility();
        }
    }
}