using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Editor
{
    /// <summary>
    /// Hooks into Unity build pipeline.
    /// </summary>
    public static class UnityBuilderHooks
    {
        private const string BASE_PATH = "./Builds/";
        private const string PATH_WEBGL = BASE_PATH + "WebGLPlayer";
        private const string PATH_WSAX86 = BASE_PATH + "WSAPlayerX86";
        private const string PATH_IOS = BASE_PATH + "iOS";

        /// <summary>
        /// Switches to Webgl.
        /// </summary>
        [MenuItem("Tools/Platforms/WebGl")]
        public static void SwitchPlatformWebgl()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
        }

        /// <summary>
        /// Switches to Wsa.
        /// </summary>
        [MenuItem("Tools/Platforms/Wsa")]
        private static void SwitchPlatformWsa()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
        }

        /// <summary>
        /// Switches to Ios.
        /// </summary>
        [MenuItem("Tools/Platforms/Ios")]
        private static void SwitchToIos()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/WebGl")]
        public static void BuildWebGlPlayer()
        {
            SwitchPlatformWebgl();

            BuildPlayer(PATH_WEBGL, BuildTarget.WebGL);
        }

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/Wsa")]
        public static void BuildWsaPlayer()
        {
            SwitchPlatformWsa();

            BuildPlayer(PATH_WSAX86,
                BuildTarget.WSAPlayer);
        }
        
        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/iOS")]
        public static void BuildIos()
        {
            SwitchToIos();

            BuildPlayer(PATH_IOS,
                BuildTarget.iOS);
        }

        /// <summary>
        /// Runs unit tests.
        /// </summary>
        [MenuItem("Tools/Build/Run Unit Tests")]
        public static void RunTests()
        {
            Debug.Log("Running tests.");

            UnitTestRunner.Run(new UnitTestListener());
        }

        /// <summary>
        /// Builds the player.
        /// </summary>
        /// <param name="path">Path to build to.</param>
        /// <param name="target">The target to build for.</param>
        private static void BuildPlayer(
            string path,
            BuildTarget target)
        {
            BuildPipeline.BuildPlayer(
                new[]
                {
                    new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
                    new EditorBuildSettingsScene("Assets/Scenes/PlayMode.unity", true)
                },
                path,
                target,
                BuildOptions.None);
        }
    }
}