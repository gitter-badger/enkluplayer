using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using Jint;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Implementation that lazily creates and caches ElementJS instances.
    /// </summary>
    public class ElementJsCache : IElementJsCache
    {
        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementJsFactory _elements;
        
        /// <summary>
        /// Lookup from element to js interface.
        /// </summary>
        private readonly Dictionary<Element, ElementJs> _elementMap = new Dictionary<Element, ElementJs>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementJsCache(IElementJsFactory elements)
        {
            _elements = elements;
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

            el = _elementMap[element] = _elements.Instance(this, element);

            return el;
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var el in _elementMap.Values)
            {
                el.Cleanup();
            }
            _elementMap.Clear();
        }
    }
}