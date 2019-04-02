#if !UNITY_EDITOR && UNITY_WSA

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.CreateAnchor;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This object contains complete state and behavior for exporting a single
    /// anchor. It can be used once and only once.
    /// </summary>
    public class ExportPipelineSaga : IAsyncAction
    {
        /// <summary>
        /// Dictates options for exporting.
        /// </summary>
        [Flags]
        public enum ExportOptions
        {
            None = 0,
            Compressed = 1
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        private readonly IHttpService _http;
        private readonly IElementTxnManager _txns;
        private readonly IAppSceneManager _scenes;
        private readonly WorldAnchorStore _store;

        /// <summary>
        /// Unique id of the anchor.
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Current version of the anchor.
        /// </summary>
        private readonly int _version;

        /// <summary>
        /// Id of the app.
        /// </summary>
        private readonly string _appId;
        
        /// <summary>
        /// The GameObject to act as reference object.
        /// </summary>
        private readonly GameObject _gameObject;

        /// <summary>
        /// Resolution of async export.
        /// </summary>
        private readonly AsyncResolution<Void> _exportResolution = new AsyncResolution<Void>();
        
        /// <summary>
        /// Options for exporting.
        /// </summary>
        private ExportOptions _options;
        
        /// <summary>
        /// Externally facing token.
        /// </summary>
        private AsyncToken<Void> _token;

        /// <summary>
        /// The anchor we use or create.
        /// </summary>
        private WorldAnchor _anchor;

        /// <summary>
        /// Windows API.
        /// </summary>
        private WorldAnchorTransferBatch _batch;

        /// <summary>
        /// Eek, a buffer.
        /// </summary>
        private List<byte> _buffer;

        /// <summary>
        /// The bytes we have output.
        /// </summary>
        private byte[] _bytes;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExportPipelineSaga(
            IBootstrapper bootstrapper,
            IHttpService http,
            IElementTxnManager txns,
            IAppSceneManager scenes,
            WorldAnchorStore store,
            string id,
            int version,
            string appId,
            GameObject gameObject,
            ExportOptions options)
        {
            _bootstrapper = bootstrapper;
            _http = http;
            _txns = txns;
            _scenes = scenes;
            _store = store;
            _id = id;
            _version = version;
            _appId = appId;
            _gameObject = gameObject;
            _options = options;
        }
        
        /// <summary>
        /// Starts the export pipeline.
        /// </summary>
        public IAsyncToken<Void> Start()
        {
            Trace("Starting anchor export.");
      
            _token = new AsyncToken<Void>();
            
            // load existing anchor
            _anchor = _store.Load(_id, _gameObject);
            if (null != _anchor)
            {
                Trace("Exporting anchor that is already part of the anchor store.");
            }
            // create a new anchor
            else
            {
                _anchor = _gameObject.AddComponent<WorldAnchor>();
            }
            
            // poll anchor on main thread for when it is anchored
            _bootstrapper.BootstrapCoroutine(PollForIsLocated());
            
            return _token;
        }

        /// <summary>
        /// Called every frame on the main thread to determine if anchor has been located or not.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PollForIsLocated()
        {
            // wait until the anchor is located
            while (true)
            {
                if (!_anchor)
                {
                    _token.Fail(new Exception("Anchor destroyed while waiting for it to locate."));

                    yield break;
                }

                // if anchor is located, break out of loop
                if (_anchor.isLocated)
                {
                    break;
                }

                yield return null;
            }

            // save locally if anchor doesn't already exist
            if (!_store.GetAllIds().Contains(_id))
            {
                Trace("Saving anchor to local store.");

                if (!_store.Save(_id, _anchor))
                {
                    _token.Fail(new Exception("Could not save anchor to local anchor store."));

                    yield break;
                }
            }

            // start polling for completion
            _bootstrapper.BootstrapCoroutine(PollForCompletion());

            Export();
        }

        /// <summary>
        /// Called every frame on the main thread to determine if anchor has
        /// been exported or not.
        /// </summary>
        /// <returns></returns>
        private IEnumerator PollForCompletion()
        {
            // listen to local token
            var token = new AsyncToken<Void>();
            token
                .OnSuccess(_ => Upload())
                .OnFailure(_token.Fail);

            // try to apply export resolution to local token
            while (!_exportResolution.Apply(token))
            {
                yield return null;
            }
        }

        /// <summary>
        /// Exports data.
        /// </summary>
        private void Export()
        {
            Trace("Exporting anchor into transfer batch.");

            // prep buffer for receiving data
            _buffer = new List<byte>();

            // begin export
            _batch = new WorldAnchorTransferBatch();
            _batch.AddWorldAnchor(_id, _anchor);

            // called when data is available
            void OnExportDataAvailable(byte[] bytes) => _buffer.AddRange(bytes);

            // called when export is complete
            void OnExportComplete(SerializationCompletionReason reason)
            {
                // dispose of batch now that we have all the bytes
                _batch.Dispose();

                if (reason == SerializationCompletionReason.Succeeded)
                {
                    Trace("WorldAnchor has been exported into bytes.");

                    if (0 == (ExportOptions.Compressed & _options))
                    {
                        _bytes = _buffer.ToArray();
                        _exportResolution.Resolve(Void.Instance);
                    }
                    else
                    {
                        Compress(
                            _buffer.ToArray(),
                            compressed =>
                            {
                                Trace("Anchor data successfully compressed.");

                                _bytes = compressed;

                                _exportResolution.Resolve(Void.Instance);
                            });
                    }
                }
                else
                {
                    Trace("WorldAnchor export failed.");

                    _exportResolution.Resolve(new Exception("Could not export anchor data."));
                }
            }

            // start export
            WorldAnchorTransferBatch.ExportAsync(
                _batch,
                OnExportDataAvailable,
                OnExportComplete);
        }
        
        /// <summary>
        /// Compresses bytes and calls the callback if successful.
        /// </summary>
        /// <param name="callback">The callback.</param>
        private void Compress(byte[] bytes, Action<byte[]> callback)
        {
            Trace("Compressing anchor data for export.");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.System.Threading.ThreadPool.RunAsync(context =>
            {
                byte[] output;
                try
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var deflate = new DeflateStream(memoryStream, CompressionMode.Compress))
                        {
                            deflate.Write(bytes, 0, bytes.Length);
                        }

                        output = memoryStream.ToArray();
                    }
                }
                catch (Exception exception)
                {
                    _exportResolution.Resolve(exception);
                    return;
                }

                callback(output);
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Uploads to Trellis.
        /// </summary>
        private void Upload()
        {
            var sceneId = _scenes.All.FirstOrDefault();
            if (string.IsNullOrEmpty(sceneId))
            {
                _token.Fail(new Exception("No scenes are available."));
                return;
            }

            Trace("Uploading anchor.");

            var url = $"/editor/app/{_appId}/scene/{sceneId}/anchor/{_id}";
            
            _http
                .PostFile<Response>(
                    _http.Urls.Url(url),
                    new Commons.Unity.DataStructures.Tuple<string, string>[0],
                    ref _bytes)
                .OnSuccess(res =>
                {
                    if (res.Payload.Success)
                    {
                        Trace("Anchor uploaded successfully.");

                        FinalizeScene(sceneId);
                    }
                    else
                    {
                        _token.Fail(new Exception($"Could not upload anchor: {res.Payload.Error}."));
                    }
                })
                .OnFailure(ex => _token.Fail(new Exception($"Could not upload anchor: {ex.Message}.")));
        }

        /// <summary>
        /// Updates the scene with new anchor information.
        /// </summary>
        private void FinalizeScene(string sceneId)
        {
            Trace("Finalizing scene.");

            _txns
                .Request(new ElementTxn(sceneId)
                    .Update(_id, "src", $"{sceneId}.{_id}.{_version}.anchor")
                    .Update(_id, "version", _version)
                    .Update(_id, "autoexport", false)
                    .Update(_id, "compressed", 0 != (_options & ExportOptions.Compressed)))
                .OnSuccess(res =>
                {
                    Trace("Scene successfully updated.");

                    _token.Succeed(Void.Instance);
                })
                .OnFailure(ex => _token.Fail(new Exception($"Could not update the scene: {ex.Message}.")));
        }

        /// <summary>
        /// Logging wrapper.
        /// </summary>
        private void Trace(string message, params object[] replacements)
        {
            Log.Info(this, "[Anchor Id={0}] " + message, replacements);
        }
    }
}

#endif