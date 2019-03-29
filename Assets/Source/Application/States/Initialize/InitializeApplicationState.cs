using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.BLE;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Initializes the application.
    /// </summary>
    public class InitializeApplicationState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;
        private readonly IAssetManager _assets;
        private readonly IAssetLoader _assetLoader;
        private readonly IBleService _ble;
        private readonly IGestureManager _gestures;
        private readonly IAnchorManager _anchors;
        private readonly IMetricsService _metrics;
        private readonly IHttpService _http;
        private readonly CommandService _commands;
        private readonly BleServiceConfiguration _bleConfig;

        /// <summary>
        /// Id for timer.
        /// </summary>
        private int _timerId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IAssetManager assets,
            IAssetLoader assetLoader,
            IBleService ble,
            IGestureManager gestures,
            IAnchorManager anchors,
            IMetricsService metrics,
            IHttpService http,
            CommandService commands,
            BleServiceConfiguration bleConfig)
        {
            _messages = messages;
            _assets = assets;
            _assetLoader = assetLoader;            
            _ble = ble;
            _gestures = gestures;
            _anchors = anchors;
            _metrics = metrics;
            _http = http;
            _commands = commands;
            _bleConfig = bleConfig;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // time it
            _timerId = _metrics.Timer(MetricsKeys.STATE_INIT).Start();

            // ble
            _ble.Setup(_bleConfig);

            // gesture recognition
            _gestures.Initialize();

            // reset assets
            _assets.Uninitialize();

            // set global timeout
            _http.TimeoutMs = long.MaxValue;

            AddCommands();
            
            // wait for tasks to finish
            var tasks = new List<IAsyncToken<Void>>
            {
                // TODO: Move into service.
                _assets.Initialize(new AssetManagerConfiguration
                {
                    Loader = _assetLoader,
                    Queries = new StandardQueryResolver()
                }),
                _anchors.Setup()
            };
            
            Async
                .All(tasks.ToArray())
                .OnSuccess(_ => _messages.Publish(
                    MessageTypes.APPLICATION_INITIALIZED,
                    Void.Instance))
                .OnFailure(exception =>
                {
                    // rethrow
                    throw exception;
                });
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _metrics.Timer(MetricsKeys.STATE_INIT).Stop(_timerId);
        }

        /// <summary>
        /// Adds all commands to command service.
        /// </summary>
        private void AddCommands()
        {
            _commands.SetHandler("log", Commands_OnLog);
        }

        /// <summary>
        /// Handles logging commands.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="client">The client.</param>
        private void Commands_OnLog(string command, ICommandClient client)
        {
            Log.Info(this, "Received log command.");

            // log -l [Debug|...|Fatal] (Optional) --off (Optional)
            //
            // -l sets the log level for all clients.
            // --off turns off logging to specific client.

            // prep the log target first
            var target = Log.Targets.OfType<CommandClientLogTarget>().FirstOrDefault();
            if (null == target)
            {
                target = new CommandClientLogTarget(new ConductorLogFormatter());

                Log.AddLogTarget(target);
            }

            // make sure this client is added to the target
            target.Add(client);

            // configure level
            string level;
            if (CommandParser.Value('l', command, out level))
            {
                target.Filter = (LogLevel) Enum.Parse(typeof(LogLevel), level);
            }

            // handle off
            if (CommandParser.Toggle("off", command))
            {
                target.Remove(client);
            }
        }
    }
}