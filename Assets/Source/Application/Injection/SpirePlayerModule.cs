using System;
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Commons.Unity.Storage;
using CreateAR.Commons.Vine;
using CreateAR.SpirePlayer.AR;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.BLE;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using CreateAR.Trellis.Messages;
using Jint.Parser;
using Jint.Unity;
using strange.extensions.injector.impl;
using UnityEngine;
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
            // main configuration
            var configAsset = Resources.Load<TextAsset>("ApplicationConfig");
            Log.Info(this, "ApplicationConfig Source:\n{0}", configAsset.text);

            var serializer = new JsonSerializer();
            var bytes = Encoding.UTF8.GetBytes(configAsset.text);
            object app;
            serializer.Deserialize(typeof(ApplicationConfig), ref bytes, out app);

            var config = (ApplicationConfig) app;
            Log.Info(this, "ApplicationConfig:\n{0}", config);

            binder.Bind<ApplicationConfig>().ToValue(config);

            // misc dependencies
            {
                binder.Bind<ISerializer>().To<JsonSerializer>();
                binder.Bind<JsonSerializer>().To<JsonSerializer>();
                binder.Bind<BridgeMessageHandler>().To<BridgeMessageHandler>().ToSingleton();
                binder.Bind<IMessageRouter>().To<MessageRouter>().ToSingleton();
                binder.Bind<IHttpService>()
                    .To(new HttpService(
                        new JsonSerializer(),
                        LookupComponent<MonoBehaviourBootstrapper>()))
                    .ToSingleton();
                binder.Bind<ApiController>().To<ApiController>().ToSingleton();

#if !UNITY_EDITOR && UNITY_WSA
                binder.Bind<IHashProvider>().To<ShaUwpHashProvider>();
#else
                binder.Bind<IHashProvider>().To<Sha256HashProvider>();
#endif
                binder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
                binder.Bind<IAssetPoolManager>().To<LazyAssetPoolManager>().ToSingleton();
                binder.Bind<IFileManager>().To<FileManager>().ToSingleton();
                binder.Bind<IImageLoader>().To<StandardImageLoader>().ToSingleton();
                binder.Bind<IElementFactory>().To<ElementFactory>().ToSingleton();
                binder.Bind<IVinePreProcessor>().To<JsVinePreProcessor>().ToSingleton();
                binder.Bind<IVineTable>().To(LookupComponent<VineTable>());
                binder.Bind<VineLoader>().To<VineLoader>().ToSingleton();
                binder.Bind<VineImporter>().To<VineImporter>().ToSingleton();

                binder.Bind<IStorageWorker>().To<StorageWorker>().ToSingleton();
                binder.Bind<IStorageService>().To<StorageService>().ToSingleton();

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
                    binder.Bind<ILoadProgressManager>().ToValue(LookupComponent<LoadProgressManager>());
                }
            }

            // application
            {
#if UNITY_EDITOR || UNITY_IOS
                    binder.Bind<IBridge>().To<WebSocketBridge>().ToSingleton();
#elif UNITY_WEBGL
                    binder.Bind<IBridge>().ToValue(LookupComponent<WebBridge>());
#elif NETFX_CORE
                    binder.Bind<IBridge>().To<UwpBridge>().ToSingleton();
#endif
                
                // spire-specific bindings
                AddSpireBindings(binder);

                // services
                {
                    binder.Bind<ApplicationStateService>().To<ApplicationStateService>().ToSingleton();
                    binder.Bind<AuthorizationService>().To<AuthorizationService>().ToSingleton();
                    binder.Bind<AssetUpdateService>().To<AssetUpdateService>().ToSingleton();
                    binder.Bind<HierarchyUpdateService>().To<HierarchyUpdateService>().ToSingleton();
                    binder.Bind<ContentUpdateService>().To<ContentUpdateService>().ToSingleton();
                    binder.Bind<ScriptUpdateService>().To<ScriptUpdateService>().ToSingleton();
                    binder.Bind<MaterialUpdateService>().To<MaterialUpdateService>().ToSingleton();
                    binder.Bind<ShaderUpdateService>().To<ShaderUpdateService>().ToSingleton();
                }

                // application states
                {
                    binder.Bind<TestDataConfig>().To(LookupComponent<TestDataConfig>());
                    binder.Bind<ITestDataController>().To<TestDataController>();
                    binder.Bind<InitializeApplicationState>().To<InitializeApplicationState>();
                    binder.Bind<WaitingForConnectionApplicationState>().To<WaitingForConnectionApplicationState>();
                    binder.Bind<EditApplicationState>().To<EditApplicationState>();
                    binder.Bind<PreviewApplicationState>().To<PreviewApplicationState>();
                    binder.Bind<PlayApplicationState>().To<PlayApplicationState>();
                    binder.Bind<HierarchyApplicationState>().To<HierarchyApplicationState>();
                    binder.Bind<BleSearchApplicationState>().To<BleSearchApplicationState>();
                    binder.Bind<InstaApplicationState>().To<InstaApplicationState>();

                    // tools
                    {
                        binder.Bind<ToolModeApplicationState>().To<ToolModeApplicationState>();
                        binder.Bind<WorldScanPipelineConfiguration>().ToValue(new WorldScanPipelineConfiguration
                        {

                        });
                        binder.Bind<WorldScanPipeline>().To<WorldScanPipeline>().ToSingleton();
#if NETFX_CORE
                        binder.Bind<MeshCaptureApplicationState>().To<MeshCaptureApplicationState>();
#endif
                    }
                }

                // service manager + appplication
                binder.Bind<IApplicationServiceManager>().ToValue(new ApplicationServiceManager(
                    binder.GetInstance<IBridge>(),
                    new ApplicationService[]
                    {
                        binder.GetInstance<ApplicationStateService>(),
                        binder.GetInstance<AuthorizationService>(),
                        binder.GetInstance<AssetUpdateService>(),
                        binder.GetInstance<HierarchyUpdateService>(),
                        binder.GetInstance<ContentUpdateService>(),
                        binder.GetInstance<ScriptUpdateService>(),
                        binder.GetInstance<MaterialUpdateService>(),
                        binder.GetInstance<ShaderUpdateService>()
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

            // AR
            {
                binder.Bind<ArCameraRig>().ToValue(LookupComponent<ArCameraRig>());
                binder.Bind<ArServiceConfiguration>().ToValue(LookupComponent<ArServiceConfiguration>());
#if !UNITY_EDITOR && UNITY_IOS
                binder.Bind<UnityEngine.XR.iOS.UnityARSessionNativeInterface>().ToValue(UnityEngine.XR.iOS.UnityARSessionNativeInterface.GetARSessionNativeInterface());
                binder.Bind<IArService>().To<IosArService>().ToSingleton();
#elif !UNITY_EDITOR && UNITY_WSA
                binder.Bind<IArService>().To<HoloLensArService>().ToSingleton();
#else
                binder.Bind<IArService>().To<EditorArService>().ToSingleton();
#endif
            }

            // BLE
            {
                binder.Bind<BleServiceConfiguration>().ToValue(LookupComponent<BleServiceConfiguration>());
#if NETFX_CORE
                binder.Bind<IBleService>().To<UwpBleService>().ToSingleton();
#else
                binder.Bind<IBleService>().To<NullBleService>().ToSingleton();
#endif
            }

            // Voice
            {
#if NETFX_CORE
                binder.Bind<IVoiceCommandManager>().To<VoiceCommandManager>().ToSingleton();
#else
                binder.Bind<IVoiceCommandManager>().To<PassthroughVoiceCommandManager>().ToSingleton();
#endif
            }

            // IUX
            {
                binder.Bind<IPrimitiveFactory>().To<PrimitiveFactory>().ToSingleton();

                // content
                {
                    binder.Bind<IContentManager>().To<ContentManager>().ToSingleton();
                    binder.Bind<IContentFactory>().To<ContentFactory>();
                    binder.Bind<IAnchorReferenceFrameFactory>().To<AnchorReferenceFrameFactory>();
                }

                // configs
                {
                    binder.Bind<WidgetConfig>().ToValue(LookupComponent<WidgetConfig>());
                    binder.Bind<TweenConfig>().ToValue(LookupComponent<TweenConfig>());
                    binder.Bind<ColorConfig>().ToValue(LookupComponent<ColorConfig>());
                    binder.Bind<IFontConfig>().ToValue(LookupComponent<FontConfig>());
                    binder.Bind<FocusManager>().ToValue(LookupComponent<FocusManager>());
                }

                // manager monobehaviours
                {
                    binder.Bind<IElementManager>().ToValue(LookupComponent<ElementManager>());
                    binder.Bind<IIntentionManager>().ToValue(LookupComponent<IntentionManager>());
                    binder.Bind<IInteractionManager>().ToValue(LookupComponent<InteractionManager>());
                    binder.Bind<ISceneManager>().ToValue(LookupComponent<SceneManager>());
                    binder.Bind<ILayerManager>().ToValue(LookupComponent<LayerManager>());
                }
            }

            // design
            {
                binder.Bind<DesignController>().To<DesignController>().ToSingleton();
                binder.Bind<IPropManager>().To<PropManager>().ToSingleton();
            }

            // hierarchy
            {
                binder.Bind<HierarchyDatabase>().To<HierarchyDatabase>().ToSingleton();
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