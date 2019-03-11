using System;
using System.Collections.Generic;
using System.Reflection;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.Scripting;
using Enklu.Orchid;
using strange.extensions.injector.api;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Resolves objects from calls to require().
    /// </summary>
    public class EnkluScriptRequireResolver : IScriptRequireResolver
    {
        /// <summary>
        /// Abstraction of generic implementation of a JsInterface
        /// </summary>
        private interface IJsInterfaceRecord
        {
            /// <summary>
            /// Value passed to require().
            /// </summary>
            string Id { get; }

            /// <summary>
            /// Creates a new requires record for the interface.
            /// </summary>
            IRequireRecord NewRecord(string id, IJsExecutionContext executionContext);
        }

        /// <summary>
        /// Abstraction of generic implementation of a RequireRecord
        /// </summary>
        private interface IRequireRecord
        {
            /// <summary>
            /// Value passed to require().
            /// </summary>
            string Id { get; }

            /// <summary>
            /// Gets the record value as generic object.
            /// </summary>
            object Value { get; }
        }

        /// <summary>
        /// Tracks JsInterface implementations.
        /// </summary>
        private class JsInterfaceRecord<T> : IJsInterfaceRecord
        {
            /// <inheritdoc />
            public string Id { get; private set; }

            /// <summary>
            /// C# value.
            /// </summary>
            private readonly T _value;

            /// <summary>
            /// Constructor.
            /// </summary>
            public JsInterfaceRecord(string id, T value)
            {
                Id = id;
                _value = value;
            }

            /// <inheritdoc />
            public IRequireRecord NewRecord(string varName, IJsExecutionContext executionContext)
            {
                executionContext.SetValue<T>(varName, _value);

                return new RequireRecord<T>(Id, _value);
            }
        }

        /// <summary>
        /// Tracks require.
        /// </summary>
        private class RequireRecord<T> : IRequireRecord
        {
            /// <summary>
            /// Value to return.
            /// </summary>
            private readonly T _value;

            /// <inheritdoc />
            public string Id { get; private set; }

            /// <inheritdoc />
            public object Value { get { return _value; } }

            /// <summary>
            /// Constructor.
            /// </summary>
            public RequireRecord(string id, T value)
            {
                Id = id;
                _value = value;
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
        private readonly List<IJsInterfaceRecord> _global = new List<IJsInterfaceRecord>();

        /// <summary>
        /// Lists of objects by executionContext.
        /// </summary>
        private readonly Dictionary<IJsExecutionContext, List<IRequireRecord>> _records = new Dictionary<IJsExecutionContext, List<IRequireRecord>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="binder">Requires the binder to resolve JsInterfaces.</param>
        public EnkluScriptRequireResolver(IInjectionBinder binder)
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
                        _global.Add(MakeInterfaceRecord(attribute.Name, _binder, type));
                    }
                }
            }
        }

        /// <inheritdoc cref="IScriptRequireResolver"/>
        public object Resolve(
            IScriptManager scripts,
            IJsExecutionContext executionContext,
            string require)
        {
            // retrieve already existing records
            var records = Records(executionContext);
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

                    var record = jsInterface.NewRecord(id, executionContext);
                    records.Add(record);

                    return record.Value;
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
                    Log.Error(this, "Could not find require in Resources '{0}'.", require);
                    return null;
                }

                source = asset.text;
            }
            // find script
            else
            {
                var script = scripts.FindOne(require);
                if (null == script)
                {
                    Log.Error(this, "Could not find require '{0}'.", require);
                    return null;
                }

                source = script.Source;
            }

            // modularize it
            var variableName = "require" + _ids++;
            var moduleCode = REQUIRE_TEMPLATE
                .Replace("{{script}}", source)
                .Replace("{{variableName}}", variableName);

            object module;
            try
            {
                executionContext.RunScript(moduleCode);
                module = executionContext.GetValue<object>(variableName);
            }
            catch (Exception exception)
            {
                Log.Error(this, string.Format("Could not execute {0} : {1}.", require, exception));
                return null;
            }

            records.Add(new RequireRecord<object>(require, module));
            return module;
        }

        /// <summary>
        /// Finds or creates a list of records for an <c>Engine</c>.
        /// </summary>
        /// <param name="engine">The executionContext.</param>
        /// <returns></returns>
        private List<IRequireRecord> Records(IJsExecutionContext engine)
        {
            List<IRequireRecord> records;
            if (!_records.TryGetValue(engine, out records))
            {
                records = _records[engine] = new List<IRequireRecord>();
            }

            return records;
        }

        /// <summary>
        /// Cache Generic Method Info
        /// </summary>
        private static readonly MethodInfo NewInterfaceMethodInfo = typeof(EnkluScriptRequireResolver)
            .GetMethod("NewInterfaceRecord", BindingFlags.Static | BindingFlags.NonPublic);

        /// <summary>
        /// Creates a new <see cref="IJsInterfaceRecord"/> implementation by passing the <see cref="Type"/>
        /// as a generic parameter into a factory method.
        /// </summary>
        private static IJsInterfaceRecord MakeInterfaceRecord(string id, IInjectionBinder binder, Type type)
        {
            var genericMethod = NewInterfaceMethodInfo.MakeGenericMethod(type);

            return (IJsInterfaceRecord) genericMethod.Invoke(null, new object[] { id, binder });
        }

        /// <summary>
        /// Generic factory method we call via reflection to ensure type parameters are carried through.
        /// </summary>
        private static IJsInterfaceRecord NewInterfaceRecord<T>(string id, IInjectionBinder binder)
        {
            return new JsInterfaceRecord<T>(id, binder.GetInstance<T>());
        }
    }
}