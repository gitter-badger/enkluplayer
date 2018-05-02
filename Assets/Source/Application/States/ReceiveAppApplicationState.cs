using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that waits for receiving all necessary data before progressing to
    /// play. This state is used when the editor is connected.
    /// </summary>
    public class ReceiveAppApplicationState : IState
    {
        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Connection to Trellis.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReceiveAppApplicationState(
            IMessageRouter messages,
            IHttpService http,
            IConnection connection,
            ApplicationConfig config,
            MessageTypeBinder binder)
        {
            _messages = messages;
            _http = http;
            _connection = connection;
            _config = config;
            
            binder.Add<UserCredentialsEvent>(MessageTypes.RECV_CREDENTIALS);
            binder.Add<AppInfoEvent>(MessageTypes.RECV_APP_INFO);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            var waits = new Action<Action>[]
            {
                WaitForCredentials,
                WaitForAppInfo,
                WaitForAssets,
                WaitForScripts
            };

            var calls = 0;
            Action callback = () =>
            {
                if (++calls == waits.Length)
                {
                    Log.Info(this, "Prerequisites accounted for. Proceed to play app!");

                    _messages.Publish(MessageTypes.PLAY);
                }
                else
                {
                    Log.Info(this, "Still waiting on {0} item(s).", waits.Length - calls);
                }
            };

            for (int i = 0, len = waits.Length; i < len; i++)
            {
                waits[i](callback);
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            // 
        }

        /// <inheritdoc />
        public void Exit()
        {
            // 
        }

        /// <summary>
        /// Waits for credentials to be passed in.
        /// </summary>
        /// <param name="callback">The action to call once credentials have been received.</param>
        private void WaitForCredentials(Action callback)
        {
            Log.Info(this, "Waiting on credentials.");

            _messages.SubscribeOnce(
                MessageTypes.RECV_CREDENTIALS,
                obj =>
                {
                    var message = (UserCredentialsEvent) obj;

                    Log.Info(this, "Credentials received : {0}.", message);
                    
                    // update ApplicationConfig
                    var creds = _config.Network.Credentials;
                    creds.UserId = message.Profile.Id;
                    creds.Token = message.Credentials.Token;
                    creds.Email = message.Profile.Email;

                    // setup http service
                    creds.Apply(_http);

                    // connect to Trellis
                    _connection
                        .Connect(_config.Network.Environment)
                        .OnSuccess(_ =>
                        {
                            Log.Info(this, "Connected to Trellis.");

                            callback();
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this, "Could not connect to Trellis : {0}.", exception);

                            callback();
                        });
                });
        }

        /// <summary>
        /// Wais for app info to come through.
        /// </summary>
        /// <param name="callback">Callback once app info has been received.</param>
        private void WaitForAppInfo(Action callback)
        {
            Log.Info(this, "Waiting on AppInfo.");

            _messages.SubscribeOnce(
                MessageTypes.RECV_APP_INFO,
                obj =>
                {
                    var info = (AppInfoEvent) obj;

                    Log.Info(this, "App info received : {0}.", info);

                    // update application config
                    _config.Play.AppId = info.AppId;

                    callback();
                });
        }

        /// <summary>
        /// Waits for assets.
        /// </summary>
        /// <param name="callback">Callback.</param>
        private void WaitForAssets(Action callback)
        {
            Log.Info(this, "Waiting for assets.");

            _messages.SubscribeOnce(
                MessageTypes.RECV_ASSET_LIST,
                obj =>
                {
                    Log.Info(this, "Assets received.");

                    // assets service takes care of them

                    callback();
                });
        }

        /// <summary>
        /// Waits for scripts.
        /// </summary>
        /// <param name="callback">Callback.</param>
        private void WaitForScripts(Action callback)
        {
            Log.Info(this, "Waiting for scripts.");

            _messages.SubscribeOnce(
                MessageTypes.RECV_SCRIPT_LIST,
                obj =>
                {
                    Log.Info(this, "Scripts received.");

                    // scripts service takes care of them

                    callback();
                });
        }
    }
}