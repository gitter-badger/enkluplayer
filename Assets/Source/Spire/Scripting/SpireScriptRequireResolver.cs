using System;
using System.Collections.Generic;
using System.Reflection;
using CreateAR.Commons.Unity.Logging;
using Jint;
using Jint.Native;
using Jint.Unity;
using strange.extensions.injector.api;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Resolves objects from calls to require().
    /// </summary>
    public class SpireScriptRequireResolver : IScriptRequireResolver
    {
        /// <summary>
        /// Tracks require.
        /// </summary>
        private class RequireRecord
        {
            /// <summary>
            /// Value passed to require().
            /// </summary>
            public readonly string Id;

            /// <summary>
            /// Value to return.
            /// </summary>
            public readonly JsValue Value;

            /// <summary>
            /// Constructor.
            /// </summary>
            public RequireRecord(string id, JsValue value)
            {
                Id = id;
                Value = value;
            }
        }

        /// <summary>
        /// Tracks JsInterface implementations.
        /// </summary>
        private class JsInterfaceRecord
        {
            /// <summary>
            /// Value passed to require().
            /// </summary>
            public readonly string Id;

            /// <summary>
            /// C# value.
            /// </summary>
            public readonly object Value;

            /// <summary>
            /// Constructor.
            /// </summary>
            public JsInterfaceRecord(string id, object value)
            {
                Id = id;
                Value = value;
            }
        }

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
        /// True iff Initialize() has been called.
        /// </summary>
        private bool _isInitialized = false;

        /// <summary>
        /// Binder for JsInterfaces.
        /// </summary>
        private readonly IInjectionBinder _binder;
        
        /// <summary>
        /// List of global C# objects that are shared across hosts.
        /// </summary>
        private readonly List<JsInterfaceRecord> _global = new List<JsInterfaceRecord>();

        /// <summary>
        /// Lists of objects by engine.
        /// </summary>
        private readonly Dictionary<Engine, List<RequireRecord>> _records = new Dictionary<Engine, List<RequireRecord>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="binder">Requires the binder to resolve JsInterfaces.</param>
        public SpireScriptRequireResolver(IInjectionBinder binder)
        {
            _binder = binder;
        }

        /// <inheritdoc cref="IScriptRequireResolver"/>
        public void Initialize(params Assembly[] assemblies)
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            // peek through assemblies
            // TODO: this will be expensive.
            for (int i = 0, iLen = assemblies.Length; i < iLen; i++)
            {
                var types = assemblies[i].GetTypes();
                for (int j = 0, jlen = types.Length; j < jlen; j++)
                {
                    var type = types[j];
                    var attributes = type.GetCustomAttributes(typeof(JsInterfaceAttribute), false);
                    if (1 == attributes.Length)
                    {
                        var attribute = (JsInterfaceAttribute) attributes[0];
                        Log.Info(this, "Adding JS interface for {0}.", attribute.Name);
                        _global.Add(new JsInterfaceRecord(
                            attribute.Name,
                            _binder.GetInstance(type)));
                    }
                }
            }
        }

        /// <inheritdoc cref="IScriptRequireResolver"/>
        public JsValue Resolve(
            IScriptManager scripts,
            Engine engine,
            string require)
        {
            // retrieve already existing records
            var records = Records(engine);
            for (int i = 0, len = records.Count; i < len; i++)
            {
                var record = records[i];
                if (record.Id == require)
                {
                    return record.Value;
                }
            }
            
            // peek through globals
            for (int i = 0, len = _global.Count; i < len; i++)
            {
                var jsInterface = _global[i];
                if (jsInterface.Id == require)
                {
                    var id = "__global_" + _ids++;
                    engine.SetValue(id, jsInterface.Value);

                    var value = engine.GetValue(id);

                    records.Add(new RequireRecord(
                        require,
                        value));

                    return value;
                }
            }

            // see if we can find the source of the missing module
            string source;

            // check if this is from Resources
            if (require.StartsWith("Resources/"))
            {
                var asset = Resources.Load<TextAsset>(require.Replace(
                    "Resources/",
                    string.Empty));
                if (null == asset)
                {
                    throw new Exception(string.Format(
                        "Could not find require in Resources '{0}'.",
                        require));
                }

                source = asset.text;
            }
            // find script
            else
            {
                var script = scripts.FindOne(require);
                if (null == script)
                {
                    throw new Exception(string.Format(
                        "Could not find require '{0}'.",
                        require));
                }

                source = script.Source;
            }

            // modularize it
            var variableName = "require" + _ids++;
            var moduleCode = REQUIRE_TEMPLATE
                .Replace("{{script}}", source)
                .Replace("{{variableName}}", variableName);

            JsValue module;
            try
            {
                engine.Execute(moduleCode);
                module = engine.GetValue(variableName);
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format(
                    "Could not execute {0} : {1}.",
                    require,
                    exception));
            }

            records.Add(new RequireRecord(require, module));

            return module;
        }

        /// <summary>
        /// Finds or creates a list of records for an <c>Engine</c>.
        /// </summary>
        /// <param name="engine">The engine.</param>
        /// <returns></returns>
        private List<RequireRecord> Records(Engine engine)
        {
            List<RequireRecord> records;
            if (!_records.TryGetValue(engine, out records))
            {
                records = _records[engine] = new List<RequireRecord>();
            }

            return records;
        }
    }
}