#if !UNITY_EDITOR && UNITY_WSA

using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This object contains complete state and behavior for importing a single
    /// anchor. It can be used once and only once.
    /// </summary>
    public class ImportPipelineSaga : IAsyncAction
    {
        /// <summary>
        /// Options available during import.
        /// </summary>
        [Flags]
        public enum ImportOptions
        {
            None = 0,
            Compressed = 1
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        private readonly IHttpService _http;
        private readonly IAppSceneManager _scenes;
        private readonly WorldAnchorStore _store;
        
        /// <summary>
        /// Id of the anchor.
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Version of the anchor.
        /// </summary>
        private readonly int _version;

        /// <summary>
        /// The GameObject to anchor.
        /// </summary>
        private readonly GameObject _gameObject;

        /// <summary>
        /// Resolution object, set in another thread.
        /// </summary>
        private readonly AsyncResolution<byte[]> _decompressResolution = new AsyncResolution<byte[]>();

        /// <summary>
        /// Resolution object, set in another thread.
        /// </summary>
        private readonly AsyncResolution<Void> _importResolution = new AsyncResolution<Void>();

        /// <summary>
        /// The raw anchor bytes.
        /// </summary>
        private byte[] _bytes;

        /// <summary>
        /// Token returned from Start().
        /// </summary>
        private AsyncToken<Void> _token;

        /// <summary>
        /// Options to import with.
        /// </summary>
        private ImportOptions _options;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ImportPipelineSaga(
            IBootstrapper bootstrapper,
            IHttpService http,
            IAppSceneManager scenes,
            WorldAnchorStore store,
            string id,
            int version,
            GameObject gameObject,
            ImportOptions options)
        {
            _bootstrapper = bootstrapper;
            _http = http;
            _scenes = scenes;
            _store = store;
            _id = id;
            _version = version;
            _gameObject = gameObject;
            _options = options;
        }
        
        /// <summary>
        /// Starts the saga.
        /// </summary>
        public IAsyncToken<Void> Start()
        {
            Trace("Started anchor import.");

            _token = new AsyncToken<Void>();
            
            // start download
            _bootstrapper.BootstrapCoroutine(Download());
            
            return _token;
        }

        /// <summary>
        /// Downloads anchor data.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Download()
        {
            var sceneId = _scenes.All.FirstOrDefault();
            if (string.IsNullOrEmpty(sceneId))
            {
                _token.Fail(new Exception("No scenes are available."));
                yield break;
            }

            Trace("Downloading anchor data.");

            var formattedUrl = _http.Urls.Url($"anchors://{sceneId}.{_id}.{_version}.anchor");

            // TODO: switch to IHttpService when you can override headers for a single request
            var request = UnityWebRequest.Get(formattedUrl);
            
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                _token.Fail(new Exception($"Could not download anchor data: {request.error}."));
                yield break;
            }

            Trace("Anchor downloaded.");

            _bytes = request.downloadHandler.data;
            
            if (0 == (ImportOptions.Compressed & _options))
            {
                Trace("Starting anchor import.");

                // we poll for completion
                _bootstrapper.BootstrapCoroutine(PollForImportCompletion());

                // start the import!
                WorldAnchorTransferBatch.ImportAsync(_bytes, OnComplete);
            }
            else
            {
                // decompress first
                Decompress(_bytes);
            }
        }

        /// <summary>
        /// Decompresses bytes on a separate thread, then resolves resolution
        /// </summary>
        private void Decompress(byte[] bytes)
        {
            Trace("Decompressing anchor data of {0} bytes.", bytes.Length);

            // start polling on main thread
            _bootstrapper.BootstrapCoroutine(PollForDecompressionCompletion());

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.System.Threading.ThreadPool.RunAsync(context =>
            {
                Trace("Decompression thread online.");

                byte[] decompressed;
                try
                {
                    using (var output = new MemoryStream())
                    {
                        using (var input = new MemoryStream(bytes))
                        {
                            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                            {
                                deflate.CopyTo(output);
                            }
                        }
                        
                        // allocate like crazy
                        decompressed = output.ToArray();
                    }
                }
                catch (Exception exception)
                {
                    Trace("Decompression error.");

                    _decompressResolution.Resolve(new Exception($"Could not decompress anchor: {exception}."));
                    return;
                }

                Trace("Decompressed into {0} bytes.", decompressed.Length);

                _decompressResolution.Resolve(decompressed);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Poll function that runs on the main thread and watches for decompression to complete.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PollForDecompressionCompletion()
        {
            var token = new AsyncToken<byte[]>();
            token
                .OnSuccess(decompressed =>
                {
                    Trace("Anchor data decompressed.");

                    _bytes = decompressed;

                    Trace("Starting anchor import.");

                    _bootstrapper.BootstrapCoroutine(PollForImportCompletion());

                    WorldAnchorTransferBatch.ImportAsync(_bytes, OnComplete);
                })
                .OnFailure(_token.Fail);

            while (!_decompressResolution.Apply(token))
            {
                yield return null;
            }
        }

        /// <summary>
        /// Poll function that runs on the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PollForImportCompletion()
        {
            // attempt to resolve the token
            while (!_importResolution.Apply(_token))
            {
                yield return null;
            }
        }

        /// <summary>
        /// Called when import is complete.
        /// </summary>
        /// <param name="reason">The reason-- could be an error.</param>
        /// <param name="batch">The associated batch.</param>
        private void OnComplete(SerializationCompletionReason reason, WorldAnchorTransferBatch batch)
        {
            if (reason != SerializationCompletionReason.Succeeded)
            {
                Trace("Import into transfer batch failed.");

                _importResolution.Resolve(new Exception("Import into transfer batch failed."));
            }
            // make sure GameObject is still alive
            else if (_gameObject)
            {
                Trace("Import into transfer batch succeeded. Attempting to lock object.");

                var anchorId = $"{_id}.{_version}";
                var anchor = batch.LockObject(anchorId, _gameObject);
                if (null != anchor)
                {
                    Trace("Object locked successfully. Attempting to save in store.");

                    if (_store.Save(anchorId, anchor))
                    {
                        Trace("Anchor successfully saved in local anchor store.");

                        _importResolution.Resolve(Void.Instance);
                    }
                    else
                    {
                        _importResolution.Resolve(new Exception("Locked object but could not save in anchor store."));
                    }
                }
                else
                {
                    _importResolution.Resolve(new Exception("Import succeeded but could not lock object."));
                }
            }
            else
            {
                _importResolution.Resolve(new Exception("GameObject was destroyed while importing anchor."));
            }
        }

        /// <summary>
        /// Logging wrapper.
        /// </summary>
        private void Trace(string message, params object[] replacements)
        {
            Log.Info(this, "[Anchor Id=" + _id + "] " + message, replacements);
        }
    }
}

#endif