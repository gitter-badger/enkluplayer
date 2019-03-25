//#if !NETFX_CORE

using CreateAR.Commons.Unity.Logging;
using Enklu.Orchid;
using Enklu.Orchid.Jint;
using Jint;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Orchid.Jint implementation for creating <see cref="IJsRuntime"/> instances.
    /// </summary>
    public class JintScriptRuntimeFactory : IScriptRuntimeFactory
    {
        private readonly ApplicationConfig _applicationConfig;

        public JintScriptRuntimeFactory(ApplicationConfig applicationConfig)
        {
            _applicationConfig = applicationConfig;
        }

        /// <summary>
        /// This is our default engine configuration delegate which uses a set of reasonable defaults
        /// as well as configuration from the application config.
        /// </summary>
        private void ConfigureWithApplicationConfig(Options options)
        {
            // These options are standard
            options.AllowClr();
            options.CatchClrExceptions(exception =>
            {
                Log.Error(this, "CLR exception: {0}: {1}", exception.Message, exception.StackTrace);
                throw exception;
            });

            // Debugging Configuration
            var enableScriptDebugging = _applicationConfig.Debug.EnableScriptDebugging;
            options.DebugMode(enableScriptDebugging);
            options.AllowDebuggerStatement(enableScriptDebugging);
        }

        /// <summary>
        /// Create a new JavaScript runtime implemented with Jint.
        /// </summary>
        public IJsRuntime NewRuntime()
        {
            return new JsRuntime(ConfigureWithApplicationConfig);
        }
    }
}
//#endif