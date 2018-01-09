using System;
using System.IO;
using System.Text;
using CreateAR.Commons.Unity.DataStructures;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Http.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Holds objects and builds them appropriately for edit mode..
    /// </summary>
    [InitializeOnLoad]
    public static class EditorApplication
    {
        /// <summary>
        /// Backing variable for Bootstrapper.
        /// </summary>
        private static readonly EditorBootstrapper _bootstrapper = new EditorBootstrapper();

        /// <summary>
        /// Configuration for all environments.
        /// </summary>
        public static EnvironmentConfig Environments { get; private set; }

        /// <summary>
        /// User settings.
        /// </summary>
        public static UserSettings UserSettings { get; private set; }

        /// <summary>
        /// Dependencies.
        /// </summary>
        public static IBootstrapper Bootstrapper { get { return _bootstrapper; } }
        public static IHttpService Http { get; private set; }
        public static ISerializer Serializer { get; private set; }
        public static ApiController Api { get; private set; }
        
        /// <summary>
        /// Static constructor.
        /// </summary>
        static EditorApplication()
        {
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Timestamp = false,
                Level = false
            }));
            
            UnityEditor.EditorApplication.update += _bootstrapper.Update;
            UnityEditor.EditorApplication.update += WatchForCompile;
            
            Serializer = new JsonSerializer();
            Http = new EditorHttpService(Serializer, Bootstrapper);
            Api = new ApiController(Http);

            LoadEnvironments();
            LoadCredentials();
        }

        /// <summary>
        /// Saves credentials to disk.
        /// </summary>
        public static void SaveUserSettings()
        {
            var path = GetSettingsPath();

            Log.Info(UserSettings,
                "Saving UserSettings to {0}.",
                path);

            byte[] bytes;
            Serializer.Serialize(UserSettings, out bytes);
            var json = Encoding.UTF8.GetString(bytes);

            File.WriteAllText(
                path,
                json);
        }

        /// <summary>
        /// Watches for uninit.
        /// </summary>
        private static void WatchForCompile()
        {
            if (UnityEditor.EditorApplication.isCompiling)
            {
                Http.Abort();

                Log.Info(Bootstrapper, "Shutting down bootstrapper.");

                UnityEditor.EditorApplication.update -= _bootstrapper.Update;
                UnityEditor.EditorApplication.update -= WatchForCompile;
            }
        }

        /// <summary>
        /// Loads information about environments.
        /// </summary>
        private static void LoadEnvironments()
        {
            var config = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Config/Environments.json");
            if (null == config)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(config.text);
            object @object;

            Serializer.Deserialize(
                typeof(EnvironmentConfig),
                ref bytes,
                out @object);

            Environments = (EnvironmentConfig) @object;

            Log.Info(Environments, "Loaded environments : {0}.", Environments);
        }

        /// <summary>
        /// Loads credentials.
        /// </summary>
        private static void LoadCredentials()
        {
            // create settings object if one doesn't exist
            var path = GetSettingsPath();
            if (!File.Exists(path))
            {
                UserSettings = new UserSettings();
                SaveUserSettings();
            }

            // load object
            var json = File.ReadAllText(path);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            object @object;
            Serializer.Deserialize(
                typeof(UserSettings),
                ref jsonBytes,
                out @object);
            UserSettings = (UserSettings)@object;

            // make sure there is an entry for every environment
            var changed = false;
            for (int i = 0, len = Environments.Environments.Length; i < len; i++)
            {
                var environment = Environments.Environments[i];
                if (null == UserSettings.Credentials(environment.Name))
                {
                    UserSettings.All = UserSettings.All.Add(new EnvironmentCredentials
                    {
                        Environment = environment.Name
                    });

                    changed = true;
                }
            }

            if (changed)
            {
                SaveUserSettings();
            }

            // remove Authentication
            for (int i = 0, len = Http.Headers.Count; i < len; i++)
            {
                var header = Http.Headers[i];
                if (header.Item1.StartsWith("Authorization"))
                {
                    Http.Headers.RemoveAt(i);

                    break;
                }
            }

            var creds = UserSettings.Credentials(UserSettings.Environment);
            if (null != creds && !string.IsNullOrEmpty(creds.Token))
            {
                Log.Info(UserSettings, "Setting Authorization header.");

                Http.Headers.Add(Tuple.Create(
                    "Authorization",
                    string.Format("Bearer {0}", creds.Token)));
            }

            Log.Info(UserSettings, "Loaded credentials {0}.", UserSettings);
        }

        /// <summary>
        /// Path to credentials.
        /// </summary>
        /// <returns></returns>
        private static string GetSettingsPath()
        {
            var path = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "User.Environment.config");
            return path;
        }
    }
}