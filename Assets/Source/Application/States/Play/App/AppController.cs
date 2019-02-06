using System;
using System.Diagnostics;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using Enklu.Mycelium.Messages;
using Enklu.Mycerializer;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    public class MultiplayerController
    {
        private readonly ApplicationConfig _config;
        private readonly ReflectionMessageReader _reader = new ReflectionMessageReader();
        private readonly ReflectionMessageWriter _writer = new ReflectionMessageWriter();

        private TcpConnection _tcp;
        private AsyncToken<Void> _connect;

        public MultiplayerController(ApplicationConfig config)
        {
            _config = config;
        }

        public IAsyncToken<Void> Connect()
        {
            if (null == _connect)
            {
                _connect = new AsyncToken<Void>();

                Log.Info(this, "Connecting to {0}:{1}.",
                    _config.Network.Environment.MyceliumUrl,
                    _config.Network.Environment.MyceliumPort);

                _tcp = new TcpConnection(
                    new LengthBasedSocketMessageReader(2, OnMessageRead),
                    new LengthBasedSocketMessageWriter(2));
                _tcp.OnConnectionOpened += Tcp_OnConnectionOpened;
                _tcp.Connect(
                    _config.Network.Environment.MyceliumUrl,
                    _config.Network.Environment.MyceliumPort);
            }
            
            return _connect.Token();
        }

        private void Tcp_OnConnectionOpened()
        {
            _connect.Succeed(Void.Instance);
        }

        private void OnMessageRead(ArraySegment<byte> bytes)
        {
            var stream = new ByteStream(bytes.Array, bytes.Offset);

            // read type
            var id = stream.ReadUnsignedShort();
            Type type;
            try
            {
                type = MyceliumMessagesMap.Get(id);
            }
            catch
            {
                Log.Error(this, "Unknown message type {0}.", id);
                return;
            }

            object message;
            try
            {
                message = _reader.Read(type, stream);
            }
            catch (Exception exception)
            {
                Log.Error(this, "Could not read from stream: {0}.", exception);
                return;
            }

            Log.Info(this, "Received a {0}.", message);
        }
    }

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

        private MultiplayerController _multiplayer;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _appConfig;

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
            IMetricsService metrics,
            ApplicationConfig config)
        {
            _loader = loader;
            Scenes = scenes;
            _txns = txns;
            _connection = connection;
            _metrics = metrics;
            _appConfig = config;
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
                        _multiplayer = new MultiplayerController(_appConfig);
                        _multiplayer
                            .Connect()
                            .OnSuccess(v =>
                            {
                                _metrics.Timer(MetricsKeys.APP_PLAY).Stop(playId);

                                Log.Info(this, "Connected!!!!!!!");

                                if (null != OnReady)
                                {
                                    OnReady();
                                }
                            })
                            .OnFailure(ex =>
                            {
                                Log.Error(this, "Cannot connect to multiplayer: {0}.", ex);
                            });
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