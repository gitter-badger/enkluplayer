using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Authenticates txns over HTTP.
    /// </summary>
    public class HttpAppTxnAuthenticator : IAppTxnAuthenticator
    {
        /// <summary>
        /// Makes Http services.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HttpAppTxnAuthenticator(IHttpService service)
        {
            _http = service;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Request(int id, string appId, string sceneId, ElementActionData[] actions)
        {
            var token = new AsyncToken<Void>();

            _http
                .Put<ElementTxnResponse>(
                    _http.Urls.Url(string.Format(
                        "trellis://editor/app/{0}/scene/{1}",
                        appId,
                        sceneId)),
                    new ElementTxnRequest(id)
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