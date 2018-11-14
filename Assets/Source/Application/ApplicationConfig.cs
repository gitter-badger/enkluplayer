using System;
using System.Linq;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Application wide configuration.
    /// </summary>
    [Serializable]
    public class ApplicationConfig
    {
        /// <summary>
        /// Version of the player.
        /// </summary>
        public string Version = "0.0.0";

        /// <summary>
        /// Sets the platform. Leave empty for the application to decide.
        /// </summary>
        public string Platform;

        /// <summary>
        /// True iff we should open the IUXDesigner.
        /// </summary>
        public bool IuxDesigner;
        
        /// <summary>
        /// Logging.
        /// </summary>
        public LogAppConfig Log = new LogAppConfig();

        /// <summary>
        /// Configuration for playing an app.
        /// </summary>
        public PlayAppConfig Play = new PlayAppConfig();
        
        /// <summary>
        /// Network configuration.
        /// </summary>
        public NetworkConfig Network = new NetworkConfig();

        /// <summary>
        /// Conductor-related configuration.
        /// </summary>
        public ConductorConfig Conductor = new ConductorConfig();

        /// <summary>
        /// Metrics-related configuration.
        /// </summary>
        public MetricsConfig Metrics = new MetricsConfig();

        /// <summary>
        /// Cursor configuration.
        /// </summary>
        public CursorConfig Cursor = new CursorConfig();

        /// <summary>
        /// Debug configuration
        /// </summary>
        public DebugConfig Debug = new DebugConfig();

        /// <summary>
        /// Platform to use.
        /// </summary>
        public RuntimePlatform ParsedPlatform
        {
            get
            {
                if (!string.IsNullOrEmpty(Platform))
                {
                    try
                    {
                        return (RuntimePlatform)Enum.Parse(typeof(RuntimePlatform), Platform);
                    }
                    catch
                    {
                        // fallthrough
                    }
                }

#if UNITY_EDITOR
                switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
                {
                    case UnityEditor.BuildTarget.Android:
                    {
                        return RuntimePlatform.Android;
                    }
                    case UnityEditor.BuildTarget.iOS:
                    {
                        return RuntimePlatform.IPhonePlayer;
                    }
                    case UnityEditor.BuildTarget.WebGL:
                    {
                        return RuntimePlatform.WebGLPlayer;
                    }
                    case UnityEditor.BuildTarget.WSAPlayer:
                    {
                        return RuntimePlatform.WSAPlayerX86;
                    }
                    default:
                    {
                        return UnityEngine.Application.platform;
                    }
                }
#else
                return UnityEngine.Application.platform;
#endif
            }
        }
        
        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[ApplicationConfig Network={0}, Play={1}]",
                Network,
                Play);
        }

        /// <summary>
        /// Applies the settings from a config to this config.
        /// </summary>
        /// <param name="overrideConfig">The config to override with.</param>
        public void Override(ApplicationConfig overrideConfig)
        {
            // TODO: Override at JSON level instead of here.
            if (!string.IsNullOrEmpty(overrideConfig.Platform))
            {
                Platform = overrideConfig.Platform;
            }

            if (overrideConfig.IuxDesigner)
            {
                IuxDesigner = true;
            }

            Log.Override(overrideConfig.Log);
            Network.Override(overrideConfig.Network);
            Play.Override(overrideConfig.Play);
            Conductor.Override(overrideConfig.Conductor);
            Metrics.Override(overrideConfig.Metrics);
            Debug.Override(overrideConfig.Debug);
        }
    }

    /// <summary>
    /// Logging.
    /// </summary>
    public class LogAppConfig
    {
        /// <summary>
        /// Log level.
        /// </summary>
        public string Level;

        /// <summary>
        /// Parsed level.
        /// </summary>
        public LogLevel ParsedLevel
        {
            get
            {
                try
                {
                    return (LogLevel) Enum.Parse(typeof(LogLevel), Level);
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Applies the settings from a config to this config.
        /// </summary>
        /// <param name="overrideConfig">The config to override with.</param>
        public void Override(LogAppConfig overrideConfig)
        {
            if (!string.IsNullOrEmpty(overrideConfig.Level))
            {
                Level = overrideConfig.Level;
            }
        }
    }
    
    /// <summary>
    /// Application to play.
    /// </summary>
    public class PlayAppConfig
    {
        /// <summary>
        /// Enumerates all potential designer types.
        /// </summary>
        public enum DesignerType
        {
            Invalid,
            Ar,
            Desktop,
            Mobile,
            None
        }

        /// <summary>
        /// Id of the app.
        /// </summary>
        public string AppId;

        /// <summary>
        /// Type of designer to use. Leave empty for application to decide.
        /// </summary>
        public string Designer;

        /// <summary>
        /// Edit or play.
        /// </summary>
        public bool Edit = true;

        /// <summary>
        /// If true, the player will only check for updates every X minutes,
        /// as given by <c>PeriodicUpdatesMinutes</c>. If false, the player
        /// checks for updates each time play mode is entered.
        /// </summary>
        public bool PeriodicUpdates;

        /// <summary>
        /// Minutes to wait before checking for updates again. Only used if
        /// <c>PeriodicUpdates</c> is set to true.
        /// </summary>
        public int PeriodicUpdatesMinutes;

        /// <summary>
        /// If true, skips device registration.
        /// </summary>
        public bool SkipDeviceRegistration;

        /// <summary>
        /// If true, skips version check.
        /// </summary>
        public bool SkipVersionCheck;

        /// <summary>
        /// Parses designer name.
        /// </summary>
        public DesignerType ParsedDesigner
        {
            get
            {
                try
                {
                    return (DesignerType) Enum.Parse(
                        typeof(DesignerType),
                        Designer);
                }
                catch
                {
                    return DesignerType.Invalid;
                }
            }
        }

        /// <summary>
        /// Overrides configuration values with passed in values.
        /// </summary>
        /// <param name="overrideConfig">Override config.</param>
        public void Override(PlayAppConfig overrideConfig)
        {
            if (!string.IsNullOrEmpty(overrideConfig.AppId))
            {
                AppId = overrideConfig.AppId;
            }

            if (!string.IsNullOrEmpty(overrideConfig.Designer))
            {
                Designer = overrideConfig.Designer;
            }

            if (overrideConfig.PeriodicUpdates)
            {
                PeriodicUpdates = overrideConfig.PeriodicUpdates;
            }

            if (overrideConfig.PeriodicUpdatesMinutes > 0)
            {
                PeriodicUpdatesMinutes = overrideConfig.PeriodicUpdatesMinutes;
            }

            if (overrideConfig.SkipDeviceRegistration)
            {
                SkipDeviceRegistration = overrideConfig.SkipDeviceRegistration;
            }

            if (overrideConfig.SkipVersionCheck)
            {
                SkipVersionCheck = overrideConfig.SkipVersionCheck;
            }
        }
    }

    /// <summary>
    /// Network related configuration.
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// Lag, in seconds, to add to asset downloads.
        /// </summary>
        public float AssetDownloadLagSec;

        /// <summary>
        /// Likelyhood of forcing an asset download to fail.
        /// </summary>
        public float AssetDownloadFailChance;

        /// <summary>
        /// Likelyhood of forcing an anchor download to fail.
        /// </summary>
        public float AnchorDownloadFailChance = 0.0f;

        /// <summary>
        /// Likelyhood of forcing anchor import to fail.
        /// </summary>
        public float AnchorImportFailChance = 0.0f;

        /// <summary>
        /// If true, forces all Http requests to fail.
        /// </summary>
        public bool Offline;

        /// <summary>
        /// Api version this player is compatible with.
        /// </summary>
        public string ApiVersion = "0.0.0";

        /// <summary>
        /// Current environment we should connect to.
        /// </summary>
        public string Current;
        
        /// <summary>
        /// List of environments.
        /// </summary>
        public EnvironmentData[] AllEnvironments = new EnvironmentData[0];

        /// <summary>
        /// List of environments.
        /// </summary>
        public CredentialsData[] AllCredentials = new CredentialsData[0];

        /// <summary>
        /// Retrieves the current environment.
        /// </summary>
        public EnvironmentData Environment
        {
            get
            {
                return EnvironmentByName(Current);
            }
        }

        /// <summary>
        /// Retrieves the current credentials.
        /// </summary>
        public CredentialsData Credentials
        {
            get
            {
                for (int i = 0, len = AllCredentials.Length; i < len; i++)
                {
                    var creds = AllCredentials[i];
                    if (creds.Environment == Current)
                    {
                        return creds;
                    }
                }

                var defaultCreds = new CredentialsData
                {
                    Environment = Current
                };

                AllCredentials = AllCredentials.Add(defaultCreds);

                return defaultCreds;
            }
        }

        /// <summary>
        /// Retrieves an environment by name.
        /// </summary>
        /// <param name="name">The name of the environment to use.</param>
        /// <returns></returns>
        public EnvironmentData EnvironmentByName(string name)
        {
            if (null == AllEnvironments || 0 == AllEnvironments.Length)
            {
                return null;
            }

            for (int i = 0, len = AllEnvironments.Length; i < len; i++)
            {
                var env = AllEnvironments[i];
                if (env.Name == name)
                {
                    return env;
                }
            }

            return null;
        }

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[EnvironmentConfig Count={0}]",
                null == AllEnvironments ? 0 : AllEnvironments.Length);
        }

        /// <summary>
        /// Applies the settings from a config to this config.
        /// </summary>
        /// <param name="overrideConfig">The config to override with.</param>
        public void Override(NetworkConfig overrideConfig)
        {
            if (!string.IsNullOrEmpty(overrideConfig.Current))
            {
                Current = overrideConfig.Current;
            }

            if (overrideConfig.AssetDownloadLagSec > double.Epsilon)
            {
                AssetDownloadLagSec = overrideConfig.AssetDownloadLagSec;
            }

            if (overrideConfig.AssetDownloadFailChance > Mathf.Epsilon)
            {
                AssetDownloadFailChance = overrideConfig.AssetDownloadFailChance;
            }

            if (overrideConfig.AnchorDownloadFailChance > Mathf.Epsilon)
            {
                AnchorDownloadFailChance = overrideConfig.AnchorDownloadFailChance;
            }

            if (overrideConfig.AnchorImportFailChance > Mathf.Epsilon)
            {
                AnchorImportFailChance = overrideConfig.AnchorImportFailChance;
            }

            if (!string.IsNullOrEmpty(overrideConfig.ApiVersion))
            {
                ApiVersion = overrideConfig.ApiVersion;
            }

            Offline = overrideConfig.Offline;

            // combine arrays
            AllEnvironments = AllEnvironments.Concat(overrideConfig.AllEnvironments).ToArray();
            AllCredentials = AllCredentials.Concat(overrideConfig.AllCredentials).ToArray();
        }
    }

    /// <summary>
    /// Holds information about a specific environment.
    /// </summary>
    public class EnvironmentData
    {
        /// <summary>
        /// Name of this environment.
        /// </summary>
        public string Name = "local";

        /// <summary>
        /// The Url.
        /// </summary>
        public string TrellisUrl = "localhost";

        /// <summary>
        /// Url for asset import server.
        /// </summary>
        public string AssetsUrl = "localhost";

        /// <summary>
        /// Url for bundle download.
        /// </summary>
        public string BundlesUrl = "localhost";

        /// <summary>
        /// Url for thumbs.
        /// </summary>
        public string ThumbsUrl = "localhost";

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[EnvironmentData TrellisUrl={0}, AssetsUrl={1}, BundlesUrl={2}, ThumbsUrl={3}]",
                TrellisUrl,
                AssetsUrl,
                BundlesUrl,
                ThumbsUrl);
        }
    }

    /// <summary>
    /// Simple POCO for editor interaction with Trellis.
    /// </summary>
    public class CredentialsData
    {
        /// <summary>
        /// Environment these credentials are for.
        /// </summary>
        public string Environment;

        /// <summary>
        /// Email.
        /// </summary>
        public string Email;

        /// <summary>
        /// Password.
        /// </summary>
        public string Password;

        /// <summary>
        /// Token!
        /// </summary>
        public string Token;

        /// <summary>
        /// User id.
        /// </summary>
        public string UserId;

        /// <summary>
        /// True iff user is a guest.
        /// </summary>
        public bool IsGuest
        {
            get { return UserId == "Guest"; }
        }

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "[TrellisCredentials Email={0}, Password={1}, Token={2}]",
                Email,
                Password,
                Token);
        }

        /// <summary>
        /// Applies credentials to HTTP service.
        /// </summary>
        /// <param name="http">Makes http calls.</param>
        public void Apply(IHttpService http)
        {
            http.Urls.Formatter("trellis").Replacements["userId"] = UserId;
            http.Headers["Authorization"] = string.Format("Bearer {0}", Token);
        }
    }

    /// <summary>
    /// Conductor-related configuration.
    /// </summary>
    public class ConductorConfig
    {
        /// <summary>
        /// Ms delta between battery updates.
        /// </summary>
        public int BatteryUpdateDeltaMs = 10 * 60 * 1000;

        /// <summary>
        /// Overrides settings.
        /// </summary>
        /// <param name="config">Another config.</param>
        public void Override(ConductorConfig config)
        {
            BatteryUpdateDeltaMs = config.BatteryUpdateDeltaMs;
        }
    }

    /// <summary>
    /// Configuration for metrics.
    /// </summary>
    public class MetricsConfig
    {
        /// <summary>
        /// Whether metrics should be enabled or not.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// A comma-delimited list of targets.
        /// </summary>
        public string Targets = "HostedGraphite";

        /// <summary>
        /// Hostname of the metrics box.
        /// </summary>
        public string Hostname;

        /// <summary>
        /// Authorization.
        /// </summary>
        public string ApplicationKey;

        /// <summary>
        /// Overrides settings.
        /// </summary>
        /// <param name="config">The config.</param>
        public void Override(MetricsConfig config)
        {
            Enabled = config.Enabled;

            if (!string.IsNullOrEmpty(config.Hostname))
            {
                Hostname = config.Hostname;
            }

            if (!string.IsNullOrEmpty(config.ApplicationKey))
            {
                ApplicationKey = config.ApplicationKey;
            }

            if (!string.IsNullOrEmpty(config.Targets))
            {
                Targets = config.Targets;
            }
        }
    }

    /// <summary>
    /// Configuration for the Cursor.
    /// </summary>
    public class CursorConfig
    {
        /// <summary>
        /// Authoritative setting. If true no app logic should hide the cursor.
        /// to the screen.
        /// </summary>
        public bool ForceShow { get; set; }
    }

    /// <summary>
    /// Configuration for debugging.
    /// </summary>
    public class DebugConfig
    {
        /// <summary>
        /// If true, disables debug lock on voice commands.
        /// </summary>
        public bool DisableVoiceLock = false;

        /// <summary>
        /// If true, requires saying "admin" before admin voice commands.
        /// </summary>
        public bool DisableAdminLock = false;

        /// <summary>
        /// Overrides settings.
        /// </summary>
        /// <param name="config">Other config.</param>
        public void Override(DebugConfig config)
        {
            if (config.DisableVoiceLock)
            {
                DisableVoiceLock = true;
            }

            if (config.DisableAdminLock)
            {
                DisableAdminLock = true;
            }
        }
    }
}