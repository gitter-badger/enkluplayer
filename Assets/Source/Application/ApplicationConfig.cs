using System;
using System.Linq;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Enumeration of the major application states.
    /// </summary>
    public enum ApplicationStateType
    {
        Invalid = -1,
        None,
        LoadApp,
        ReceiveApp,
        Tool,
        Insta,
        Login,
        UserProfile,
        Orientation,
        ArSetup
    }

    /// <summary>
    /// Application wide configuration.
    /// </summary>
    [Serializable]
    public class ApplicationConfig
    {
        /// <summary>
        /// Sets the initial state. Leave empty for the application to decide.
        /// </summary>
        public string State;

        /// <summary>
        /// Sets the platform. Leave empty for the application to decide.
        /// </summary>
        public string Platform;
        
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
            if (!string.IsNullOrEmpty(overrideConfig.Platform))
            {
                Platform = overrideConfig.Platform;
            }

            if (!string.IsNullOrEmpty(overrideConfig.State))
            {
                State = overrideConfig.State;
            }

            Log.Override(overrideConfig.Log);
            Network.Override(overrideConfig.Network);
            Play.Override(overrideConfig.Play);
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
        /// If true, forces all Http requests to fail.
        /// </summary>
        public bool Offline;

        /// <summary>
        /// Current environment we should connect to.
        /// </summary>
        public string Current;
        
        /// <summary>
        /// List of environments.
        /// </summary>
        public EnvironmentData[] AllEnvironments;

        /// <summary>
        /// List of environments.
        /// </summary>
        public CredentialsData[] AllCredentials;

        /// <summary>
        /// Retrieves the current environment.
        /// </summary>
        public EnvironmentData Environment
        {
            get
            {
                if (null == AllEnvironments || 0 == AllEnvironments.Length)
                {
                    return null;
                }

                for (int i = 0, len = AllEnvironments.Length; i < len; i++)
                {
                    var env = AllEnvironments[i];
                    if (env.Name == Current)
                    {
                        return env;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Retrieves the current credentials.
        /// </summary>
        public CredentialsData Credentials
        {
            get
            {
                if (null == AllCredentials || 0 == AllCredentials.Length)
                {
                    return null;
                }

                for (int i = 0, len = AllCredentials.Length; i < len; i++)
                {
                    var creds = AllCredentials[i];
                    if (creds.Environment == Current)
                    {
                        return creds;
                    }
                }

                return null;
            }
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
            if (overrideConfig.AssetDownloadLagSec > double.Epsilon)
            {
                AssetDownloadLagSec = overrideConfig.AssetDownloadLagSec;
            }

            if (!string.IsNullOrEmpty(overrideConfig.Current))
            {
                Current = overrideConfig.Current;
            }

            Offline = overrideConfig.Offline;

            // combine arrays
            AllEnvironments = AllEnvironments.Concat(overrideConfig.AllEnvironments).ToArray();
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
}