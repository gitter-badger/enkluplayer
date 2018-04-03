using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
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
        /// Transactions.
        /// </summary>
        private readonly IElementTxnManager _txns;
        
        /// <summary>
        /// Updates elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementUpdater;

        /// <summary>
        /// Elements!
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Manages controllers for all elements.
        /// </summary>
        private readonly IElementControllerManager _controllers;

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
        /// Constuctor.
        /// </summary>
        public ArDesignController(
            IElementTxnManager txns,
            IElementUpdateDelegate elementUpdater,
            IElementFactory elements,
            IElementControllerManager controllers,
            ApiController api,

            // design states
            MainDesignState main,
            NewContentDesignState newContent,
            EditContentDesignState editContent,
            ReparentDesignState reparent,
            AnchorDesignState anchors)
        {
            _txns = txns;
            _elementUpdater = elementUpdater;
            _elements = elements;
            _controllers = controllers;
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
        public void Setup(PlayModeConfig config, IAppController app)
        {
            Config = config;
            App = app;
            _root = new GameObject("Design");
            _root.AddComponent<LineManager>();
            
            if (null == _elementUpdater.Active)
            {
                Log.Info(this, "No active Scene!");
            }
            else
            {
                Start();
            }
        }
        
        /// <inheritdoc />
        public void Teardown()
        {
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
                            .OnFailure(token.Fail);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Starts design mode.
        /// </summary>
        private void Start()
        {
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
    }
}