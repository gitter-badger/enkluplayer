using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Implementation of <c>IScriptLoader</c>.
    /// </summary>
    public class StandardScriptLoader : IScriptLoader
    {
        /// <summary>
        /// Network configuration.
        /// </summary>
        private readonly NetworkConfig _config;

        /// <summary>
        /// Script cache.
        /// </summary>
        private readonly IScriptCache _cache;

        /// <summary>
        /// Makes Http reqs.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardScriptLoader(
            NetworkConfig config,
            IScriptCache cache,
            IHttpService http)
        {
            _config = config;
            _cache = cache;
            _http = http;
        }

        /// <inheritdoc cref="IScriptLoader"/>
        public IAsyncToken<string> Load(ScriptData script)
        {
            var token = new AsyncToken<string>();
            
            if (_cache.Contains(script.Id, script.Version))
            {
                _cache
                    .Load(script.Id, script.Version)
                    .OnSuccess(token.Succeed)
                    .OnFailure(exception =>
                    {
                        Log.Error(
                            this,
                            "There was an error loading the script from disk : {0}. Attempting to load from network..",
                            exception);

                        LoadScriptFromNetwork(script)
                            .OnSuccess(source =>
                            {
                                _cache.Save(script.Id, script.Version, source);

                                token.Succeed(source);
                            })
                            .OnFailure(token.Fail);
                    });
            }
            else
            {
                LoadScriptFromNetwork(script)
                    .OnSuccess(source =>
                    {
                        _cache.Save(script.Id, script.Version, source);

                        token.Succeed(source);
                    })
                    .OnFailure(token.Fail);
            }

            return token;
        }

        /// <summary>
        /// Loads the script from the network.
        /// </summary>
        /// <param name="script">The script to load.</param>
        /// <returns></returns>
        private IAsyncToken<string> LoadScriptFromNetwork(ScriptData script)
        {
            var token = new AsyncToken<string>();

            if (_config.Offline)
            {
                token.Fail(new Exception("Offline."));

                return token;
            }

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