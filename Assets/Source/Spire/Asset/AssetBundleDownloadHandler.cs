using System;
using System.Collections;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
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
        /// Logging tag.
        /// </summary>
        private readonly string _tag;
        
        /// <summary>
        /// Raw bytes.
        /// </summary>
        private byte[] _buffer;
        
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
        /// <param name="tag">Tag to append to logs.</param>
        public AssetBundleDownloadHandler(
            IBootstrapper bootstrapper,
            string tag)
        {
            _bootstrapper = bootstrapper;
            _tag = tag;
        }
        
        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override byte[] GetData()
        {
            return _buffer;
        }

        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override float GetProgress()
        {
            return _progress;
        }

        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override void ReceiveContentLength(int contentLength)
        {
            _buffer = new byte[contentLength];
        }
        
        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override bool ReceiveData(byte[] bytes, int dataLength)
        {
            var remainingBufferLen = _buffer.Length - _index;
            
            // too much data
            if (remainingBufferLen < dataLength)
            {
                Trace("Buffer overflow-- received more data than content length.");
                
                return false;
            }
            
            Array.Copy(bytes, 0, _buffer, _index, dataLength);
            _index += dataLength;

            _progress = _index / (float) _buffer.Length;
            
            Trace("Received {0} bytes. {1} of {2} total.",
                dataLength,
                _index,
                _buffer.Length);

            return true;
        }

        /// <inheritdoc cref="DownloadHandlerScript"/>
        protected override void CompleteContent()
        {
            _progress = 1;
            
            Trace("Download complete ({0} / {1} bytes).",
                _index,
                _buffer.Length);
            
            // create bundle
            _bootstrapper.BootstrapCoroutine(Wait(AssetBundle.LoadFromMemoryAsync(_buffer)));
        }

        /// <summary>
        /// Waits on the AssetBundle to build.
        /// </summary>
        /// <param name="request">The asset bundle request.</param>
        /// <returns></returns>
        private IEnumerator Wait(AssetBundleCreateRequest request)
        {
            yield return request;

            if (null != request.assetBundle)
            {
                _onReady.Succeed(request.assetBundle);
            }
            else
            {
                _onReady.Fail(new Exception("Could not create asset bundle."));
            }
        }
        
        /// <summary>
        /// Verbose logging.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="replacements">Logging replacements.</param>
        [Conditional("LOGGING_VERBOSE")]
        private void Trace(string message, params object[] replacements)
        {
            Log.Info(this,
                "[{0}] {1}",
                _tag,
                string.Format(message, replacements));
        }
    }
}