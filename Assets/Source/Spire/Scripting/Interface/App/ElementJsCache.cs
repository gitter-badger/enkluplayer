using System.Collections.Generic;
using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Implementation that lazily creates and caches JS instances.
    /// </summary>
    public class ElementJsCache : IElementJsCache
    {
        /// <summary>
        /// Js engine.
        /// </summary>
        private readonly Engine _engine;
        
        /// <summary>
        /// Lookup from element to js interface.
        /// </summary>
        private readonly Dictionary<Element, ElementJs> _elementMap = new Dictionary<Element, ElementJs>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJsCache(Engine engine)
        {
            _engine = engine;
        }
        
        /// <inheritdoc />
        public ElementJs Element(Element element)
        {
            if (null == element)
            {
                return null;
            }

            ElementJs el;
            if (_elementMap.TryGetValue(element, out el))
            {
                return el;
            }

            el = _elementMap[element] = new ElementJs(_engine, this, element);

            return el;
        }
    }
}