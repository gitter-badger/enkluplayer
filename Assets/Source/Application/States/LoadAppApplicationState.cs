﻿using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that loads necessary data before progressing to play. This is used
    /// when the player is running without a connected editor to push it data.
    /// </summary>
    public class LoadAppApplicationState : IState
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Trellis connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Controls apps.
        /// </summary>
        private readonly IAppController _app;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadAppApplicationState(
            ApplicationConfig config,
            IMessageRouter messages,
            IConnection connection,
            IAppController app)
        {
            _config = config;
            _messages = messages;
            _connection = connection;
            _app = app;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Loading app...");

            // TODO: Show loading screen.

            _app
                .Load(_config.Play.AppId)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "App loaded.");

                    Log.Info(this, "Connecting to Trellis...");

                    _connection
                        .Connect(_config.Network.Environment)
                        .OnFailure(exception =>
                        {
                            Log.Error(this, "Could not connect to Trellis : {0}.", exception);
                        })
                        .OnFinally(__ =>
                        {
                            Log.Info(this, "Moving to play mode.");

                            _messages.Publish(MessageTypes.PLAY);
                        });
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load app : {0}.", exception);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            // TODO: Hide loading screen.
        }
    }
}
