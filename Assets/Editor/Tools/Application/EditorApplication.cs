using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Http.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis;
using UnityEditor;

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
        /// Obj importer, lazily created.
        /// </summary>
        private static ObjImporter _importer;
        
        /// <summary>
        /// Managed configuration.
        /// </summary>
        public static EditorConfigurationManager Config { get; private set; }

        /// <summary>
        /// Dependencies.
        /// </summary>
        public static IBootstrapper Bootstrapper { get { return _bootstrapper; } }
        public static IHttpService Http { get; private set; }
        public static ISerializer Serializer { get; private set; }
        public static ApiController Api { get; private set; }

        /// <summary>
        /// Lazily create this importer as it has a long-running coroutine.
        /// </summary>
        public static ObjImporter ObjImporter
        {
            get
            {
                if (null == _importer)
                {
                    _importer = new ObjImporter(Bootstrapper);
                }

                return _importer;
            }
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static EditorApplication()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Timestamp = false,
                Level = false
            }));
            
            UnityEditor.EditorApplication.update += _bootstrapper.Update;
            UnityEditor.EditorApplication.update += WatchForUninit;
            
            Serializer = new JsonSerializer();
            Http = new EditorHttpService(Serializer, Bootstrapper);
            Api = new ApiController(Http);
            Config = new EditorConfigurationManager();
            Config.OnUpdate += Config_OnUpdate;
            Config.Startup();
        }

        /// <summary>
        /// Polls Unity for uninitialize info.
        /// </summary>
        private static void WatchForUninit()
        {
            if (UnityEditor.EditorApplication.isCompiling
                || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Config.OnUpdate -= Config_OnUpdate;
                Config.Teardown();
                Http.Abort();

                Log.Info(Bootstrapper, "Shutting down bootstrapper.");

                UnityEditor.EditorApplication.update -= _bootstrapper.Update;
                UnityEditor.EditorApplication.update -= WatchForUninit;
            }
        }

        /// <summary>
        /// Called when a configuration has been updated.
        /// </summary>
        private static void Config_OnUpdate()
        {
            // set up HttpService
            var env = Config.Environment;
            if (null == env)
            {
                return;
            }

            Http.UrlBuilder.BaseUrl = env.BaseUrl;
            Http.UrlBuilder.Port = env.Port;
            Http.UrlBuilder.Version = env.ApiVersion;

            var credentials = Config.Credentials;
            if (null != credentials)
            {
                SetAuthenticationHeader(credentials.Token);
            }
        }

        /// <summary>
        /// Sets the auth header.
        /// </summary>
        private static void SetAuthenticationHeader(string token)
        {
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

            Log.Info(Config, "Setting Authorization header.");

            Http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                "Authorization",
                string.Format("Bearer {0}", token)));
        }
    }
}