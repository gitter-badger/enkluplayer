using System;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using Jint.Parser;
using Jint.Unity;
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
            // misc dependencies
            {
                binder.Bind<ISerializer>().To<JsonSerializer>();
                binder.Bind<BridgeMessageHandler>().To<BridgeMessageHandler>().ToSingleton();
                binder.Bind<IMessageRouter>().To<MessageRouter>().ToSingleton();
                binder.Bind<IHttpService>()
                    .To(new HttpService(
                        new JsonSerializer(),
                        LookupComponent<MonoBehaviourBootstrapper>()))
                    .ToSingleton();
                binder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
                binder.Bind<IAssetPoolManager>().To<LazyAssetPoolManager>().ToSingleton();
                binder.Bind<IFileManager>().To<FileManager>().ToSingleton();
                binder.Bind<IElementFactory>().To<ElementFactory>().ToSingleton();

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

                // spire-specific bindings
                AddSpireBindings(binder);

                // services
                {
                    binder.Bind<ApplicationStateService>().To<ApplicationStateService>().ToSingleton();
                    binder.Bind<AuthorizationService>().To<AuthorizationService>().ToSingleton();
                    binder.Bind<AssetUpdateService>().To<AssetUpdateService>().ToSingleton();
                    binder.Bind<ContentGraphUpdateService>().To<ContentGraphUpdateService>().ToSingleton();
                    binder.Bind<ContentUpdateService>().To<ContentUpdateService>().ToSingleton();
                    binder.Bind<ScriptUpdateService>().To<ScriptUpdateService>().ToSingleton();
                }

                // application states
                {
                    binder.Bind<InitializeApplicationState>().To<InitializeApplicationState>();
                    binder.Bind<WaitingForConnectionApplicationState>().To<WaitingForConnectionApplicationState>();
                    binder.Bind<EditApplicationState>().To<EditApplicationState>();
                    binder.Bind<PreviewApplicationState>().To<PreviewApplicationState>();
                    binder.Bind<PlayApplicationState>().To<PlayApplicationState>();
                    binder.Bind<HierarchyApplicationState>().To<HierarchyApplicationState>();
                }

                // service manager + appplication
                binder.Bind<IApplicationServiceManager>().ToValue(new ApplicationServiceManager(
                    binder.GetInstance<IBridge>(),
                    new ApplicationService[]
                    {
                        binder.GetInstance<ApplicationStateService>(),
                        binder.GetInstance<AuthorizationService>(),
                        binder.GetInstance<AssetUpdateService>(),
                        binder.GetInstance<ContentGraphUpdateService>(),
                        binder.GetInstance<ContentUpdateService>(),
                        binder.GetInstance<ScriptUpdateService>()
                    }));
                binder.Bind<Application>().To<Application>().ToSingleton();
            }
        }

        /// <summary>
        /// Adds bindings for spire.
        /// </summary>
        /// <param name="binder">Object to add bindings to.</param>
        private void AddSpireBindings(InjectionBinder binder)
        {
            binder.Bind<AppController>().To<AppController>();
            binder.Bind<IContentManager>().To<ContentManager>().ToSingleton();

            // factory
            {
                binder.Bind<IContentFactory>().To<ContentFactory>();
                binder.Bind<IAnchorReferenceFrameFactory>().To<AnchorReferenceFrameFactory>();
            }

            // configs
            {
                binder.Bind<IWidgetConfig>().ToValue(LookupComponent<WidgetConfig>());
                binder.Bind<ITweenConfig>().ToValue(LookupComponent<TweenConfig>());
                binder.Bind<IColorConfig>().ToValue(LookupComponent<ColorConfig>());
                binder.Bind<FocusManager>().ToValue(LookupComponent<FocusManager>());
            }

            // manager monobehaviours
            {
                binder.Bind<IElementManager>().ToValue(LookupComponent<ElementManager>());
                binder.Bind<IPrimitiveFactory>().ToValue(LookupComponent<PrimitiveFactory>());
                binder.Bind<IIntentionManager>().ToValue(LookupComponent<IntentionManager>());
                binder.Bind<IInteractionManager>().ToValue(LookupComponent<InteractionManager>());
                binder.Bind<ISceneManager>().ToValue(LookupComponent<SceneManager>());
                binder.Bind<ILayerManager>().ToValue(LookupComponent<LayerManager>());
                binder.Bind<MusicManager>().ToValue(LookupComponent<MusicManager>());
            }

            // hierarchy
            {
                binder.Bind<ContentGraph>().To<ContentGraph>().ToSingleton();
                binder.Bind<HierarchyManager>().To<HierarchyManager>().ToSingleton();
            }

            // scripting
            {
                binder.Bind<JavaScriptParser>().ToValue(new JavaScriptParser(false));
                binder.Bind<IScriptParser>().To<DefaultScriptParser>().ToSingleton();
                binder.Bind<IScriptLoader>().To<ScriptLoader>().ToSingleton();
                binder.Bind<IScriptRequireResolver>().ToValue(new SpireScriptRequireResolver(binder));
                binder.Bind<IScriptManager>().To<ScriptManager>().ToSingleton();

                // scripting interfaces
                {
                    binder.Bind<AppDataScriptingInterface>().To<AppDataScriptingInterface>().ToSingleton();
                    binder.Bind<MessageRouterScriptingInterface>().To<MessageRouterScriptingInterface>().ToSingleton();
                    binder.Bind<WidgetsScriptingInterface>().To<WidgetsScriptingInterface>().ToSingleton();
                }
            }

            // misc
            {
                binder.Bind<IQueryResolver>().To<StandardQueryResolver>();
            }

            // dependant on previous bindings
            {
                binder.Bind<IAdminAppDataManager>().To<AppDataManager>().ToSingleton();

                var appData = binder.GetInstance<IAdminAppDataManager>();
                binder.Bind<IAppDataManager>().ToValue(appData);
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