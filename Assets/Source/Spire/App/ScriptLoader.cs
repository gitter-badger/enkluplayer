using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Implementation of <c>IScriptLoader</c>.
    /// </summary>
    public class ScriptLoader : IScriptLoader
    {
        /// <summary>
        /// Makes Http reqs.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScriptLoader(IHttpService http)
        {
            _http = http;
        }

        /// <inheritdoc cref="IScriptLoader"/>
        public IAsyncToken<string> Load(ScriptData script)
        {
            var token = new AsyncToken<string>();

            var url = _http.Urls.Url("trellis://" + script.Uri);
            
            Log.Info(this, "Downloading script at {0}.", url);
            
            _http
                .Download(url)
                .OnSuccess(response =>
                {
                    if (response.NetworkSuccess)
                    {
                        token.Succeed(Encoding.UTF8.GetString(response.Payload));
                    }
                    else
                    {
                        Log.Error(this, "Could not download script at {0} : {1}.",
                            url,
                            response.NetworkError);

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