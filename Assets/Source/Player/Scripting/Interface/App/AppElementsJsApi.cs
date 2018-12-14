using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Interface for an app's elements.
    /// </summary>
    public class AppElementsJsApi : JsEventDispatcher
    {
        /// <summary>
        /// Event types.
        /// </summary>
        public const string EVENT_CREATED = "created";
        public const string EVENT_DESTROYED = "destroyed";

        /// <summary>
        /// Caches js interfaces for elements.
        /// </summary>
        private readonly IElementJsCache _cache;
        
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elementFactory;
        
        /// <summary>
        /// Manages all elements.
        /// </summary>
        private readonly IElementManager _elements;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppElementsJsApi(
            IElementJsCache cache,
            IElementFactory elementFactory,
            IElementManager elements)
        {
            _cache = cache;
            _elementFactory = elementFactory;
            _elements = elements;

            _elements.OnCreated += Elements_OnCreated;
            _elements.OnDestroyed += Elements_OnDestroyed;
        }

        /// <summary>
        /// Creates an element.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="type">The type of element to create.</param>
        /// <returns></returns>
        public ElementJs create(ElementJs parent, string type)
        {
            return create(parent, type, Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Creates an element with an id.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="type">The type of element to create.</param>
        /// <param name="id">The id to create it with.</param>
        /// <returns></returns>
        public ElementJs create(ElementJs parent, string type, string id)
        {
            return createFromVine(parent, string.Format(
                @"<?Vine><{0} id='{1}' />",
                type,
                id));
        }

        /// <summary>
        /// Creates an element with an id.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="vine">Vine.</param>
        /// <returns></returns>
        public ElementJs createFromVine(ElementJs parent, string vine)
        {
            Element element;
            try
            {
                element = _elementFactory.Element(vine);
            }
            catch (Exception exception)
            {
                Log.Error(this,
                    "Could not create Element : {0}.",
                    exception);
                return null;
            }

            return _cache.Element(element);
        }

        /// <summary>
        /// Destroys an element.
        /// </summary>
        /// <param name="element">The element to destroy.</param>
        public void destroy(ElementJs element)
        {
            if (null == element)
            {
                return;
            }

            element.destroy();
        }

        /// <summary>
        /// Retrieves an element by id.
        /// </summary>
        /// <param name="id">The id of the element.</param>
        /// <returns></returns>
        public ElementJs byId(string id)
        {
            return _cache.Element(_elements.ById(id));
        }

        /// <summary>
        /// Called when an element has been created.
        /// </summary>
        /// <param name="element"></param>
        private void Elements_OnCreated(Element element)
        {
            dispatch(EVENT_CREATED, _cache.Element(element));
        }

        /// <summary>
        /// Called when element has been destroyed.
        /// </summary>
        /// <param name="element">The element in question.</param>
        private void Elements_OnDestroyed(Element element)
        {
            dispatch(EVENT_DESTROYED, _cache.Element(element));
        }
    }
}