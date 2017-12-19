using System;
using System.Text;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using UnityEngine;

namespace CreateAR.SpirePlayer.Vine
{
    public class VinePreprocessor
    {
        private static long IDS = 0;
        
        private readonly Engine _engine = new Engine(options =>
        {
            options.AllowClr();
            options.CatchClrExceptions(exception =>
            {
                throw exception;
            });
        });

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
                Debug.Log(script);
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