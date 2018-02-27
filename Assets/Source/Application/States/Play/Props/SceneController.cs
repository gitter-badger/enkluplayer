using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages a set of Element controllers for a scene, and pipes updates
    /// about.
    /// </summary>
    public class SceneController
    {
        /// <summary>
        /// Additional data to store about elements.
        /// </summary>
        public class SceneElementContext
        {
            /// <summary>
            /// Anchor data.
            /// </summary>
            public readonly LineData AnchorLine = new LineData();

            /// <summary>
            /// Nearest WorldAnchor up the hierarchy.
            /// </summary>
            public WorldAnchorWidget ParentAnchor;
        }

        /// <summary>
        /// Receives update requests from Elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementUpdateDelegate;

        /// <summary>
        /// Received propset update events.
        /// </summary>
        private readonly ISceneUpdateDelegate _sceneDelegate;

        /// <summary>
        /// Backing property for Controllers.
        /// </summary>
        private readonly List<ContentDesignController> _controllers = new List<ContentDesignController>();

        /// <summary>
        /// Map from anchor to list of child controllers.
        /// </summary>
        private readonly Dictionary<WorldAnchorWidget, ContentDesignController[]> _anchorGraph = new Dictionary<WorldAnchorWidget, ContentDesignController[]>();
        
        /// <summary>
        /// Manages line rendering.
        /// </summary>
        private readonly LineManager _lines;

        /// <summary>
        /// Root of the scene.
        /// </summary>
        private readonly Element _root;

        /// <summary>
        /// The unique id of this scene.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// All element controllers.
        /// </summary>
        public ReadOnlyCollection<ContentDesignController> ContentControllers { get; private set; }

        /// <summary>
        /// True iff anchors should show children.
        /// </summary>
        public bool ShowAnchorChildren
        {
            get { return _lines.enabled; }
            set { _lines.enabled = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneController(
            IElementUpdateDelegate elementUpdateDelegate,
            ISceneUpdateDelegate sceneDelegate,
            string id,
            Element root)
        {
            _elementUpdateDelegate = elementUpdateDelegate;
            _sceneDelegate = sceneDelegate;
            _root = root;

            Id = id;
            ContentControllers = new ReadOnlyCollection<ContentDesignController>(_controllers);
            
            // setup line manager
            var unityEle = root as IUnityElement;
            if (null != unityEle)
            {
                _lines = unityEle.GameObject.AddComponent<LineManager>();
            }
            else
            {
                Log.Error(this,
                    "Root of scene {0} is not an IUnityElement! The root of a scene MUST be an IUnityElement.",
                    id);
            }

            // create controllers
            ElementUtil.Walk(
                root,
                element =>
                {
                    var content = element as ContentWidget;
                    if (null == content)
                    {
                        return;
                    }

                    var controller = CreateController(element);
                    if (null != controller)
                    {
                        _controllers.Add(controller);
                    }
                });

            Reindex();
        }

        /// <summary>
        /// Should be called when hierarchy changes.
        /// </summary>
        public void Reindex()
        {
            var anchors = new List<WorldAnchorWidget>();
            _root.Find("..(@type=WorldAnchorWidget)", anchors);
                
            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                // we're assuming a world anchor can't be a child of another
                // world anchor
                var controllers
                    = _anchorGraph[anchor]
                    = anchor.GameObject.GetComponentsInChildren<ContentDesignController>();
                for (var j = 0; j < controllers.Length; j++)
                {
                    controllers[j].Context.ParentAnchor = anchor;
                }
            }
        }

        /// <summary>
        /// Creates an ElementController from an ElementData. The ElementData
        /// is expected to have a valid Content Id.
        /// </summary>
        /// <param name="data">The propdata.</param>
        /// <returns></returns>
        public IAsyncToken<ContentDesignController> Create(ElementData data)
        {
            var token = new AsyncToken<ContentDesignController>();

            _sceneDelegate
                .Add(this, data)
                .OnSuccess(element =>
                {
                    var controller = CreateController(element);

                    if (null != controller)
                    {
                        _controllers.Add(controller);
                    }

                    token.Succeed(controller);
                })
                .OnFailure(token.Fail);

            return token;
        }
        
        /// <summary>
        /// Destroys an Element by id.
        /// </summary>
        /// <param name="id">The id of the Element.</param>
        /// <returns></returns>
        public IAsyncToken<Void> Destroy(string id)
        {
            var element = ById(id);
            if (null == element)
            {
                return new AsyncToken<Void>(new Exception("Could not find element by id."));
            }

            var token = new AsyncToken<Void>();
            _sceneDelegate
                .Remove(this, element.Element)
                .OnSuccess(_ =>
                {
                    _lines.Remove(element.Context.AnchorLine);
                    _controllers.Remove(element);

                    DestroyInternal(element);

                    token.Succeed(Void.Instance);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Destroys all elements.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<Void> DestroyAll()
        {
            return Async.Map(
                Async.All(
                    _controllers
                        // copy first
                        .ToArray()
                        // then destroy, which removes them from _controllers
                        .Select(controller => Destroy(controller.Element.Id))
                        .ToArray()),
                _ => Void.Instance);
        }

        /// <summary>
        /// Internal Create method which creates an <c>ElementController</c> for
        /// an <c>Element</c>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private ContentDesignController CreateController(Element element)
        {
            var unityElement = element as IUnityElement;
            if (null == unityElement)
            {
                return null;
            }

            var controller = unityElement.GameObject.AddComponent<ContentDesignController>();
            var context = new SceneElementContext();
            controller.Initialize(
                element,
                context,
                _elementUpdateDelegate);

            _lines.Add(context.AnchorLine);

            return controller;
        }

        /// <summary>
        /// Safely destroys an Element.
        /// </summary>
        /// <param name="element">The element to destroy</param>
        private void DestroyInternal(ContentDesignController element)
        {
            var content = element.Element;

            element.Uninitialize();

            // destroy content after Element, as Content holds the container
            // GameObject
            content.Destroy();
        }

        /// <summary>
        /// Retrieves an <c>ElementController</c> by id.
        /// </summary>
        /// <param name="id">The unique id of the element.</param>
        /// <returns></returns>
        private ContentDesignController ById(string id)
        {
            for (var i = 0; i < _controllers.Count; i++)
            {
                var controller = _controllers[i];
                if (controller.Element.Id == id)
                {
                    return controller;
                }
            }

            return null;
        }
    }
}