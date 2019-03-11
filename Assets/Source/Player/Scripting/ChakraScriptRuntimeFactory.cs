#if NETFX_CORE
using Enklu.Orchid;
using Enklu.Orchid.Chakra;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Orchid.Chakra implementation for creating <see cref="IJsRuntime"/> instances.
    /// </summary>
    public class ChakraScriptRuntimeFactory : IScriptRuntimeFactory
    {
        private readonly ApplicationConfig _applicationConfig;

        /// <summary>
        /// Creates a new <see cref="ChakraScriptRuntimeFactory"/> instance.
        /// </summary>
        public ChakraScriptRuntimeFactory(ApplicationConfig applicationConfig)
        {
            _applicationConfig = applicationConfig;
        }

        /// <summary>
        /// Create a new JavaScript runtime implemented with Jint.
        /// </summary>
        public IJsRuntime NewRuntime()
        {
            return new JsRuntime();
        }
    }
}

#endif