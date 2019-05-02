using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using Enklu.Data;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
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

            var url = string.Format(
                "trellis://editor/app/{0}/scene/{1}",
                appId,
                sceneId);

            _http
                .Put<ElementTxnResponse>(
                    url,
                    new ElementTxnRequest(id)
                    {
                        Actions = actions
                    })
                .OnSuccess(response =>
                {
                    if (null == response.Payload)
                    {
                        token.Fail(new Exception(string.Format(
                            "Could not deserialize payload : [{0}].",
                            Encoding.UTF8.GetString(response.Raw))));
                    }
                    else if (response.Payload.Success)
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