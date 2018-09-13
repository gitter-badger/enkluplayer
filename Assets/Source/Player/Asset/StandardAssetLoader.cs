using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
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
        /// URI to loader.
        /// </summary>
        private readonly Dictionary<string, AssetBundleLoader> _bundles = new Dictionary<string, AssetBundleLoader>();

        /// <summary>
        /// Download queue.
        /// </summary>
        private readonly Queue<Action> _queue = new Queue<Action>();

        /// <summary>
        /// Number of downloads in progress right now.
        /// </summary>
        private int _numDownloads;

        /// <inheritdoc />
        public UrlFormatterCollection Urls { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StandardAssetLoader(
            ApplicationConfig config,
            IBootstrapper bootstrapper,
            UrlFormatterCollection urls)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            
            Urls = urls;

            _bootstrapper.BootstrapCoroutine(ProcessQueue());
        }
        
        /// <inheritdoc />
        public IAsyncToken<Object> Load(
            AssetData data,
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
            
            var substrings = data.Uri.Split('/');
            var url = Urls.Url("assets://" + substrings[substrings.Length - 1]);
            var externalProgress = progress = new LoadProgress();

            var token = new AsyncToken<Object>();

            Action startLoad = () =>
            {
                _numDownloads++;

                AssetBundleLoader loader;
                if (!_bundles.TryGetValue(url, out loader))
                {
                    loader = _bundles[url] = new AssetBundleLoader(
                        _config.Network,
                        _bootstrapper,
                        url);
                    loader.Load();
                }

                // AssetImportServer uses the Guid
                LoadProgress internalProgress;
                loader
                    .Asset(data.Guid, out internalProgress)
                    .OnSuccess(token.Succeed)
                    .OnFailure(token.Fail)
                    .OnFinally(_ => _numDownloads--);

                internalProgress.Chain(externalProgress);
            };

            _queue.Enqueue(startLoad);
            
            return token;
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
        /// ensure WebGL GC can have a chance to eval.
        /// </summary>
        private IEnumerator ProcessQueue()
        {
            // runs for the lifetime of the application
            while (true)
            {
                while (_numDownloads < MAX_CONCURRENT && _queue.Count > 0)
                {
                    _queue.Dequeue()();
                }

                yield return true;
            }
        }
    }
}