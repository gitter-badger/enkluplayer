using System;
using System.IO;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Abstracts out Application v User configuration.
    /// </summary>
    public class EditorConfigurationManager
    {
        /// <summary>
        /// Paths.
        /// </summary>
        /// <returns></returns>
        private static string APP_CONFIG_DIRECTORY = Path.Combine(
            UnityEngine.Application.dataPath,
            "Resources");
        private static string APP_CONFIG_NAME = "ApplicationConfig.json";
        private static string USER_CONFIG_DIRECTORY = UnityEngine.Application.persistentDataPath;
        private static string USER_CONFIG_NAME = "UserConfig.json";

        /// <summary>
        /// Watchers for each config.
        /// </summary>
        private FileSystemWatcher _appWatcher;
        private FileSystemWatcher _userWatcher;

        /// <summary>
        /// Configs.
        /// </summary>
        public ApplicationConfig AppConfig { get; private set; }
        public ApplicationConfig UserConfig { get; private set; }

        /// <summary>
        /// Called when a config has been updated.
        /// </summary>
        public event Action OnUpdate;

        /// <summary>
        /// The current environment.
        /// </summary>
        public EnvironmentData Environment
        {
            get
            {
                var current = UserConfig.Network.Environment;
                if (null == current)
                {
                    current = AppConfig.Network.Environment;
                }

                return current;
            }
        }

        /// <summary>
        /// The current credentials.
        /// </summary>
        public CredentialsData Credentials
        {
            get
            {
                var current = UserConfig.Network.Credentials;
                if (null == current)
                {
                    current = AppConfig.Network.Credentials;
                }

                return current;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditorConfigurationManager()
        {
            OnUpdate += This_OnUpdate;
        }

        /// <summary>
        /// Starts the manager.
        /// </summary>
        public void Startup()
        {
            LoadApplicationConfig();
            LoadUserConfig();

            _appWatcher = new FileSystemWatcher(APP_CONFIG_DIRECTORY)
            {
                Filter = APP_CONFIG_NAME,
                EnableRaisingEvents = true
            };
            _appWatcher.Changed += App_OnChanged;

            _userWatcher = new FileSystemWatcher(USER_CONFIG_DIRECTORY)
            {
                Filter = USER_CONFIG_NAME,
                EnableRaisingEvents = true
            };
            _userWatcher.Changed += User_OnChanged;

            if (null != OnUpdate)
            {
                OnUpdate();
            }
        }

        /// <summary>
        /// Tearsdown the manager.
        /// </summary>
        public void Teardown()
        {
            _appWatcher.Changed -= App_OnChanged;
            _userWatcher.Changed -= User_OnChanged;
        }

        /// <summary>
        /// Called when updated.
        /// </summary>
        private void This_OnUpdate()
        {
            //
        }

        /// <summary>
        /// Loads application configuration.
        /// </summary>
        private void LoadApplicationConfig()
        {
            var path = Path.Combine(APP_CONFIG_DIRECTORY, APP_CONFIG_NAME);
            if (!File.Exists(path))
            {
                throw new Exception("ApplicationConfig.json does not exist!");
            }

            var serializer = new JsonSerializer();
            var bytes = File.ReadAllBytes(path);

            object app;
            serializer.Deserialize(typeof(ApplicationConfig), ref bytes, out app);

            AppConfig = (ApplicationConfig) app;
        }

        /// <summary>
        /// Loads user settings.
        /// </summary>
        private void LoadUserConfig()
        {
            var path = Path.Combine(USER_CONFIG_DIRECTORY, USER_CONFIG_NAME);
            if (!File.Exists(path))
            {
                UserConfig = new ApplicationConfig();
                return;
            }

            var serializer = new JsonSerializer();
            var bytes = File.ReadAllBytes(path);

            object app;
            serializer.Deserialize(typeof(ApplicationConfig), ref bytes, out app);

            UserConfig = (ApplicationConfig) app;
        }

        /// <summary>
        /// Called when the app config has changed.
        /// </summary>
        private void App_OnChanged(
            object sender,
            FileSystemEventArgs fileSystemEventArgs)
        {
            LoadApplicationConfig();

            if (null != OnUpdate)
            {
                OnUpdate();
            }
        }

        /// <summary>
        /// Called when the user confid has changed.
        /// </summary>
        private void User_OnChanged(
            object sender,
            FileSystemEventArgs fileSystemEventArgs)
        {
            LoadUserConfig();

            if (null != OnUpdate)
            {
                OnUpdate();
            }
        }
    }
}