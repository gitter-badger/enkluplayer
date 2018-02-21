using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    public class ReceiveAppApplicationState : IState
    {
        private readonly IMessageRouter _messages;
        private readonly ApplicationConfig _config;

        public ReceiveAppApplicationState(
            IMessageRouter messages,
            ApplicationConfig config,
            MessageTypeBinder binder)
        {
            _messages = messages;
            _config = config;
            
            binder.Add<UserCredentialsEvent>(MessageTypes.RECV_CREDENTIALS);
            binder.Add<AppInfoEvent>(MessageTypes.RECV_APP_INFO);
        }

        public void Enter(object context)
        {
            var waits = new Action<Action>[]
            {
                WaitForCredentials,
                WaitForAppInfo
            };

            var calls = 0;
            Action callback = () =>
            {
                if (++calls == waits.Length)
                {
                    _messages.Publish(MessageTypes.LOAD_APP);
                }
            };

            for (int i = 0, len = waits.Length; i < len; i++)
            {
                waits[i](callback);
            }
        }

        public void Update(float dt)
        {
            // 
        }

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
                    var creds = _config.Network.Credentials(_config.Network.Current);
                    creds.UserId = message.Profile.Id;
                    creds.Token = message.Credentials.Token;
                    creds.Email = message.Profile.Email;

                    // return flow
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
    }
}