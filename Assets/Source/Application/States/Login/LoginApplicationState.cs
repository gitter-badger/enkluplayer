using System.IO;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Decides how to login.
    /// </summary>
    public class LoginApplicationState : IState
    {
        /// <summary>
        /// Credentials.
        /// </summary>
        private const string CREDS = "login://DefaultCredentials";
        private const string PREFERENCES_PREFIX = "login://Preferences/";

        /// <summary>
        /// Reads and writes files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Pub/sub interface.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Http implementation.
        /// </summary>
        private readonly IHttpService _http;
        
        /// <summary>
        /// Strategy for logging in.
        /// </summary>
        private readonly ILoginStrategy _strategy;

        /// <summary>
        /// Controls API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginApplicationState(
            IFileManager files,
            IMessageRouter messages,
            ILoginStrategy strategy,
            IHttpService http,
            ApiController api,
            ApplicationConfig config)
        {
            _files = files;
            _messages = messages;
            _strategy = strategy;
            _http = http;
            _api = api;
            _config = config;

            _files.Register(
                "login://",
                new JsonSerializer(),
                new LocalFileSystem(Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "Login")));
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "LoginApplicationState::Enter");

            // check disk cache for credentials
            if (_files.Exists(CREDS))
            {
                _files
                    .Get<CredentialsData>(CREDS)
                    .OnSuccess(file =>
                    {
                        // load into default app
                        Log.Info(this, "Credentials loaded from disk.");

                        LoadDefaultApp(file.Data);
                    })
                    .OnFailure(exception =>
                    {
                        Log.Error(this, "Could not load credential information: {0}", exception);

                        Login();
                    });
            }
            else
            {
                // nothing on disk, fresh login
                Login();
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            
        }

        /// <summary>
        /// Logs in and obtains credentials object.
        /// </summary>
        private void Login()
        {
            // different platforms have different login strategies
            _strategy
                .Login()
                .OnSuccess(credentials =>
                {
                    Log.Info(this, "Logged in.");
                    Log.Info(this, "Saving credentials to disk.");

                    _files
                        .Set(CREDS, credentials)
                        .OnFailure(exception => Log.Error(this, "Could not write credentials to disk : {0}.", exception));

                    LoadDefaultApp(credentials);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not log in. Now what? {0}", exception);
                });
        }

        /// <summary>
        /// Loads default app.
        /// </summary>
        /// <param name="credentials"></param>
        private void LoadDefaultApp(CredentialsData credentials)
        {
            // setup filemanager
            _files.Register(
                "appdata://",
                new JsonSerializer(),
                new LocalFileSystem(Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    Path.Combine("AppData", credentials.UserId))));
            _files.Register(
                "userdata://",
                new JsonSerializer(),
                new LocalFileSystem(Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    Path.Combine("UserData", credentials.UserId))));

            // apply credentials to http service
            credentials.Apply(_http);

            // find and replace credentials in config
            var creds = _config.Network.Credentials;
            if (null == creds)
            {
                _config.Network.AllCredentials = _config.Network.AllCredentials.Add(credentials);
            }
            else
            {
                creds.Token = credentials.Token;
                creds.UserId = credentials.UserId;
            }

            // load preferences from cache
            var path = PREFERENCES_PREFIX + credentials.UserId;
            if (_files.Exists(path))
            {
                _files
                    .Get<LoginPreferenceData>(path)
                    .OnSuccess(file =>
                    {
                        Log.Info(this, "Most recent app id found on disk.");

                        LoadApp(file.Data.MostRecentAppId);
                    });
            }
            else
            {
                // nothing cached, choose an app to load into
                ChooseDefaultApp();
            }    
        }

        /// <summary>
        /// Chooses a default app by peeking through all user's apps.
        /// </summary>
        private void ChooseDefaultApp()
        {
            Log.Info(this, "Choosing a default app.");

            _api
                .Apps
                .GetMyApps()
                .OnSuccess(response =>
                {
                    // for now, pick first app or null
                    var apps = response.Payload.Body;
                    if (apps.Length > 0)
                    {
                        var appId = apps[0].Id;

                        _files.Set(
                            PREFERENCES_PREFIX + _config.Network.Credentials.UserId,
                            new LoginPreferenceData
                            {
                                MostRecentAppId = appId
                            });

                        // good gravy, load the app already
                        LoadApp(appId);
                    }
                    else
                    {
                        // TODO: user has no apps, display special screen
                    }
                })
                .OnFailure(exception =>
                {
                    // TODO: error retrieving apps, allow retry
                });
        }

        /// <summary>
        /// Loads an app.
        /// </summary>
        /// <param name="appId">The id of the app to load.</param>
        private void LoadApp(string appId)
        {
            _config.Play.AppId = appId;
            
            _messages.Publish(MessageTypes.LOAD_APP);
        }
    }
}