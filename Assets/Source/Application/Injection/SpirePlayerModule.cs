using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Spire;
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
            // dependencies
            {
                binder.Bind<ISerializer>().To<JsonSerializer>();
                binder.Bind<BridgeMessageHandler>().To<BridgeMessageHandler>().ToSingleton();
            }

            // application
            {
#if UNITY_EDITOR
                binder.Bind<IBridge>().To<EditorBridge>().ToSingleton();
#elif UNITY_WEBGL
                binder.Bind<IBridge>().ToValue(LookupComponent<WebBridge>());
#elif NETFX_CORE
                binder.Bind<IBridge>().To<UwpBridge>().ToSingleton();
#endif

                binder.Bind<IApplicationHost>().To<ApplicationHost>().ToSingleton();
                binder.Bind<IApplicationState>().To<ApplicationState>().ToSingleton();

                binder.Bind<Application>().To<Application>().ToSingleton();

                // application states
                {
                    binder.Bind<InitializeApplicationState>().To<InitializeApplicationState>();
                    binder.Bind<EditApplicationState>().To<EditApplicationState>();
                    binder.Bind<PreviewApplicationState>().To<PreviewApplicationState>();
                }
                
                binder.Bind<IMessageRouter>().To<MessageRouter>().ToSingleton();
                binder.Bind<IHttpService>()
                    .To(new HttpService(
                            new JsonSerializer(),
                            LookupComponent<MonoBehaviourBootstrapper>()))
                    .ToSingleton();
                binder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();

                // TODO: These should just be events from the bridge.
#if UNITY_EDITOR
                binder.Bind<IAssetUpdateService>().To<EditorAssetUpdateService>();
#else
                binder.Bind<IAssetUpdateService>().To<WebAssetUpdateService>();
#endif
            }

            // input
            {
                binder.Bind<IState>().To<EditModeInputState>().ToName(NamedInjections.INPUT_STATE_DEFAULT);
                binder.Bind<IInputManager>().To<InputManager>().ToSingleton();
                binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
            }

            // tagged components
            {
                binder.Bind<MainCamera>().ToValue(LookupComponent<MainCamera>());
                binder.Bind<InputConfig>().ToValue(LookupComponent<InputConfig>());
                binder.Bind<IBootstrapper>().ToValue(LookupComponent<MonoBehaviourBootstrapper>());
                binder.Bind<WebBridge>().ToValue(LookupComponent<WebBridge>());
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