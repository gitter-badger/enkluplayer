using UnityEditor;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Hooks into Unity build pipeline.
    /// </summary>
    public static class UnityBuilder
    {
        /// <summary>
        /// Builds a Unity webgl app.
        /// </summary>
        public static void BuildWebGlPlayer()
        {
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/main.unity" },
                locationPathName = "../Build",
                target = BuildTarget.WebGL,
                options = BuildOptions.AllowDebugging
                          | BuildOptions.Development
                          | BuildOptions.ForceEnableAssertions
            };
            
            BuildPipeline.BuildPlayer(options);
        }
    }
}