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
            Log.Info(this, "Export({0})", gameObject.name);

            AsyncToken<byte[]> token;
            if (_exports.TryGetValue(gameObject, out token))
            {
                return token.Token();
            }

            RetainWatcher();
            token = _exports[gameObject] = new AsyncToken<byte[]>();
            token.OnFinally(_ => ReleaseWatcher());

            // TODO: pool these
            var buffer = new byte[4096];
            var index = 0;

            Action<byte[]> onExportDataAvailable = bytes =>
            {
                var len = bytes.Length;
                var delta = buffer.Length - index;

                // resize buffer
                while (len > delta)
                {
                    var target = buffer.Length * 2;
                    Log.Debug(this, "Increasing buffer size to {0} bytes.", target);

                    var newBuffer = new byte[target];
                    Array.Copy(buffer, 0, newBuffer, 0, index);
                    buffer = newBuffer;

                    delta = buffer.Length - index;
                }

                Array.Copy(bytes, 0, buffer, index, len);
                index += len;
            };

            Action<SerializationCompletionReason> onExportComplete = reason =>
            {
                Log.Info(this, "WorldAnchor export complete. Compressing data.");

                if (reason == SerializationCompletionReason.Succeeded)
                {
                    // compress data
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
                                "Compression complete. Saved {0} bytes.",
                                index - compressed.Length);

                            token.Succeed(compressed);
                        });
                    });
                }
                else
                {
                    token.Fail(new Exception(string.Format(
                        "Could not export : {0}.",
                        reason)));
                }
            };

            var anchor = gameObject.GetComponent<WorldAnchor>()
                ?? gameObject.AddComponent<WorldAnchor>();

            var batch = new WorldAnchorTransferBatch();
            batch.AddWorldAnchor(id, anchor);
            WorldAnchorTransferBatch.ExportAsync(
                batch,
                new WorldAnchorTransferBatch.SerializationDataAvailableDelegate(onExportDataAvailable),
                new WorldAnchorTransferBatch.SerializationCompleteDelegate(onExportComplete));

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, GameObject gameObject, byte[] bytes)
        {
            Log.Info(this, "Import({0})", gameObject.name);

            AsyncToken<Void> token;
            if (_imports.TryGetValue(gameObject, out token))
            {
                return token.Token();
            }

            RetainWatcher();
            token = _imports[gameObject] = new AsyncToken<Void>();
            token.OnFinally(_ => ReleaseWatcher());

            Action<SerializationCompletionReason, WorldAnchorTransferBatch> onComplete = (reason, batch) =>
            {
                _imports.Remove(gameObject);

                if (reason != SerializationCompletionReason.Succeeded)
                {
                    token.Fail(new Exception(string.Format(
                        "Could not import : {0}.",
                        reason)));
                    return;
                }

                batch.LockObject(id, gameObject);

                Log.Info(this, "Import complete.");

                token.Succeed(Void.Instance);
            };

            // inflate
            Windows.System.Threading.ThreadPool.RunAsync(context =>
            {
                byte[] compressed;
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

                Synchronize(() =>
                {
                    Log.Info(this, "Decompression complete.");

                    WorldAnchorTransferBatch.ImportAsync(
                        compressed,
                        new WorldAnchorTransferBatch.DeserializationCompleteDelegate(onComplete));
                });
            });

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