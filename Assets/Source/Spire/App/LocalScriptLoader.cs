using System;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Implementation of <c>IScriptLoader</c> that returns a string.
    /// </summary>
    public class LocalScriptLoader : IScriptLoader
    {
        /// <summary>
        /// The program string.
        /// </summary>
        public string Program { get; set; }

        /// <inheritdoc cref="IScriptLoader"/>
        public IAsyncToken<string> Load(ScriptData script)
        {
            if (string.IsNullOrEmpty(Program))
            {
                return new AsyncToken<string>(new Exception("No script to load!"));
            }

            return new AsyncToken<string>(Program);
        }
    }
}