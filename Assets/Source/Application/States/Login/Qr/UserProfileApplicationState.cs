using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Network data cached on disk.
    /// </summary>
    public class UserProfileCacheData
    {
        /// <summary>
        /// Info about an app.
        /// </summary>
        public class AppInfo
        {
            /// <summary>
            /// Name of the app.
            /// </summary>
            public string Name;

            /// <summary>
            /// Id of the app.
            /// </summary>
            public string Id;
        }

        /// <summary>
        /// List of all apps.
        /// </summary>
        public AppInfo[] Apps;
    }

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
        /// Api calls.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Files.
        /// </summary>
        private readonly IFileManager _files;

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
            ApiController api,
            IMessageRouter messages,
            IFileManager files)
        {
            _config = config;
            _messages = messages;
            _api = api;
            _files = files;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entered {0}.", GetType().Name);

            _root = new GameObject("UserRoot");

            var uri = "userdata://userprofile";

            // get apps
            _api
                .Apps
                .GetMyApps()
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        Log.Info(this, "Loaded UserProfileCacheData from network.");

                        var cacheData = new UserProfileCacheData
                        {
                            Apps = response.Payload.Body
                                .Select(body => new UserProfileCacheData.AppInfo
                                {
                                    Id = body.Id,
                                    Name = body.Name
                                })
                                .ToArray()
                        };

                        _files
                            .Set(uri, cacheData)
                            .OnFailure(exception => Log.Error(this, "Could not save UserProfileCacheData : {0}.", exception));

                        OpenUserSplash(cacheData);
                    }
                    else
                    {
                        Log.Error(this, "Server refused to get my apps : {0}.", response.Payload.Error);

                        // TODO: Show error panel.
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Info(this, "Could not get my apps : {0}.", exception);

                    // okay... try from disk
                    _files
                        .Get<UserProfileCacheData>(uri)
                        .OnSuccess(file =>
                        {
                            Log.Info(this, "Loaded UserProfileCacheData from disk.");

                            OpenUserSplash(file.Data);
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
            Object.Destroy(_root);
        }

        /// <summary>
        /// Opens the splash.
        /// </summary>
        /// <param name="cacheData">Cache data.</param>
        private void OpenUserSplash(UserProfileCacheData cacheData)
        {
            _userSplash = _root.AddComponent<UserSplashMenuController>();
            _userSplash.OnAppSelected += UserSplash_OnAppSelected;
            _userSplash.Initialize(cacheData);
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