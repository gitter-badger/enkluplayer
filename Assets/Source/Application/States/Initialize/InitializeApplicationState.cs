using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
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
        private readonly IWorldAnchorProvider _anchors;
        private readonly IAppSceneManager _scenes;
        private readonly CommandService _commands;
        private readonly BleServiceConfiguration _bleConfig;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InitializeApplicationState(
            IMessageRouter messages,
            IAssetManager assets,
            IAssetLoader assetLoader,
            IBleService ble,
            IWorldAnchorProvider anchors,
            IAppSceneManager scenes,
            CommandService commands,
            BleServiceConfiguration bleConfig)
        {
            _messages = messages;
            _assets = assets;
            _assetLoader = assetLoader;            
            _ble = ble;
            _anchors = anchors;
            _scenes = scenes;
            _commands = commands;
            _bleConfig = bleConfig;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // ble
            _ble.Setup(_bleConfig);
            
            // reset assets
            _assets.Uninitialize();

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
                _anchors.Initialize(_scenes)
            };
            
            Async
                .All(tasks.ToArray())
                .OnSuccess(_ =>
                {
                    _messages.Publish(
                        MessageTypes.APPLICATION_INITIALIZED,
                        Void.Instance);
                })
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
                target = new CommandClientLogTarget(new DefaultLogFormatter
                {
                    Level = true,
                    ObjectToString = true,
                    Timestamp = true,
                    TypeName = true
                });

                Log.AddLogTarget(target);

                target.Add(client);
            }

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