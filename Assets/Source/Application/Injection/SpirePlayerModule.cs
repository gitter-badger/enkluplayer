using System;
using CreateAR.Commons.Unity.Logging;
using strange.extensions.injector.impl;
using Object = UnityEngine.Object;

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
                binder.Bind<IState>().To<EditModeInputState>();
                binder.Bind<IInputManager>().To<InputManager>().ToSingleton();
                binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
            }

            // tagged components
            {
                binder.Bind<MainCamera>().ToValue(LookupComponent<MainCamera>());
                binder.Bind<InputConfig>().ToValue(LookupComponent<InputConfig>());
            }
        }

        /// <summary>
        /// Looks up a component on the Unity hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of component to lookup.</typeparam>
        /// <returns></returns>
        private T LookupComponent<T>() where T : Object
        {
            var value = Object.FindObjectOfType<T>();

            if (null == value)
            {
                var message = string.Format(
                    "Could not find an object of type {0}.",
                    typeof(T));
                Log.Fatal(this, message);
                throw new Exception(message);
            }

            return value;
        }
    }
}