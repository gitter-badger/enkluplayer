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
    public class ArDesignController : IDesignController
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
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
        public ArDesignController(
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
            EditContentDesignState editContent,
            ReparentDesignState reparent,
            AnchorDesignState anchors)
        {
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
                editContent,
                reparent,
                anchors
            };

            _fsm = new FiniteStateMachine(_states);
        }

        /// <inheritdoc />
        public void Setup(DesignerContext context, IAppController app)
        {
            Config = context.PlayConfig;
            App = app;

            _root = new GameObject("Design");
            _root.AddComponent<LineManager>();

            if (null == _elementUpdater.Active)
            {
                Log.Error(this, "No active Scene!");

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
                        el.OnOk += () => _messages.Publish(MessageTypes.USER_PROFILE);
                    })
                    .OnFailure(exception => Log.Error(this, "Could not load error popup : {0}.", exception));
                return;
            }
            
            if (context.Edit)
            {
                if (_connection.IsConnected)
                {
                    StartEdit();
                }
                else
                {
                    StartPlay();

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
                            el.Message = "It appears that you are currently offline. Edit mode will be disabled.";
                            el.Action = "Ok";
                            el.OnOk += () => _ui.Pop();
                        })
                        .OnFailure(exception => Log.Error(this, "Could not open ErrorPopupUIView : {0}.", exception));
                }
            }
            else
            {
                StartPlay();
            }
        }

        /// <inheritdoc />
        public void Teardown()
        {
            _voice.Unregister("play");
            _voice.Unregister("edit");

            // uninitialize states
            for (var i = 0; i < _states.Length; i++)
            {
                _states[i].Uninitialize();
            }

            _fsm.Change(null);

            _controllers.Active = false;

            _float.Destroy();
            _staticRoot.Destroy();

            Object.Destroy(_root);
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

        /// <summary>
        /// Starts design mode.
        /// </summary>
        private void StartEdit()
        {
            _voice.Register("play", Voice_OnPlay);

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
        /// Starts play.
        /// </summary>
        private void StartPlay()
        {
            _voice.Register("edit", Voice_OnEdit);
        }

        /// <summary>
        /// Called when the word "play" is heard.
        /// </summary>
        /// <param name="command">The voice command.</param>
        private void Voice_OnPlay(string command)
        {
            _messages.Publish(
                MessageTypes.CHANGE_STATE,
                new DesignerContext
                {
                    Edit = false
                });
        }

        /// <summary>
        /// Called when the word "edit" is heard.
        /// </summary>
        /// <param name="command">The voice command.</param>
        private void Voice_OnEdit(string command)
        {
            _messages.Publish(
                MessageTypes.CHANGE_STATE,
                new DesignerContext
                {
                    Edit = true
                });
        }
    }
}