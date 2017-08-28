using CreateAR.Commons.Unity.DebugRenderer;
using strange.extensions.injector.impl;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bindings for SpirePlayer.
    /// </summary>
    public class SpirePlayerModule : IInjectionModule
    {
        /// <summary>
        /// DebugRenderer.
        /// </summary>
        private readonly DebugRenderer _renderer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SpirePlayerModule(DebugRenderer renderer)
        {
            _renderer = renderer;
        }

        /// <inheritdoc cref="IInjectionModule"/>
        public void Load(InjectionBinder binder)
        {
            // application
            {
                binder.Bind<Application>().To<Application>().ToSingleton();
                binder.Bind<IApplicationState>().To<EditApplicationState>().ToName("Default");
                binder.Bind<DebugRenderer>().ToValue(_renderer);
            }

            // input
            {
                binder.Bind<IInputManager>().To<InputManager>().ToSingleton();
                binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
                binder.Bind<IInputState>().To<EditModeInputState>().ToName("EditMode");
            }
        }
    }
}