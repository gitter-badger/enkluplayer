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
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;
        
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
            IElementFactory elements,
            IElementUpdateDelegate elementUpdateDelegate,
            ISceneUpdateDelegate sceneDelegate,
            string id,
            ElementData[] data)
        {
            _elements = elements;
            _elementUpdateDelegate = elementUpdateDelegate;
            _sceneDelegate = sceneDelegate;

            Id = id;
            Controllers = new ReadOnlyCollection<ElementController>(_controllers);

            // create controllers
            for (var i = 0; i < data.Length; i++)
            {
                var elementData = data[i];
                var controller = CreateInternal(elementData);
                if (null == controller)
                {
                    Log.Warning(this,
                        "Could not create ElementController from ElementData {0}.",
                        elementData);
                    continue;
                }
                
                _controllers.Add(controller);
            }
        }

        /// <summary>
        /// Creates an ElementController from an ElementData. The ElementData
        /// is expected to have a valid Content Id.
        /// </summary>
        /// <param name="data">The propdata.</param>
        /// <returns></returns>
        public IAsyncToken<ElementController> Create(ElementData data)
        {
            ElementController controller = null;
            
            return Async.Map(
                _sceneDelegate
                    .Add(this, data)
                    .OnSuccess(_ =>
                    {
                        controller = CreateInternal(data);

                        _controllers.Add(controller);
                    }),
                _ => controller);
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

            return _sceneDelegate
                .Remove(this, element.Element)
                .OnSuccess(_ =>
                {
                    _controllers.Remove(element);

                    DestroyInternal(element);
                });
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
        /// Internal Create method which creates a <c>ContentWidget</c> and
        /// <c>ElementController</c>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private ElementController CreateInternal(ElementData data)
        {
            var element = (ContentWidget) _elements.Element(new ElementDescription
            {
                Root = new ElementRef
                {
                    Id = data.Id
                },
                Elements = new[]
                {
                    data
                }
            });
            
            var controller = element.GameObject.AddComponent<ElementController>();
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