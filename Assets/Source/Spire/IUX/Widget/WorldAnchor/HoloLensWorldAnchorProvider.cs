#if !UNITY_EDITOR && UNITY_WSA

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Sharing;
using Object = UnityEngine.Object;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Implementation for HoloLens.
    /// </summary>
    public class HoloLensWorldAnchorProvider : IWorldAnchorProvider
    {
        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Queue of actions to run on the main thread.
        /// </summary>
        private readonly List<Action> _actions = new List<Action>();
        private readonly List<Action> _actionsReadBuffer = new List<Action>();

        /// <summary>
        /// Import dictionary.
        /// </summary>
        private readonly Dictionary<GameObject, AsyncToken<Void>> _imports = new Dictionary<GameObject, AsyncToken<Void>>();

        /// <summary>
        /// Export dictionary.
        /// </summary>
        private readonly Dictionary<GameObject, AsyncToken<byte[]>> _exports = new Dictionary<GameObject, AsyncToken<byte[]>>();

        /// <summary>
        /// Ref count for watcher.
        /// </summary>
        private int _watcherRefCount;

        /// <summary>
        /// True if the watcher is watching!
        /// </summary>
        private bool _isWatching;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensWorldAnchorProvider(IBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        /// <inheritdoc />
        public IAsyncToken<byte[]> Export(string id, GameObject gameObject)
        {
            Log.Info(this, "{0}::Export({1})", id, gameObject.name);

            // do not export the same anchor more than once at the same time
            AsyncToken<byte[]> token;
            if (_exports.TryGetValue(gameObject, out token))
            {
                Log.Info(this, "{0}::Anchor is already being exported currently.", id);
                return token.Token();
            }

            RetainWatcher();
            token = _exports[gameObject] = new AsyncToken<byte[]>();
            token.OnFinally(_ => ReleaseWatcher());

            // keep track of # of retries left (used below)
            var retries = 3;

            // create anchor
            var anchor = gameObject.GetComponent<WorldAnchor>();
            if (null == anchor)
            {
                anchor = gameObject.AddComponent<WorldAnchor>();
            }

            // create buffer for export data
            byte[] buffer = null;
            var index = 0;

            Action<SerializationCompletionReason> onExportComplete = null;
            Action<byte[]> onExportDataAvailable = null;
            Action export = () =>
            {
                Log.Info(this, "{0}::Starting export process.", id);

                // reset buffer and index
                buffer = new byte[4096];
                index = 0;

                // begin export
                var batch = new WorldAnchorTransferBatch();
                batch.AddWorldAnchor(id, anchor);
                WorldAnchorTransferBatch.ExportAsync(
                    batch,
                    new WorldAnchorTransferBatch.SerializationDataAvailableDelegate(onExportDataAvailable),
                    new WorldAnchorTransferBatch.SerializationCompleteDelegate(onExportComplete));
            };

            onExportDataAvailable = bytes =>
            {
                var len = bytes.Length;
                var delta = buffer.Length - index;

                // resize buffer
                while (len > delta)
                {
                    var target = buffer.Length * 2;
                    Log.Debug(this,
                        "{0}::Increasing buffer size to {1} bytes.",
                        id,
                        target);

                    var newBuffer = new byte[target];
                    Array.Copy(buffer, 0, newBuffer, 0, index);
                    buffer = newBuffer;

                    delta = buffer.Length - index;
                }

                Array.Copy(bytes, 0, buffer, index, len);
                index += len;
            };

            onExportComplete = reason =>
            {
                if (reason == SerializationCompletionReason.Succeeded)
                {
                    Log.Info(this, "{0}::WorldAnchor export complete. Compressing data.", id);

                    // compress data
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Windows.System.Threading.ThreadPool.RunAsync(context =>
                    {
                        byte[] compressed;
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var deflate = new DeflateStream(memoryStream, CompressionMode.Compress))
                            {
                                deflate.Write(buffer, 0, index);
                            }

                            compressed = memoryStream.ToArray();
                        }
                        
                        Synchronize(() =>
                        {
                            _exports.Remove(gameObject);
                            
                            Log.Info(this,
                                "{0}::Compression complete. Saved {1} bytes.",
                                id,
                                index - compressed.Length);

                            // stop tracking token so we can export again later
                            _exports.Remove(gameObject);

                            token.Succeed(compressed);
                        });
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    Log.Warning(this, "{0}::WorldAnchor export failed.", id);

                    if (--retries > 0)
                    {
                        Log.Info(this, "{0}::Retrying export.", id);

                        export();
                    }
                    else
                    {
                        // stop tracking token so we can export again later
                        _exports.Remove(gameObject);

                        token.Fail(new Exception(string.Format(
                            "Could not export : {0}.",
                            reason)));
                    }
                }
            };

            // begin export
            export();
            
            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, GameObject gameObject, byte[] bytes)
        {
            Log.Info(this, "{0}::Import({1})", id, gameObject.name);

            AsyncToken<Void> token;
            if (_imports.TryGetValue(gameObject, out token))
            {
                return token.Token();
            }

            RetainWatcher();
            token = _imports[gameObject] = new AsyncToken<Void>();
            token.OnFinally(_ => ReleaseWatcher());

            // number of retries
            byte[] compressed = new byte[0];
            var retries = 3;

            Action<SerializationCompletionReason, WorldAnchorTransferBatch> onComplete = null;
            onComplete = (reason, batch) =>
            {
                // object may be destroyed at this point
                if (!gameObject)
                {
                    Log.Info(this, "{0}::Import into transfer batch complete but GameObject is already destroyed.", id);

                    // stop tracking so we can import again later
                    _imports.Remove(gameObject);

                    token.Fail(new Exception("Target GameObject is destroyed."));

                    return;
                }

                if (reason != SerializationCompletionReason.Succeeded)
                {
                    Log.Warning(this, "{0}::Import into transfer batch failed.", id);

                    if (--retries < 0)
                    {
                        Log.Info(this, "{0}::Retrying import.", id);

                        WorldAnchorTransferBatch.ImportAsync(
                            compressed,
                            new WorldAnchorTransferBatch.DeserializationCompleteDelegate(onComplete));
                    }
                    else
                    {
                        // stop tracking so we can import again later
                        _imports.Remove(gameObject);

                        token.Fail(new Exception(string.Format(
                            "Could not import : {0}.",
                            reason)));
                    }
                }
                else
                {
                    Log.Info(this, "{0}::Import into transfer batch complete.", id);

                    batch.LockObject(id, gameObject);
                    
                    // stop tracking so we can import again later
                    _imports.Remove(gameObject);

                    token.Succeed(Void.Instance);
                }
            };

            // inflate
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.System.Threading.ThreadPool.RunAsync(context =>
            {
                // decompress bytes
                using (var output = new MemoryStream())
                {
                    using (var input = new MemoryStream(bytes))
                    {
                        using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                        {
                            deflate.CopyTo(output);
                        }
                    }
                    
                    compressed = output.ToArray();
                }

                // start import from main thread
                Synchronize(() =>
                {
                    Log.Info(this, "{0}::Decompression complete.", id);

                    if (!gameObject)
                    {
                        // remove so we can import again later
                        _imports.Remove(gameObject);

                        Log.Info(this, "GameObject destroyed. Not progressing to import into transfer batch.");
                        token.Fail(new Exception("GameObject destroyed before imported into transfer batch."));

                        return;
                    }

                    WorldAnchorTransferBatch.ImportAsync(
                        compressed,
                        new WorldAnchorTransferBatch.DeserializationCompleteDelegate(onComplete));
                });
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return token;
        }
        
        /// <inheritdoc />
        public void Disable(GameObject gameObject)
        {
            var anchor = gameObject.GetComponent<WorldAnchor>();
            if (null != anchor)
            {
                Object.Destroy(anchor);
            }
        }
        
        /// <summary>
        /// Adds to action list.
        /// </summary>
        /// <param name="action">Action to perform on main thread.</param>
        private void Synchronize(Action action)
        {
            lock (_actions)
            {
                _actions.Add(action);
            }
        }

        /// <summary>
        /// Ref counting, essentially.
        /// </summary>
        private void RetainWatcher()
        {
            _watcherRefCount++;

            if (!_isWatching)
            {
                _bootstrapper.BootstrapCoroutine(Watch());
            }
        }

        /// <summary>
        /// Ref counting, essentially.
        /// </summary>
        private void ReleaseWatcher()
        {
            _watcherRefCount--;
        }

        /// <summary>
        /// Long running poll.
        /// </summary>
        private IEnumerator Watch()
        {
            _isWatching = true;

            while (_watcherRefCount > 0)
            {
                lock (_actions)
                {
                    if (_actions.Count > 0)
                    {
                        _actionsReadBuffer.AddRange(_actions);
                        _actions.Clear();
                    }
                }

                if (_actionsReadBuffer.Count > 0)
                {
                    for (var i = 0; i < _actionsReadBuffer.Count; i++)
                    {
                        _actionsReadBuffer[i]();
                    }
                    _actionsReadBuffer.Clear();
                }

                yield return null;
            }

            _isWatching = false;
        }
    }
}

#endif