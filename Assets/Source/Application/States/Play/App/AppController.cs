﻿using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Loads and manages an app.
    /// </summary>
    public class AppController : IAppController
    {
        /// <summary>
        /// Loads app data.
        /// </summary>
        private readonly IAppDataLoader _loader;

        /// <summary>
        /// Creates scenes from data and manages them.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Pipe for all element updates.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Connection to trellis.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// True iff the app has been loaded.
        /// </summary>
        private bool _isLoaded;

        /// <inheritdoc />
        public string Id { get; private set; }

        /// <inheritdoc />
        public bool CanEdit { get; private set; }

        /// <inheritdoc />
        public bool CanDelete { get; private set; }

        /// <inheritdoc />
        public event Action OnReady;

        /// <inheritdoc />
        public IAppSceneManager Scenes
        {
            get { return _scenes; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppController(
            IAppDataLoader loader,
            IAppSceneManager scenes,
            IElementTxnManager txns,
            IConnection connection,
            IMetricsService metrics)
        {
            _loader = loader;
            _scenes = scenes;
            _txns = txns;
            _connection = connection;
            _metrics = metrics;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Load(string appId)
        {
            Id = appId;

            CanEdit = true;
            CanDelete = true;

            LogVerbose("Load().");

            // metrics
            var loadId = _metrics.Timer(MetricsKeys.APP_DATA_LOAD).Start();

            return _loader
                .Load(appId)
                .OnSuccess(_ =>
                {
                    // metrics
                    _metrics.Timer(MetricsKeys.APP_DATA_LOAD).Stop(loadId);

                    _isLoaded = true;
                })
                .OnFailure(ex =>
                {
                    // metrics
                    _metrics.Timer(MetricsKeys.APP_DATA_LOAD).Abort(loadId);
                })
                .Token();
        }

        /// <inheritdoc />
        public void Unload()
        {
            _isLoaded = false;

            _loader.Unload();
            _scenes.Uninitialize();
            _txns.Uninitialize();
        }

        /// <inheritdoc />
        public void Play()
        {
            if (!_isLoaded)
            {
                throw new Exception("App has not been loaded!");
            }

            _scenes
                .Initialize(Id, _loader)
                .OnSuccess(_ =>
                {
                    var authenticate = _connection.IsConnected;

                    // webgl is special
                    #if UNITY_WEBGL
                    authenticate = true;
                    #endif

                    _txns
                        .Initialize(new AppTxnConfiguration
                        {
                            AppId = Id,
                            Scenes =_scenes,
                            AuthenticateTxns = authenticate 
                        })
                        .OnSuccess(__ =>
                        {
                            Log.Info(this, "Txns initialized.");

                            if (null != OnReady)
                            {
                                OnReady();
                            }
                        })
                        .OnFailure(exception => Log.Error(this, "Could not initialize txns : {0}.", exception));
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not initialize scenes : {0}.", exception);
                });
        }
        
        /// <summary>
        /// Logging.
        /// </summary>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}