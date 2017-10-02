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
        /// <summary>
        /// Mode.
        /// </summary>
        private readonly PlayMode _mode;

        /// <summary>
        /// Crates a module.
        /// </summary>
        /// <param name="mode">The mode this module should use to advise bindings.</param>
        public SpirePlayerModule(PlayMode mode)
        {
            _mode = mode;
        }

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
                if (_mode == PlayMode.Release)
                {
                    binder.Bind<IBridge>().To<ReleaseBridge>().ToSingleton();
                }
                else
                {
#if UNITY_EDITOR
                    binder.Bind<IBridge>().To<EditorBridge>().ToSingleton();
#elif UNITY_WEBGL
                    binder.Bind<IBridge>().ToValue(LookupComponent<WebBridge>());
#elif NETFX_CORE
                    binder.Bind<IBridge>().To<UwpBridge>().ToSingleton();
#endif   
                }

                binder.Bind<IApplicationHost>().To<ApplicationHost>().ToSingleton();
                binder.Bind<IApplicationState>().To<ApplicationState>().ToSingleton();
                binder.Bind<Application>().To<Application>().ToSingleton();

                // application states
                {
                    binder.Bind<InitializeApplicationState>().To<InitializeApplicationState>();
                    binder.Bind<EditApplicationState>().To<EditApplicationState>();
                    binder.Bind<PreviewApplicationState>().To<PreviewApplicationState>();
                    binder.Bind<PlayApplicationState>().To<PlayApplicationState>();
                }
                
                binder.Bind<IMessageRouter>().To<MessageRouter>().ToSingleton();
                binder.Bind<IHttpService>()
                    .To(new HttpService(
                            new JsonSerializer(),
                            LookupComponent<MonoBehaviourBootstrapper>()))
                    .ToSingleton();
                binder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
                binder.Bind<IFileManager>().To<FileManager>().ToSingleton();

                // TODO: These could just be events from the bridge.
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

            // spire-specific bindings
            AddSpireBindings(binder);
        }

        /// <summary>
        /// Adds bindings for spire.
        /// </summary>
        /// <param name="binder">Object to add bindings to.</param>
        private void AddSpireBindings(InjectionBinder binder)
        {
            binder.Bind<AppController>().To<AppController>();
            binder.Bind<IAppDataManager>().To<AppDataManager>().ToSingleton();
            binder.Bind<IContentManager>().To<ContentManager>().ToSingleton();

            // factory
            {
                binder.Bind<IContentFactory>().To<ContentFactory>();
                binder.Bind<IAnchorReferenceFrameFactory>().To<AnchorReferenceFrameFactory>();
            }

            // configs
            {
                binder.Bind<WidgetConfig>().ToValue(LookupComponent<WidgetConfig>());
                binder.Bind<TweenConfig>().ToValue(LookupComponent<TweenConfig>());
                binder.Bind<ColorConfig>().ToValue(LookupComponent<ColorConfig>());
            }

            // manager monobehaviours
            {
                binder.Bind<ElementManager>().ToValue(LookupComponent<ElementManager>());
                binder.Bind<IntentionManager>().ToValue(LookupComponent<IntentionManager>());
                binder.Bind<ISceneManager>().ToValue(LookupComponent<SceneManager>());
                binder.Bind<LayerManager>().ToValue(LookupComponent<LayerManager>());
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