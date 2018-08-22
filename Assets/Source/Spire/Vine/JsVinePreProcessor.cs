using System;
using System.Text;
using System.Text.RegularExpressions;
using CreateAR.SpirePlayer.IUX;
using Jint;
using Jint.Native;

namespace CreateAR.SpirePlayer.Vine
{
    /// <summary>
    /// Preprocessor.
    /// </summary>
    public class JsVinePreProcessor : IVinePreProcessor
    {
        /// <summary>
        /// For creating unique ids.
        /// </summary>
        private static long IDS = 0;
        
        /// <summary>
        /// JS engine.
        /// </summary>
        private readonly Engine _engine = new Engine(options =>
        {
            options.AllowClr();
            options.CatchClrExceptions(exception =>
            {
                throw exception;
            });
        });

        /// <inheritdoc />
        public ElementSchema DataStore { get; set; }

        /// <inheritdoc />
        public string Execute(string data)
        {
            _engine.Execute("state = {};");

            // data replacement from schema
            data = ReplaceProps(data);

            // run JS
            return RunJs(data);
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
                return data;
            }

            var query = new Regex(@"\{\[([\w-_\.]+)\]\}");
            var match = query.Match(data);
            while (match.Success)
            {
                var propName = match.Groups[1].Value;
                if (DataStore.HasOwnProp(propName))
                {
                    var propValue = DataStore.GetOwnValue(propName).ToString();
                    data = data.Substring(0, match.Index) + propValue + data.Substring(match.Index + match.Length);
                }

                // move along
                match = match.NextMatch();
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
    }
}