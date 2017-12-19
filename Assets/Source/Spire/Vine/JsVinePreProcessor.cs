using System;
using System.Text;
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
        private static long _ids = 0;
        
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

        /// <summary>
        /// Transforms an input stream.
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <returns></returns>
        public string Execute(string data)
        {
            var builder = new StringBuilder();

            _engine.Execute("state = {};");

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
        /// Creates a safe-ish execution context.
        /// </summary>
        /// <param name="script">Script to execute.</param>
        /// <param name="contextName">Outputs a unique context name.</param>
        /// <returns></returns>
        private string ExecutionContext(
            string script,
            out string contextName)
        {
            contextName = string.Format("Context_{0}", _ids++);
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