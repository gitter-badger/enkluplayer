using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface for loading script source.
    /// </summary>
    public interface IScriptLoader
    {
        /// <summary>
        /// Loads script source, asynchronously.
        /// </summary>
        /// <param name="script">The associated data.</param>
        /// <returns></returns>
        IAsyncToken<string> Load(ScriptData script);
        
        /// <summary>
        /// Clears the download queue.
        /// </summary>
        void Clear();
    }
}