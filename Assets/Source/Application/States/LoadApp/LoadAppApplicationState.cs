using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.EnkluPlayer
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
        /// UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Tracks app load.
        /// </summary>
        private IAsyncToken<Void> _loadToken;

        /// <summary>
        /// Id of the loading stack.
        /// </summary>
        private int _loadingStackId;

        /// <summary>
        /// Id of the error stack.
        /// </summary>
        private int _errorStackId;

        /// <summary>
        /// Unique id of the timer.
        /// </summary>
        private int _timerId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoadAppApplicationState(
            ApplicationConfig config,
            IMessageRouter messages,
            IConnection connection,
            IAppController app,
            IUIManager ui,
            IMetricsService metrics)
        {
            _config = config;
            _messages = messages;
            _connection = connection;
            _app = app;
            _ui = ui;
            _metrics = metrics;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering LoadAppApplicationState.");

            _timerId = _metrics.Timer(MetricsKeys.STATE_LOAD).Start();

            // open loading UI
            _ui
                .Open<ICommonLoadingView>(new UIReference
                {
                    UIDataId = UIDataIds.LOADING
                }, out _loadingStackId);

            // load app
            _loadToken = _app.Load(_config.Play);
            _loadToken
                .OnSuccess(_ =>
                {
                    Log.Info(this, "App loaded.");

                    if (_config.Play.Edit)
                    {
                        Log.Info(this, "Connecting to Trellis...");

                        _connection
                            .Connect(_config.Network.Environment)
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not connect to Trellis : {0}.", exception);

                                // That's okay.
                            })
                            .OnFinally(__ =>
                            {
                                Log.Info(this, "Moving to play mode.");

                                _messages.Publish(MessageTypes.PLAY);
                            });
                    }
                    else
                    {
                        Log.Info(this, "Moving to play mode.");

                        _messages.Publish(MessageTypes.PLAY);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load app : {0}.", exception);

                    _ui.Close(_loadingStackId);

                    // show panel
                    _ui
                        .Open<ICommonErrorView>(new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        }, out _errorStackId)
                        .OnSuccess(element =>
                        {
                            element.Message = "Oops! Could not load this app. Please check your connection.";
                            element.Action = "My Apps";
                            element.OnOk += Error_OnOk;
                        })
                        .OnFailure(ex =>
                        {
                            Log.Error(this, "Could not open ERROR POPUP : {0}", ex);
                        });
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _loadToken.Abort();

            _ui.Close(_loadingStackId);
            _ui.Close(_errorStackId);

            _metrics.Timer(MetricsKeys.STATE_LOAD).Stop(_timerId);
        }

        /// <summary>
        /// Called when the error UI is complete.
        /// </summary>
        private void Error_OnOk()
        {
            _ui.Close(_errorStackId);

            // go to user profile
            _messages.Publish(MessageTypes.USER_PROFILE);
        }
    }
}
