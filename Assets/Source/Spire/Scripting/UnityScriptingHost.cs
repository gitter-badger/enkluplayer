﻿using System;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using Jint.Unity;
using UnityEngine;
using Object = System.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Hosts scripts and provides a default Unity API.
    /// </summary>
    public class UnityScriptingHost : Engine
    {
        /// <summary>
        /// Template for executing require'd scripts.
        /// </summary>
        private const string REQUIRE_TEMPLATE = @"
// prep modules
var module = module || {
    exports : {
        //
    }
};

// execute require
(function() {
    {{script}}
})();

var {{variableName}} = module.exports;
module = null;
";

        /// <summary>
        /// Sequential ids for requires.
        /// </summary>
        private int _ids = 0;

        /// <summary>
        /// Loads scripts.
        /// </summary>
        public IScriptLoader Loader;

        /// <summary>
        /// Object to resolve dependencies.
        /// </summary>
        public IScriptDependencyResolver Resolver;

        /// <summary>
        /// Constructor.
        /// </summary>
        public UnityScriptingHost(
            Object context,
            IScriptLoader loader,
            IScriptDependencyResolver resolver)
            : base(options => options.AllowClr())
        {
            Loader = loader;
            Resolver = resolver;

            SetValue("log", new JsLogWrapper(context));
            SetValue("scene", new UnitySceneManager());
            
            SetValue("require", new Func<string, JsValue>(Require));
            SetValue("inject", new Func<string, object>(Inject));
        }

        /// <summary>
        /// Loads script through resources + executes.
        /// </summary>
        /// <param name="scriptName"></param>
        private JsValue Require(string scriptName)
        {
            if (null == Loader)
            {
                Log.Info(this, "require() failed: no IScriptLoader.");
                return JsValue.Undefined;
            }

            string script;
            if (!Loader.Load(scriptName, out script))
            {
                Log.Info(this, "require() failed: could not load {0}.", scriptName);
                return JsValue.Undefined;
            }
            
            // modularize it
            var variableName = "require" + _ids++;
            var moduleCode = REQUIRE_TEMPLATE
                .Replace("{{script}}", script)
                .Replace("{{variableName}}", variableName);

            JsValue module;
            try
            {
                Execute(moduleCode);
                module = GetValue(variableName);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message);

                return JsValue.Undefined;
            }

            return module;
        }

        /// <summary>
        /// Injector.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private object Inject(string name)
        {
            if (null == Resolver)
            {
                return null;
            }

            return Resolver.Resolve(name);
        }
    }
}