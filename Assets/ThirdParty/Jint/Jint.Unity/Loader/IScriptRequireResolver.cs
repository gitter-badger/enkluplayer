using System.Reflection;
using CreateAR.SpirePlayer;
using Jint.Native;

namespace Jint.Unity
{
    /// <summary>
    /// Describes an interface for requiring scripts.
    /// </summary>
    public interface IScriptRequireResolver
    {
        /// <summary>
        /// Readies the resolver.
        /// </summary>
        void Initialize(params Assembly[] assemblies);

        /// <summary>
        /// Retrieves a JsValue for a string.
        /// </summary>
        JsValue Resolve(IScriptManager scripts, Engine engine, string require);
    }
}