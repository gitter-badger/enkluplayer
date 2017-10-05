using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Unity;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class SpireScriptRequireResolver : IScriptRequireResolver
    {
        private class RequireRecord
        {
            public readonly string Id;
            public readonly JsValue Value;

            public RequireRecord(string id, JsValue value)
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

        private readonly IScriptManager _scripts;
        private readonly Engine _engine;
        
        private readonly List<RequireRecord> _records = new List<RequireRecord>();

        public SpireScriptRequireResolver(
            IScriptManager scripts,
            Engine engine)
        {
            _scripts = scripts;
            _engine = engine;
        }

        public JsValue Resolve(string require)
        {
            // retrieve already existing records
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Id == require)
                {
                    return record.Value;
                }
            }

            // retrieve from engine directly
            var value = _engine.GetValue(require);
            if (!value.IsNull() && !value.IsUndefined())
            {
                _records.Add(new RequireRecord(
                    require,
                    value));
                return value;
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
                var script = _scripts.FindOne(require);
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
                _engine.Execute(moduleCode);
                module = _engine.GetValue(variableName);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message);

                return JsValue.Undefined;
            }

            _records.Add(new RequireRecord(require, module));

            return module;
        }
    }
}