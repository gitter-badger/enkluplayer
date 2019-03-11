using System;
using Enklu.Data;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Basic pass-through factory for instantiating pre-configured scripting engine instances.
    /// </summary>
    public class ScriptExecutorFactory : IScriptExecutorFactory
    {
        private readonly IScriptRuntimeFactory _scriptRuntimeFactory;
        private readonly IScriptRequireResolver _resolver;
        private readonly IScriptManager _scripts;

        /// <summary>
        /// JS Runtime implementation. Use single runtime.
        /// </summary>
        private IJsRuntime _runtime;

        /// <summary>
        /// Creates a new <see cref="ScriptExecutorFactory"/> instance.
        /// </summary>
        public ScriptExecutorFactory(
            IScriptRequireResolver resolver,
            IScriptManager scripts,
            IScriptRuntimeFactory scriptRuntimeFactory)
        {
            _resolver = resolver;
            _scripts = scripts;
            _scriptRuntimeFactory = scriptRuntimeFactory;
        }

        /// <inheritdoc />
        public IJsExecutionContext NewExecutionContext(object context)
        {
            if (null == _runtime)
            {
                _runtime = _scriptRuntimeFactory.NewRuntime();
            }

            return AddHostSupport(context, _runtime.NewExecutionContext());
        }

        /// <summary>
        /// Enhance the available scripting APIs.
        /// </summary>
        private IJsExecutionContext AddHostSupport(object context, IJsExecutionContext executionContext)
        {
            executionContext.SetValue("log", new LogJsApi(context));
            executionContext.SetValue("require", new Func<string, object>(
                value => _resolver.Resolve(_scripts, executionContext, value)));

            // common apis
            executionContext.SetValue("v", Vec3Methods.Instance);
            executionContext.SetValue("vec3", new Func<float, float, float, Vec3>(Vec3Methods.create));
            executionContext.SetValue("vec2", new Func<float, float, Vec2>((x, y) => new Vec2(x, y)));
            executionContext.SetValue("q", QuatMethods.Instance);
            executionContext.SetValue("quat", new Func<float, float, float, float, Quat>(QuatMethods.create));
            executionContext.SetValue("c", Col4Methods.Instance);
            executionContext.SetValue("col", new Func<float, float, float, float, Col4>(Col4Methods.create));
            executionContext.SetValue("time", TimeJsApi.Instance);

            return executionContext;
        }
    }
}