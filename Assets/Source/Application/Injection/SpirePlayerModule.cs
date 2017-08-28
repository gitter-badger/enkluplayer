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
            // application
            {
                binder.Bind<Application>().To<Application>().ToSingleton();
                binder.Bind<EditApplicationState>().To<EditApplicationState>();
            }

            // input
            {
                binder.Bind<IInputState>().To<EditModeInputState>();
                binder.Bind<IInputManager>().To<InputManager>().ToSingleton();
                binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
            }
        }
    }
}