using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages a set of Element controllers for a scene, and pipes updates
    /// about.
    /// </summary>
    public class SceneDesignController
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
        /// Play mode configuration.
        /// </summary>
        private readonly PlayModeConfig _config;

        /// <summary>
        /// Provider.
        /// </summary>
        private readonly IWorldAnchorProvider _provider;

        /// <summary>
        /// Http.
        /// </summary>
        private readonly IHttpService _http;

        /// <summary>
        /// Receives update requests from Elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementUpdateDelegate;

        /// <summary>
        /// Received propset update events.
        /// </summary>
        private readonly ISceneUpdateDelegate _sceneDelegate;
        
        /// <summary>
        /// Backing property for ContentControllers.
        /// </summary>
        private readonly List<ContentDesignController> _contentControllers = new List<ContentDesignController>();

        /// <summary>
        /// Backing property for AnchorControllers.
        /// </summary>
        private readonly List<AnchorDesignController> _anchorControllers = new List<AnchorDesignController>();
        
        /// <summary>
        /// Manages line rendering.
        /// </summary>
        private LineManager _lines;

        /// <summary>
        /// Root of the scene.
        /// </summary>
        private readonly Element _root;

        /// <summary>
        /// The unique id of this scene.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// All content controllers.
        /// </summary>
        public ReadOnlyCollection<ContentDesignController> ContentControllers { get; private set; }

        /// <summary>
        // All anchor controllers.
        /// </summary>
        public ReadOnlyCollection<AnchorDesignController> AnchorControllers { get; private set; }

        /// <summary>
        /// True iff anchors should show children.
        /// </summary>
        public bool ShowAnchorChildren
        {
            get { return _lines.enabled; }
            set
            {
                _lines.enabled = value;

                SetAnchorControllerVisibility(value);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneDesignController(
            PlayModeConfig config,
            IWorldAnchorProvider provider,
            IHttpService http,
            IElementUpdateDelegate elementUpdateDelegate,
            ISceneUpdateDelegate sceneDelegate,
            string id,
            Element root)
        {
            _config = config;
            _provider = provider;
            _http = http;
            _elementUpdateDelegate = elementUpdateDelegate;
            _sceneDelegate = sceneDelegate;
            _root = root;

            Id = id;
            ContentControllers = new ReadOnlyCollection<ContentDesignController>(_contentControllers);
            AnchorControllers = new ReadOnlyCollection<AnchorDesignController>(_anchorControllers);
            
            SetupElementControllers(id, root);
        }
        
        /// <summary>
        /// Creates a ContentDesignController from an ElementData.
        /// </summary>
        /// <param name="data">The ElementData.</param>
        /// <returns></returns>
        public IAsyncToken<ContentDesignController> CreateContent(ElementData data)
        {
            var token = new AsyncToken<ContentDesignController>();

            _sceneDelegate
                .Add(this, data)
                .OnSuccess(element =>
                {
                    var content = element as ContentWidget;
                    if (null == content)
                    {
                        token.Fail(new Exception("Element was not of type ContentWidget."));
                        return;
                    }

                    var controller = ContentController(content);
                    _contentControllers.Add(controller);

                    token.Succeed(controller);
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Creates a AnchorDesignController from an ElementData.
        /// </summary>
        /// <param name="data">The ElementData.</param>
        /// <returns></returns>
        public IAsyncToken<AnchorDesignController> CreateAnchor(ElementData data)
        {
            var token = new AsyncToken<AnchorDesignController>();

            _sceneDelegate
                .Add(this, data)
                .OnSuccess(element =>
                {
                    var anchor = element as WorldAnchorWidget;
                    if (null == anchor)
                    {
                        token.Fail(new Exception("Element was not of type AnchorWidget."));
                        return;
                    }

                    var controller = AnchorController(anchor);
                    _anchorControllers.Add(controller);

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
                    _contentControllers.Remove(element);

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
                    _contentControllers
                        // copy first
                        .ToArray()
                        // then destroy, which removes them from _controllers
                        .Select(controller => Destroy(controller.Element.Id))
                        .ToArray()),
                _ => Void.Instance);
        }

        /// <summary>
        /// Internal Create method which creates an <c>ContentDesignController</c> for
        /// a <c>ContentWidget</c>.
        /// </summary>
        /// <param name="content">The ContentWidget.</param>
        /// <returns></returns>
        private ContentDesignController ContentController(ContentWidget content)
        {
            var controller = content.GameObject.AddComponent<ContentDesignController>();
            var context = new SceneElementContext();
            controller.Initialize(
                content,
                context,
                _elementUpdateDelegate);

            _lines.Add(context.AnchorLine);

            return controller;
        }

        /// <summary>
        /// Creates a controller for an anchor.
        /// </summary>
        /// <param name="anchor">The anchor.</param>
        /// <returns></returns>
        private AnchorDesignController AnchorController(WorldAnchorWidget anchor)
        {
            var controller = anchor.GameObject.AddComponent<AnchorDesignController>();
            controller.Initialize(_config, anchor, _provider, _http);
            controller.IsVisualEnabled = false;

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
            for (var i = 0; i < _contentControllers.Count; i++)
            {
                var controller = _contentControllers[i];
                if (controller.Element.Id == id)
                {
                    return controller;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets visibility on anchors.
        /// </summary>
        /// <param name="value">True iff visible.</param>
        private void SetAnchorControllerVisibility(bool value)
        {
            for (var i = 0; i < _anchorControllers.Count; i++)
            {
                _anchorControllers[i].IsVisualEnabled = value;
            }
        }

        private void SetupElementControllers(string id, Element root)
        {
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
                        var anchor = element as WorldAnchorWidget;
                        if (null == anchor)
                        {
                            return;
                        }

                        _anchorControllers.Add(AnchorController(anchor));
                        return;
                    }

                    _contentControllers.Add(ContentController(content));
                });

            Reindex();
        }

        /// <summary>
        /// Should be called when hierarchy changes.
        /// </summary>
        private void Reindex()
        {
            var anchors = new List<WorldAnchorWidget>();
            _root.Find("..(@type=WorldAnchorWidget)", anchors);

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];

                // we're assuming a world anchor can't be a child of another
                // world anchor
                var controllers = anchor.GameObject.GetComponentsInChildren<ContentDesignController>();
                for (var j = 0; j < controllers.Length; j++)
                {
                    controllers[j].Context.ParentAnchor = anchor;
                }
            }
        }
    }
}