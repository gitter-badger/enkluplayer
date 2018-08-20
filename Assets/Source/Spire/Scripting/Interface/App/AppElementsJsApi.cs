using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for an app's elements.
    /// </summary>
    public class AppElementsJsApi
    {
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
        }

        /// <summary>
        /// Creates an element.
        /// </summary>
        /// <param name="type">The type of element to create.</param>
        /// <returns></returns>
        public ElementJs create(string type)
        {
            return create(type, Guid.NewGuid().ToString());
        }
        
        /// <summary>
        /// Creates an element with an id.
        /// </summary>
        /// <param name="type">The type of element to create.</param>
        /// <param name="id">The id to create it with.</param>
        /// <returns></returns>
        public ElementJs create(string type, string id)
        {
            Element element;
            try
            {
                element = _elementFactory.Element(string.Format(
                    @"<?Vine><{0} id='{1}' />",
                    type,
                    id));
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
    }
}