using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;

using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Loads asset bundles.
    /// </summary>
    public class AssetBundleLoader
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Caches bundles.
        /// </summary>
        private readonly IAssetBundleCache _cache;

        /// <summary>
        /// The URL being loaded.
        /// </summary>
        private readonly string _url;

        /// <summary>
        /// True iff the loader has been destroyed.
        /// </summary>
        private bool _destroy;

        /// <summary>
        /// The load.
        /// </summary>
        private IAsyncToken<AssetBundle> _bundleLoad;
        
        /// <summary>
        /// The bundle that was loaded.
        /// </summary>
        public AssetBundle Bundle { get; private set; }

        /// <summary>
        /// The progress of the load.
        /// </summary>
        public readonly LoadProgress Progress = new LoadProgress();

        /// <summary>
        /// Constructor.
        /// </summary>
        public AssetBundleLoader(
            IBootstrapper bootstrapper,
            IAssetBundleCache cache,
            string url)
        {
            _bootstrapper = bootstrapper;
            _cache = cache;
            _url = url;
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        public void Destroy()
        {
            Trace("Destroy");
            
            _destroy = true;

            if (null != Bundle)
            {
                Bundle.Unload(true);
                
                Trace("Bundle unloaded.");
            }
        }

        /// <summary>
        /// Loads the bundle.
        /// </summary>
        public void Load()
        {
            // check cache
            Trace("Checking cache.");

            if (false && _cache.Contains(_url))
            {
                Trace("Cache hit.");

                LoadProgress progress;
                _bundleLoad = _cache.Load(_url, out progress);
                
                progress.Chain(Progress);
            }
            else
            {
                Trace("Cache miss.");
                
                _bootstrapper.BootstrapCoroutine(DownloadBundle());
            }
        }

        /// <summary>
        /// Retrieves an asset from the bundle.
        /// </summary>
        /// <param name="assetName">The name of the asset.</param>
        /// <param name="progress">The progress of the load.</param>
        /// <returns></returns>
        public IAsyncToken<Object> Asset(string assetName, out LoadProgress progress)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentException(assetName);
            }

            Trace("Load Asset {0}.", assetName);

            var token = new AsyncToken<Object>();
            var load = new LoadProgress();

            _bundleLoad
                .OnSuccess(bundle =>
                {
                    Trace("Load Asset {0}: bundle load complete.", assetName);

                    Bundle = bundle;
                    
                    var request = Bundle.LoadAssetAsync(assetName);
                    _bootstrapper.BootstrapCoroutine(WaitForLoadAsset(
                        request,
                        token,
                        load));
                })
                .OnFailure(token.Fail);

            progress = load;
            return token;
        }
        
        /// <summary>
        /// Retrieves an asset from the bundle.
        /// </summary>
        /// <param name="assetName">The name of the asset.</param>
        /// <returns></returns>
        public IAsyncToken<Object> Asset(string assetName)
        {
            LoadProgress progress;
            return Asset(assetName, out progress);
        }

        /// <summary>
        /// Loads the asset bundle.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DownloadBundle()
        {
            var token = new AsyncToken<AssetBundle>();
            _bundleLoad = token;

            /*var request = new UnityWebRequest(
                _url,
                "GET",
                new AssetBundleDownloadHandler(_bootstrapper, _url),
                null);
            
            request.SendWebRequest();
            while (!request.isDone)
            {
                Progress.Value = request.downloadProgress;

                yield return null;
            }*/

            var request = UnityWebRequest.GetAssetBundle(_url);
            yield return request.SendWebRequest();
            
            Trace("DownloadBundle complete.");

            if (_destroy)
            {
                request.Dispose();
                
                Trace("Loader was destroyed when load came back. Disposing bundle.");
                
                yield break;
            }

            if (request.isNetworkError || request.isHttpError)
            {
                Trace("Network or Http error: {0}.", request.error);
                
                token.Fail(new Exception(request.error));
            }
            else
            {
                // wait for bundle to complete
                /*
                var handler = (AssetBundleDownloadHandler) request.downloadHandler;
                handler
                    .OnReady
                    .OnSuccess(bundle =>
                    {
                        _cache.Save(_url, handler.data);

                        token.Succeed(bundle);
                    })
                    .OnFailure(token.Fail);*/

                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (null != bundle)
                {
                    token.Succeed(bundle);
                }
                else
                {
                    token.Fail(new Exception("Could not create bundle."));
                }
            }

            request.Dispose();
        }

        /// <summary>
        /// Waits for the asset to be loaded.
        /// </summary>
        /// <param name="request">The Unity request.</param>
        /// <param name="token">The token to resolve.</param>
        /// <param name="progress">The progress of the load.</param>
        /// <returns></returns>
        private IEnumerator WaitForLoadAsset(
            AssetBundleRequest request,
            AsyncToken<Object> token,
            LoadProgress progress)
        {
            Log.Info(this, "Loading asset from bundle.");

            while (!request.isDone)
            {
                progress.Value = request.progress;

                yield return null;
            }

            // set it explicitly
            progress.Value = 1f;

            var asset = request.asset;
            if (null == asset)
            {
                Log.Error(this, "Could not find asset in bundle.");

                token.Fail(new Exception("Could not find asset."));
            }
            else
            {
                Log.Info(this, "Found asset in bundle.");

                token.Succeed(asset);
            }
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="replacements">Logging replacements.</param>
        private void Trace(string message, params object[] replacements)
        {
            Log.Info(this,
                "[{0}] {1}",
                _url,
                string.Format(message, replacements));
        }
    }
}