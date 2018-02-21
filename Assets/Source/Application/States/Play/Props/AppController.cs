using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateScene;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages <c>Scene</c> objects.
    /// </summary>
    public class AppController : IAppController, ISceneUpdateDelegate, IElementUpdateDelegate
    {
        /// <summary>
        /// Trellis api.
        /// </summary>
        public readonly ApiController _api;

        /// <summary>
        /// Pipe for all element updates.
        /// </summary>
        private readonly IElementTxnManager _txns;
        
        /// <summary>
        /// Backing variable for Scenes prpoperty.
        /// </summary>
        private readonly List<SceneController> _sceneControllers = new List<SceneController>();

        /// <summary>
        /// Element transactions currently tracked.
        /// </summary>
        private readonly Dictionary<Element, ElementTxn> _transactions = new Dictionary<Element, ElementTxn>();

        /// <summary>
        /// The current app id.
        /// </summary>
        private string _appId;

        /// <inheritdoc />
        public ReadOnlyCollection<SceneController> Scenes { get; private set; }

        /// <inheritdoc />
        public SceneController Active { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppController(
            ApiController api,
            IElementTxnManager txns)
        {
            Scenes = new ReadOnlyCollection<SceneController>(_sceneControllers);

            _api = api;
            _txns = txns;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId)
        {
            _appId = appId;

            var token = new AsyncToken<Void>();

            LogVerbose("Initialize().");
            
            _txns
                .Initialize(appId)
                .OnSuccess(_ =>
                {
                    LogVerbose("Txns initialized.");

                    // create scene controllers
                    var scenes = _txns.TrackedScenes;
                    for (var i = 0; i < scenes.Length; i++)
                    {
                        var sceneId = scenes[i];
                        var controller = new SceneController(
                            this, this,
                            sceneId,
                            _txns.Root(sceneId));

                        _sceneControllers.Add(controller);
                    }

                    // select a default scene
                    if (_sceneControllers.Count > 0)
                    {
                        Active = _sceneControllers[0];
                    }

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);
            
            return token;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            //_props.Clear();
        }

        /// <inheritdoc />
        public IAsyncToken<SceneController> Create()
        {
            var token = new AsyncToken<SceneController>();

            // create a scene
            _api
                .Scenes
                .CreateScene(_appId, new Request())
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        var sceneId = response.Payload.Body.Id;
                        _txns
                            .TrackScene(sceneId)
                            .OnSuccess(_ =>
                            {
                                var controller = new SceneController(
                                    this, this,
                                    sceneId,
                                    _txns.Root(sceneId));
                                _sceneControllers.Add(controller);

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

        /// <inheritdoc />
        public IAsyncToken<SceneController> Destroy(string id)
        {
            var set = ById(id);
            if (null == set)
            {
                return new AsyncToken<SceneController>(new Exception(string.Format(
                    "Could not find PropSet with id {0}.",
                    id)));
            }

            var tokens = new List<IAsyncToken<Void>>();
            var props = set.Controllers.ToArray();
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];

                tokens.Add(set.Destroy(prop.Element.Id));
            }

            _sceneControllers.Remove(set);

            return Async.Map(
                Async.All(tokens.ToArray()),
                _ => set);
        }
        
        /// <inheritdoc />
        public IAsyncToken<Element> Add(SceneController scene, ElementData data)
        {
            if (null == Active)
            {
                return new AsyncToken<Element>(new Exception("Could not Add element: no active scene."));
            }
            
            return Async.Map(
                _txns.Request(new ElementTxn(Active.Id).Create("root", data)),
                response => response.Elements[0]);
        }

        /// <inheritdoc />
        public IAsyncToken<Element> Remove(SceneController scene, Element element)
        {
            if (null == Active)
            {
                return new AsyncToken<Element>(new Exception("Could not Delete element: no active scene."));
            }

            return Async.Map(
                _txns.Request(new ElementTxn(Active.Id).Delete(element.Id)),
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
        private SceneController ById(string id)
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
        /// Logging.
        /// </summary>
        //[Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}