using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Source.Messages.ToApplication;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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
        /// Loads app data.
        /// </summary>
        private readonly IAppController _app;

        /// <summary>
        /// The bridge.
        /// </summary>
        private readonly IBridge _bridge;

        /// <summary>
        /// Config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Editor settings.
        /// </summary>
        private readonly EditorProxy _editor;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReceiveAppApplicationState(
            IMessageRouter messages,
            IHttpService http,
            IConnection connection,
            IAppController app,
            IBridge bridge,
            BridgeMessageHandler handler,
            ApplicationConfig config,
            EditorProxy editor)
        {
            _messages = messages;
            _http = http;
            _connection = connection;
            _app = app;
            _bridge = bridge;
            _config = config;
            _editor = editor;
            
            handler.Binder.Add<UserCredentialsEvent>(MessageTypes.RECV_CREDENTIALS);
            handler.Binder.Add<AppInfoEvent>(MessageTypes.RECV_APP_INFO);
            handler.Binder.Add<EditorSettingsEvent>(MessageTypes.INITIAL_EDITOR_SETTINGS);

            _bridge.Initialize(handler);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            var waits = new Action<Action>[]
            {
                WaitForCredentials,
                WaitForAppInfo,
                WaitForEditorSettings,
                WaitForAssets,
                WaitForScripts
            };

            var calls = 0;
            Action callback = () =>
            {
                if (++calls == waits.Length)
                {
                    Log.Info(this, "Prerequisites received. Opening connection to Trellis.");

                    // connect to Trellis
                    _connection
                        .Connect(_config.Network.Environment)
                        .OnSuccess(_ =>
                        {
                            Log.Info(this, "Connected to Trellis, finishing app load.");

                            // load
                            _app
                                .Load(_config.Play)
                                .OnSuccess(__ => _messages.Publish(MessageTypes.PLAY))
                                .OnFailure(ex => Log.Error(this, "Could not load app : {0}", ex));
                        })
                        .OnFailure(exception =>
                        {
                            Log.Error(this, "Could not connect to Trellis : {0}.", exception);
                        });
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

            _bridge.BroadcastReady();
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

                    callback();
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
        /// Waits for initial settings to come through.
        /// </summary>
        /// <param name="callback">Callback once app info has been received.</param>
        private void WaitForEditorSettings(Action callback)
        {
            Log.Info(this, "Waiting on editor settings.");

            _messages.SubscribeOnce(
                MessageTypes.INITIAL_EDITOR_SETTINGS,
                obj =>
                {
                    var settingsEvent = (EditorSettingsEvent) obj;
                    Log.Info(this, "Editor settings received : {0}.", settingsEvent);

                    _editor.Settings.PopulateFromEvent(settingsEvent);
                    
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