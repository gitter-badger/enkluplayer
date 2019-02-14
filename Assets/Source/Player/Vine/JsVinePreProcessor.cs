using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Source.Player.Scripting;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.EnkluPlayer.Vine
{
    /// <summary>
    /// Preprocessor.
    /// </summary>
    public class JsVinePreProcessor : IVinePreProcessor
    {
        /// <summary>
        /// For creating unique ids.
        /// </summary>
        private static long IDS;

        /// <summary>
        /// JS engine
        ///
        /// TODO: This doesn't taken into account script debugging configuration because it
        /// TODO: uses a regular JS Engine instead of UnityScriptingHost. TBD
        /// </summary>
        private readonly Engine _engine = new Engine(options =>
        {
            options.AllowClr();
            options.CatchClrExceptions(exception =>
            {
                throw exception;
            });

            options.DebugMode(false);
            options.AllowDebuggerStatement(false);
        });

        /// <inheritdoc />
        public ElementSchema DataStore { get; set; }

        /// <inheritdoc />
        public string Execute(string data)
        {
            _engine.Execute("state = {};");

            Verbose("Starting preprocessor : {0}", data);

            // data replacement from schema
            data = ReplaceProps(data);

            Verbose("Props replaced : {0}", data);

            // run JS
            data = RunJs(data);

            Verbose("JS run : {0}", data);

            return data;
        }

        /// <summary>
        /// Runs JS and returns resulting string.
        /// </summary>
        /// <param name="data">The vine document.</param>
        /// <returns></returns>
        private string RunJs(string data)
        {
            var builder = new StringBuilder();
            var start = data.IndexOf("{{", StringComparison.Ordinal);
            while (-1 != start)
            {
                var end = data.IndexOf("}}", start, StringComparison.Ordinal);
                if (-1 == end)
                {
                    throw new Exception("Invalid preprocessor directives. Start with no end.");
                }

                // cut data
                var markup = data.Substring(0, start);
                var script = data.Substring(start + 2, end - (start + 2));
                data = data.Substring(end + 2);

                // append up till script
                builder.Append(markup);

                // process script + append output
                string contextName;
                script = ExecutionContext(script, out contextName);
                _engine.Execute(script);

                var callable = _engine.GetFunction(contextName);
                var value = callable.Call(JsValue.Undefined, new JsValue[0]);

                if (!value.IsUndefined())
                {
                    var stringValue = value.AsString();
                    builder.Append(stringValue);
                }

                // move along
                start = data.IndexOf("{{", StringComparison.Ordinal);
            }

            // append the last of the data
            builder.Append(data);
            return builder.ToString();
        }

        /// <summary>
        /// Replace prop keys with values.
        /// </summary>
        /// <param name="data">The full data string.</param>
        /// <returns></returns>
        private string ReplaceProps(string data)
        {
            if (null == DataStore)
            {
                DataStore = new ElementSchema();
            }

            var propValue = string.Empty;

            var simpleQuery = new Regex(@"\{\[([\d\w=-_\s\:\.""']+)\]\}");
            var match = simpleQuery.Match(data);
            while (match.Success)
            {
                var value = match.Groups[1].Value;
                var substrings = value.Split(':');
                if (1 == substrings.Length)
                {
                    // no definition is provided, assume it is a string
                    // TODO: keep a running list of this info, as this may be the second time to see this
                    var propName = substrings[0];
                    if (DataStore.HasOwnProp(propName))
                    {
                        propValue = DataStore.GetOwn(propName, "").Value;
                    }
                    else
                    {
                        Log.Warning(this, "No match for '{0}' in schema and no default value provided.", propName);
                        propValue = "";
                    }
                }
                else
                {
                    // definition is provided
                    var propName = substrings[0].Trim();
                    var defaultValue = string.Empty;
                    var definition = substrings[1].Trim();
                    substrings = definition.Split('=');

                    string type;
                    if (1 == substrings.Length)
                    {
                        // no default value, just type
                        type = definition;
                    }
                    else
                    {
                        // type and default value are included in definition
                        type = substrings[0].Trim();
                        defaultValue = substrings[1].Trim();
                    }

                    switch (type)
                    {
                        case ElementActionSchemaTypes.INT:
                        {
                            int defaultIntValue;
                            int.TryParse(defaultValue, out defaultIntValue);
                            propValue = DataStore.GetOwn(propName, defaultIntValue).Value.ToString();

                            break;
                        }
                        case ElementActionSchemaTypes.FLOAT:
                        {
                            float defaultFloatValue;
                            float.TryParse(defaultValue, out defaultFloatValue);
                            propValue = DataStore.GetOwn(propName, defaultFloatValue).Value.ToString();

                            break;
                        }
                        case ElementActionSchemaTypes.BOOL:
                        {
                            bool defaultBoolValue;
                            bool.TryParse(defaultValue, out defaultBoolValue);
                            propValue = DataStore.GetOwn(propName, defaultBoolValue).Value.ToString().ToLower();

                            break;
                        }
                        case ElementActionSchemaTypes.STRING:
                        {
                            if (defaultValue.StartsWith("\'") && defaultValue.EndsWith("\'"))
                            {
                                defaultValue = defaultValue.Trim('\'');
                            }
                            else if (defaultValue.StartsWith("\"") && defaultValue.EndsWith("\""))
                            {
                                defaultValue = defaultValue.Trim('"');
                            }

                            propValue = DataStore.GetOwn(propName, defaultValue).Value;

                            break;
                        }
                    }
                }

                // replace
                data = data.Substring(0, match.Index) + propValue + data.Substring(match.Index + match.Length);

                // move along
                match = simpleQuery.Match(data);
            }

            return data;
        }
        
        /// <summary>
        /// Creates a safe-ish execution context.
        /// </summary>
        /// <param name="script">Script to execute.</param>
        /// <param name="contextName">Outputs a unique context name.</param>
        /// <returns></returns>
        private string ExecutionContext(
            string script,
            out string contextName)
        {
            contextName = string.Format("Context_{0}", IDS++);
            return string.Format(@"
function {0}()
{{
    {1}
}}",
                contextName,
                script);
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}