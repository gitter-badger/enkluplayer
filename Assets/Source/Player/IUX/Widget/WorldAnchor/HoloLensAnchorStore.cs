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
using Random = System.Random;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Implementation for HoloLens.
    /// </summary>
    public class HoloLensWorldAnchorProvider : IAnchorStore
    {
        /// <summary>
        /// PRNG.
        /// </summary>
        private static readonly Random _Prng = new Random();

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// Handles metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Configuration for the application.
        /// </summary>
        private readonly ApplicationConfig _config;

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
        /// True iff a queued action is currently being processed.
        /// </summary>
        private bool _isProcessingQueues;

        /// <summary>
        /// HoloLens API.
        /// </summary>
        private WorldAnchorStore _store;

        /// <summary>
        /// Manages all scenes.
        /// </summary>
        private IAppSceneManager _scenes;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensWorldAnchorProvider(
            IBootstrapper bootstrapper,
            IMetricsService metrics,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _metrics = metrics;
            _config = config;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Setup(IAppSceneManager scenes)
        {
            _scenes = scenes;
            
            // listen for tracking loss event
            WorldManager.OnPositionalLocatorStateChanged += WorldManager_OnPositionalLocatorStateChanged;
            
            return LoadStore().Token();
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
        public void ClearAllAnchors()
        {
            if (null == _store)
            {
                Log.Warning(this, "Store is not yet loaded. This method must be called after Initialize has completed.");
                return;
            }

            // kill all local anchors!
            _store.Clear();

            // kill all queues
            _importQueue.Clear();
            _exportQueue.Clear();

            var anchors = new List<WorldAnchorWidget>();
            foreach (var sceneId in _scenes.All)
            {
                anchors.Clear();
                _scenes.Root(sceneId).Find("..(@type==WorldAnchorWidget)", anchors);

                foreach (var anchor in anchors)
                {
                    anchor.Reload();
                }
            }
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

            // grab anchor
            var anchor = _store.Load(id, gameObject);
            if (null != anchor)
            {
                Log.Warning(this, "Tried to export anchor that was already part of lo0cal anchor store.");
                token.Fail(new Exception("Tried to export anchor that was already part of local anchor store."));

                return token;
            }

            // create anchor
            anchor = gameObject.AddComponent<WorldAnchor>();
            
            // create buffer for export data
            var buffer = new List<byte>();
            WorldAnchorTransferBatch batch = null;

            Action<SerializationCompletionReason> onExportComplete = null;

            void OnExportDataAvailable(byte[] bytes) => buffer.AddRange(bytes);
            
            void Save()
            {
                _isProcessingQueues = true;

                Log.Info(this, "{0}::Saving anchor to local store.");

                // save locally
                if (!_store.Save(id, anchor))
                {
                    if (--retries > 0)
                    {
                        Log.Warning(this, "{0}::Could not save anchor in local store, retrying.", id);
                        Save();
                    }
                    else
                    {
                        Log.Error(this, "{0}::Could not save anchor in local store!", id);
                        token.Fail(new Exception("Could not save in local store."));

                        _isProcessingQueues = false;
                    }
                }
                else
                {
                    Export();
                }
            }

            void Export()
            {
                Log.Info(this, "{0}::Exporting anchor into transfer batch.", id);
                
                // reset buffer
                buffer.Clear();

                // begin export
                batch = new WorldAnchorTransferBatch();
                batch.AddWorldAnchor(id, anchor);
                WorldAnchorTransferBatch.ExportAsync(
                    batch,
                    OnExportDataAvailable,
                    new WorldAnchorTransferBatch.SerializationCompleteDelegate(onExportComplete));
            }
            
            onExportComplete = reason =>
            {
                // export
                _metrics.Timer(MetricsKeys.ANCHOR_EXPORT).Stop(exportId);

                if (reason == SerializationCompletionReason.Succeeded)
                {
                    Log.Info(this, "{0}::WorldAnchor export complete. Compressing data.", id);

                    // dispose of batch
                    batch.Dispose();

                    // metrics
                    var compressId = _metrics.Timer(MetricsKeys.ANCHOR_COMPRESSION).Start();

                    // compress data
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Windows.System.Threading.ThreadPool.RunAsync(context =>
                    {
                        byte[] compressed = null;

                        try
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var deflate = new DeflateStream(memoryStream, CompressionMode.Compress))
                                {
                                    deflate.Write(buffer.ToArray(), 0, buffer.Count);
                                }

                                compressed = memoryStream.ToArray();
                            }
                        }
                        catch
                        {
                            // ignore
                        }
                        
                        Synchronize(() =>
                        {
                            _exports.Remove(gameObject);

                            if (null == compressed)
                            {
                                Log.Warning(this, "{0}::Anchor export failed: out of memory.", id);

                                token.Fail(new Exception("Could not compress anchor: out of memory."));

                                _isProcessingQueues = false;
                                return;
                            }

                            // metrics
                            _metrics.Value(MetricsKeys.ANCHOR_SIZE_RAW).Value(buffer.Count);
                            _metrics.Value(MetricsKeys.ANCHOR_SIZE_COMPRESSED).Value(compressed.Length);
                            _metrics.Value(MetricsKeys.ANCHOR_SIZE_RATIO).Value(compressed.Length / (float)buffer.Count);

                            Log.Info(this,
                                "{0}::Compression complete. Saved {1} bytes.",
                                id,
                                buffer.Count - compressed.Length);

                            // metrics
                            _metrics.Timer(MetricsKeys.ANCHOR_COMPRESSION).Stop(compressId);

                            // stop tracking token so we can export again later
                            _exports.Remove(gameObject);

                            token.Succeed(compressed);

                            _isProcessingQueues = false;
                        });
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    Log.Warning(this, "{0}::WorldAnchor export failed.", id);

                    // dispose of batch
                    batch.Dispose();

                    if (--retries > 0)
                    {
                        Log.Info(this, "{0}::Retrying export.", id);

                        Export();
                    }
                    else
                    {
                        // stop tracking token so we can export again later
                        _exports.Remove(gameObject);

                        token.Fail(new Exception(string.Format(
                            "Could not export : {0}.",
                            reason)));

                        _isProcessingQueues = false;
                    }
                }
            };
            
            if (anchor.isLocated)
            {
                Log.Info(this, "Enqueue export.");

                _exportQueue.Enqueue(Save);

                ProcessQueues();
            }
            else
            {
                void OnTrackingChanged(WorldAnchor _, bool located)
                {
                    if (located)
                    {
                        anchor.OnTrackingChanged -= OnTrackingChanged;

                        Log.Info(this, "Enqueue export.");

                        _exportQueue.Enqueue(Save);

                        ProcessQueues();
                    }
                }

                anchor.OnTrackingChanged += OnTrackingChanged;
            }
            
            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, byte[] bytes, GameObject gameObject)
        {
            Log.Info(this, "{0}::Import()", id);

            // random failure?
            if (_config.Network.AnchorImportFailChance > Mathf.Epsilon)
            {
                if (_Prng.NextDouble() < _config.Network.AnchorImportFailChance)
                {
                    return new AsyncToken<Void>(new Exception("Random failure configured by ApplicationConfig."));
                }
            }
            
            // metrics
            var queuedMetricId = _metrics.Timer(MetricsKeys.ANCHOR_EXPORT_QUEUED).Start();
            var importMetricId = 0;

            // create a new token for it immediately
            RetainWatcher();
            var token = new AsyncToken<Void>();
            token.OnFinally(_ => ReleaseWatcher());
            
            // local function for queued import
            void QueuedImport()
            {
                // metrics
                _metrics.Timer(MetricsKeys.ANCHOR_EXPORT_QUEUED).Stop(queuedMetricId);

                Log.Info(this, "{0}::Begin queued import.", id);
                
                byte[] decompressed = null;

                // number of retries
                var retries = 3;

                // called to import from batch
                void Import()
                {
                    WorldAnchorTransferBatch.ImportAsync(decompressed, OnComplete);
                }

                // called when import into transfew batch is complete
                void OnComplete(SerializationCompletionReason reason, WorldAnchorTransferBatch batch)
                {
                    // metrics
                    _metrics.Timer(MetricsKeys.ANCHOR_IMPORT).Stop(importMetricId);

                    if (reason != SerializationCompletionReason.Succeeded)
                    {
                        Log.Warning(this, "{0}::Import into transfer batch failed.", id);

                        // retry
                        if (--retries > 0)
                        {
                            Log.Info(this, "{0}::Retrying import.", id);

                            Import();
                        }
                        else
                        {
                            token.Fail(new Exception("Import into transfer batch failed."));
                        }
                    }
                    // make sure gameobject is still alive
                    else if (gameObject)
                    {
                        var anchor = batch.LockObject(id, gameObject);
                        if (null != anchor)
                        {
                            if (_store.Save(id, anchor))
                            {
                                token.Succeed(Void.Instance);
                            }
                            else
                            {
                                token.Fail(new Exception("Locked object but could not save in anchor store."));
                            }
                        }
                        else
                        {
                            token.Fail(new Exception("Import succeded byt could not lock object."));
                        }
                    }

                    // done processing
                    _isProcessingQueues = false;
                }

                // metrics
                var compressId = _metrics.Timer(MetricsKeys.ANCHOR_DECOMPRESSION).Start();

                // start inflate in a threadpool
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Windows.System.Threading.ThreadPool.RunAsync(context =>
                {
                    // decompress bytes
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

                            decompressed = output.ToArray();
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                    
                    // import must be started from main thread
                    Synchronize(() =>
                    {
                        if (null == decompressed)
                        {
                            Log.Warning(this, "{0}::Decompression failed: out of memory.", id);

                            // metrics
                            _metrics.Timer(MetricsKeys.ANCHOR_DECOMPRESSION).Abort(compressId);

                            // done processing
                            _isProcessingQueues = false;

                            token.Fail(new Exception("Ran out of memory when decompressing."));
                            return;
                        }

                        Log.Info(this, "{0}::Decompression complete.", id);

                        // metrics
                        _metrics.Timer(MetricsKeys.ANCHOR_DECOMPRESSION).Stop(compressId);

                        // metrics
                        importMetricId = _metrics.Timer(MetricsKeys.ANCHOR_IMPORT).Start();

                        Import();
                    });
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            Log.Info(this, "{0}::Enqueued import.", id);
            _importQueue.Enqueue(QueuedImport);

            ProcessQueues();

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
        /// Loads the anchor store.
        /// </summary>
        /// <returns></returns>
        private AsyncToken<Void> LoadStore()
        {
            var token = new AsyncToken<Void>();

            WorldAnchorStore.GetAsync(store =>
            {
                _store = store;

                token.Succeed(Void.Instance);
            });

            return token;
        }

        /// <summary>
        /// Processes import queue.
        /// </summary>
        private void ProcessQueues()
        {
            if (_isProcessingQueues)
            {
                return;
            }

            if (0 == _importQueue.Count && 0 == _exportQueue.Count)
            {
                return;
            }

            _isProcessingQueues = true;

            Log.Info(this, "Processing next in queue.");

            // process imports first
            if (_importQueue.Count > 0)
            {
                _importQueue.Dequeue()();
            }
            else
            {
                _exportQueue.Dequeue()();
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

                ProcessQueues();
                
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

        /// <summary>
        /// Called when locator state changes.
        /// </summary>
        private void WorldManager_OnPositionalLocatorStateChanged(
            PositionalLocatorState oldstate,
            PositionalLocatorState newstate)
        {
            if (newstate == PositionalLocatorState.Active)
            {
                Log.Info(this, "Positional tracking switched to active. Attempting to reload store.");

                // reload store
                LoadStore()
                    .OnSuccess(_ =>
                    {
                        Log.Info(this, "Reloaded store. Beginning to reload elements.");

                        // reload anchors
                        var anchors = new List<WorldAnchorWidget>();
                        for (var i = 0; i < _scenes.All.Length; i++)
                        {
                            var sceneId = _scenes.All[i];
                            _scenes.Root(sceneId).Find("..(@type==WorldAnchorWidget)", anchors);

                            for (var j = 0; j < anchors.Count; j++)
                            {
                                anchors[j].Reload();
                            }
                        }
                    })
                    .OnFailure(ex => Log.Error(this, "Could not load anchor store : {0}.", ex));
            }
        }
    }
}

#endif