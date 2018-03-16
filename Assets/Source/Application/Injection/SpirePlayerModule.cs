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
                binder.Bind<IElementActionStrategyFactory>().To<ElementActionStrategyFactory>();
                binder.Bind<IElementTxnTransport>().To<HttpElementTxnTransport>();
                binder.Bind<IElementTxnStoreFactory>().To<ElementTxnStoreFactory>();
                binder.Bind<IElementTxnManager>().To<ElementTxnManager>().ToSingleton();

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
                    binder.Bind<GridRenderer>().ToValue(LookupComponent<GridRenderer>());
                }
            }

            // application
            {
                binder.Bind<MessageTypeBinder>().To<MessageTypeBinder>().ToSingleton();
                binder.Bind<MessageFilter>().To<MessageFilter>().ToSingleton();
                binder.Bind<ConnectionMessageHandler>().To<ConnectionMessageHandler>().ToSingleton();
                binder.Bind<BridgeMessageHandler>().To<BridgeMessageHandler>().ToSingleton();

#if UNITY_EDITOR || UNITY_IOS
                binder.Bind<IConnection>().To<WebSocketSharpConnection>().ToSingleton();
                binder.Bind<IBridge>().To<WebSocketBridge>().ToSingleton();
#elif UNITY_WEBGL
                binder.Bind<IConnection>().To<PassthroughConnection>().ToSingleton();
                binder.Bind<IBridge>().ToValue(LookupComponent<WebBridge>());
#elif NETFX_CORE
                binder.Bind<IConnection>().To<UwpConnection>().ToSingleton();
                binder.Bind<IBridge>().To<UwpBridge>().ToSingleton();
#endif


                // spire-specific bindings
                AddSpireBindings(config, binder);

                // services
                {
                    binder.Bind<ApplicationStateService>().To<ApplicationStateService>().ToSingleton();
                    binder.Bind<AssetUpdateService>().To<AssetUpdateService>().ToSingleton();
                    binder.Bind<ScriptUpdateService>().To<ScriptUpdateService>().ToSingleton();
                    binder.Bind<MaterialUpdateService>().To<MaterialUpdateService>().ToSingleton();
                    binder.Bind<ShaderUpdateService>().To<ShaderUpdateService>().ToSingleton();
                    binder.Bind<SceneUpdateService>().To<SceneUpdateService>().ToSingleton();
                }

                // application states
                {
                    binder.Bind<TestDataConfig>().To(LookupComponent<TestDataConfig>());
                    binder.Bind<ITestDataController>().To<TestDataController>();
                    binder.Bind<InitializeApplicationState>().To<InitializeApplicationState>();
                    binder.Bind<QrLoginApplicationState>().To<QrLoginApplicationState>();
                    binder.Bind<LoadAppApplicationState>().To<LoadAppApplicationState>();
                    binder.Bind<ReceiveAppApplicationState>().To<ReceiveAppApplicationState>();
                    binder.Bind<PreviewApplicationState>().To<PreviewApplicationState>();
                    binder.Bind<PlayApplicationState>().To<PlayApplicationState>();
                    binder.Bind<BleSearchApplicationState>().To<BleSearchApplicationState>();
                    binder.Bind<InstaApplicationState>().To<InstaApplicationState>();

                    // tools
                    {
                        binder.Bind<ToolModeApplicationState>().To<ToolModeApplicationState>();
                        binder.Bind<WorldScanPipelineConfiguration>().ToValue(new WorldScanPipelineConfiguration());
                        binder.Bind<WorldScanPipeline>().To<WorldScanPipeline>().ToSingleton();
#if NETFX_CORE
                        binder.Bind<MeshCaptureApplicationState>().To<MeshCaptureApplicationState>();
#endif
                    }
                }

                binder.Bind<IAppController>().To<AppController>();
                
                // service manager + application
                binder.Bind<IApplicationServiceManager>().ToValue(new ApplicationServiceManager(
                    binder.GetInstance<IMessageRouter>(),
                    binder.GetInstance<IBridge>(),
                    binder.GetInstance<IElementTxnManager>(),
                    binder.GetInstance<MessageFilter>(),
                    binder.GetInstance<BridgeMessageHandler>(),
                    new ApplicationService[]
                    {
                        binder.GetInstance<ApplicationStateService>(),
                        binder.GetInstance<AssetUpdateService>(),
                        binder.GetInstance<ScriptUpdateService>(),
                        binder.GetInstance<MaterialUpdateService>(),
                        binder.GetInstance<ShaderUpdateService>(),
                        binder.GetInstance<SceneUpdateService>()
                    }));
                binder.Bind<Application>().To<Application>().ToSingleton();
            }
        }

        /// <summary>
        /// Adds bindings for spire.
        /// </summary>
        private void AddSpireBindings(
            ApplicationConfig config,
            InjectionBinder binder)
        {
            // AR
            {
                binder.Bind<ArCameraRig>().ToValue(LookupComponent<ArCameraRig>());
                binder.Bind<ArServiceConfiguration>().ToValue(LookupComponent<ArServiceConfiguration>());

#if !UNITY_EDITOR && UNITY_IOS
                binder.Bind<UnityEngine.XR.iOS.UnityARSessionNativeInterface>().ToValue(UnityEngine.XR.iOS.UnityARSessionNativeInterface.GetARSessionNativeInterface());
                binder.Bind<IArService>().To<IosArService>().ToSingleton();
                binder.Bind<IWorldAnchorProvider>().To<ArKitWorldAnchorProvider>().ToSingleton();
#elif !UNITY_EDITOR && UNITY_WSA
                binder.Bind<IArService>().To<HoloLensArService>().ToSingleton();
                binder.Bind<IWorldAnchorProvider>().To<HoloLensWorldAnchorProvider>().ToSingleton();
#else
                binder.Bind<IWorldAnchorProvider>().To<PassthroughWorldAnchorProvider>().ToSingleton();
                binder.Bind<IArService>().To<PassthroughArService>().ToSingleton();
#endif
            }

            // QR
            {
#if NETFX_CORE
                binder.Bind<IQrReaderService>().To<WsaQrReaderService>().ToSingleton();
#elif UNITY_IOS
                binder.Bind<IQrReaderService>().To<IosQrReaderService>().ToSingleton();
#else
                binder.Bind<IQrReaderService>().To<UnsupportedQrReaderService>().ToSingleton();
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
                    binder.Bind<ILayerManager>().ToValue(LookupComponent<LayerManager>());
                }
            }

            // design
            {
                binder.Bind<IElementControllerManager>().To<ElementControllerManager>().ToSingleton();

                // states
                {
                    binder.Bind<MainDesignState>().To<MainDesignState>();
                    binder.Bind<NewContentDesignState>().To<NewContentDesignState>();
                    binder.Bind<EditContentDesignState>().To<EditContentDesignState>();
                    binder.Bind<ReparentDesignState>().To<ReparentDesignState>();
                    binder.Bind<AnchorDesignState>().To<AnchorDesignState>();
                }

                binder.Bind<IElementUpdateDelegate>().To<ElementUpdateDelegate>().ToSingleton();

                if (UnityEngine.Application.isEditor)
                {
                    if (config.Play.Designer == PlayAppConfig.DesignerType.Desktop)
                    {
                        binder.Bind<IDesignController>().To<DesktopDesignController>().ToSingleton();
                    }
                    else
                    {
                        binder.Bind<IDesignController>().To<ArDesignController>().ToSingleton();
                    }
                }
                else
                {
#if NETFX_CORE || UNITY_IOS || UNITY_ANDROID
                    binder.Bind<IDesignController>().To<ArDesignController>().ToSingleton();
#elif UNITY_WEBGL
                    binder.Bind<IDesignController>().To<DesktopDesignController>().ToSingleton();
#else
                    binder.Bind<IDesignController>().To<DesktopDesignController>().ToSingleton();
#endif
                }
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