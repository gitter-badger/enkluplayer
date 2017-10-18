using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

using ContentGraphNode = CreateAR.SpirePlayer.ContentGraph.ContentGraphNode;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public interface IAssetPoolManager
    {
        T Get<T>(GameObject prefab) where T : class;
        bool Put(GameObject gameObject);
    }

    public class LazyAssetPoolManager : IAssetPoolManager
    {
        public T Get<T>(GameObject prefab) where T : class
        {
            return Cast<T>(Object.Instantiate(prefab));
        }

        public bool Put(GameObject gameObject)
        {
            Object.Destroy(gameObject);

            return true;
        }

        private static T Cast<T>(GameObject gameObject) where T : class
        {
            var cast = gameObject as T;
            if (null == cast)
            {
                if (typeof(Component) == typeof(T))
                {
                    var component = gameObject.GetComponent<T>()
                                    ?? gameObject.GetComponentInChildren<T>();
                    if (null == component)
                    {
                        Object.Destroy(gameObject);
                    }

                    return component;
                }

                Object.Destroy(gameObject);

                return null;
            }

            return cast;
        }
    }

    public class HierarchyManager
    {
        private readonly IAssetManager _assets;
        private readonly IAssetPoolManager _pools;
        private readonly IAppDataManager _appData;
        private readonly ContentGraph _graph;
        private readonly GameObject _root;
        private readonly Dictionary<string, HierarchyNodeMonoBehaviour> _gameObjects = new Dictionary<string, HierarchyNodeMonoBehaviour>();

        public HierarchyManager(
            IAssetManager assets,
            IAssetPoolManager pools,
            IAppDataManager appData,
            ContentGraph graph)
        {
            _assets = assets;
            _pools = pools;
            _appData = appData;
            _graph = graph;

            _root = new GameObject("Hierarchy");

            _graph.OnLoaded += Graph_OnLoaded;
        }

        private void Graph_OnLoaded(ContentGraphNode node)
        {
            Create(_root.transform, node);
        }

        public void Select(string contentId)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        private void Create(Transform parent, ContentGraphNode node)
        {
            var contentData = _appData.Get<ContentData>(node.ContentId);
            if (null == contentData)
            {
                Log.Error(this,
                    "Could not create HierarchyNodeMonoBehaviour for {0} : No content by that id.",
                    node.ContentId);
                return;
            }

            // TODO: pool
            var gameObject = new GameObject(contentData.Name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;

            var behavior = gameObject.AddComponent<HierarchyNodeMonoBehaviour>();
            behavior.Initialize(contentData);

            _gameObjects[contentData.Id] = behavior;

            // children
            var children = node.Children;
            var transform = gameObject.transform;
            for (int i = 0, len = children.Count; i < len; i++)
            {
                Create(transform, children[i]);
            }
        }
    }

    public class HierarchyNodeMonoBehaviour : MonoBehaviour
    {
        public void Initialize(ContentData contentData)
        {
            
        }

        public void Uninitialize()
        {
            
        }
    }

    /// <summary>
    /// State for moving about the hierarchy.
    /// </summary>
    public class HierarchyApplicationState : IState
    {
        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        private const string SCENE_NAME = "HierarchyMode";

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IApplicationState _state;
        private readonly IAssetManager _assets;
        private readonly IInputManager _input;
        private readonly IMessageRouter _router;
        private readonly ContentGraph _graph = new ContentGraph();
        private readonly HierarchyManager _gameObjects;

        /// <summary>
        /// Unsubscribe.
        /// </summary>
        private Action _unsub;
        
        /// <summary>
        /// Input state.
        /// </summary>
        [Inject(NamedInjections.INPUT_STATE_DEFAULT)]
        public IState InputState { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HierarchyApplicationState(
            IAppDataManager appData,
            IApplicationState state,
            IAssetManager assets,
            IAssetPoolManager pools,
            IInputManager input,
            IMessageRouter router)
        {
            _state = state;
            _assets = assets;
            _input = input;
            _router = router;

            _gameObjects = new HierarchyManager(assets, pools, appData, _graph);
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                SCENE_NAME,
                LoadSceneMode.Additive);

            // input
            _input.ChangeState(InputState);

            // load graph full of goodies
            _graph.Load(((HierarchyEvent) context).Root);

            // listen for selection
            _unsub = _router.Subscribe(
                MessageTypes.SELECT_CONTENT,
                Messages_OnSelectContent);
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _input.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _gameObjects.Clear();
            _graph.Clear();

            _unsub();

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }

        /// <summary>
        /// Called when content has been selected.
        /// </summary>
        /// <param name="message">Event.</param>
        private void Messages_OnSelectContent(object message)
        {
            var @event = (SelectContentEvent) message;

            Log.Info(this, "Select content : {0}.", @event.ContentId);

            _gameObjects.Select(@event.ContentId);
        }
    }
}