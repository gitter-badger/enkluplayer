using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;

using Object = UnityEngine.Object;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    public class LoadProgress
    {
        public float Value;

        public bool IsComplete
        {
            get { return Math.Abs(Value - 1f) < Mathf.Epsilon; }
        }
    }

    public class AssetBundleLoader
    {
        private readonly IBootstrapper _bootstrapper;
        private readonly string _url;
        private IAsyncToken<AssetBundle> _bundleLoad;
        
        public AssetBundle Bundle { get; private set; }

        public readonly LoadProgress Progress = new LoadProgress();

        public AssetBundleLoader(
            IBootstrapper bootstrapper,
            string url)
        {
            _bootstrapper = bootstrapper;
            _url = url;
        }

        public void Load()
        {
            _bootstrapper.BootstrapCoroutine(LoadBundle());
        }

        public IAsyncToken<Object> Asset(string assetName, out LoadProgress progress)
        {
            var token = new AsyncToken<Object>();

            var load = new LoadProgress();

            _bundleLoad
                .OnSuccess(bundle =>
                {
                    Log.Info(this, "Completed bundle load successfully : {0}.", bundle);

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
        
        public IAsyncToken<Object> Asset(string assetName)
        {
            LoadProgress progress;
            return Asset(assetName, out progress);
        }

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

                token.Succeed(bundle);
            }
        }

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
    }
}