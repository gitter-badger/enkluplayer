using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Trellis.Messages;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Implementation on top of HTTP.
    /// </summary>
    public class HttpElementTxnTransport : IElementTxnTransport
    {
        /// <summary>
        /// Api.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Makes Http services.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpElementTxnTransport(
            ApiController api,
            IHttpService http)
        {
            _api = api;
            _http = http;
        }

        /// <inheritdoc />
        public IAsyncToken<Trellis.Messages.GetApp.Response> GetApp(string appId)
        {
            var token = new AsyncToken<Trellis.Messages.GetApp.Response>();

            _api
                .Apps
                .GetApp(appId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        token.Succeed(response.Payload);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Trellis.Messages.GetScene.Response> GetScene(string appId, string sceneId)
        {
            var token = new AsyncToken<Trellis.Messages.GetScene.Response>();

            _api
                .Scenes
                .GetScene(appId, sceneId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        token.Succeed(response.Payload);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Request(string appId, string sceneId, ElementActionData[] actions)
        {
            var token = new AsyncToken<Void>();

            _http
                .Put<ElementTxnResponse>(
                    _http.UrlBuilder.Url(string.Format(
                        "/editor/app/{0}/scene/{1}",
                        appId,
                        sceneId)),
                    new ElementTxnRequest
                    {
                        Actions = actions
                    })
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }
    }
}