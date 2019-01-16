using System;
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Commons.Unity.Storage;
using CreateAR.Commons.Vine;
using CreateAR.EnkluPlayer.AR;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.BLE;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Qr;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.States.HoloLogin;
using CreateAR.EnkluPlayer.Util;
using CreateAR.EnkluPlayer.Vine;
using CreateAR.Trellis.Messages;
using Jint.Parser;
using Newtonsoft.Json;
using strange.extensions.injector.impl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Bindings for EnkluPlayer.
    /// </summary>
    public class EnkluPlayerModule : IInjectionModule
    {
        /// <inheritdoc cref="IInjectionModule"/>
        public void Load(InjectionBinder binder)
        {
            // bind tagged components first-- these already exist
            {
                binder.Bind<MainCamera>().ToValue(LookupComponent<MainCamera>());
                binder.Bind<InputConfig>().ToValue(LookupComponent<InputConfig>());
                binder.Bind<IBootstrapper>().ToValue(LookupComponent<MonoBehaviourBootstrapper>());
                binder.Bind<WebBridge>().ToValue(LookupComponent<WebBridge>());
                binder.Bind<GridRenderer>().ToValue(LookupComponent<GridRenderer>());
                binder.Bind<IVineTable>().To(LookupComponent<VineTable>());
                binder.Bind<MeshCaptureConfig>().To(LookupComponent<MeshCaptureConfig>());
                binder.Bind<DrawingJsApi>().To(LookupComponent<DrawingJsApi>());
            }

            // required for loggly
            binder.Bind<IMessageRouter>().To<MessageRouter>().ToSingleton();

            // load configuration
            var config = ApplicationConfigCompositor.Config;
            binder.Bind<ApplicationConfig>().ToValue(config);
            binder.Bind<NetworkConfig>().ToValue(config.Network);

            // setup logging
            SetupLogging(binder, config.Log);

            // log out the config ASAP
            Log.Info(this, "ApplicationConfig:\n{0}", JsonConvert.SerializeObject(config, Formatting.Indented));

            // misc dependencies
            {
                binder.Bind<ISerializer>().To<JsonSerializer>();
                binder.Bind<JsonSerializer>().To<JsonSerializer>();
                binder.Bind<UrlFormatterCollection>().To<UrlFormatterCollection>().ToSingleton();

                // parser is async on platforms with threads
#if UNITY_WEBGL
                binder.Bind<IParserWorker>().To<SyncParserWorker>().ToSingleton();
#else
                binder.Bind<IParserWorker>().To<ThreadedParserWorker>().ToSingleton();
#endif

                // start worker
                var worker = binder.GetInstance<IParserWorker>();
#if NETFX_CORE
                Windows.System.Threading.ThreadPool.RunAsync(_ => worker.Start());
#elif UNITY_WEBGL
                worker.Start();
#else
                System.Threading.ThreadPool.QueueUserWorkItem(_ => worker.Start());
#endif

                // metrics
#if NETFX_CORE
                binder.Bind<IHostedGraphiteMetricsTarget>().To<UwpHostedGraphiteMetricsTarget>().ToSingleton();
#else
                binder.Bind<IHostedGraphiteMetricsTarget>().To<FrameworkHostedGraphiteMetricsTarget>().ToSingleton();
#endif
                binder.Bind<IMetricsService>().To<MetricsService>().ToSingleton();

                if (config.Network.Offline)
                {
                    Log.Info(this, "Using OfflineHttpService.");
                    binder.Bind<IHttpService>().To<OfflineHttpService>().ToSingleton();
                }
                else
                {
                    binder.Bind<IHttpService>()
                        .To(new HttpService(
                            binder.GetInstance<JsonSerializer>(),
                            LookupComponent<MonoBehaviourBootstrapper>(),
                            binder.GetInstance<UrlFormatterCollection>()))
                        .ToSingleton();
                }

                binder.Bind<HttpRequestCacher>().To<HttpRequestCacher>().ToSingleton();
                binder.Bind<AwsPingController>().To<AwsPingController>().ToSingleton();
                binder.Bind<ApiController>().To<ApiController>().ToSingleton();

#if !UNITY_EDITOR && UNITY_WSA
                binder.Bind<IHashProvider>().To<ShaUwpHashProvider>();
#else
                binder.Bind<IHashProvider>().To<Sha256HashProvider>();
#endif

                // assets
                {
                    binder.Bind<IScanImporter>().To<ScanImporter>().ToSingleton();
                    binder.Bind<IScanLoader>().To<StandardScanLoader>().ToSingleton();
                    binder.Bind<IAssetLoader>().To<StandardAssetLoader>().ToSingleton();
                    binder.Bind<IAssetManager>().To<AssetManager>().ToSingleton();
                    binder.Bind<IAssetPoolManager>().To<LazyAssetPoolManager>().ToSingleton();
                }
                
                binder.Bind<IFileManager>().To<FileManager>().ToSingleton();
                binder.Bind<IImageLoader>().To<StandardImageLoader>().ToSingleton();

                if (config.ParsedPlatform == RuntimePlatform.WebGLPlayer)
                {
                    binder.Bind<IGizmoManager>().To(LookupComponent<GizmoManager>());
                }
                else
                {
                    binder.Bind<IGizmoManager>().To<PassthroughGizmoManager>().ToSingleton();
                }
                
                binder.Bind<IElementFactory>().To<ElementFactory>().ToSingleton();
                binder.Bind<IVinePreProcessor>().To<JsVinePreProcessor>().ToSingleton();
                binder.Bind<VineLoader>().To<VineLoader>().ToSingleton();
                binder.Bind<VineImporter>().To<VineImporter>().ToSingleton();

                binder.Bind<IStorageWorker>().To<StorageWorker>().ToSingleton();
                binder.Bind<IStorageService>().To<StorageService>().ToSingleton();
                binder.Bind<IElementActionStrategyFactory>().To<ElementActionStrategyFactory>();
                
                binder.Bind<IElementTxnTransport>().To<HttpElementTxnTransport>();
                binder.Bind<IAppTxnAuthenticator>().To<HttpAppTxnAuthenticator>();
                binder.Bind<IElementTxnStoreFactory>().To<ElementTxnStoreFactory>();
                binder.Bind<IAppSceneManager>().To<AppSceneManager>().ToSingleton();
                binder.Bind<IAppDataLoader>().To<AppDataLoader>().ToSingleton();


                binder.Bind<IElementTxnManager>().To<ElementTxnManager>().ToSingleton();

                // input
                {
                    binder.Bind<IState>().To<EditModeInputState>().ToName(NamedInjections.INPUT_STATE_DEFAULT);
                    binder.Bind<IInputManager>().To<InputManager>().ToSingleton();
                    binder.Bind<IMultiInput>().To<MultiInput>().ToSingleton();
                }

                // tweening
                {
                    binder.Bind<ITweenManager>().To<TweenManager>().ToSingleton();
                }
            }

            // application
            {
                binder.Bind<MessageTypeBinder>().To<MessageTypeBinder>().ToSingleton();
                binder.Bind<MessageFilter>().To<MessageFilter>().ToSingleton();
                binder.Bind<ConnectionMessageHandler>().To<ConnectionMessageHandler>().ToSingleton();
                binder.Bind<BridgeMessageHandler>().To<BridgeMessageHandler>().ToSingleton();
                
                if (config.Network.Offline)
                {
                    binder.Bind<IConnection>().To<OfflineConnection>().ToSingleton();
                    binder.Bind<IBridge>().To<OfflineBridge>().ToSingleton();
                }
                else
                {
#if UNITY_IOS || UNITY_ANDROID
                    binder.Bind<IConnection>().To<WebSocketSharpConnection>().ToSingleton();
                    binder.Bind<IBridge>().To<WebSocketBridge>().ToSingleton();
#elif UNITY_WEBGL
                    binder.Bind<IConnection>().To<PassthroughConnection>().ToSingleton();
#if UNITY_EDITOR
                        binder.Bind<IBridge>().To<WebSocketBridge>().ToSingleton();
#else
                        binder.Bind<IBridge>().To(LookupComponent<WebBridge>());
#endif
#elif UNITY_EDITOR
                    binder.Bind<IBridge>().To<WebSocketBridge>().ToSingleton();
                    binder.Bind<IConnection>().To<WebSocketSharpConnection>().ToSingleton();
#elif NETFX_CORE
                    binder.Bind<IConnection>().To<UwpConnection>().ToSingleton();
                    binder.Bind<IBridge>().To<OfflineBridge>().ToSingleton();
#endif
                }

                // services
                {
                    binder.Bind<ApplicationStateService>().To<ApplicationStateService>().ToSingleton();
                    binder.Bind<EnvironmentUpdateService>().To<EnvironmentUpdateService>().ToSingleton();
                    binder.Bind<AssetUpdateService>().To<AssetUpdateService>().ToSingleton();
                    binder.Bind<ScriptUpdateService>().To<ScriptUpdateService>().ToSingleton();
                    binder.Bind<ShaderUpdateService>().To<ShaderUpdateService>().ToSingleton();
                    binder.Bind<SceneUpdateService>().To<SceneUpdateService>().ToSingleton();
                    binder.Bind<ElementActionHelperService>().To<ElementActionHelperService>().ToSingleton();
                    binder.Bind<UserPreferenceService>().To<UserPreferenceService>().ToSingleton();
                    binder.Bind<VersioningService>().To<VersioningService>().ToSingleton();
                    binder.Bind<MetricsUpdateService>().To<MetricsUpdateService>().ToSingleton();

#if NETFX_CORE
                    binder.Bind<CommandService>().To<UwpCommandService>().ToSingleton();
#else
                    binder.Bind<CommandService>().To<CommandService>().ToSingleton();
#endif

#if NETFX_CORE
                    binder.Bind<IDeviceMetaProvider>().To<NetCoreDeviceMetaProvider>().ToSingleton();
#else
                    binder.Bind<IDeviceMetaProvider>().To<FrameworkDeviceMetaProvider>().ToSingleton();
#endif

                    binder.Bind<DeviceResourceUpdateService>().To<DeviceResourceUpdateService>().ToSingleton();
                }

                // player specific bindings
                AddPlayerBindings(config, binder);

                // login
                {
                    if (config.ParsedPlatform == RuntimePlatform.WSAPlayerX86 && UnityEngine.Application.isEditor)
                    {
                        binder.Bind<ILoginStrategy>().To<EditorLoginStrategy>();
                    }
                    else if (config.ParsedPlatform == RuntimePlatform.WSAPlayerX86
                       || config.ParsedPlatform == RuntimePlatform.WSAPlayerARM
                       || config.ParsedPlatform == RuntimePlatform.WSAPlayerX64)
                    {
                        binder.Bind<ILoginStrategy>().To<QrLoginStrategy>();
                    }
                    else
                    {
                        binder.Bind<ILoginStrategy>().To<MobileLoginStrategy>();
                    }
                }

                // application states
                {
                    // flows
                    {
                        binder.Bind<HmdStateFlow>().To<HmdStateFlow>();
                        binder.Bind<MobileLoginStateFlow>().To<MobileLoginStateFlow>();
                        binder.Bind<MobileGuestStateFlow>().To<MobileGuestStateFlow>();
                        binder.Bind<WebStateFlow>().To<WebStateFlow>();
                    }

                    // all states
                    {
                        binder.Bind<VersionErrorApplicationState>().To<VersionErrorApplicationState>();
                        binder.Bind<InitializeApplicationState>().To<InitializeApplicationState>();
                        binder.Bind<LoginApplicationState>().To<LoginApplicationState>();
                        binder.Bind<HoloLoginApplicationState>().To<HoloLoginApplicationState>();
                        binder.Bind<SignOutApplicationState>().To<SignOutApplicationState>();
                        binder.Bind<QrLoginStrategy>().To<QrLoginStrategy>();
                        binder.Bind<GuestApplicationState>().To<GuestApplicationState>();
                        binder.Bind<DeviceRegistrationApplicationState>().To<DeviceRegistrationApplicationState>();
                        binder.Bind<OrientationApplicationState>().To<OrientationApplicationState>();
                        binder.Bind<UserProfileApplicationState>().To<UserProfileApplicationState>();
                        binder.Bind<MobileArSetupApplicationState>().To<MobileArSetupApplicationState>();
                        binder.Bind<HmdArSetupApplicationState>().To<HmdArSetupApplicationState>();
                        binder.Bind<MobileLoginStrategy>().To<MobileLoginStrategy>();
                        binder.Bind<EditorLoginStrategy>().To<EditorLoginStrategy>();
                        binder.Bind<LoadAppApplicationState>().To<LoadAppApplicationState>();
                        binder.Bind<LoadDefaultAppApplicationState>().To<LoadDefaultAppApplicationState>();
                        binder.Bind<ReceiveAppApplicationState>().To<ReceiveAppApplicationState>();
                        binder.Bind<PlayApplicationState>().To<PlayApplicationState>();
                        binder.Bind<BleSearchApplicationState>().To<BleSearchApplicationState>();
                        binder.Bind<IuxDesignerApplicationState>().To<IuxDesignerApplicationState>();
                        
                        // tools
                        {
                            binder.Bind<ToolModeApplicationState>().To<ToolModeApplicationState>();
#if !UNITY_EDITOR && UNITY_WSA
                            binder.Bind<IMeshCaptureService>().To<HoloLensMeshCaptureService>().ToSingleton();
#else
                            binder.Bind<IMeshCaptureService>().To<MockMeshCaptureService>().ToSingleton();
#endif
                            binder.Bind<MeshCaptureExportServiceConfiguration>().ToValue(new MeshCaptureExportServiceConfiguration());
                            binder.Bind<IMeshCaptureExportService>().To<MeshCaptureExportService>().ToSingleton();
                            binder.Bind<BugReportApplicationState>().To<BugReportApplicationState>();
                        }
                    }

                    // packages
                    {
                        switch (config.ParsedPlatform)
                        {
                            case RuntimePlatform.Android:
                            case RuntimePlatform.IPhonePlayer:
                            {
                                binder.Bind<ApplicationStatePackage>().To(new ApplicationStatePackage(
                                    new IState[]
                                    {
                                        binder.GetInstance<VersionErrorApplicationState>(),
                                        binder.GetInstance<InitializeApplicationState>(),
                                        binder.GetInstance<LoginApplicationState>(),
                                        binder.GetInstance<HoloLoginApplicationState>(),
                                        binder.GetInstance<SignOutApplicationState>(),
                                        binder.GetInstance<GuestApplicationState>(),
                                        binder.GetInstance<MobileArSetupApplicationState>(),
                                        binder.GetInstance<UserProfileApplicationState>(),
                                        binder.GetInstance<LoadAppApplicationState>(),
                                        binder.GetInstance<LoadDefaultAppApplicationState>(),
                                        binder.GetInstance<PlayApplicationState>()
                                    },
                                    new IStateFlow[]
                                    {
                                        binder.GetInstance<MobileLoginStateFlow>(),
                                        binder.GetInstance<MobileGuestStateFlow>()
                                    }));
                                break;
                            }
                            case RuntimePlatform.WSAPlayerX86:
                            case RuntimePlatform.WSAPlayerX64:
                            case RuntimePlatform.WSAPlayerARM:
                            {
                                binder.Bind<ApplicationStatePackage>().To(new ApplicationStatePackage(
                                    new IState[]
                                    {
                                        binder.GetInstance<VersionErrorApplicationState>(),
                                        binder.GetInstance<InitializeApplicationState>(),
                                        binder.GetInstance<LoginApplicationState>(),
                                        binder.GetInstance<SignOutApplicationState>(),
                                        binder.GetInstance<GuestApplicationState>(),
                                        binder.GetInstance<DeviceRegistrationApplicationState>(),
                                        binder.GetInstance<OrientationApplicationState>(),
                                        binder.GetInstance<UserProfileApplicationState>(),
                                        binder.GetInstance<LoadAppApplicationState>(),
                                        binder.GetInstance<LoadDefaultAppApplicationState>(),
                                        binder.GetInstance<PlayApplicationState>(),
                                        binder.GetInstance<BleSearchApplicationState>(),
                                        binder.GetInstance<BugReportApplicationState>(),
                                        binder.GetInstance<ToolModeApplicationState>(),
                                        binder.GetInstance<IuxDesignerApplicationState>()
                                    },
                                    new IStateFlow[]
                                    {
                                        binder.GetInstance<HmdStateFlow>()
                                    }));
                                    
                                    break;
                            }
                            default:
                            {
                                binder.Bind<ApplicationStatePackage>().To(new ApplicationStatePackage(
                                    new IState[]
                                    {
                                        binder.GetInstance<InitializeApplicationState>(),
                                        binder.GetInstance<ReceiveAppApplicationState>(),
                                        binder.GetInstance<PlayApplicationState>()
                                    },
                                    new IStateFlow[]
                                    {
                                        binder.GetInstance<WebStateFlow>()
                                    }));
                                    
                                    break;
                            }
                        }
                    }
                }
                
                // service manager + application
                binder.Bind<IApplicationServiceManager>().ToValue(new ApplicationServiceManager(
                    binder.GetInstance<IMessageRouter>(),
                    binder.GetInstance<IElementTxnManager>(),
                    binder.GetInstance<MessageFilter>(),
                    new ApplicationService[]
                    {
                        // Order is important.
                        binder.GetInstance<EnvironmentUpdateService>(),
                        binder.GetInstance<VersioningService>(),
                        binder.GetInstance<AssetUpdateService>(),
                        binder.GetInstance<ScriptUpdateService>(),
                        binder.GetInstance<ShaderUpdateService>(),
                        binder.GetInstance<SceneUpdateService>(),
                        binder.GetInstance<ElementActionHelperService>(),
                        binder.GetInstance<UserPreferenceService>(),
                        binder.GetInstance<DeviceResourceUpdateService>(),
                        binder.GetInstance<ApplicationStateService>(),
                        binder.GetInstance<CommandService>(),
                        binder.GetInstance<MetricsUpdateService>()
                    }));
                binder.Bind<Application>().To<Application>().ToSingleton();
            }
        }

        /// <summary>
        /// Sets up log targets.
        /// </summary>
        private void SetupLogging(InjectionBinder binder, LogAppConfig config)
        {
            // forward logs to unity
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter
            {
                Level = false,
                Timestamp = false,
                TypeName = true
            })
            {
                Filter = config.ParsedLevel
            });

            // non-webgl should log to file
#if !UNITY_WEBGL
            Log.AddLogTarget(new FileLogTarget(
                new DefaultLogFormatter
                {
                    Level = true,
                    Timestamp = true,
                    TypeName = true
                },
                System.IO.Path.Combine(
                    UnityEngine.Application.persistentDataPath,
                    "Application.log"))
            {
                // warning and up
                Filter = LogLevel.Debug
            });
#endif // UNITY_WEBGL

            // non-editor builds should log to loggly
            if (!UnityEngine.Application.isEditor)
            {
                binder.Bind<ILogglyMetadataProvider>().To<LogglyMetadataProvider>().ToSingleton();

                Log.AddLogTarget(new LogglyLogTarget(
                    // TODO: move to config
                    "1f0810f5-db28-4ea3-aeea-ec83d8cb3c0f",
                    "EnkluPlayer",
                    binder.GetInstance<ILogglyMetadataProvider>(),
                    binder.GetInstance<IBootstrapper>())
                {
                    Filter = LogLevel.Error
                });
            }
        }

        /// <summary>
        /// Adds bindings for player.
        /// </summary>
        private void AddPlayerBindings(
            ApplicationConfig config,
            InjectionBinder binder)
        {
            binder.Bind<IAppQualityController>().To<AppQualityController>().ToSingleton();

            // AR
            {
                binder.Bind<ArCameraRig>().ToValue(LookupComponent<ArCameraRig>());
                binder.Bind<ArServiceConfiguration>().ToValue(LookupComponent<ArServiceConfiguration>());
                binder.Bind<IPrimaryAnchorManager>().To<PrimaryAnchorManager>().ToSingleton();

#if !UNITY_EDITOR && UNITY_IOS
                binder.Bind<UnityEngine.XR.iOS.UnityARSessionNativeInterface>().ToValue(UnityEngine.XR.iOS.UnityARSessionNativeInterface.GetARSessionNativeInterface());
                binder.Bind<IArService>().To<IosArService>().ToSingleton();
                binder.Bind<IWorldAnchorProvider>().To<PassthroughWorldAnchorProvider>().ToSingleton();
#elif !UNITY_EDITOR && UNITY_ANDROID
                binder.Bind<IArService>().To<AndroidArService>().ToSingleton();
                binder.Bind<IWorldAnchorProvider>().To<PassthroughWorldAnchorProvider>().ToSingleton();
#elif !UNITY_EDITOR && UNITY_WSA
                binder.Bind<IArService>().To<HoloLensArService>().ToSingleton();
                binder.Bind<IWorldAnchorProvider>().To<HoloLensWorldAnchorProvider>().ToSingleton();
#else
                binder.Bind<IWorldAnchorProvider>().To<PassthroughWorldAnchorProvider>().ToSingleton();
                binder.Bind<IArService>().To<PassthroughArService>().ToSingleton();
#endif

                binder.Bind<IWorldAnchorCache>().To<StandardWorldAnchorCache>().ToSingleton();
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

            // Gesture
            {
#if NETFX_CORE
                binder.Bind<IGestureManager>().To<HoloLensGestureManager>().ToSingleton();          
#else
                binder.Bind<IGestureManager>().To<PassthroughGestureManager>().ToSingleton();
#endif
                binder.Bind<ITouchManager>().To<TouchManager>().ToSingleton();
            }

            // Camera
            {
                binder.Bind<IMediaCapture>().To<MediaCapture>().ToSingleton();
            }

            // UI
            {
                binder.Bind<IUIManager>().To<UIManager>().ToSingleton();
                binder.Bind<IUIElementFactory>().To(LookupComponent<SceneUIElementFactory>());
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
                    binder.Bind<NewAnchorDesignState>().To<NewAnchorDesignState>();
                    binder.Bind<NewContainerDesignState>().To<NewContainerDesignState>();
                    binder.Bind<NewContentDesignState>().To<NewContentDesignState>();
                    binder.Bind<EditElementDesignState>().To<EditElementDesignState>();
                    binder.Bind<ReparentDesignState>().To<ReparentDesignState>();
                    binder.Bind<EditAnchorDesignState>().To<EditAnchorDesignState>();
                    binder.Bind<EditPrimaryAnchorDesignState>().To<EditPrimaryAnchorDesignState>();
                    binder.Bind<AppListViewDesignState>().To<AppListViewDesignState>();
                    binder.Bind<CreateNewAppDesignState>().To<CreateNewAppDesignState>();
                }

                binder.Bind<IElementUpdateDelegate>().To<ElementUpdateDelegate>().ToSingleton();

                // designer
                var designer = config.Play.ParsedDesigner;
                if (designer == PlayAppConfig.DesignerType.Invalid)
                {
                    switch (config.ParsedPlatform)
                    {
                        case RuntimePlatform.IPhonePlayer:
                        {
                            designer = PlayAppConfig.DesignerType.Mobile;
                            break;
                        }
                        case RuntimePlatform.WebGLPlayer:
                        {
                            designer = PlayAppConfig.DesignerType.Desktop;
                            break;
                        }
                        case RuntimePlatform.WSAPlayerX86:
                        {
                            designer = PlayAppConfig.DesignerType.Ar;
                            break;
                        }
                        default:
                        {
                            designer = PlayAppConfig.DesignerType.None;
                            break;
                        }
                    }
                }

                switch (designer)
                {
                    case PlayAppConfig.DesignerType.Desktop:
                    {
                        binder.Bind<ScanImporter>().To<ScanImporter>().ToSingleton();
                        binder.Bind<IDesignController>().To<DesktopDesignController>().ToSingleton();
                        break;
                    }
                    case PlayAppConfig.DesignerType.Ar:
                    {
                        binder.Bind<IDesignController>().To<HmdDesignController>().ToSingleton();
                        break;
                    }
                    case PlayAppConfig.DesignerType.Mobile:
                    {
                        binder.Bind<IDesignController>().To<MobileDesignController>().ToSingleton();
                        break;
                    }
                    case PlayAppConfig.DesignerType.None:
                    {
                        binder.Bind<IDesignController>().To<PassthroughDesignController>().ToSingleton();
                        break;
                    }
                    case PlayAppConfig.DesignerType.Invalid:
                    {
                        throw new Exception("Invalid designer.");
                    }
                }
            }

            // scripting
            {
                binder.Bind<JavaScriptParser>().ToValue(new JavaScriptParser(false));
                binder.Bind<IScriptParser>().To<DefaultScriptParser>().ToSingleton();

                if (UnityEngine.Application.isEditor)
                {
                    binder.Bind<IScriptCache>().To<PassthroughScriptCache>().ToSingleton();
                }
                else
                {
#if UNITY_WEBGL
                    binder.Bind<IScriptCache>().To<PassthroughScriptCache>().ToSingleton();
#else
                    binder.Bind<IScriptCache>().To<DelayedScriptCache>().ToSingleton();
#endif
                }
                
                binder.Bind<IScriptLoader>().To<StandardScriptLoader>().ToSingleton();
                binder.Bind<IScriptRequireResolver>().ToValue(new EnkluScriptRequireResolver(binder));
                binder.Bind<IElementJsCache>().To<ElementJsCache>().ToSingleton();
                binder.Bind<IElementJsFactory>().To<ElementJsFactory>().ToSingleton();
                binder.Bind<IScriptManager>().To<ScriptManager>().ToSingleton();
                binder.Bind<PlayerJs>().ToValue(LookupComponent<PlayerJs>());

                // scripting interfaces
                {
                    binder.Bind<MessageRouterScriptingInterface>().To<MessageRouterScriptingInterface>().ToSingleton();
                    binder.Bind<ProximityManager>().ToValue(LookupComponent<ProximityManager>());
                    binder.Bind<GestureJsInterface>().To<GestureJsInterface>().ToSingleton();
                    binder.Bind<JsMessageRouter>().To<JsMessageRouter>().ToSingleton();
                    binder.Bind<MessagingJsInterface>().To<MessagingJsInterface>().ToSingleton();
                    binder.Bind<TimerJsInterface>().To<TimerJsInterface>().ToSingleton();
                    binder.Bind<SnapJsInterface>().To<SnapJsInterface>().ToSingleton();
                    binder.Bind<MetricsJsInterface>().To<MetricsJsInterface>().ToSingleton();
                    binder.Bind<PhysicsJsInterface>().To<PhysicsJsInterface>().ToSingleton();
                    binder.Bind<TweenManagerJsApi>().To<TweenManagerJsApi>().ToSingleton();
                    binder.Bind<EditJsApi>().To<EditJsApi>().ToSingleton();
                    binder.Bind<TxnJsApi>().To<TxnJsApi>().ToSingleton();
                    binder.Bind<TouchManagerJsApi>().To<TouchManagerJsApi>().ToSingleton();
                }
            }

            // misc
            {
                binder.Bind<IQueryResolver>().To<StandardQueryResolver>();
            }

            // dependent on previous bindings
            {
                binder.Bind<IAdminAppDataManager>().To<AppDataManager>().ToSingleton();

                var appData = binder.GetInstance<IAdminAppDataManager>();
                binder.Bind<IAppDataManager>().ToValue(appData);
                
                SystemJsApi.Initialize(
                    binder.GetInstance<IDeviceMetaProvider>(),
                    binder.GetInstance<IMediaCapture>(),
                    binder.GetInstance<AwsPingController>(),
                    binder.GetInstance<IMessageRouter>(),
                    binder.GetInstance<ApiController>(),
                    binder.GetInstance<ApplicationConfig>());
            }

            binder.Bind<IAppController>().To<AppController>().ToSingleton();
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