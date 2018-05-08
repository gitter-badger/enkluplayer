using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    /// <summary>
    /// Hooks into Unity build pipeline.
    /// </summary>
    public static class UnityBuilderHooks
    {
        private static readonly BuildOptions _BuildOptions = BuildOptions.AllowDebugging
            | BuildOptions.Development
            | BuildOptions.ForceEnableAssertions;

        private const string BASE_PATH = "./Builds/";
        private const string PATH_WEBGL = BASE_PATH + "WebGl";
        private const string PATH_WSAX86 = BASE_PATH + "Wsa.x86";
        private const string PATH_IOS = BASE_PATH + "iOS";

        /// <summary>
        /// Switches to Webgl.
        /// </summary>
        [MenuItem("Tools/Platforms/WebGl")]
        public static void SwitchPlatformWebgl()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/PlayMode.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Qr.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/WorldMeshCaptureMode.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/InputLogin.unity", false),
            };
        }

        /// <summary>
        /// Switches to Wsa.
        /// </summary>
        [MenuItem("Tools/Platforms/Wsa")]
        private static void SwitchPlatformWsa()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/PlayMode.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Qr.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/WorldMeshCaptureMode.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/InputLogin.unity", false),
            };
        }

        /// <summary>
        /// Switches to Ios.
        /// </summary>
        [MenuItem("Tools/Platforms/Ios")]
        private static void SwitchToIos()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/PlayMode.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Qr.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/WorldMeshCaptureMode.unity", false),
                new EditorBuildSettingsScene("Assets/Scenes/InputLogin.unity", true),
            };
        }

        /// <summary>
        /// Builds app.
        /// </summary>
        [MenuItem("Tools/Build/Targets/WebGl")]
        public static void BuildWebGlPlayer()
        {
            SwitchPlatformWebgl();

            BuildPlayer(PATH_WEBGL,
                BuildTarget.WebGL);
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
            var options = new BuildPlayerOptions
            {
                locationPathName = path,
                target = target,
                options = _BuildOptions
            };

            BuildPipeline.BuildPlayer(options);
        }
    }
}