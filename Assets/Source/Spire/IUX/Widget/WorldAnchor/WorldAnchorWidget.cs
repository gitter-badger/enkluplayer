using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
        private ElementSchemaProp<string> _anchorProp;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WorldAnchorWidget(
            GameObject gameObject,
            WidgetConfig config,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors,
            IMessageRouter messages,
            IHttpService http,
            IWorldAnchorProvider provider)
            : base(gameObject, config, layers, tweens, colors, messages)
        {
            _http = http;
            _provider = provider;
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();
            
            _anchorProp = Schema.GetOwn("anchorSrc", string.Empty);
            _anchorProp.OnChanged += Anchor_OnChanged;

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

            _anchorProp.OnChanged -= Anchor_OnChanged;
        }

        /// <summary>
        /// Reloads the world anchor.
        /// </summary>
        private void UpdateWorldAnchor()
        {
            if (null != _downloadToken)
            {
                _downloadToken.Abort();
                _downloadToken = null;
            }

            var url = _anchorProp.Value;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            _downloadToken = _http
                .Download(_anchorProp.Value)
                .OnSuccess(response => _provider.Import(GameObject, response.Payload))
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
        /// Called when the anchor URL changes.
        /// </summary>
        private void Anchor_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            UpdateWorldAnchor();
        }
    }
}