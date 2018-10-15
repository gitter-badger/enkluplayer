using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
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

        /// <summary>
        /// Configuration.
        /// </summary>
        private PlayAppConfig _config;

        /// <inheritdoc />
        public string Id { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public bool CanEdit { get; private set; }

        /// <inheritdoc />
        public bool CanDelete { get; private set; }

        /// <inheritdoc />
        public event Action OnReady;

        /// <inheritdoc />
        public IAppSceneManager Scenes { get; private set; }

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
            Scenes = scenes;
            _txns = txns;
            _connection = connection;
            _metrics = metrics;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Load(PlayAppConfig config)
        {
            _config = config;

            Id = _config.AppId;

            CanEdit = true;
            CanDelete = true;

            LogVerbose("Load().");

            // metrics
            var loadId = _metrics.Timer(MetricsKeys.APP_DATA_LOAD).Start();
            
            return _loader
                .Load(_config)
                .OnSuccess(_ =>
                {
                    Name = _loader.Name;

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
            Scenes.Uninitialize();
            _txns.Uninitialize();
        }

        /// <inheritdoc />
        public void Play()
        {
            if (!_isLoaded)
            {
                throw new Exception("App has not been loaded!");
            }

            var playId = _metrics.Timer(MetricsKeys.APP_PLAY).Start();

            // init scenes
            Scenes
                .Initialize(Id, _loader)
                .OnSuccess(_ =>
                {
                    // edit mode
                    if (_config.Edit)
                    {
                        var authenticate = DeviceHelper.IsWebGl() || _connection.IsConnected;

                        _txns
                            .Initialize(new AppTxnConfiguration
                            {
                                AppId = Id,
                                Scenes = Scenes,
                                AuthenticateTxns = authenticate
                            })
                            .OnSuccess(__ =>
                            {
                                Log.Info(this, "Txns initialized.");

                                _metrics.Timer(MetricsKeys.APP_PLAY).Stop(playId);

                                if (null != OnReady)
                                {
                                    OnReady();
                                }
                            })
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not initialize txns : {0}.", exception);

                                _metrics.Timer(MetricsKeys.APP_PLAY).Abort(playId);
                            });
                    }
                    // play mode
                    else
                    {
                        _metrics.Timer(MetricsKeys.APP_PLAY).Stop(playId);

                        if (null != OnReady)
                        {
                            OnReady();
                        }
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not initialize scenes : {0}.", exception);

                    _metrics.Timer(MetricsKeys.APP_PLAY).Abort(playId);
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