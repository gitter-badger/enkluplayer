using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
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
        private const string TAG_SCAN = "scan";

        /// <summary>
        /// Configuration values.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Manages controllers.
        /// </summary>
        private readonly IElementControllerManager _controllers;
        
        /// <summary>
        /// Http interface.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Caches world anchor data.
        /// </summary>
        private readonly IWorldAnchorCache _anchorCache;

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
        /// Constructor.
        /// </summary>
        public MainDesignState(
            ApplicationConfig config,
            IMessageRouter messages,
            IElementControllerManager controllers,
            IHttpService http,
            IWorldAnchorCache anchorCache,
            IWorldAnchorProvider anchorProvider,
            IUIManager ui,
            UserPreferenceService preferenceService)
        {
            _config = config;
            _messages = messages;
            _controllers = controllers;
            _http = http;
            _anchorCache = anchorCache;
            _anchorProvider = anchorProvider;
            _ui = ui;
            _preferenceService = preferenceService;
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

                    // scans
                    _controllers
                        .Group(TAG_SCAN)
                        .Filter(new TypeElementControllerFilter(typeof(ScanWidget)))
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
                            Cache = _anchorCache,
                            Provider = _anchorProvider,
                            OnAdjust = Anchor_OnAdjust
                        });

                    // turn on the controller groups
                    _controllers.Activate(TAG_CONTENT, TAG_CONTAINER, TAG_SCAN);

                    // open the splash menu
                    OpenSplashMenu();
                })
                .OnFailure(ex => Log.Error(this, "Could not load user preferences!"));
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
            _controllers.Deactivate(TAG_CONTENT, TAG_CONTAINER, TAG_SCAN);

            // kill any other UI
            _frame.Release();

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
                    el.OnOpenMenu += Splash_OnOpenMenu;
                    el.OnBack += Splash_OnBack;
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
            _ui
                .Open<MainMenuUIView>(new UIReference
                {
                    UIDataId = "Design.MainMenu"
                }, out _mainId)
                .OnSuccess(el =>
                {
                    el.OnBack += MainMenu_OnBack;
                    el.OnNew += MainMenu_OnNew;
                    el.OnPlay += MainMenu_OnPlay;
                    el.OnDefaultPlayModeChanged += MainMenu_OnDefaultPlayModeChanged;
                    el.Initialize(_prefs.Data.App(_config.Play.AppId).Play);
                })
                .OnFailure(ex => Log.Error(this,
                    "Could not open MainMenuUIView : {0}",
                    ex)); ;
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
            _design.ChangeState<EditAnchorDesignState>(controller);
        }
    }
}