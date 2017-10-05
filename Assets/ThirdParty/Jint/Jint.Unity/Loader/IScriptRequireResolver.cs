using Jint.Native;

namespace Jint.Unity
{
    /// <summary>
    /// Describes an interface for requiring scripts.
    /// </summary>
    public interface IScriptRequireResolver
    {
        /// <summary>
        /// Retrieves a JsValue for a string.
        /// </summary>
        JsValue Resolve(string require);
    }
}