using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// This implementation prototype represents an object capable of generating <see cref="IJsRuntime"/> implementations.
    /// </summary>
    public interface IScriptRuntimeFactory
    {
        /// <summary>
        /// This method creates a new <see cref="IJsRuntime"/> implementation.
        /// </summary>
        IJsRuntime NewRuntime();
    }
}