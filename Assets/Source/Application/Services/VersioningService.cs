using System;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Service that watches for versioning.
    /// </summary>
    public class VersioningService : ApplicationService
    {
        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Trellis API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// Connection with Trellis.
        /// </summary>
        private readonly IConnection _connection;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public VersioningService(
            ApplicationConfig config,
            ApiController api,
            IConnection connection,
            MessageTypeBinder binder,
            IMessageRouter messages)
            : base(binder, messages)
        {
            _config = config;
            _api = api;
            _connection = connection;
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            _connection.OnConnected += Connection_OnConnected;
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _connection.OnConnected -= Connection_OnConnected;
        }

        /// <summary>
        /// Checks for version match with Trellis.
        /// </summary>
        public IAsyncToken<Void> CheckVersion()
        {
            Log.Info(this, "Checking Trellis version.");

            var token = new AsyncToken<Void>();

            _api
                .Versionings
                .GetApiVersion()
                .OnSuccess(response =>
                {
                    if (null != response.Payload && response.Payload.Success)
                    {
                        var req = response.Payload.Body.Version.Split('.');
                        var local = _config.Network.ApiVersion.Split('.');

                        // check major version only
                        if (req[0] != local[0])
                        {
                            Log.Warning(this, "Version mismatch.");

                            _messages.Publish(MessageTypes.VERSION_MISMATCH);

                            token.Fail(new Exception("Version mismatch."));
                        }
                        else
                        {
                            Log.Info(this, "Trellis version match.");

                            token.Succeed(Void.Instance);
                        }
                    }
                    else
                    {
                        var raw = Encoding.UTF8.GetString(response.Raw);
                        Log.Error(this,
                            "Could not verify version match. Response: {0}",
                            raw);

                        token.Fail(new Exception(null == response.Payload ? raw : response.Payload.Error));
                    }
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not verify version match : {0}", ex);
                    
                    token.Fail(ex);
                });

            return token;
        }

        /// <summary>
        /// On reconnect, make sure we're compatible with server.
        /// </summary>
        private void Connection_OnConnected()
        {
            CheckVersion();
        }
    }
}
