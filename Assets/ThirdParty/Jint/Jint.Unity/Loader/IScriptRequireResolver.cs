using System.Reflection;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
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
        object Resolve(IScriptManager scripts, IJsExecutionContext executionContext, string require);
    }
}