using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;

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

            element.OnDestroyed += Element_OnDestroy;

            return el;
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var kvp in _elementMap)
            {
                kvp.Key.OnDestroyed -= Element_OnDestroy;
                kvp.Value.Cleanup();
            }
            _elementMap.Clear();
        }

        /// <summary>
        /// Cleans up the cache entry for an Element when it gets destroyed.
        /// </summary>
        /// <param name="element"></param>
        public void Element_OnDestroy(Element element)
        {
            ElementJs el;
            if (_elementMap.TryGetValue(element, out el))
            {
                _elementMap.Remove(element);
                el.Cleanup();
            }
        }
    }
}