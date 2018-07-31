using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateScene;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls design mode menus.
    /// </summary>
    public class HmdDesignController : IDesignController
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly ApplicationConfig _config;
        private readonly IElementTxnManager _txns;
        private readonly IAppSceneManager _scenes;
        private readonly IElementUpdateDelegate _elementUpdater;
        private readonly IElementFactory _elements;
        private readonly IElementControllerManager _controllers;
        private readonly IConnection _connection;
        private readonly IVoiceCommandManager _voice;
        private readonly IUIManager _ui;
        private readonly IMessageRouter _messages;

        /// <summary>
        /// All states.
        /// </summary>
        private readonly IArDesignState[] _states;
        
        /// <summary>
        /// Trellis API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// State machine.
        /// </summary>
        private readonly FiniteStateMachine _fsm;

        /// <summary>
        /// Root of controls.
        /// </summary>
        private GameObject _root;
        
        /// <summary>
        /// Root float.
        /// </summary>
        private FloatWidget _float;

        /// <summary>
        /// Root element of dynamic menus.
        /// </summary>
        private ScaleTransition _dynamicRoot;

        /// <summary>
        /// Root element of static menus.
        /// </summary>
        private ContainerWidget _staticRoot;

        /// <summary>
        /// Id of play menu.
        /// </summary>
        private int _playMenuId;

        /// <summary>
        /// Saves whether it was edit or play mode that was setup.
        /// </summary>
        private bool _setupEdit;

        /// <summary>
        /// Config for play mode.
        /// </summary>
        public PlayModeConfig Config { get; private set; }

        /// <summary>
        /// Manipulates elements.
        /// </summary>
        public IElementUpdateDelegate Elements
        {
            get { return _elementUpdater; }
        }

        /// <summary>
        /// Controls the app.
        /// </summary>
        public IAppController App { get; private set; }

        /// <summary>
        /// Manages element transactions.
        /// </summary>
        public IElementTxnManager Txns
        {
            get { return _txns; }
        }

        /// <summary>
        /// Manages scenes.
        /// </summary>
        public IAppSceneManager Scenes
        {
            get { return _scenes; }
        }

        /// <summary>
        /// Constuctor.
        /// </summary>
        public HmdDesignController(
            ApplicationConfig config,
            IElementTxnManager txns,
            IAppSceneManager scenes,
            IElementUpdateDelegate elementUpdater,
            IElementFactory elements,
            IElementControllerManager controllers,
            IConnection connection,
            IVoiceCommandManager voice,
            IUIManager ui,
            IMessageRouter messages,
            ApiController api,

            // design states
            MainDesignState main,
            NewContentDesignState newContent,
            NewAnchorDesignState newAnchor,
            NewContainerDesignState newContainer,
            EditElementDesignState editElement,
            ReparentDesignState reparent,
            EditAnchorDesignState anchors,
            AppListViewDesignState appList,
            CreateNewAppDesignState createNewApp)
        {
            _config = config;
            _txns = txns;
            _scenes = scenes;
            _elementUpdater = elementUpdater;
            _elements = elements;
            _controllers = controllers;
            _connection = connection;
            _voice = voice;
            _ui = ui;
            _messages = messages;
            _api = api;

            _states = new IArDesignState[]
            {
                main,
                newContent,
                newAnchor,
                newContainer,
                editElement,
                reparent,
                anchors,
                appList,
                createNewApp
            };

            _fsm = new FiniteStateMachine(_states);
        }

        /// <inheritdoc />
        public void Setup(DesignerContext context, IAppController app)
        {
            Config = context.PlayConfig;
            App = app;

            _root = new GameObject("Design");
            
            if (UnityEngine.Application.isEditor)
            {
                _root.AddComponent<HmdEditorKeyboardControls>();
            }

            if (null == _elementUpdater.Active)
            {
                Log.Error(this, "No active Scene!");

                ShowFatalError();
                return;
            }
            
            if (context.Edit)
            {
                SetupEdit();

                if (!_connection.IsConnected)
                {
                    ShowOfflineModeNotice();
                }
            }
            else
            {
                SetupPlay();
            }
        }

        /// <inheritdoc />
        public void Teardown()
        {
            if (_setupEdit)
            {
                TeardownEdit();
            }
            else
            {
                TeardownPlay();
            }

            if (null != _root)
            {
                Object.Destroy(_root);
            }
        }

        /// <summary>
        /// Changes design state.
        /// </summary>
        /// <typeparam name="T">The type of design state.</typeparam>
        public void ChangeState<T>(object context = null) where T : IArDesignState
        {
            _fsm.Change<T>(context);
        }
        
        /// <summary>
        /// Creates a scene.
        /// </summary>
        public IAsyncToken<string> Create()
        {
            var token = new AsyncToken<string>();

            // create a scene
            _api
                .Scenes
                .CreateScene(App.Id, new Request())
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        // TODO: FIX THIS
                        /*
                        var sceneId = response.Payload.Body.Id;
                        
                        _txns
                            .TrackScene(sceneId)
                            .OnSuccess(_ =>
                            {
                                if (null == _elementUpdater.Active)
                                {
                                    _elementUpdater.Active = sceneId;
                                }

                                token.Succeed(sceneId);
                            })
                            .OnFailure(token.Fail);*/
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc />
        public void Select(string sceneId, string elementId)
        {
            // ignore forced selection in AR mode
        }

        /// <inheritdoc />
        public void Focus(string sceneId, string elementId)
        {
            // ignore forced focus in AR mode
        }

        /// <summary>
        /// Starts design mode.
        /// </summary>
        private void SetupEdit()
        {
            _setupEdit = true;

            _voice.Register("play", Voice_OnPlay);

            // hierarchy rendering
            Camera.main.gameObject.AddComponent<HierarchyLineRenderer>();

            // create dynamic root
            {
                _float = (FloatWidget)_elements.Element(
                    @"<?Vine><Float id='Root' position=(0, 0, 2) face='camera' focus.visible=false><ScaleTransition /></Float>");
                _float.GameObject.transform.parent = _root.transform;
                _dynamicRoot = (ScaleTransition)_float.Children[0];
            }

            // create static root
            {
                _staticRoot = (ContainerWidget)_elements.Element(@"<?Vine><Container />");
                _staticRoot.GameObject.transform.parent = _root.transform;
            }

            _controllers.Active = true;

            // initialize states
            for (var i = 0; i < _states.Length; i++)
            {
                _states[i].Initialize(
                    this,
                    _root,
                    _dynamicRoot,
                    _staticRoot);
            }

            // start initial state
            _fsm.Change<MainDesignState>();
        }

        /// <summary>
        /// Tears down edit mode.
        /// </summary>
        private void TeardownEdit()
        {
            _voice.Unregister("play");

            // uninitialize states
            for (var i = 0; i < _states.Length; i++)
            {
                _states[i].Uninitialize();
            }

            _fsm.Change(null);

            _controllers.Active = false;
            _controllers.Release();

            _float.Destroy();
            _staticRoot.Destroy();

            Object.Destroy(_root);

            // hierarchy rendering
            Object.Destroy(Camera.main.gameObject.GetComponent<HierarchyLineRenderer>());
        }

        /// <summary>
        /// Starts play.
        /// </summary>
        private void SetupPlay()
        {
            _setupEdit = false;

            _voice.Register("menu", Voice_OnPlayMenu);
            _voice.Register("edit", Voice_OnEdit);

            // for editor only
            if (UnityEngine.Application.isEditor)
            {
                Voice_OnPlayMenu("menu");
            }
        }

        /// <summary>
        /// Tears down play mode.
        /// </summary>
        private void TeardownPlay()
        {
            _voice.Unregister("menu");
            _voice.Unregister("edit");

            _ui.Close(_playMenuId);
        }

        /// <summary>
        /// Displays a fatal error popup.
        /// </summary>
        private void ShowFatalError()
        {
            int id;
            _ui
                .Open<ErrorPopupUIView>(
                    new UIReference
                    {
                        UIDataId = UIDataIds.ERROR
                    },
                    out id)
                .OnSuccess(el =>
                {
                    el.Message = "An error occurred when playing this app: there are no scenes.";
                    el.Action = "Okay";
                    el.OnOk += () =>
                    {
                        _ui.Pop();

                        _messages.Publish(MessageTypes.USER_PROFILE);
                    };
                })
                .OnFailure(exception =>
                {
                    _messages.Publish(MessageTypes.USER_PROFILE);

                    Log.Error(this, "Could not load error popup : {0}.", exception);
                });
        }

        /// <summary>
        /// Displays a menu that offline mode is on.
        /// </summary>
        private void ShowOfflineModeNotice()
        {
            int id;
            _ui
                .Open<ErrorPopupUIView>(
                    new UIReference
                    {
                        UIDataId = UIDataIds.ERROR
                    },
                    out id)
                .OnSuccess(el =>
                {
                    el.Message = "You appear to be offline. Any edits to the scene will be discarded.";
                    el.Action = "Got it";
                    el.OnOk += () => _ui.Pop();
                });
        }

        /// <summary>
        /// Called when play menu is called for.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnPlayMenu(string command)
        {
            Log.Info(this, "Voice command opening play menu.");

            _ui
                .Open<PlayMenuUIView>(
                    new UIReference
                    {
                        UIDataId = "Play.Main"
                    },
                    out _playMenuId)
                .OnSuccess(el =>
                {
                    el.OnEdit += () =>
                    {
                        _config.Play.Edit = true;

                        _messages.Publish(MessageTypes.LOAD_APP);
                    };

                    el.OnBack += () => _messages.Publish(MessageTypes.USER_PROFILE);
                })
                .OnFailure(exception =>
                {
                    _messages.Publish(MessageTypes.USER_PROFILE);

                    Log.Error(this, "Could not open play menu : {0}.", exception);
                });
        }

        /// <summary>
        /// Called when edit mode is called for.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnEdit(string command)
        {
            Log.Info(this, "Voice command starting edit mode.");

            _config.Play.Edit = true;

            _messages.Publish(MessageTypes.LOAD_APP);
        }

        /// <summary>
        /// Called when play mode is called.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnPlay(string command)
        {
            Log.Info(this, "Voice command starting edit mode.");

            _config.Play.Edit = false;

            _messages.Publish(MessageTypes.LOAD_APP);
        }
    }
}