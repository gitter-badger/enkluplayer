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
            binder.Bind<Application>().To<Application>().ToSingleton();
            binder.Bind<IApplicationState>().To<EditApplicationState>().ToName("Default");

            binder.Bind<IInputManager>().To<InputManager>().ToSingleton();
            binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
            binder.Bind<IInputState>().To<EditModeInputState>().ToName("EditMode");
        }
    }
}