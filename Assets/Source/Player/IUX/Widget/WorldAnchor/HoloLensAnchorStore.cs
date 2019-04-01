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
    /// Keeps a resolution for an <c>AsyncToken</c> separate from the token.
    /// This is particularly useful for passing resolution between threads.
    /// </summary>
    /// <typeparam name="T">The type of resolution supported.</typeparam>
    public class AsyncResolution<T>
    {
        /// <summary>
        /// The success value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// The error value.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// True iff resolved.
        /// </summary>
        private bool _isResolved;

        /// <summary>
        /// Resolves successfully.
        /// </summary>
        /// <param name="value"></param>
        public void Resolve(T value)
        {
            if (_isResolved)
            {
                throw new Exception("Resolution is already resolved!");
            }

            Value = value;
            _isResolved = true;
        }

        /// <summary>
        /// Resolves with an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Resolve(Exception exception)
        {
            if (_isResolved)
            {
                throw new Exception("Resolution is already resolved!");
            }

            Exception = exception;
            _isResolved = true;
        }

        /// <summary>
        /// Attempts to apply the resolution to a token. Returns true iff the
        /// resolution was already resolved.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool Apply(AsyncToken<T> token)
        {
            if (!_isResolved)
            {
                return false;
            }

            if (null == Exception)
            {
                token.Fail(Exception);
            }
            else
            {
                token.Succeed(Value);
            }

            return true;
        }
    }

    /// <summary>
    /// This object contains complete state and behavior for importing a single
    /// anchor. It can be used once and only once.
    /// </summary>
    public class ImportPipelineSaga
    {
        [Flags]
        public enum ImportOptions
        {
            None = 0,
            Compressed = 1
        }

        /// <summary>
        /// Random number generator.
        /// </summary>
        private static readonly Random _Prng = new Random();

        private readonly IBootstrapper _bootstrapper;
        private readonly ApplicationConfig _config;
        private readonly WorldAnchorStore _store;
        private readonly string _id;
        private readonly GameObject _gameObject;
        
        private readonly AsyncResolution<Void> _resolution = new AsyncResolution<Void>();

        private byte[] _bytes;

        /// <summary>
        /// Token returned from Start().
        /// </summary>
        private AsyncToken<Void> _token;

        public ImportPipelineSaga(
            IBootstrapper bootstrapper,
            ApplicationConfig config,
            WorldAnchorStore store,
            string id,
            byte[] bytes,
            GameObject gameObject)
        {
            _bootstrapper = bootstrapper;
            _config = config;
            _store = store;
            _id = id;
            _bytes = bytes;
            _gameObject = gameObject;
        }
        
        public IAsyncToken<Void> Start(ImportOptions options)
        {
            Trace("Started import.");

            // random failure?
            if (_config.Network.AnchorImportFailChance > Mathf.Epsilon)
            {
                if (_Prng.NextDouble() < _config.Network.AnchorImportFailChance)
                {
                    return new AsyncToken<Void>(new Exception("Random failure configured by ApplicationConfig."));
                }
            }
            
            _token = new AsyncToken<Void>();

            // we poll for completion
            _bootstrapper.BootstrapCoroutine(Poll());

            if (0 == (ImportOptions.Compressed & options))
            {
                // start the import!
                WorldAnchorTransferBatch.ImportAsync(_bytes, OnComplete);
            }
            else
            {
                // decompress first
                Decompress(() => WorldAnchorTransferBatch.ImportAsync(_bytes, OnComplete));
            }

            return _token;
        }

        /// <summary>
        /// Poll function that runs on the main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Poll()
        {
            // attempt to resolve the token
            while (!_resolution.Apply(_token))
            {
                yield return null;
            }
        }

        /// <summary>
        /// Decompresses bytes on a separate thread, then calls the callback.
        /// </summary>
        /// <param name="callback">Callback to call-- will be called on a separate thread.</param>
        private void Decompress(Action callback)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Windows.System.Threading.ThreadPool.RunAsync(context =>
            {
                // decompress bytes
                try
                {
                    using (var output = new MemoryStream())
                    {
                        using (var input = new MemoryStream(_bytes))
                        {
                            using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                            {
                                deflate.CopyTo(output);
                            }
                        }

                        // allocate like crazy
                        _bytes = output.ToArray();
                    }
                }
                catch (Exception exception)
                {
                    _resolution.Resolve(new Exception($"Could not decompress anchor: {exception}."));

                    return;
                }

                callback();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

                _resolution.Resolve(new Exception("Import into transfer batch failed."));
            }
            // make sure GameObject is still alive
            else if (_gameObject)
            {
                Trace("Import into transfer batch succeeded. Attempting to lock object.");

                var anchor = batch.LockObject(_id, _gameObject);
                if (null != anchor)
                {
                    Trace("Object locked successfully. Attempting to save in store.");

                    if (_store.Save(_id, anchor))
                    {
                        Trace("Anchor successfully saved!");

                        _resolution.Resolve(Void.Instance);
                    }
                    else
                    {
                        _resolution.Resolve(new Exception("Locked object but could not save in anchor store."));
                    }
                }
                else
                {
                    _resolution.Resolve(new Exception("Import succeeded but could not lock object."));
                }
            }
            else
            {
                _resolution.Resolve(new Exception("GameObject was destroyed while importing anchor."));
            }
        }

        /// <summary>
        /// Logging wrapper.
        /// </summary>
        private void Trace(string message, params object[] replacements)
        {
            Log.Info(this, "[Anchor Id={0}] " + message, replacements);
        }
    }

    public class ExportPipelineSaga
    {
        [Flags]
        public enum ExportOptions
        {
            None = 0,
            Compressed = 1
        }

        private readonly IBootstrapper _bootstrapper;
        private readonly string _id;
        private readonly GameObject _gameObject;
        private readonly WorldAnchorStore _store;

        private readonly AsyncResolution<Void> _resolution = new AsyncResolution<Void>();

        private ExportOptions _options;

        private AsyncToken<Void> _token;
        private WorldAnchor _anchor;
        private WorldAnchorTransferBatch _batch;
        private List<byte> _buffer;

        public ExportPipelineSaga(string id)
        {
            _id = id;
        }
        
        public IAsyncToken<Void> Start(ExportOptions options)
        {
            _options = options;

            Trace("{0}::Export({1})", _id, _gameObject.name);
            
            _token = new AsyncToken<Void>();
            
            // grab anchor
            _anchor = _store.Load(_id, _gameObject);
            if (null != _anchor)
            {
                Log.Warning(this, "Tried to export anchor that was already part of lo0cal anchor store.");
                _token.Fail(new Exception("Tried to export anchor that was already part of local anchor store."));

                return _token;
            }

            // create anchor
            _anchor = _gameObject.AddComponent<WorldAnchor>();
            
            _bootstrapper.BootstrapCoroutine(PollForIsLocated());
            
            return _token;
        }

        private IEnumerator PollForIsLocated()
        {
            while (!_anchor.isLocated)
            {
                yield return null;
            }

            SaveLocally();
        }

        private IEnumerator PollForCompletion()
        {
            while (!_resolution.Apply(_token))
            {
                yield return null;
            }
        }

        private void SaveLocally()
        {
            Trace("Saving anchor to local store.");

            if (!_store.Save(_id, _anchor))
            {
                _token.Fail(new Exception("Could not save anchor to local anchor store."));
            }
            else
            {
                // start polling for completion
                _bootstrapper.BootstrapCoroutine(PollForCompletion());

                Export();
            }
        }

        private void Export()
        {
            Trace("Exporting anchor into transfer batch.");

            // prep buffer for receiving data
            _buffer = new List<byte>();

            // begin export
            _batch = new WorldAnchorTransferBatch();
            _batch.AddWorldAnchor(_id, _anchor);
            
            WorldAnchorTransferBatch.ExportAsync(
                _batch,
                OnExportDataAvailable,
                OnExportComplete);
        }

        private void OnExportDataAvailable(byte[] bytes) => _buffer.AddRange(bytes);

        private void OnExportComplete(SerializationCompletionReason reason)
        {
            // dispose of batch
            _batch.Dispose();

            if (reason == SerializationCompletionReason.Succeeded)
            {
                Trace("WorldAnchor export complete.");
                
                if (0 == (ExportOptions.Compressed & _options))
                {
                    _resolution.Resolve(Void.Instance);
                }
                else
                {
                    Compress(() => _resolution.Resolve(Void.Instance));
                }
            }
            else
            {
                Trace("WorldAnchor export failed.");
                
                _resolution.Resolve(new Exception("Could not export anchor data."));
            }
        }

        private void Compress(Action callback)
        {
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
                            deflate.Write(_buffer.ToArray(), 0, _buffer.Count);
                        }

                        compressed = memoryStream.ToArray();
                    }
                }
                catch (Exception exception)
                {
                    _resolution.Resolve(exception);
                    return;
                }

                callback();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Logging wrapper.
        /// </summary>
        private void Trace(string message, params object[] replacements)
        {
            Log.Info(this, "[Anchor Id={0}] " + message, replacements);
        }
    }

    /// <summary>
    /// Implementation for HoloLens.
    /// </summary>
    public class HoloLensAnchorStore : IAnchorStore
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
        /// Manages all scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

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
        /// Constructor.
        /// </summary>
        public HoloLensAnchorStore(
            IBootstrapper bootstrapper,
            IMetricsService metrics,
            IAppSceneManager scenes,
            ApplicationConfig config)
        {
            _bootstrapper = bootstrapper;
            _metrics = metrics;
            _scenes = scenes;
            _config = config;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Setup()
        {
            //_scenes = scenes;
            
            // listen for tracking loss event
            WorldManager.OnPositionalLocatorStateChanged += WorldManager_OnPositionalLocatorStateChanged;
            
            return LoadStore().Token();
        }

        /// <inheritdoc />
        public void Teardown()
        {
            // ??
        }

        /// <inheritdoc />
        public void Anchor(string id, int version, GameObject gameObject)
        {
            var ids = _store.GetAllIds();
            for (int i = 0, len = ids.Length; i < len; i++)
            {
                if (id == ids[i])
                {
                    var anchor = _store.Load(id, gameObject);
                    if (null == anchor)
                    {
                        //return new AsyncToken<Void>(new Exception("WorldAnchorStore::Load completed but did not return an anchor."));
                    }
                    
                    TrackUnlocatedAnchor(anchor);

                    //return new AsyncToken<Void>(Void.Instance);
                }
            }

            //return new AsyncToken<Void>(new Exception("No anchor by that id."));
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
            return null;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Import(string id, byte[] bytes, GameObject gameObject)
        {
            return null;
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