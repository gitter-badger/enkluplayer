using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine.Networking;

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
        /// Makes Http requests.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardScriptLoader(
            NetworkConfig config,
            IScriptCache cache,
            IHttpService http,
            IMetricsService metrics,
            IBootstrapper bootstrapper)
        {
            _config = config;
            _cache = cache;
            _http = http;
            _metrics = metrics;
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc cref="IScriptLoader"/>
        public IAsyncToken<string> Load(ScriptData script)
        {
            var token = new AsyncToken<string>();
            
            if (_cache.Contains(script.Id, script.Version))
            {
                var id = _metrics.Timer(MetricsKeys.SCRIPT_LOADFROMCACHETIME).Start();

                _cache
                    .Load(script.Id, script.Version)
                    .OnSuccess(txt =>
                    {
                        _metrics.Timer(MetricsKeys.SCRIPT_LOADFROMCACHETIME).Stop(id);

                        token.Succeed(txt);
                    })
                    .OnFailure(exception =>
                    {
                        _metrics.Timer(MetricsKeys.SCRIPT_LOADFROMCACHETIME).Abort(id);

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

            var url = _http.Urls.Url(PrepUri(script.Uri));

            Log.Info(this, "Downloading script at {0}.", url);
            
            _bootstrapper.BootstrapCoroutine(Download(url, token));

            return token;
        }

        /// <summary>
        /// Downloads script.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="token">The token to complete.</param>
        /// <returns></returns>
        private IEnumerator Download(string url, AsyncToken<string> token)
        {
            var id = _metrics.Timer(MetricsKeys.SCRIPT_DOWNLOADTIME).Start();

            var req = UnityWebRequest.Get(url);

            yield return req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
            {
                _metrics.Timer(MetricsKeys.SCRIPT_DOWNLOADTIME).Abort(id);

                token.Fail(new Exception(req.error));
            }
            else
            {
                _metrics.Timer(MetricsKeys.SCRIPT_DOWNLOADTIME).Stop(id);

                Log.Info(this, "Downloaded script: {0}.", req.downloadHandler.text);

                token.Succeed(req.downloadHandler.text);
            }
        }

        /// <summary>
        /// Creates URI from script data.
        /// </summary>
        /// <param name="uri">The uri from script data.</param>
        /// <returns></returns>
        private string PrepUri(string uri = "")
        {
            var index = uri.IndexOf("://", StringComparison.Ordinal);
            if (-1 == index)
            {
                return string.Empty;
            }

            int version;
            if (!int.TryParse(uri.Substring(1, index - 1), out version))
            {
                return string.Empty;
            }

            if (3 != version)
            {
                Log.Warning(this, "Unknown script version in uri : {0}", uri);

                return string.Empty;
            }

            return string.Format("scripts://{0}", uri.Substring(index + 3));
        }
    }
}