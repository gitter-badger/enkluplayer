using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Information about a Script failing to load.
    /// </summary>
    public struct ScriptLoadFailure
    {
        /// <summary>
        /// The ScriptData that failed.
        /// </summary>
        public ScriptData ScriptData;
        
        /// <summary>
        /// The Exception causing failure.
        /// </summary>
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
        
        /// <summary>
        /// A collection of load failures this IScriptLoader experienced.
        /// </summary>
        List<ScriptLoadFailure> LoadFailures { get; }

        /// <summary>
        /// Loads script source, asynchronously.
        /// </summary>
        /// <param name="script">The associated data.</param>
        /// <returns></returns>
        IAsyncToken<string> Load(ScriptData script);

        /// <summary>
        /// Resets tracking of load failures and resets the queue length.
        /// </summary>
        void ResetLoadTracking();
    }
}