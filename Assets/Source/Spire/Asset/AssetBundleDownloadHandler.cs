using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace CreateAR.SpirePlayer.Assets
{
    public class AssetBundleDownloadHandler : DownloadHandlerScript
    {
        private readonly IBootstrapper _bootstrapper;
        private byte[] _bytes;
        private float _progress;

        private int _index;
        private readonly AsyncToken<AssetBundle> _onReady = new AsyncToken<AssetBundle>();

        public IAsyncToken<AssetBundle> OnReady
        {
            get { return _onReady; }
        }

        public AssetBundleDownloadHandler(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }
        
        protected override byte[] GetData()
        {
            return _bytes;
        }

        protected override float GetProgress()
        {
            return _progress;
        }

        protected override void ReceiveContentLength(int contentLength)
        {
            _bytes = new byte[contentLength];
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            var delta = _bytes.Length - _index;
            
            // too much data
            if (delta < dataLength)
            {
                return false;
            }
            
            Array.Copy(data, 0, _bytes, _index, dataLength);
            _index += dataLength;

            _progress = _index / (float) _bytes.Length;

            return true;
        }

        protected override void CompleteContent()
        {
            _progress = 1;
            
            // create bundle
            _bootstrapper.BootstrapCoroutine(Wait(AssetBundle.LoadFromMemoryAsync(_bytes)));
        }

        private IEnumerator Wait(AssetBundleCreateRequest request)
        {
            while (!request.isDone)
            {
                yield return null;
            }

            if (null != request.assetBundle)
            {
                _onReady.Succeed(request.assetBundle);
            }
            else
            {
                _onReady.Fail(new Exception("Could not create asset bundle."));
            }
        }
    }
}