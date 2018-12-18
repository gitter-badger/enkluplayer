using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// Standard implementation of <c>IAssetLoader</c>.
    /// </summary>
    public class StandardAssetLoader : IAssetLoader
    {
        /// <summary>
        /// User internally to track loads.
        /// </summary>
        private class QueuedLoad
        {
            /// <summary>
            /// The data for the asset we want to load.
            /// </summary>
            public AssetData Data;

            /// <summary>
            /// The version we wish to load.
            /// </summary>
            public int Version;

            /// <summary>
            /// The loader used.
            /// </summary>
            public AssetBundleLoader Loader;

            /// <summary>
            /// Timer metric.
            /// </summary>
            public int Timer;
        }

        /// <summary>
        /// Max current downloads allowed.
        /// </summary>
        private const int MAX_CONCURRENT = 1;

        /// <summary>
        /// PRNG.
        /// </summary>
        private static readonly Random _Prng = new Random();

        /// <summary>
        /// Network configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;
        
        /// <summary>
        /// URI to loader.
        /// </summary>
        private readonly Dictionary<string, AssetBundleLoader> _bundles = new Dictionary<string, AssetBundleLoader>();

        /// <summary>
        /// Download queue.
        /// </summary>
        private readonly List<QueuedLoad> _queue = new List<QueuedLoad>();

        /// <summary>
        /// Number of downloads in progress right now.
        /// </summary>
        private int _numDownloads;

        /// <summary>
        /// Timer id for a non-empty queue.
        /// </summary>
        private int _queueNonEmptyId;

        /// <inheritdoc />
        public UrlFormatterCollection Urls { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardAssetLoader(
            ApplicationConfig config,
            IBootstrapper bootstrapper,
            IMetricsService metrics,
            UrlFormatterCollection urls)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _metrics = metrics;
            
            Urls = urls;
            
            _bootstrapper.BootstrapCoroutine(ProcessQueue());
        }
        
        /// <inheritdoc />
        public IAsyncToken<Object> Load(
            AssetData data,
            int version,
            out LoadProgress progress)
        {
            // see if this load should fail (for testing porpoises)
            var failChance = _config.Network.AssetDownloadFailChance;
            if (failChance > Mathf.Epsilon)
            {
                if (_Prng.NextDouble() < failChance)
                {
                    progress = new LoadProgress();
                    return new AsyncToken<Object>(new Exception("Random failure configured by ApplicationConfig."));
                }
            }
            
            var url = Url(data, version);
            var token = new AsyncToken<Object>();
            
            // create a loader if one doesn't exist
            AssetBundleLoader loader;
            if (!_bundles.TryGetValue(url, out loader))
            {
                Log.Info(this, "Adding {0} to the queue, length of {1}.",
                    data.Guid,
                    _queue.Count);

                // timer how long this is in the queue
                var timer = _metrics.Timer(MetricsKeys.ASSET_DL_QUEUE);

                loader = _bundles[url] = new AssetBundleLoader(
                    _config.Network,
                    _bootstrapper,
                    url);

                // add loader to queue
                var queuedLoad = new QueuedLoad
                {
                    Loader = loader,
                    Data = data,
                    Version = version,
                    Timer = timer.Start()
                };

                if (_queue.Count == 0)
                {
                    _queueNonEmptyId = _metrics.Timer(MetricsKeys.ASSET_DL_QUEUE_NONEMPTY).Start();
                }
                
                _queue.Add(queuedLoad);

                _metrics.Counter(MetricsKeys.ASSET_DL_QUEUE_LENGTH).Increment();
            }

            // load from loader
            loader
                .Asset(AssetName(data), out progress)
                .OnSuccess(token.Succeed)
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc />
        public void ClearDownloadQueue()
        {
            Log.Info(this, "Clear download queue.");

            // remove all queued loads
            while (_queue.Count > 0)
            {
                var record = _queue[0];

                _queue.RemoveAt(0);
                _bundles.Remove(Url(record.Data, record.Version));
            }
        }

        /// <inheritdoc />
        public void Destroy()
        {
            foreach (var pair in _bundles)
            {
                pair.Value.Destroy();
            }
            _bundles.Clear();
        }

        /// <summary>
        /// Checks the queue every frame. We wait for frame updates rather
        /// than immediately moving to the next in the queue so that we can
        /// ensure WebGL GC can have a chance to run.
        /// </summary>
        private IEnumerator ProcessQueue()
        {
            // runs for the lifetime of the application
            while (true)
            {
                while (_numDownloads < MAX_CONCURRENT && _queue.Count > 0)
                {
                    var next = _queue[0];
                    _queue.RemoveAt(0);

                    _metrics.Counter(MetricsKeys.ASSET_DL_QUEUE_LENGTH).Decrement();

                    if (0 == _queue.Count)
                    {
                        _metrics.Timer(MetricsKeys.ASSET_DL_QUEUE_NONEMPTY).Stop(_queueNonEmptyId);
                    }

                    // record metrics
                    _metrics.Timer(MetricsKeys.ASSET_DL_QUEUE).Stop(next.Timer);

                    Log.Info(this, "Starting next load : {0}", next.Data.AssetName);

                    var timer = _metrics.Timer(MetricsKeys.ASSET_DL_LOADING);
                    var timerId = timer.Start();

                    _numDownloads++;
                    next.Loader
                        .Load()
                        // record metrics
                        .OnSuccess(_ => timer.Stop(timerId))
                        .OnFailure(ex =>
                        {
                            // remove so we can allow retries
                            _bundles.Remove(Url(next.Data, next.Version));

                            // abort metrics
                            timer.Abort(timerId);
                        })
                        .OnFinally(_=>
                        {
                            _numDownloads--;
                        });
                }

                yield return true;
            }
        }

        /// <summary>
        /// Creates a URL from asset data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private string Url(AssetData data, int version)
        {
            var uri = data.Uri;

            // try to insert version
            var pieces = data.Uri.Split('.');
            if (pieces.Length == 3)
            {
                uri = string.Format("{0}.{1}.{2}",
                    pieces[0],
                    version,
                    pieces[2]);
            }

            return Urls.Url("assets://" + uri);
        }

        /// <summary>
        /// Determines an asset's name from the <c>AssetData</c>.
        /// </summary>
        /// <param name="data">The data to find the name for.</param>
        /// <returns></returns>
        private static string AssetName(AssetData data)
        {
            var path = Path.GetFileNameWithoutExtension(data.Uri);
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            return path.Split('_')[0];
        }
    }
}