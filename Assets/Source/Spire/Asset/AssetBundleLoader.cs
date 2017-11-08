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
    /// Communicates progress of a load.
    /// </summary>
    public class LoadProgress
    {
        /// <summary>
        /// Normalized load percentage, between 0 and 1.
        /// </summary>
        public float Value;

        /// <summary>
        /// True iff the load is complete.
        /// </summary>
        public bool IsComplete
        {
            get { return Math.Abs(Value - 1f) < Mathf.Epsilon; }
        }
    }

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
        /// The URL being loaded.
        /// </summary>
        private readonly string _url;

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
            string url)
        {
            _bootstrapper = bootstrapper;
            _url = url;
        }

        /// <summary>
        /// Loads the bundle.
        /// </summary>
        public void Load()
        {
            _bootstrapper.BootstrapCoroutine(LoadBundle());
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

            Log.Info(this, "Load asset {0}.", assetName);

            var token = new AsyncToken<Object>();
            var load = new LoadProgress();

            _bundleLoad
                .OnSuccess(bundle =>
                {
                    Log.Info(this, "Completed bundle load successfully.");

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
        private IEnumerator LoadBundle()
        {
            var token = new AsyncToken<AssetBundle>();
            _bundleLoad = token;

            Log.Info(this, "Downloading bundle {0}.", _url);

            var request = UnityWebRequest.GetAssetBundle(_url);
            request.Send();

            while (!request.isDone)
            {
                Progress.Value = request.downloadProgress;

                yield return null;
            }

            if (request.isNetworkError || request.isHttpError)
            {
                token.Fail(new Exception(request.error));
            }
            else
            {
                var bundle = ((DownloadHandlerAssetBundle) request.downloadHandler).assetBundle;

                if (null == bundle)
                {
                    token.Fail(new Exception("Could not create bundle."));
                }
                else
                {
                    token.Succeed(bundle);
                }
            }
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
    }
}