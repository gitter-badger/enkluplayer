using strange.extensions.injector.impl;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Extensions to strange ioc.
    /// </summary>
    public static class InjectionExtensions
    {
        /// <summary>
        /// Extension to help load modules.
        /// </summary>
        /// <param name="this">The <c>InjectionBinder</c> instance.</param>
        /// <param name="module">The module to laod.</param>
        /// <returns>Itself.</returns>
        public static InjectionBinder Load(
            this InjectionBinder @this,
            IInjectionModule module)
        {
            module.Load(@this);

            return @this;
        }
    }
}