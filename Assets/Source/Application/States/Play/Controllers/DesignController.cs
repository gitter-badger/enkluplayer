using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class DesignController : ISceneUpdateDelegate, IElementUpdateDelegate
    {
        /// <summary>
        /// Transactions.
        /// </summary>
        private readonly IElementTxnManager _txns;

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
        private readonly IDesignState[] _states;
        
        /// <summary>
        /// Trellis API.
        /// </summary>
        private readonly ApiController _api;

        /// <summary>
        /// State machine.
        /// </summary>
        private readonly FiniteStateMachine _fsm;

        /// <summary>
        /// Backing variable for Scenes prpoperty.
        /// </summary>
        private readonly List<SceneDesignController> _sceneControllers = new List<SceneDesignController>();

        /// <summary>
        /// Element transactions currently tracked.
        /// </summary>
        private readonly Dictionary<Element, ElementTxn> _transactions = new Dictionary<Element, ElementTxn>();

        /// <summary>
        /// The app.
        /// </summary>
        private IAppController _app;
        
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
        /// Scenes.
        /// </summary>
        public ReadOnlyCollection<SceneDesignController> Scenes { get; private set; }

        /// <summary>
        /// Active scene.
        /// </summary>
        public SceneDesignController Active { get; set; }

        /// <summary>
        /// Config for play mode.
        /// </summary>
        public PlayModeConfig Config { get; private set; }

        /// <summary>
        /// Constuctor.
        /// </summary>
        public DesignController(
            IElementTxnManager txns,
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
            _elements = elements;
            _controllers = controllers;
            _api = api;

            _states = new IDesignState[]
            {
                main,
                newContent,
                editContent,
                reparent,
                anchors
            };

            _fsm = new FiniteStateMachine(_states);

            Scenes = new ReadOnlyCollection<SceneDesignController>(_sceneControllers);
        }

        /// <summary>
        /// Starts controllers.
        /// </summary>
        public void Setup(PlayModeConfig config, IAppController app)
        {
            Config = config;
            _app = app;
            _root = new GameObject("Design");
            _root.AddComponent<LineManager>();

            SetupSceneControllers();

            if (null == Active)
            {
                Log.Info(this, "No active Scene, creating a default.");

                Create()
                    .OnSuccess(scene => Start())
                    .OnFailure(exception =>
                    {
                        Log.Error(this, "Could not create Scene!");
                    });
            }
            else
            {
                Start();
            }
        }

        
        /// <summary>
        /// Tears down controller.
        /// </summary>
        public void Teardown()
        {
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
        public void ChangeState<T>(object context = null) where T : IDesignState
        {
            _fsm.Change<T>(context);
        }

        /// <summary>
        /// Moves an element.
        /// </summary>
        /// <param name="element">The element to move.</param>
        /// <param name="parent">The new parent.</param>
        /// <returns></returns>
        public IAsyncToken<Element> Move(
            Element element,
            Element parent)
        {
            return Async.Map(
                _txns.Request(new ElementTxn(Active.Id).Move(
                    element.Id,
                    parent.Id,
                    TransformedPosition(element, parent))),
                response => element);
        }

        /// <summary>
        /// Creates a scene.
        /// </summary>
        public IAsyncToken<SceneDesignController> Create()
        {
            var token = new AsyncToken<SceneDesignController>();

            // create a scene
            _api
                .Scenes
                .CreateScene(_app.Id, new Request())
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        var sceneId = response.Payload.Body.Id;
                        _txns
                            .TrackScene(sceneId)
                            .OnSuccess(_ =>
                            {
                                var controller = CreateSceneDesignController(sceneId);

                                if (null == Active)
                                {
                                    Active = controller;
                                }

                                token.Succeed(controller);
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
        /// Destroys a scene.
        /// </summary>
        /// <param name="id">The id of the scene.</param>
        /// <returns></returns>
        public IAsyncToken<SceneDesignController> Destroy(string id)
        {
            var cenet = ById(id);
            if (null == cenet)
            {
                return new AsyncToken<SceneDesignController>(new Exception(string.Format(
                    "Could not find scene with id {0}.",
                    id)));
            }

            /*var tokens = new List<IAsyncToken<Void>>();
            var props = cenet.ContentControllers.ToArray();
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];

                tokens.Add(cenet.Destroy(prop.Element.Id));
            }

            _sceneControllers.Remove(cenet);

            return Async.Map(
                Async.All(tokens.ToArray()),
                _ => cenet);*/
            return new AsyncToken<SceneDesignController>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Add(string sceneId, ElementData data)
        {
            return Async.Map(
                _txns.Request(new ElementTxn(sceneId).Create("root", data)),
                response => response.Elements[0]);
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Remove(string sceneId, Element element)
        {
            return Async.Map(
                _txns.Request(new ElementTxn(sceneId).Delete(element.Id)),
                response => response.Elements[0]);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, string value)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, int value)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, float value)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, bool value)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, Vec3 value)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Update(Element element, string key, Col4 value)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Update element: no active scene.");
                return;
            }

            Txn(element).Update(element.Id, key, value);
        }

        /// <inheritdoc />
        public void Finalize(Element element)
        {
            if (null == Active)
            {
                Log.Warning(this, "Could not Finalize element: no active scene.");
                return;
            }

            var txn = Txn(element);
            if (0 != txn.Actions.Count)
            {
                _txns.Request(txn);
            }

            _transactions.Remove(element);
        }

        /// <summary>
        /// Starts design mode.
        /// </summary>
        private void Start()
        {
            // create dynamic root
            {
                _float = (FloatWidget)_elements.Element(
                    @"<?Vine><Float id='Root' position=(0, 0, 2) face='camera'><ScaleTransition /></Float>");
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
        /// Sets up all design controllers.
        /// </summary>
        private void SetupSceneControllers()
        {
            // create scene controllers
            var scenes = _txns.TrackedScenes;
            for (var i = 0; i < scenes.Length; i++)
            {
                CreateSceneDesignController(scenes[i]);
            }

            // select a default scene
            if (_sceneControllers.Count > 0)
            {
                Active = _sceneControllers[0];
            }
        }

        /// <summary>
        /// Factory method for scenes.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        /// <returns></returns>
        private SceneDesignController CreateSceneDesignController(string sceneId)
        {
            var controller = new SceneDesignController(
                this,
                sceneId,
                _txns.Root(sceneId));

            _sceneControllers.Add(controller);

            return controller;
        }

        /// <summary>
        /// Creates or retrieves an element txn.
        /// </summary>
        /// <param name="element">The element in question</param>
        /// <returns></returns>
        private ElementTxn Txn(Element element)
        {
            ElementTxn txn;
            if (!_transactions.TryGetValue(element, out txn))
            {
                txn = _transactions[element] = new ElementTxn(Active.Id);
            }

            return txn;
        }

        /// <summary>
        /// Returns a <c>PropSet</c> by id.
        /// </summary>
        /// <param name="id">The id of the set.</param>
        /// <returns></returns>
        private SceneDesignController ById(string id)
        {
            for (int i = 0, len = _sceneControllers.Count; i < len; i++)
            {
                var set = _sceneControllers[i];
                if (set.Id == id)
                {
                    return set;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves a transformed position from one element to another.
        /// </summary>
        /// <param name="element">The element that is to be moved.</param>
        /// <param name="parent">The new parent.</param>
        /// <returns></returns>
        private Vec3 TransformedPosition(Element element, Element parent)
        {
            var unityElement = NearestUnityElement(element);
            var unityParent = NearestUnityElement(parent);

            // trivial case
            if (unityParent == unityElement)
            {
                return element.Schema.Get<Vec3>("position").Value;
            }

            // transform to new parent's local space
            var pos = unityElement.GameObject.transform.position;
            return unityParent
                .GameObject
                .transform
                .worldToLocalMatrix
                .MultiplyPoint3x4(pos)
                .ToVec();
        }

        /// <summary>
        /// Traverses upward till a unity element is found. The initial element
        /// is also tested.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private IUnityElement NearestUnityElement(Element element)
        {
            while (null != element)
            {
                var unityElement = element as IUnityElement;
                if (null != unityElement)
                {
                    return unityElement;
                }

                element = element.Parent;
            }

            return null;
        }
    }
}