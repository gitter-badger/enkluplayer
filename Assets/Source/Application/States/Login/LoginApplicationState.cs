using System.IO;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Decides how to login.
    /// </summary>
    public class LoginApplicationState : IState
    {
        /// <summary>
        /// Credentials.
        /// </summary>
        public const string CREDS_URI = "login://DefaultCredentials";

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
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

        /// <summary>
        /// Application wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// For versioning.
        /// </summary>
        private readonly VersioningService _versioning;

        /// <summary>
        /// Credentials.
        /// </summary>
        private CredentialsData _credentials;

        /// <summary>
        /// Times state.
        /// </summary>
        private int _timerId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public LoginApplicationState(
            IFileManager files,
            ILoginStrategy strategy,
            IHttpService http,
            IMessageRouter messages,
            IMetricsService metrics,
            ApplicationConfig config,
            VersioningService versioning)
        {
            _files = files;
            _strategy = strategy;
            _http = http;
            _messages = messages;
            _metrics = metrics;
            _config = config;
            _versioning = versioning;

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

            _timerId = _metrics.Timer(MetricsKeys.STATE_LOGIN).Start();

            if (_config.Play.SkipVersionCheck)
            {
                StartLogin();
            }
            else
            {
                _versioning
                    .CheckVersions()
                    .OnSuccess(_ => StartLogin());
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

            _metrics.Timer(MetricsKeys.STATE_LOGIN).Stop(_timerId);
        }

        /// <summary>
        /// Begins login procedure.
        /// </summary>
        private void StartLogin()
        {
            // load creds
            _files
                .Get<CredentialsData>(CREDS_URI)
                .OnSuccess(file =>
                {
                    // load into default app
                    Log.Info(this, "Credentials loaded from disk.");

                    ConfigureCredentials(file.Data);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load credential information: {0}", exception);

                    NetworkLogin();
                });
        }

        /// <summary>
        /// Logs in and obtains credentials object.
        /// </summary>
        private void NetworkLogin()
        {
            // different platforms have different login strategies
            _strategy
                .Login()
                .OnSuccess(credentials =>
                {
                    Log.Info(this, "Logged in.");

                    ConfigureCredentials(credentials);
                    
                    Log.Info(this, "Saving credentials to disk.");

                    _files
                        .Set(CREDS_URI, credentials)
                        .OnFailure(exception => Log.Error(this, "Could not write credentials to disk : {0}.", exception));
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