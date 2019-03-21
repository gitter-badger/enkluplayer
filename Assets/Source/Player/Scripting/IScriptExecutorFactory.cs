using System;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This implementation prototype defines an object capable of generating scripting host instances pre-configured.
    /// </summary>
    public interface IScriptExecutorFactory
    {
        /// <summary>
        /// Creates a new <see cref="IJsExecutionContext"/> instance using the default configuration options.
        /// </summary>
        /// <param name="context">The context is usually the calling instance.</param>
        IJsExecutionContext NewExecutionContext(object context);
    }
}