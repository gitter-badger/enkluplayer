using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages element transactions.
    /// </summary>
    public class ElementTxnManager : IElementTxnManager
    {
        /// <summary>
        /// Json.
        /// </summary>
        private readonly JsonSerializer _json = new JsonSerializer();

        /// <summary>
        /// Transport implementation.
        /// </summary>
        private readonly IElementTxnTransport _transport;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;

        /// <summary>
        /// Creates strategies.
        /// </summary>
        private readonly IElementActionStrategyFactory _strategyFactory;

        /// <summary>
        /// Creates stores.
        /// </summary>
        private readonly IElementTxnStoreFactory _storeFactory;

        /// <summary>
        /// Lookup from sceneId -> scene loads.
        /// </summary>
        private readonly Dictionary<string, IAsyncToken<Void>> _sceneLoads = new Dictionary<string, IAsyncToken<Void>>();

        /// <summary>
        /// Lookup from sceneId -> store.
        /// </summary>
        private readonly Dictionary<string, IElementTxnStore> _stores = new Dictionary<string, IElementTxnStore>();

        /// <summary>
        /// Lookup from sceneId -> root element.
        /// </summary>
        private readonly Dictionary<string, Element> _scenes = new Dictionary<string, Element>();

        /// <summary>
        /// Ids of transactions.
        /// </summary>
        private readonly List<long> _txnIds = new List<long>();
        
        /// <summary>
        /// App id.
        /// </summary>
        private string _appId;

        /// <inheritdoc />
        public string[] TrackedScenes { get { return _scenes.Keys.ToArray(); } }

        /// <inheritdoc />
        public event Action<string> OnSceneAfterTracked;

        /// <inheritdoc />
        public event Action<string> OnSceneBeforeUntracked;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxnManager(
            IElementTxnTransport transport,
            IElementFactory elements,
            IElementActionStrategyFactory strategyFactory,
            IElementTxnStoreFactory storeFactory)
        {
            _transport = transport;
            _elements = elements;
            _strategyFactory = strategyFactory;
            _storeFactory = storeFactory;
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId)
        {
            _appId = appId;

            var token = new AsyncToken<Void>();

            // get app
            _transport
                .GetApp(appId)
                .OnSuccess(response =>
                {
                    Log.Info(this,
                        "Loaded app data for '{0}'. Now loading scenes.",
                        appId);

                    // load each scene
                    var scenes = response.Body.Scenes;
                    Async
                        .All(scenes.Select(LoadScene).ToArray())
                        .OnSuccess(_ =>
                        {
                            Log.Info(this,
                                "Successfully loaded {0} scenes.",
                                scenes.Length);

                            token.Succeed(Void.Instance);
                        })
                        .OnFailure(token.Fail);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Uninitialize()
        {
            _appId = string.Empty;

            // stop loads
            foreach (var pair in _sceneLoads)
            {
                pair.Value.Abort();
            }
            _sceneLoads.Clear();

            // destroy scene stores
            foreach (var pair in _stores)
            {
                pair.Value.Destroy();
            }
            _stores.Clear();

            // unload all scenes
            foreach (var scene in TrackedScenes)
            {
                UnloadScene(scene);
            }

            return new AsyncToken<Void>(Void.Instance);
        }

        /// <inheritdoc />
        public Element Root(string sceneId)
        {
            Element element;
            if (_scenes.TryGetValue(sceneId, out element))
            {
                return element;
            }

            return null;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> TrackScene(string sceneId)
        {
            IAsyncToken<Void> token;
            if (!_sceneLoads.TryGetValue(sceneId, out token))
            {
                token = _sceneLoads[sceneId] = LoadScene(sceneId);
            }

            return token;
        }

        /// <inheritdoc />
        public void UntrackScene(string sceneId)
        {
            // abort load
            IAsyncToken<Void> token;
            if (_sceneLoads.TryGetValue(sceneId, out token))
            {
                token.Abort();
            }
            _sceneLoads.Remove(sceneId);

            // destroy store
            IElementTxnStore store;
            if (_stores.TryGetValue(sceneId, out store))
            {
                if (null != OnSceneBeforeUntracked)
                {
                    OnSceneBeforeUntracked(sceneId);
                }

                // TODO: teardown scene
            }
            _stores.Remove(sceneId);
        }

        /// <inheritdoc />
        public IAsyncToken<ElementResponse> Request(ElementTxn txn)
        {
            // find scene
            IElementTxnStore store;
            if (!_stores.TryGetValue(txn.SceneId, out store))
            {
                return new AsyncToken<ElementResponse>(new Exception(
                    "Cannot make transaction request against untracked scene. Did you forget to call Track() first?"));
            }

            var elementResponse = new ElementResponse();

            // find affected elements
            AddAffectedElements(txn, elementResponse, ElementActionTypes.DELETE);
            AddAffectedElements(txn, elementResponse, ElementActionTypes.UPDATE);

            // send txn to store
            string error;
            if (!store.Request(txn, out error))
            {
                return new AsyncToken<ElementResponse>(new Exception(string.Format(
                    "Could not process txn : {0}.", error)));
            }

            var token = new AsyncToken<ElementResponse>();
            
            // send
            _txnIds.Add(txn.Id);

            while (_txnIds.Count > 1000)
            {
                _txnIds.RemoveAt(0);
            }

            _transport
                .Request(txn.Id, _appId, txn.SceneId, txn.Actions.ToArray())
                .OnSuccess(_ =>
                {
                    store.Commit(txn.Id);

                    // add created elements
                    AddAffectedElements(txn, elementResponse, ElementActionTypes.CREATE);

                    token.Succeed(elementResponse);
                })
                .OnFailure(exception =>
                {
                    // rollback txn
                    store.Rollback(txn.Id);

                    token.Fail(new Exception(string.Format(
                        "Error sending element txn : {0}.",
                        exception)));
                });

            return token;
        }

        /// <inheritdoc />
        public ElementResponse Apply(ElementTxn txn)
        {
            // find scene
            IElementTxnStore store;
            if (!_stores.TryGetValue(txn.SceneId, out store))
            {
                Log.Warning(
                    this,
                    "Cannot apply transaction against untracked scene. Did you forget to call Track() first?");
                return new ElementResponse();
            }

            var elementResponse = new ElementResponse();

            // find affected elements
            AddAffectedElements(txn, elementResponse, ElementActionTypes.DELETE);
            AddAffectedElements(txn, elementResponse, ElementActionTypes.UPDATE);

            // apply!
            store.Apply(txn);
            
            // add created elements
            AddAffectedElements(txn, elementResponse, ElementActionTypes.CREATE);

            return elementResponse;
        }

        /// <inheritdoc />
        public bool IsTracked(long txnId)
        {
            return _txnIds.Contains(txnId);
        }

        /// <summary>
        /// Unloads a scene.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        private void UnloadScene(string sceneId)
        {
            var root = Root(sceneId);
            if (null != root)
            {
                _scenes.Remove(sceneId);

                root.Destroy();
            }
        }

        /// <summary>
        /// Loads a scene by id.
        /// </summary>
        /// <param name="sceneId">The id of the scene.</param>
        /// <returns></returns>
        private IAsyncToken<Void> LoadScene(string sceneId)
        {
            var token = new AsyncToken<Void>();

            _transport
                .GetScene(_appId, sceneId)
                .OnSuccess(response =>
                {
                    object obj;
                    var bytes = Encoding.UTF8.GetBytes(response.Body.Elements);
                    _json.Deserialize(
                        typeof(ElementData),
                        ref bytes,
                        out obj);

                    var root = _elements.Element(new ElementDescription
                    {
                        Root = new ElementRef
                        {
                            Id = "root"
                        },
                        Elements = new[]
                        {
                            (ElementData) obj
                        }
                    });

                    var strategy = _strategyFactory.Instance(root);
                    _stores[sceneId] = _storeFactory.Instance(strategy);
                    _scenes[sceneId] = root;

                    if (null != OnSceneAfterTracked)
                    {
                        OnSceneAfterTracked(sceneId);
                    }

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Finds affected elements and adds them to the response.
        /// </summary>
        /// <param name="txn">The txn.</param>
        /// <param name="elementResponse">The response object.</param>
        /// <param name="actionType">Type of action to inspect.</param>
        private void AddAffectedElements(
            ElementTxn txn,
            ElementResponse elementResponse,
            string actionType)
        {
            var root = Root(txn.SceneId);
            for (var i = 0; i < txn.Actions.Count; i++)
            {
                var action = txn.Actions[i];
                if (actionType != action.Type)
                {
                    continue;
                }

                var elementId = null == action.Element || string.IsNullOrEmpty(action.Element.Id)
                    ? action.ElementId
                    : action.Element.Id;
                var element = root.Id == elementId
                    ? root
                    : root.FindOne<Element>(".." + elementId);
                if (null == element)
                {
                    Log.Warning(this,
                        "Could not find affected Element for action {0} : {1}.",
                        action,
                        elementId);
                }
                else
                {
                    elementResponse.Elements.Add(element);
                }
            }
        }
    }
}