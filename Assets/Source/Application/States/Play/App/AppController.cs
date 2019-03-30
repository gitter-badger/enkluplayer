using System;
using System.Collections.Generic;
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
        /// Multiplayer.
        /// </summary>
        private readonly IMultiplayerController _multiplayer;

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
        public event Action OnReady;
        
        /// <inheritdoc />
        public event Action OnUnloaded;

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
            IMetricsService metrics,
            IMultiplayerController multiplayer)
        {
            _loader = loader;
            Scenes = scenes;
            _txns = txns;
            _connection = connection;
            _metrics = metrics;
            _multiplayer = multiplayer;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Load(PlayAppConfig config)
        {
            _config = config;

            Id = _config.AppId;
            
            // load data
            var tokens = new List<IAsyncToken<Void>>
            {
                _loader.Load(_config)
            };

            if (!config.Edit)
            {
                tokens.Add(_multiplayer.Initialize());
            }

            return Async
                .All(tokens.ToArray())
                .OnSuccess(_ =>
                {
                    Name = _loader.Name;

                    _isLoaded = true;
                })
                .Map(_ => Void.Instance);
        }

        /// <inheritdoc />
        public void Unload()
        {
            _isLoaded = false;

            _multiplayer.Disconnect();
            _loader.Unload();
            Scenes.Uninitialize();
            _txns.Uninitialize();
            
            OnUnloaded.Execute();
        }

        /// <inheritdoc />
        public void Play()
        {
            if (!_isLoaded)
            {
                throw new Exception("App has not been loaded!");
            }

            Log.Info(this, "Starting play mode.");

            var playId = _metrics.Timer(MetricsKeys.APP_PLAY).Start();

            // allow multiplayer to apply diff before we create the scene
            _multiplayer.ApplyDiff(_loader);

            // now create scene
            Scenes
                .Initialize(Id, _loader)
                .OnSuccess(_ =>
                {
                    _metrics.Timer(MetricsKeys.APP_PLAY).Stop(playId);

                    // now switch multiplayer on
                    _multiplayer.Play();

                    if (null != OnReady)
                    {
                        OnReady();
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not initialize scenes : {0}.", exception);

                    _metrics.Timer(MetricsKeys.APP_PLAY).Abort(playId);
                });
        }

        /// <inheritdoc />
        public void Edit()
        {
            if (!_isLoaded)
            {
                throw new Exception("App has not been loaded!");
            }

            var editId = _metrics.Timer(MetricsKeys.APP_EDIT).Start();

            Log.Info(this, "Starting Edit mode.");

            // create scene
            Scenes
                .Initialize(Id, _loader)
                .OnSuccess(_ =>
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
                            Log.Info(this, "Edit mode started.");

                            _metrics.Timer(MetricsKeys.APP_EDIT).Stop(editId);

                            if (null != OnReady)
                            {
                                OnReady();
                            }
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this, "Could not initialize txns for edit mode : {0}.", exception);

                            _metrics.Timer(MetricsKeys.APP_EDIT).Abort(editId);
                        });
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not initialize scenes for edit mode : {0}.", exception);

                    _metrics.Timer(MetricsKeys.APP_EDIT).Abort(editId);
                });
        }
    }
}