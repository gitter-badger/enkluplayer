using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using Jint;

namespace CreateAR.SpirePlayer
{
    public interface IElementJsCache
    {
        ElementJs Element(Element element);
    }
    
    public class ElementJsCache : IElementJsCache
    {
        private readonly Engine _engine;
        private readonly Dictionary<Element, ElementJs> _elementMap = new Dictionary<Element, ElementJs>();

        public ElementJsCache(Engine engine)
        {
            _engine = engine;
        }
        
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
    
    public class AppElementsJsApi
    {
        private readonly IElementJsCache _cache;
        private readonly IElementFactory _elementFactory;
        private readonly IElementManager _elements;
        
        public AppElementsJsApi(
            IElementJsCache cache,
            IElementFactory elementFactory,
            IElementManager elements)
        {
            _cache = cache;
            _elementFactory = elementFactory;
            _elements = elements;
        }

        public ElementJs create(string type)
        {
            return create(type, Guid.NewGuid().ToString());
        }
        
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

        public ElementJs byId(string id)
        {
            return _cache.Element(_elements.ById(id));
        }
    }
    
    public class AppJsApi
    {
        public readonly AppElementsJsApi elements;
        
        public AppJsApi(AppElementsJsApi elements)
        {
            this.elements = elements;
        }
    }
}