using strange.extensions.injector.impl;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bindings for SpirePlayer.
    /// </summary>
    public class SpirePlayerModule : IInjectionModule
    {
        /// <inheritdoc cref="IInjectionModule"/>
        public void Load(InjectionBinder binder)
        {
            binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
        }
    }
}