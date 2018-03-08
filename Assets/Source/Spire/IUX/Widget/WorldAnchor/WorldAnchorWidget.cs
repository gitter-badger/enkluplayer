using System;
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
        /// <summary>
        /// For downloading anchors.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Abstracts anchoring method.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Token for anchor download.
        /// </summary>
        private IAsyncToken<HttpResponse<byte[]>> _downloadToken;

        /// <summary>
        /// Prop for anchoring url.
        /// </summary>
        private ElementSchemaProp<int> _versionProp;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldAnchorWidget(
            GameObject gameObject,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IHttpService http,
            IWorldAnchorProvider provider)
            : base(gameObject, layers, tweens, colors)
        {
            _http = http;
            _provider = provider;
        }

        /// <summary>
        /// Imports anchor.
        /// </summary>
        public IAsyncToken<Void> Import()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();
            
            _versionProp = Schema.GetOwn("version", -1);
            _versionProp.OnChanged += Version_OnChanged;

            UpdateWorldAnchor();
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
        private void UpdateWorldAnchor()
        {
            // abort previous
            if (null != _downloadToken)
            {
                _downloadToken.Abort();
                _downloadToken = null;
            }

            var version = _versionProp.Value;
            if (-1 == version)
            {
                // anchor has not completed initial upload yet
                return;
            }

            var url = string.Format(
                "/v1/editor/app/{0}/scene/{1}/anchor/{2}",
                "appId",
                "sceneId",
                Id);

            _downloadToken = _http
                .Download(_http.UrlBuilder.Url(url))
                .OnSuccess(response =>
                {
                    Log.Info(this, "Anchor downloaded. Importing.");

                    _provider
                        .Import(GameObject, response.Payload)
                        .OnSuccess(_ => Log.Info(this, "Successfully imported anchor."))
                        .OnFailure(exception => Log.Error(this,
                            "Could not import anchor : {0}.",
                            exception));
                })
                .OnFailure(exception => Log.Error(this, 
                    "Could not download {0} : {1}.",
                    url,
                    exception))
                .OnFinally(token =>
                {
                    if (token == _downloadToken)
                    {
                        _downloadToken = null;
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
    }
}