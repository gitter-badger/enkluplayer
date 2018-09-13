using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Trellis.Messages;

namespace CreateAR.EnkluPlayer
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
        /// Constructor.
        /// </summary>
        public HttpElementTxnTransport(ApiController api)
        {
            _api = api;
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
    }
}