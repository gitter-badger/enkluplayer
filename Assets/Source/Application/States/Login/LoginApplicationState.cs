using System;
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
        private const string CREDS = "login.creds";
        private const string PREFERENCES_PREFIX = "login.preferences.";

        /// <summary>
        /// Caches bytes on disk.
        /// </summary>
        private readonly IDiskCache _cache;

        /// <summary>
        /// Pub/sub interface.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Http implementation.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly ISerializer _serializer;

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
            IDiskCache cache,
            IMessageRouter messages,
            ISerializer serializer,
            ILoginStrategy strategy,
            IHttpService http,
            ApiController api,
            ApplicationConfig config)
        {
            _cache = cache;
            _messages = messages;
            _serializer = serializer;
            _strategy = strategy;
            _http = http;
            _api = api;
            _config = config;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "LoginApplicationState::Enter");

            // check disk cache for credentials
            if (_cache.Contains(CREDS))
            {
                _cache
                    .Load(CREDS)
                    .OnSuccess(bytes =>
                    {
                        var data = bytes;
                        object obj;

                        try
                        {
                            _serializer.Deserialize(
                                typeof(CredentialsData),
                                ref data,
                                out obj);
                        }
                        catch (Exception exception)
                        {
                            Log.Error(this, "Could not deserialize saved credentials: {0}", exception);

                            // skip this and login
                            Login();
                            return;
                        }

                        // load into default app
                        Log.Info(this, "Credentials loaded from disk.");

                        LoadDefaultApp((CredentialsData) obj);
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
                    
                    try
                    {
                        byte[] bytes;
                        _serializer.Serialize(credentials, out bytes);

                        // save to cache
                        Log.Info(this, "Saving credentials to disk.");

                        _cache.Save(CREDS, bytes);
                    }
                    catch (Exception exception)
                    {
                        var message = string.Format("Could not serialize login credentials : {0}.", exception);

                        Log.Error(this, message);
                    }

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
            if (_cache.Contains(path))
            {
                _cache
                    .Load(path)
                    .OnSuccess(bytes =>
                    {
                        object obj;
                        try
                        {
                            _serializer.Deserialize(typeof(LoginPreferenceData), ref bytes, out obj);
                        }
                        catch (Exception exception)
                        {
                            Log.Error(this, "Could not deserialize preferences: {0}.", exception);

                            // preference fail, we'll need to regenerate them
                            ChooseDefaultApp();
                            return;
                        }

                        Log.Info(this, "Most recent app id found on disk.");

                        LoadApp(((LoginPreferenceData) obj).MostRecentAppId);
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
                        var preferences = new LoginPreferenceData
                        {
                            MostRecentAppId = appId
                        };

                        byte[] bytes;
                        try
                        {
                            _serializer.Serialize(preferences, out bytes);

                            Log.Info(this, "Writing preferences to disk.");

                            _cache.Save(
                                PREFERENCES_PREFIX + _config.Network.Credentials.UserId,
                                bytes);
                        }
                        catch (Exception exception)
                        {
                            Log.Error(this, "Could not write preferences to disk: {0}.", exception);
                        }

                        // good gravy, load the app already
                        LoadApp(appId);
                    }
                    else
                    {
                        // user has no apps, display special screen
                    }
                })
                .OnFailure(exception =>
                {
                    // error retrieving apps, allow retry
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