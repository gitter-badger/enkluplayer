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

        /// <summary>
        /// Caches bytes on disk.
        /// </summary>
        private readonly IDiskCache _cache;

        /// <summary>
        /// Pub/sub interface.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Controls API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly ISerializer _serializer;

        /// <summary>
        /// Strategy for logging in.
        /// </summary>
        private readonly ILoginStrategy _strategy;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginApplicationState(
            IDiskCache cache,
            IMessageRouter messages,
            ISerializer serializer,
            ILoginStrategy strategy)
        {
            _cache = cache;
            _messages = messages;
            _serializer = serializer;
            _strategy = strategy;
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
                        }

                        // load into default app
                        Log.Info(this, "Credentials loaded from disk.");

                        LoadDefaultApp();
                    })
                    .OnFailure(exception =>
                    {
                        Log.Error(this, "Could not load credential information: {0}", exception);

                        Login();
                    });
            }
            else
            {
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

        private void Login()
        {
            // login
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
                        _cache.Save(CREDS, bytes);
                    }
                    catch (Exception exception)
                    {
                        var message = string.Format("Could not serialize login credentials : {0}.", exception);

                        Log.Error(this, message);
                    }

                    LoadDefaultApp();
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not log in. Now what? {0}", exception);
                });
        }

        /// <summary>
        /// Loads default app.
        /// </summary>
        private void LoadDefaultApp()
        {
            _api
                .Apps
                .GetMyApps()
                .OnSuccess(response =>
                {
                    var apps = response.Payload.Body;
                    for (int i = 0, len = apps.Length; i < len; i++)
                    {

                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not get my apps : {0}.", exception);
                });
        }
    }
}