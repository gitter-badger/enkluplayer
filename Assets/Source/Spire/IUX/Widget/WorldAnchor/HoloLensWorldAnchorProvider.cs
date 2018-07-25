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
using UnityEngine.XR.WSA.Persistence;
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
        /// Handles metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Queue of actions to run on the main thread.
        /// </summary>
        private readonly List<Action> _actions = new List<Action>();
        private readonly List<Action> _actionsReadBuffer = new List<Action>();

        /// <summary>
        /// Queue for importing.
        /// </summary>
        private readonly Queue<Action> _importQueue = new Queue<Action>();

        /// <summary>
        /// Queue for exporting.
        /// </summary>
        private readonly Queue<Action> _exportQueue = new Queue<Action>();

        /// <summary>
        /// Import dictionary.
        /// </summary>
        private readonly Dictionary<string, AsyncToken<Void>> _imports = new Dictionary<string, AsyncToken<Void>>();

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

        private bool _isProcessing;

        private WorldAnchorStore _store;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensWorldAnchorProvider(
            IBootstrapper bootstrapper,
            IMetricsService metrics)
        {
            _bootstrapper = bootstrapper;
            _metrics = metrics;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Initialize()
        {
            var token = new AsyncToken<Void>();

            WorldAnchorStore.GetAsync(store =>
            {
                _store = store;

                RetainWatcher();
                Synchronize(() =>
                {
                    ReleaseWatcher();

                    token.Succeed(Void.Instance);
                });
            });

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Anchor(string id, GameObject gameObject)
        {
            var ids = _store.GetAllIds();
            for (int i = 0, len = ids.Length; i < len; i++)
            {
                if (id == ids[i])
                {
                    var anchor = _store.Load(id, gameObject);
                    if (null == anchor)
                    {
                        return new AsyncToken<Void>(new Exception("WorldAnchorStore::Load completed but did not return an anchor."));
                    }
                    
                    TrackUnlocatedAnchor(anchor);

                    return new AsyncToken<Void>(Void.Instance);
                }
            }

            return new AsyncToken<Void>(new Exception("No anchor by that id."));
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

            // instrument
            var exportId = _metrics.Timer(MetricsKeys.ANCHOR_EXPORT).Start();

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
            Action export = null;
            export = () =>
            {
                Log.Info(this, "{0}::Starting export process.", id);

                // reset buffer and index
                buffer = new byte[4096];
                index = 0;
                
                // save locally
                if (!_store.Save(id, anchor))
                {
                    if (--retries > 0)
                    {
                        Log.Warning(this, "{0}::Could not save anchor in local store, retrying.", id);
                        export();
                    }
                    else
                    {
                        Log.Error(this, "{0}::Could not save anchor in local store!", id);
                        token.Fail(new Exception("Could not save in local store."));
                    }
                    

                    return;
                }

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
                // export
                _metrics.Timer(MetricsKeys.ANCHOR_EXPORT).Stop(exportId);

                if (reason == SerializationCompletionReason.Succeeded)
                {
                    Log.Info(this, "{0}::WorldAnchor export complete. Compressing data.", id);

                    // metrics
                    var compressId = _metrics.Timer(MetricsKeys.ANCHOR_COMPRESSION).Start();

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

                        // metrics
                        _metrics.Value(MetricsKeys.ANCHOR_SIZE_RAW).Value(buffer.Length);
                        _metrics.Value(MetricsKeys.ANCHOR_SIZE_COMPRESSED).Value(compressed.Length);
                        _metrics.Value(MetricsKeys.ANCHOR_SIZE_RATIO).Value(compressed.Length / (float) buffer.Length);

                        Synchronize(() =>
                        {
                            _exports.Remove(gameObject);
                            
                            Log.Info(this,
                                "{0}::Compression complete. Saved {1} bytes.",
                                id,
                                index - compressed.Length);

                            // metrics
                            _metrics.Timer(MetricsKeys.ANCHOR_COMPRESSION).Stop(compressId);

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
        public IAsyncToken<Void> Import(string id, byte[] bytes)
        {
            Log.Info(this, "{0}::Import()", id);

            if (_imports.TryGetValue(id, out var token))
            {
                return token.Token();
            }

            // metrics
            var queuedId = _metrics.Timer(MetricsKeys.ANCHOR_EXPORT_QUEUED).Start();

            // create a new token for it immediately
            RetainWatcher();
            token = _imports[id] = new AsyncToken<Void>();
            token.OnFinally(_ => ReleaseWatcher());

            // local function for queued import
            void QueuedImport()
            {
                // metrics
                _metrics.Timer(MetricsKeys.ANCHOR_EXPORT_QUEUED).Stop(queuedId);

                Log.Info(this, "{0}::Begin queued import.", id);

                // metrics
                var importId = -1;

                // number of retries
                var compressed = new byte[0];
                var retries = 3;

                // called when import into transfew batch is complete
                Action<SerializationCompletionReason, WorldAnchorTransferBatch> onComplete = null;
                onComplete = (reason, batch) =>
                {
                    // metrics
                    _metrics.Timer(MetricsKeys.ANCHOR_IMPORT).Stop(importId);
                    
                    if (reason != SerializationCompletionReason.Succeeded)
                    {
                        Log.Warning(this, "{0}::Import into transfer batch failed.", id);

                        // retry
                        if (--retries < 0)
                        {
                            Log.Info(this, "{0}::Retrying import.", id);

                            WorldAnchorTransferBatch.ImportAsync(
                                compressed,
                                new WorldAnchorTransferBatch.DeserializationCompleteDelegate(onComplete));
                        }
                        else
                        {
                            // stop tracking
                            _imports.Remove(id);

                            // save
                            var temp = new GameObject("__WorldAnchorTemp");
                            var anchor = batch.LockObject(id, temp);
                            var success = _store.Save(id, anchor);

                            Object.Destroy(temp);

                            if (success)
                            {
                                token.Succeed(Void.Instance);
                            }
                            else
                            {
                                token.Fail(new Exception("Successful import, but could not create anchor."));
                            }
                        }
                    }
                    else
                    {
                        Log.Info(this, "{0}::Import into transfer batch complete.", id);
                        
                        // stop tracking
                        _imports.Remove(id);

                        token.Succeed(Void.Instance);
                    }

                    // done processing
                    _isProcessing = false;
                };

                // metrics
                var compressId = _metrics.Timer(MetricsKeys.ANCHOR_DECOMPRESSION).Start();

                // start inflate in a threadpool
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

                    // metrics
                    _metrics.Timer(MetricsKeys.ANCHOR_DECOMPRESSION).Stop(compressId);

                    // import must be started from main thread
                    Synchronize(() =>
                    {
                        Log.Info(this, "{0}::Decompression complete.", id);
                        
                        // metrics
                        importId = _metrics.Timer(MetricsKeys.ANCHOR_IMPORT).Start();

                        WorldAnchorTransferBatch.ImportAsync(
                            compressed,
                            new WorldAnchorTransferBatch.DeserializationCompleteDelegate(onComplete));
                    });
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            Log.Info(this, "{0}::Enqueued import.", id);
            _importQueue.Enqueue(QueuedImport);

            ProcessImportQueue();

            return token;
        }
        
        /// <inheritdoc />
        public void UnAnchor(GameObject gameObject)
        {
            var anchor = gameObject.GetComponent<WorldAnchor>();
            if (null != anchor)
            {
                Object.Destroy(anchor);
            }
        }

        /// <summary>
        /// Processes import queue.
        /// </summary>
        private void ProcessImportQueue()
        {
            if (_isProcessing || 0 == _importQueue.Count)
            {
                return;
            }

            _isProcessing = true;

            Log.Info(this, "Processing next in queue.");

            var next = _importQueue.Dequeue();
            next();
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

                ProcessImportQueue();

                yield return null;
            }

            _isWatching = false;
        }

        /// <summary>
        /// Times how long an anchor is unlocated.
        /// </summary>
        /// <param name="anchor">The anchor to track.</param>
        private void TrackUnlocatedAnchor(WorldAnchor anchor)
        {
            // anchor is already located
            if (anchor.isLocated)
            {
                return;
            }

            // start timer
            var metricId = _metrics.Timer(MetricsKeys.ANCHOR_UNLOCATED).Start();

            WorldAnchor.OnTrackingChangedDelegate handler = null;
            handler = (_, isLocated) =>
            {
                anchor.OnTrackingChanged -= handler;

                _metrics.Timer(MetricsKeys.ANCHOR_UNLOCATED).Stop(metricId);
            };

            // listen
            anchor.OnTrackingChanged += handler;
        }
    }
}

#endif