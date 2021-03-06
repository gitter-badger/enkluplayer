﻿using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Http.Editor;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages;
using UnityEditor;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Holds objects and builds them appropriately for edit mode..
    /// </summary>
    [InitializeOnLoad]
    public static class EditorApplication
    {
        /// <summary>
        /// True iff the app has been initialized.
        /// </summary>
        private static bool _isRunning;

        /// <summary>
        /// Backing variable for Bootstrapper.
        /// </summary>
        private static readonly EditorBootstrapper _bootstrapper = new EditorBootstrapper();

        /// <summary>
        /// Log target we add and remove.
        /// </summary>
        private static readonly ILogTarget _logTarget = new UnityLogTarget(new DefaultLogFormatter
        {
            Timestamp = false,
            Level = false
        });

        /// <summary>
        /// Obj importer, lazily created.
        /// </summary>
        private static ScanImporter _importer;

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
        public static ScanImporter ScanImporter
        {
            get
            {
                if (null == _importer)
                {
                    _importer = new ScanImporter(Bootstrapper);
                }

                return _importer;
            }
        }

        /// <summary>
        /// True iff editor app is initialized.
        /// </summary>
        public static bool IsRunning
        {
            get { return _isRunning; }
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static EditorApplication()
        {
            UnityEditor.EditorApplication.update += WatchForUninit;
        }

        /// <summary>
        /// Polls Unity for uninitialize info.
        /// </summary>
        private static void WatchForUninit()
        {
            if (UnityEditor.EditorApplication.isCompiling
                || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Uninit();
            }
            else if (!UnityEditor.EditorApplication.isPlaying)
            {
                Init();
            }
        }

        private static void Init()
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;

            Log.AddLogTarget(_logTarget);
            
            UnityEditor.EditorApplication.update += _bootstrapper.Update;

            Serializer = new JsonSerializer();
            Http = new EditorHttpService(Serializer, Bootstrapper, new UrlFormatterCollection());
            Api = new ApiController(Http);
            Config = new EditorConfigurationManager();
            Config.OnUpdate += Config_OnUpdate;
            Config.Startup();
        }

        private static void Uninit()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            
            Log.RemoveLogTarget(_logTarget);

            Config.OnUpdate -= Config_OnUpdate;
            Config.Teardown();
            Http.Abort();

            UnityEditor.EditorApplication.update -= _bootstrapper.Update;
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

            var formatter = new UrlFormatter();
            formatter.FromUrl(env.TrellisUrl);

            Http.Urls.Register("trellis", formatter);

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
            Http.Headers["Authorization"] = string.Format("Bearer {0}", token);
        }
    }
}