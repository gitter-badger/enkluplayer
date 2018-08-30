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
        /// Semver parser.
        /// </summary>
        public struct VersionData
        {
            /// <summary>
            /// Major version.
            /// </summary>
            public int Major;

            /// <summary>
            /// Minor version.
            /// </summary>
            public int Minor;

            /// <summary>
            /// Bug meta.
            /// </summary>
            public string Bug;

            /// <summary>
            /// Creates a version from a string.
            /// </summary>
            /// <param name="version">Version string.</param>
            public VersionData(string version)
            {
                var split = version.Split('.');
                if (split.Length != 3
                    || !int.TryParse(split[0], out Major)
                    || !int.TryParse(split[1], out Minor))
                {
                    Major = Minor = 0;
                    Bug = string.Empty;
                }
                else
                {
                    Bug = split[2];
                }
            }
        }

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
        /// Checks API and HoloLens versions.
        /// 
        /// Fails the token if the service dispatched a message to handle
        /// a version mismatch.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<Void> CheckVersions()
        {
            return Async.Map(
                Async.All(CheckApiVersion(), CheckHoloLensVersion()),
                _ => Void.Instance);
        }

        /// <summary>
        /// Checks for latest version of HoloLens build.
        /// </summary>
        private IAsyncToken<Void> CheckHoloLensVersion()
        {
            Log.Info(this, "Checking Trellis version.");

            var token = new AsyncToken<Void>();

            _api
                .Versionings
                .GetHololensVersion()
                .OnSuccess(response =>
                {
                    if (null != response.Payload && response.Payload.Success)
                    {
                        var req = new VersionData(response.Payload.Body.Version);
                        var local = new VersionData(_config.Version);
                        
                        if (req.Major != local.Major)
                        {
                            Log.Warning(this, "New HoloLens build required.");

                            _messages.Publish(MessageTypes.VERSION_UPGRADE);

                            token.Fail(new Exception("Version upgrade required."));
                        }
                        else
                        {
                            if (req.Minor != local.Minor)
                            {
                                Log.Warning(this, "New version available.");
                            }
                            else
                            {
                                Log.Info(this, "HoloLens version match.");
                            }

                            token.Succeed(Void.Instance);
                        }
                    }
                    else
                    {
                        var raw = Encoding.UTF8.GetString(response.Raw);
                        Log.Error(this,
                            "Could not version HoloLens version match. Response: {0}",
                            raw);

                        token.Succeed(Void.Instance);
                    }
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not verify HoloLens version match : {0}", ex);

                    token.Succeed(Void.Instance);
                });

            return token;
        }

        /// <summary>
        /// Checks for version match with Trellis.
        /// </summary>
        private IAsyncToken<Void> CheckApiVersion()
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

                        _messages.Publish(MessageTypes.VERSION_MISMATCH);

                        token.Fail(new Exception(null == response.Payload ? raw : response.Payload.Error));
                    }
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not verify version match : {0}", ex);

                    _messages.Publish(MessageTypes.VERSION_MISMATCH);

                    token.Fail(ex);
                });

            return token;
        }

        /// <summary>
        /// On reconnect, make sure we're compatible with server.
        /// </summary>
        private void Connection_OnConnected()
        {
            CheckVersions();
        }
    }
}
