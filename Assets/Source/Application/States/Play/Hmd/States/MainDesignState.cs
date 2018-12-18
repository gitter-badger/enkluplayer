using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Entry state of design controller.
    /// </summary>
    public class MainDesignState : IArDesignState
    {
        /// <summary>
        /// Controller group tags.
        /// </summary>
        private const string TAG_CONTENT = "content";
        private const string TAG_CONTAINER = "container";
        private const string TAG_ANCHOR = "anchor";

        /// <summary>
        /// Configuration values.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        ///To register voice commands
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Primary anchor.
        /// </summary>
        private readonly IPrimaryAnchorManager _primaryAnchor;

        /// <summary>
        /// Manages controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;

        /// <summary>
        /// Http interface.
        /// </summary>
        private readonly IHttpService _http;
        
        /// <summary>
        /// Provides anchor import/export.
        /// </summary>
        private readonly IWorldAnchorProvider _anchorProvider;

        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;
        
        /// <summary>
        /// User preferences.
        /// </summary>
        private readonly UserPreferenceService _preferenceService;

        /// <summary>
        /// Manages scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Manages txns.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// Id of splash menu.
        /// </summary>
        private int _splashId;

        /// <summary>
        /// Id of main menu.
        /// </summary>
        private int _mainId;

        /// <summary>
        /// User preferences.
        /// </summary>
        private SynchronizedObject<UserPreferenceData> _prefs;

        /// <summary>
        /// Load for preferences.
        /// </summary>
        private IAsyncToken<SynchronizedObject<UserPreferenceData>> _prefLoad;

        /// <summary>
        /// Reference for the opened mainmenu ui view
        /// </summary>
        private MainMenuUIView _mainMenuUiViewReference;

        /// <summary>
        /// UI id for the perf hud.
        /// </summary>
        private int _perfHudId;

        /// <summary>
        /// UI id for the logging hud.
        /// </summary>
        private int _loggingHudId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainDesignState(
            ApplicationConfig config,
            IMessageRouter messages,
            IElementControllerManager controllers,
            IHttpService http,
            IWorldAnchorProvider anchorProvider,
            IUIManager ui,
            UserPreferenceService preferenceService,
            IPrimaryAnchorManager primaryAnchor,
            IVoiceCommandManager voice,
            IAppSceneManager scenes,
            IElementTxnManager txns)
        {
            _config = config;
            _messages = messages;
            _controllers = controllers;
            _http = http;
            _anchorProvider = anchorProvider;
            _ui = ui;
            _preferenceService = preferenceService;
            _primaryAnchor = primaryAnchor;
            _voice = voice;
            _scenes = scenes;
            _txns = txns;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            // 
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            // push a frame
            _frame = _ui.CreateFrame();

            // load prefs first
            _prefLoad = _preferenceService
                .ForCurrentUser()
                .OnSuccess(prefs =>
                {
                    _prefs = prefs;

                    // content
                    _controllers
                        .Group(TAG_CONTENT)
                        .Filter(new TypeElementControllerFilter(typeof(ContentWidget)))
                        .Add<ElementSplashDesignController>(
                            new ElementSplashDesignController.DesignContext
                            {
                                Delegate = _design.Elements,
                                OnAdjust = Element_OnAdjust
                            });

                    // containers
                    _controllers
                        .Group(TAG_CONTAINER)
                        .Filter(new TypeElementControllerFilter(typeof(ContainerWidget)))
                        .Add<ElementSplashDesignController>(
                            new ElementSplashDesignController.DesignContext
                            {
                                Delegate = _design.Elements,
                                OnAdjust = Element_OnAdjust
                            });
                    
                    // anchors
                    _controllers
                        .Group(TAG_ANCHOR)
                        .Filter(new TypeElementControllerFilter(typeof(WorldAnchorWidget)))
                        .Add<AnchorDesignController>(new AnchorDesignController.AnchorDesignControllerContext
                        {
                            AppId = _design.App.Id,
                            SceneId = SceneIdForElement,
                            Config = _design.Config,
                            Http = _http,
                            Provider = _anchorProvider,
                            OnAdjust = Anchor_OnAdjust
                        });
                    
                    // turn on the controller groups
                    _controllers.Activate(TAG_CONTENT, TAG_CONTAINER, TAG_ANCHOR);

                    // open the splash menu
                    OpenSplashMenu();

                    _voice.Register("new", Voice_OnNew);
                 
                })
                .OnFailure(ex => Log.Warning(this, "Could not load user preferences: {0}", ex));
        }

        /// <summary>
        /// Method on voice command
        /// </summary>
        /// <param name="command">command</param>
        private void Voice_OnNew(string command)
        {
            if ((_primaryAnchor.Status == WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated))
            {
                CloseSplashMenu();
                OpenMainMenu();
                _mainMenuUiViewReference.NewSubMenu.Open();
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {

        }

        /// <inheritdoc />
        public void Exit()
        {
            _prefLoad.Abort();

            // kill element menus
            _controllers.Deactivate(TAG_CONTENT, TAG_CONTAINER, TAG_ANCHOR);

            // kill any other UI
            _frame.Release();

            _voice.Unregister("new");

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Opens the splash menu.
        /// </summary>
        private void OpenSplashMenu()
        {
            _ui
                .Open<SplashMenuUIView>(new UIReference
                {
                    UIDataId = "Design.Splash"
                }, out _splashId)
                .OnSuccess(el =>
                {
                    el.TxtName.Label = _design.App.Name;
                    el.OnOpenMenu += Splash_OnOpenMenu;
                    el.OnPlay += Splash_OnPlay;
                })
                .OnFailure(ex => Log.Error(this,
                    "Could not open SplashMenuUIView : {0}",
                    ex));
        }

        /// <summary>
        /// Closes splash menu.
        /// </summary>
        private void CloseSplashMenu()
        {
            _ui.Close(_splashId);
        }

        /// <summary>
        /// Opens main menu.
        /// </summary>
        private void OpenMainMenu()
        {
            if (_mainMenuUiViewReference == null)
            {
                _ui
                .Open<MainMenuUIView>(new UIReference
                {
                    UIDataId = "Design.MainMenu"
                }, out _mainId)
                .OnSuccess(el =>
                {
                    _mainMenuUiViewReference = el;
                    el.OnBack += MainMenu_OnBack;
                    el.OnNew += MainMenu_OnNew;
                    el.OnExperience += MainMenu_OnExperience;
                    el.OnLoggingHud += MainMenu_OnLoggingHud;
                    el.OnResetData += MainMenu_OnResetData;
                    el.OnClearAnchors += MainMenu_OnClearAnchors;
                    el.OnDefaultPlayModeChanged += MainMenu_OnDefaultPlayModeChanged;
                    el.OnDeviceRegistration += MainMenu_OnDeviceRegistration;
                    el.OnSignout += MainMenu_OnSignout;
                    el.OnMetricsHud += MainMenu_OnMetricsHud;

                    // find root
                    var id = _scenes.All.FirstOrDefault();
                    el.Initialize(
                        id,
                        _scenes.Root(id),
                        _txns,
                        _config,
                        _prefs.Data.App(_config.Play.AppId).Play);
                })
                .OnFailure(ex => Log.Error(this,
                    "Could not open MainMenuUIView : {0}",
                    ex));
            }
        }

        /// <summary>
        /// Opens the 'not implemented' message.
        /// </summary>
        private void OpenNotImplementedView()
        {
            if (_mainMenuUiViewReference != null)
            {
                _mainMenuUiViewReference.Root.Schema.Set("visible", false);
                int errorViewId;
                _ui
                    .Open<ICommonErrorView>(
                        new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        },
                        out errorViewId)
                    .OnSuccess(popup =>
                    {
                        popup.Message = "Feature not yet implemented";
                        popup.Action = "Press ok to continue";
                        popup.OnOk += () =>
                        {
                            _ui.Close(errorViewId);
                            _mainMenuUiViewReference.Root.Schema.Set("visible", true);
                        };
                    })
                    .OnFailure(er =>
                    {
                        Log.Fatal(this, "Could not open error popup : {0}.", er);
                    });
            }
        }

        /// <summary>
        /// Closes main menu.
        /// </summary>
        private void CloseMainMenu()
        {
            _ui.Close(_mainId);
        }
        
        /// <summary>
        /// Scene id for element.
        /// </summary>
        /// <param name="element">Element.</param>
        /// <returns></returns>
        private string SceneIdForElement(Element element)
        {
            // find root
            var parent = element;
            while (true)
            {
                if (null != parent.Parent)
                {
                    parent = parent.Parent;
                }
                else
                {
                    break;
                }
            }

            // find id of root
            var sceneIds = _design.Scenes.All;
            foreach (var sceneId in sceneIds)
            {
                var root = _design.Scenes.Root(sceneId);
                if (root == parent)
                {
                    return sceneId;
                }
            }

            return null;
        }

        /// <summary>
        /// Called when the splash menu wants to play.
        /// </summary>
        private void Splash_OnPlay()
        {
            _config.Play.Edit = false;
            _messages.Publish(MessageTypes.LOAD_APP);
        }

        /// <summary>
        /// Called when the splash menu wants to go back.
        /// </summary>
        private void Splash_OnBack()
        {
            _messages.Publish(MessageTypes.USER_PROFILE);
        }

        /// <summary>
        /// Called when splash wants to open the menu.
        /// </summary>
        private void Splash_OnOpenMenu()
        {
            CloseSplashMenu();
            OpenMainMenu();
        }

        /// <summary>
        /// Called when main menu wants to go back.
        /// </summary>
        private void MainMenu_OnBack()
        {
            CloseMainMenu();
            OpenSplashMenu();
        }

        /// <summary>
        /// Called when Reset button is activated
        /// </summary>
        private void MainMenu_OnResetData()
        {
            OpenNotImplementedView();
        }

        /// <summary>
        /// Called when anchors should be cleared.
        /// </summary>
        private void MainMenu_OnClearAnchors()
        {
            _anchorProvider.ClearAllAnchors();
        }

        /// <summary>
        /// Called when the logging hud should be opened.
        /// </summary>
        private void MainMenu_OnLoggingHud()
        {
            CloseMainMenu();
            OpenSplashMenu();

            _ui
                .OpenOverlay<LoggingUIView>(new UIReference
                {
                    UIDataId = "Logging.Hud"
                }, out _loggingHudId)
                .OnSuccess(el =>
                {
                    el.OnClose += () => _ui.Close(_loggingHudId);
                })
                .OnFailure(ex => Log.Error(this, "Could not open Logging.Hud : {0}", ex));
        }

        /// <summary>
        /// Called when the user selects one submenus under experience.
        /// </summary>
        /// <param name="type">The type of element ot create.</param>
        private void MainMenu_OnExperience(MainMenuUIView.ExperienceSubMenu type)
        {
            switch (type)
            {
                case MainMenuUIView.ExperienceSubMenu.New:
                {
                    _design.ChangeState<CreateNewAppDesignState>();
                    break;
                }
                case MainMenuUIView.ExperienceSubMenu.Load:
                {
                    _design.ChangeState<AppListViewDesignState>();
                    break;
                }
                case MainMenuUIView.ExperienceSubMenu.Duplicate:
                {
                    OpenNotImplementedView();
                    break;
                }
            }
        }

        /// <summary>
        /// Called when the user asks to create a new element.
        /// </summary>
        /// <param name="elementType">The type of element ot create.</param>
        private void MainMenu_OnNew(int elementType)
        {
            switch (elementType)
            {
                case ElementTypes.CONTENT:
                {
                    _design.ChangeState<NewContentDesignState>();
                    break;
                }
                case ElementTypes.WORLD_ANCHOR:
                {
                    _design.ChangeState<NewAnchorDesignState>();
                    break;
                }
                case ElementTypes.CONTAINER:
                {
                    _design.ChangeState<NewContainerDesignState>();
                    break;
                }
                case ElementTypes.CAPTION:
                {
                    OpenNotImplementedView();
                    break;
                }
                case ElementTypes.LIGHT:
                {
                    OpenNotImplementedView();
                    break;
                }
                default:
                {
                    Log.Warning(this,
                        "User requested to create {0}, but there is no inmplementation.",
                        elementType);
                    break;
                }
            }
        }

        /// <summary>
        /// Move to play mode.
        /// </summary>
        private void MainMenu_OnPlay()
        {
            _config.Play.Edit = false;
            _messages.Publish(MessageTypes.PLAY);
        }

        /// <summary>
        /// Called when default play mode has changed.
        /// </summary>
        /// <param name="play">Play.</param>
        private void MainMenu_OnDefaultPlayModeChanged(bool play)
        {
            Log.Info(this, "Queueing preference update.");

            _prefs.Queue((data, next) =>
            {
                // update
                var appData = data.App(_config.Play.AppId);
                appData.Play = play;

                Log.Info(this, "Preferences updated.");

                next(data);
            });
        }

        /// <summary>
        /// Called when the user requests to sync registrations.
        /// </summary>
        private void MainMenu_OnDeviceRegistration()
        {
            _messages.Publish(MessageTypes.DEVICE_REGISTRATION);
        }

        /// <summary>
        /// Called when signout is requested.
        /// </summary>
        private void MainMenu_OnSignout()
        {
            Log.Info(this, "Signout requested.");

            _messages.Publish(MessageTypes.SIGNOUT);
        }

        /// <summary>
        /// Called when the metrics hud should be opened.
        /// </summary>
        private void MainMenu_OnMetricsHud()
        {
            // open
            _ui
                .OpenOverlay<PerfDisplayUIView>(new UIReference
                {
                    UIDataId = "Perf.Hud"
                }, out _perfHudId)
                .OnSuccess(el =>
                {
                    el.OnClose += () => _ui.Close(_perfHudId);
                });

            CloseMainMenu();
            OpenSplashMenu();
        }

        /// <summary>
        /// Called when element asks for adjustment.
        /// </summary>
        private void Element_OnAdjust(ElementSplashDesignController controller)
        {
            _design.ChangeState<EditElementDesignState>(controller);
        }

        /// <summary>
        /// Called by anchor to open adjust menu.
        /// </summary>
        /// <param name="controller"></param>
        private void Anchor_OnAdjust(AnchorDesignController controller)
        {
            var isPrimary = controller.Anchor.Schema.Get<string>(PrimaryAnchorManager.PROP_TAG_KEY).Value == PrimaryAnchorManager.PROP_TAG_VALUE;
            if (isPrimary)
            {
                _design.ChangeState<EditPrimaryAnchorDesignState>(controller);
            }
            else
            {
                _design.ChangeState<EditAnchorDesignState>(controller);
            }
        }
    }
}