using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Implementation of <c>IScriptLoader</c>.
    /// </summary>
    public class StandardScriptLoader : IScriptLoader
    {
        /// <summary>
        /// Information about a Script failing to load.
        /// </summary>
        public struct ScriptLoadFailure
        {
            /// <summary>
            /// The ScriptData that failed.
            /// </summary>
            public ScriptData ScriptData;
        
            /// <summary>
            /// The Exception causing failure.
            /// </summary>
            public Exception Exception;
        }
        
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
        /// Pseudo random number generator.
        /// </summary>
        private static readonly Random _Prng = new Random();
        
        /// <summary>
        /// The number of currently loading scripts.
        /// </summary>
        public int QueueLength { get; private set; }
        
        /// <summary>
        /// A collection of load failures this IScriptLoader experienced.
        /// </summary>
        public List<ScriptLoadFailure> LoadFailures { get; private set; }

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
            
            LoadFailures = new List<ScriptLoadFailure>();
        }

        /// <inheritdoc cref="IScriptLoader"/>
        public IAsyncToken<string> Load(ScriptData script)
        {
            QueueLength++;

            var failChance = _config.ScriptDownloadFailChance;
            if (failChance > Mathf.Epsilon)
            {
                if (_Prng.NextDouble() < failChance)
                {
                    QueueLength--;
                    
                    var exception = new Exception("Random failure configured by ApplicationConfig.");
                    LoadFailures.Add(new ScriptLoadFailure
                    {
                        ScriptData = script,
                        Exception = exception
                    });
                    return new AsyncToken<string>(exception);
                }
            }
            
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
                        LoadFailures.Add(new ScriptLoadFailure
                        {
                            ScriptData = script,
                            Exception = exception
                        });

                        LoadScriptFromNetwork(script)
                            .OnSuccess(source =>
                            {
                                _cache.Save(script.Id, script.Version, source);

                                token.Succeed(source);
                            })
                            .OnFailure(token.Fail);
                    })
                    .OnFinally(_ => QueueLength--);
            }
            else
            {
                LoadScriptFromNetwork(script)
                    .OnSuccess(source =>
                    {
                        _cache.Save(script.Id, script.Version, source);

                        token.Succeed(source);
                    })
                    .OnFailure(exception =>
                    {
                        LoadFailures.Add(new ScriptLoadFailure
                        {
                            ScriptData = script,
                            Exception = exception
                        });
                        token.Fail(exception);
                    })
                    .OnFinally(_ => QueueLength--);
            }

            return token;
        }

        /// <inheritdoc />
        public void Clear()
        {
            QueueLength = 0;
            LoadFailures.Clear();
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

            var start = DateTime.Now;
            var req = UnityWebRequest.Get(url);
            req.SendWebRequest();

            while (!req.isDone)
            {
                if (_http.TimeoutMs > 0 && DateTime.Now.Subtract(start).TotalMilliseconds > _http.TimeoutMs)
                {
                    // request timed out
                    req.Dispose();

                    token.Fail(new Exception("Request timed out."));

                    yield break;
                }

                yield return null;
            }

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