using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Hooks into Unity build pipeline.
    /// </summary>
    public static class UnityBuilder
    {
        private static readonly string[] _Scenes = new[]
        {
            "Assets/Scenes/main.unity"
        };

        private static readonly BuildOptions _BuildOptions = BuildOptions.AllowDebugging
            | BuildOptions.Development
            | BuildOptions.ForceEnableAssertions;

        private const string BASE_PATH = "./Builds/";
        private const string PATH_WEBGL = BASE_PATH + "WebGl";
        private const string PATH_WSAX86 = BASE_PATH + "Wsa.x86";
        private const string PATH_WSAX64 = BASE_PATH + "Wsa.x64";
        private const string PATH_WSAARM = BASE_PATH + "Wsa.ARM";
        private const string PATH_IOS = BASE_PATH + "iOS";
        private const string PATH_ANDROID = BASE_PATH + "Android";
        private const string PATH_WINDOWS = BASE_PATH + "Standalone.Windows";
        private const string PATH_OSX = BASE_PATH + "Standalone.Osx";

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/WebGl")]
        public static void BuildWebGlPlayer()
        {
            BuildPlayer(
                RuntimePlatform.WebGLPlayer,
                PATH_WEBGL,
                BuildTarget.WebGL);
        }

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/Wsa")]
        public static void BuildWsaPlayer()
        {
            BuildPlayer(
                RuntimePlatform.WSAPlayerX86,
                PATH_WSAX86,
                BuildTarget.WSAPlayer);
        }

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/Android")]
        public static void BuildAndroid()
        {
            BuildPlayer(
                RuntimePlatform.Android,
                PATH_ANDROID,
                BuildTarget.Android);
        }

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/iOS")]
        public static void BuildIos()
        {
            BuildPlayer(
                RuntimePlatform.IPhonePlayer,
                PATH_IOS,
                BuildTarget.iOS);
        }

        /// <summary>
        /// Runs unit tests.
        /// </summary>
        [MenuItem("Tools/Run Unit Tests %t")]
        public static void RunTests()
        {
            UnitTestRunner.Run(new UnitTestListener());
        }

        /// <summary>
        /// Builds the player.
        /// </summary>
        /// <param name="requiredPlatform">Required runtime platform (to avoid accidental reimports).</param>
        /// <param name="path">Path to build to.</param>
        /// <param name="target">The target to build for.</param>
        private static void BuildPlayer(
            RuntimePlatform requiredPlatform,
            string path,
            BuildTarget target)
        {
            /*if (UnityEngine.Application.platform != requiredPlatform)
            {
                throw new Exception(string.Format(
                    "Current RuntimePlatform ({0}) does not match target: {1}.",
                    UnityEngine.Application.platform,
                    requiredPlatform));
            }*/
            
            var options = new BuildPlayerOptions
            {
                scenes = _Scenes,
                locationPathName = path,
                target = target,
                options = _BuildOptions
            };

            BuildPipeline.BuildPlayer(options);
        }
    }
}