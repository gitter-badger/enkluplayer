using System;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Enumeration of the major application states.
    /// </summary>
    public enum ApplicationStateTypes
    {
        Invalid = -1,
        None,
        LoadApp,
        ReceiveApp,
        Tool,
        Insta,
    }

    /// <summary>
    /// Application wide configuration.
    /// </summary>
    [Serializable]
    public class ApplicationConfig
    {
        /// <summary>
        /// If set, overrides the initial state after application is initialized.
        /// </summary>
        public string StateOverride;

        /// <summary>
        /// If true, simulates webgl player. Used only in the Unity Editor.
        /// </summary>
        public bool SimulateWebgl;

        /// <summary>
        /// Configuration for playing an app.
        /// </summary>
        public PlayAppConfig Play = new PlayAppConfig();
        
        /// <summary>
        /// Network configuration.
        /// </summary>
        public NetworkConfig Network = new NetworkConfig();
        
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
            Ar,
            Desktop,
            Mobile
        }

        /// <summary>
        /// Id of the app.
        /// </summary>
        public string AppId;

        /// <summary>
        /// Type of designer to use.
        /// </summary>
        public string DesignerOverride;

        /// <summary>
        /// Parses designer name.
        /// </summary>
        public DesignerType Designer
        {
            get
            {
                try
                {
                    return (DesignerType) Enum.Parse(
                        typeof(DesignerType),
                        DesignerOverride);
                }
                catch
                {
                    return DesignerType.Ar;
                }
            }
        }
    }

    /// <summary>
    /// Network related configuration.
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// Current environment we should connect to.
        /// </summary>
        public string Current;
        
        /// <summary>
        /// List of environments.
        /// </summary>
        public EnvironmentData[] AllEnvironments;

        /// <summary>
        /// List of credentials.
        /// </summary>
        public CredentialsData[] AllCredentials;
        
        /// <summary>
        /// Retrieves an environment by name.
        /// </summary>
        /// <param name="name">Environment name.</param>
        /// <returns></returns>
        public EnvironmentData Environment(string name)
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
        /// Retrieves credentials by environment.
        /// </summary>
        /// <param name="env">The name of the environment.</param>
        /// <returns></returns>
        public CredentialsData Credentials(string env)
        {
            if (null == AllCredentials || 0 == AllCredentials.Length)
            {
                return null;
            }

            for (int i = 0, len = AllCredentials.Length; i < len; i++)
            {
                var creds = AllCredentials[i];
                if (creds.Environment == env)
                {
                    return creds;
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
        /// Hostname.
        /// </summary>
        public string BaseUrl = "localhost";

        /// <summary>
        /// Port.
        /// </summary>
        public int Port = 9999;

        /// <summary>
        /// Api version.
        /// </summary>
        public string ApiVersion = "v1";
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
            http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                "userId",
                UserId));
            http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                "Authorization",
                string.Format("Bearer {0}", Token)));
        }
    }
}