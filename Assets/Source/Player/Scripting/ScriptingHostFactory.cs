using System;
using CreateAR.EnkluPlayer;
using CreateAR.EnkluPlayer.Scripting;
using Jint;

namespace Assets.Source.Player.Scripting
{
    /// <summary>
    /// Basic pass-through factory for instantiating pre-configured scripting engine instances.
    /// </summary>
    public class ScriptingHostFactory : IScriptingHostFactory
    {
        #region Static Factory Methods

        /// <summary>
        /// Static helper method for creating a <see cref="Engine"/> using default configuration.
        /// </summary>
        public static Engine NewEngine(bool enableDebug = false)
        {
            return new Engine(options =>
            {
                options.AllowClr();
                options.CatchClrExceptions(exception =>
                {
                    throw exception;
                });

                // Debugging Configuration
                options.DebugMode(enableDebug);
                options.AllowDebuggerStatement(enableDebug);
            });
        }

        #endregion

        private readonly IScriptRequireResolver _resolver;
        private readonly IScriptManager _scripts;
        private readonly ApplicationConfig _applicationConfig;

        /// <summary>
        /// Creates a new <see cref="ScriptingHostFactory"/> instance.
        /// </summary>
        public ScriptingHostFactory(
            IScriptRequireResolver resolver,
            IScriptManager scripts,
            ApplicationConfig applicationConfig)
        {
            _resolver = resolver;
            _scripts = scripts;
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
                throw exception;
            });

            // Debugging Configuration
            var enableScriptDebugging = _applicationConfig.Debug.EnableScriptDebugging;
            options.DebugMode(enableScriptDebugging);
            options.AllowDebuggerStatement(enableScriptDebugging);
        }

        /// <inheritdoc />
        public UnityScriptingHost NewScriptingHost(object context)
        {
            return new UnityScriptingHost(context, _resolver, _scripts, ConfigureWithApplicationConfig);
        }

        /// <inheritdoc />
        public UnityScriptingHost NewScriptingHost(object context, Action<Options> optionsOverride)
        {
            return new UnityScriptingHost(context, _resolver, _scripts,
                options =>
                {
                    // Apply Defaults
                    ConfigureWithApplicationConfig(options);

                    // Apply Override
                    if (null != optionsOverride)
                    {
                        optionsOverride(options);
                    }
                });
        }
    }
}