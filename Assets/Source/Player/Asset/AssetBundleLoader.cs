using System;
using System.Collections;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer.Assets
{
    /// <summary>
    /// Loads asset bundles.
    /// </summary>
    public class AssetBundleLoader
    {
        /// <summary>
        /// Configuration for networking.
        /// </summary>
        private readonly NetworkConfig _config;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
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
            NetworkConfig config,
            IBootstrapper bootstrapper,
            string url)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _url = url;
        }

        /// <summary>
        /// Frees resources.
        /// </summary>
        public void Destroy()
        {
            Verbose("Destroy");
            
            _destroy = true;

            if (null != Bundle)
            {
                Bundle.Unload(true);
                
                Verbose("Bundle unloaded.");
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

            if (null == _bundleLoad)
            {
                Load();
            }

            Verbose("Load Asset {0}.", assetName);

            var token = new AsyncToken<Object>();
            var load = new LoadProgress();

            _bundleLoad
                .OnSuccess(bundle =>
                {
                    Verbose("Load Asset {0}: bundle load complete.", assetName);

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
        /// Loads the bundle.
        /// </summary>
        private void Load()
        {
            _bootstrapper.BootstrapCoroutine(DownloadBundle());
        }

        /// <summary>
        /// Loads the asset bundle.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DownloadBundle()
        {
            var token = new AsyncToken<AssetBundle>();
            _bundleLoad = token;

            // artificial lag
            if (_config.AssetDownloadLagSec > Mathf.Epsilon)
            {
                yield return new WaitForSecondsRealtime(_config.AssetDownloadLagSec);
            }

            // offline mode
            if (_config.Offline)
            {
                token.Fail(new Exception("Could not load asset: Offline Mode enabled."));
                yield break;
            }

#if FALSE && !NETFX_CORE
            Verbose("Download bundle from {0}.", _url);
            
            var request = WWW.LoadFromCacheOrDownload(
                _url,
                0);
            yield return request;
#else
            var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(_url, 0, 0);

            request.SendWebRequest();
            while (!request.isDone)
            {
                Progress.Value = request.downloadProgress;

                yield return null;
            }
#endif
            
            Verbose("DownloadBundle complete.");

            if (_destroy)
            {
                request.Dispose();
                
                Verbose("Loader was destroyed when load came back. Disposing bundle.");
                
                yield break;
            }

            if (!string.IsNullOrEmpty(request.error))
            {
                Verbose("Network or Http error: {0}.", request.error);

                // allow retries
                _bundleLoad = null;

                token.Fail(new Exception(request.error));
            }
            else
            {
#if FALSE && !NETFX_CORE
                token.Succeed(request.assetBundle);
#else           
                // wait for bundle to complete
                var bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
                if (null == bundle)
                {
                    token.Fail(new Exception(request.error));
                }
                else
                {
                    token.Succeed(bundle);
                }
#endif
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
                token.Fail(new Exception("Could not find asset."));
            }
            else
            {
                token.Succeed(asset);
            }
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="replacements">Logging replacements.</param>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this,
                "[{0}] {1}",
                _url,
                string.Format(message, replacements));
        }
    }
}