using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class ScriptLoader : IScriptLoader
    {
        private readonly IHttpService _http;

        public ScriptLoader(IHttpService http)
        {
            _http = http;
        }

        public IAsyncToken<string> Load(ScriptData script)
        {
            var token = new AsyncToken<string>();

            var url = script.Uri;

            _http
                .Download(_http.UrlBuilder.Url(url))
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess)
                    {
                        token.Succeed(Encoding.UTF8.GetString(response.Payload));
                    }
                    else
                    {
                        token.Fail(new Exception(response.NetworkError));
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not download script at {0} : {1}.",
                        url,
                        exception);

                    token.Fail(exception);
                });

            return token;
        }
    }
}