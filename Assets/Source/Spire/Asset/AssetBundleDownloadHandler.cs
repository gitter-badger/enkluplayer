using System;
using System.Collections;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Custom download handler which keeps asset bundle bytes.
    /// </summary>
    public class AssetBundleDownloadHandler : DownloadHandlerScript
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Raw bytes.
        /// </summary>
        private byte[] _bytes;
        
        /// <summary>
        /// Download progress.
        /// </summary>
        private float _progress;

        /// <summary>
        /// Index into byte array.
        /// </summary>
        private int _index;
        
        /// <summary>
        /// Backing variable for OnReady property.
        /// </summary>
        private readonly AsyncToken<AssetBundle> _onReady = new AsyncToken<AssetBundle>();

        /// <summary>
        /// Called when bundle is ready.
        /// </summary>
        public IAsyncToken<AssetBundle> OnReady
        {
            get { return _onReady; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="bootstrapper">For courintes.</param>
        public AssetBundleDownloadHandler(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }
        
        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override byte[] GetData()
        {
            return _bytes;
        }

        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override float GetProgress()
        {
            return _progress;
        }

        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override void ReceiveContentLength(int contentLength)
        {
            _bytes = new byte[contentLength];
        }
        
        /// <inheritdoc cref="DownloadHandlerScript"/>
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

        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override void CompleteContent()
        {
            _progress = 1;
            
            // create bundle
            _bootstrapper.BootstrapCoroutine(Wait(AssetBundle.LoadFromMemoryAsync(_bytes)));
        }

        /// <summary>
        /// Waits on the AssetBundle to build.
        /// </summary>
        /// <param name="request">The asset bundle request.</param>
        /// <returns></returns>
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