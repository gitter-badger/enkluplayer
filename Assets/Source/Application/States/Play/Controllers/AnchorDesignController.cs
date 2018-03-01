using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class AnchorDesignController : MonoBehaviour
    {
        private const float SAVE_MIN_SECS = 3f;

        private PlayModeConfig _config;
        private IWorldAnchorProvider _provider;
        private IHttpService _http;

        private GameObject _marker;

        private Material[] _materials;

        private bool _isUpdateEnabled = false;

        private DateTime _lastSave = DateTime.MinValue;
        private AsyncToken<Void> _exportToken;

        public WorldAnchorWidget Element { get; private set; }

        public bool IsVisualEnabled
        {
            get
            {
                return _marker.activeSelf;
            }
            set
            {
                _marker.SetActive(value);
            }
        }

        public void Initialize(
            PlayModeConfig config,
            WorldAnchorWidget element,
            IWorldAnchorProvider provider,
            IHttpService http)
        {
            _config = config;
            _provider = provider;
            _http = http;

            Element = element;

            _marker = Instantiate(_config.AnchorPrefab, transform);
            _marker.transform.localPosition = Vector3.zero;
            _marker.transform.localRotation = Quaternion.identity;

            var materials = new List<Material>();
            var renderers = _marker.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0, len = renderers.Length; i < len; i++)
            {
                materials.AddRange(renderers[i].sharedMaterials);
            }

            _materials = materials.ToArray();
        }

        private void Update()
        {
            if (!_isUpdateEnabled)
            {
                return;
            }
            
            if (IsDirty() && CanExport())
            {
                Export();
            }
        }

        private bool IsDirty()
        {
            return false;
        }

        private bool CanExport()
        {
            return null == _exportToken;
        }

        private void Export()
        {
            _exportToken = new AsyncToken<Void>();

            // first, export anchor
            _provider
                .Export(Element.GameObject)
                .OnSuccess(bytes =>
                {
                    // next, upload anchor
                    _http
                        .PostFile<Trellis.Messages.UploadAnchor.Response>(
                            _http.UrlBuilder.Url(string.Format(
                                "/v1/editor/app/{0}/scene/{1}/anchor/{2}",
                                "appId",
                                "sceneId",
                                Element.Id)),
                            new Commons.Unity.DataStructures.Tuple<string, string>[0],
                            ref bytes)
                        .OnSuccess(response =>
                        {
                            if (response.Payload.Success)
                            {
                                Log.Info(this, "Successfully uploaded world anchor.");
                            }
                            else
                            {
                                Log.Error(this, "Could not upload world anchor : {0}.", response.Payload.Error);
                            }
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this,
                                "Could not upload world anchor : {0}.",
                                exception);
                        });
                })
                .OnFailure(exception =>
                {
                    Log.Error(this,"Could not export anchor for {0} : {1}.",
                        Element,
                        exception);

                    var token = _exportToken;
                    _exportToken = null;

                    token.Fail(exception);
                });
        }
    }
}