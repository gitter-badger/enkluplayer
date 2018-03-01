using System;
using CreateAR.Commons.Unity.Async;
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
        /// Received propset update events.
        /// </summary>
        private readonly ISceneUpdateDelegate _sceneDelegate;
        
        /// <summary>
        /// Root of the scene.
        /// </summary>
        private readonly Element _root;

        /// <summary>
        /// The unique id of this scene.
        /// </summary>
        public string Id { get; private set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneDesignController(
            ISceneUpdateDelegate sceneDelegate,
            string id,
            Element root)
        {
            _sceneDelegate = sceneDelegate;
            _root = root;

            Id = id;
        }
        
        /// <summary>
        /// Creates an element from an ElementData.
        /// </summary>
        /// <param name="data">The ElementData.</param>
        /// <returns></returns>
        public IAsyncToken<Element> Create(ElementData data)
        {
            return _sceneDelegate.Add(Id, data);
        }

        /// <summary>
        /// Destroys an Element by id.
        /// </summary>
        /// <param name="id">The id of the Element.</param>
        /// <returns></returns>
        public IAsyncToken<Element> Destroy(string id)
        {
            var element = ById(id);
            if (null == element)
            {
                return new AsyncToken<Element>(new Exception("Could not find element by id."));
            }

            return _sceneDelegate.Remove(Id, element);
        }

        /// <summary>
        /// Destroys all elements.
        /// </summary>
        /// <returns></returns>
        public IAsyncToken<Void> DestroyAll()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }
        
        /// <summary>
        /// Retrieves an element by id.
        /// </summary>
        /// <param name="id">The unique id of the element.</param>
        /// <returns></returns>
        private Element ById(string id)
        {
            // cannot remove root
            return _root.FindOne<Element>(".." + id);
        }
    }
}