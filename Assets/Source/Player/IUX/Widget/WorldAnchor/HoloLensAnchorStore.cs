#if !UNITY_EDITOR && UNITY_WSA

using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using Object = UnityEngine.Object;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an operation that is async.
    /// </summary>
    public interface IAsyncAction
    {
        /// <summary>
        /// Starts the saga.
        /// </summary>
        /// <returns></returns>
        IAsyncToken<Void> Start();
    }

    /// <summary>
    /// Implementation for HoloLens.
    /// </summary>
    public class HoloLensAnchorStore : IAnchorStore
    {
        /// <summary>
        /// Queue of async actions.
        /// </summary>
        private class QueueRecord
        {
            /// <summary>
            /// The saga to execute.
            /// </summary>
            public IAsyncAction Saga;

            /// <summary>
            /// The token.
            /// </summary>
            public AsyncToken<Void> Token;
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMetricsService _metrics;
        private readonly IBootstrapper _bootstrapper;
        private readonly IHttpService _http;

        /// <summary>
        /// Configuration for the application.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Queue of async sagas.
        /// </summary>
        private readonly Queue<QueueRecord> _queue = new Queue<QueueRecord>();

        /// <summary>
        /// Dependencies passed in via Setup.
        /// </summary>
        private IElementTxnManager _txns;
        private IAppSceneManager _scenes;

        /// <summary>
        /// HoloLens API.
        /// </summary>
        private WorldAnchorStore _store;

        /// <summary>
        /// Token for currently executing saga.
        /// </summary>
        private IAsyncToken<Void> _pipelineToken;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HoloLensAnchorStore(
            IMetricsService metrics,
            IBootstrapper bootstrapper,
            IHttpService http,
            ApplicationConfig config)
        {
            _metrics = metrics;
            _bootstrapper = bootstrapper;
            _http = http;
            _config = config;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Setup(
            IElementTxnManager txns,
            IAppSceneManager scenes)
        {
            _txns = txns;
            _scenes = scenes;

            // listen for tracking loss event
            WorldManager.OnPositionalLocatorStateChanged += WorldManager_OnPositionalLocatorStateChanged;
            
            return LoadStore().Token();
        }

        /// <inheritdoc />
        public void Teardown()
        {
            var copy = _queue.ToArray();
            _queue.Clear();

            for (int i = 0, len = copy.Length; i < len; i++)
            {
                var token = copy[i].Token;
                if (null != token)
                {
                    token.Fail(new Exception("Anchor store teardown."));
                }
            }
        }

        /// <inheritdoc />
        public void Anchor(string id, int version, GameObject gameObject)
        {
            // load from store
            var anchorId = $"{id}.{version}";
            if (_store.GetAllIds().Contains(anchorId))
            {
                var anchor = _store.Load(anchorId, gameObject);
                if (null == anchor)
                {
                    Log.Error(this,
                        "WorldAnchorStore load completed but did not return an anchor for id '{0}'.",
                        id);
                }

                TrackUnlocatedAnchor(anchor);

                return;
            }

            // could not load anchor, so add a saga to import it
            _queue.Enqueue(new QueueRecord
            {
                Saga = new ImportPipelineSaga(
                    _bootstrapper,
                    _http,
                    _scenes,
                    _store,
                    id,
                    version,
                    gameObject,
                    ImportPipelineSaga.ImportOptions.Compressed)
            });

            ProcessPipeline();
        }
        
        /// <inheritdoc />
        public void UnAnchor(GameObject gameObject)
        {
            if (!gameObject)
            {
                return;
            }

            var anchor = gameObject.GetComponent<WorldAnchor>();
            if (!anchor)
            {
                Object.Destroy(anchor);
            }
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Export(string id, int version, GameObject gameObject)
        {
            var token = new AsyncToken<Void>();

            _queue.Enqueue(new QueueRecord
            {
                Saga = new ExportPipelineSaga(
                    _bootstrapper,
                    _http,
                    _txns,
                    _scenes,
                    _store,
                    id,
                    version,
                    _config.Play.AppId,
                    gameObject,
                    ExportPipelineSaga.ExportOptions.Compressed),
                Token = token
            });

            return token;
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

            // reload all anchors
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

        /// <summary>
        /// Starts on next thing in the pipeline if necessary.
        /// </summary>
        private void ProcessPipeline()
        {
            if (0 == _queue.Count)
            {
                return;
            }

            if (null == _pipelineToken)
            {
                var record = _queue.Dequeue();

                _pipelineToken = record.Saga.Start();

                // chain
                if (null != record.Token)
                {
                    _pipelineToken.OnSuccess(record.Token.Succeed);
                    _pipelineToken.OnFailure(record.Token.Fail);
                }

                _pipelineToken
                    .OnFailure(ex => Log.Error(this, "Anchor import failed: {0}", ex))
                    .OnFinally(_ => ProcessPipeline());
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

            void Handler(WorldAnchor _, bool isLocated)
            {
                anchor.OnTrackingChanged -= Handler;

                _metrics.Timer(MetricsKeys.ANCHOR_UNLOCATED).Stop(metricId);
            }

            // listen
            anchor.OnTrackingChanged += Handler;
        }

        /// <summary>
        /// Called when locator state changes.
        /// </summary>
        private void WorldManager_OnPositionalLocatorStateChanged(
            PositionalLocatorState prev,
            PositionalLocatorState next)
        {
            if (next == PositionalLocatorState.Active)
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