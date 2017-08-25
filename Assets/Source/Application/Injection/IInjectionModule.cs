using strange.extensions.injector.impl;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple interface for setting up modules.
    /// </summary>
    public interface IInjectionModule
    {
        /// <summary>
        /// Loads bindings into the <c>InjectionBinder</c> instance.
        /// </summary>
        /// <param name="binder">The binder on which to set bindings.</param>
        void Load(InjectionBinder binder);
    }
}