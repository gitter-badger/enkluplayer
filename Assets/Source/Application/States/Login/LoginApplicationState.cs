using System.IO;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

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
        public const string CREDS = "login://DefaultCredentials";

        /// <summary>
        /// Reads and writes files.
        /// </summary>
        private readonly IFileManager _files;

        /// <summary>
        /// Http implementation.
        /// </summary>
        private readonly IHttpService _http;
        
        /// <summary>
        /// Strategy for logging in.
        /// </summary>
        private readonly ILoginStrategy _strategy;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Credentials.
        /// </summary>
        private CredentialsData _credentials;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginApplicationState(
            IFileManager files,
            ILoginStrategy strategy,
            IHttpService http,
            IMessageRouter messages,
            ApplicationConfig config)
        {
            _files = files;
            _strategy = strategy;
            _http = http;
            _messages = messages;
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

                        ConfigureCredentials(file.Data);
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
            // we wait for the update loop to publish
            if (null != _credentials)
            {
                _messages.Publish(MessageTypes.LOGIN_COMPLETE);
            }
        }

        /// <inheritdoc />
        public void Exit()
        {
            _credentials = null;
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
                    if (credentials.IsGuest)
                    {
                        Log.Info(this, "Continuing as guest.");
                        
                        _messages.Publish(MessageTypes.GUEST);
                        return;
                    }
                    
                    Log.Info(this, "Logged in.");
                    Log.Info(this, "Saving credentials to disk.");

                    _files
                        .Set(CREDS, credentials)
                        .OnFailure(exception => Log.Error(this, "Could not write credentials to disk : {0}.", exception));

                    ConfigureCredentials(credentials);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not log in. Now what? {0}", exception);
                });
        }

        /// <summary>
        /// Configures systems using the credentials.
        /// </summary>
        /// <param name="credentials">The credentials in question.</param>
        private void ConfigureCredentials(CredentialsData credentials)
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
                creds = credentials;
                _config.Network.AllCredentials = _config.Network.AllCredentials.Add(creds);
            }
            else
            {
                creds.Token = credentials.Token;
                creds.UserId = credentials.UserId;
            }

            _credentials = credentials;
        }
    }
}