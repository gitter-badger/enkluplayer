using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    public struct ScriptLoadFailure
    {
        public ScriptData ScriptData;
        public Exception Exception;
    }
    
    /// <summary>
    /// Interface for loading script source.
    /// </summary>
    public interface IScriptLoader
    {
        /// <summary>
        /// The number of currently loading scripts.
        /// </summary>
        int QueueLength { get; }
        
        List<ScriptLoadFailure> LoadFailures { get; }

        /// <summary>
        /// Loads script source, asynchronously.
        /// </summary>
        /// <param name="script">The associated data.</param>
        /// <returns></returns>
        IAsyncToken<string> Load(ScriptData script);
    }
}