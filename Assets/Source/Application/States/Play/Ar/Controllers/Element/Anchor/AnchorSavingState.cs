using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for anchor save.
    /// </summary>
    public class AnchorSavingState : IState
    {
        /// <summary>
        /// The controller.
        /// </summary>
        private readonly AnchorDesignController _controller;

        /// <summary>
        /// Exports anchor data.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Token for export.
        /// </summary>
        private IAsyncToken<byte[]> _exportToken;

        /// <summary>
        /// Token for upload.
        /// </summary>
        private IAsyncToken<HttpResponse<Trellis.Messages.UploadAnchor.Response>> _uploadToken;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AnchorSavingState(
            AnchorDesignController controller,
            IWorldAnchorProvider provider,
            IHttpService http)
        {
            _controller = controller;
            _provider = provider;
            _http = http;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _controller.Lock();
            _controller.Color = Color.yellow;

            Export();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {

        }

        /// <inheritdoc />
        public void Exit()
        {
            if (null != _exportToken)
            {
                _exportToken.Abort();
                _exportToken = null;
            }

            if (null != _uploadToken)
            {
                _uploadToken.Abort();
                _uploadToken = null;
            }
        }
        
        /// <summary>
        /// Exports + uploads.
        /// </summary>
        private void Export()
        {
            // first, export anchor
            _exportToken = _provider
                .Export(_controller.gameObject)
                .OnSuccess(bytes =>
                {
                    // next, upload anchor
                    _uploadToken = _http
                        .PostFile<Trellis.Messages.UploadAnchor.Response>(
                            _http.UrlBuilder.Url(string.Format(
                                "/v1/editor/app/{0}/scene/{1}/anchor/{2}",
                                "appId",
                                "sceneId",
                                _controller.Element.Id)),
                            new Commons.Unity.DataStructures.Tuple<string, string>[0],
                            ref bytes)
                        .OnSuccess(response =>
                        {
                            if (response.Payload.Success)
                            {
                                Log.Info(this, "Successfully uploaded world anchor.");

                                // TODO: next update version

                                _controller.ChangeState<AnchorReadyState>();
                            }
                            else
                            {
                                Log.Error(this, "Could not upload world anchor : {0}.", response.Payload.Error);

                                _controller.ChangeState<AnchorErrorState>();
                            }
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this,
                                "Could not upload world anchor : {0}.",
                                exception);

                            _controller.ChangeState<AnchorErrorState>();
                        });
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not export anchor for {0} : {1}.",
                        _controller,
                        exception);

                    _controller.ChangeState<AnchorErrorState>();
                });
        }
    }
}