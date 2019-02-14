using System;
using Jint;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This implementation prototype defines an object capable of generating scripting host instances pre-configured.
    /// </summary>
    public interface IScriptingHostFactory
    {
        /// <summary>
        /// Creates a new <see cref="UnityScriptingHost"/> instance using the default configuration options.
        /// </summary>
        /// <param name="context">The context is usually the calling instance.</param>
        UnityScriptingHost NewScriptingHost(object context);

        /// <summary>
        /// Creates a new <see cref="UnityScriptingHost"/> instance using the <see cref="Options"/> override method
        /// provided.
        /// </summary>
        /// <param name="context">The context is usually the calling instance.</param>
        /// <param name="optionsOverride">A delegate which accepts an <see cref="Options"/> object which can
        /// be used to configure the engine instance</param>
        UnityScriptingHost NewScriptingHost(object context, Action<Options> optionsOverride);
    }
}