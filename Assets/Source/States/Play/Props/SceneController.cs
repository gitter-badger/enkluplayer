using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CreateAR.Commons.Unity.Async;
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
        /// Receives update requests from Elements.
        /// </summary>
        private readonly IElementUpdateDelegate _elementUpdateDelegate;

        /// <summary>
        /// Received propset update events.
        /// </summary>
        private readonly ISceneUpdateDelegate _sceneDelegate;

        /// <summary>
        /// Backing property for Controllers..
        /// </summary>
        private readonly List<ElementController> _controllers = new List<ElementController>();

        /// <summary>
        /// The unique id of this scene.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// All element controllers.
        /// </summary>
        public ReadOnlyCollection<ElementController> Controllers { get; private set; }

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

            Id = id;
            Controllers = new ReadOnlyCollection<ElementController>(_controllers);

            // create controllers
            ElementUtil.Walk(
                root,
                element =>
                {
                    var controller = CreateController(element);
                    if (null != controller)
                    {
                        _controllers.Add(controller);
                    }
                });
        }

        /// <summary>
        /// Creates an ElementController from an ElementData. The ElementData
        /// is expected to have a valid Content Id.
        /// </summary>
        /// <param name="data">The propdata.</param>
        /// <returns></returns>
        public IAsyncToken<ElementController> Create(ElementData data)
        {
            var token = new AsyncToken<ElementController>();

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
        private ElementController CreateController(Element element)
        {
            var unityElement = element as IUnityElement;
            if (null == unityElement)
            {
                return null;
            }

            var controller = unityElement.GameObject.AddComponent<ElementController>();
            controller.Initialize(element,  _elementUpdateDelegate);

            return controller;
        }

        /// <summary>
        /// Safely destroys an Element.
        /// </summary>
        /// <param name="element">The element to destroy</param>
        private void DestroyInternal(ElementController element)
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
        private ElementController ById(string id)
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