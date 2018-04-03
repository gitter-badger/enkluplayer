using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for user information.
    /// </summary>
    public class UserProfileApplicationState : IState
    {
        /// <summary>
        /// App-wide config.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Root game object.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// User splash menu.
        /// </summary>
        private UserSplashMenuController _userSplash;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UserProfileApplicationState(
            ApplicationConfig config,
            IMessageRouter messages,
            ApiController api)
        {
            _config = config;
            _messages = messages;
            _api = api;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entered {0}.", GetType().Name);

            _root = new GameObject("UserRoot");

            // get apps
            _api
                .Apps
                .GetMyApps()
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        _userSplash = _root.AddComponent<UserSplashMenuController>();
                        _userSplash.OnAppSelected += UserSplash_OnAppSelected;
                        _userSplash.Initialize(response.Payload.Body);
                    }
                    else
                    {
                        Log.Error(this, "Server refused to get my apps : {0}.", response.Payload.Error);
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not get my apps : {0}.", exception);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            Object.Destroy(_root);
        }

        /// <summary>
        /// Called when the user splash controller selects an app.
        /// </summary>
        /// <param name="appId">The id of the app to load.</param>
        private void UserSplash_OnAppSelected(string appId)
        {
            _config.Play.AppId = appId;

            _messages.Publish(MessageTypes.LOAD_APP);
        }

        /// <summary>
        /// Called when the user asks to take a world scan.
        /// </summary>
        private void UserSplash_OnWorldScan()
        {
            _messages.Publish(MessageTypes.MESHCAPTURE);
        }
    }
}